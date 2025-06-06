using CPEMUS.Motorola.M68000.Extensions;

namespace CPEMUS.Motorola.M68000.Helpers
{
    internal class MemHelper
    {
        private readonly byte[] _mem;
        private readonly M68KRegs _regs;
        public MemHelper(M68KRegs regs, byte[] mem)
        {
            _mem = mem;
            _regs = regs;
        }

        public void Write(int value, int address, StoreLocation location, OperandSize operandSize)
        {
            switch (location)
            {
                case StoreLocation.DataRegister:
                    Write(ref _regs.D[address], value, operandSize);
                    break;
                case StoreLocation.AddressRegister:
                    Write(ref _regs.A[address], value, operandSize);
                    break;
                case StoreLocation.Memory:
                    _mem.Write(address, value, operandSize);
                    break;
            }
        }

        public void Write(ref int dest, int value, OperandSize operandSize)
        {
            switch (operandSize)
            {
                case OperandSize.Byte:
                    dest = (dest & (~0xFF)) | (value & 0xFF);
                    break;
                case OperandSize.Word:
                    dest = (dest & (~0xFFFF)) | (value & 0xFFFF);
                    break;
                case OperandSize.Long:
                    dest = value;
                    break;
            }
        }
    }
}
