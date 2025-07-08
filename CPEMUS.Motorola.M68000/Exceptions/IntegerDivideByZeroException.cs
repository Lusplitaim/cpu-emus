using CPEMUS.Motorola.M68000.Helpers;

namespace CPEMUS.Motorola.M68000.Exceptions
{
    internal class IntegerDivideByZeroException : M68KException
    {
        public uint ExceptionAddress { get; set; }
        public int InstructionClockPeriods { get; set; }
        public IntegerDivideByZeroException(int instrClockPeriods) : base(ExceptionVectorType.IntegerDivideByZero)
        {
            InstructionClockPeriods = instrClockPeriods;
        }
    }
}
