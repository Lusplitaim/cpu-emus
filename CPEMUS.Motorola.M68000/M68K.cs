namespace CPEMUS.Motorola.M68000
{
    public class M68K
    {
        M68KRegs _regs = new();

        public void FetchNext()
        {

        }

        public void Abcd(ref byte src, ref byte dest)
        {
            bool carry = false;
            int res = src + dest + (_regs.X ? 1 : 0);
            // If the lower nibble > 9 perform correction (add 0x6).
            if ((res & 0x0F) > 0x09)
            {
                res += 0x6;
                carry = true;
            }
            // If the higher nibble > 9 perform correction (add 0x60).
            if ((res & 0xF0) > 0x90)
            {
                res += 0x60;
            }
            if (res > 0xFF)
            {
                carry = true;
            }

            _regs.X = carry;
            _regs.C = carry;
            // Clear if the result is non-zero.
            if ((byte)res != 0)
            {
                _regs.Z = false;
            }

            dest = (byte)res;
        }
    }
}
