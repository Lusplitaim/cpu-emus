namespace CPEMUS.Motorola.M68000.Helpers
{
    internal class ExceptionHelper
    {
        private readonly Stack<ExceptionVectorType> _raisedExceptions = new();
        private readonly M68KRegs _regs;
        private readonly MemHelper _memHelper;
        public ExceptionHelper(M68KRegs regs, MemHelper memHelper)
        {
            _regs = regs;
            _memHelper = memHelper;
        }

        public bool HasExceptions => _raisedExceptions.Count > 0;

        public void Raise(uint vectorNumber)
        {
            ExceptionVectorType exceptionType = vectorNumber switch
            {
                < 32 => (ExceptionVectorType)vectorNumber,
                >= 32 and <= 47 => ExceptionVectorType.Trap,
                _ => throw new InvalidOperationException("Vector number is unknown or ignored"),
            };
            var vectorAddress = vectorNumber * (int)OperandSize.Long;

            FillExceptionStackFrame(vectorAddress, exceptionType);

            _raisedExceptions.Push(exceptionType);
        }

        private void FillExceptionStackFrame(uint vectorAddress, ExceptionVectorType exceptionType)
        {
            var prevStatusRegister = _regs.SR;
            EnterSupervisorMode();

            switch (exceptionType)
            {
                case ExceptionVectorType.IllegalInstruction:
                case ExceptionVectorType.IntegerDivideByZero:
                case ExceptionVectorType.Chk:
                case ExceptionVectorType.TrapV:
                case ExceptionVectorType.PrivilegeViolation:
                case ExceptionVectorType.Trace:
                case ExceptionVectorType.Trap:
                    _memHelper.PushStack(_regs.PC, OperandSize.Long);
                    _memHelper.PushStack(prevStatusRegister, OperandSize.Word);
                    break;
            }

            _regs.PC = _memHelper.Read(vectorAddress, StoreLocation.Memory, OperandSize.Long);
        }

        private void EnterSupervisorMode()
        {
            _regs.SR = (ushort)(_regs.SR | 0x2000);
        }

        public void Return()
        {
            ExceptionVectorType exceptionType = _raisedExceptions.Pop();

            ClearExceptionStackFrame(exceptionType);
        }

        private void ClearExceptionStackFrame(ExceptionVectorType exceptionType)
        {
            switch (exceptionType)
            {
                case ExceptionVectorType.IllegalInstruction:
                case ExceptionVectorType.IntegerDivideByZero:
                case ExceptionVectorType.Chk:
                case ExceptionVectorType.TrapV:
                case ExceptionVectorType.PrivilegeViolation:
                case ExceptionVectorType.Trace:
                case ExceptionVectorType.Trap:
                    _regs.SR = (ushort)_memHelper.PopStack(OperandSize.Word);
                    _regs.PC = _memHelper.PopStack(OperandSize.Long);
                    break;
                case ExceptionVectorType.AddressError:

                    break;
            }
        }
    }

    internal enum ExceptionVectorType
    {
        ResetInitInterruptStackPointer = 0,
        ResetInitProgramCounter = 1,
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
