namespace CPEMUS.Motorola.M68000.Tests
{
    public class And
    {
        [Fact]
        public void AndB()
        {
            var filePath = "C:\\Users\\Lusplitaim\\Desktop\\projects\\m68k-tests\\AND.b.json";

            using var streamReader = File.OpenText(filePath);

            for (int i = 0; i < 1; i++)
            {
                var line = streamReader.ReadLine();
                Console.WriteLine(line);
            }
        }
    }
}