using AiLinLib.Media;
using JsonParser.Formatting;
using System;
using System.IO;

namespace AiLinCsvMediaMerger
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var csvFilePath = args[0];
                var jsonFilePath = args[1];
                var targetJsonFilePath = args[2];
                var targetVersion = args.Length > 3? args[3] : null;

                MediaRepository source;
                using (var sr = new StreamReader(csvFilePath))
                {
                    source = CsvMediaLoader.Load(sr);
                }

                MediaRepository target;
                using (var sr = new StreamReader(jsonFilePath))
                {
                    var jsonStr = sr.ReadToEnd();
                    target = MediaRepository.TryParse(jsonStr);
                }

                MediaListMerger.Merge(source, target, true);

                if (targetVersion != null)
                {
                    target.Version = targetVersion;
                }

                using (var sw = new StreamWriter(targetJsonFilePath))
                {
                    var json = target.WriteToJson();
                    var jsonStr = json.ToString(JsonFormat.NormalFormat, null, 0, 2);
                    sw.Write(jsonStr);
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
