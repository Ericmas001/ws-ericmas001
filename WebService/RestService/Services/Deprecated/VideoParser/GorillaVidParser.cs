using EricUtility;
using EricUtility.Networking.Gathering;

namespace RestService.Services.Deprecated.VideoParser
{
    public class GorillaVidParser : IVideoParser
    {
        public string BuildURL(string url, string args)
        {
            return "http://" + url + "/" + args;
        }

        public string ParseArgs(string url)
        {
            return url.Substring(url.LastIndexOf('/') + 1);
        }

        public string GetDownloadURL(string url, System.Net.CookieContainer cookies)
        {
            string res = GatheringUtility.GetPageSource(url, cookies);
            while (res.Contains("Please wait while we verify your request"))
            {
                string id = res.Extract("<input type=\"hidden\" name=\"id\" value=\"", "\">");
                string fname = res.Extract("<input type=\"hidden\" name=\"fname\" value=\"", "\">");
                string post = "op=download1&usr_login=&id=" + id + "&fname=" + fname + "&referer=&channel=cna&method_free=tel%3Fchargement+libre+";
                res = GatheringUtility.GetPageSource("http://gorillavid.in/cna/" + id, cookies, "op=download1&usr_login=&id=" + id + "&fname=" + fname + "&referer=&channel=cna&method_free=tel%3Fchargement+libre+");
            }

            return res.Extract("file:\"", "\"");
        }
    }
}