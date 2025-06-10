namespace CPEMUS.Motorola.M68000.Helpers
{
    internal class FlagsHelper
    {
        private Dictionary<OperandSize, uint> _operandSizeMask = new()
        {
            { OperandSize.Byte, 0xFF },
            { OperandSize.Word, 0xFFFF },
            { OperandSize.Long, 0xFFFFFFFF },
        };
        private readonly M68KRegs _regs;

        public FlagsHelper(M68KRegs regs)
        {
            _regs = regs;
        }

        public void AlterN(uint val, OperandSize operandSize)
        {
            var mask = _operandSizeMask[operandSize];
            _regs.N = ((val & mask) >> ((int)operandSize * 8 - 1)) == 1;
        }

        public void AlterZ(uint val, OperandSize operandSize)
        {
            var mask = _operandSizeMask[operandSize];
            _regs.Z = (val & mask) == 0;
        }

        public void AlterC(long val, OperandSize operandSize)
        {
            var mask = _operandSizeMask[operandSize];
            _regs.C = (val & ~mask) > 0;
        }

        public void AlterV(uint op1, uint op2, long result, OperandSize operandSize)
        {
            var mask = _operandSizeMask[operandSize];
            var op1Msb = (op1 & mask) >> ((int)operandSize * 8 - 1);
            var op2Msb = (op2 & mask) >> ((int)operandSize * 8 - 1);
            var resultMsb = (result & mask) >> ((int)operandSize * 8 - 1);

            var operandsAreSameSign = op1Msb == op2Msb;
            var resultSignDiffers = op1Msb != resultMsb;
            _regs.V = operandsAreSameSign && resultSignDiffers;
        }
    }
}
