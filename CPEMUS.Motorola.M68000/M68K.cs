using CPEMUS.Motorola.M68000.EA;
using CPEMUS.Motorola.M68000.Helpers;

namespace CPEMUS.Motorola.M68000
{
    public partial class M68K
    {
        #region Opcode suffixes.
        private const int MULU_SFX = 0x00C0;
        private const int MULS_SFX = 0x01C0;
        private const int ABCD_SFX = 0x0100;
        private const int EXG_SFX = 0xC100;
        private const int ANDI_TO_CCR_SFX = 0x023C;
        private const int ANDI_SFX = 0x0200;
        private const int ADDI_SFX = 0x0600;
        private const int ADDA_SFX_1 = 0xD0C0;
        private const int ADDA_SFX_2 = 0xD1C0;
        private const int ASL_ASR_SFX = 0xE0C0;
        private const int BRA_SFX = 0x6000;
        private const int BSR_SFX = 0x6100;
        private const int BCHG_SFX_1 = 0x0140;
        private const int BCHG_SFX_2 = 0x0840;
        private const int BCLR_SFX_1 = 0x0180;
        private const int BCLR_SFX_2 = 0x0880;
        private const int BSET_SFX_1 = 0x01C0;
        private const int BSET_SFX_2 = 0x08C0;
        private const int BTST_SFX_1 = 0x0100;
        private const int BTST_SFX_2 = 0x0800;
        private const int CLR_SFX = 0x4200;
        private const int CMP_SFX = 0xB000;
        private const int CMPA_SFX = 0xB0C0;
        private const int CMPI_SFX = 0x0C00;
        private const int CMPM_SFX = 0xB108;
        private const int DBCC_SFX = 0x50C8;
        private const int DIVS_SFX = 0x81C0;
        private const int DIVU_SFX = 0x80C0;
        private const int EOR_SFX = 0xB100;
        private const int EORI_SFX = 0x0A00;
        private const int EORI_TO_CCR_SFX = 0x0A3C;
        private const int EXT_SFX = 0x4800;
        private const int CHK_SFX = 0x4000;
        private const int ILLEGAL_SFX = 0x4AFC;
        private const int JMP_SFX = 0x4EC0;
        private const int JSR_SFX = 0x4E80;
        private const int LEA_SFX = 0x41C0;
        private const int LINK_SFX = 0x4E50;
        #endregion

        #region Opcode masks.
        private const int MULU_MASK = 0x01C0;
        private const int MULS_MASK = 0x01C0;
        private const int ABCD_MASK = 0x01F0;
        private const int EXG_MASK = 0xF130;
        private const int ANDI_TO_CCR_MASK = 0xFFFF;
        private const int ANDI_MASK = 0xFF00;
        private const int ADDI_MASK = 0xFF00;
        private const int ADDA_MASK = 0xF1C0;
        private const int ASL_ASR_MASK = 0xFEC0;
        private const int BRA_MASK = 0xFF00;
        private const int BSR_MASK = 0xFF00;
        private const int BCHG_MASK_1 = 0xF1C0;
        private const int BCHG_MASK_2 = 0xFFC0;
        private const int BCLR_MASK_1 = 0xF1C0;
        private const int BCLR_MASK_2 = 0xFFC0;
        private const int BSET_MASK_1 = 0xF1C0;
        private const int BSET_MASK_2 = 0xFFC0;
        private const int BTST_MASK_1 = 0xF1C0;
        private const int BTST_MASK_2 = 0xFFC0;
        private const int CLR_MASK = 0xFF00;
        private const int CMP_MASK = 0xF100;
        private const int CMPA_MASK = 0xF0C0;
        private const int CMPI_MASK = 0xFF00;
        private const int CMPM_MASK = 0xF138;
        private const int DBCC_MASK = 0xF0F8;
        private const int DIVS_MASK = 0xF1C0;
        private const int DIVU_MASK = 0xF1C0;
        private const int EOR_MASK = 0xF100;
        private const int EORI_MASK = 0xFF00;
        private const int EORI_TO_CCR_MASK = 0xFFFF;
        private const int EXT_MASK = 0xFE38;
        private const int CHK_MASK = 0xF040;
        private const int ILLEGAL_MASK = 0xFFFF;
        private const int JMP_MASK = 0xFFC0;
        private const int JSR_MASK = 0xFFC0;
        private const int LEA_MASK = 0xF1C0;
        private const int LINK_MASK = 0xFFF8;
        #endregion

