/*using System;
using System.Collections.Generic;
using System.Text;
using EMCMasterPluginLib.VideoParser;
using EricUtility;

namespace RestService.Services.Emc.Deprecated.VideoParser.BrokenOrGone
{
    public class MegaVideoParser : IVideoWebsiteParser
    {
        public ParsedVideoWebsite FindInterestingContent(string content, string url, System.Net.CookieContainer cookies)
        {
            return new ParsedVideoWebsite(url,ParsedVideoWebsite.Extension.Flv,getVideoUrl(content));
        }
        public string getVideoUrl(string src)
        {
            string un = src.Extract("flashvars.un = \"", "\";");
            string k1 = src.Extract("flashvars.k1 = \"", "\";");
            string k2 = src.Extract("flashvars.k2 = \"", "\";");
            string s = src.Extract("flashvars.s = \"", "\";");
            string binary = "";
            Dictionary<char, string> assoc = new Dictionary<char, string>();
            assoc.Add('0', "0000");
            assoc.Add('1', "0001");
            assoc.Add('2', "0010");
            assoc.Add('3', "0011");
            assoc.Add('4', "0100");
            assoc.Add('5', "0101");
            assoc.Add('6', "0110");
            assoc.Add('7', "0111");
            assoc.Add('8', "1000");
            assoc.Add('9', "1001");
            assoc.Add('a', "1010");
            assoc.Add('b', "1011");
            assoc.Add('c', "1100");
            assoc.Add('d', "1101");
            assoc.Add('e', "1110");
            assoc.Add('f', "1111");
            Dictionary<string, char> theotherway = new Dictionary<string, char>();
            theotherway.Add("0000", '0');
            theotherway.Add("0001", '1');
            theotherway.Add("0010", '2');
            theotherway.Add("0011", '3');
            theotherway.Add("0100", '4');
            theotherway.Add("0101", '5');
            theotherway.Add("0110", '6');
            theotherway.Add("0111", '7');
            theotherway.Add("1000", '8');
            theotherway.Add("1001", '9');
            theotherway.Add("1010", 'a');
            theotherway.Add("1011", 'b');
            theotherway.Add("1100", 'c');
            theotherway.Add("1101", 'd');
            theotherway.Add("1110", 'e');
            theotherway.Add("1111", 'f');
            foreach (char c in un)
                binary += assoc[c];
            List<int> ar2 = new List<int>();
            int ik1 = int.Parse(k1);
            int ik2 = int.Parse(k2);
            for (int i = 0; i < 384; ++i)
            {
                ik1=(ik1*11+77213)%81371;
			    ik2=(ik2*17+92717)%192811;
                ar2.Add((ik1 + ik2) % 128);
            }

            for (int i = 256; i > -1; --i)
            {
			    int mi=Math.Min(ar2[i],i%128);
			    int ma=Math.Max(ar2[i],i%128);
                if (mi != ma)
                    binary = binary.Substring(0, mi) + binary[ma] + binary.Substring(mi + 1, ma - (mi + 1)) + binary[mi] + binary.Substring(ma + 1);
            }
            for (int i = 0; i < 128; ++i)
            {
                int tmp = 0;
                if (binary[i] == '0')
                    tmp = ar2[i + 256] % 2;
                else
                    tmp = 1 - (ar2[i + 256] % 2);
                binary = binary.Substring(0, i) + tmp + binary.Substring(i+1);
            }
            string result = "";
            for (int i = 0; i < binary.Length; i += 4)
            {
                string bin = binary.Substring(i, 4);
                if (theotherway.ContainsKey(bin))
                    result += theotherway[bin];
            }
            return "http://www" + s + ".megavideo.com/files/" + result + "/video.flv";
        }

        #region IVideoWebsiteParser Members

        public string BuildURL(string url, string args)
        {
            throw new NotImplementedException();
        }

        #endregion IVideoWebsiteParser Members
    }
}*/