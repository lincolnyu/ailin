using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using AiLinLib.Media;

namespace AiLinTests
{
    [TestClass]
    public class MediaJsonTest
    {
        [TestMethod]
        public void TestJsonParsing()
        {
            const string jsonFile = @"..\..\TestData\medialist1.json";
            using (var sr = new StreamReader(jsonFile))
            {
                var json = sr.ReadToEnd();
                var mr = MediaRepository.TryParse(json);
                Assert.AreEqual("1.0", mr.Version);
                Assert.AreEqual(json, mr.OriginalJson);
                Assert.AreEqual(5, mr.MediaList.Count);
            }
        }
    }
}
