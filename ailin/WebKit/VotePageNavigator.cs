//#define SIMULATE_TIMEOUT

using Redback.Helpers;
using System;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebKit.Helpers;
#if SIMULATE_TIMEOUT
using System.Threading;
#endif

namespace WebKit
{
    public class VotePageNavigator
    {
        public enum ErrorCodes
        {
            Success,
            WebRequestError,
            ParsingError,
            TimeOutError,
            Cancelled
        }

        public const int DefaultVoteId = 43;
        public const int SecondVoteId = 1069;
        public const string MainPageUrlPattern = "http://www.ttpaihang.com/vote/rank.php?voteid=";
        public const string SubmitUrl = "http://www.ttpaihang.com/vote/rankpost.php";
        public const string MobilePageUrlPattern = "http://m.ttpaihang.com/vote/rank.php?voteid=";
        public const string ExpectedRadioName = "choice_id[]";
        private const string MobileUserAgent = "Mozilla/5.0 (Linux; Android 7.1.1; Nexus 6P Build/NMF26F) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.85 Mobile Safari/537.36";

        private MyWebClient _client = new MyWebClient();

        private string _mainPageUrl;
        private string _baseUrl;

        private bool _isMobile;
        private bool _cancelled;
        private bool _downloading;
#if SIMULATE_TIMEOUT
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
#endif

        public VotePageNavigator(string mainPageUrl, bool isMobile = false)
        {
            _mainPageUrl = mainPageUrl;
            _baseUrl = mainPageUrl.GetBaseUrl();
            _isMobile = isMobile;
            SetClient(_client);
        }

        public VotePageNavigator(int voteId = DefaultVoteId, string mainPageUrlPattern = MainPageUrlPattern, 
            bool isMobile = false) 
            : this(mainPageUrlPattern + voteId.ToString(), isMobile)
        {
        }

        private void SetClient(WebClient client)
        {
            client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
            client.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");

            var host = _baseUrl.BaseUrlToHost();
            client.Headers.Add(HttpRequestHeader.Host, host);

            SetUserAgentIfMobile();

            client.Headers.Add(HttpRequestHeader.CacheControl, "no-cache");
            client.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-AU,en;q=0.8,zh-Hans-CN;q=0.5,zh-Hans;q=0.3");
            //client.Headers.Add(HttpRequestHeader.Connection, "Keep-Alive"); // Can't do this, will throw error
        }

        public async Task<string> GetPageGB2312(string url)
        {
            SetUserAgentIfMobile();
            try
            {
                _downloading = true;
                var data = await _client.DownloadDataTaskAsync(url);
                _downloading = false;
                var page = data.ConvertGB2312ToUTF();
                return page;
            }
            catch (WebException)
            {
                Debug.WriteLine("Downloading page raised WebException");
                return null;
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("Downloading page raised TaskCanceledException");
                return null;
            }
            catch (ObjectDisposedException)
            {
                Debug.WriteLine("Downloading page raised ObjectDisposedException");
                return null;
            }
        }

        private void SetUserAgentIfMobile()
        {
            if (_isMobile)
            {
                _client.Headers.Add(HttpRequestHeader.UserAgent, MobileUserAgent);
            }
        }

        public void CancelRefresh()
        {
            if (_cancelled == false)
            {
                _cancelled = true;
                try
                {
                    if (_downloading)
                    {
                        _client.CancelAsync();
                    }
                    Debug.WriteLine("Cancelling refresh is successful");
                }
                catch (WebException)
                {
                    Debug.WriteLine("Cancelling refresh raised WebException");
                }
            }
#if SIMULATE_TIMEOUT
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
#endif
        }

        public Tuple<string, ErrorCodes> SearchForZhuLinMobileUrl()
        {
            var task = SearchForZhuLinMobileUrlAsync();
            task.Wait();
            return task.Result;
        }

        public async Task<Tuple<string, ErrorCodes>> SearchForZhuLinMobileUrlAsync()
        {
            _cancelled = false;
            for (var url = _mainPageUrl; url != null && !_cancelled; )
            {
                var page = await GetPageGB2312(url);
                if (page == null)
                {
                    return new Tuple<string, ErrorCodes>(null, _cancelled ? ErrorCodes.Cancelled
                        : ErrorCodes.WebRequestError);
                }
                if (page.Contains("朱琳")) // TODO what about another Zhu Lin?
                {
                    return new Tuple<string, ErrorCodes>(url, ErrorCodes.Success);
                }
                url = GetLinkToNextPage(url, page);
            }
            return new Tuple<string, ErrorCodes>(null, _cancelled ? ErrorCodes.Cancelled 
                : ErrorCodes.ParsingError);
        }

