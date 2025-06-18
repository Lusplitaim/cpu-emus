using CPEMUS.Motorola.M68000.EA;

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

        // Load Effective Address.
        private int Lea(ushort opcode)
        {
            var addrRegIdx = (uint)((opcode >> 9) & 0x7);
            var eaProps = _eaHelper.Get(opcode, OperandSize.Long);
            _memHelper.Write(eaProps.Operand, addrRegIdx, StoreLocation.AddressRegister, OperandSize.Long);
            return INSTR_DEFAULT_SIZE;
        }

        // Link and Allocate.
        private int Link(ushort opcode)
        {
            var addrRegIdx = (uint)((opcode >> 9) & 0x7);
            var addrReg = _memHelper.Read(addrRegIdx, StoreLocation.AddressRegister, OperandSize.Long);
            PushStack(addrReg, OperandSize.Long);

            _memHelper.Write(_regs.SP, addrRegIdx, StoreLocation.AddressRegister, OperandSize.Long);

            int displacement = (int)_memHelper.ReadImmediate(_regs.PC + INSTR_DEFAULT_SIZE, OperandSize.Word, signExtended: true);
            _regs.SP = (uint)(_regs.SP + displacement);

            return INSTR_DEFAULT_SIZE + 2;
        }
    }
}
