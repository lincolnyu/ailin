﻿using System;
using System.Net;
using System.Text.RegularExpressions;

namespace WebKit
{
    public class VotePageNavigator
    {
        public const int DefaultVoteId = 43;
        public const int SecondVoteId = 1069;
        public const string DefaultMainPageUrlPattern = "http://www.ttpaihang.com/vote/rank.php?voteid=";
        public const string DeafultSubmitUrl = "http://www.ttpaihang.com/vote/rankpost.php";
        public const string ExpectedRadioName = "choice_id[]";

        private MyWebClient _client = new MyWebClient();

        private string _mainPageUrl;

        public VotePageNavigator(string mainPageUrl)
        {
            _mainPageUrl = mainPageUrl;
            SetClient(_client);
        }

        public VotePageNavigator(int voteId = DefaultVoteId) : this(DefaultMainPageUrlPattern + voteId.ToString())
        {
        }

        private static void SetClient(WebClient client)
        {
            // TODO some stuff hardcoded
            client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
            client.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            client.Headers.Add(HttpRequestHeader.Host, "www.ttpaihang.com");
            //client.Headers.Add(HttpRequestHeader.Connection, "Keep-Alive"); // Can't do this
            client.Headers.Add(HttpRequestHeader.CacheControl, "no-cache");
            client.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-AU,en;q=0.8,zh-Hans-CN;q=0.5,zh-Hans;q=0.3");
        }

        public string GetPageGB2312(string url)
        {
            var data = _client.DownloadData(url);
            var page = data.ConvertGB2312ToUTF();
            return page;
        }

        public PageInfo SearchForZhuLin()
        {
            // should be like <a href="/vote/rankdetail-2685.html" target=_blank class=zthei >朱琳</a>
            for (var url = _mainPageUrl; url != null; )
            {
                var page = GetPageGB2312(url);
                var pattern = @"<a href=[^>]+>朱琳</a>";
                var regex = new Regex(pattern);
                var match = regex.Match(page);
                if (match.Success)
                {
                    var start = match.Index + match.Length;

                    var input = page.GetNextInput(start).Item1;
                    var rbname = input.GetAttribute("name");
                    int? id = null;
                    if (rbname == ExpectedRadioName)
                    {
                        id = input.GetAttributeInt("value");
                    }

                    GetRank(page, start, out int? votes, out int? popularity);

                    var q = GetQuestion(page);

                    var pageId = GetPageId(url);

                    return new PageInfo
                    {
                        PageUrl = url,
                        PageId = pageId,
                        PageContent = page,
                        Id = id?.ToString(),
                        Votes = votes,
                        Popularity = popularity,
                        Question = q
                    };
                }
                url = GetLinkToNextPage(url, page);
            }
            return null;
        }

        private int GetPageId(string url)
        {
            var m = Regex.Match(url, "page=([0-9]+)");
            if (m.Success && int.TryParse(m.Groups[1].Value, out int id))
            {
                return id;
            }
            return 1;
        }

        private void GetRank(string page, int start, out int? votes, out int? popularity)
        {
            votes = null;
            popularity = null;
            var rex = new Regex(@"票数:([0-9]+)");
            var m = rex.Match(page, start);
            if (m.Success && int.TryParse(m.Groups[1].Value, out int v))
            {
                votes = v;
            }

            rex = new Regex(@"人气:<font[^>]*>([0-9]+)");
            m = rex.Match(page, start);
            if (m.Success && int.TryParse(m.Groups[1].Value, out int p))
            {
                popularity = p;
            }
        }

        private Question GetQuestion(string page)
        {
            // <strong>问题验证:</strong><font color='#ff0000'>哪个是战斗武器？</font> 选择答案：
            string pattern = @"<strong>问题验证:</strong>[ ]*<font[^>]+>([^<]*)</font>[ ]*选择答案";
            var match = Regex.Match(page, pattern);

            if (!match.Success) return null;

            var q = new Question()
            {
                Title = match.Groups[1].Value
            };

            // <Input name="answer" type="radio" Value="6b49819163"><label style="display:none">49efb</label>8
            var start = match.Index + match.Length;

            var rex = new Regex("<label[^>]*>([^<]+)</label>([^<]+)", RegexOptions.IgnoreCase);
            for (;;)
            {
                var inputInfo = page.GetNextInput(start);
                if (inputInfo == null)
                {
                    break;
                }
                var input = inputInfo.Item1;
                var name = input.GetAttribute("name");
                if (!name.Equals("answer", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                var value = input.GetAttribute("value");
                var afterInput = inputInfo.Item2 + input.Length;
                match = rex.Match(page, afterInput);
                if (match.Success)
                {
                    var label = match.Value;
                    var labelStyle =  label.GetAttribute("style");
                    var textSel = labelStyle == "display:none" ? 2 : 1;
                    var c = new Question.Choice
                    {
                        Value = value,
                        Text = match.Groups[textSel].Value
                    };
                    q.Choices.Add(c);
                }
                start = match.Index + match.Length;
            }
            return q;
        }

        public static SubmitHandler CreateSubmit(PageInfo pi, int sel)
        {
            var sh = new SubmitHandler()
            {
                RefPage = pi
            };
            sh.Process(pi.PageContent);
            if (sel < 0 || sel >= pi.Question.Choices.Count) sel = 9;
            var c = pi.Question.Choices[sel];
            sh.KeyValues["answer"] = c.Value;
            sh.KeyValues[ExpectedRadioName] = pi.Id;
            sh.KeyValues["page"] = pi.PageId.ToString();
            return sh;
        }

        public string Submit(SubmitHandler sh, string url = DeafultSubmitUrl)
        {
            _client.Headers.Add(HttpRequestHeader.Referer, sh.RefPage.PageUrl);
            var bs = _client.UploadValues(url, "POST", sh.KeyValues);
            return bs.ConvertGB2312ToUTF();
        }

        public string GetLinkToNextPage(string pageUrl, string pageContent)
        {
            // it's like： <a href=/vote/rank.php?voteid=43&amp;page=3 >下页</a>
            var pattern = @"<a href=([^>]+)>[ ]*下页[ ]*</a>";
            var match = Regex.Match(pageContent, pattern);
            if (match.Success)
            {
                var val = match.Groups[1].Value;
                val = val.UrlInHtmlToUrl();
                if (!val.IsAbsolute())
                {
                    val = _mainPageUrl.RelativeToAbsolute(val);
                }
                val = val.Trim();
                return val;
            }
            return null;
        }
    }
}
