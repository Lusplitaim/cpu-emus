namespace CPEMUS.Motorola.M68000.EA
{
    internal struct EAProps
    {
        public uint Operand;
        public uint Address;
        public StoreLocation Location;
        public int InstructionSize;
        public int ClockPeriods;
        public EAMode Mode;
    }
}
