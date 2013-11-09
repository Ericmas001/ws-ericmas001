using RestService.Services.Emc.Deprecated.Entities;
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

namespace RestService.Services.Emc.Deprecated
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class TubePlusService
    {
        [WebGet(UriTemplate = "GetPopulars")]
        public string GetPopulars()
        {
            List<TvShowEntry> availables = new List<TvShowEntry>();
            string src = GatheringUtility.GetPageSource("http://www.tubeplus.me/browse/tv-shows/Last/ALL/");
            string allShows = src.Extract("<div id=\"chart_list\">", "</div>");
            string showurl = "<li>";
            int start = allShows.IndexOf(showurl) + showurl.Length;
            while (start >= showurl.Length)
            {
                int end = allShows.IndexOf("</li>", start);
                string item = allShows.Substring(start, end - start);

                TvShowEntry entry = new TvShowEntry();
                entry.ShowName = item.Extract( "/player/", "/");
                entry.ShowTitle = item.Extract( "<img alt=\"", "  - ");
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
            List<string> availables = new List<string>();
            string src = GatheringUtility.GetPageSource("http://www.tubeplus.me/browse/tv-shows/");
            string choices = src.Extract("<div id=\"alphabetic\">", "</div>");
            string showurl = "/browse/tv-shows/All_Genres/";
            int start = choices.IndexOf(showurl) + showurl.Length;
            while (start >= showurl.Length)
            {
                int end = choices.IndexOf("/\">", start);
                string letter = choices.Substring(start, end - start);
                availables.Add(letter);
                start = choices.IndexOf(showurl, end) + showurl.Length;
            }
            string[] items = new string[availables.Count];
            availables.CopyTo(items, 0);
            Array.Sort(items);
            return JsonConvert.SerializeObject(items);
        }

        [WebGet(UriTemplate = "GetLetter/{letter}")]
        public string GetLetter(string letter)
        {
            return getAvailableShows("http://www.tubeplus.me/browse/tv-shows/All_Genres/" + letter + "/");
        }

        private string getAvailableShows(string baseurl)
        {
            List<TvShowEntry> availables = new List<TvShowEntry>();
            string src = GatheringUtility.GetPageSource(baseurl);
            string allShows = src.Extract("<div id=\"list_body\">", "<div id=\"list_footer\">");
            string itemp = "<div class=\"list_item";
            int start = allShows.IndexOf(itemp) + itemp.Length;
            while (start >= itemp.Length)
            {
                int end = allShows.IndexOf("</div><div class=\"list_item", start);
                end = end == -1 ? allShows.Length - 1 : end;
                string item = allShows.Substring(start, end - start);

                TvShowEntry entry = new TvShowEntry();
                entry.ShowName = item.Extract( "/player/", "/");
                entry.ShowTitle = item.Extract( "<b>", "</b>");
                string yearRes = item.Extract( "<div class=\"frelease\">", "-");
                entry.ReleaseYear = yearRes != null && yearRes.Length == 4 ? int.Parse(yearRes) : 0;
                availables.Add(entry);
                start = allShows.IndexOf(itemp, end) + itemp.Length;
            }
            TvShowEntry[] items = new TvShowEntry[availables.Count];
            availables.CopyTo(items, 0);
            Array.Sort(items);
            return JsonConvert.SerializeObject(items);
        }

        [WebGet(UriTemplate = "Search/{keywords}")]
        public string Search(string keywords)
        {
            return getAvailableShows("http://www.tubeplus.me/search/tv-shows/" + keywords.Replace(" ", "_") + "/");
        }

        [WebGet(UriTemplate = "GetShow/{showname}")]
        public string GetShow(string showname)
        {
            TvShowDetailedEntry entry = new TvShowDetailedEntry();

            //Patch laide, temporary convert watchseries to tubeplus:
            showname = showname.ToLower();
            showname = showname.Replace("2_broke_girls", "1964654");
            showname = showname.Replace("21_jump_street", "661838");
            showname = showname.Replace("666_park_avenue", "2106585");
            showname = showname.Replace("ally_mcbeal", "692463");
            showname = showname.Replace("alphas", "1680142");
            showname = showname.Replace("animal_practice", "1979669");
            showname = showname.Replace("arrow", "1978302");//Arrow");//");
            showname = showname.Replace("awkward", "1962544");//Awkward");//");
            showname = showname.Replace("big_bang_theory", "1386155");//The_Big_Bang_Theory");//");
            showname = showname.Replace("blue_bloods", "1619483");//Blue_Bloods");//");
            showname = showname.Replace("body_of_proof", "1897661");//Body_of_Proof");//");
            showname = showname.Replace("breaking_bad", "756428");//Breaking_Bad");//");
            showname = showname.Replace("californication", "765391");//Californication");//");
            showname = showname.Replace("chuck", "786851");//Chuck");//");
            showname = showname.Replace("dexter", "853038");//Dexter");//");
            showname = showname.Replace("doctor_who", "867635");//Doctor_Who");//");
            showname = showname.Replace("dont_trust_the_b----_in_apartment_23", "1974608");//Don%26rsquo%3Bt_Trust_the_B----_in_Apartment_23");//");
            showname = showname.Replace("fringe", "939225");//Fringe");//");
            showname = showname.Replace("go_on", "1979478");//Go_On");//");
            showname = showname.Replace("grimm", "1966487");//Grimm");//");
            showname = showname.Replace("happy_endings", "1916681");//Happy_Endings");//");
            showname = showname.Replace("hart_of_dixie", "1965011");//Hart_of_Dixie");//");
            showname = showname.Replace("hells_kitchen", "984459");//Hell%26rsquo%3Bs_Kitchen");//");
            showname = showname.Replace("homeland", "1756065");//Homeland");//");
            showname = showname.Replace("hotel_hell", "1979818");//Hotel_Hell");//");
            showname = showname.Replace("house_of_lies", "1969778");//House_of_Lies");//");
            showname = showname.Replace("how_i_met_your_mother", "1006325");//How_I_Met_Your_Mother");//");
            showname = showname.Replace("james_mays_toy_stories", "1034534");//James_May%26rsquo%3Bs_Toy_Stories");//");
            showname = showname.Replace("kitchen_nightmares", "1056450");//Kitchen_Nightmares");//");
            showname = showname.Replace("last_resort", "1981037");//Last_Resort");//");
            showname = showname.Replace("mad_men", "1118954");//Mad_Men");//");
            showname = showname.Replace("masterchef_us", "1666690");//Masterchef");//");
            showname = showname.Replace("mike_and_molly", "1686935");//Mike_and_Molly");//");
            showname = showname.Replace("new_girl", "1964186");//New_Girl");//");
            showname = showname.Replace("once_upon_a_time", "1964438");//Once_Upon_a_Time");//");
            showname = showname.Replace("person_of_interest", "1964978");//Person_of_Interest");//");
            showname = showname.Replace("pretty_little_liars", "1245100");//Pretty_Little_Liars");//");
            showname = showname.Replace("private_practice", "1247342");//Private_Practice");//");
            showname = showname.Replace("raising_hope", "1643419");//Raising_Hope");//");
            showname = showname.Replace("revenge", "1883945");//Revenge");//");
            showname = showname.Replace("revolution_(2012)", "1980033");//Revolution");//");
            showname = showname.Replace("rubicon", "1644029");//Rubicon");//");
            showname = showname.Replace("shameless", "1306455");//Shameless");//");
            showname = showname.Replace("shameless_(us)", "1857880");//Shameless");//");
            showname = showname.Replace("shameless_us", "1857880");//Shameless");//");
            showname = showname.Replace("skins", "1317528");//Skins");//");
            showname = showname.Replace("supernatural", "1351897");//Supernatural");//");
            showname = showname.Replace("switched_at_birth", "1961979");//Switched_at_Birth");//");
            showname = showname.Replace("teen_wolf", "1860665");//Teen_Wolf");//");
            showname = showname.Replace("the_borgias", "1396232");//The_Borgias");//");
            showname = showname.Replace("the_killing", "1864307");//The_Killing");//");
            showname = showname.Replace("the_lying_game", "1822774");//The_Lying_Game");//");
            showname = showname.Replace("the_mindy_project", "2106462");//The_Mindy_Project");//");
            showname = showname.Replace("the_mob_doctor", "2106322");//The_Mob_Doctor");//");
            showname = showname.Replace("the_new_normal", "1980449");//The_New_Normal");//");
            showname = showname.Replace("the_shark_tank", "1306749");//Shark_Tank");//");
            showname = showname.Replace("the_vampire_diaries", "1487684");//The_Vampire_Diaries");//");
            showname = showname.Replace("the_walking_dead", "1490194");//The_Walking_Dead");//");
            showname = showname.Replace("touch", "1971247");//Touch");//");
            showname = showname.Replace("true_blood", "1527503");//True_Blood");//");
            showname = showname.Replace("two_and_a_half_men", "1533314");//Two_and_a_Half_Men");//");
            showname = showname.Replace("whitney", "1964330");//Whitney");//");

            int bidon = 0;
            if (!int.TryParse(showname, out bidon))
                return null;

            entry.ShowName = showname;

            string baseurl = "http://www.tubeplus.me/info/" + showname + "/";
            string src = GatheringUtility.GetPageSource(baseurl);

            if (src.Contains("Movie have been removed"))
                return null;

            entry.RssFeed = "";

            string summary = src.Extract("<div id=\"description\">", "<br>");
            entry.Logo = "http://www.tubeplus.me" + summary.Extract( "src=\"", "\">");
            entry.ShowTitle = src.Extract("<h1>", "</h1>");

            entry.ReleaseDate = DateTime.MinValue;
            entry.ReleaseYear = 0;

            entry.Genre = "";
            entry.Status = "";
            entry.Network = "";
            entry.Imdb = "";

            entry.Description = StringUtility.RemoveHTMLTags(summary);

            entry.NbEpisodes = 0;

            string allSeasons = src.Extract("<div id=\"seasons\">", "</div>");
            string seasDeb = "<a id=";
            int startS = allSeasons.IndexOf(seasDeb) + seasDeb.Length;
            int lastS = -1;
            while (startS >= seasDeb.Length)
            {
                TvSeasonEntry season = new TvSeasonEntry();
                int endS = allSeasons.IndexOf("</span>", startS);
                string itemS = allSeasons.Substring(startS, endS - startS).Trim();
                String sInfo = itemS.Extract( "javascript:show_season(", "\")'>");
                string[] eps = sInfo.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                Dictionary<int, DateTime> infos = new Dictionary<int, DateTime>();
                foreach (string ep in eps)
                {
                    string[] parts = ep.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 2)
                    {
                        int id = int.Parse(parts[2]);
                        try
                        {
                            DateTime date = DateTime.ParseExact(parts.Last(), "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None);
                            infos.Add(id, date);
                        }
                        catch { }
                    }
                }

                startS = allSeasons.IndexOf(seasDeb, endS) + seasDeb.Length;
                season.SeasonName = null;
                int no = 0;
                if (!int.TryParse(itemS.Extract( "lseason_", "\""), out no))
                    continue;
                if (no == lastS)
                    season = entry.Seasons.Last();
                season.SeasonNo = no;

                string epDeb = "<a name=";
                int startE = itemS.IndexOf(epDeb) + epDeb.Length;
                while (startE >= epDeb.Length)
                {
                    TvEpisodeEntry episode = new TvEpisodeEntry();
                    int endE = itemS.IndexOf("</a>", startE);
                    string itemE = itemS.Substring(startE, endE - startE).Trim();

                    episode.EpisodeName = itemE.Extract( "id=\"", "\"");
                    episode.EpisodeId = int.Parse(itemE.Extract( "/player/", "/"));
                    episode.EpisodeNo = int.Parse(itemE.Extract( ">Episode ", " -"));

                    episode.EpisodeTitle = itemE.Substring(itemE.LastIndexOf(" - ") + 3);

                    episode.ReleaseDate = DateTime.MinValue;
                    if (infos.ContainsKey(episode.EpisodeId))
                    {
                        episode.ReleaseDate = infos[episode.EpisodeId];
                        if (episode.ReleaseDate <= DateTime.Now)
                            season.Episodes.Insert(0, episode);
                    }
                    startE = itemS.IndexOf(epDeb, endE) + epDeb.Length;
                }
                season.NbEpisodes = season.Episodes.Count;
                if (no != lastS && season.NbEpisodes > 0)
                    entry.Seasons.Insert(0, season);
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
        //[WebGet(UriTemplate = "GetEpisode/{epname}")]
        //public string GetEpisode(string epname)
        //{
        //    epname = epname.Replace(';', ':');
        //    TvEpisodeDetailedEntry entry = new TvEpisodeDetailedEntry();
        //    entry.EpisodeName = epname;
        //    entry.EpisodeId = int.Parse(epname.Substring(epname.LastIndexOf('-') + 1));
        //    entry.EpisodeNo = int.Parse(StringUtility.Extract(epname.Substring(epname.LastIndexOf('_')), "_e", "-"));
        //    string season = epname.Remove(epname.LastIndexOf('_'));
        //    entry.SeasonNo = int.Parse(season.Substring(season.LastIndexOf('_') + 2));
        //    //entry.Links = Links(entry.EpisodeId);
        //    entry.Links = new List<TvWebsiteEntry>();

        //    string baseurl = "http://watchseries.li/episode/" + epname + ".html";
        //    string src = StringUtility.Extract(GatheringUtility.GetPageSource(baseurl),"<div class=\"fullwrap\">","</div>");

        //    string showtitle = src.Extract("<a href=\"http://watchseries.li/serie/", "</a>");
        //    entry.ShowTitle = showtitle.Substring(showtitle.LastIndexOf('>') + 1);

        //    entry.EpisodeTitle = src.Extract("  - ", "</span>").Trim();

        //    string airdate = src.Extract("<b>Air Date:</b> ", "<br />");
        //    DateTime d = DateTime.MinValue;
        //    DateTime.TryParseExact(airdate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out d);
        //    entry.ReleaseDate = d;

        //    string desc = src.Extract("<p><b>Summary:</b>", "<br /><br />");
        //    entry.Description = StringUtility.RemoveHTMLTags(desc.Replace("[+]more", "")).Trim();

        //    return JsonConvert.SerializeObject(entry);
        //}
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