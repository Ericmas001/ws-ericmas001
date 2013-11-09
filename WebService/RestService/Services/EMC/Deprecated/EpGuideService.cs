using RestService.Services.Emc.Deprecated.Entities;
using EricUtility;
using EricUtility.Networking.Gathering;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace RestService.Services.Emc.Deprecated
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class EpGuideService
    {
        public static string EPGUIDE_USELESS { get { return "(a Titles & Air Dates Guide)"; } }

        public static List<SearchResultEntry> GoogleSearch(string searchString, int maxresults)
        {
            string pattern = "http://ajax.googleapis.com/ajax/services/search/web?v=1.0&rsz=large&safe=active&q={0}&start={1}";
            List<SearchResultEntry> resultsList = new List<SearchResultEntry>();
            if ((maxresults % 8) > 0)
                maxresults += (8 - (maxresults % 8));
            for (int p = 0; p < maxresults; p += 8)
            {
                string res = GatheringUtility.GetPageSource(string.Format(pattern, searchString, p));
                int i = -1;
                while ((i = res.IndexOf("GsearchResultClass", i + 1)) >= 0)
                {
                    string url = res.Extract("\"url\":\"", "\"", i);
                    string title = WebUtility.HtmlDecode(res.Extract("\"titleNoFormatting\":\"", "\"", i).Replace("\\u0026", "&"));

                    //string title = Uri.UnescapeDataString(res.Extract("\"titleNoFormatting\":\"", "\"", i).Replace("\\u0026", "&"));
                    //string title = WebStringUtility.DecodeString(res.Extract("\"titleNoFormatting\":\"", "\"", i).Replace("\\u0026", "&"));
                    string content = res.Extract("\"content\":\"", "\"", i);
                    resultsList.Add(new SearchResultEntry(url, title, content, SearchEngineType.Google));
                }
            }
            return resultsList;
        }

        public static List<SearchResultEntry> GoogleSearch(string searchString)
        {
            return GoogleSearch(searchString, 8);
        }

        [WebGet(UriTemplate = "Search/{id}")]
        public string Search(string id)
        {
            List<SearchResultEntry> entries = GoogleSearch(System.Uri.EscapeDataString("allintitle: site:epguides.com \"" + EPGUIDE_USELESS + "\" " + id));
            Dictionary<string, string> results = new Dictionary<string, string>();

            foreach (SearchResultEntry e in entries)
            {
                string title = e.Title.Replace(EPGUIDE_USELESS, "").Trim();
                string name = e.Url.Replace("http://epguides.com/", "").Replace("/", "");
                if (!name.Contains(".shtml"))
                    results.Add(name, title);
            }

            return JsonConvert.SerializeObject(results);
        }

        [WebGet(UriTemplate = "GetShow/{id}")]
        public string GetShow(string id)
        {
            string baseurl = "http://epguides.com/" + id + "/";

            CookieContainer cookies = new CookieContainer();
            string srcTvRage = GatheringUtility.GetPageSource(baseurl, cookies);
            string srcTv = GatheringUtility.GetPageSource(baseurl + "./", cookies, "list=tv.com");

            EpGuideEntry entry = new EpGuideEntry();

            entry.TvRageId = srcTvRage.Extract( "http://www.tvrage.com/shows/id-", "/");
            entry.TvId = srcTv.Extract( "http://www.tv.com/show/", "/");
            entry.FutonCriticId = srcTv.Extract( "http://thefutoncritic.com/showatch.aspx?id=", "&#38");
            entry.ImdbId = srcTv.Extract( "http://us.imdb.com/title/", "\"");
            entry.ShareTvId = srcTv.Extract( "http://sharetv.org/shows/", "\"");
            entry.TvClubId = srcTv.Extract( "http://www.avclub.com/tvclub/tvshow/", "\"");
            entry.TvGuideId = srcTv.Extract( "http://www.tvguide.com/detail/tv-show.aspx?tvobjectid=", "\"");
            entry.WikiId = srcTv.Extract( "http://en.wikipedia.org/wiki/", "\"");
            entry.ClickerId = srcTv.Extract( "http://www.clicker.com/tv/", "\"");

            return JsonConvert.SerializeObject(entry);
        }
    }
}