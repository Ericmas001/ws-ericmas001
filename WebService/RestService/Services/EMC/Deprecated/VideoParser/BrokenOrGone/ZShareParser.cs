/*using System;
using System.Collections.Generic;
using System.Text;
using EMCMasterPluginLib.VideoParser;
using EricUtility;

namespace RestService.Services.Emc.Deprecated.VideoParser.BrokenOrGone
{
    public class ZShareParser
    {
        public ParsedVideoWebsite FindInterestingContent(string src, string url, System.Net.CookieContainer cookies)
        {
            string playerURL = src.Extract("<iframe src=\"", "\"");
            if (playerURL == null)
                return new ParsedVideoWebsite(url);
            return new ParsedVideoWebsite(url, ParsedVideoWebsite.Extension.Flv, playerURL);

            / * test 1.0
            string res = GatheringUtility.GetPageSource(playerURL, cookies);
            string movie = "http://www.zshare.net/" + res.Extract("<param name=\"movie\" value=\"", "?");
            string vars = res.Extract("<param name=\"flashvars\" value=\"", "\"");
            return new FlashViewer(movie, playerURL, vars);* /

            / * Test 2.0
            string id = playerURL.Extract( "&H=", "&");
            string res = GatheringUtility.GetPageSource("http://www.zshare.net/download/" + id, cookies, "download=1");
            string encoded = StringUtility.Extract("var link_enc=new Array('h','t','t','p',':','/','/','d','l','0','4','1','.','z','s','h','a','r','e','.','n','e','t','/','d','o','w','n','l','o','a','d','/','8','e','a','4','a','0','b','1','9','5','2','0','e','8','f','1','f','6','6','7','e','c','3','0','2','f','c','4','2','2','c','3','/','1','2','8','8','9','9','8','2','6','3','/','7','1','3','8','8','7','3','6','/','T','w','o','.','a','n','d','.','a','.','H','a','l','f','.','M','e','n','.','S','0','7','E','1','3','%','2','0','t','v','-','d','o','m','e','.','n','e','t','.','f','l','v');		link = '';for(i=0;i<link_enc.length;i++){link+=link_enc[i];}", "var link_enc=new Array(", ");");
            string videoUrl = encoded.Replace(",","").Replace("'","");
            return new FlashPlayerViewer(FlashPlayerType.MovShare, videoUrl, url); * /
        }
    }
}*/