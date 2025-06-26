using CPEMUS.Motorola.M68000.Helpers;

namespace CPEMUS.Motorola.M68000.Exceptions
{
    internal class M68KException : Exception
    {
        public ExceptionVectorType ExceptionVectorType { get; private set; }
        public M68KException(ExceptionVectorType exceptionType) : base()
        {
            ExceptionVectorType = exceptionType;
        }
    }
}
