/*using System;
using System.Collections.Generic;
using System.Text;
using EMCMasterPluginLib.VideoParser;
using EricUtility;
using EricUtility.Networking.Gathering;

namespace RestService.Services.Emc.Deprecated.VideoParser.BrokenOrGone
{
    public class MovShareParser : IVideoWebsiteParser
    {
        public ParsedVideoWebsite FindInterestingContent(string content, string url, System.Net.CookieContainer cookies)
        {
            //What a great robot check ! :)
            while (content.Contains("Please click continue to video to prove you're not a robot"))
                content = GatheringUtility.GetPageSource(url, cookies, "wm=1");

            string newurl = content.Extract( "s1.addVariable(\"file\",\"", "\"");
            if (newurl != null)
                return new ParsedVideoWebsite(url, ParsedVideoWebsite.Extension.Flv,newurl);
            else
            {
                string divxurl = content.Extract( "<param name=\"src\" value=\"", "\"");
                if (divxurl != null)
                    return new ParsedVideoWebsite(url, ParsedVideoWebsite.Extension.Avi, divxurl);
                else
                    return new ParsedVideoWebsite(url);
            }
        }

        #region IVideoWebsiteParser Members

        public string BuildURL(string url, string args)
        {
            return "http://www." + url + "/video/" + args;
        }

        #endregion IVideoWebsiteParser Members
    }
}
*/