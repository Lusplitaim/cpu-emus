using System.Runtime.InteropServices;

namespace CPEMUS.Motorola.M68000
{
    [StructLayout(LayoutKind.Explicit)]
    internal class M68KRegs
    {
        // Data registers.
        [FieldOffset(0)]
        public int D0;
        [FieldOffset(4)]
        public int D1;
        [FieldOffset(8)]
        public int D2;
        [FieldOffset(12)]
        public int D3;
        [FieldOffset(16)]
        public int D4;
        [FieldOffset(20)]
        public int D5;
        [FieldOffset(24)]
        public int D6;
        [FieldOffset(28)]
        public int D7;

        // Address registers.
        [FieldOffset(32)]
        public int A0;
        [FieldOffset(36)]
        public int A1;
        [FieldOffset(40)]
        public int A2;
        [FieldOffset(44)]
        public int A3;
        [FieldOffset(48)]
        public int A4;
        [FieldOffset(52)]
        public int A5;
        [FieldOffset(56)]
        public int A6;

        // User Stack Pointer.
        [FieldOffset(60)]
        public int A7;
        [FieldOffset(60)]
        public int USP;

        // Program Counter.
        [FieldOffset(64)]
        public int PC;

        // Condition Code Register.
        [FieldOffset(68)]
        public byte CCR;
    }
}
