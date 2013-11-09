/*using System;
using System.Collections.Generic;
using System.Text;
using EMCMasterPluginLib.VideoParser;
using EricUtility;
using EricUtility.Networking.Gathering;
using System.Net;

namespace RestService.Services.Deprecated.VideoParser.BrokenOrGone
{
    public class WiseVidParser : IVideoWebsiteParser
    {
        public ParsedVideoWebsite FindInterestingContent(string res, string url, System.Net.CookieContainer cookies)
        {
            for (int i = 0; i < arrChrs.Length; i++)
            {
                reversegetFChars[arrChrs[i]] = i;
            }

            string s1 = "http://www.wisevid.com/play?v=";
            int i1 = res.IndexOf(s1);
            int i2 = res.IndexOf("'", i1);
            int i3 = res.IndexOf("\"", i1);
            string idV = res.Substring(i1 + s1.Length, Math.Min(i3, i2) - (i1 + s1.Length));
            url = res.Substring(i1, Math.Min(i3, i2) - i1);
            int count = 0;
            while (++count < 10 && (i1 = res.IndexOf("Yes, let me watch")) > 0)
                res = gateway(res, idV, cookies);
            if (i1 > 0)
                return new ParsedVideoWebsite(url);
            string varDeb = "'+getF('";
            i1 = res.IndexOf(varDeb) + varDeb.Length;
            i2 = res.IndexOf("')+'", i1);
            string file = getF(res.Substring(i1, i2 - i1));
            return new ParsedVideoWebsite(url, ParsedVideoWebsite.Extension.Flv, file);
        }

        Dictionary<char, int> reversegetFChars = new Dictionary<char, int>();
        private int END_OF_INPUT = -1;
        private char[] arrChrs = new char[]{
            'A','B','C','D','E','F','G','H',
            'I','J','K','L','M','N','O','P',
            'Q','R','S','T','U','V','W','X',
            'Y','Z','a','b','c','d','e','f',
            'g','h','i','j','k','l','m','n',
            'o','p','q','r','s','t','u','v',
            'w','x','y','z','0','1','2','3',
            '4','5','6','7','8','9','+','/'
        };
        private string getFStr;
        private int getFCount;
        void setgetFStr(string str)
        {
            getFStr = str;
            getFCount = 0;
        }
        int readgetF()
        {
            if (getFStr == null)
                return END_OF_INPUT;
            if (getFCount >= getFStr.Length)
                return END_OF_INPUT;
            int c = getFStr[getFCount] & 0xff;
            getFCount++;
            return c;
        }
        int readReversegetF()
        {
            if (getFStr == null)
                return END_OF_INPUT;
            while (true)
            {
                if (getFCount >= getFStr.Length) return END_OF_INPUT;
                char nextCharacter = getFStr[getFCount];
                getFCount++;
                if (reversegetFChars.ContainsKey(nextCharacter))
                {
                    return reversegetFChars[nextCharacter];
                }
                if (nextCharacter == 'A') return 0;
            }
        }
        string ntos(int n)
        {
            return ((char)n).ToString();
        }
        string getF(string str)
        {
            setgetFStr(str);
            var result = "";
            var inBuffer = new int[4];
            var done = false;
            while (!done && (inBuffer[0] = readReversegetF()) != END_OF_INPUT
                && (inBuffer[1] = readReversegetF()) != END_OF_INPUT)
            {
                inBuffer[2] = readReversegetF();
                inBuffer[3] = readReversegetF();
                result += ntos((((inBuffer[0] << 2) & 0xff) | inBuffer[1] >> 4));
                if (inBuffer[2] != END_OF_INPUT)
                {
                    result += ntos((((inBuffer[1] << 4) & 0xff) | inBuffer[2] >> 2));
                    if (inBuffer[3] != END_OF_INPUT)
                    {
                        result += ntos((((inBuffer[2] << 6) & 0xff) | inBuffer[3]));
                    }
                    else
                    {
                        done = true;
                    }
                }
                else
                {
                    done = true;
                }
            }
            return result;
        }
        private string gateway(string src, string id, CookieContainer cookies)
        {
            return GatheringUtility.GetPageSource("http://www.wisevid.com/play?v=" + id, cookies, "a=1&no1=Yes,%20let%20me%20watch&v=" + id);
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