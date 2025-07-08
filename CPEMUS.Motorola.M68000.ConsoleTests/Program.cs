using System.Text.Json;

namespace CPEMUS.Motorola.M68000.ConsoleTests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var filePath = "C:\\m68k-tests\\STOP.json";

            using var streamReader = File.OpenText(filePath);

            var content = streamReader.ReadToEnd();

            var options = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true };
            var testCases = JsonSerializer.Deserialize<List<CpuTest>>(content, options);

            if (testCases != null && testCases.Count != 0)
            {
                RunTestCases(testCases, null);
            }
        }

        private static void RunTestCases(List<CpuTest> testCases, int? testCaseIdx)
        {
            var passedTestCount = 0;
            if (testCaseIdx.HasValue)
            {
                var testCase = testCases[testCaseIdx.Value];

                var cpu = CpuTest.ToM68K(testCase.Initial);
                var clocks = cpu.Run();
                var expected = CpuTest.ToM68K(testCase.Final);
                if (!CpuTest.Assert(cpu, expected, clocks, testCase.Length))
                {
                    Console.WriteLine($"Failed on {testCaseIdx.Value} test case: {testCase.Name}");
                    return;
                }
            }
            else
            {
                List<CpuTest> cpuTests = new();
                // Filtering tests that throw Address exception.
                foreach (var cpuTest in testCases)
                {
                    var finalCpuState = cpuTest.Final;
                    if (!finalCpuState.Ram.Any(ram => ram[0] >= 12 && ram[0] <= 15))
                    {
                        cpuTests.Add(cpuTest);
                    }
                }

                for (int i = 0; i < cpuTests.Count; i++, passedTestCount++)
                {
                    var testCase = cpuTests[i];

                    var cpu = CpuTest.ToM68K(testCase.Initial);
                    var clocks = cpu.Run();
                    var expected = CpuTest.ToM68K(testCase.Final);
                    if (!CpuTest.Assert(cpu, expected, clocks, testCase.Length))
                    {
                        Console.WriteLine($"Failed on {i} test case: {testCase.Name}");
                        return;
                    }
                }
            }
            Console.WriteLine($"All tests passed! Count : {passedTestCount}");
        }
    }
}
