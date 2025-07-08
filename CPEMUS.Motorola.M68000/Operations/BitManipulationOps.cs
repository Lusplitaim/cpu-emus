using CPEMUS.Motorola.M68000.EA;

namespace CPEMUS.Motorola.M68000
{
    public partial class M68K
    {
        private (int instrSize, EAProps eaProps) TestAndChangeBit(ushort opcode, bool srcImmediate, bool updateDestValue, Func<uint, int, uint> changeBit)
        {
            var mode = (EAMode)((opcode >> 3) & 0x7);
            var operandSize = mode == EAMode.DataDirect ? OperandSize.Long : OperandSize.Byte;
            var opcodeSize = srcImmediate ? 4 : 2;

            int bitNumber;
            if (srcImmediate)
            {
                bitNumber = (int)_memHelper.Read(_regs.PC + INSTR_DEFAULT_SIZE, StoreLocation.ImmediateData, OperandSize.Word) % ((int)operandSize * 8);
            }
            else
            {
                var dataRegIdx = (uint)((opcode >> 9) & 0x7);
                bitNumber = (int)(_memHelper.Read(dataRegIdx, StoreLocation.DataRegister, OperandSize.Long) % ((int)operandSize * 8));
            }

            var eaResult = _eaHelper.Get(opcode, operandSize, opcodeSize);

            _flagsHelper.AlterZ((eaResult.Operand >> bitNumber) & 0x1, OperandSize.Byte);

            var result = changeBit(eaResult.Operand, bitNumber);

            if (updateDestValue)
            {
                _memHelper.Write(result, eaResult.Address, eaResult.Location, operandSize);
            }

            return (eaResult.InstructionSize, eaResult);
        }

        // Test Bit and Change.
        private M68KExecResult Bchg(ushort opcode, bool srcImmediate)
        {
            (int instrSize, EAProps eaProps) = TestAndChangeBit(opcode, srcImmediate, updateDestValue: true, (operand, bitNumber) =>
            {
                return (uint)(operand ^ (1 << bitNumber));
            });

            int clockPeriods = eaProps.ClockPeriods;
            if (eaProps.Mode == EAMode.DataDirect)
            {
                clockPeriods += 4;
            }

            return new()
            {
                InstructionSize = instrSize,
                ClockPeriods = clockPeriods
            };
        }

        // Test Bit and Clear.
        private M68KExecResult Bclr(ushort opcode, bool srcImmediate)
        {
            (int instrSize, EAProps eaProps) = TestAndChangeBit(opcode, srcImmediate, updateDestValue: true, (operand, bitNumber) =>
            {
                return (uint)(operand & ~(1 << bitNumber));
            });

            int clockPeriods = eaProps.ClockPeriods;
            if (eaProps.Mode == EAMode.DataDirect)
            {
                clockPeriods += 6;
            }

            return new()
            {
                InstructionSize = instrSize,
                ClockPeriods = clockPeriods
            };
        }

        // Test Bit and Set.
        private M68KExecResult Bset(ushort opcode, bool srcImmediate)
        {
            (int instrSize, EAProps eaProps) = TestAndChangeBit(opcode, srcImmediate, updateDestValue: true, (operand, bitNumber) =>
            {
                return operand | (uint)(1 << bitNumber);
            });

            int clockPeriods = eaProps.ClockPeriods;
            if (eaProps.Mode == EAMode.DataDirect)
            {
                clockPeriods += 4;
            }

            return new()
            {
                InstructionSize = instrSize,
                ClockPeriods = clockPeriods
            };
        }

        private M68KExecResult Btst(ushort opcode, bool srcImmediate)
        {
            (int instrSize, EAProps eaProps) = TestAndChangeBit(opcode, srcImmediate, updateDestValue: false, (operand, bitNumber) =>
            {
                return operand;
            });

            int clockPeriods = eaProps.ClockPeriods;
            if (eaProps.Mode == EAMode.DataDirect)
            {
                clockPeriods += 2;
            }

            return new()
            {
                InstructionSize = instrSize,
                ClockPeriods = clockPeriods
            };
        }
    }
}
