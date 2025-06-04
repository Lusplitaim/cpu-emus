using CPEMUS.Motorola.M68000.Extensions;

namespace CPEMUS.Motorola.M68000.EA
{
    internal class EAHelper
    {
        private readonly int DEFAULT_INSTR_SIZE = 2;
        private readonly M68KRegs _regs;
        private readonly byte[] _mem;
        public EAHelper(M68KRegs regs, byte[] memory)
        {
            _regs = regs;
            _mem = memory;
        }

        public EAProps Get(ushort opcode, OperandSize operandSize)
        {
            int registerField = opcode & 0x7;
            var mode = (EAMode)((opcode >> 3) & 0x7);
            EAProps? result = null;
            switch (mode)
            {
                case EAMode.DataDirect: // EA = Dn.
                    result = GetDnDirectVal(registerField);
                    break;
                case EAMode.AddressDirect: // EA = An.
                    result = GetAnDirectVal(registerField);
                    break;
                case EAMode.AddressIndirect: // EA = [An].
                    result = GetAnIndirectVal(registerField);
                    break;
                case EAMode.PostincIndirect: // EA = [An++].
                    result = GetAnIndirectPostincVal(registerField, operandSize);
                    break;
                case EAMode.PredecIndirect: // EA = [--An].
                    result = GetAnIndirectPredecVal(registerField, operandSize);
                    break;
                case EAMode.BaseDisplacement: // EA = [An + displacement16].
                    result = GetAnIndirectDisplaceVal(registerField);
                    break;
                case EAMode.IndexedAddressing: // EA = [An + displacement8 + Xn.size * scale].
                    result = GetIndexedAddressingVal(registerField);
                    break;
                case EAMode.PCAbsoluteImmediate:
                    if (registerField == 0x2)
                    {
                        result = GetPCDisplaceVal();
                    }
                    else if (registerField == 0x0)
                    {
                        result = GetAbsoluteVal(true);
                    }
                    else if (registerField == 0x1)
                    {
                        result = GetAbsoluteVal(false);
                    }
                    else if (registerField == 0x4)
                    {
                        result = GetImmediateVal(operandSize);
                    }
                    break;
            }

            if (!result.HasValue)
            {
                throw new NotImplementedException("This addressing mode is not implemented.");
            }

            return result.Value;
        }

        private EAProps GetDnDirectVal(int idx)
        {
             return new()
             {
                 Operand = _regs.D[idx],
                 Address = idx,
                 Location = EALocation.DataRegister,
                 InstructionSize = DEFAULT_INSTR_SIZE,
             };
        }

        private EAProps GetAnDirectVal(int idx)
        {
            return new()
            {
                Operand = _regs.A[idx],
                Address = idx,
                Location = EALocation.AddressRegister,
                InstructionSize = DEFAULT_INSTR_SIZE,
            };
        }

        private EAProps GetAnIndirectVal(int idx)
        {
            return new()
            {
                Operand = _mem[_regs.A[idx]],
                Address = idx,
                Location = EALocation.Memory,
                InstructionSize = DEFAULT_INSTR_SIZE,
            };
        }

        private EAProps GetAnIndirectPostincVal(int idx, OperandSize operandSize)
        {
            int operand = _mem[_regs.A[idx]];
            int address = _regs.A[idx];
            _regs.A[idx] += CalcAddressIncOrDecVal(idx, operandSize);
            return new()
            {
                Operand = operand,
                Address = address,
                Location = EALocation.Memory,
                InstructionSize = DEFAULT_INSTR_SIZE,
            };
        }

        private EAProps GetAnIndirectPredecVal(int idx, OperandSize operandSize)
        {
            _regs.A[idx] -= CalcAddressIncOrDecVal(idx, operandSize);
            int address = _regs.A[idx];
            int operand = _mem[_regs.A[idx]];
            return new()
            {
                Operand = operand,
                Address = address,
                Location = EALocation.Memory,
                InstructionSize = DEFAULT_INSTR_SIZE,
            };
        }

        private int CalcAddressIncOrDecVal(int idx, OperandSize operandSize)
        {
            // If An is SP and operand is byte, increase/decrease
            // by two to keep the SP aligned to a word boundary.
            if (idx == 0x7 && operandSize == OperandSize.Byte)
            {
                return (int)OperandSize.Word;
            }
            else
            {
                return (int)operandSize;
            }
        }

        private EAProps GetAnIndirectDisplaceVal(int idx)
        {
            var an = _regs.A[idx];
            int displacement = (short)_mem.ReadLong(_regs.PC);
            int address = an + displacement;
            int operand = _mem[address];
            return new()
            {
                Operand = operand,
                Address = address,
                Location = EALocation.Memory,
                InstructionSize = DEFAULT_INSTR_SIZE + 2,
            };
        }

        private EAProps GetIndexedAddressingVal(int idx)
        {
            var anVal = _regs.A[idx];

            var extWord = (ushort)_mem.ReadLong(_regs.PC);
            int displacement = (sbyte)extWord;

            int indexRegisterIdx = (extWord & 0x7000) >> 12;
            int indexRegister;
            if ((IndexRegister)((extWord & 0x8000) >> 15) == IndexRegister.Address)
            {
                indexRegister = _regs.A[indexRegisterIdx];
            }
            else
            {
                indexRegister = _regs.D[indexRegisterIdx];
            }

            if ((IndexSize)((extWord & 0x0800) >> 11) == IndexSize.SignExtendedWord)
            {
                indexRegister = (short)indexRegister;
            }

            int scale = (extWord & 0x0600) >> 9;
            
            int address = anVal + displacement + indexRegister * (int)Math.Pow(2, scale);
            int operand = _mem[address];

            return new()
            {
                Operand = operand,
                Address = address,
                Location = EALocation.Memory,
                InstructionSize = DEFAULT_INSTR_SIZE + 2,
            };
        }

        private EAProps GetPCDisplaceVal()
        {
            var pc = _regs.PC;
            int displacement = (short)_mem.ReadLong(pc);
            int address = pc + displacement;
            int operand = _mem[address];
            return new()
            {
                Operand = operand,
                Address = address,
                Location = EALocation.Memory,
                InstructionSize = DEFAULT_INSTR_SIZE + 2,
            };
        }

        private EAProps GetAbsoluteVal(bool word)
        {
            var pcVal = _regs.PC;
            int displacement;
            if (word)
            {
                displacement = (short)_mem.ReadLong(pcVal);
            }
            else
            {
                displacement = _mem.ReadLong(pcVal + 2);
            }

            int address = displacement;
            int operand = _mem[address];
            return new()
            {
                Operand = operand,
                Address = address,
                Location = EALocation.Memory,
                InstructionSize = DEFAULT_INSTR_SIZE + (word ? 2 : 4),
            };
        }

        private EAProps GetImmediateVal(OperandSize operandSize)
        {
            var pc = _regs.PC;
            int? result = null;
            switch (operandSize)
            {
                case OperandSize.Byte:
                    result = (byte)_mem.ReadLong(pc);
                    break;
                case OperandSize.Word:
                    result = (ushort)_mem.ReadLong(pc);
                    break;
                case OperandSize.Long:
                    result = _mem.ReadLong(pc);
                    break;
                default:
                    throw new NotImplementedException("This type of immediate data is not handled.");
            }

            int address = pc;
            int operand = result.Value;
            int operationLength = operandSize == OperandSize.Byte ? 2 : (int)operandSize;
            return new()
            {
                Operand = operand,
                Address = address,
                Location = EALocation.Memory,
                InstructionSize = DEFAULT_INSTR_SIZE + operationLength,
            };
        }
    }
}
