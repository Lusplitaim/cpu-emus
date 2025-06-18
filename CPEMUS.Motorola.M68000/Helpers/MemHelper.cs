using CPEMUS.Motorola.M68000.Extensions;

namespace CPEMUS.Motorola.M68000.Helpers
{
    internal class MemHelper
    {
        private const int SP_ADDRESS = 7;

        private readonly IList<byte> _mem;
        private readonly M68KRegs _regs;
        public MemHelper(M68KRegs regs, IList<byte> mem)
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
                    if (address == SP_ADDRESS)
                    {
                        _regs.SP = MergeDestWithValue(_regs.SP, value, operandSize);
                        return;
                    }
                    Write(ref _regs.A[address], value, operandSize);
                    break;
                case StoreLocation.Memory:
                    _mem.Write(address, value, operandSize);
                    break;
                case StoreLocation.StatusRegister:
                    if (operandSize == OperandSize.Long)
                    {
                        throw new InvalidOperationException("Long operand size is not supported for status register");
                    }
                    _regs.SR = (ushort)MergeDestWithValue(_regs.SR, value, operandSize);
                    break;
            }
        }

        public void Write(ref uint dest, uint value, OperandSize operandSize)
        {
            dest = MergeDestWithValue(dest, value, operandSize);
        }

        private uint MergeDestWithValue(uint dest, uint value, OperandSize operandSize)
        {
            return operandSize switch
            {
                OperandSize.Byte => (uint)((dest & (~0xFF)) | (value & 0xFF)),
                OperandSize.Word => (uint)((dest & (~0xFFFF)) | (value & 0xFFFF)),
                OperandSize.Long => value,
                _ => throw new InvalidOperationException("Operand size type is unknown"),
            };
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

        public uint Read(uint address, StoreLocation location, OperandSize operandSize, bool signExtended = false)
        {
            var operand = location switch
            {
                StoreLocation.DataRegister => Read(_regs.D[address], operandSize),
                StoreLocation.AddressRegister => address == SP_ADDRESS
                    ? Read(_regs.SP, operandSize)
                    : Read(_regs.A[address], operandSize),
                StoreLocation.Memory => _mem.Read(address & 0xFFFFFF, operandSize),
                _ => throw new InvalidOperationException("Operand location type is unknown"),
            };

            if (signExtended)
            {
                return (uint)ExtendOperandSign(operand, operandSize);
            }

            return operand;
        }

        public uint ReadImmediate(uint address, OperandSize operandSize, bool signExtended = false)
        {
            var operand = operandSize switch
            {
                OperandSize.Byte => _mem.ReadWord(address) & 0xFF,
                OperandSize.Word => _mem.ReadWord(address) & 0xFFFF,
                OperandSize.Long => _mem.ReadLong(address),
                _ => throw new InvalidOperationException("Operand size type is unknown"),
            };

            if (signExtended)
            {
                return (uint)ExtendOperandSign(operand, operandSize);
            }

            return operand;
        }

        private int ExtendOperandSign(uint operand, OperandSize operandSize)
        {
            return operandSize switch
            {
                OperandSize.Byte => (sbyte)operand,
                OperandSize.Word => (short)operand,
                OperandSize.Long => (int)operand,
                _ => throw new InvalidOperationException("Operand size type is unknown"),
            };
        }
    }
}
