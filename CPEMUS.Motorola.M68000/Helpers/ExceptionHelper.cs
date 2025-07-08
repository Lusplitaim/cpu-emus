using CPEMUS.Motorola.M68000.Exceptions;

namespace CPEMUS.Motorola.M68000.Helpers
{
    internal class ExceptionHelper
    {
        private const int AUTOVECTOR_NUMBER_OFFSET = 24;
        private const int MAX_PRIORITY_LEVEL = 7;
        private const int EXCEPTION_DEFAULT_CLOCK_PERIODS = 24;

        private Dictionary<ExceptionVectorType, int> _exceptionClockPeriods = new()
        {
            { ExceptionVectorType.Reset, 36 },
            { ExceptionVectorType.AddressError, 46 },
            { ExceptionVectorType.IllegalInstruction, 30 },
            { ExceptionVectorType.IntegerDivideByZero, 34 },
            { ExceptionVectorType.Chk, 36 },
            { ExceptionVectorType.TrapV, 30 },
            { ExceptionVectorType.PrivilegeViolation, 30 },
            { ExceptionVectorType.Trace, 30 },
            { ExceptionVectorType.Trap, 30 },
        };

        private readonly PriorityQueue<int?, int> _pendingInterrupts = new(Comparer<int>.Create((x, y) => y.CompareTo(x)));
        private readonly M68KRegs _regs;
        private readonly MemHelper _memHelper;
        public ExceptionHelper(M68KRegs regs, MemHelper memHelper)
        {
            _regs = regs;
            _memHelper = memHelper;
        }

        public bool HasAllowedInterrupts
        {
            get
            {
                return _pendingInterrupts.TryPeek(out int? _, out int priority)
                    && (priority > _regs.InterruptPriorityLevel || priority == MAX_PRIORITY_LEVEL);
            }
        }

        public bool TryProcessInterrupt(out int clockPeriods)
        {
            clockPeriods = default;
            if (_pendingInterrupts.TryDequeue(out int? vectorNumber, out int priority))
            {
                if (priority < _regs.InterruptPriorityLevel)
                {
                    _pendingInterrupts.Enqueue(vectorNumber, priority);
                    return false;
                }

                uint vectorAddress = (uint)((vectorNumber ?? priority + AUTOVECTOR_NUMBER_OFFSET) * 4);
                clockPeriods = ProcessException(vectorAddress, newPriorityLevel: priority);
                return true;
            }
            return false;
        }

        public int ProcessReset()
        {
            _regs.SR = default;
            EnterSupervisorMode();
            DisableTracing();
            UpdateCurrentInterruptLevel(MAX_PRIORITY_LEVEL);

            _regs.SP = _memHelper.Read(0x0, StoreLocation.Memory, OperandSize.Long);
            _regs.PC = _memHelper.Read(0x4, StoreLocation.Memory, OperandSize.Long);

            return _exceptionClockPeriods[ExceptionVectorType.Reset];
        }

        public int Raise(uint vectorNumber, uint? newPc = null)
        {
            ExceptionVectorType exceptionType = vectorNumber switch
            {
                < 32 => (ExceptionVectorType)vectorNumber,
                >= 32 and <= 47 => ExceptionVectorType.Trap,
                _ => throw new InvalidOperationException("Vector number is unknown or ignored"),
            };

            var vectorAddress = vectorNumber * (int)OperandSize.Long;
            return ProcessException(vectorAddress, newPc);
        }

        public int Raise(M68KException exception)
        {
            uint? newPc = exception.ExceptionVectorType switch
            {
                ExceptionVectorType.PrivilegeViolation => _regs.PC,
                ExceptionVectorType.AddressError => _regs.PC,
                ExceptionVectorType.IllegalInstruction => _regs.PC + 2,
                _ => null,
            };

            int internalClockPeriods = 0;
            if (exception is IntegerDivideByZeroException divException)
            {
                internalClockPeriods = divException.InstructionClockPeriods;
            }

            var vectorAddress = (uint)exception.ExceptionVectorType * (int)OperandSize.Long;
            return ProcessException(vectorAddress, newPc) + internalClockPeriods;
        }

        private int ProcessException(uint vectorAddress, uint? newPc = null, int? newPriorityLevel = null)
        {
            var prevStatusRegister = _regs.SR;
            EnterSupervisorMode();
            DisableTracing();
            if (newPriorityLevel.HasValue)
            {
                UpdateCurrentInterruptLevel(newPriorityLevel.Value);
            }

            _memHelper.PushStack(newPc ?? _regs.PC, OperandSize.Long);
            _memHelper.PushStack(prevStatusRegister, OperandSize.Word);
            _regs.PC = _memHelper.Read(vectorAddress, StoreLocation.Memory, OperandSize.Long);

            if (!_exceptionClockPeriods.TryGetValue((ExceptionVectorType)(vectorAddress / 4), out int clockPeriods))
            {
                clockPeriods = EXCEPTION_DEFAULT_CLOCK_PERIODS;
            }
            return clockPeriods;
        }

        private void EnterSupervisorMode()
        {
            _regs.SR = (ushort)(_regs.SR | 0x2000);
        }

        private void DisableTracing()
        {
            _regs.SR = (ushort)(_regs.SR & 0x3FFF);
        }

        private void UpdateCurrentInterruptLevel(int priorityLevel)
        {
            _regs.UpdatePriorityLevel(priorityLevel);
        }

        public void Return()
        {
            var statusReg = (ushort)_memHelper.PopStack(OperandSize.Word);
            _regs.PC = _memHelper.PopStack(OperandSize.Long);
            _regs.SR = statusReg;
        }
    }

    internal enum ExceptionVectorType
    {
        Reset = 0,
        AddressError = 3,
        IllegalInstruction = 4,
        IntegerDivideByZero = 5,
        Chk = 6,
        TrapV = 7,
        PrivilegeViolation = 8,
        Trace = 9,
        Trap = 32,
    }
}
