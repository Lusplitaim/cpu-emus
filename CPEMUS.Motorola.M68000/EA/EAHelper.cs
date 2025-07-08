using CPEMUS.Motorola.M68000.Exceptions;
using CPEMUS.Motorola.M68000.Helpers;

namespace CPEMUS.Motorola.M68000.EA
{
    internal class EAHelper
    {
        private const int DEFAULT_OPCODE_SIZE = 2;

        private readonly Dictionary<EAMode, int> _additionalClockPeriods = new()
        {
            { EAMode.DataDirect, 0 },
            { EAMode.AddressDirect, 0 },
            { EAMode.AddressIndirect, 0 },
            { EAMode.PostincIndirect, 0 },
            { EAMode.PredecIndirect, 2 },
            { EAMode.BaseDisplacement16, 0 },
            { EAMode.IndexedAddressing, 2 },
            { EAMode.AbsoluteShort, 0 },
            { EAMode.AbsoluteLong, 0 },
            { EAMode.PCDisplacement8, 2 },
            { EAMode.PCDisplacement16, 0 },
            { EAMode.ImmediateData, 0 },
        };

        private readonly M68KRegs _regs;
        private readonly MemHelper _memHelper;
        private readonly IList<byte> _mem;
        public EAHelper(M68KRegs regs, IList<byte> memory, MemHelper memHelper)
        {
            _regs = regs;
            _mem = memory;
            _memHelper = memHelper;
        }

        public EAProps Get(EAMode eaMode, int registerField, OperandSize operandSize, int opcodeSize = DEFAULT_OPCODE_SIZE, bool signExtended = false)
        {
            ushort pseudoOpcode = (ushort)(((int)eaMode << 3) | (registerField & 0x7));
            return Get(pseudoOpcode, operandSize, opcodeSize, signExtended);
        }

        public EAProps Get(ushort opcode, OperandSize operandSize, int opcodeSize = DEFAULT_OPCODE_SIZE,
            bool signExtended = false, bool loadOperand = true)
        {
            EAProps result = GetProps(opcode, operandSize, opcodeSize);

            int additionalClockPeriods = _additionalClockPeriods[result.Mode];
            result.ClockPeriods += additionalClockPeriods;
            if (loadOperand)
            {
                result.Operand = _memHelper.Read(result.Address, result.Location, operandSize, signExtended);
            }
            result.InstructionSize += opcodeSize;

            return result;
        }

        private EAProps GetProps(ushort opcode, OperandSize operandSize, int opcodeSize)
        {
            uint registerField = (uint)(opcode & 0x7);
            var mode = (EAMode)((opcode >> 3) & 0x7);
            EAProps result;

            if (!Enum.IsDefined(mode))
            {
                mode = (EAMode)(((int)mode << 4) | (int)registerField);
            }

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
                case EAMode.BaseDisplacement16: // EA = [An + displacement16].
                    result = GetAnIndirectDisplaceVal(registerField, opcodeSize);
                    break;
                case EAMode.IndexedAddressing: // EA = [An + displacement8 + Xn.size * scale].
                    result = GetIndexedAddressingVal(ReadAddressReg(registerField), opcodeSize);
                    break;
                case EAMode.AbsoluteShort:
                    result = GetAbsoluteVal(word: true, opcodeSize);
                    break;
                case EAMode.AbsoluteLong:
                    result = GetAbsoluteVal(word: false, opcodeSize);
                    break;
                case EAMode.PCDisplacement16: // EA = [PC + displacement16].
                    result = GetPCDisplaceVal(opcodeSize);
                    break;
                case EAMode.PCDisplacement8: // EA = [PC + displacement8 + Xn.size * scale].
                    result = GetIndexedAddressingVal((uint)(_regs.PC + opcodeSize), opcodeSize);
                    break;
                case EAMode.ImmediateData:
                    result = GetImmediateVal(operandSize, opcodeSize);
                    break;
                default:
                    throw new IllegalInstructionException();
            }

            result.Mode = mode;

            return result;
        }

        private uint ReadAddressReg(uint idx)
        {
            return _memHelper.Read(idx, StoreLocation.AddressRegister, OperandSize.Long);
        }

        private void WriteToAddressReg(uint idx, uint val)
        {
            _memHelper.Write(val, idx, StoreLocation.AddressRegister, OperandSize.Long);
        }

