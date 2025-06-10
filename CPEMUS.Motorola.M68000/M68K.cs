using CPEMUS.Motorola.M68000.EA;
using CPEMUS.Motorola.M68000.Helpers;

namespace CPEMUS.Motorola.M68000
{
    public class M68K
    {
        #region Opcode suffixes.
        private readonly int MULU_SFX = 0x00C0;
        private readonly int MULS_SFX = 0x01C0;
        private readonly int ABCD_SFX = 0x0100;
        private readonly int EXG_SFX = 0x0100;
        #endregion

        #region Opcode masks.
        private readonly int MULU_MASK = 0x01C0;
        private readonly int MULS_MASK = 0x01C0;
        private readonly int ABCD_MASK = 0x01F0;
        private readonly int EXG_MASK = 0x0130;
        #endregion

        private const int INSTR_DEFAULT_SIZE = 2;

        private readonly M68KRegs _regs;
        private readonly byte[] _mem;
        private readonly EAHelper _eaHelper;
        private readonly MemHelper _memHelper;
        private readonly FlagsHelper _flagsHelper;
        public M68K(byte[] mem)
        {
            _mem = mem;
            _regs = new();
            _memHelper = new(_regs, mem);
            _eaHelper = new(_regs, mem, _memHelper);
            _flagsHelper = new(_regs);
        }

        public M68K(byte[] mem, M68KRegs regs)
        {
            _mem = mem;
            _regs = regs;
            _memHelper = new(_regs, mem);
            _eaHelper = new(_regs, mem, _memHelper);
            _flagsHelper = new(_regs);
        }

        public bool Run()
        {
            if (_regs.PC >= _mem.Length)
            {
                return false;
            }

            var opcode = (ushort)_memHelper.Read(_regs.PC, StoreLocation.Memory, OperandSize.Word);
            var instructionSize = DecodeOpcode(opcode);

            _regs.PC += (uint)instructionSize;

            return true;
        }

        public int DecodeOpcode(ushort opcode)
        {
            if ((opcode & 0xF000) == 0x0000)
            {
                return Decode0x0(opcode);
            }
            if ((opcode & 0xF000) == 0xC000)
            {
                return Decode0xC(opcode);
            }
            if ((opcode & 0xF000) == 0xD000)
            {
                return Decode0xD(opcode);
            }

            throw new InvalidOperationException($"The opcode {Convert.ToString(opcode, 16)} is unknown or not supported");
        }

        private int Decode0xC(ushort opcode)
        {
            if ((opcode & MULU_MASK) == MULU_SFX)
            {
                return Mulu();
            }
            else if ((opcode & MULS_MASK) == MULS_SFX)
            {
                return Muls();
            }
            else if ((opcode & ABCD_MASK) == ABCD_SFX)
            {
                throw new NotImplementedException();
            }
            else if ((opcode & EXG_MASK) == EXG_SFX)
            {
                return Exg();
            }
            else
            {
                return And(opcode);
            }
        }

        private int Decode0xD(ushort opcode)
        {
            return Add(opcode);
        }

        private int Decode0x0(ushort opcode)
        {
            return Addi(opcode);
        }

        private int Mulu()
        {
            throw new NotImplementedException();
        }

        private int Muls()
        {
            throw new NotImplementedException();
        }

        private int Exg()
        {
            throw new NotImplementedException();
        }

        private int And(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);

            uint srcIdx = (uint)((opcode >> 9) & 0x7);
            uint src = _memHelper.Read(srcIdx, StoreLocation.DataRegister, operandSize);

            var eaProps = _eaHelper.Get(opcode, operandSize);
            uint dest = eaProps.Operand;

            uint result = src & dest;

            // Flags.
            _regs.N = (result >> ((int)operandSize * 8 - 1)) == 1;
            _regs.Z = result == 0;
            _regs.V = false;
            _regs.C = false;

            // Storing.
            int storeDirection = (opcode >> 8) & 0x1;
            if ((StoreDirection)storeDirection == StoreDirection.Source)
            {
                _memHelper.Write(result, srcIdx, StoreLocation.DataRegister, operandSize);
            }
            else
            {
                _memHelper.Write(result, eaProps.Address, eaProps.Location, operandSize);
            }

            return eaProps.InstructionSize;
        }

        private int Andi(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);
            var immediateDataSize = operandSize == OperandSize.Long
                ? (int)OperandSize.Long
                : (int)OperandSize.Word;
            var pc = _regs.PC;

            uint src = _memHelper.ReadImmediate((uint)(pc + INSTR_DEFAULT_SIZE), operandSize);

            var eaProps = _eaHelper.Get(opcode, operandSize, INSTR_DEFAULT_SIZE + immediateDataSize);
            uint dest = eaProps.Operand;

            uint result = src & dest;

            // Flags.
            _regs.N = (result >> ((int)operandSize * 8 - 1)) == 1;
            _regs.Z = result == 0;
            _regs.V = false;
            _regs.C = false;

            // Storing.
            _memHelper.Write(result, eaProps.Address, eaProps.Location, operandSize);

            return eaProps.InstructionSize;
        }

        private void Abcd(ref byte src, ref byte dest)
        {
            bool carry = false;
            int res = src + dest + (_regs.X ? 1 : 0);
            // If the lower nibble > 9 perform correction (add 0x6).
            if ((res & 0x0F) > 0x09)
            {
                res += 0x6;
                carry = true;
            }
            // If the higher nibble > 9 perform correction (add 0x60).
            if ((res & 0xF0) > 0x90)
            {
                res += 0x60;
            }
            if (res > 0xFF)
            {
                carry = true;
            }

            _regs.X = carry;
            _regs.C = carry;
            // Clear if the result is non-zero.
            if ((byte)res != 0)
            {
                _regs.Z = false;
            }

            dest = (byte)res;
        }

        // Add.
        private int Add(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);

            var dataRegIdx = (uint)((opcode >> 9) & 0x3);
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

        // Adda.
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

            var addrRegIdx = (uint)((opcode >> 9) & 0x3);
            // The entire destination address register is used regardless of the operation size.
            var addrReg = _memHelper.Read(addrRegIdx, StoreLocation.AddressRegister, OperandSize.Long);

            var eaProps = _eaHelper.Get(opcode, operandSize);

            long result = (int)eaProps.Operand + addrReg;

            // Storing.
            _memHelper.Write((uint)result, addrRegIdx, StoreLocation.AddressRegister, operandSize);

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
    }
}
