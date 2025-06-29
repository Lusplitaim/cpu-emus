using CPEMUS.Motorola.M68000.EA;
using CPEMUS.Motorola.M68000.Exceptions;
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
        private const int ADDQ_SFX = 0x5000;
        private const int ADDX_SFX = 0xD100;
        private const int ASL_ASR_REG_SFX = 0xE000;
        private const int ASL_ASR_MEM_SFX = 0xE0C0;
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
        private const int LSL_LSR_REG_SFX = 0xE008;
        private const int LSL_LSR_MEM_SFX = 0xE2C0;
        private const int MOVE_SFX = 0x0000;
        private const int MOVEA_SFX = 0x0040;
        private const int MOVE_FROM_SR_SFX = 0x40C0;
        private const int MOVE_TO_CCR_SFX = 0x44C0;
        private const int MOVEM_SFX = 0x4880;
        private const int MOVEP_SFX = 0x0108;
        private const int MOVEQ_SFX = 0x7000;
        private const int NBCD_SFX = 0x4800;
        private const int NEG_SFX = 0x4400;
        private const int NEGX_SFX = 0x4000;
        private const int NOP_SFX = 0x4E71;
        private const int NOT_SFX = 0x4600;
        private const int ORI_SFX = 0x0000;
        private const int ORI_TO_CCR_SFX = 0x003C;
        private const int PEA_SFX = 0x4840;
        private const int ROL_ROR_REG_ROTATE_SFX = 0xE018;
        private const int ROL_ROR_MEM_ROTATE_SFX = 0xE6C0;
        private const int ROXL_ROXR_REG_ROTATE_SFX = 0xE010;
        private const int ROXL_ROXR_MEM_ROTATE_SFX = 0xE4C0;
        private const int RTR_SFX = 0x4E77;
        private const int RTS_SFX = 0x4E75;
        private const int SBCD_SFX = 0x8100;
        private const int SCC_SFX = 0x50C0;
        private const int SUBA_SFX_1 = 0x90C0;
        private const int SUBA_SFX_2 = 0x91C0;
        private const int SUBI_SFX = 0x0400;
        private const int SUBQ_SFX = 0x5100;
        private const int SUBX_SFX = 0x9100;
        private const int SWAP_SFX = 0x4840;
        private const int TAS_SFX = 0x4AC0;
        private const int TRAP_SFX = 0x4E40;
        private const int TRAPV_SFX = 0x4E76;
        private const int TST_SFX = 0x4A00;
        private const int UNLK_SFX = 0x4E58;
        private const int EORI_TO_SR_SFX = 0x0A7C;
        private const int ANDI_TO_SR_SFX = 0x027C;
        private const int MOVE_TO_SR_SFX = 0x46C0;
        private const int MOVE_USP_SFX = 0x4E60;
        private const int ORI_TO_SR_SFX = 0x007C;
        private const int RESET_SFX = 0x4E70;
        private const int RTE_SFX = 0x4E73;
        private const int STOP_SFX = 0x4E72;
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
        private const int ADDQ_MASK = 0xF100;
        private const int ADDX_MASK = 0xF130;
        private const int ASL_ASR_REG_MASK = 0xF018;
        private const int ASL_ASR_MEM_MASK = 0xFEC0;
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
        private const int LSL_LSR_REG_MASK = 0xF018;
        private const int LSL_LSR_MEM_MASK = 0xFEC0;
        private const int MOVE_MASK = 0xC000;
        private const int MOVEA_MASK = 0xC1C0;
        private const int MOVE_FROM_SR_MASK = 0xFFC0;
        private const int MOVE_TO_CCR_MASK = 0xFFC0;
        private const int MOVEM_MASK = 0xFB80;
        private const int MOVEP_MASK = 0xF138;
        private const int MOVEQ_MASK = 0xF100;
        private const int NBCD_MASK = 0xFFC0;
        private const int NEG_MASK = 0xFF00;
        private const int NEGX_MASK = 0xFF00;
        private const int NOP_MASK = 0xFFFF;
        private const int NOT_MASK = 0xFF00;
        private const int ORI_MASK = 0xFF00;
        private const int ORI_TO_CCR_MASK = 0xFFFF;
        private const int PEA_MASK = 0xFFC0;
        private const int ROL_ROR_REG_ROTATE_MASK = 0xF018;
        private const int ROL_ROR_MEM_ROTATE_MASK = 0xFEC0;
        private const int ROXL_ROXR_REG_ROTATE_MASK = 0xF018;
        private const int ROXL_ROXR_MEM_ROTATE_MASK = 0xFEC0;
        private const int RTR_MASK = 0xFFFF;
        private const int RTS_MASK = 0xFFFF;
        private const int SBCD_MASK = 0xF1F0;
        private const int SCC_MASK = 0xF0C0;
        private const int SUBA_MASK = 0xF1C0;
        private const int SUBI_MASK = 0xFF00;
        private const int SUBQ_MASK = 0xF100;
        private const int SUBX_MASK = 0xF130;
        private const int SWAP_MASK = 0xFFF8;
        private const int TAS_MASK = 0xFFC0;
        private const int TRAP_MASK = 0xFFF0;
        private const int TRAPV_MASK = 0xFFFF;
        private const int TST_MASK = 0xFF00;
        private const int UNLK_MASK = 0xFFF8;
        private const int EORI_TO_SR_MASK = 0xFFFF;
        private const int ANDI_TO_SR_MASK = 0xFFFF;
        private const int MOVE_TO_SR_MASK = 0xFFC0;
        private const int MOVE_USP_MASK = 0xFFF0;
        private const int ORI_TO_SR_MASK = 0xFFFF;
        private const int RESET_MASK = 0xFFFF;
        private const int RTE_MASK = 0xFFFF;
        private const int STOP_MASK = 0xFFFF;
        #endregion

        private const int INSTR_DEFAULT_SIZE = 2;

        private bool _isResetTriggered;

        public IList<byte> Memory {  get { return _mem; } }
        public M68KRegs Registers {  get { return _regs; } }
        public M68KStatus Status { get; private set; } = M68KStatus.Running;

        private readonly M68KRegs _regs;
        private readonly IList<byte> _mem;
        private readonly EAHelper _eaHelper;
        private readonly MemHelper _memHelper;
        private readonly FlagsHelper _flagsHelper;
        private readonly ExceptionHelper _exceptionHelper;
        public M68K(IList<byte> mem)
        {
            _mem = mem;
            _regs = new();
            _memHelper = new(_regs, mem);
            _eaHelper = new(_regs, mem, _memHelper);
            _flagsHelper = new(_regs);
            _exceptionHelper = new(_regs, _memHelper);
        }

        public M68K(IList<byte> mem, M68KRegs regs)
        {
            _mem = mem;
            _regs = regs;
            _memHelper = new(_regs, mem);
            _eaHelper = new(_regs, mem, _memHelper);
            _flagsHelper = new(_regs);
            _exceptionHelper = new(_regs, _memHelper);
        }

        public bool Run()
        {
            ushort opcode;
            try
            {
                if (_isResetTriggered)
                {
                    _exceptionHelper.RaiseReset();
                    _isResetTriggered = false;
                    Status = M68KStatus.Running;
                    return true;
                }

                if (Status == M68KStatus.Halted)
                {
                    return false;
                }

                if (_exceptionHelper.HasPendingInterrupts)
                {
                    //if (_exceptionHelper.TryProcessInterrupt())
                    //{
                    //    Status = M68KStatus.Running;
                    //    return true;
                    //}
                }

                bool shouldTriggerTracing = _regs.IsTracingEnabled;

                if (Status == M68KStatus.Running)
                {
                    opcode = (ushort)_memHelper.Read(_regs.PC, StoreLocation.Memory, OperandSize.Word);

                    var instructionSize = DecodeOpcode(opcode);

                    _regs.PC += (uint)instructionSize;
                }

                // Address in Program Counter should not be odd.
                if (Status == M68KStatus.Running && (_regs.PC % 2) != 0)
                {
                    _exceptionHelper.Raise((uint)ExceptionVectorType.AddressError);
                    return true;
                }

                // If the tracing is enabled at the beginning of an instruction
                // execution then trace exception will be raised after
                // the instruction is executed.
                // For info:
                // Motorola Microprocessors User's Manual (9th Edition), Paragraph 6.3.8.
                /*if (shouldTriggerTracing)
                {
                    _exceptionHelper.Raise((uint)ExceptionVectorType.Trace);
                    Status = M68KStatus.Running;
                    return true;
                }*/

                return true;
            }
            catch (M68KException ex)
            {
                _exceptionHelper.Raise(ex);
                return true;
            }
        }

        public void Reset()
        {
            _isResetTriggered = true;
        }

        public int DecodeOpcode(ushort opcode)
        {
            int? pcDisplacement = (opcode & 0xF000) switch
            {
                0x0000 => Decode0x0(opcode),
                0x4000 => Decode0x4(opcode),
                0x5000 => Decode0x5(opcode),
                0x6000 => Decode0x6(opcode),
                0x7000 => Decode0x7(opcode),
                0x8000 => Decode0x8(opcode),
                0x9000 => Decode0x9(opcode),
                0xB000 => Decode0xB(opcode),
                0xC000 => Decode0xC(opcode),
                0xD000 => Decode0xD(opcode),
                0xE000 => Decode0xE(opcode),
                _ => null
            };

            if (pcDisplacement == null && (opcode & 0xC000) == 0x0000)
            {
                pcDisplacement = Decode0x0(opcode);
            }

            return pcDisplacement ?? throw new InvalidOperationException($"The opcode {Convert.ToString(opcode, 16)} is unknown or not supported");
        }

        private int Decode0xB(ushort opcode)
        {
            if ((opcode & CMPA_MASK) == CMPA_SFX)
            {
                return Cmpa(opcode);
            }
            if ((opcode & CMP_MASK) == CMP_SFX)
            {
                return Cmp(opcode);
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
                return Mulu(opcode);
            }
            else if ((opcode & MULS_MASK) == MULS_SFX)
            {
                return Muls(opcode);
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
            if ((opcode & ADDX_MASK) == ADDX_SFX)
            {
                return Addx(opcode);
            }
            return Add(opcode);
        }

        private int Decode0xE(ushort opcode)
        {
            if ((opcode & ASL_ASR_MEM_MASK) == ASL_ASR_MEM_SFX)
            {
                return AslAsrMemShift(opcode);
            }
            if ((opcode & LSL_LSR_MEM_MASK) == LSL_LSR_MEM_SFX)
            {
                return LslLsrMemShift(opcode);
            }
            if ((opcode & ROL_ROR_MEM_ROTATE_MASK) == ROL_ROR_MEM_ROTATE_SFX)
            {
                return RolRorMemRotate(opcode, withExtend: false);
            }
            if ((opcode & ROXL_ROXR_MEM_ROTATE_MASK) == ROXL_ROXR_MEM_ROTATE_SFX)
            {
                return RolRorMemRotate(opcode, withExtend: true);
            }

            if ((opcode & ASL_ASR_REG_MASK) == ASL_ASR_REG_SFX)
            {
                return AslAsrRegShift(opcode);
            }
            if ((opcode & LSL_LSR_REG_MASK) == LSL_LSR_REG_SFX)
            {
                return LslLsrRegShift(opcode);
            }
            if ((opcode & ROL_ROR_REG_ROTATE_MASK) == ROL_ROR_REG_ROTATE_SFX)
            {
                return RolRorRegRotate(opcode, withExtend: false);
            }
            if ((opcode & ROXL_ROXR_REG_ROTATE_MASK) == ROXL_ROXR_REG_ROTATE_SFX)
            {
                return RolRorRegRotate(opcode, withExtend: true);
            }
            throw new NotImplementedException();
        }

        private int? Decode0x0(ushort opcode)
        {
            if ((opcode & ANDI_TO_CCR_MASK) == ANDI_TO_CCR_SFX)
            {
                return AndiToCcr(opcode);
            }
            if ((opcode & ANDI_TO_SR_MASK) == ANDI_TO_SR_SFX)
            {
                return AndiToSr(opcode);
            }
            if ((opcode & EORI_TO_CCR_MASK) == EORI_TO_CCR_SFX)
            {
                return EoriToCcr(opcode);
            }
            if ((opcode & EORI_TO_SR_MASK) == EORI_TO_SR_SFX)
            {
                return EoriToSr(opcode);
            }
            if ((opcode & ORI_TO_SR_MASK) == ORI_TO_SR_SFX)
            {
                return OriToSr(opcode);
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

            if ((opcode & MOVEP_MASK) == MOVEP_SFX)
            {
                return Movep(opcode);
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
            if ((opcode & ORI_TO_CCR_MASK) == ORI_TO_CCR_SFX)
            {
                return OriToCcr(opcode);
            }
            if ((opcode & ORI_MASK) == ORI_SFX)
            {
                return Ori(opcode);
            }
            if ((opcode & SUBI_MASK) == SUBI_SFX)
            {
                return Subi(opcode);
            }
            if ((opcode & MOVEA_MASK) == MOVEA_SFX)
            {
                return Movea(opcode);
            }
            if ((opcode & MOVE_MASK) == MOVE_SFX)
            {
                return Move(opcode);
            }
            return null;
        }

        private int Decode0x4(ushort opcode)
        {
            if ((opcode & RESET_MASK) == RESET_SFX)
            {
                return Reset(opcode);
            }
            if ((opcode & RTE_MASK) == RTE_SFX)
            {
                return Rte(opcode);
            }
            if ((opcode & STOP_MASK) == STOP_SFX)
            {
                return Stop(opcode);
            }
            if ((opcode & TRAP_MASK) == TRAP_SFX)
            {
                return Trap(opcode);
            }
            if ((opcode & TRAPV_MASK) == TRAPV_SFX)
            {
                return TrapV(opcode);
            }
            if ((opcode & SWAP_MASK) == SWAP_SFX)
            {
                return Swap(opcode);
            }
            if ((opcode & TAS_MASK) == TAS_SFX)
            {
                return Tas(opcode);
            }
            if ((opcode & CLR_MASK) == CLR_SFX)
            {
                return Clr(opcode);
            }
            if ((opcode & EXT_MASK) == EXT_SFX)
            {
                return Ext(opcode);
            }
            if ((opcode & JSR_MASK) == JSR_SFX)
            {
                return Jsr(opcode);
            }
            if ((opcode & MOVEM_MASK) == MOVEM_SFX)
            {
                return Movem(opcode);
            }
            if ((opcode & ILLEGAL_MASK) == ILLEGAL_SFX)
            {
                return Illegal(opcode);
            }
            if ((opcode & JMP_MASK) == JMP_SFX)
            {
                return Jmp(opcode);
            }
            if ((opcode & LEA_MASK) == LEA_SFX)
            {
                return Lea(opcode);
            }
            if ((opcode & LINK_MASK) == LINK_SFX)
            {
                return Link(opcode);
            }
            if ((opcode & MOVE_FROM_SR_MASK) == MOVE_FROM_SR_SFX)
            {
                return MoveFromSr(opcode);
            }
            if ((opcode & MOVE_TO_CCR_MASK) == MOVE_TO_CCR_SFX)
            {
                return MoveToCcr(opcode);
            }
            if ((opcode & NEG_MASK) == NEG_SFX)
            {
                return Neg(opcode, includeExtend: false);
            }
            if ((opcode & NEGX_MASK) == NEGX_SFX)
            {
                return Neg(opcode, includeExtend: true);
            }
            if ((opcode & MOVE_TO_SR_MASK) == MOVE_TO_SR_SFX)
            {
                return MoveToSr(opcode);
            }
            if ((opcode & MOVE_USP_MASK) == MOVE_USP_SFX)
            {
                return MoveUsp(opcode);
            }
            if ((opcode & NBCD_MASK) == NBCD_SFX)
            {
                return Nbcd(opcode);
            }
            if ((opcode & NOP_MASK) == NOP_SFX)
            {
                return Nop(opcode);
            }
            if ((opcode & NOT_MASK) == NOT_SFX)
            {
                return Not(opcode);
            }
            if ((opcode & PEA_MASK) == PEA_SFX)
            {
                return Pea(opcode);
            }
            if ((opcode & RTR_MASK) == RTR_SFX)
            {
                return Rtr(opcode);
            }
            if ((opcode & RTS_MASK) == RTS_SFX)
            {
                return Rts(opcode);
            }
            if ((opcode & TST_MASK) == TST_SFX)
            {
                return Tst(opcode);
            }
            if ((opcode & UNLK_MASK) == UNLK_SFX)
            {
                return Unlk(opcode);
            }
            if ((opcode & CHK_MASK) == CHK_SFX)
            {
                return Chk(opcode);
            }
            throw new NotImplementedException("The operation is unknown or not implemented");
        }

        private int Decode0x5(ushort opcode)
        {
            if ((opcode & DBCC_MASK) == DBCC_SFX)
            {
                return Dbcc(opcode);
            }
            if ((opcode & SCC_MASK) == SCC_SFX)
            {
                return Scc(opcode);
            }
            if ((opcode & SUBQ_MASK) == SUBQ_SFX)
            {
                return Subq(opcode);
            }
            if ((opcode & ADDQ_MASK) == ADDQ_SFX)
            {
                return Addq(opcode);
            }
            throw new NotImplementedException();
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

        private int Decode0x7(ushort opcode)
        {
            if ((opcode & MOVEQ_MASK) == MOVEQ_SFX)
            {
                return Moveq(opcode);
            }
            throw new NotImplementedException("The operation is unknown or not implemented");
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
            if ((opcode & SBCD_MASK) == SBCD_SFX)
            {
                return Sbcd(opcode);
            }
            return Or(opcode);
        }

        private int Decode0x9(ushort opcode)
        {
            if ((opcode & SUBA_MASK) == SUBA_SFX_1
                || (opcode & SUBA_MASK) == SUBA_SFX_2)
            {
                return Suba(opcode);
            }
            if ((opcode & SUBX_MASK) == SUBX_SFX)
            {
                return Subx(opcode);
            }
            return Sub(opcode);
        }
    }
}
