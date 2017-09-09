using AngleSharp.Parser.Html;
using HttpCode.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace spider
{
    class Program
    {
        public List<string> UrlList { get; set; }
        public string BaseUrl { get; set; }
        public string PostUrl1 { get { return "http://localhost:8088/"; } private set { } }
        public string PostUrl2 { get { return "http://localhost:8089/"; } private set { } }

        public int AllPages { get; set; }
        public static void Main(string[] args)
        {
            var program = new Program();
            var input = "y";
            var _url = string.Empty;
            List<ValueTuple<string, string>> animationInfos = null;
            program.BaseUrl = "https://acg12.com/category/others/";
            //if (args.Length > 1)
            //{
            //    program.BaseUrl = "https://acg12.com/category/others/";
            //}
            //else
            //{
            //    Console.WriteLine("Error to load the args, please input correct url");
            //}

            Console.WriteLine("start....");
            Console.WriteLine("ACG小站资源展示：");
            #region test
            //var downloadPage = program.GetDownloadPageAsync("https://acg12.com/200340/");
            //downloadPage.GetAwaiter();
            //Console.ReadLine();
            #endregion
            dynamic pageNumber = 1;
            dynamic otherPage = new object();
            while (input.Equals("y"))
            {
                // 第一页请求
                if (pageNumber == 1) {
                   otherPage = program.GetPageHtmlAsync(program.BaseUrl);
                }
                else
                {
                    if(animationInfos != null)
                        animationInfos.Clear();
                    otherPage = program.GetPageHtmlAsync($"{ program.BaseUrl }page/{ pageNumber }");
                }
                animationInfos = program.ParsePageAllAnimation(otherPage.Result);

                Console.WriteLine("是否加载下一页([y]/n) or 输入编号获取百度云链接");
                var temp = Console.ReadLine();
                input = temp.Equals("y") ? "y" : temp.Equals("Y") ? "y" : "n";
                switch (input)
                {
                    case "y":
                        pageNumber += 1;
                        if (program.AllPages != 0 && pageNumber > program.AllPages)
                        {
                            Console.WriteLine("已经到最后一页了，请输入编号获取动画百度云链接吧 =￣ω￣=");
                            temp = Console.ReadLine();
                        }
                        break;
                    default:
                        break;
                }
                if (animationInfos != null && int.TryParse(temp, out int order))
                {
                    var animationUrl = animationInfos[order].Item1;
                    var downloadPage = program.GetDownloadPageAsync(animationUrl);
                    var downloadInfos = program.ParseDwonloadPage(downloadPage.Result);
                }
            }
            Console.WriteLine("end....");
            Console.ReadLine();
        }

        public async Task<string> GetPageHtmlAsync(string url)
        {
            string res = string.Empty;  //请求结果,请求类型不是图片时有效
            CookieContainer cc = new CookieContainer(); //自动处理Cookie对象
            HttpHelpers helper = new HttpHelpers();  //发起请求对象
            HttpItems items = new HttpItems();  //请求设置对象
            HttpResults hr = new HttpResults();  //请求结果
            items.Method = "GET";
            items.Url = url;   //设置请求地址
            items.Container = cc;  //自动处理Cookie时,每次提交时对cc赋值即可
            hr = await helper.GetHtmlAsync(items);
            res = hr.Html;  //得到请求结果
            return res;
        }

        public List<ValueTuple<string, string>> ParsePageAllAnimation(string html)
        {
            List<ValueTuple<string, string>> animationInfos = new List<ValueTuple<string, string>>();
            var parser = new HtmlParser();
            var document = parser.Parse(html);
            
            var aTagList = document.QuerySelectorAll("a.post-title");
            for (int i = 0; i < aTagList.Length; i++)
            {
                //Console.Write($"{ aTagList[i].GetAttribute("href")}");
                Console.WriteLine($"{ i }) { aTagList[i].GetAttribute("title") }");
                var href = aTagList[i].GetAttribute("href");
                var title = aTagList[i].GetAttribute("title");
                animationInfos.Add((href, title));
            }
            if (AllPages == 0)
            {
                var optionTagCount = document.All.Where(m => m.LocalName.Equals("option")).Count();
                this.AllPages = optionTagCount;
                Console.WriteLine(optionTagCount);
            }
            return animationInfos;
        }

        public async Task<string> GetDownloadPageAsync(string url)
        {
            string result = string.Empty;
            //请求phantomjs 获取下载页面
            string dom = "Tappable-inactive animated fadeIn";
            KeyValuePair<string, string> url2dom = new KeyValuePair<string, string>(url, dom);
            var postData = JsonConvert.SerializeObject(url2dom);
            CookieContainer cc = new CookieContainer();  
            HttpHelpers helper = new HttpHelpers();  
            HttpItems items = new HttpItems();
            HttpResults hr = new HttpResults();
            items.Url = this.PostUrl1;
            items.Method = "POST";
            items.Container = cc;
            items.Postdata = postData;
            items.Timeout = 100000;
            hr = await helper.GetHtmlAsync(items);
            var downloadPageUrl = hr.Html;
            Console.WriteLine($"first => { downloadPageUrl }");
            if(downloadPageUrl.Contains("http"))
            {
                //获取百度云下载地址和分享密码
                //string code1 = "1";
                dom = "Tappable-inactive btn btn-success btn-block"; // 下载链接
                url2dom = new KeyValuePair<string, string>(downloadPageUrl, dom);
                postData = JsonConvert.SerializeObject(url2dom);
                items = new HttpItems
                {
                    Url = this.PostUrl2
                };
                items.Method = "POST";
                items.Container = cc;
                items.Postdata = postData;
                items.Timeout = 1000000;
                hr = await helper.GetHtmlAsync(items);
                result = hr.Html; //返回json数据
                Console.WriteLine($"second => { result }");
            }
            else
            {
                result = downloadPageUrl; //输出错误信息
            }
            return result;
        }

        public List<ValueTuple<string, string>> ParseDwonloadPage(string html)
        {
            List<ValueTuple<string, string>> downloadInfos = new List<ValueTuple<string, string>>();
            var parser = new HtmlParser();
            var document = parser.Parse(html);
            var aTagList = document.QuerySelectorAll("a.post-title");
            for (int i = 0; i < aTagList.Length; i++)
            {
                //Console.Write($"{ aTagList[i].GetAttribute("href")}");
                Console.WriteLine($"{ i }) { aTagList[i].GetAttribute("title") }");
                var href = aTagList[i].GetAttribute("href");
                var title = aTagList[i].GetAttribute("title");
                downloadInfos.Add((href, title));
            }
            return downloadInfos;
        }

#region not need
        //public Tuple<string, string> GetCurrentPageAllAnimations(string url)
        public async Task<string> GetCurrentPageAllAnimationsAsync(string url)
        {
            //Tuple<string, string> result = null;
            var program = new Program();
            //var page = program.HttpPost(program.PostUrl, url);
            //var parser = new HtmlParser();
            //var document = parser.Parse(page);
            ////var aTagList = document.All.Where(m => m.LocalName.Equals("a") && m.ClassName !=null && m.ClassName.Equals("post-title"));
            //var aTagList = document.QuerySelectorAll("a.post-title");
            //foreach (var item in aTagList)
            //{
            //    Console.Write($"{ item.GetAttribute("href")} ***** ");
            //    Console.WriteLine($"{ item.GetAttribute("title")}");
            //}
            //var data = new KeyValuePair<string, string>("data", url);
            var page = await program.HttpPostAsync(program.PostUrl1, url);
            Console.WriteLine("****" + page);
            return page;
        }

        /// <summary>
        /// Http发送Get请求方法
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="postDataStr"></param>
        /// <returns></returns>
        public string HttpGet(string Url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.59 Safari/537.36";
            request.ContentType = "text/html;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }
        /// <summary>
        /// Http发送Post请求方法
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="postDataStr"></param>
        /// <returns></returns>
        public string HttpPost(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.59 Safari/537.36";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postDataStr.Length;
            byte[] byteArray = Encoding.UTF8.GetBytes(postDataStr);
            using (Stream newStream = request.GetRequestStream())
            {
                newStream.Write(byteArray, 0, byteArray.Length);
            }
            Console.WriteLine("------");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Console.WriteLine(response.StatusCode);
            string retString = string.Empty;
            using (Stream stream = response.GetResponseStream())
            {
                Console.WriteLine("1111");
                using (StreamReader sr = new StreamReader(stream, Encoding.UTF8))
                {
                    retString = sr.ReadToEnd();
                    Console.WriteLine(retString);
                }
            }
            Console.WriteLine("OK");
            return retString;
        }

        /// <summary>
        /// 异步请求post（键值对形式,可等待的）
        /// </summary>
        /// <param name="formData">键值对List<KeyValuePair<string, string>> formData = new List<KeyValuePair<string, string>>();formData.Add(new KeyValuePair<string, string>("userid", "29122"));formData.Add(new KeyValuePair<string, string>("umengids", "29122"));</param>
        /// <param name="charset">编码格式</param>
        /// <param name="mediaType">头媒体类型</param>
        /// <returns></returns>
        public async Task<string> HttpPostAsync(string url, string formData, string charset = "UTF-8", string mediaType = "application/x-www-form-urlencoded")
        {
            string tokenUri = url;
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.None };
            var client = new HttpClient(handler);
            var content = new StringContent(formData);
            HttpResponseMessage resp = await client.PostAsync(tokenUri, content);
            resp.EnsureSuccessStatusCode();
            var token = await resp.Content.ReadAsStringAsync();
            Console.WriteLine("-----" + token);
            return token;
        }
# endregion
    }
}
