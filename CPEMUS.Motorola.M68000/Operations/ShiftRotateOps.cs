using System.Collections.Specialized;

namespace CPEMUS.Motorola.M68000
{
    public partial class M68K
    {
        private int LogicalArithmeticRegShift(ushort opcode, Func<uint, int, bool, OperandSize, uint> getShiftResult)
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
            var result = getShiftResult(valueForShift, count, isShiftLeft, operandSize);

            _memHelper.Write(result, dataRegIdx, StoreLocation.DataRegister, operandSize);

            return INSTR_DEFAULT_SIZE;
        }

        private int LogicalArithmeticMemShift(ushort opcode, Func<uint, int, bool, OperandSize, uint> getShiftResult)
        {
            bool isShiftLeft = ((opcode >> 8) & 0x1) == 1;
            var operandSize = OperandSize.Word;
            var eaProps = _eaHelper.Get(opcode, operandSize);

            var result = getShiftResult(eaProps.Operand, 1, isShiftLeft, operandSize);

            _memHelper.Write(result, eaProps.Address, eaProps.Location, operandSize);

            return eaProps.InstructionSize;
        }

        // Arithmetic Shift Register Left/Right.
        private int AslAsrRegShift(ushort opcode)
        {
            return LogicalArithmeticRegShift(opcode, GetArithmeticShiftResult);
        }

        // Arithmetic Shift Memory Left/Right.
        private int AslAsrMemShift(ushort opcode)
        {
            return LogicalArithmeticMemShift(opcode, GetArithmeticShiftResult);
        }

        private uint GetArithmeticShiftResult(uint valueForShift, int count, bool isShiftLeft, OperandSize operandSize)
        {
            int result;
            bool newC;
            bool newV = false;
            if (isShiftLeft)
            {
                result = (int)valueForShift;
                int initialV = (int)((valueForShift >> ((int)operandSize * 8 - 1)) & 0x1);
                for (int i = 0; i < count; i++)
                {
                    result = result << 1;
                    int intermediateV = (result >> ((int)operandSize * 8 - 1)) & 0x1;
                    if (intermediateV != initialV)
                    {
                        newV = true;
                        break;
                    }
                }

                result = (int)valueForShift << count;
                newC = (((valueForShift << (count - 1)) >> ((int)operandSize * 8 - 1)) & 0x1) == 1;
            }
            else
            {
                result = (int)valueForShift;
                int initialV = (int)((valueForShift >> ((int)operandSize * 8 - 1)) & 0x1);
                for (int i = 0; i < count; i++)
                {
                    result = result >> 1;
                    int intermediateV = (result >> ((int)operandSize * 8 - 1)) & 0x1;
                    if (intermediateV != initialV)
                    {
                        newV = true;
                        break;
                    }
                }

                result = (int)valueForShift >> count;
                newC = ((valueForShift >> (count - 1)) & 0x1) == 1;
            }

            _regs.X = count == 0 ? _regs.X : newC;
            _regs.C = newC;
            _flagsHelper.AlterZ((uint)result, operandSize);
            _flagsHelper.AlterN((uint)result, operandSize);
            _regs.V = newV;

            return (uint)result;
        }

        // Arithmetic Shift Left/Right, Register Shift.
        private int LslLsrRegShift(ushort opcode)
        {
            return LogicalArithmeticRegShift(opcode, GetLogicalShiftResult);
        }

        // Arithmetic Shift Left/Right, Memory Shift.
        private int LslLsrMemShift(ushort opcode)
        {
            return LogicalArithmeticMemShift(opcode, GetLogicalShiftResult);
        }

        private uint GetLogicalShiftResult(uint valueForShift, int count, bool isShiftLeft, OperandSize operandSize)
        {
            uint result;
            bool newC;
            if (isShiftLeft)
            {
                result = valueForShift << count;
                newC = (((valueForShift << (count - 1)) >> ((int)operandSize * 8 - 1)) & 0x1) == 1;
            }
            else
            {
                result = valueForShift >> count;
                newC = ((valueForShift >> (count - 1)) & 0x1) == 1;
            }

            _regs.X = count == 0 ? _regs.X : newC;
            _regs.C = newC;
            _flagsHelper.AlterZ(result, operandSize);
            _flagsHelper.AlterN(result, operandSize);
            _regs.V = false;

            return result;
        }

