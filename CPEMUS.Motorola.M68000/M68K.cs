using CPEMUS.Motorola.M68000.EA;
using CPEMUS.Motorola.M68000.Extensions;

namespace CPEMUS.Motorola.M68000
{
    public class M68K
    {
        #region Opcode suffixes.
        private readonly int MULU_SFX = 0x00C0;
        private readonly int MULS_SFX = 0x01C0;
        private readonly int ABCD_SFX = 0x0100;
        private readonly int EXG_SFX = 0x0100;
        #endregion

        #region Opcode masks.
        private readonly int MULU_MASK = 0x01C0;
        private readonly int MULS_MASK = 0x01C0;
        private readonly int ABCD_MASK = 0x01F0;
        private readonly int EXG_MASK = 0x0130;
        #endregion

        private readonly int INSTRUCTION_DEFAULT_SIZE = 2;

        private readonly M68KRegs _regs;
        private readonly byte[] _mem;
        private readonly EAHelper _eaHelper;
        public M68K(byte[] mem)
        {
            _mem = mem;
            _regs = new();
            _eaHelper = new(_regs, mem);
        }

        public void DecodeOpcode(ushort opcode)
        {
            if ((opcode & 0xF000) == 0xC000)
            {
                Decode0xC(opcode);
            }
        }

        private void Decode0xC(ushort opcode)
        {
            if ((opcode & MULU_MASK) == MULU_SFX)
            {
                Mulu();
                return;
            }
            else if ((opcode & MULS_MASK) == MULS_SFX)
            {
                Muls();
                return;
            }
            else if ((opcode & ABCD_MASK) == ABCD_SFX)
            {
                //Abcd();
            }
            else if ((opcode & EXG_MASK) == EXG_SFX)
            {
                Exg();
            }
            else
            {
                And(opcode);
            }
        }

        private void Mulu()
        {

        }

        private void Muls()
        {

        }

        private void Exg()
        {

        }

        private int And(ushort opcode)
        {
            int srcIdx = (opcode >> 9) & 0x7;
            int src = _regs.D[srcIdx];

            var eaResult = _eaHelper.Get(opcode);
            int dest = eaResult.Operand;

            int result = src & dest;

            int storeDirection = (opcode >> 8) & 0x1;
            if ((StoreDirection)storeDirection == StoreDirection.Source)
            {
                _regs.D[srcIdx] = result;
            }
            else
            {
                _mem.WriteLong(eaResult.Address!.Value, result);
            }

            return eaResult.InstructionSize;
        }

        private void Abcd(ref byte src, ref byte dest)
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
