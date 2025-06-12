using System.Text.Json;

namespace CPEMUS.Motorola.M68000.ConsoleTests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var filePath = "C:\\m68k-tests\\ADD.b.json";

            using var streamReader = File.OpenText(filePath);

            var content = streamReader.ReadToEnd();

            var options = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true };
            var testCases = JsonSerializer.Deserialize<List<CpuTest>>(content, options);

            if (testCases != null && testCases.Count != 0)
            {
                for (int i = 0; i < testCases.Count; i++)
                {
                    var initialCpuState = testCases[i].Initial;
                    var opcodeAddress = initialCpuState.Pc;
                    ushort opcode = (ushort)initialCpuState.Prefetch[0];
                    var immediateData = initialCpuState.Prefetch[1];

                    var cpu = CpuTest.ToM68K(testCases[i].Initial, opcode, immediateData, opcodeAddress);
                    cpu.Run();
                    var expected = CpuTest.ToM68K(testCases[i].Final, opcode, immediateData, opcodeAddress);
                    if (!CpuTest.Assert(cpu, expected))
                    {
                        Console.WriteLine($"Failed on {i + 1} test case: {testCases[i].Name}");
                        return;
                    }
                }
                Console.WriteLine($"All tests passed!");
            }
        }
    }
}
