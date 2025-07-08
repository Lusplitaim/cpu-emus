using CPEMUS.Motorola.M68000.EA;

namespace CPEMUS.Motorola.M68000
{
    public partial class M68K
    {
        private (int instructionSize, int pcDisplacement, bool isBranchTaken) BranchWithInternalDisplacement(ushort opcode, Func<ushort, bool> branchRequired)
        {
            int instructionSize = INSTR_DEFAULT_SIZE;
            int displacement = (sbyte)opcode;

            // If internal displacement is zero, take displacement
            // from immediate word next to the opcode.
            if (displacement == 0)
            {
                displacement = (int)_memHelper.Read(_regs.PC + INSTR_DEFAULT_SIZE, StoreLocation.ImmediateData, OperandSize.Word, signExtended: true);
                instructionSize += 2;
            }

            if (branchRequired(opcode))
            {
                return (instructionSize, INSTR_DEFAULT_SIZE + displacement, true);
            }
            return (instructionSize, instructionSize, false);
        }

        // Branch Conditionally.
        private M68KExecResult Bcc(ushort opcode)
        {
            (int _, int pcDisplacement, bool isBranchTaken) = BranchWithInternalDisplacement(opcode, (opcode) =>
            {
                var condition = (ConditionCode)((opcode >> 8) & 0xF);
                if (condition == ConditionCode.True || condition == ConditionCode.False)
                {
                    throw new InvalidOperationException("The branch condition is not allowed for Bcc instruction");
                }
                return TestCondition(condition);
            });

            int clockPeriods = isBranchTaken ? 2 : 4;

            return new()
            {
                InstructionSize = pcDisplacement,
                ClockPeriods = clockPeriods
            };
        }

        // Branch Always.
        private M68KExecResult Bra(ushort opcode)
        {
            (int _, int pcDisplacement, bool _) = BranchWithInternalDisplacement(opcode, (_) =>
            {
                return true;
            });

            int clockPeriods = 2;

            return new()
            {
                InstructionSize = pcDisplacement,
                ClockPeriods = clockPeriods
            };
        }

        // Branch to Subroutine.
        private M68KExecResult Bsr(ushort opcode)
        {
            (int instructionSize, int pcDisplacement, bool _) = BranchWithInternalDisplacement(opcode, (_) =>
            {
                return true;
            });

            // Pushing the address of the next instruction following bsr
            // to stack.
            _memHelper.PushStack((uint)(_regs.PC + instructionSize), OperandSize.Long);

            int clockPeriods = 2;

            return new()
            {
                InstructionSize = pcDisplacement,
                ClockPeriods = clockPeriods
            };
        }

        // Test Condition, Decrement, and Branch.
        private M68KExecResult Dbcc(ushort opcode)
        {
            var opcodeSize = INSTR_DEFAULT_SIZE;
            int clockPeriods = 2;

            var condition = (ConditionCode)((opcode >> 8) & 0xF);
            bool isConditionTrue = TestCondition(condition);
            if (isConditionTrue)
            {
                clockPeriods = 4;
                return new()
                {
                    InstructionSize = opcodeSize + 2,
                    ClockPeriods = clockPeriods,
                };
            }

            var operandSize = OperandSize.Word;
            uint dataRegIdx = (uint)(opcode & 0x7);
            var dataReg = _memHelper.Read(dataRegIdx, StoreLocation.DataRegister, operandSize);

            dataReg--;
            _memHelper.Write(dataReg, dataRegIdx, StoreLocation.DataRegister, operandSize);
            if ((short)dataReg == -1)
            {
                return new()
                {
                    InstructionSize = opcodeSize + 2,
                    ClockPeriods = clockPeriods,
                };
            }

            var displacement = (int)_memHelper.Read(_regs.PC + INSTR_DEFAULT_SIZE, StoreLocation.ImmediateData, operandSize, signExtended: true);

            return new()
            {
                InstructionSize = opcodeSize + displacement,
                ClockPeriods = clockPeriods,
            };
        }

        // Set According to Condition.
        private M68KExecResult Scc(ushort opcode)
        {
            var eaProps = _eaHelper.Get(opcode, OperandSize.Byte);

            var condition = (ConditionCode)((opcode >> 8) & 0xF);
            bool isTrue = TestCondition(condition);
            int result = isTrue ? -1 : 0;
            _memHelper.Write((uint)result, eaProps.Address, eaProps.Location, OperandSize.Byte);

            int clockPeriods = eaProps.ClockPeriods;
            if (isTrue && (eaProps.Mode == EAMode.DataDirect || eaProps.Mode == EAMode.AddressDirect))
            {
                clockPeriods += 2;
            }

            return new()
            {
                InstructionSize = eaProps.InstructionSize,
                ClockPeriods = clockPeriods,
            };
        }

