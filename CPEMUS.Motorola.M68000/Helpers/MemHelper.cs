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

        public void Write(uint value, uint address, StoreLocation location, OperandSize operandSize)
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

        public void Write(ref uint dest, uint value, OperandSize operandSize)
        {
            switch (operandSize)
            {
                case OperandSize.Byte:
                    dest = (uint)((dest & (~0xFF)) | (value & 0xFF));
                    break;
                case OperandSize.Word:
                    dest = (uint)((dest & (~0xFFFF)) | (value & 0xFFFF));
                    break;
                case OperandSize.Long:
                    dest = value;
                    break;
            }
        }

        public uint Read(uint value, OperandSize operandSize)
        {
            return operandSize switch
            {
                OperandSize.Byte => value & 0xFF,
                OperandSize.Word => value & 0xFFFF,
                OperandSize.Long => value,
                _ => throw new InvalidOperationException("Operand size type is unknown"),
            };
        }

        public uint Read(uint address, StoreLocation location, OperandSize operandSize)
        {
            return location switch
            {
                StoreLocation.DataRegister => Read(_regs.D[address], operandSize),
                StoreLocation.AddressRegister => Read(_regs.A[address], operandSize),
                StoreLocation.Memory => _mem.Read(address, operandSize),
                _ => throw new InvalidOperationException("Operand location type is unknown"),
            };
        }

        public int ReadSignExt(uint address, StoreLocation location, OperandSize operandSize)
        {
            var operand = Read(address, location, operandSize);
            return operandSize switch
            {
                OperandSize.Byte => (sbyte)operand,
                OperandSize.Word => (short)operand,
                OperandSize.Long => (int)operand,
                _ => throw new InvalidOperationException("Operand size type is unknown"),
            };
        }

        public uint ReadImmediate(uint address, OperandSize operandSize)
        {
            return operandSize switch
            {
                OperandSize.Byte => _mem.ReadWord(address) & 0xFF,
                OperandSize.Word => _mem.ReadWord(address) & 0xFFFF,
                OperandSize.Long => _mem.ReadLong(address),
                _ => throw new InvalidOperationException("Operand size type is unknown"),
            };
        }
    }
}
