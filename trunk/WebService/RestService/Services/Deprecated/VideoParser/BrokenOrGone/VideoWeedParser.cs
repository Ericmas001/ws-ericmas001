/*using System;
using System.Collections.Generic;
using System.Text;
using EMCMasterPluginLib.VideoParser;
using EricUtility;

namespace RestService.Services.Deprecated.VideoParser.BrokenOrGone
{
    public class VideoWeedParser
    {
        public ParsedVideoWebsite FindInterestingContent(string res, string url, System.Net.CookieContainer cookies)
        {
            string file = res.Extract("flashvars.file=\"", "\";");
            if (file == null)
                return new ParsedVideoWebsite(url);

            return new ParsedVideoWebsite(url, ParsedVideoWebsite.Extension.Flv, file);
        }
    }
}
*/