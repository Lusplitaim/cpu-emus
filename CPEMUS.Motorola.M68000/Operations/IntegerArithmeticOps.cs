using CPEMUS.Motorola.M68000.EA;

namespace CPEMUS.Motorola.M68000
{
    public partial class M68K
    {
        // Add.
        private int Add(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);

            var dataRegIdx = (uint)((opcode >> 9) & 0x7);
            var dataReg = _memHelper.Read(dataRegIdx, StoreLocation.DataRegister, operandSize);

            var eaProps = _eaHelper.Get(opcode, operandSize);

            long result = eaProps.Operand + dataReg;

            // Flags.
            _flagsHelper.AlterN((uint)result, operandSize);
            _flagsHelper.AlterZ((uint)result, operandSize);
            _flagsHelper.AlterV(dataReg, eaProps.Operand, result, operandSize);
            _flagsHelper.AlterC(result, operandSize);
            _regs.X = _regs.C;

            // Storing.
            int storeDirection = (opcode >> 8) & 0x1;
            if ((StoreDirection)storeDirection == StoreDirection.Source)
            {
                _memHelper.Write((uint)result, dataRegIdx, StoreLocation.DataRegister, operandSize);
            }
            else
            {
                _memHelper.Write((uint)result, eaProps.Address, eaProps.Location, operandSize);
            }

            return eaProps.InstructionSize;
        }

        // Add Address.
        private int Adda(ushort opcode)
        {
            OperandSize operandSize;
            switch ((opcode >> 6) & 0x7)
            {
                case 0x3:
                    operandSize = OperandSize.Word;
                    break;
                case 0x7:
                    operandSize = OperandSize.Long;
                    break;
                default:
                    throw new InvalidOperationException("The given operation size is unknown.");

            }

            var addrRegIdx = (uint)((opcode >> 9) & 0x7);
            // The entire destination address register is used regardless of the operation size.
            var addrReg = _memHelper.Read(addrRegIdx, StoreLocation.AddressRegister, OperandSize.Long);

            var eaProps = _eaHelper.Get(opcode, operandSize, signExtended: true);

            long result = (int)eaProps.Operand + addrReg;

            // Storing.
            _memHelper.Write((uint)result, addrRegIdx, StoreLocation.AddressRegister, OperandSize.Long);

            return eaProps.InstructionSize;
        }

        // Add Immediate.
        private int Addi(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);

            var immediateOperand = (int)_memHelper.ReadImmediate(_regs.PC + INSTR_DEFAULT_SIZE, operandSize);

            var opcodeSize = INSTR_DEFAULT_SIZE + (operandSize == OperandSize.Byte ? 2 : (int)operandSize);
            var eaProps = _eaHelper.Get(opcode, operandSize, opcodeSize);

            long result = eaProps.Operand + immediateOperand;

            // Flags.
            _flagsHelper.AlterN((uint)result, operandSize);
            _flagsHelper.AlterZ((uint)result, operandSize);
            _flagsHelper.AlterV((uint)immediateOperand, eaProps.Operand, result, operandSize);
            _flagsHelper.AlterC(result, operandSize);
            _regs.X = _regs.C;

            // Storing.
            _memHelper.Write((uint)result, eaProps.Address, eaProps.Location, operandSize);

            return eaProps.InstructionSize;
        }

        // Add Quick.
        private int Addq(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);

            var dataField = (opcode >> 9) & 0x7;
            var immediateOperand = dataField == 0 ? 8 : dataField;

            var eaProps = _eaHelper.Get(opcode, operandSize);
            bool isEaAddressRegister = eaProps.Location == StoreLocation.AddressRegister;
            if (isEaAddressRegister)
            {
                // The entire destination address register is used regardless of the operation size.
                operandSize = OperandSize.Long;
                eaProps = _eaHelper.Get(opcode, operandSize);
            }

            long result = eaProps.Operand + immediateOperand;

            // When adding to address registers, the condition codes are not altered.
            if (!isEaAddressRegister)
            {
                _flagsHelper.AlterN((uint)result, operandSize);
                _flagsHelper.AlterZ((uint)result, operandSize);
                _flagsHelper.AlterV((uint)immediateOperand, eaProps.Operand, result, operandSize);
                _flagsHelper.AlterC(result, operandSize);
                _regs.X = _regs.C;
            }

            // Storing.
            _memHelper.Write((uint)result, eaProps.Address, eaProps.Location, operandSize);

            return eaProps.InstructionSize;
        }

        // Add Extended.
        private int Addx(ushort opcode)
        {
            var eaMode = ((opcode >> 3) & 0x1) == 0 ? EAMode.DataDirect : EAMode.PredecIndirect;
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);

            var destRegIdx = (uint)((opcode >> 9) & 0x7);
            var destRegProps = _eaHelper.Get(eaMode, (int)destRegIdx, operandSize);

            var srcRegIdx = (uint)(opcode & 0x7);
            var srcRegProps = _eaHelper.Get(eaMode, (int)srcRegIdx, operandSize);

            long result = destRegProps.Operand + srcRegProps.Operand + (_regs.X ? 1 : 0);

            // Flags.
            _flagsHelper.AlterN((uint)result, operandSize);
            _flagsHelper.AlterZ((uint)result, operandSize); // TODO: Cleared if the result is nonzero; unchanged otherwise.
            _flagsHelper.AlterV(destRegProps.Operand, srcRegProps.Operand, result, operandSize);
            _flagsHelper.AlterC(result, operandSize);
            _regs.X = _regs.C;

            // Storing.
            _memHelper.Write((uint)result, destRegProps.Address, destRegProps.Location, operandSize);

            return destRegProps.InstructionSize;
        }

        private int Mulu()
        {
            throw new NotImplementedException();
        }

        private int Muls()
        {
            throw new NotImplementedException();
        }
    }
}
