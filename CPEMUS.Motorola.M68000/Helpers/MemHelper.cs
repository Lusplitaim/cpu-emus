using CPEMUS.Motorola.M68000.Exceptions;
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
                    _mem.Write(TruncateAddress(address, operandSize), value, operandSize);
                    break;
                case StoreLocation.StatusRegister:
                    if (operandSize == OperandSize.Long)
                    {
                        throw new IllegalInstructionException();
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
                _ => throw new IllegalInstructionException(),
            };
        }

        public uint Read(uint value, OperandSize operandSize)
        {
            return operandSize switch
            {
                OperandSize.Byte => value & 0xFF,
                OperandSize.Word => value & 0xFFFF,
                OperandSize.Long => value,
                _ => throw new IllegalInstructionException(),
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
                StoreLocation.Memory => _mem.Read(TruncateAddress(address, operandSize), operandSize),
                StoreLocation.ImmediateData => ReadImmediate(TruncateAddress(address, operandSize), operandSize),
                _ => throw new IllegalInstructionException(),
            };

            if (signExtended)
            {
                return (uint)ExtendOperandSign(operand, operandSize);
            }

            return operand;
        }

        private uint TruncateAddress(uint address, OperandSize operandSize)
        {
            address = address & 0xFFFFFF;
            if (operandSize != OperandSize.Byte && (address % 2) != 0)
            {
                throw new AddressErrorException();
            }
            return address;
        }

        private uint ReadImmediate(uint address, OperandSize operandSize)
        {
            var operand = operandSize switch
            {
                OperandSize.Byte => _mem.ReadWord(address) & 0xFF,
                OperandSize.Word => _mem.ReadWord(address) & 0xFFFF,
                OperandSize.Long => _mem.ReadLong(address),
                _ => throw new IllegalInstructionException(),
            };

            return operand;
        }

        public void PushStack(uint value, OperandSize operandSize)
        {
            _regs.SP -= Math.Max((uint)operandSize, (uint)OperandSize.Word);
            Write(value, _regs.SP, StoreLocation.Memory, operandSize);
        }

        public uint PopStack(OperandSize operandSize)
        {
            var result = Read(_regs.SP, StoreLocation.Memory, operandSize);
            _regs.SP += Math.Max((uint)operandSize, (uint)OperandSize.Word);
            return result;
        }

        private int ExtendOperandSign(uint operand, OperandSize operandSize)
        {
            return operandSize switch
            {
                OperandSize.Byte => (sbyte)operand,
                OperandSize.Word => (short)operand,
                OperandSize.Long => (int)operand,
                _ => throw new IllegalInstructionException(),
            };
        }
    }
}
