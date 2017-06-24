using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebKit.Helpers;

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

        public async Task<string> GetPageGB2312(string url)
        {
            var data = await _client.DownloadDataTaskAsync(url);
            var page = data.ConvertGB2312ToUTF();
            return page;
        }

        public PageInfo SearchForZhuLin(bool getQuestions = true)
        {
            var task = SearchForZhuLinAsync(getQuestions);
            task.Wait();
            return task.Result;
        }

        public async Task<PageInfo> SearchForZhuLinAsync(bool getQuestions = true)
        {
            // should be like <a href="/vote/rankdetail-2685.html" target=_blank class=zthei >朱琳</a>
            for (var url = _mainPageUrl; url != null; )
            {
                var page = await GetPageGB2312(url);
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

                    var q = getQuestions? GetQuestion(page) : null;

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

            // could be like:
            // <Input name="answer" type="radio" Value="6b49819163"><label style="display:none">49efb</label>8
            var start = match.Index + match.Length;

            for (;;)
            {
                var block = page.GetNextNode(start, x=>x.ToLower() == "input", true);
                if (block.OpeningTag.ToLower() == "input")
                {
                    var t = page.Substring(block.Start, block.ContentStart - block.Start);
                    var val = t.GetAttribute("value");
                    var inner = page.Substring(block.ContentStart, block.ContentLength);
                    var text = inner.GetTextContent();
                    var c = new Question.Choice
                    {
                        Value = val,
                        Text = text
                    };
                    q.Choices.Add(c);
                }
                if (block.EndingType == HtmlNodeHelper.NodeInfo.EndingTypes.Mismatched)
                {
                    // means it reaches the parent node
                    break;
                }
                start = block.End;
            }

            return q;
        }

        public static SubmitHandler CreateSubmit(PageInfo pi, int sel)
        {
            if (sel < 0 || sel >= pi.Question.Choices.Count)
            {
                sel = 0;
            }
            var c = pi.Question.Choices[sel];
            return CreateSubmit(pi, c);
        }

        public static SubmitHandler CreateSubmit(PageInfo pi, Question.Choice c)
        {
            var sh = new SubmitHandler()
            {
                RefPage = pi
            };
            sh.Process(pi.PageContent);
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

        public async Task<string> SubmitAsync(SubmitHandler sh, string url = DeafultSubmitUrl)
        {
            _client.Headers.Add(HttpRequestHeader.Referer, sh.RefPage.PageUrl);
            var bs = await _client.UploadValuesTaskAsync(url, "POST", sh.KeyValues);
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
