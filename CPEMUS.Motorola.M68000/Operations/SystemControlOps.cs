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
            var vectorNumber = (opcode & 0xF) + 32;
            _exceptionHelper.Raise((uint)vectorNumber);
            return 0;
        }

        // Trap on Overflow.
        private int TrapV(ushort opcode)
        {
            if (_regs.V)
            {
                _exceptionHelper.Raise((uint)ExceptionVectorType.TrapV);
                return 0;
            }
            return INSTR_DEFAULT_SIZE;
        }
    }
}
