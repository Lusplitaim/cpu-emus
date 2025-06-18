using System.Reflection.Emit;

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
            var ccr = _regs.SR;

            byte result = (byte)(src ^ ccr);

            _memHelper.Write(result, default, StoreLocation.StatusRegister, OperandSize.Byte);

            return INSTR_DEFAULT_SIZE + 2;
        }
    }
}