        public Tuple<PageInfo, ErrorCodes> SearchForZhuLin(bool getQuestions = true)
        {
            Debug.Assert(!_isMobile);
            var task = SearchForZhuLinAsync(getQuestions);
            task.Wait();
            return task.Result;
        }

        public async Task<Tuple<PageInfo, ErrorCodes>> SearchForZhuLinAsync(bool getQuestions = true)
        {
            Debug.Assert(!_isMobile);
            _cancelled = false;
#if SIMULATE_TIMEOUT
            try
            {
                await Task.Delay(3000, _cancellationTokenSource.Token);
                return new Tuple<PageInfo, ErrorCodes>(null, ErrorCodes.TimeOutError);
            }
            catch (TaskCanceledException)
            {
                return new Tuple<PageInfo, ErrorCodes>(null, ErrorCodes.Cancelled);
            }
#else
            // should be like <a href="/vote/rankdetail-2685.html" target=_blank class=zthei >朱琳</a>
            for (var url = _mainPageUrl; url != null && !_cancelled; )
            {
                var page = await GetPageGB2312(url);
                if (page == null)
                {
                    return new Tuple<PageInfo, ErrorCodes>(null, _cancelled ? 
                        ErrorCodes.Cancelled : ErrorCodes.WebRequestError);
                }
                var pattern = @"(<a href=[^>]+>)朱琳</a>"; // TODO what about another Zhu Lin?
                var regex = new Regex(pattern);
                var match = regex.Match(page);
                if (match.Success)
                {
                    var a = match.Groups[1].Value;
                    var proflink = a.GetAttribute("href");
                    proflink = url.GetAbsoluteUrl(proflink);

                    var start = match.Index + match.Length;

                    var input = page.GetNextInput(start).Item1;
                    var rbname = input.GetAttribute("name");
                    int? id = null;
                    if (rbname == ExpectedRadioName)
                    {
                        id = input.GetAttributeInt("value");
                    }

                    GetVotesAndPopularity(page, start, out int? votes, out int? popularity);
                    GetRankAndThumbnail(page, match.Index, out int? rank, out string thumbnail);

                    var q = getQuestions? GetQuestion(page) : null;

                    if (thumbnail != null)
                    {
                        thumbnail = url.GetAbsoluteUrl(thumbnail);
                    }

                    var pageId = GetPageId(url);

                    var pi = new PageInfo
                    {
                        PageUrl = url,
                        ProfileUrl = proflink,
                        PageId = pageId,
                        PageContent = page,
                        Id = id?.ToString(),
                        Votes = votes,
                        Popularity = popularity,
                        Rank = rank,
                        Thumbnail = thumbnail,
                        Question = q
                    };
                    return new Tuple<PageInfo, ErrorCodes>(pi, ErrorCodes.Success);
                }
                url = GetLinkToNextPage(url, page);
            }
            return new Tuple<PageInfo, ErrorCodes>(null,
                _cancelled ? ErrorCodes.Cancelled : ErrorCodes.ParsingError);
#endif
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

        private void GetVotesAndPopularity(string page, int start, out int? votes, out int? popularity)
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

        private void GetRankAndThumbnail(string page, int start, out int? rank, out string thumbnail)
        {
            // go backwards

            thumbnail = null;
            rank = null;

            var imgstart = page.LastIndexOf("<img", start);
            if (imgstart >= 0)
            {
                var imgend = page.IndexOf(">", imgstart) + 1;
                if (imgend > imgstart)
                {
                    var imgtag = page.Substring(imgstart, imgend - imgstart);
                    thumbnail = imgtag.GetAttribute("src");

                    var s = page.Substring(imgend, start - imgend);
                    var rex = new Regex("TOP.([0-9]+)", RegexOptions.IgnoreCase);
                    var m = rex.Match(s);
                    if (m.Success && int.TryParse(m.Groups[1].Value, out int r))
                    {
                        rank = r;
                    }
                }
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

        public byte[] Submit(SubmitHandler sh, string url = SubmitUrl)
        {
            _client.Headers.Add(HttpRequestHeader.Referer, sh.RefPage.PageUrl);
            return _client.UploadValues(url, "POST", sh.KeyValues);
        }

        public async Task<byte[]> SubmitAsync(SubmitHandler sh, string url = SubmitUrl)
        {
            _client.Headers.Add(HttpRequestHeader.Referer, sh.RefPage.PageUrl);
            return await _client.UploadValuesTaskAsync(url, "POST", sh.KeyValues);
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
                val = _mainPageUrl.GetAbsoluteUrl(val);
                val = val.Trim();
                return val;
            }
            return null;
        }
    }
}
