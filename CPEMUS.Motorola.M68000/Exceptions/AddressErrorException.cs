using CPEMUS.Motorola.M68000.Helpers;

namespace CPEMUS.Motorola.M68000.Exceptions
{
    internal class AddressErrorException : M68KException
    {
        public AddressErrorException(): base(ExceptionVectorType.AddressError)
        {
        }
    }
}
