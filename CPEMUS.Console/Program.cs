using CPEMUS.Motorola.M68000.Extensions;
using System.Buffers.Binary;

namespace CPEMUS.Console
{
    internal class Program
    {
        static int _p = 5;
        static void Main(string[] args)
        {
            int num = 0x0A0B0C0D;
            Span<byte> sp = new([0,0,0,0], 0, 4);
            BinaryPrimitives.WriteInt32BigEndian(sp, num);

            System.Console.Write("[ ");
            foreach (byte b in sp)
            {
                System.Console.Write($"{b} ");
            }
            System.Console.WriteLine(" ]");
            //System.Console.WriteLine($"{Convert.ToString(byteArray.ReadSignExtByte(0), 16)}");
        }
    }
}
