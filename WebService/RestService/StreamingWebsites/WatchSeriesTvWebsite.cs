using RestService.StreamingWebsites.Entities;
using EricUtility;
using EricUtility.Networking.Gathering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace RestService.StreamingWebsites
{
    public class WatchSeriesTvWebsite : ITvWebsite
    {
        public static string NAME { get { return "watchseries.to"; } }
        public static string URL { get { return "watchseries.to"; } }

        public async Task<IEnumerable<ListedTvShow>> SearchAsync(string keywords)
        {
            try
            {
                CookieContainer cookies = new CookieContainer();
                List<ListedTvShow> availables = new List<ListedTvShow>();
                bool endReached = false;
                string baseurl = "http://" + URL + "/search/" + keywords;
                string url = baseurl;
                do
                {
                    string src = await new HttpClient(new HttpClientHandler() { CookieContainer = cookies }).GetStringAsync(url);
                    string allShows = src.Extract("<div class=\"fullwrap\">", "<!-- end fullwrap -->");
                    if (allShows == null)
                        return null;
                    string td = "<td valign=\"top\">";
                    int start = allShows.IndexOf(td) + td.Length;
                    while (start >= td.Length)
                    {
                        int end = allShows.IndexOf("</tr>", start);
                        string item = allShows.Substring(start, end - start).Trim();

                        ListedTvShow entry = new ListedTvShow();
                        entry.Name = item.Extract("<a href=\"/serie/", "\"");
                        entry.Title = item.Extract("><strong>", "</strong>");
                        availables.Add(entry);
                        start = allShows.IndexOf(td, end) + td.Length;
                    }
                    int lio = src.LastIndexOf("\">Next Search Page</a>");
                    endReached = lio == -1;
                    if (!endReached)
                    {
                        int lioPage = src.LastIndexOf("/page/",lio);
                        url = baseurl + src.Substring(lioPage, lio - lioPage);
                    }
                } while (!endReached);
                ListedTvShow[] items = availables.ToArray();
                Array.Sort(items);
                return items;
            }
            catch { return null; }
        }

        public async Task<IEnumerable<ListedTvShow>> StartsWithAsync(string letter)
        {
            try
            {
                CookieContainer cookies = new CookieContainer();
                List<ListedTvShow> availables = new List<ListedTvShow>();
                string src = await new HttpClient(new HttpClientHandler() { CookieContainer = cookies }).GetStringAsync("http://www." + URL + "/letters/" + letter);
                string allShows = src.Extract("<div class=\"episode-summary\">", "</div>");
                if (allShows == null)
                    return null;
                string td = "<td valign=\"top\">";
                string showurl = "http://" + URL + "/serie/";
                int start = allShows.IndexOf(td) + td.Length;
                while (start >= showurl.Length)
                {
                    int end = allShows.IndexOf("</li>", start);
                    string item = allShows.Substring(start, end - start);

                    ListedTvShow entry = new ListedTvShow();
                    entry.Name = item.Remove(item.IndexOf("\"")).Replace(":", ";2pts;");
                    entry.Title = item.Extract("\">", "<");
                    availables.Add(entry);
                    start = allShows.IndexOf(showurl, end) + showurl.Length;
                }
                ListedTvShow[] items = availables.ToArray();
                Array.Sort(items);
                return items;
            }
            catch { return null; }
        }

        public async Task<TvShow> ShowAsync(string name, bool full)
        {
            CookieContainer cookies = new CookieContainer();
            TvShow show = new TvShow();
            name = name.Replace(";2pts;", ":");
            show.Name = name;
            show.IsComplete = true;
            string baseurl = "http://" + URL + "/serie/" + name;
            string src = await new HttpClient(new HttpClientHandler() { CookieContainer = cookies }).GetStringAsync(baseurl);

            if (src.Contains("Um, Where did the page go?"))
                return null;

            string summary = src.Extract("<div class=\"show-summary\">", "</div>");
            string imageAndTitle = summary.Extract("<img src=\"", "</a>");
            show.Title = imageAndTitle.Extract("Watch Series - ", "\"");

            string allSeasons = src.Extract("<div id=\"left\">", "<!-- end of left -->") + src.Extract("<div id=\"right\">", "<!-- end right -->");
            string seasDeb = "<h2 class=\"lists\">";
            int startS = allSeasons.IndexOf(seasDeb) + seasDeb.Length;
            while (startS >= seasDeb.Length)
            {
                int endS = allSeasons.IndexOf("</ul>", startS);
                string itemS = allSeasons.Substring(startS, endS - startS).Trim();
                startS = allSeasons.IndexOf(seasDeb, endS) + seasDeb.Length;

                int no = 0;
                if (!int.TryParse(itemS.Extract("/season-", "/"), out no))
                    continue;

                if (!show.Episodes.ContainsKey(no))
                    show.Episodes.Add(no, new List<ListedEpisode>());
                List<ListedEpisode> episodes = (List<ListedEpisode>)show.Episodes[no];

                string epDeb = "<li>";
                int startE = itemS.IndexOf(epDeb) + epDeb.Length;
                while (startE >= epDeb.Length)
                {
                    ListedEpisode episode = new ListedEpisode();
                    int endE = itemS.IndexOf("</li>", startE);
                    string itemE = itemS.Substring(startE, endE - startE).Trim();

                    episode.Name = itemE.Extract("href=\"/episode/", ".html");
                    episode.NoEpisode = int.Parse(itemE.Extract("_e", ".html"));
                    episode.NoSeason = no;
                    episode.Title = itemE.Extract("&nbsp;&nbsp;&nbsp;", "</span>");
                    
                    string eRDate = itemE.Extract("<span class=\"epnum\">", "</span>");
                    DateTime d = DateTime.MinValue;
                    DateTime.TryParseExact(eRDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out d);
                    episode.ReleaseDate = d;

                    episodes.Add(episode);
                    startE = itemS.IndexOf(epDeb, endE) + epDeb.Length;
                }
            }

            ListedEpisode lastEp = show.Episodes.Last().Value.Last();
            show.NoLastEpisode = lastEp.NoEpisode;
            show.NoLastSeason = lastEp.NoSeason;
            return show;
        }

        public async Task<Episode> EpisodeAsync(string epId)
        {
            Episode ep = new Episode();
            ep.Name = epId;
            string epIdClean = HttpUtility.UrlDecode(epId.Replace(".", "%"));
            string baseurl = "http://" + URL + "/episode/" + epId + ".html";
            string src = await new HttpClient().GetStringAsync(baseurl);

            ep.Title = src.Extract(" - ", " - Watch Series</title>").Trim();
            ep.NoSeason = int.Parse(epId.Extract("_s","_"));
            ep.NoEpisode = int.Parse(epId.Substring(epId.IndexOf("_e") + 2));

            string airdate = src.Extract("<strong>Aired: </strong>", "</p>");
            DateTime d = DateTime.MinValue;
            DateTime.TryParseExact(airdate, "MMM dd, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out d);
            ep.ReleaseDate = d;

            string all = src.Extract("<div id=\"linktable\">", "<div class=\"nextprev\">");

            string linkDeb = "<tr class=\"download_link";
            int startP = all.IndexOf(linkDeb) + linkDeb.Length;
            while (startP >= linkDeb.Length)
            {
                int endP = all.IndexOf("</tr>", startP);
                string itemP = all.Substring(startP, endP - startP).Trim();
                startP = all.IndexOf(linkDeb, endP) + linkDeb.Length;

                string website = itemP.Extract("_", "\"");
                string url = itemP.Extract("open/cale/", ".html\"");

                if (!ep.Links.ContainsKey(website))
                    ep.Links.Add(website, new List<string>());
                ep.Links[website].Add(HttpUtility.UrlEncode(url).Replace("%", "."));
            }
            return ep;
        }

        public async Task<StreamingInfo> StreamAsync(string website, string args)
        {
            string url = "http://www." + URL + "/open/cale/" + args + ".html";
            string srcUrl = await new HttpClient().GetStringAsync(url);
            string urlBlock = srcUrl.Extract("<div id=\"popup2-middle\" style=\"border: 0px;margin-top: 50px; height: auto;\">","</div>");
            if( urlBlock != null )
                url = urlBlock.Extract( "<a href=\"", "\"");
            return url == null ? null : new StreamingInfo() { StreamingURL = url, Arguments = args, Website = website, DownloadURL = null };
        }

        public string ShowURL(string name)
        {
            return "http://" + URL + "/serie/" + name;
        }

        public string EpisodeURL(string epId)
        {
            return "http://" + URL + "/episode/" + epId + ".html";
        }
    }
}