        // Rotate Register Data.
        private int RolRorRegRotate(ushort opcode, bool withExtend)
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
            uint result;
            if (withExtend)
            {
                result = GetRotateWithExtendResult(valueForShift, count, isShiftLeft, operandSize);
            }
            else
            {
                result = GetRotateResult(valueForShift, count, isShiftLeft, operandSize);
            }

            _memHelper.Write(result, dataRegIdx, StoreLocation.DataRegister, operandSize);

            return INSTR_DEFAULT_SIZE;
        }

        // Rotate Memory Data.
        private int RolRorMemRotate(ushort opcode, bool withExtend)
        {
            bool isShiftLeft = ((opcode >> 8) & 0x1) == 1;
            var operandSize = OperandSize.Word;
            var eaProps = _eaHelper.Get(opcode, operandSize);

            uint result;
            if (withExtend)
            {
                result = GetRotateWithExtendResult(eaProps.Operand, 1, isShiftLeft, operandSize);
            }
            else
            {
                result = GetRotateResult(eaProps.Operand, 1, isShiftLeft, operandSize);
            }

            _memHelper.Write(result, eaProps.Address, eaProps.Location, operandSize);

            return eaProps.InstructionSize;
        }

        private uint GetRotateResult(uint valueForRotate, int count, bool isRotateLeft, OperandSize operandSize)
        {
            uint result = valueForRotate;
            var operandBitQuantity = (int)operandSize * 8;
            bool newC = false;
            if (count != 0)
            {
                if (isRotateLeft)
                {
                    result = (valueForRotate << count) | (valueForRotate >> (operandBitQuantity - count));
                    newC = (result & 0x1) == 1;
                }
                else
                {
                    result = (valueForRotate >> count) | (valueForRotate << (operandBitQuantity - count));
                    newC = ((result >> (operandBitQuantity - 1)) & 0x1) == 1;
                }
            }

            _regs.C = newC;
            _flagsHelper.AlterZ(result, operandSize);
            _flagsHelper.AlterN(result, operandSize);
            _regs.V = false;

            return result;
        }

        private uint GetRotateWithExtendResult(uint valueForRotate, int count, bool isRotateLeft, OperandSize operandSize)
        {
            uint result = valueForRotate;
            var operandBitQuantity = (int)operandSize * 8;
            bool newX = _regs.X;
            bool newC = _regs.X;
            if (isRotateLeft)
            {
                for (int i = 0; i < count; i++)
                {
                    bool prevX = newX;
                    newX = newC = ((result >> (operandBitQuantity - 1)) & 0x1) == 1;
                    uint flagX = (uint)(prevX ? 1 : 0);
                    result = (result << 1) | flagX;
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    bool prevX = newX;
                    newX = newC = ((result) & 0x1) == 1;
                    uint flagX = (uint)(prevX ? 1 : 0);
                    result = (result >> 1) | (flagX << (operandBitQuantity - 1));
                }
            }

            _regs.X = newX;
            _regs.C = newC;
            _flagsHelper.AlterZ(result, operandSize);
            _flagsHelper.AlterN(result, operandSize);
            _regs.V = false;

            return result;
        }

        // Swap Register Halves.
        private int Swap(ushort opcode)
        {
            var dataRegIdx = (uint)(opcode & 0x7);
            var dataReg = _memHelper.Read(dataRegIdx, StoreLocation.DataRegister, OperandSize.Long);

            var wordBitSize = (int)OperandSize.Word * 8;
            var highWord = (ushort)dataReg;
            var lowWord = (ushort)(dataReg >> wordBitSize);
            uint result = (uint)((highWord << wordBitSize) | lowWord);

            _memHelper.Write(result, dataRegIdx, StoreLocation.DataRegister, OperandSize.Long);

            _flagsHelper.AlterN(result, OperandSize.Long);
            _flagsHelper.AlterZ(result, OperandSize.Long);
            _regs.V = _regs.C = false;

            return INSTR_DEFAULT_SIZE;
        }
    }
}
