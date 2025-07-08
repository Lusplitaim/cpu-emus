using CPEMUS.Motorola.M68000.EA;
using CPEMUS.Motorola.M68000.Exceptions;

namespace CPEMUS.Motorola.M68000
{
    public partial class M68K
    {
        // Exchange Registers.
        private M68KExecResult Exg(ushort opcode)
        {
            var srcRegIdx = (uint)((opcode >> 9) & 0x7);
            var destRegIdx = (uint)(opcode & 0x7);
            StoreLocation destRegLocation;
            StoreLocation srcRegLocation;

            var mode = (opcode >> 3) & 0x1F;
            switch (mode)
            {
                case 8:
                    destRegLocation = StoreLocation.DataRegister;
                    srcRegLocation = StoreLocation.DataRegister;
                    break;
                case 9:
                    destRegLocation = StoreLocation.AddressRegister;
                    srcRegLocation = StoreLocation.AddressRegister;
                    break;
                case 17:
                    destRegLocation = StoreLocation.AddressRegister;
                    srcRegLocation = StoreLocation.DataRegister;
                    break;
                default:
                    throw new InvalidOperationException("Incorrect Exg opcode mode");
            }

            var operandSize = OperandSize.Long;
            var destReg = _memHelper.Read(destRegIdx, destRegLocation, operandSize);
            var srcReg = _memHelper.Read(srcRegIdx, srcRegLocation, operandSize);

            _memHelper.Write(srcReg, destRegIdx, destRegLocation, operandSize);
            _memHelper.Write(destReg, srcRegIdx, srcRegLocation, operandSize);

            int clockPeriods = 2;

            return new()
            {
                InstructionSize = INSTR_DEFAULT_SIZE,
                ClockPeriods = clockPeriods,
            };
        }

        private Dictionary<EAMode, int> _leaClockPeriods = new()
        {
            { EAMode.AddressIndirect, 4 },
            { EAMode.BaseDisplacement16, 8 },
            { EAMode.IndexedAddressing, 12 },
            { EAMode.AbsoluteShort, 8 },
            { EAMode.AbsoluteLong, 12 },
            { EAMode.PCDisplacement16, 8 },
            { EAMode.PCDisplacement8, 12 },
        };
        // Load Effective Address.
        private M68KExecResult Lea(ushort opcode)
        {
            var addrRegIdx = (uint)((opcode >> 9) & 0x7);
            var eaProps = _eaHelper.Get(opcode, OperandSize.Long, loadOperand: false);
            _memHelper.Write(eaProps.Address, addrRegIdx, StoreLocation.AddressRegister, OperandSize.Long);

            int clockPeriods = _leaClockPeriods[eaProps.Mode];
            bool isTotalCycleCount = true;

            return new()
            {
                InstructionSize = eaProps.InstructionSize,
                ClockPeriods = clockPeriods,
                IsTotalCycleCount = isTotalCycleCount,
            };
        }

        private Dictionary<EAMode, int> _peaClockPeriods = new()
        {
            { EAMode.AddressIndirect, 12 },
            { EAMode.BaseDisplacement16, 16 },
            { EAMode.IndexedAddressing, 20 },
            { EAMode.AbsoluteShort, 16 },
            { EAMode.AbsoluteLong, 20 },
            { EAMode.PCDisplacement16, 16 },
            { EAMode.PCDisplacement8, 20 },
        };
        // Push Effective Address.
        private M68KExecResult Pea(ushort opcode)
        {
            var eaProps = _eaHelper.Get(opcode, OperandSize.Long, loadOperand: false);
            _memHelper.PushStack(eaProps.Address, OperandSize.Long);

            int clockPeriods = _peaClockPeriods[eaProps.Mode];
            bool isTotalCycleCount = true;

            return new()
            {
                InstructionSize = eaProps.InstructionSize,
                ClockPeriods = clockPeriods,
                IsTotalCycleCount = isTotalCycleCount,
            };
        }

        // Link and Allocate.
        private M68KExecResult Link(ushort opcode)
        {
            var addrRegIdx = (uint)(opcode & 0x7);
            var addrReg = _memHelper.Read(addrRegIdx, StoreLocation.AddressRegister, OperandSize.Long);
            _memHelper.PushStack(addrReg, OperandSize.Long);

            _memHelper.Write(_regs.SP, addrRegIdx, StoreLocation.AddressRegister, OperandSize.Long);

            int displacement = (int)_memHelper.Read(_regs.PC + INSTR_DEFAULT_SIZE, StoreLocation.ImmediateData, OperandSize.Word, signExtended: true);
            _regs.SP = (uint)(_regs.SP + displacement);

            int clockPeriods = 0;

            return new()
            {
                InstructionSize = INSTR_DEFAULT_SIZE + 2,
                ClockPeriods = clockPeriods,
            };
        }

