using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using WebKit.Helpers;

namespace AiLinTests
{
    [TestClass]
    public class ContentTest
    {
        [TestMethod]
        public void TestContent1()
        {
            const string inputFile = @"..\..\TestData\TestData2.txt";
            using (var inputStream = new StreamReader(inputFile))
            {
                var str = inputStream.ReadToEnd();
                var res = str.GetVoteResponseMessage();
                var msg = res.Item1;
                const string expected = " 您选择的选项可能已投过票,请勿重复投票！若您当天尚未投过，请返回投票页面稍后再投！ \r\n\t\t\t\t\t    (\r\n\t\t\t\t\t    RPT-3\t\t\t\t\t    )\t\t\t\r\n";
                Assert.AreEqual(expected, msg);
            }
        }

        [TestMethod]
        public void TestContent2()
        {
            const string inputFile = @"..\..\TestData\TestData2.txt";
            using (var inputStream = new StreamReader(inputFile))
            {
                var str = inputStream.ReadToEnd();
                var res = str.GetVoteResponseMessage(true);
                var msg = res.Item1;
                const string expected = "您选择的选项可能已投过票,请勿重复投票！若您当天尚未投过，请返回投票页面稍后再投！(RPT-3)";
                Assert.AreEqual(expected, msg);
            }
        }

        [TestMethod]
        public void TestContent3()
        {
            const string inputFile = @"..\..\TestData\TestData3.txt";
            using (var inputStream = new StreamReader(inputFile))
            {
                var str = inputStream.ReadToEnd();
                var res = str.GetVoteResponseMessage();
                var msg = res.Item1;
                const string expected = "1个选项被成功提交！";
                Assert.AreEqual(expected, msg);
            }
        }
    }
}
