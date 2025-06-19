namespace CPEMUS.Motorola.M68000
{
    public partial class M68K
    {
        // Arithmetic Shift Right/Left.
        private int AslAsr(ushort opcode)
        {
            throw new NotImplementedException();
        }

        // Arithmetic Shift Left/Right, Register Shift.
        private int LslLsrRegShift(ushort opcode)
        {
            var count = (opcode >> 9) & 0x7;
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);

            bool isCountInReg = ((opcode >> 5) & 0x1) == 1;
            if (isCountInReg)
            {
                count = (int)_memHelper.Read((uint)count, StoreLocation.DataRegister, operandSize) % 64;
            }
            else if (count == 0)
            {
                count = 8;
            }

            var dataRegIdx = (uint)(opcode & 0x7);
            var valueForShift = _memHelper.Read(dataRegIdx, StoreLocation.DataRegister, operandSize);

            bool isShiftLeft = ((opcode >> 8) & 0x1) == 1;
            var result = GetLogicalShiftResult(valueForShift, count, isShiftLeft, operandSize);

            _memHelper.Write(result, dataRegIdx, StoreLocation.DataRegister, operandSize);

            return INSTR_DEFAULT_SIZE;
        }

        // Arithmetic Shift Left/Right, Memory Shift.
        private int LslLsrMemShift(ushort opcode)
        {
            bool isShiftLeft = ((opcode >> 8) & 0x1) == 1;
            var operandSize = OperandSize.Word;
            var eaProps = _eaHelper.Get(opcode, operandSize);

            var result = GetLogicalShiftResult(eaProps.Operand, 1, isShiftLeft, operandSize);

            _memHelper.Write(result, eaProps.Address, eaProps.Location, operandSize);

            return INSTR_DEFAULT_SIZE;
        }

        private uint GetLogicalShiftResult(uint valueForShift, int count, bool isShiftLeft, OperandSize operandSize)
        {
            uint result;
            bool newX, newC;
            if (isShiftLeft)
            {
                result = valueForShift << count;
                newC = (((valueForShift << (count - 1)) >> ((int)operandSize * 8 - 1)) & 0x1) == 1;
                newX = count == 0 ? _regs.X : newC;
            }
            else
            {
                result = valueForShift >> count;
                newC = ((valueForShift >> (count - 1)) & 0x1) == 1;
                newX = count == 0 ? _regs.X : newC;
            }

            _regs.X = newX;
            _regs.C = newC;
            _flagsHelper.AlterZ(result, operandSize);
            _flagsHelper.AlterN(result, operandSize);
            _regs.V = false;

            return result;
        }
    }
}
