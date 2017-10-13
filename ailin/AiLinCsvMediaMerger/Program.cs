using AiLinLib.Media;
using JsonParser.Formatting;
using System;
using System.IO;

namespace AiLinCsvMediaMerger
{
    class Program
    {
        static string TestWriteJson(string jsonIn)
        {
            var mr = MediaRepository.TryParse(jsonIn);
            var jsout = mr.WriteToJson();
            return jsout.ToString(JsonFormat.NormalFormat, null, 0, 2);
        }

        static void Main(string[] args)
        {
            try
            {
                var csvFilePath = args[0];
                var jsonFilePath = args[1];
                var targetJsonFilePath = args[2];
                var targetVersion = args[3];

                string jsonIn;
                using (var sr = new StreamReader(jsonFilePath))
                {
                    jsonIn = sr.ReadToEnd();
                }

                var jsonOut = TestWriteJson(jsonIn);
                using (var sw = new StreamWriter(targetJsonFilePath))
                {
                    sw.Write(jsonOut);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("CSV to Json merging encountered an exception of type '{0}', details:", e.GetType().Name);
                Console.WriteLine(e.Message);
            }
        }
    }
}
