using RestService.Services.Deprecated.Entities;
using EricUtility;
using EricUtility.Networking.Gathering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace RestService.Services.Deprecated
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class WatchSeriesService
    {
        private const string RANDOM_EPISODE_URL = "http://watchseries.li/episode/big_bang_theory_s1_e9-8171.html";

        [WebGet(UriTemplate = "GetPopulars")]
        public string GetPopulars()
        {
            List<TvShowEntry> availables = new List<TvShowEntry>();
            string src = GatheringUtility.GetPageSource("http://watchseries.li/");
            string allShows = src.Extract("<div class=\"div-home-inside-left\">", "</div>");
            string showurl = "http://watchseries.li/serie/";
            int start = allShows.IndexOf(showurl) + showurl.Length;
            while (start >= showurl.Length)
            {
                int end = allShows.IndexOf("<br/>", start);
                string item = allShows.Substring(start, end - start);

                TvShowEntry entry = new TvShowEntry();
                entry.ShowName = item.Remove(item.IndexOf("\""));

                //entry.ShowTitle = item.Extract( "\">", "<");
                entry.ShowTitle = new CultureInfo("en-US", false).TextInfo.ToTitleCase(entry.ShowName.Replace("_", " "));
                entry.ReleaseYear = 0;
                availables.Add(entry);
                start = allShows.IndexOf(showurl, end) + showurl.Length;
            }
            TvShowEntry[] items = new TvShowEntry[availables.Count];
            availables.CopyTo(items, 0);

            //Array.Sort(items);
            return JsonConvert.SerializeObject(items);
        }

        [WebGet(UriTemplate = "AvailableLetters")]
        public string AvailableLetters()
        {
            return getAvailableItems("http://watchseries.li/letters/");
        }

        [WebGet(UriTemplate = "AvailableGenres")]
        public string AvailableGenres()
        {
            return getAvailableItems("http://watchseries.li/genres/");
        }

        private string getAvailableItems(string baseurl)
        {
            List<string> availables = new List<string>();
            string src = GatheringUtility.GetPageSource(baseurl);
            string choices = src.Extract("<ul class=\"pagination\">", "</ul>");
            int start = choices.IndexOf(baseurl) + baseurl.Length;
            while (start >= baseurl.Length)
            {
                int end = choices.IndexOf("\"", start);
                string letter = choices.Substring(start, end - start);
                availables.Add(letter);
                start = choices.IndexOf(baseurl, end) + baseurl.Length;
            }
            string[] items = new string[availables.Count];
            availables.CopyTo(items, 0);
            Array.Sort(items);
            return JsonConvert.SerializeObject(items);
        }

        [WebGet(UriTemplate = "GetLetter/{letter}")]
        public string GetLetter(string letter)
        {
            return getAvailableShows("http://watchseries.li/letters/" + letter);
        }

        [WebGet(UriTemplate = "GetGenre/{genre}")]
        public string GetGenre(string genre)
        {
            return getAvailableShows("http://watchseries.li/genres/" + genre);
        }

        private string getAvailableShows(string baseurl)
        {
            List<TvShowEntry> availables = new List<TvShowEntry>();
            string src = GatheringUtility.GetPageSource(baseurl);
            string allShows = src.Extract("<div id=\"left\" >", "</ul>") + src.Extract("<div id=\"right\">", "</ul>");
            string showurl = "http://watchseries.li/serie/";
            int start = allShows.IndexOf(showurl) + showurl.Length;
            while (start >= showurl.Length)
            {
                int end = allShows.IndexOf("</li>", start);
                string item = allShows.Substring(start, end - start);

                TvShowEntry entry = new TvShowEntry();
                entry.ShowName = item.Remove(item.IndexOf("\""));
                entry.ShowTitle = item.Extract( "\">", "<");
                entry.ReleaseYear = int.Parse(item.Extract( "<span class=\"epnum\">", "</span>"));
                availables.Add(entry);
                start = allShows.IndexOf(showurl, end) + showurl.Length;
            }
            TvShowEntry[] items = new TvShowEntry[availables.Count];
            availables.CopyTo(items, 0);
            Array.Sort(items);
            return JsonConvert.SerializeObject(items);
        }

        [WebGet(UriTemplate = "Search/{keywords}")]
        public string Search(string keywords)
        {
            List<TvShowEntry> availables = new List<TvShowEntry>();
            string src = GatheringUtility.GetPageSource("http://watchseries.li/search/" + keywords);
            string allShows = src.Extract("<div class=\"episode-summary\">", "</div>");
            if (allShows == null)
                return null;
            string td = "<td valign=\"top\">";
            string showurl = "http://watchseries.li/serie/";
            int start = allShows.IndexOf(td) + td.Length;
            while (start >= td.Length)
            {
                int end = allShows.IndexOf("</td></tr>", start);
                string item = allShows.Substring(start, end - start).Trim();

                TvShowEntry entry = new TvShowEntry();
                entry.ShowName = item.Extract( showurl, "\"");
                entry.ShowTitle = item.Extract( "><b>", "</b>");
                string year = entry.ShowTitle.Substring(entry.ShowTitle.LastIndexOf("("));
                entry.ReleaseYear = int.Parse(year.Extract( "(", ")"));
                entry.ShowTitle = entry.ShowTitle.Remove(entry.ShowTitle.LastIndexOf("(") - 1);
                availables.Add(entry);
                start = allShows.IndexOf(td, end) + td.Length;
            }
            TvShowEntry[] items = new TvShowEntry[availables.Count];
            availables.CopyTo(items, 0);
            Array.Sort(items);
            return JsonConvert.SerializeObject(items);
        }

        [WebGet(UriTemplate = "GetShow/{showname}")]
        public string GetShow(string showname)
        {
            showname = showname.Replace(';', ':');
            TvShowDetailedEntry entry = new TvShowDetailedEntry();
            entry.ShowName = showname;

            string baseurl = "http://watchseries.li/serie/" + showname;
            string src = GatheringUtility.GetPageSource(baseurl);

            if (src.Contains("Um, Where did the page go?"))
                return null;

            entry.RssFeed = src.Extract(" <a class=\"rss-title\" href=\"../rss/", ".xml");

            string summary = src.Extract("<div class=\"show-summary\">", "</div>");

            string imageAndTitle = summary.Extract( "<img src=\"", "</a>");
            entry.Logo = imageAndTitle.Remove(imageAndTitle.IndexOf("\""));
            entry.ShowTitle = imageAndTitle.Extract( "Watch Series - ", "\"");

            string rDate = summary.Extract( "<b>Release Date :</b> ", "<br />");
            entry.ReleaseDate = DateTime.ParseExact(rDate, "dd , MMMM yyyy", CultureInfo.InvariantCulture);
            entry.ReleaseYear = entry.ReleaseDate.Year;

            entry.Genre = summary.Extract( "http://watchseries.li/genres/", "\"");

            entry.Status = summary.Extract( "<b>Status : </b>", "<");
            if (entry.Status != null) entry.Status = entry.Status.Trim();

            entry.Network = summary.Extract( "<b>Network : </b>", "<");
            if (entry.Network != null) entry.Network = entry.Network.Trim();

            entry.Imdb = summary.Extract( "http://www.imdb.com/title/", "/");

            entry.Description = summary.Extract( "<b>Description :</b> ", "<br />");
            if (entry.Description != null) entry.Description = StringUtility.RemoveHTMLTags(entry.Description).Replace("[+]more", "").Trim();

            entry.NbEpisodes = int.Parse(summary.Extract( "<b>No. of episodes :</b> ", " episode"));

            string allSeasons = src.Extract("<div id=\"left\">", "<!-- end of left -->") + src.Extract("<div id=\"right\">", "<!-- end right -->");
            string seasDeb = "<h2 class=\"lists\">";
            int startS = allSeasons.IndexOf(seasDeb) + seasDeb.Length;
            int lastS = -1;
            while (startS >= seasDeb.Length)
            {
                TvSeasonEntry season = new TvSeasonEntry();
                int endS = allSeasons.IndexOf("</ul>", startS);
                string itemS = allSeasons.Substring(startS, endS - startS).Trim();
                startS = allSeasons.IndexOf(seasDeb, endS) + seasDeb.Length;

                season.NbEpisodes = int.Parse(itemS.Extract( "  (", " episodes)"));
                season.SeasonName = null;
                int no = 0;
                if (!int.TryParse(itemS.Extract( "watchseries.li/season-", "/"), out no))
                    continue;
                if (no == lastS)
                    season = entry.Seasons.Last();
                season.SeasonNo = no;

                string epDeb = "<li>";
                int startE = itemS.IndexOf(epDeb) + epDeb.Length;
                while (startE >= epDeb.Length)
                {
                    TvEpisodeEntry episode = new TvEpisodeEntry();
                    int endE = itemS.IndexOf("</li>", startE);
                    string itemE = itemS.Substring(startE, endE - startE).Trim();

                    episode.EpisodeName = itemE.Extract( "href=\"../episode/", ".html");
                    episode.EpisodeId = int.Parse(episode.EpisodeName.Substring(episode.EpisodeName.LastIndexOf("-") + 1));
                    episode.EpisodeNo = int.Parse(itemE.Extract( ">Episode ", "&nbsp;"));
                    episode.EpisodeTitle = itemE.Extract( "&nbsp;&nbsp;&nbsp;", "</span>");

                    string eRDate = itemE.Extract( "<span class=\"epnum\">", "</span>");
                    DateTime d = DateTime.MinValue;
                    DateTime.TryParseExact(eRDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out d);
                    episode.ReleaseDate = d;

                    season.Episodes.Add(episode);
                    startE = itemS.IndexOf(epDeb, endE) + epDeb.Length;
                }
                if (no != lastS)
                    entry.Seasons.Add(season);
                lastS = no;
            }

            return JsonConvert.SerializeObject(entry);
        }

        //public List<TvWebsiteEntry> Links(int id)
        //{
        //    CookieContainer cookies = new CookieContainer();
        //    // Build cookies
        //    GatheringUtility.GetPageSource(RANDOM_EPISODE_URL, cookies);
        //    Dictionary<string, List<int>> all = new Dictionary<string, List<int>>();
        //    string baseurl = "http://watchseries.li/getlinks.php?q=" + id + "&domain=all";
        //    string src = GatheringUtility.GetPageSource(baseurl, cookies);
        //    string deb = "<div class=\"linewrap\" >";
        //    int start = src.IndexOf(deb) + deb.Length;
        //    while (start >= deb.Length)
        //    {
        //        int end = src.IndexOf("<br class=\"clear\">", start);
        //        string item = src.Substring(start, end - start).Trim();

        //        string site = item.Extract( "<div class=\"site\">", "</div>").Trim();
        //        if (site.Contains(" "))
        //            site = site.Remove(site.IndexOf(" "));
        //        int wid = int.Parse(item.Extract( "../open/cale/", "/idepisod/").Trim());

        //        if (!all.ContainsKey(site))
        //            all.Add(site, new List<int>());
        //        all[site].Add(wid);

        //        start = src.IndexOf(deb, end) + deb.Length;
        //    }
        //    string[] items = new string[all.Count];
        //    all.Keys.CopyTo(items, 0);
        //    Array.Sort(items);
        //    List<TvWebsiteEntry> websites = new List<TvWebsiteEntry>();
        //    foreach (string s in items)
        //        websites.Add(new TvWebsiteEntry(){ Name = s, LinkIDs = all[s]});
        //    return websites;
        //}
        //[WebGet(UriTemplate = "GetLinks/{epid}")]
        //public string GetLinks(string epid)
        //{
        //    return JsonConvert.SerializeObject(Links(int.Parse(epid)));
        //}
        [WebGet(UriTemplate = "GetEpisode/{epname}")]
        public string GetEpisode(string epname)
        {
            epname = epname.Replace(';', ':');
            TvEpisodeDetailedEntry entry = new TvEpisodeDetailedEntry();
            entry.EpisodeName = epname;
            entry.EpisodeId = int.Parse(epname.Substring(epname.LastIndexOf('-') + 1));
            entry.EpisodeNo = int.Parse(StringUtility.Extract(epname.Substring(epname.LastIndexOf('_')), "_e", "-"));
            string season = epname.Remove(epname.LastIndexOf('_'));
            entry.SeasonNo = int.Parse(season.Substring(season.LastIndexOf('_') + 2));

            //entry.Links = Links(entry.EpisodeId);
            entry.Links = new List<TvWebsiteEntry>();

            string baseurl = "http://watchseries.li/episode/" + epname + ".html";
            string src = StringUtility.Extract(GatheringUtility.GetPageSource(baseurl), "<div class=\"fullwrap\">", "</div>");

            string showtitle = src.Extract("<a href=\"http://watchseries.li/serie/", "</a>");
            entry.ShowTitle = showtitle.Substring(showtitle.LastIndexOf('>') + 1);

            entry.EpisodeTitle = src.Extract("  - ", "</span>").Trim();

            string airdate = src.Extract("<b>Air Date:</b> ", "<br />");
            DateTime d = DateTime.MinValue;
            DateTime.TryParseExact(airdate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out d);
            entry.ReleaseDate = d;

            string desc = src.Extract("<p><b>Summary:</b>", "<br /><br />");
            entry.Description = StringUtility.RemoveHTMLTags(desc.Replace("[+]more", "")).Trim();

            return JsonConvert.SerializeObject(entry);
        }

        //[WebGet(UriTemplate = "GetUrl/{linkid}")]
        //public string GetUrl(string linkid)
        //{
        //    CookieContainer cookies = new CookieContainer();
        //    // Build cookies
        //    GatheringUtility.GetPageSource(RANDOM_EPISODE_URL, cookies);

        //    //Get link
        //    string gateway = "http://watchseries.li/gateway.php?link=";
        //    string cale = GatheringUtility.GetPageSource("http://watchseries.li/open/cale/" + linkid + "/idepisod/42.html", cookies);
        //    string token = cale.Extract( gateway, "\"");

        //    //Get RealURL
        //    string rurl = GatheringUtility.GetPageUrl(gateway + token,cookies,"","application/x-www-form-urlencoded");

        //    string trimmed = rurl.Replace("http://", "").Replace("https://", "").Replace("www.", "");
        //    string websiteName = trimmed.Remove(trimmed.IndexOf('/'));
        //    bool isSupported = VideoParsingService.Parsers.ContainsKey(websiteName);
        //    string websiteArgs = null;
        //    if (isSupported)
        //        websiteArgs = VideoParsingService.Parsers[websiteName].ParseArgs(trimmed);
        //    return JsonConvert.SerializeObject(new { url = rurl, supported = isSupported, website = websiteName, args = websiteArgs });
        //}
    }
}