        private const int INSTR_DEFAULT_SIZE = 2;

        private readonly M68KRegs _regs;
        private readonly IList<byte> _mem;
        private readonly EAHelper _eaHelper;
        private readonly MemHelper _memHelper;
        private readonly FlagsHelper _flagsHelper;

        public IList<byte> Memory {  get { return _mem; } }
        public M68KRegs Registers {  get { return _regs; } }

        public M68K(IList<byte> mem)
        {
            _mem = mem;
            _regs = new();
            _memHelper = new(_regs, mem);
            _eaHelper = new(_regs, mem, _memHelper);
            _flagsHelper = new(_regs);
        }

        public M68K(IList<byte> mem, M68KRegs regs)
        {
            _mem = mem;
            _regs = regs;
            _memHelper = new(_regs, mem);
            _eaHelper = new(_regs, mem, _memHelper);
            _flagsHelper = new(_regs);
        }

        public bool Run()
        {
            ushort opcode;
            try
            {
                opcode = (ushort)_memHelper.Read(_regs.PC, StoreLocation.Memory, OperandSize.Word);
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }

            var instructionSize = DecodeOpcode(opcode);

            _regs.PC += (uint)instructionSize;

            return true;
        }

        public int DecodeOpcode(ushort opcode)
        {
            return (opcode & 0xF000) switch
            {
                0x0000 => Decode0x0(opcode),
                0x4000 => Decode0x4(opcode),
                0x5000 => Decode0x5(opcode),
                0x6000 => Decode0x6(opcode),
                0x8000 => Decode0x8(opcode),
                0xB000 => Decode0xB(opcode),
                0xC000 => Decode0xC(opcode),
                0xD000 => Decode0xD(opcode),
                0xE000 => Decode0xE(opcode),
                _ => throw new InvalidOperationException($"The opcode {Convert.ToString(opcode, 16)} is unknown or not supported"),
            };
        }

        private int Decode0xB(ushort opcode)
        {
            if ((opcode & CMP_MASK) == CMP_SFX)
            {
                return Cmp(opcode);
            }
            if ((opcode & CMPA_MASK) == CMPA_SFX)
            {
                return Cmpa(opcode);
            }
            if ((opcode & CMPM_MASK) == CMPM_SFX)
            {
                return Cmpm(opcode);
            }
            if ((opcode & EOR_MASK) == EOR_SFX)
            {
                return Eor(opcode);
            }
            throw new NotImplementedException("The operation is unknown or not implemented");
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
                return Exg(opcode);
            }
            else
            {
                return And(opcode);
            }
        }

        private int Decode0xD(ushort opcode)
        {
            bool isAdda = (opcode & ADDA_MASK) == ADDA_SFX_1
                || (opcode & ADDA_MASK) == ADDA_SFX_2;
            if (isAdda)
            {
                return Adda(opcode);
            }
            return Add(opcode);
        }

        private int Decode0xE(ushort opcode)
        {
            if ((opcode & ASL_ASR_MASK) == ASL_ASR_SFX)
            {
                return AslAsr(opcode);
            }
            return AslAsr(opcode);
        }