        private EAProps GetDnDirectVal(uint idx)
        {
             return new()
             {
                 Address = idx,
                 Location = StoreLocation.DataRegister,
             };
        }

        private EAProps GetAnDirectVal(uint idx)
        {
            return new()
            {
                Address = idx,
                Location = StoreLocation.AddressRegister,
            };
        }

        private EAProps GetAnIndirectVal(uint idx)
        {
            return new()
            {
                Address = ReadAddressReg(idx),
                Location = StoreLocation.Memory,
            };
        }

        private EAProps GetAnIndirectPostincVal(uint idx, OperandSize operandSize)
        {
            uint address = ReadAddressReg(idx);
            WriteToAddressReg(idx, ReadAddressReg(idx) + CalcAddressIncOrDecVal(idx, operandSize));
            return new()
            {
                Address = address,
                Location = StoreLocation.Memory,
            };
        }

        private EAProps GetAnIndirectPredecVal(uint idx, OperandSize operandSize)
        {
            WriteToAddressReg(idx, ReadAddressReg(idx) - CalcAddressIncOrDecVal(idx, operandSize));
            uint address = ReadAddressReg(idx);
            return new()
            {
                Address = address,
                Location = StoreLocation.Memory,
            };
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

        private EAProps GetAnIndirectDisplaceVal(uint idx, int opcodeSize)
        {
            var an = ReadAddressReg(idx);
            int displacement = (short)_memHelper.Read((uint)(_regs.PC + opcodeSize), StoreLocation.Memory, OperandSize.Word);
            uint address = (uint)(an + displacement);
            return new()
            {
                Address = address,
                Location = StoreLocation.Memory,
                InstructionSize = 2,
            };
        }

        private EAProps GetIndexedAddressingVal(uint an, int opcodeSize)
        {
            var extWord = (ushort)_memHelper.Read((uint)(_regs.PC + opcodeSize), StoreLocation.Memory, OperandSize.Word);
            int displacement = (sbyte)extWord;

            int indexRegisterIdx = (extWord >> 12) & 0x7;
            int indexRegister;
            if ((IndexRegister)((extWord >> 15) & 0x1) == IndexRegister.Address)
            {
                indexRegister = (int)ReadAddressReg((uint)indexRegisterIdx);
            }
            else
            {
                indexRegister = (int)_regs.D[indexRegisterIdx];
            }

            if ((IndexSize)((extWord >> 11) & 0x1) == IndexSize.SignExtendedWord)
            {
                indexRegister = (short)(indexRegister & 0xFFFF);
            }

            int scale = (extWord >> 9) & 0x3;
            
            uint address = (uint)(an + displacement + indexRegister);

            return new()
            {
                Address = address,
                Location = StoreLocation.Memory,
                InstructionSize = 2,
            };
        }

        private EAProps GetPCDisplaceVal(int opcodeSize)
        {
            var pc = _regs.PC;
            int displacement = (short)_memHelper.Read((uint)(pc + opcodeSize), StoreLocation.Memory, OperandSize.Word);
            uint address = (uint)(pc + opcodeSize + displacement);
            return new()
            {
                Address = address,
                Location = StoreLocation.Memory,
                InstructionSize = 2,
            };
        }

        private EAProps GetAbsoluteVal(bool word, int opcodeSize)
        {
            var pc = _regs.PC;
            int displacement;
            if (word)
            {
                displacement = (short)_memHelper.Read((uint)(pc + opcodeSize), StoreLocation.Memory, OperandSize.Word);
            }
            else
            {
                displacement = (int)_memHelper.Read((uint)(pc + opcodeSize), StoreLocation.Memory, OperandSize.Long);
            }

            uint address = (uint)displacement;
            return new()
            {
                Address = address,
                Location = StoreLocation.Memory,
                InstructionSize = word ? 2 : 4,
            };
        }

        private EAProps GetImmediateVal(OperandSize operandSize, int opcodeSize)
        {
            uint address = (uint)(_regs.PC + opcodeSize);
            int immediateDataLength = operandSize == OperandSize.Byte ? 2 : (int)operandSize;
            return new()
            {
                Address = address,
                Location = StoreLocation.ImmediateData,
                InstructionSize = immediateDataLength,
            };
        }
    }
}
