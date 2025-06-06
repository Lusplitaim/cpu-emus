using CPEMUS.Motorola.M68000.Extensions;
using CPEMUS.Motorola.M68000.Helpers;

namespace CPEMUS.Motorola.M68000.EA
{
    internal class EAHelper
    {
        private const int DEFAULT_OPCODE_SIZE = 2;
        private readonly M68KRegs _regs;
        private readonly MemHelper _memHelper;
        private readonly byte[] _mem;
        public EAHelper(M68KRegs regs, byte[] memory, MemHelper memHelper)
        {
            _regs = regs;
            _mem = memory;
            _memHelper = memHelper;
        }

        public EAProps Get(ushort opcode, OperandSize operandSize, int opcodeSize = DEFAULT_OPCODE_SIZE)
        {
            uint registerField = (uint)(opcode & 0x7);
            var mode = (EAMode)((opcode >> 3) & 0x7);
            EAProps result;
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
                    (getOperand, result) = GetAnIndirectDisplaceVal(registerField, opcodeSize);
                    break;
                case EAMode.IndexedAddressing: // EA = [An + displacement8 + Xn.size * scale].
                    (getOperand, result) = GetIndexedAddressingVal(_regs.A[registerField], opcodeSize);
                    break;
                case EAMode.PCAbsoluteImmediate:
                    if (registerField == 0x2) // EA = [PC + displacement16].
                    {
                        (getOperand, result) = GetPCDisplaceVal(opcodeSize);
                    }
                    if (registerField == 0x3) // EA = [PC + displacement8 + Xn.size * scale].
                    {
                        (getOperand, result) = GetIndexedAddressingVal(_regs.PC, opcodeSize);
                    }
                    else if (registerField == 0x0)
                    {
                        (getOperand, result) = GetAbsoluteVal(true, opcodeSize);
                    }
                    else if (registerField == 0x1)
                    {
                        (getOperand, result) = GetAbsoluteVal(false, opcodeSize);
                    }
                    else if (registerField == 0x4)
                    {
                        (getOperand, result) = GetImmediateVal(operandSize, opcodeSize);
                    }
                    else
                    {
                        throw new NotImplementedException("This addressing mode is unknown or not implemented.");
                    }
                    break;
                default:
                    throw new NotImplementedException("This addressing mode is unknown or not implemented.");
            }

            if (getOperand)
            {
                result.Operand = _memHelper.Read(result.Address, result.Location, operandSize);
            }
            result.InstructionSize += opcodeSize;

            return result;
        }

        private (bool getOperand, EAProps props) GetDnDirectVal(uint idx)
        {
             return (true, new()
             {
                 Address = idx,
                 Location = StoreLocation.DataRegister,
             });
        }

        private (bool getOperand, EAProps props) GetAnDirectVal(uint idx)
        {
            return (true, new()
            {
                Address = idx,
                Location = StoreLocation.AddressRegister,
            });
        }

        private (bool getOperand, EAProps props) GetAnIndirectVal(uint idx)
        {
            return (true, new()
            {
                Address = _regs.A[idx],
                Location = StoreLocation.Memory,
            });
        }

        private (bool getOperand, EAProps props) GetAnIndirectPostincVal(uint idx, OperandSize operandSize)
        {
            uint address = _regs.A[idx];
            _regs.A[idx] += CalcAddressIncOrDecVal(idx, operandSize);
            return (true, new()
            {
                Address = address,
                Location = StoreLocation.Memory,
            });
        }

        private (bool getOperand, EAProps props) GetAnIndirectPredecVal(uint idx, OperandSize operandSize)
        {
            _regs.A[idx] -= CalcAddressIncOrDecVal(idx, operandSize);
            uint address = _regs.A[idx];
            return (true, new()
            {
                Address = address,
                Location = StoreLocation.Memory,
            });
        }

        private uint CalcAddressIncOrDecVal(uint idx, OperandSize operandSize)
        {
            // If An is SP and operand is byte, increase/decrease
            // by two to keep the SP aligned to a word boundary.
            if (idx == 0x7 && operandSize == OperandSize.Byte)
            {
                return (uint)OperandSize.Word;
            }
            else
            {
                return (uint)operandSize;
            }
        }

        private (bool getOperand, EAProps props) GetAnIndirectDisplaceVal(uint idx, int opcodeSize)
        {
            var an = _regs.A[idx];
            int displacement = (short)_mem.ReadWord((uint)(_regs.PC + opcodeSize));
            uint address = (uint)(an + displacement);
            return (true, new()
            {
                Address = address,
                Location = StoreLocation.Memory,
                InstructionSize = 2,
            });
        }

        private (bool getOperand, EAProps props) GetIndexedAddressingVal(uint an, int opcodeSize)
        {
            var extWord = (ushort)_mem.ReadWord((uint)(_regs.PC + opcodeSize));
            int displacement = (sbyte)extWord;

            int indexRegisterIdx = (extWord >> 12) & 0x7;
            int indexRegister;
            if ((IndexRegister)((extWord >> 15) & 0x1) == IndexRegister.Address)
            {
                indexRegister = (int)_regs.A[indexRegisterIdx];
            }
            else
            {
                indexRegister = (int)_regs.D[indexRegisterIdx];
            }

            if ((IndexSize)((extWord & 0x0800) >> 11) == IndexSize.SignExtendedWord)
            {
                indexRegister = (short)(indexRegister & 0xFFFF);
            }

            int scale = (extWord & 0x0600) >> 9;
            
            uint address = (uint)(an + displacement + indexRegister * (int)Math.Pow(2, scale));

            return (true, new()
            {
                Address = address,
                Location = StoreLocation.Memory,
                InstructionSize = 2,
            });
        }

        private (bool getOperand, EAProps props) GetPCDisplaceVal(int opcodeSize)
        {
            var pc = _regs.PC;
            int displacement = (short)_mem.ReadWord((uint)(pc + opcodeSize));
            uint address = (uint)(pc + displacement);
            return (true, new()
            {
                Address = address,
                Location = StoreLocation.Memory,
                InstructionSize = 2,
            });
        }

        private (bool getOperand, EAProps props) GetAbsoluteVal(bool word, int opcodeSize)
        {
            var pc = _regs.PC;
            int displacement;
            if (word)
            {
                displacement = (short)_mem.ReadWord((uint)(pc + opcodeSize));
            }
            else
            {
                displacement = (int)_mem.ReadLong((uint)(pc + opcodeSize));
            }

            uint address = (uint)displacement;
            return (true, new()
            {
                Address = address,
                Location = StoreLocation.Memory,
                InstructionSize = word ? 2 : 4,
            });
        }

        private (bool getOperand, EAProps props) GetImmediateVal(OperandSize operandSize, int opcodeSize)
        {
            uint address = (uint)(_regs.PC + opcodeSize);
            uint operand = _memHelper.ReadImmediate(address, operandSize);
            int immediateDataLength = operandSize == OperandSize.Byte ? 2 : (int)operandSize;
            return (false, new()
            {
                Operand = operand,
                Address = address,
                Location = StoreLocation.Memory,
                InstructionSize = immediateDataLength,
            });
        }
    }
}
