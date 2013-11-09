/*using System;
using System.Collections.Generic;
using System.Text;
using EMCMasterPluginLib.VideoParser;
using EricUtility;

namespace RestService.Services.Deprecated.VideoParser.BrokenOrGone
{
    public class YoutubeParser
    {
        public ParsedVideoWebsite FindInterestingContent(string src, string url, System.Net.CookieContainer cookies)
        {
            string IF = "<link rel=\"canonical\" href=\"/watch?v=";
            int indexIF = src.LastIndexOf(IF) + IF.Length;
            if (indexIF < IF.Length)
                return new ParsedVideoWebsite(url);
            int indexU = src.IndexOf("\"", indexIF);
            if (indexU < 0)
                return new ParsedVideoWebsite(url);
            string videoID = src.Substring(indexIF, indexU - indexIF);

            string buffer = src;
            int start = 0, end = 0;
            string startTag = "\"fmt_url_map\": ";
            string endTag = "\",";
            start = buffer.IndexOf(startTag, StringComparison.CurrentCultureIgnoreCase);
            end = buffer.IndexOf(endTag, start, StringComparison.CurrentCultureIgnoreCase);
            string str = buffer.Substring(start + startTag.Length, end - (start + startTag.Length));
            start = str.LastIndexOf("http");
            str = str.Substring(start);
            str = str.Replace("\\/","/");
            str += "&video.flv";

            //string str2 = WebStringUtility.EncodeUrl(str);
            //return new ParsedVideoWebsite(url, ParsedVideoWebsite.Extension.Flv, str, str2);
            return new ParsedVideoWebsite(url, ParsedVideoWebsite.Extension.Flv, str);
        }
    }
}
*/