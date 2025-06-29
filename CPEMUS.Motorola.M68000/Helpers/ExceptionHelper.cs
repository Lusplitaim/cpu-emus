using CPEMUS.Motorola.M68000.Exceptions;

namespace CPEMUS.Motorola.M68000.Helpers
{
    internal class ExceptionHelper
    {
        private readonly Stack<ExceptionVectorType> _pendingInterrupts = new();
        private readonly M68KRegs _regs;
        private readonly MemHelper _memHelper;
        public ExceptionHelper(M68KRegs regs, MemHelper memHelper)
        {
            _regs = regs;
            _memHelper = memHelper;
        }

        public bool HasPendingInterrupts => _pendingInterrupts.Count > 0;

        public void RaiseReset()
        {
            _regs.SR = default;
            EnterSupervisorMode();
            DisableTracing();
            DisableInterrupts();

            _regs.SP = _memHelper.Read(0x0, StoreLocation.Memory, OperandSize.Long);
            _regs.PC = _memHelper.Read(0x4, StoreLocation.Memory, OperandSize.Long);
        }

        public void Raise(uint vectorNumber, uint? newPc = null)
        {
            ExceptionVectorType exceptionType = vectorNumber switch
            {
                < 32 => (ExceptionVectorType)vectorNumber,
                >= 32 and <= 47 => ExceptionVectorType.Trap,
                _ => throw new InvalidOperationException("Vector number is unknown or ignored"),
            };
            var vectorAddress = vectorNumber * (int)OperandSize.Long;

            ProcessException(vectorAddress, newPc);
        }

        public void Raise(M68KException exception)
        {
            uint? newPc = exception.ExceptionVectorType switch
            {
                ExceptionVectorType.PrivilegeViolation => _regs.PC,
                ExceptionVectorType.AddressError => _regs.PC,
                ExceptionVectorType.IllegalInstruction => _regs.PC + 2,
                _ => null,
            };
            var vectorAddress = (uint)exception.ExceptionVectorType * (int)OperandSize.Long;

            ProcessException(vectorAddress, newPc);
        }

        private void ProcessException(uint vectorAddress, uint? newPc = null)
        {
            var prevStatusRegister = _regs.SR;
            EnterSupervisorMode();
            DisableTracing();

            _memHelper.PushStack(newPc ?? _regs.PC, OperandSize.Long);
            _memHelper.PushStack(prevStatusRegister, OperandSize.Word);

            _regs.PC = _memHelper.Read(vectorAddress, StoreLocation.Memory, OperandSize.Long);
        }

        private void EnterSupervisorMode()
        {
            _regs.SR = (ushort)(_regs.SR | 0x2000);
        }

        private void DisableTracing()
        {
            _regs.SR = (ushort)(_regs.SR & 0x3FFF);
        }

        private void DisableInterrupts()
        {
            _regs.SR = (ushort)(_regs.SR | 0x0700);
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
        AccessFault = 2,
        AddressError = 3,
        IllegalInstruction = 4,
        IntegerDivideByZero = 5,
        Chk = 6,
        TrapV = 7,
        PrivilegeViolation = 8,
        Trace = 9,
        FormatError = 14,
        UninitializedInterrupt = 15,
        SpuriousInterrupt = 24,
        Trap = 32,
    }
}