        // Unlink.
        private M68KExecResult Unlk(ushort opcode)
        {
            var addrRegIdx = (uint)(opcode & 0x7);
            var addrReg = _memHelper.Read(addrRegIdx, StoreLocation.AddressRegister, OperandSize.Long);

            _regs.SP = addrReg;
            var newAddrRegValue = _memHelper.PopStack(OperandSize.Long);

            _memHelper.Write(newAddrRegValue, addrRegIdx, StoreLocation.AddressRegister, OperandSize.Long);

            int clockPeriods = 0;

            return new()
            {
                InstructionSize = INSTR_DEFAULT_SIZE,
                ClockPeriods = clockPeriods,
            };
        }

        // Move Data from Source to Destination.
        private M68KExecResult Move(ushort opcode)
        {
            OperandSize operandSize;
            switch ((opcode >> 12) & 0x3)
            {
                case 1:
                    operandSize = OperandSize.Byte;
                    break;
                case 3:
                    operandSize = OperandSize.Word;
                    break;
                case 2:
                    operandSize = OperandSize.Long;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var srcEaProps = _eaHelper.Get(opcode, operandSize);

            var destEaModeField = (opcode >> 6) & 0x7;
            var destEaRegField = (opcode >> 9) & 0x7;
            var destEaProps = _eaHelper.Get((ushort)((destEaModeField << 3) | destEaRegField), operandSize,
                loadOperand: false, opcodeSize: srcEaProps.InstructionSize);

            _memHelper.Write(srcEaProps.Operand, destEaProps.Address, destEaProps.Location, operandSize);

            _flagsHelper.AlterN(srcEaProps.Operand, operandSize);
            _flagsHelper.AlterZ(srcEaProps.Operand, operandSize);
            _regs.V = false;
            _regs.C = false;

            int clockPeriods = srcEaProps.ClockPeriods;
            if (destEaProps.Mode != EAMode.PredecIndirect)
            {
                clockPeriods += destEaProps.ClockPeriods;
            }

            return new()
            {
                InstructionSize = destEaProps.InstructionSize,
                ClockPeriods = clockPeriods,
            };
        }

        // Move Address.
        private M68KExecResult Movea(ushort opcode)
        {
            OperandSize operandSize;
            switch ((opcode >> 12) & 0x3)
            {
                case 3:
                    operandSize = OperandSize.Word;
                    break;
                case 2:
                    operandSize = OperandSize.Long;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var srcEaProps = _eaHelper.Get(opcode, operandSize, signExtended: true);
            var addressRegIdx = (uint)((opcode >> 9) & 0x7);

            // All 32 bits are loaded into the address register.
            _memHelper.Write(srcEaProps.Operand, addressRegIdx, StoreLocation.AddressRegister, OperandSize.Long);

            int clockPeriods = srcEaProps.ClockPeriods;

            return new()
            {
                InstructionSize = srcEaProps.InstructionSize,
                ClockPeriods = clockPeriods,
            };
        }

        // Move to CCR.
        private M68KExecResult MoveToCcr(ushort opcode)
        {
            var eaProps = _eaHelper.Get(opcode, OperandSize.Word);

            _regs.CCR = (byte)eaProps.Operand;

            int clockPeriods = eaProps.ClockPeriods + 8;

            return new()
            {
                InstructionSize = eaProps.InstructionSize,
                ClockPeriods = clockPeriods,
            };
        }

        private Dictionary<EAMode, int> _movemRegAdditionalClockPeriods = new()
        {
            { EAMode.AddressIndirect, 12 },
            { EAMode.PostincIndirect, 12 },
            { EAMode.BaseDisplacement16, 16 },
            { EAMode.IndexedAddressing, 18 },
            { EAMode.AbsoluteShort, 16 },
            { EAMode.AbsoluteLong, 20 },
            { EAMode.PCDisplacement16, 16 },
            { EAMode.PCDisplacement8, 18 },
        };
        private Dictionary<EAMode, int> _movemMemAdditionalClockPeriods = new()
        {
            { EAMode.AddressIndirect, 8 },
            { EAMode.PredecIndirect, 8 },
            { EAMode.BaseDisplacement16, 12 },
            { EAMode.IndexedAddressing, 14 },
            { EAMode.AbsoluteShort, 12 },
            { EAMode.AbsoluteLong, 16 },
        };
        // Move Multiple Registers.
        private M68KExecResult Movem(ushort opcode)
        {
            var operandSize = ((opcode >> 6) & 0x1) == 0 ? OperandSize.Word : OperandSize.Long;
            ushort regsMask = (ushort)_memHelper.Read(_regs.PC + INSTR_DEFAULT_SIZE, StoreLocation.ImmediateData, OperandSize.Word);
            uint initialAddress;
            uint registerField = (uint)(opcode & 0x7);
            int instructionSize = 4;
            var eaMode = (EAMode)((opcode >> 3) & 0x7);
            switch (eaMode)
            {
                case EAMode.PredecIndirect:
                    ushort reversedRegsMask = 0;
                    uint regCount = 0;
                    for (int i = 0; i < 16; i++)
                    {
                        var bitValue = (regsMask >> i) & 0x1;
                        if (bitValue == 1)
                        {
                            regCount++;
                        }
                        reversedRegsMask = (ushort)(reversedRegsMask | (bitValue << (15-i)));
                    }
                    regsMask = reversedRegsMask;

                    initialAddress = _memHelper.Read(registerField, StoreLocation.AddressRegister, OperandSize.Long);
                    initialAddress -= (uint)operandSize * regCount;
                    break;
                case EAMode.PostincIndirect:
                    initialAddress = _memHelper.Read(registerField, StoreLocation.AddressRegister, OperandSize.Long);
                    break;
                default:
                    var eaProps = _eaHelper.Get(opcode, OperandSize.Long, opcodeSize: 4, loadOperand: false);
                    initialAddress = eaProps.Address;
                    instructionSize = eaProps.InstructionSize;
                    eaMode = eaProps.Mode;
                    break;
            }

            bool regToMem = ((opcode >> 10) & 0x1) == 0;

            bool isModeAllowed = regToMem
                ? _movemMemAdditionalClockPeriods.TryGetValue(eaMode, out var clockPeriods)
                : _movemRegAdditionalClockPeriods.TryGetValue(eaMode, out clockPeriods);
            if (!isModeAllowed)
            {
                throw new IllegalInstructionException();
            }

            (uint finalAddress, int movedRegCount) = regToMem
                ? MovemToMem(regsMask, initialAddress, operandSize)
                : MovemToReg(regsMask, initialAddress, operandSize);

            if (eaMode == EAMode.PredecIndirect)
            {
                _memHelper.Write(initialAddress, registerField, StoreLocation.AddressRegister, OperandSize.Long);
            }
            else if (eaMode == EAMode.PostincIndirect)
            {
                _memHelper.Write(finalAddress, registerField, StoreLocation.AddressRegister, OperandSize.Long);
            }

            bool isTotalCycleCount = true;
            clockPeriods += movedRegCount * 2 * (int)operandSize;

            return new()
            {
                InstructionSize = instructionSize,
                ClockPeriods = clockPeriods,
                IsTotalCycleCount = isTotalCycleCount,
            };
        }

        private (uint finalAddress, int movedRegCount) MovemToMem(ushort regsMask, uint address, OperandSize operandSize)
        {
            int movedRegCount = 0;
            for (int i = 0; i < 8; i++)
            {
                bool shouldStore = ((regsMask >> i) & 0x1) == 1;
                if (shouldStore)
                {
                    movedRegCount++;
                    var reg = _memHelper.Read((uint)i, StoreLocation.DataRegister, operandSize);
                    _memHelper.Write(reg, address, StoreLocation.Memory, operandSize);
                    address += (uint)operandSize;
                }
            }

            for (int i = 0; i < 8; i++)
            {
                bool shouldStore = ((regsMask >> (i + 8)) & 0x1) == 1;
                if (shouldStore)
                {
                    movedRegCount++;
                    var reg = _memHelper.Read((uint)i, StoreLocation.AddressRegister, operandSize);
                    _memHelper.Write(reg, address, StoreLocation.Memory, operandSize);
                    address += (uint)operandSize;
                }
            }

            return (address, movedRegCount);
        }

        private (uint finalAddress, int movedRegCount) MovemToReg(ushort regsMask, uint address, OperandSize operandSize)
        {
            int movedRegCount = 0;
            for (int i = 0; i < 8; i++)
            {
                bool shouldStore = ((regsMask >> i) & 0x1) == 1;
                if (shouldStore)
                {
                    movedRegCount++;
                    var regInMem = _memHelper.Read(address, StoreLocation.Memory, operandSize, signExtended: true);
                    _memHelper.Write(regInMem, (uint)i, StoreLocation.DataRegister, OperandSize.Long);
                    address += (uint)operandSize;
                }
            }

            for (int i = 0; i < 8; i++)
            {
                bool shouldStore = ((regsMask >> (i + 8)) & 0x1) == 1;
                if (shouldStore)
                {
                    movedRegCount++;
                    var regInMem = _memHelper.Read(address, StoreLocation.Memory, operandSize, signExtended: true);
                    _memHelper.Write(regInMem, (uint)i, StoreLocation.AddressRegister, OperandSize.Long);
                    address += (uint)operandSize;
                }
            }

            return (address, movedRegCount);
        }

        // Move Peripheral Data.
        private M68KExecResult Movep(ushort opcode)
        {
            throw new NotImplementedException();
        }

        // Move Quick.
        private M68KExecResult Moveq(ushort opcode)
        {
            var dataRegIdx = (uint)((opcode >> 9) & 0x7);
            int innerData = (sbyte)opcode;

            _memHelper.Write((uint)innerData, dataRegIdx, StoreLocation.DataRegister, OperandSize.Long);

            _flagsHelper.AlterZ((uint)innerData, OperandSize.Long);
            _flagsHelper.AlterN((uint)innerData, OperandSize.Long);
            _regs.V = false;
            _regs.C = false;

            return new()
            {
                InstructionSize = INSTR_DEFAULT_SIZE,
                ClockPeriods = 0,
            };
        }
    }
}
