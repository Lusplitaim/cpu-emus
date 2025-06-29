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

        public void AlterZ(uint val, OperandSize operandSize, bool doNotChangeIfZero = false)
        {
            var mask = _operandSizeMask[operandSize];
            var newZ = (val & mask) == 0;
            _regs.Z = newZ && doNotChangeIfZero ? _regs.Z : newZ;
        }

        public void AlterC(long val, OperandSize operandSize)
        {
            _regs.C = (val >> (int)operandSize*8) != 0;
        }

        public void AlterCSub(uint dest, uint src, OperandSize operandSize)
        {
            var mask = _operandSizeMask[operandSize];
            _regs.C = (dest & mask) < (src & mask);
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

        public void AlterVSub(uint dest, uint src, long result, OperandSize operandSize)
        {
            var mask = _operandSizeMask[operandSize];
            var destPositive = ((dest & mask) >> ((int)operandSize * 8 - 1)) == 1;
            var srcPositive = ((src & mask) >> ((int)operandSize * 8 - 1)) == 1;
            var resultPositive = ((result & mask) >> ((int)operandSize * 8 - 1)) == 1;

            _regs.V = srcPositive && !destPositive && resultPositive
                || !srcPositive && destPositive && !resultPositive;
        }

        public void AlterVCmp(uint dest, uint src, long result, OperandSize operandSize)
        {
            var mask = _operandSizeMask[operandSize];
            var srcPositive = ((src & mask) >> ((int)operandSize * 8 - 1)) == 0;
            var destPositive = ((dest & mask) >> ((int)operandSize * 8 - 1)) == 0;
            var resultPositive = ((result & mask) >> ((int)operandSize * 8 - 1)) == 0;

            _regs.V = (srcPositive && !destPositive && resultPositive) || (!srcPositive && destPositive && !resultPositive);
        }
    }
}
