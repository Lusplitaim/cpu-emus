using System.Text.Json;

namespace CPEMUS.Motorola.M68000.ConsoleTests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var filePath = "C:\\m68k-tests\\Bcc.json";

            using var streamReader = File.OpenText(filePath);

            var content = streamReader.ReadToEnd();

            var options = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true };
            var testCases = JsonSerializer.Deserialize<List<CpuTest>>(content, options);

            if (testCases != null && testCases.Count != 0)
            {
                RunTestCases(testCases, 0);
            }
        }

        private static void RunTestCases(List<CpuTest> testCases, int? testCaseIdx)
        {
            var passedTestCount = 0;
            if (testCaseIdx.HasValue)
            {
                var testCase = testCases[testCaseIdx.Value];
                var initialCpuState = testCase.Initial;

                var cpu = CpuTest.ToM68K(testCase.Initial);
                cpu.Run();
                var expected = CpuTest.ToM68K(testCase.Final);
                if (!CpuTest.Assert(cpu, expected))
                {
                    Console.WriteLine($"Failed on {testCaseIdx.Value} test case: {testCase.Name}");
                    return;
                }
            }
            else
            {
                for (int i = 0; i < testCases.Count; i++, passedTestCount++)
                {
                    var initialCpuState = testCases[i].Initial;

                    var cpu = CpuTest.ToM68K(testCases[i].Initial);
                    cpu.Run();
                    var expected = CpuTest.ToM68K(testCases[i].Final);
                    if (!CpuTest.Assert(cpu, expected))
                    {
                        Console.WriteLine($"Failed on {i} test case: {testCases[i].Name}");
                        return;
                    }
                }
            }
            Console.WriteLine($"All tests passed! Count : {passedTestCount}");
        }
    }
}
