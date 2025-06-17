using CPEMUS.Motorola.M68000.Extensions;

namespace CPEMUS.Motorola.M68000.ConsoleTests
{
    internal class CpuTest
    {
        public string Name { get; set; }
        public CpuState Initial { get; set; }
        public CpuState Final { get; set; }
        public int Length { get; set; }
        public List<List<object>> Transactions { get; set; }

        public static M68K ToM68K(CpuState cpuState)
        {
            M68KRegs regs = new();

            // Filling data registers.
            regs.D[0] = cpuState.D0;
            regs.D[1] = cpuState.D1;
            regs.D[2] = cpuState.D2;
            regs.D[3] = cpuState.D3;
            regs.D[4] = cpuState.D4;
            regs.D[5] = cpuState.D5;
            regs.D[6] = cpuState.D6;
            regs.D[7] = cpuState.D7;

            // Filling address registers.
            regs.A[0] = cpuState.A0;
            regs.A[1] = cpuState.A1;
            regs.A[2] = cpuState.A2;
            regs.A[3] = cpuState.A3;
            regs.A[4] = cpuState.A4;
            regs.A[5] = cpuState.A5;
            regs.A[6] = cpuState.A6;
            regs.USP = cpuState.Usp;
            regs.SSP = cpuState.Ssp;

            regs.SR = cpuState.Sr;
            regs.PC = cpuState.Pc - 4;

            IList<byte> memTest = new MemTest(cpuState.Ram);

            return new(memTest, regs);
        }

        public static bool Assert(M68K actual, M68K expected)
        {
            bool memIdentical = CompareMemory(actual.Memory, expected.Memory);
            bool dataRegsIdentical = actual.Registers.D.SequenceEqual(expected.Registers.D);
            bool addressRegsIdentical = actual.Registers.A.SequenceEqual(expected.Registers.A);
            bool uspIdentical = actual.Registers.USP == expected.Registers.USP;
            bool sspIdentical = actual.Registers.SSP == expected.Registers.SSP;
            bool srIdentical = actual.Registers.SR == expected.Registers.SR;
            bool pcIdentical = actual.Registers.PC == expected.Registers.PC;

            return memIdentical && dataRegsIdentical && addressRegsIdentical
                && uspIdentical && sspIdentical && srIdentical && pcIdentical;
        }

        private static bool CompareMemory(ICollection<byte> first, ICollection<byte> second)
        {
            using (IEnumerator<byte> e1 = first.GetEnumerator())
            using (IEnumerator<byte> e2 = second.GetEnumerator())
            {
                var comparer = EqualityComparer<byte>.Default;

                while (e1.MoveNext())
                {
                    if (!(e2.MoveNext() && comparer.Equals(e1.Current, e2.Current)))
                    {
                        return false;
                    }
                }

                return !e2.MoveNext();
            }
        }
    }

    internal class CpuState
    {
        public uint D0 { get; set; }
        public uint D1 { get; set; }
        public uint D2 { get; set; }
        public uint D3 { get; set; }
        public uint D4 { get; set; }
        public uint D5 { get; set; }
        public uint D6 { get; set; }
        public uint D7 { get; set; }
        public uint A0 { get; set; }
        public uint A1 { get; set; }
        public uint A2 { get; set; }
        public uint A3 { get; set; }
        public uint A4 { get; set; }
        public uint A5 { get; set; }
        public uint A6 { get; set; }
        public uint Usp { get; set; }
        public uint Ssp { get; set; }
        public ushort Sr { get; set; }
        public uint Pc { get; set; }
        public List<uint> Prefetch { get; set; }
        public List<List<uint>> Ram { get; set; }
    }
}
