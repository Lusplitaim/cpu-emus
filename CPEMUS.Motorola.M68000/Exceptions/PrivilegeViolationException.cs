using CPEMUS.Motorola.M68000.Helpers;

namespace CPEMUS.Motorola.M68000.Exceptions
{
    internal class PrivilegeViolationException : M68KException
    {
        public PrivilegeViolationException(): base(ExceptionVectorType.PrivilegeViolation)
        {
        }
    }
}
