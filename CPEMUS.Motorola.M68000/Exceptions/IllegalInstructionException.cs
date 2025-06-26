using CPEMUS.Motorola.M68000.Helpers;

namespace CPEMUS.Motorola.M68000.Exceptions
{
    internal class IllegalInstructionException: M68KException
    {
        public IllegalInstructionException() : base(ExceptionVectorType.IllegalInstruction)
        {
        }
    }
}
