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
            _memHelper.Write(eaProps.Address, addrRegIdx, StoreLocation.AddressRegister, OperandSize.Long);
            return INSTR_DEFAULT_SIZE;
        }

        // Push Effective Address.
        private int Pea(ushort opcode)
        {
            var eaProps = _eaHelper.Get(opcode, OperandSize.Long);
            _memHelper.PushStack(eaProps.Address, OperandSize.Long);
            return INSTR_DEFAULT_SIZE;
        }

        // Link and Allocate.
        private int Link(ushort opcode)
        {
            var addrRegIdx = (uint)((opcode >> 9) & 0x7);
            var addrReg = _memHelper.Read(addrRegIdx, StoreLocation.AddressRegister, OperandSize.Long);
            _memHelper.PushStack(addrReg, OperandSize.Long);

            _memHelper.Write(_regs.SP, addrRegIdx, StoreLocation.AddressRegister, OperandSize.Long);

            int displacement = (int)_memHelper.ReadImmediate(_regs.PC + INSTR_DEFAULT_SIZE, OperandSize.Word, signExtended: true);
            _regs.SP = (uint)(_regs.SP + displacement);

            return INSTR_DEFAULT_SIZE + 2;
        }

        // Unlink.
        private int Unlk(ushort opcode)
        {
            var addrRegIdx = (uint)(opcode & 0x7);
            var addrReg = _memHelper.Read(addrRegIdx, StoreLocation.AddressRegister, OperandSize.Long);

            _regs.SP = addrReg;
            var newAddrRegValue = _memHelper.PopStack(OperandSize.Long);

            _memHelper.Write(newAddrRegValue, addrRegIdx, StoreLocation.AddressRegister, OperandSize.Long);

            return INSTR_DEFAULT_SIZE;
        }

        // Move Data from Source to Destination.
        private int Move(ushort opcode)
        {
            OperandSize operandSize;
            switch ((opcode >> 12) & 0x3)
            {
                case 1:
                    operandSize = OperandSize.Byte;
                    break;
                case 3:
                    operandSize = OperandSize.Word;
                    break;
                case 2:
                    operandSize = OperandSize.Long;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var srcEaProps = _eaHelper.Get(opcode, operandSize);
            var destEaProps = _eaHelper.Get((ushort)(opcode >> 6), operandSize);

            _memHelper.Write(srcEaProps.Operand, destEaProps.Address, destEaProps.Location, operandSize);

            _flagsHelper.AlterN(srcEaProps.Operand, operandSize);
            _flagsHelper.AlterZ(srcEaProps.Operand, operandSize);
            _regs.V = false;
            _regs.C = false;

            return INSTR_DEFAULT_SIZE;
        }

        // Move Address.
        private int Movea(ushort opcode)
        {
            OperandSize operandSize;
            switch ((opcode >> 12) & 0x3)
            {
                case 3:
                    operandSize = OperandSize.Word;
                    break;
                case 2:
                    operandSize = OperandSize.Long;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var srcEaProps = _eaHelper.Get(opcode, operandSize, signExtended: true);
            var addressRegIdx = (uint)((opcode >> 9) & 0x7);

            _memHelper.Write(srcEaProps.Operand, addressRegIdx, StoreLocation.AddressRegister, operandSize);

            return INSTR_DEFAULT_SIZE;
        }

        // Move to CCR.
        private int MoveToCcr(ushort opcode)
        {
            var eaProps = _eaHelper.Get(opcode, OperandSize.Byte);

            _regs.CCR = (byte)eaProps.Operand;

            return INSTR_DEFAULT_SIZE;
        }

        // Move from SR.
        private int MoveFromSr(ushort opcode)
        {
            var eaProps = _eaHelper.Get(opcode, OperandSize.Word);

            uint sr = _regs.SR;

            _memHelper.Write(sr, eaProps.Address, eaProps.Location, OperandSize.Word);

            return INSTR_DEFAULT_SIZE;
        }

        // Move Multiple Registers.
        private int Movem(ushort opcode)
        {
            throw new NotImplementedException();
        }

        // Move Peripheral Data.
        private int Movep(ushort opcode)
        {
            throw new NotImplementedException();
        }

        // Move Quick.
        private int Moveq(ushort opcode)
        {
            var dataRegIdx = (uint)((opcode >> 9) & 0x7);
            int innerData = (sbyte)opcode;

            _memHelper.Write((uint)innerData, dataRegIdx, StoreLocation.DataRegister, OperandSize.Long);

            _flagsHelper.AlterZ((uint)innerData, OperandSize.Long);
            _flagsHelper.AlterN((uint)innerData, OperandSize.Long);
            _regs.V = false;
            _regs.C = false;

            return INSTR_DEFAULT_SIZE;
        }
    }
}
