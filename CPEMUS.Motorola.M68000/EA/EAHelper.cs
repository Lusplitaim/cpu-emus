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
            bool getOperand = false;
            switch (mode)
            {
                case EAMode.DataDirect: // EA = Dn.
                    (getOperand, result) = GetDnDirectVal(registerField);
                    break;
                case EAMode.AddressDirect: // EA = An.
                    (getOperand, result) = GetAnDirectVal(registerField);
                    break;
                case EAMode.AddressIndirect: // EA = [An].
                    (getOperand, result) = GetAnIndirectVal(registerField);
                    break;
                case EAMode.PostincIndirect: // EA = [An++].
                    (getOperand, result) = GetAnIndirectPostincVal(registerField, operandSize);
                    break;
                case EAMode.PredecIndirect: // EA = [--An].
                    (getOperand, result) = GetAnIndirectPredecVal(registerField, operandSize);
                    break;
                case EAMode.BaseDisplacement: // EA = [An + displacement16].
                    (getOperand, result) = GetAnIndirectDisplaceVal(registerField);
                    break;
                case EAMode.IndexedAddressing: // EA = [An + displacement8 + Xn.size * scale].
                    (getOperand, result) = GetIndexedAddressingVal(registerField);
                    break;
                case EAMode.PCAbsoluteImmediate:
                    if (registerField == 0x2) // EA = [PC + displacement16].
                    {
                        (getOperand, result) = GetPCDisplaceVal();
                    }
                    else if (registerField == 0x0)
                    {
                        (getOperand, result) = GetAbsoluteVal(true);
                    }
                    else if (registerField == 0x1)
                    {
                        (getOperand, result) = GetAbsoluteVal(false);
                    }
                    else if (registerField == 0x4)
                    {
                        (getOperand, result) = GetImmediateVal(operandSize);
                    }
                    break;
            }

            if (!result.HasValue)
            {
                throw new NotImplementedException("This addressing mode is unknown or not implemented.");
            }

            if (getOperand)
            {
                var props = result.Value;
                props.Operand = Read(props.Address, props.Location, operandSize);
                result = props;
            }

            return result.Value;
        }

        private int Read(int value, OperandSize operandSize)
        {
            return operandSize switch
            {
                OperandSize.Byte => value & 0xFF,
                OperandSize.Word => value & 0xFFFF,
                OperandSize.Long => value,
                _ => throw new InvalidOperationException("Operand size type is unknown"),
            };
        }

        private int Read(int address, StoreLocation location, OperandSize operandSize)
        {
            return location switch
            {
                StoreLocation.DataRegister => Read(_regs.D[address], operandSize),
                StoreLocation.AddressRegister => Read(_regs.A[address], operandSize),
                StoreLocation.Memory => _mem.Read(address, operandSize),
                _ => throw new InvalidOperationException("Operand location type is unknown"),
            };
        }

        private (bool getOperand, EAProps props) GetDnDirectVal(int idx)
        {
             return (true, new()
             {
                 Address = idx,
                 Location = StoreLocation.DataRegister,
                 InstructionSize = DEFAULT_INSTR_SIZE,
             });
        }

        private (bool getOperand, EAProps props) GetAnDirectVal(int idx)
        {
            return (true, new()
            {
                Address = idx,
                Location = StoreLocation.AddressRegister,
                InstructionSize = DEFAULT_INSTR_SIZE,
            });
        }

        private (bool getOperand, EAProps props) GetAnIndirectVal(int idx)
        {
            return (true, new()
            {
                Address = _regs.A[idx],
                Location = StoreLocation.Memory,
                InstructionSize = DEFAULT_INSTR_SIZE,
            });
        }

        private (bool getOperand, EAProps props) GetAnIndirectPostincVal(int idx, OperandSize operandSize)
        {
            int address = _regs.A[idx];
            _regs.A[idx] += CalcAddressIncOrDecVal(idx, operandSize);
            return (true, new()
            {
                Address = address,
                Location = StoreLocation.Memory,
                InstructionSize = DEFAULT_INSTR_SIZE,
            });
        }

        private (bool getOperand, EAProps props) GetAnIndirectPredecVal(int idx, OperandSize operandSize)
        {
            _regs.A[idx] -= CalcAddressIncOrDecVal(idx, operandSize);
            int address = _regs.A[idx];
            return (true, new()
            {
                Address = address,
                Location = StoreLocation.Memory,
                InstructionSize = DEFAULT_INSTR_SIZE,
            });
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

        private (bool getOperand, EAProps props) GetAnIndirectDisplaceVal(int idx)
        {
            var an = _regs.A[idx];
            int displacement = (short)_mem.ReadLong(_regs.PC);
            int address = an + displacement;
            return (true, new()
            {
                Address = address,
                Location = StoreLocation.Memory,
                InstructionSize = DEFAULT_INSTR_SIZE + 2,
            });
        }

        private (bool getOperand, EAProps props) GetIndexedAddressingVal(int idx)
        {
            var an = _regs.A[idx];

            var extWord = (ushort)_mem.ReadLong(_regs.PC);
            int displacement = (sbyte)extWord;

            int indexRegisterIdx = (extWord >> 12) & 0x7;
            int indexRegister;
            if ((IndexRegister)((extWord >> 15) & 0x1) == IndexRegister.Address)
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
            
            int address = an + displacement + indexRegister * (int)Math.Pow(2, scale);

            return (true, new()
            {
                Address = address,
                Location = StoreLocation.Memory,
                InstructionSize = DEFAULT_INSTR_SIZE + 2,
            });
        }

        private (bool getOperand, EAProps props) GetPCDisplaceVal()
        {
            var pc = _regs.PC;
            int displacement = (short)_mem.ReadLong(pc);
            int address = pc + displacement;
            return (true, new()
            {
                Address = address,
                Location = StoreLocation.Memory,
                InstructionSize = DEFAULT_INSTR_SIZE + 2,
            });
        }

        private (bool getOperand, EAProps props) GetAbsoluteVal(bool word)
        {
            var pc = _regs.PC;
            int displacement;
            if (word)
            {
                displacement = (short)_mem.ReadLong(pc);
            }
            else
            {
                displacement = _mem.ReadLong(pc + 2);
            }

            int address = displacement;
            return (true, new()
            {
                Address = address,
                Location = StoreLocation.Memory,
                InstructionSize = DEFAULT_INSTR_SIZE + (word ? 2 : 4),
            });
        }

        private (bool getOperand, EAProps props) GetImmediateVal(OperandSize operandSize)
        {
            int address = _regs.PC + DEFAULT_INSTR_SIZE;
            int result;
            switch (operandSize)
            {
                case OperandSize.Byte:
                    result = (byte)_mem.ReadWord(address);
                    break;
                case OperandSize.Word:
                    result = _mem.ReadWord(address);
                    break;
                case OperandSize.Long:
                    result = _mem.ReadLong(address);
                    break;
                default:
                    throw new NotImplementedException("This type of immediate data is not supported.");
            }

            int operand = result;
            int operationLength = operandSize == OperandSize.Byte ? 2 : (int)operandSize;
            return (false, new()
            {
                Operand = operand,
                Address = address,
                Location = StoreLocation.Memory,
                InstructionSize = DEFAULT_INSTR_SIZE + operationLength,
            });
        }
    }
}
