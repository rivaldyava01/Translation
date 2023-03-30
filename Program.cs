namespace translation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Translator translator = new Translator();
            String rawdata1 = "020909060000010000FF090C07E7020F03031D1000FE20001001E01100090C000003FE07020000008000FF090C00000AFE07030000008000FF0F3C03001601";
            String rawdata2 = "020209060000830008FF02090300030103000420FFFFFFFE0420FFFFFFF9111E110A1101111E";
            Console.WriteLine(translator.Translate(rawdata1));
            Console.WriteLine(translator.Translate(rawdata2));
        }
    }
}