using AiLinCsvMediaMerger;
using AiLinLib.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace AiLinTests
{
    [TestClass]
    public class CsvParsingTests
    {
        [TestMethod]
        public void TestDateParsing1()
        {
            const string input = "2017.1.3";
            const string expectedOutput = "20170103";
            var output = CsvMediaLoader.ParseDate(input);
            Assert.AreEqual(expectedOutput, output);
        }

        [TestMethod]
        public void MergeTest1()
        {
            const string jsonFile = @"..\..\TestData\medialist1.json";
            MediaRepository target;
            using (var sr = new StreamReader(jsonFile))
            {
                var json = sr.ReadToEnd();
                target = MediaRepository.TryParse(json);
            }
            const string csvFile = @"..\..\TestData\test.csv";
            MediaRepository source;
            using (var sr = new StreamReader(csvFile))
            {
                source = CsvMediaLoader.Load(sr);
            }
            MediaListMerger.Merge(source, target);
            Assert.AreEqual(14, target.MediaList.Count);

            var eshnhzxh = target.IdToInfo["EShNHZXH"];
            Assert.AreEqual("上海电影制片厂", eshnhzxh.Producer);
            Assert.AreEqual("https://baike.baidu.com/item/%E4%BA%8C%E5%8D%81%E5%B9%B4%E5%90%8E%E5%86%8D%E7%9B%B8%E4%BC%9A/15927?fr=aladdin", eshnhzxh.ExternalLink);
        }
    }
}
