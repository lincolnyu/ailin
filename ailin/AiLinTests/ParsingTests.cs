using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using static WebKit.Helpers.HtmlNodeHelper;

namespace AiLinTests
{
    [TestClass]
    public class ParsingTests
    {
        private static int GetNthTag(string s, string tag, int n)
        {
            string target = "<" + tag;
            int p = 0;
            for (var i = 0; ; i++)
            {
                p = s.IndexOf(target, p, StringComparison.OrdinalIgnoreCase);
                if (i == n) break;
                p += target.Length;
            }
            return p;
        }

        [TestMethod]
        public void Test1()
        {
            const string inputFile = @"..\..\TestData\TestData1.txt";
            using (var inputStream = new StreamReader(inputFile))
            {
                var str = inputStream.ReadToEnd();
                var parseStart = str.IndexOf("</font>") + "</font>".Length;
                var b = str.GetNextNode(parseStart, x=>x== "INPUT");
                var expectedStart = GetNthTag(str, "input", 0);
                var expectedContentStart = str.IndexOf(">", expectedStart + "<input".Length) + 1;
                var expectedEnd = GetNthTag(str, "input", 1);
                Assert.AreEqual(expectedStart, b.Start);
                Assert.AreEqual(expectedEnd, b.End);
                Assert.AreEqual(expectedContentStart, b.ContentStart);
                Assert.AreEqual(expectedEnd, b.ContentEnd);
                Assert.IsTrue(b.HasContent);
                Assert.AreEqual(NodeInfo.EndingTypes.ClosedBySame, b.EndingType);
                Assert.IsFalse(b.ContentOnly);
                Assert.IsTrue(b.HasOpeningTag);
                Assert.IsFalse(b.HasClosingTag);
            }
        }

        [TestMethod]
        public void Test2()
        {
            const string inputFile = @"..\..\TestData\TestData1.txt";
            using (var inputStream = new StreamReader(inputFile))
            {
                var str = inputStream.ReadToEnd();
                var parseStart = str.IndexOf(">"); 
                var b = str.GetNextNode(parseStart, x=>false);
                var expectedStart = GetNthTag(str, "tr", 0);
                var expectedContentStart = str.IndexOf(">", expectedStart + "<tr".Length) + 1;
                var expectedContentEnd = GetNthTag(str, "/tr", 0);
                var expectedEnd = expectedContentEnd + "</tr>".Length;
                Assert.AreEqual(expectedStart, b.Start);
                Assert.AreEqual(expectedEnd, b.End);
                Assert.AreEqual(expectedContentStart, b.ContentStart);
                Assert.AreEqual(expectedContentEnd, b.ContentEnd);
                Assert.IsTrue(b.HasContent);
                Assert.AreEqual(NodeInfo.EndingTypes.Matched, b.EndingType);
                Assert.IsFalse(b.ContentOnly);
                Assert.IsTrue(b.HasOpeningTag);
                Assert.IsTrue(b.HasClosingTag);
            }
        }

        [TestMethod]
        public void Test3()
        {
            const string inputFile = @"..\..\TestData\TestData1.txt";
            using (var inputStream = new StreamReader(inputFile))
            {
                var str = inputStream.ReadToEnd();
                var parseStart = GetNthTag(str, "nested", 0) - "garbage".Length;
                var b = str.GetNextNode(parseStart, x => false);
                var expectedStart = GetNthTag(str, "nested", 0);
                var expectedContentStart = str.IndexOf(">", expectedStart + "<nested".Length) + 1;
                var expectedContentEnd = GetNthTag(str, "/nested", 1);
                var expectedEnd = expectedContentEnd + "</nested>".Length;
                Assert.AreEqual(expectedStart, b.Start);
                Assert.AreEqual(expectedEnd, b.End);
                Assert.AreEqual(expectedContentStart, b.ContentStart);
                Assert.AreEqual(expectedContentEnd, b.ContentEnd);
                Assert.IsTrue(b.HasContent);
                Assert.AreEqual(NodeInfo.EndingTypes.Matched, b.EndingType);
                Assert.IsFalse(b.ContentOnly);
                Assert.IsTrue(b.HasOpeningTag);
                Assert.IsTrue(b.HasClosingTag);
            }
        }

        [TestMethod]
        public void Test4()
        {
            const string inputFile = @"..\..\TestData\TestData1.txt";
            using (var inputStream = new StreamReader(inputFile))
            {
                var str = inputStream.ReadToEnd();
                var parseStart = GetNthTag(str, "broken", 0) - "garbage".Length;
                var b = str.GetNextNode(parseStart, x => false);
                var expectedStart = GetNthTag(str, "broken", 0);
                var expectedContentStart = str.IndexOf(">", expectedStart + "<broken".Length) + 1;
                var expectedContentEnd = GetNthTag(str, "/tobreak", 0);
                var expectedEnd = expectedContentEnd + "</tobreak>".Length;
                Assert.AreEqual(expectedStart, b.Start);
                Assert.AreEqual(expectedEnd, b.End);
                Assert.AreEqual(expectedContentStart, b.ContentStart);
                Assert.AreEqual(expectedContentEnd, b.ContentEnd);
                Assert.IsTrue(b.HasContent);
                Assert.AreEqual(NodeInfo.EndingTypes.Mismatched, b.EndingType);
                Assert.IsFalse(b.ContentOnly);
                Assert.IsTrue(b.HasOpeningTag);
                Assert.IsTrue(b.HasClosingTag);
            }
        }
    }
}
