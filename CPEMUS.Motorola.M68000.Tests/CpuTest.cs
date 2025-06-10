using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPEMUS.Motorola.M68000.Tests
{
    internal class CpuTest
    {
        public string Name { get; set; }
        public CpuState Initial { get; set; }
        public CpuState Final { get; set; }
        public int Length { get; set; }
        public List<List<object>> Transactions { get; set; }
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
        public uint Sr { get; set; }
        public uint Pc { get; set; }
        public List<uint> Prefetch { get; set; }
        public List<List<uint>> Ram { get; set; }
    }
}
