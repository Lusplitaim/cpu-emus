using CPEMUS.Motorola.M68000.EA;
using CPEMUS.Motorola.M68000.Exceptions;
using CPEMUS.Motorola.M68000.Helpers;

namespace CPEMUS.Motorola.M68000
{
    public partial class M68K
    {
        // Check Register Against Bounds.
        private M68KExecResult Chk(ushort opcode)
        {
            throw new NotImplementedException();
        }

        // Take Illegal Instruction Trap.
        private M68KExecResult Illegal(ushort opcode)
        {
            throw new NotImplementedException();
        }

        // Trap.
        private M68KExecResult Trap(ushort opcode)
        {
            var vectorNumber = (opcode & 0xF) + 32;
            int clockPeriods = _exceptionHelper.Raise((uint)vectorNumber, newPc: _regs.PC + INSTR_DEFAULT_SIZE);
            return new()
            {
                InstructionSize = 0,
                ClockPeriods = clockPeriods
            };
        }

        // Trap on Overflow.
        private M68KExecResult TrapV(ushort opcode)
        {
            int clockPeriods = 0;
            if (_regs.V)
            {
                clockPeriods = _exceptionHelper.Raise((uint)ExceptionVectorType.TrapV, newPc: _regs.PC + INSTR_DEFAULT_SIZE);
                return new()
                {
                    InstructionSize = clockPeriods,
                    ClockPeriods = clockPeriods
                };
            }
            return new()
            {
                InstructionSize = INSTR_DEFAULT_SIZE,
                ClockPeriods = clockPeriods
            };
        }

        // AND Immediate to the Status Register.
        private M68KExecResult AndiToSr(ushort opcode)
        {
            if (!IsInSupervisorMode())
            {
                throw new PrivilegeViolationException();
            }

            var operand = _memHelper.Read(_regs.PC + INSTR_DEFAULT_SIZE, StoreLocation.ImmediateData, OperandSize.Word);

            _regs.SR = (ushort)(_regs.SR & operand);

            int clockPeriods = 8;

            return new()
            {
                InstructionSize = 4,
                ClockPeriods = clockPeriods
            };
        }

        // AND Immediate to the Condition Code Register.
        private M68KExecResult AndiToCcr(ushort opcode)
        {
            uint src = _memHelper.Read(_regs.PC + INSTR_DEFAULT_SIZE, StoreLocation.ImmediateData, OperandSize.Byte);
            var ccr = _regs.CCR;

            byte result = (byte)(src & ccr);

            // Storing.
            _regs.CCR = result;

            int clockPeriods = 8;

            return new()
            {
                InstructionSize = 4,
                ClockPeriods = clockPeriods
            };
        }

        // Exclusive-OR Immediate to the Status Register.
        private M68KExecResult EoriToSr(ushort opcode)
        {
            if (!IsInSupervisorMode())
            {
                throw new PrivilegeViolationException();
            }

            var operand = (ushort)_memHelper.Read(_regs.PC + INSTR_DEFAULT_SIZE, StoreLocation.Memory, OperandSize.Word);

            _regs.SR = (ushort)(_regs.SR ^ operand);

            int clockPeriods = 8;

            return new()
            {
                InstructionSize = 4,
                ClockPeriods = clockPeriods
            };
        }

        // Move from the Status Register.
        // This instruction is not privileged in MC68000.
        private M68KExecResult MoveFromSr(ushort opcode)
        {
            var eaProps = _eaHelper.Get(opcode, OperandSize.Word);

            _memHelper.Write(_regs.SR, eaProps.Address, eaProps.Location, OperandSize.Word);

            var regMode = eaProps.Mode == EAMode.DataDirect || eaProps.Mode == EAMode.AddressDirect;
            int clockPeriods = eaProps.ClockPeriods + (regMode ? 2 : 0);

            return new()
            {
                InstructionSize = eaProps.InstructionSize,
                ClockPeriods = clockPeriods
            };
        }

        // Move to the Status Register.
        private M68KExecResult MoveToSr(ushort opcode)
        {
            if (!IsInSupervisorMode())
            {
                throw new PrivilegeViolationException();
            }

            var eaProps = _eaHelper.Get(opcode, OperandSize.Word);

            _regs.SR = (ushort)eaProps.Operand;

            int clockPeriods = eaProps.ClockPeriods + 4;

            return new()
            {
                InstructionSize = eaProps.InstructionSize,
                ClockPeriods = clockPeriods
            };
        }

        // Move User Stack Pointer.
        private M68KExecResult MoveUsp(ushort opcode)
        {
            if (!IsInSupervisorMode())
            {
                throw new PrivilegeViolationException();
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

            int clockPeriods = 0;

            return new()
            {
                InstructionSize = INSTR_DEFAULT_SIZE,
                ClockPeriods = clockPeriods
            };
        }

        // Inclusive-OR Immediate to the Status Register.
        private M68KExecResult OriToSr(ushort opcode)
        {
            if (!IsInSupervisorMode())
            {
                throw new PrivilegeViolationException();
            }

            var operand = _memHelper.Read(_regs.PC + INSTR_DEFAULT_SIZE, StoreLocation.ImmediateData, OperandSize.Word);

            _regs.SR = (ushort)(_regs.SR | operand);

            int clockPeriods = 8;

            return new()
            {
                InstructionSize = 4,
                ClockPeriods = clockPeriods
            };
        }

        // Reset External Devices.
        private M68KExecResult Reset(ushort opcode)
        {
            if (!IsInSupervisorMode())
            {
                throw new PrivilegeViolationException();
            }

            int clockPeriods = 132;
            bool isTotalPeriods = true;

            return new()
            {
                InstructionSize = INSTR_DEFAULT_SIZE,
                ClockPeriods = clockPeriods,
                IsTotalCycleCount = isTotalPeriods,
            };
        }

        // Return from Exception.
        private M68KExecResult Rte(ushort opcode)
        {
            if (!IsInSupervisorMode())
            {
                throw new PrivilegeViolationException();
            }

            _exceptionHelper.Return();

            int clockPeriods = 20;
            bool isTotalPeriods = true;

            return new()
            {
                InstructionSize = 0,
                ClockPeriods = clockPeriods,
                IsTotalCycleCount = isTotalPeriods,
            };
        }

        // Load Status Register and Stop.
        private M68KExecResult Stop(ushort opcode)
        {
            if (!IsInSupervisorMode())
            {
                throw new PrivilegeViolationException();
            }

            var operand = _memHelper.Read(_regs.PC + INSTR_DEFAULT_SIZE, StoreLocation.ImmediateData, OperandSize.Word);
            _regs.SR = (ushort)operand;

            Status = M68KStatus.Stopped;

            int clockPeriods = 4;

            return new()
            {
                InstructionSize = INSTR_DEFAULT_SIZE + 2,
                ClockPeriods = clockPeriods,
            };
        }

        private bool IsInSupervisorMode()
        {
            return _regs.Mode == MPrivilegeMode.Supervisor;
        }
    }
}
