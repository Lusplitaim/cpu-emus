using CPEMUS.Motorola.M68000.Helpers;

namespace CPEMUS.Motorola.M68000
{
    public partial class M68K
    {
        // Check Register Against Bounds.
        private int Chk(ushort opcode)
        {
            throw new NotImplementedException();
        }

        // Take Illegal Instruction Trap.
        private int Illegal(ushort opcode)
        {
            throw new NotImplementedException();
        }

        // Trap.
        private int Trap(ushort opcode)
        {
            if (!IsInSupervisorMode())
            {
                throw new InvalidOperationException();
            }

            var vectorNumber = (opcode & 0xF) + 32;
            _exceptionHelper.Raise((uint)vectorNumber);
            return 0;
        }

        // Trap on Overflow.
        private int TrapV(ushort opcode)
        {
            if (!IsInSupervisorMode())
            {
                throw new InvalidOperationException();
            }

            if (_regs.V)
            {
                _exceptionHelper.Raise((uint)ExceptionVectorType.TrapV);
                return 0;
            }
            return INSTR_DEFAULT_SIZE;
        }

        // AND Immediate to the Status Register.
        private int AndiToSr(ushort opcode)
        {
            if (!IsInSupervisorMode())
            {
                throw new InvalidOperationException();
            }

            var operand = _memHelper.ReadImmediate(_regs.PC + INSTR_DEFAULT_SIZE, OperandSize.Word);

            _regs.SR = (ushort)(_regs.SR & operand);

            return 4;
        }

        // AND Immediate to the Condition Code Register.
        private int AndiToCcr(ushort opcode)
        {
            uint src = _memHelper.ReadImmediate(_regs.PC + INSTR_DEFAULT_SIZE, OperandSize.Byte);
            var ccr = _regs.CCR;

            byte result = (byte)(src & ccr);

            // Storing.
            _regs.CCR = result;

            return 4;
        }

        // Exclusive-OR Immediate to the Status Register.
        private int EoriToSr(ushort opcode)
        {
            if (!IsInSupervisorMode())
            {
                throw new InvalidOperationException();
            }

            var operand = _memHelper.ReadImmediate(_regs.PC + INSTR_DEFAULT_SIZE, OperandSize.Word);

            _regs.SR = (ushort)(_regs.SR ^ operand);

            return 4;
        }

        // Move from the Status Register.
        private int MoveFromSr(ushort opcode)
        {
            if (!IsInSupervisorMode())
            {
                throw new InvalidOperationException();
            }

            var eaProps = _eaHelper.Get(opcode, OperandSize.Word);

            _memHelper.Write(_regs.SR, eaProps.Address, eaProps.Location, OperandSize.Word);

            return eaProps.InstructionSize;
        }

        // Move to the Status Register.
        private int MoveToSr(ushort opcode)
        {
            if (!IsInSupervisorMode())
            {
                throw new InvalidOperationException();
            }

            var eaProps = _eaHelper.Get(opcode, OperandSize.Word);

            _regs.SR = (ushort)eaProps.Operand;

            return eaProps.InstructionSize;
        }

        // Move User Stack Pointer.
        private int MoveUsp(ushort opcode)
        {
            if (!IsInSupervisorMode())
            {
                throw new InvalidOperationException();
            }

            var addrRegIdx = (uint)(opcode & 0x7);
            var addrReg = _memHelper.Read(addrRegIdx, StoreLocation.AddressRegister, OperandSize.Long);

            var transferToUsp = (uint)((opcode >> 3) & 0x1) == 0;
            if (transferToUsp)
            {
                _regs.USP = addrReg;
            }
            else
            {
                _memHelper.Write(_regs.USP, addrRegIdx, StoreLocation.AddressRegister, OperandSize.Long);
            }

            return INSTR_DEFAULT_SIZE;
        }

        // Inclusive-OR Immediate to the Status Register.
        private int OriToSr(ushort opcode)
        {
            if (!IsInSupervisorMode())
            {
                throw new InvalidOperationException();
            }

            var operand = _memHelper.ReadImmediate(_regs.PC + INSTR_DEFAULT_SIZE, OperandSize.Word);

            _regs.SR = (ushort)(_regs.SR | operand);

            return 4;
        }

        // Reset External Devices.
        private int Reset(ushort opcode)
        {
            if (!IsInSupervisorMode())
            {
                throw new InvalidOperationException();
            }

            return INSTR_DEFAULT_SIZE;
        }

        // Return from Exception.
        private int Rte(ushort opcode)
        {
            if (!IsInSupervisorMode())
            {
                throw new InvalidOperationException();
            }

            _regs.SR = (ushort)_memHelper.PopStack(OperandSize.Word);
            _regs.PC = _memHelper.PopStack(OperandSize.Long);

            return 0;
        }

        // Load Status Register and Stop.
        private int Stop(ushort opcode)
        {
            if (!IsInSupervisorMode())
            {
                throw new InvalidOperationException();
            }

            var operand = _memHelper.ReadImmediate(_regs.PC + INSTR_DEFAULT_SIZE, OperandSize.Word);
            _regs.SR = (ushort)operand;

            return 4;
        }

        private bool IsInSupervisorMode()
        {
            return _regs.Mode == MPrivilegeMode.Supervisor;
        }
    }
}
