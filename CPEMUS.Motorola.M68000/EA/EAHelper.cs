using CPEMUS.Motorola.M68000.Extensions;

namespace CPEMUS.Motorola.M68000.EA
{
    internal class EAHelper
    {
        private readonly M68KRegs _regs;
        private readonly byte[] _mem;
        public EAHelper(M68KRegs regs, byte[] memory)
        {
            _regs = regs;
            _mem = memory;
        }

        public EAResult Get(ushort opcode)
        {
            int registerField = opcode & 0x7;
            var mode = (EAMode)((opcode >> 3) & 0x7);
            int? result = default;
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
                    result = GetAnIndirectPostincVal(registerField);
                    break;
                case EAMode.PredecIndirect: // EA = [--An].
                    result = GetAnIndirectPredecVal(registerField);
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
                        result = GetImmediateVal(/*operationLength*/);
                    }
                    break;
            }

            if (!result.HasValue)
            {
                throw new NotImplementedException("This addressing mode is not implemented.");
            }

            return new()
            {
                Operand = result.Value,
                Address = 1,
                InstructionSize = 1,
            };
        }

        private int GetDnDirectVal(int idx)
        {
            return _regs.D[idx];
        }

        private int GetAnDirectVal(int idx)
        {
            return _regs.A[idx];
        }

        private int GetAnIndirectVal(int idx)
        {
            return _mem[_regs.A[idx]];
        }

        private int GetAnIndirectPostincVal(int idx, OperandSize operandSize)
        {
            int result = _mem[_regs.A[idx]];
            _regs.A[idx] += CalcAddressIncOrDecVal(idx, operandSize);
            return result;
        }

        private int GetAnIndirectPredecVal(int idx, OperandSize operandSize)
        {
            _regs.A[idx] -= CalcAddressIncOrDecVal(idx, operandSize);
            int result = _mem[_regs.A[idx]];
            return result;
        }

        private int CalcAddressIncOrDecVal(int idx, OperandSize operandSize)
        {
            // If An is SP and operand is byte, increase/decrease
            // by two to keep the SP aligned to a word boundary.
            if (idx == 0x7 && operandSize == OperandSize.Byte)
            {
                return 2;
            }
            else
            {
                return (int)operandSize;
            }
        }

        private int GetAnIndirectDisplaceVal(int idx)
        {
            var anVal = _regs.A[idx];
            int displacement = (short)_mem.ReadLong(_regs.PC);
            return _mem[anVal + displacement];
        }

        private int GetIndexedAddressingVal(int idx)
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
            
            return _mem[anVal + displacement + indexRegister * (int)Math.Pow(2, scale)];
        }

        private int GetPCDisplaceVal()
        {
            var pcVal = _regs.PC;
            int displacement = (short)_mem.ReadLong(pcVal);
            return _mem[pcVal + displacement];
        }

        private int GetAbsoluteVal(bool word)
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

            return _mem[displacement];
        }

        private int GetImmediateVal(int operationLength)
        {
            var pcVal = _regs.PC;
            int? result = null;
            switch (operationLength)
            {
                case 1:
                    result = (byte)_mem.ReadLong(pcVal);
                    break;
                case 2:
                    result = (ushort)_mem.ReadLong(pcVal);
                    break;
                case 4:
                    result = _mem.ReadLong(pcVal);
                    break;
                default:
                    throw new NotImplementedException("This type of immediate data is not handled.");
            }

            return result.Value;
        }
    }
}
