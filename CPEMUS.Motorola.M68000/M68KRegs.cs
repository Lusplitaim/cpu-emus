namespace CPEMUS.Motorola.M68000
{
    public class M68KRegs
    {
        private readonly int CMask = 0x01;
        private readonly int VMask = 0x02;
        private readonly int ZMask = 0x04;
        private readonly int NMask = 0x08;
        private readonly int XMask = 0x10;

        // Data registers.
        public uint[] D = new uint[8];

        // Address registers.
        public uint[] A = new uint[8];

        // User Stack Pouinter.
        public uint USP
        {
            get => A[7];
            set => A[7] = value;
        }

        // Program Counter.
        public uint PC { get; set; }

        // Condition Code Register.
        // Upper byte is read as all zeroes
        // and is ignored when written.
        public ushort CCR;

        #region Flags.
        public bool X
        {
            get => IsFlagSet(XMask);
            set => UpdateFlag(value, XMask);
        }
        public bool C
        {
            get => IsFlagSet(CMask);
            set => UpdateFlag(value, CMask);
        }
        public bool Z
        {
            get => IsFlagSet(ZMask);
            set => UpdateFlag(value, ZMask);
        }
        public bool V
        {
            get => IsFlagSet(VMask);
            set => UpdateFlag(value, VMask);
        }
        public bool N
        {
            get => IsFlagSet(NMask);
            set => UpdateFlag(value, NMask);
        }

        private bool IsFlagSet(int flagMask) => (CCR & flagMask) == flagMask;
        private void UpdateFlag(bool set, int flagMask)
        {
            if (set)
            {
                CCR |= (ushort)flagMask;
            }
            else
            {
                CCR &= (ushort)~flagMask;
            }
        }
        #endregion
    }
}
