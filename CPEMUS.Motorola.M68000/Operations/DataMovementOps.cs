namespace CPEMUS.Motorola.M68000
{
    public partial class M68K
    {
        // Exchange Registers.
        private int Exg(ushort opcode)
        {
            var srcRegIdx = (uint)((opcode >> 9) & 0x7);
            var destRegIdx = (uint)(opcode & 0x7);
            StoreLocation destRegLocation;
            StoreLocation srcRegLocation;

            var mode = (opcode >> 3) & 0x1F;
            switch (mode)
            {
                case 8:
                    destRegLocation = StoreLocation.DataRegister;
                    srcRegLocation = StoreLocation.DataRegister;
                    break;
                case 9:
                    destRegLocation = StoreLocation.AddressRegister;
                    srcRegLocation = StoreLocation.AddressRegister;
                    break;
                case 17:
                    destRegLocation = StoreLocation.AddressRegister;
                    srcRegLocation = StoreLocation.DataRegister;
                    break;
                default:
                    throw new InvalidOperationException("Incorrect Exg opcode mode");
            }

            var operandSize = OperandSize.Long;
            var destReg = _memHelper.Read(destRegIdx, destRegLocation, operandSize);
            var srcReg = _memHelper.Read(srcRegIdx, srcRegLocation, operandSize);

            _memHelper.Write(srcReg, destRegIdx, destRegLocation, operandSize);
            _memHelper.Write(destReg, srcRegIdx, srcRegLocation, operandSize);

            return INSTR_DEFAULT_SIZE;
        }
    }
}
