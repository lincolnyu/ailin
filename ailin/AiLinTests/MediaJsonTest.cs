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

                Assert.AreEqual("二十年后再相会", mr.MediaList[0].Title);
                Assert.AreEqual(1, mr.MediaList[0].Songs.Count);
                Assert.AreEqual(2, mr.MediaList[0].Sources.Count);
                Assert.AreEqual(0, mr.MediaList[0].Sources[0].Playlist.Count);
                Assert.AreEqual(0, mr.MediaList[0].Sources[1].Playlist.Count);

                Assert.AreEqual("肖尔布拉克", mr.MediaList[1].Title);
                Assert.AreEqual(1, mr.MediaList[1].Songs.Count);
                Assert.AreEqual(2, mr.MediaList[1].Sources.Count);
                Assert.AreEqual(0, mr.MediaList[1].Sources[0].Playlist.Count);
                Assert.AreEqual(0, mr.MediaList[1].Sources[1].Playlist.Count);

                Assert.AreEqual("别无选择", mr.MediaList[2].Title);
                Assert.AreEqual(0, mr.MediaList[2].Songs.Count);
                Assert.AreEqual(1, mr.MediaList[2].Sources.Count);
                Assert.AreEqual(2, mr.MediaList[2].Sources[0].Playlist.Count);

                Assert.AreEqual("共和国之旗", mr.MediaList[3].Title);
                Assert.AreEqual(0, mr.MediaList[3].Songs.Count);
                Assert.AreEqual(2, mr.MediaList[3].Sources.Count);
                Assert.AreEqual(1, mr.MediaList[3].Sources[0].Playlist.Count);
                Assert.AreEqual(2, mr.MediaList[3].Sources[1].Playlist.Count);

                Assert.AreEqual("命运配方", mr.MediaList[4].Title);
                Assert.AreEqual(0, mr.MediaList[4].Songs.Count);
                Assert.AreEqual(1, mr.MediaList[4].Sources.Count);
                Assert.AreEqual(23, mr.MediaList[4].Sources[0].Playlist.Count);
            }
        }
    }
}
