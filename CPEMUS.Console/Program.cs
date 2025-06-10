using CPEMUS.Motorola.M68000;
using CPEMUS.Motorola.M68000.Console;
using CPEMUS.Motorola.M68000.Extensions;
using System.Text.Json;

namespace CPEMUS.Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            /*var filePath = "C:\\Users\\Lusplitaim\\Desktop\\projects\\m68k-tests\\AND.b.json";

            using var streamReader = File.OpenText(filePath);

            var content = streamReader.ReadToEnd();

            var options = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true };
            var testCases = JsonSerializer.Deserialize<List<CpuTestCase>>(content, options);*/

            byte[] mem = [0x50, 0x00];
            M68KRegs regs = new();
            regs.D[0] = 0x818082FF;

            M68K cpu = new(mem, regs);
            cpu.Run();
        }
    }
}
