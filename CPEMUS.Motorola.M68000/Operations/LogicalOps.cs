namespace CPEMUS.Motorola.M68000
{
    public partial class M68K
    {
        // Exclusive-OR Logical.
        private int Eor(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);

            var dataRegIdx = (uint)((opcode >> 9) & 0x7);
            var dataReg = _memHelper.Read(dataRegIdx, StoreLocation.DataRegister, operandSize);

            var eaProps = _eaHelper.Get(opcode, operandSize);

            var result = dataReg ^ eaProps.Operand;

            _flagsHelper.AlterN(result, operandSize);
            _flagsHelper.AlterZ(result, operandSize);
            _regs.V = false;
            _regs.C = false;

            _memHelper.Write(result, eaProps.Address, eaProps.Location, operandSize);

            return eaProps.InstructionSize;
        }

        // Exclusive-OR Immediate.
        private int Eori(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);

            var immediateOperand = _memHelper.ReadImmediate(_regs.PC + INSTR_DEFAULT_SIZE, operandSize);

            var opcodeSize = INSTR_DEFAULT_SIZE + (operandSize == OperandSize.Byte ? 2 : (int)operandSize);
            var eaProps = _eaHelper.Get(opcode, operandSize, opcodeSize);

            var result = eaProps.Operand ^ immediateOperand;

            _flagsHelper.AlterN(result, operandSize);
            _flagsHelper.AlterZ(result, operandSize);
            _regs.V = false;
            _regs.C = false;

            _memHelper.Write(result, eaProps.Address, eaProps.Location, operandSize);

            return eaProps.InstructionSize;
        }

        // Exclusive-OR Immediate to CCR.
        private int EoriToCcr(ushort opcode)
        {
            uint src = _memHelper.ReadImmediate(_regs.PC + INSTR_DEFAULT_SIZE, OperandSize.Byte);
            var ccr = _regs.CCR;

            byte result = (byte)(src ^ ccr);

            _regs.CCR = result;

            return INSTR_DEFAULT_SIZE + 2;
        }

        // Logical Complement.
        private int Not(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);
            var eaProps = _eaHelper.Get(opcode, operandSize);

            var result = ~eaProps.Operand;
            _memHelper.Write(~eaProps.Operand, eaProps.Address, eaProps.Location, operandSize);

            _flagsHelper.AlterN(result, operandSize);
            _flagsHelper.AlterZ(result, operandSize);
            _regs.V = _regs.C = false;

            return eaProps.InstructionSize;
        }

        // Inclusive-OR Logical.
        private int Or(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);
            var eaProps = _eaHelper.Get(opcode, operandSize);
            var dataRegIdx = (uint)((opcode >> 9) & 0x7);
            var dataReg = _memHelper.Read(dataRegIdx, StoreLocation.DataRegister, operandSize);

            var result = eaProps.Operand | dataReg;

            var writeToDataReg = ((opcode >> 8) & 0x1) == 0;
            if (writeToDataReg)
            {
                _memHelper.Write(result, dataRegIdx, StoreLocation.DataRegister, operandSize);
            }
            else
            {
                _memHelper.Write(result, eaProps.Address, eaProps.Location, operandSize);
            }

            _flagsHelper.AlterN(result, operandSize);
            _flagsHelper.AlterZ(result, operandSize);
            _regs.V = _regs.C = false;

            return eaProps.InstructionSize;
        }

        // Inclusive-OR.
        private int Ori(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);
            var opcodeSize = Math.Max((int)operandSize, (int)OperandSize.Word);
            var eaProps = _eaHelper.Get(opcode, operandSize, opcodeSize);
            var immediateData = _memHelper.ReadImmediate(_regs.PC + INSTR_DEFAULT_SIZE, operandSize);

            var result = eaProps.Operand | immediateData;

            _memHelper.Write(result, eaProps.Address, eaProps.Location, operandSize);

            _flagsHelper.AlterN(result, operandSize);
            _flagsHelper.AlterZ(result, operandSize);
            _regs.V = _regs.C = false;

            return eaProps.InstructionSize;
        }

        // Inclusive-OR Immediate to CCR.
        private int OriToCcr(ushort opcode)
        {
            var immediateData = _memHelper.ReadImmediate(_regs.PC + INSTR_DEFAULT_SIZE, OperandSize.Byte);

            var result = _regs.CCR | immediateData;

            _regs.CCR = (byte)result;

            return INSTR_DEFAULT_SIZE + 2;
        }
    }
}
