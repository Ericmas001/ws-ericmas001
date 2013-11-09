/*using System;
using System.Collections.Generic;
using System.Text;
using EMCMasterPluginLib.VideoParser;
using EricUtility;
using EricUtility.Networking.Gathering;

namespace RestService.Services.Deprecated.VideoParser.BrokenOrGone
{
    public class PlayHDParser : IVideoWebsiteParser
    {
        public ParsedVideoWebsite FindInterestingContent(string res, string url, System.Net.CookieContainer cookies)
        {
            string baseURL = "http://www.playhd.org/";
            string title = "Dedicated Servers Power hosting - ";
            int ititle = res.IndexOf(title) + title.Length;
            int ititleF = res.IndexOf("</", ititle);
            string id = res.Substring(ititle, ititleF - ititle);
            while (res.Contains("Would you like to continue and watch this video?"))
                res = GatheringUtility.GetPageSource(baseURL + id, cookies, "agree=Yes%2C%20let%20m%20watch");

            string deb = "var movieURL  = \"";
            int ideb = res.IndexOf(deb) + deb.Length;
            if (ideb < deb.Length)
                return new ParsedVideoWebsite(url);
            int ifin = res.IndexOf("\"", ideb);
            string newurl = res.Substring(ideb, ifin - ideb).Replace(":81", "");
            return new ParsedVideoWebsite(url,ParsedVideoWebsite.Extension.Flv, newurl);
        }

        #region IVideoWebsiteParser Members

        public string BuildURL(string url, string args)
        {
            throw new NotImplementedException();
        }

        #endregion IVideoWebsiteParser Members
    }
}
*/