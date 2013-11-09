using EricUtility;
using EricUtility.Networking.Gathering;

namespace RestService.Services.Emc.Deprecated.VideoParser
{
    public class PutLockerSockShareParser : IVideoParser
    {
        public string BuildURL(string url, string args)
        {
            return "http://www." + url + "/file/" + args;
        }

        public string ParseArgs(string url)
        {
            return url.Substring(url.LastIndexOf('/') + 1);
        }

        public string GetDownloadURL(string url, System.Net.CookieContainer cookies)
        {
            string res = GatheringUtility.GetPageSource(url, cookies);
            string beginurl = "http://www.sockshare.com";
            if (url.Contains("www.putlocker.com"))
                beginurl = "http://www.putlocker.com";

            while (res.Contains("Continue as Free User"))
            {
                string u = GatheringUtility.GetPageUrl(url, cookies, "", "application/x-www-form-urlencoded");
                string hash = res.Extract("<input type=\"hidden\" value=\"", "\"");
                res = GatheringUtility.GetPageSource(url, cookies, "hash=" + hash + "&confirm=Continue+as+Free+User");
            }
            if (res.Contains("This file doesn't exist"))
                return null;
            string rssU = beginurl + res.Extract("playlist: '", "',");
            string info = GatheringUtility.GetPageSource(rssU, cookies);
            return info.Extract( "<media:content url=\"", "\"");
        }
    }
}