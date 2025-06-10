using CPEMUS.Motorola.M68000;
using CPEMUS.Motorola.M68000.Extensions;

namespace CPEMUS.Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            byte[] mem = [0x50, 0x00];
            M68KRegs regs = new();
            regs.D[0] = 0x818082FF;

            M68K cpu = new(mem, regs);
            cpu.Run();
        }
    }
}
