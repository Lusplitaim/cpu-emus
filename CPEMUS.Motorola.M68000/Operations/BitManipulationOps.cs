using CPEMUS.Motorola.M68000.EA;

namespace CPEMUS.Motorola.M68000
{
    public partial class M68K
    {
        private int Bchg(ushort opcode, bool srcImmediate)
        {
            var mode = (EAMode)((opcode >> 3) & 0x7);
            var operandSize = mode == EAMode.DataDirect ? OperandSize.Long : OperandSize.Byte;
            var opcodeSize = srcImmediate ? 4 : 2;
            
            int bitNumber;
            if (srcImmediate)
            {
                bitNumber = (byte)_memHelper.ReadImmediate(_regs.PC + INSTR_DEFAULT_SIZE, OperandSize.Word) % ((int)operandSize * 8);
            }
            else
            {
                var dataRegIdx = (uint)((opcode >> 9) & 0x7);
                bitNumber = (int)(_memHelper.Read(dataRegIdx, StoreLocation.DataRegister, OperandSize.Long) % ((int)operandSize * 8));
            }

            var eaResult = _eaHelper.Get(opcode, operandSize, opcodeSize);

            _flagsHelper.AlterZ((eaResult.Operand >> bitNumber) & 0x1, OperandSize.Byte);

            var result = (uint)(eaResult.Operand ^ (1 << bitNumber));

            _memHelper.Write(result, eaResult.Address, eaResult.Location, operandSize);

            return eaResult.InstructionSize;
        }

        private int Bclr(ushort opcode)
        {
            throw new NotImplementedException();
        }

        private int Bset(ushort opcode)
        {
            throw new NotImplementedException();
        }

        private int Btst(ushort opcode)
        {
            throw new NotImplementedException();
        }
    }
}
