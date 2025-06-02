using CPEMUS.Motorola.M68000.Extensions;

namespace CPEMUS.Console
{
    internal class Program
    {
        static int _p = 5;
        static void Main(string[] args)
        {
            byte[] byteArray = { 0x00, 0x10 };
            byteArray[0] = 0x80;
            System.Console.WriteLine(byteArray.Length);
            System.Console.WriteLine($"{Convert.ToString(byteArray.ReadSignExtByte(0), 16)}");
        }
    }
}
