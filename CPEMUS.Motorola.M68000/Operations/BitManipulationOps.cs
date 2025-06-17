using CPEMUS.Motorola.M68000.EA;

namespace CPEMUS.Motorola.M68000
{
    public partial class M68K
    {
        private int TestAndChangeBit(ushort opcode, bool srcImmediate, bool updateDestValue, Func<uint, int, uint> changeBit)
        {
            var mode = (EAMode)((opcode >> 3) & 0x7);
            var operandSize = mode == EAMode.DataDirect ? OperandSize.Long : OperandSize.Byte;
            var opcodeSize = srcImmediate ? 4 : 2;

            int bitNumber;
            if (srcImmediate)
            {
                bitNumber = (int)_memHelper.ReadImmediate(_regs.PC + INSTR_DEFAULT_SIZE, OperandSize.Word) % ((int)operandSize * 8);
            }
            else
            {
                var dataRegIdx = (uint)((opcode >> 9) & 0x7);
                bitNumber = (int)(_memHelper.Read(dataRegIdx, StoreLocation.DataRegister, OperandSize.Long) % ((int)operandSize * 8));
            }

            var eaResult = _eaHelper.Get(opcode, operandSize, opcodeSize);

            _flagsHelper.AlterZ((eaResult.Operand >> bitNumber) & 0x1, OperandSize.Byte);

            var result = changeBit(eaResult.Operand, bitNumber);

            if (updateDestValue)
            {
                _memHelper.Write(result, eaResult.Address, eaResult.Location, operandSize);
            }

            return eaResult.InstructionSize;
        }

        // Test Bit and Change.
        private int Bchg(ushort opcode, bool srcImmediate)
        {
            return TestAndChangeBit(opcode, srcImmediate, updateDestValue: true, (operand, bitNumber) =>
            {
                return (uint)(operand ^ (1 << bitNumber));
            });
        }

        // Test Bit and Clear.
        private int Bclr(ushort opcode, bool srcImmediate)
        {
            return TestAndChangeBit(opcode, srcImmediate, updateDestValue: true, (operand, bitNumber) =>
            {
                return (uint)(operand & ~(1 << bitNumber));
            });
        }

        // Test Bit and Set.
        private int Bset(ushort opcode, bool srcImmediate)
        {
            return TestAndChangeBit(opcode, srcImmediate, updateDestValue: true, (operand, bitNumber) =>
            {
                return operand | (uint)(1 << bitNumber);
            });
        }

        private int Btst(ushort opcode, bool srcImmediate)
        {
            return TestAndChangeBit(opcode, srcImmediate, updateDestValue: false, (operand, bitNumber) =>
            {
                return operand;
            });
        }
    }
}