        private bool TestCondition(ConditionCode condition)
        {
            bool carry = _regs.C;
            bool extended = _regs.X;
            bool negative = _regs.N;
            bool overflow = _regs.V;
            bool zero = _regs.Z;
            return condition switch
            {
                ConditionCode.True => true,
                ConditionCode.False => false,
                ConditionCode.High => !carry && !zero,
                ConditionCode.LowOrSame => carry || zero,
                ConditionCode.CarryClear => !carry,
                ConditionCode.CarrySet => carry,
                ConditionCode.NotEqual => !zero,
                ConditionCode.Equal => zero,
                ConditionCode.OverflowClear => !overflow,
                ConditionCode.OverflowSet => overflow,
                ConditionCode.Plus => !negative,
                ConditionCode.Minus => negative,
                ConditionCode.GreaterOrEqual => negative == overflow,
                ConditionCode.LessThan => negative != overflow,
                ConditionCode.GreaterThan => negative == overflow && !zero,
                ConditionCode.LessOrEqual => zero || negative != overflow,
                _ => throw new InvalidOperationException("Condition test is unknown or not supported"),
            };
        }

        private Dictionary<EAMode, int> _jpmClockPeriods = new()
        {
            { EAMode.AddressIndirect, 8 },
            { EAMode.BaseDisplacement16, 10 },
            { EAMode.IndexedAddressing, 14 },
            { EAMode.AbsoluteShort, 10 },
            { EAMode.AbsoluteLong, 12 },
            { EAMode.PCDisplacement16, 10 },
            { EAMode.PCDisplacement8, 14 },
        };
        // Jump.
        private M68KExecResult Jmp(ushort opcode)
        {
            var eaProps = _eaHelper.Get(opcode, OperandSize.Long, loadOperand: false);
            _regs.PC = eaProps.Address;
            int clockPeriods = _jpmClockPeriods[eaProps.Mode];
            bool isTotalCycleCount = true;

            return new()
            {
                InstructionSize = 0,
                ClockPeriods = clockPeriods,
                IsTotalCycleCount = isTotalCycleCount
            };
        }

        private Dictionary<EAMode, int> _jsrClockPeriods = new()
        {
            { EAMode.AddressIndirect, 16 },
            { EAMode.BaseDisplacement16, 18 },
            { EAMode.IndexedAddressing, 22 },
            { EAMode.AbsoluteShort, 18 },
            { EAMode.AbsoluteLong, 20 },
            { EAMode.PCDisplacement16, 18 },
            { EAMode.PCDisplacement8, 22 },
        };
        // Jump to Subroutine.
        private M68KExecResult Jsr(ushort opcode)
        {
            var eaProps = _eaHelper.Get(opcode, OperandSize.Long, loadOperand: false);
            _memHelper.PushStack((uint)(_regs.PC + eaProps.InstructionSize), OperandSize.Long);
            _regs.PC = eaProps.Address;
            int clockPeriods = _jsrClockPeriods[eaProps.Mode];
            bool isTotalCycleCount = true;

            return new()
            {
                InstructionSize = 0,
                ClockPeriods = clockPeriods,
                IsTotalCycleCount = isTotalCycleCount
            };
        }

        // No Operation.
        private M68KExecResult Nop(ushort opcode)
        {
            int clockPeriods = 0;
            return new()
            {
                InstructionSize = INSTR_DEFAULT_SIZE,
                ClockPeriods = clockPeriods,
            };
        }

        // Return and Restore Condition Codes.
        private M68KExecResult Rtr(ushort opcode)
        {
            var sr = _memHelper.PopStack(OperandSize.Word);
            _regs.CCR = (byte)sr;

            var pc = _memHelper.PopStack(OperandSize.Long);
            _regs.PC = pc;

            int clockPeriods = 20;
            bool isTotalPeriods = true;

            return new()
            {
                InstructionSize = 0,
                ClockPeriods = clockPeriods,
                IsTotalCycleCount = isTotalPeriods,
            };
        }

        // Return from Subroutine.
        private M68KExecResult Rts(ushort opcode)
        {
            var pc = _memHelper.PopStack(OperandSize.Long);
            _regs.PC = pc;

            int clockPeriods = 16;
            bool isTotalPeriods = true;

            return new()
            {
                InstructionSize = 0,
                ClockPeriods = clockPeriods,
                IsTotalCycleCount = isTotalPeriods,
            };
        }

        // Test an Operand.
        private M68KExecResult Tst(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);
            var eaProps = _eaHelper.Get(opcode, operandSize);

            _flagsHelper.AlterN(eaProps.Operand, operandSize);
            _flagsHelper.AlterZ(eaProps.Operand, operandSize);
            _regs.V = _regs.C = false;

            int clockPeriods = eaProps.ClockPeriods;

            return new()
            {
                InstructionSize = eaProps.InstructionSize,
                ClockPeriods = clockPeriods,
            };
        }
    }

    internal enum ConditionCode
    {
        True = 0x0,
        False = 0x1,
        High = 0x2,
        LowOrSame = 0x3,
        CarryClear = 0x4,
        CarrySet = 0x5,
        NotEqual = 0x6,
        Equal = 0x7,
        OverflowClear = 0x8,
        OverflowSet = 0x9,
        Plus = 0xA,
        Minus = 0xB,
        GreaterOrEqual = 0xC,
        LessThan = 0xD,
        GreaterThan = 0xE,
        LessOrEqual = 0xF,
    }
}
