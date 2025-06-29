namespace CPEMUS.Motorola.M68000
{
    public partial class M68K
    {
        private (int instructionSize, int pcDisplacement) BranchWithInternalDisplacement(ushort opcode, Func<ushort, bool> branchRequired)
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
                return (instructionSize, INSTR_DEFAULT_SIZE + displacement);
            }
            return (instructionSize, instructionSize);
        }

        // Branch Conditionally.
        private int Bcc(ushort opcode)
        {
            (int _, int pcDisplacement) = BranchWithInternalDisplacement(opcode, (opcode) =>
            {
                var condition = (ConditionCode)((opcode >> 8) & 0xF);
                if (condition == ConditionCode.True || condition == ConditionCode.False)
                {
                    throw new InvalidOperationException("The branch condition is not allowed for Bcc instruction");
                }
                return TestCondition(condition);
            });

            return pcDisplacement;
        }

        // Branch Always.
        private int Bra(ushort opcode)
        {
            (int _, int pcDisplacement) = BranchWithInternalDisplacement(opcode, (_) =>
            {
                return true;
            });

            return pcDisplacement;
        }

        // Branch to Subroutine.
        private int Bsr(ushort opcode)
        {
            (int instructionSize, int pcDisplacement) = BranchWithInternalDisplacement(opcode, (_) =>
            {
                return true;
            });

            // Pushing the address of the next instruction following bsr
            // to stack.
            _memHelper.PushStack((uint)(_regs.PC + instructionSize), OperandSize.Long);

            return pcDisplacement;
        }

        // Test Condition, Decrement, and Branch.
        private int Dbcc(ushort opcode)
        {
            //var instructionSize = INSTR_DEFAULT_SIZE + 2;
            var opcodeSize = INSTR_DEFAULT_SIZE;

            var condition = (ConditionCode)((opcode >> 8) & 0xF);
            if (TestCondition(condition))
            {
                return opcodeSize + 2;
            }

            var operandSize = OperandSize.Word;
            uint dataRegIdx = (uint)(opcode & 0x7);
            var dataReg = _memHelper.Read(dataRegIdx, StoreLocation.DataRegister, operandSize);

            dataReg--;
            _memHelper.Write(dataReg, dataRegIdx, StoreLocation.DataRegister, operandSize);
            if ((short)dataReg == -1)
            {
                return opcodeSize + 2;
            }

            var displacement = (int)_memHelper.Read(_regs.PC + INSTR_DEFAULT_SIZE, StoreLocation.ImmediateData, operandSize, signExtended: true);
            return opcodeSize + displacement;
        }

        // Set According to Condition.
        private int Scc(ushort opcode)
        {
            var eaProps = _eaHelper.Get(opcode, OperandSize.Byte);

            var condition = (ConditionCode)((opcode >> 8) & 0xF);
            int result = TestCondition(condition) ? -1 : 0;
            _memHelper.Write((uint)result, eaProps.Address, eaProps.Location, OperandSize.Byte);

            return eaProps.InstructionSize;
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

        // Jump.
        private int Jmp(ushort opcode)
        {
            var eaProps = _eaHelper.Get(opcode, OperandSize.Long);
            _regs.PC = eaProps.Address;
            return 0;
        }

        // Jump to Subroutine.
        private int Jsr(ushort opcode)
        {
            var eaProps = _eaHelper.Get(opcode, OperandSize.Long);
            _memHelper.PushStack((uint)(_regs.PC + eaProps.InstructionSize), OperandSize.Long);
            _regs.PC = eaProps.Address;
            return 0;
        }

        // No Operation.
        private int Nop(ushort opcode)
        {
            return INSTR_DEFAULT_SIZE;
        }

        // Return and Restore Condition Codes.
        private int Rtr(ushort opcode)
        {
            var sr = _memHelper.PopStack(OperandSize.Word);
            _regs.CCR = (byte)sr;

            var pc = _memHelper.PopStack(OperandSize.Long);
            _regs.PC = pc;

            return 0;
        }

        // Return from Subroutine.
        private int Rts(ushort opcode)
        {
            var pc = _memHelper.PopStack(OperandSize.Long);
            _regs.PC = pc;

            return 0;
        }

        // Test an Operand.
        private int Tst(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);
            var eaProps = _eaHelper.Get(opcode, operandSize);

            _flagsHelper.AlterN(eaProps.Operand, operandSize);
            _flagsHelper.AlterZ(eaProps.Operand, operandSize);
            _regs.V = _regs.C = false;

            return eaProps.InstructionSize;
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
