namespace CPEMUS.Motorola.M68000
{
    public partial class M68K
    {
        private (int instructionSize, int pcDisplacement) Branch(ushort opcode, Func<ushort, bool> branchRequired)
        {
            int instructionSize = INSTR_DEFAULT_SIZE;
            int displacement = (sbyte)opcode;

            if ((byte)displacement == 0xFF)
            {
                throw new InvalidOperationException("Displacement contained in a long word is not supported");
            }

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
            (int _, int pcDisplacement) = Branch(opcode, (opcode) =>
            {
                return NeedToBranch((ConditionTest)((opcode >> 8) & 0xF));
            });

            return pcDisplacement;
        }

        // Branch Always.
        private int Bra(ushort opcode)
        {
            (int _, int pcDisplacement) = Branch(opcode, (_) =>
            {
                return true;
            });

            return pcDisplacement;
        }

        // Branch to Subroutine.
        private int Bsr(ushort opcode)
        {
            (int instructionSize, int pcDisplacement) = Branch(opcode, (_) =>
            {
                return true;
            });

            // Pushing the address of the next instruction following bsr
            // to stack.
            PushStack((uint)(_regs.PC + instructionSize), OperandSize.Long);

            return pcDisplacement;
        }

        private bool NeedToBranch(ConditionTest condition)
        {
            bool carry = _regs.C;
            bool extended = _regs.X;
            bool negative = _regs.N;
            bool overflow = _regs.V;
            bool zero = _regs.Z;
            return condition switch
            {
                ConditionTest.High => !carry && !zero,
                ConditionTest.LowOrSame => carry || zero,
                ConditionTest.CarryClear => !carry,
                ConditionTest.CarrySet => carry,
                ConditionTest.NotEqual => !zero,
                ConditionTest.Equal => zero,
                ConditionTest.OverflowClear => !overflow,
                ConditionTest.OverflowSet => overflow,
                ConditionTest.Plus => !negative,
                ConditionTest.Minus => negative,
                ConditionTest.GreaterOrEqual => negative == overflow,
                ConditionTest.LessThan => negative != overflow,
                ConditionTest.GreaterThan => negative = overflow && !zero,
                ConditionTest.LessOrEqual => zero || negative != overflow,
                _ => throw new InvalidOperationException("Condition test is unknown or not supported"),
            };
        }
    }

    internal enum ConditionTest
    {
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
