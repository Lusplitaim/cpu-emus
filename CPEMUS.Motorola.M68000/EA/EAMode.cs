namespace CPEMUS.Motorola.M68000.EA
{
    internal enum EAMode
    {
        DataDirect = 0x0,
        AddressDirect = 0x1,
        AddressIndirect = 0x2,
        PostincIndirect = 0x3,
        PredecIndirect = 0x4,
        BaseDisplacement16 = 0x5,
        IndexedAddressing = 0x6,
        AbsoluteShort = 0x70,
        AbsoluteLong= 0x71,
        PCDisplacement16 = 0x72,
        PCDisplacement8 = 0x73,
        ImmediateData = 0x74,
    }
}
