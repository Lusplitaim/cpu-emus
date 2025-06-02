namespace CPEMUS.Motorola.M68000.EA
{
    internal enum EAMode
    {
        DataDirect = 0x0,
        AddressDirect = 0x1,
        AddressIndirect = 0x2,
        PostincIndirect = 0x3,
        PredecIndirect = 0x4,
        BaseDisplacement = 0x5,
        IndexedAddressing = 0x6,
        PCAbsoluteImmediate = 0x7,
    }
}
