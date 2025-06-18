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
                displacement = (int)_memHelper.ReadImmediate(_regs.PC + INSTR_DEFAULT_SIZE, OperandSize.Word, signExtended: true);
                instructionSize += 2;
            }

            if (branchRequired(opcode))
            {
                return (instructionSize, instructionSize + displacement);
            }
            return (instructionSize, instructionSize);
        }

        // Branch Conditionally.
        private int Bcc(ushort opcode)
        {
            (int _, int pcDisplacement) = BranchWithInternalDisplacement(opcode, (opcode) =>
            {
                var condition = (BranchCondition)((opcode >> 8) & 0xF);
                if (condition == BranchCondition.True || condition == BranchCondition.False)
                {
                    throw new InvalidOperationException("The branch condition is not allowed for Bcc instruction");
                }
                return TestBranchCondition(condition);
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
            PushStack((uint)(_regs.PC + instructionSize), OperandSize.Long);

            return pcDisplacement;
        }

        // Test Condition, Decrement, and Branch.
        private int Dbcc(ushort opcode)
        {
            var instructionSize = INSTR_DEFAULT_SIZE + 2;

            var condition = (BranchCondition)((opcode >> 8) & 0xF);
            if (TestBranchCondition(condition))
            {
                return instructionSize;
            }

            var operandSize = OperandSize.Word;
            uint dataRegIdx = (uint)(opcode & 0x7);
            var dataReg = _memHelper.Read(dataRegIdx, StoreLocation.DataRegister, operandSize);

            dataReg--;
            _memHelper.Write(dataReg, dataRegIdx, StoreLocation.DataRegister, operandSize);
            if ((short)dataReg == -1)
            {
                return instructionSize;
            }

            var displacement = (int)_memHelper.ReadImmediate(_regs.PC + INSTR_DEFAULT_SIZE, operandSize, signExtended: true);
            return instructionSize + displacement;
        }

        private bool TestBranchCondition(BranchCondition condition)
        {
            bool carry = _regs.C;
            bool extended = _regs.X;
            bool negative = _regs.N;
            bool overflow = _regs.V;
            bool zero = _regs.Z;
            return condition switch
            {
                BranchCondition.True => true,
                BranchCondition.False => false,
                BranchCondition.High => !carry && !zero,
                BranchCondition.LowOrSame => carry || zero,
                BranchCondition.CarryClear => !carry,
                BranchCondition.CarrySet => carry,
                BranchCondition.NotEqual => !zero,
                BranchCondition.Equal => zero,
                BranchCondition.OverflowClear => !overflow,
                BranchCondition.OverflowSet => overflow,
                BranchCondition.Plus => !negative,
                BranchCondition.Minus => negative,
                BranchCondition.GreaterOrEqual => negative == overflow,
                BranchCondition.LessThan => negative != overflow,
                BranchCondition.GreaterThan => negative = overflow && !zero,
                BranchCondition.LessOrEqual => zero || negative != overflow,
                _ => throw new InvalidOperationException("Condition test is unknown or not supported"),
            };
        }
    }

    internal enum BranchCondition
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
