/*using System;
using System.Collections.Generic;
using System.Text;
using EMCMasterPluginLib.VideoParser;
using EricUtility;
using EricUtility.Networking.Gathering;

namespace RestService.Services.Deprecated.VideoParser.BrokenOrGone
{
    public class StageVuParser : IVideoWebsiteParser
    {
        public ParsedVideoWebsite FindInterestingContent(string res, string url, System.Net.CookieContainer cookies)
        {
            string id = res.Extract("pluginplaying[", "]");
            if (id == null)
                return new ParsedVideoWebsite(url);
            string file = res.Extract("url[" + id + "] = '", "';");
            if (file == null)
                return new ParsedVideoWebsite(url);
            return new ParsedVideoWebsite(url, ParsedVideoWebsite.Extension.Avi, file);
        }

        #region IVideoWebsiteParser Members

        public string BuildURL(string url, string args)
        {
            return "http://" + url + "/video/" + args;
        }

        #endregion IVideoWebsiteParser Members
    }
}
*/