        private int Decode0x0(ushort opcode)
        {
            if ((opcode & ANDI_TO_CCR_MASK) == ANDI_TO_CCR_SFX)
            {
                return AndiToCcr(opcode);
            }
            if ((opcode & EORI_TO_CCR_MASK) == EORI_TO_CCR_SFX)
            {
                return EoriToCcr(opcode);
            }
            if ((opcode & EORI_MASK) == EORI_SFX)
            {
                return Eori(opcode);
            }
            if ((opcode & ADDI_MASK) == ADDI_SFX)
            {
                return Addi(opcode);
            }
            if ((opcode & ANDI_MASK) == ANDI_SFX)
            {
                return Andi(opcode);
            }

            if ((opcode & BCHG_MASK_1) == BCHG_SFX_1)
            {
                return Bchg(opcode, srcImmediate: false);
            }
            else if ((opcode & BCHG_MASK_2) == BCHG_SFX_2)
            {
                return Bchg(opcode, srcImmediate: true);
            }

            if ((opcode & BCLR_MASK_1) == BCLR_SFX_1)
            {
                return Bclr(opcode, srcImmediate: false);
            }
            else if ((opcode & BCLR_MASK_2) == BCLR_SFX_2)
            {
                return Bclr(opcode, srcImmediate: true);
            }

            if ((opcode & BSET_MASK_1) == BSET_SFX_1)
            {
                return Bset(opcode, srcImmediate: false);
            }
            else if ((opcode & BSET_MASK_2) == BSET_SFX_2)
            {
                return Bset(opcode, srcImmediate: true);
            }

            if ((opcode & BTST_MASK_1) == BTST_SFX_1)
            {
                return Btst(opcode, srcImmediate: false);
            }
            else if ((opcode & BTST_MASK_2) == BTST_SFX_2)
            {
                return Btst(opcode, srcImmediate: true);
            }

            if ((opcode & CMPI_MASK) == CMPI_SFX)
            {
                return Cmpi(opcode);
            }
            throw new NotImplementedException("The operation is unknown or not implemented");
        }

        private int Decode0x4(ushort opcode)
        {
            if ((opcode & CLR_MASK) == CLR_SFX)
            {
                return Clr(opcode);
            }
            if ((opcode & EXT_MASK) == EXT_SFX)
            {
                return Ext(opcode);
            }
            if ((opcode & CHK_MASK) == CHK_SFX)
            {
                return Chk(opcode);
            }
            if ((opcode & ILLEGAL_MASK) == ILLEGAL_SFX)
            {
                return Illegal(opcode);
            }
            if ((opcode & JMP_MASK) == JMP_SFX)
            {
                return Jmp(opcode);
            }
            if ((opcode & JSR_MASK) == JSR_SFX)
            {
                return Jsr(opcode);
            }
            if ((opcode & LEA_MASK) == LEA_SFX)
            {
                return Lea(opcode);
            }
            if ((opcode & LINK_MASK) == LINK_SFX)
            {
                return Link(opcode);
            }
            throw new NotImplementedException("The operation is unknown or not implemented");
        }

        private int Decode0x5(ushort opcode)
        {
            if ((opcode & DBCC_MASK) == DBCC_SFX)
            {
                return Dbcc(opcode);
            }
            return Addq(opcode);
        }

        private int Decode0x6(ushort opcode)
        {
            if ((opcode & BRA_MASK) == BRA_SFX)
            {
                return Bra(opcode);
            }
            if ((opcode & BSR_MASK) == BSR_SFX)
            {
                return Bsr(opcode);
            }
            return Bcc(opcode);
        }

        private int Decode0x8(ushort opcode)
        {
            if ((opcode & DIVS_MASK) == DIVS_SFX)
            {
                return Divs(opcode);
            }
            if ((opcode & DIVU_MASK) == DIVU_SFX)
            {
                return Divu(opcode);
            }
            throw new NotImplementedException("The operation is unknown or not implemented");
        }

        private void PushStack(uint value, OperandSize operandSize)
        {
            _regs.SP -= Math.Max((uint)operandSize, (uint)OperandSize.Word);
            _memHelper.Write(value, _regs.SP, StoreLocation.Memory, operandSize);
        }

        private uint PopStack(OperandSize operandSize)
        {
            var result = _memHelper.Read(_regs.SP, StoreLocation.Memory, operandSize);
            _regs.SP += Math.Max((uint)operandSize, (uint)OperandSize.Word);
            return result;
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

        // Andi to CCR.
        private int AndiToCcr(ushort opcode)
        {
            uint src = _memHelper.ReadImmediate(_regs.PC + INSTR_DEFAULT_SIZE, OperandSize.Byte);
            var ccr = _regs.SR;

            byte result = (byte)(src & ccr);

            // Storing.
            _memHelper.Write(result, default, StoreLocation.StatusRegister, OperandSize.Byte);

            return 4;
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
    }
}
