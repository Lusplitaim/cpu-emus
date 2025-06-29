namespace CPEMUS.Motorola.M68000
{
    public class M68KRegs
    {
        private const int CMask = 0x01;
        private const int VMask = 0x02;
        private const int ZMask = 0x04;
        private const int NMask = 0x08;
        private const int XMask = 0x10;

        // Data registers.
        public uint[] D = new uint[8];

        // Address registers.
        public uint[] A = new uint[7];

        // User Stack Pointer.
        public uint USP
        {
            get;
            set;
        }

        // Supervisor Stack Pointer.
        public uint SSP
        {
            get;
            set;
        }

        // Stack Pointer.
        public uint SP
        {
            get => Mode == MPrivilegeMode.User ? USP : SSP;
            set
            {
                if (Mode == MPrivilegeMode.User)
                {
                    USP = value;
                }
                else
                {
                    SSP = value;
                }
            }
        }

        public MPrivilegeMode Mode => (MPrivilegeMode)((SR >> 13) & 0x1);
        public bool IsTracingEnabled => ((SR >> 14) & 0x3) == 2;

        // Program Counter.
        public uint PC { get; set; }

        private ushort _sr;
        // Status Register.
        public ushort SR
        {
            get => _sr;
            set
            {
                // MC68000 doesn't support T0 tracing
                // and Master Mode thus it has bits 14 and 12 as zero.
                // Ref: Motorola Programmer's Reference Manual, paragraph 1.3.2.
                _sr = (ushort)(value & 0xA71F);
            }
        }

        // Condition Code Register.
        public byte CCR
        {
            get => (byte)SR;
            set => SR = (ushort)((SR & 0xFF00) | (value & 0x1F));
        }

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

        private bool IsFlagSet(int flagMask) => (SR & flagMask) == flagMask;
        private void UpdateFlag(bool set, int flagMask)
        {
            if (set)
            {
                SR |= (ushort)flagMask;
            }
            else
            {
                SR &= (ushort)~flagMask;
            }
        }
        #endregion
    }
}
