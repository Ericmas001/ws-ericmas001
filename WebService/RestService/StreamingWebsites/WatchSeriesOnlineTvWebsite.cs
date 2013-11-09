using RestService.StreamingWebsites.Entities;
using EricUtility;
using EricUtility.Networking.Gathering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RestService.StreamingWebsites
{
    public class WatchSeriesOnlineTvWebsite : ITvWebsite
    {
        public static string NAME { get { return "watchseries-online.eu"; } }
        public static string URL { get { return "www.watchseries-online.eu"; } }

        private async Task<IEnumerable<ListedTvShow>> AvailableShowsAsync(params string[] keywords)
        {
            List<ListedTvShow> availables = new List<ListedTvShow>();
            string src = await new HttpClient().GetStringAsync("http://" + URL + "/2005/07/index.html");
            string allShows = src.Extract("<div id=\"ddmcc_container\">", "<div class=\"cleared\">");
            string itemp = "<li>";
            int start = allShows.IndexOf(itemp) + itemp.Length;
            while (start >= itemp.Length)
            {
                int end = allShows.IndexOf("</li>", start);
                end = end == -1 ? allShows.Length - 1 : end;
                string item = allShows.Substring(start, end - start);

                ListedTvShow entry = new ListedTvShow();
                entry.Name = item.Extract("http://" + URL + "/category/", "\">");
                entry.Title = item.Extract(entry.Name + "\">", "</a>");
                if (keywords.Count(x => entry.Title.ToLower().Contains(x.ToLower()) || entry.Name.ToLower().Contains(x.ToLower())) > 0)
                    availables.Add(entry);
                start = allShows.IndexOf(itemp, end) + itemp.Length;
            }
            ListedTvShow[] items = availables.ToArray();
            Array.Sort(items);
            return items;
        }

        public async Task<IEnumerable<ListedTvShow>> SearchAsync(string keywords)
        {
            try
            {
                return await AvailableShowsAsync(keywords.Split(' '));
            }
            catch { return null; }
        }

        public async Task<IEnumerable<ListedTvShow>> StartsWithAsync(string letter)
        {
            try
            {
                char debut = letter.ToLower()[0];
                return (await AvailableShowsAsync(debut.ToString())).Where(x => x.Title.ToLower()[0] == debut || ((debut < 'a' || debut > 'z') && (x.Title.ToLower()[0] < 'a' || x.Title.ToLower()[0] > 'z')));
            }
            catch { return null; }
        }

        public void ExtractTitleAndNos(ListedEpisode episode, string title, string showname)
        {
            string[] tparts = title.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] sparts = showname.Split(' ');
            int i = 1;
            episode.NoSeason = episode.NoEpisode = -1;
            while ((i < tparts.Length) && episode.NoSeason == -1 && episode.NoEpisode == -1)
            {
                string nfo = tparts[i].Trim();
                try
                {
                    if (nfo.StartsWith("-"))
                    {
                        string mid = "&#215;";
                        if (nfo.Contains("x"))
                            mid = "x";
                        if (nfo.Contains("X"))
                            mid = "X";
                        if (nfo.Contains("×"))
                            mid = "×";
                        episode.NoSeason = int.Parse(nfo.Extract("-", mid));
                        episode.NoEpisode = int.Parse(nfo.Extract(mid, "-"));
                    }
                    else if (nfo.StartsWith("S"))
                    {
                        episode.NoSeason = int.Parse(nfo.Extract("S", "E"));
                        episode.NoEpisode = int.Parse(nfo.Substring(nfo.IndexOf('E') + 1));
                    }
                    else
                        episode.NoSeason = episode.NoEpisode = -1;
                }
                catch
                {
                    episode.NoSeason = episode.NoEpisode = -1;
                }
                ++i;
            }
            episode.Title = String.Join(" ", tparts.Skip(i));
        }

        public List<ListedEpisode> GetEpisodesOnPage(string showname, string src)
        {
            List<ListedEpisode> eps = new List<ListedEpisode>();
            string all = src.Extract("<h2 class=\"pagetitle\">", "<div class=\"sidebar2\">");
            string postDeb = "<div class=\"Post-inner article\">";
            int startP = all.IndexOf(postDeb) + postDeb.Length;
            while (startP >= postDeb.Length)
            {
                int endP = all.IndexOf("<div class=\"PostContent\">", startP);
                string itemP = all.Substring(startP, endP - startP).Trim();
                startP = all.IndexOf(postDeb, endP) + postDeb.Length;

                ListedEpisode episode = new ListedEpisode();
                episode.Name = itemP.Extract("<a href=\"http://" + URL + "/", ".html").Replace("/", "_");

                string title = StringUtility.RemoveHTMLTags(itemP.Extract("<span class=\"PostHeader\">", "</a>")).Replace("\n", "");
                ExtractTitleAndNos(episode, title, showname);

                string date = itemP.Extract("alt=\"PostDateIcon\" />", " | ").Replace("\n", "").Replace("th,", ",").Replace("st,", ",").Replace("nd,", ",").Replace("rd,", ",");
                episode.ReleaseDate = DateTime.ParseExact(date, "MMMM d, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);

                eps.Add(episode);
            }
            return eps;
        }

        public async Task<TvShow> ShowAsync(string name, bool full)
        {
            TvShow show = new TvShow();
            show.Name = name;

            string baseurl = "http://" + URL + "/category/" + name;
            string src = await new HttpClient().GetStringAsync(baseurl);

            if (src.Contains("Page not found"))
                return null;
            show.Title = src.Extract("Episodes Available for: &#8216;", "&#8217;");
            if (show.Title == null)
                return null;
            List<ListedEpisode> eps = GetEpisodesOnPage(show.Title, src);
            show.IsComplete = true;
            if (src.Contains("<ol class=\"wp-paginate\">"))
            {
                if (full)
                {
                    string navig = src.Extract("<ol class=\"wp-paginate\">", "class=\"next\">");
                    navig = navig.Substring(navig.LastIndexOf("class='page'>"));
                    int nbPages = int.Parse(navig.Extract(">", "</"));
                    for (int i = 2; i <= nbPages; ++i)
                        eps.AddRange(GetEpisodesOnPage(show.Title, await new HttpClient().GetStringAsync(baseurl + "/page/" + i)));
                }
                else
                    show.IsComplete = false;
            }

            Dictionary<int, IEnumerable<ListedEpisode>> les = new Dictionary<int, IEnumerable<ListedEpisode>>();
            foreach (ListedEpisode le in eps)
            {
                if (!les.ContainsKey(le.NoSeason))
                    les.Add(le.NoSeason, new List<ListedEpisode>());
                List<ListedEpisode> episodes = (List<ListedEpisode>)les[le.NoSeason];
                episodes.Add(le);
            }
            foreach (int no in les.Keys)
            {
                ListedEpisode[] epis = les[no].ToArray();
                Array.Sort(epis);
                show.Episodes.Add(no, epis);
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
            string baseurl = "http://" + URL + "/" + epId.Replace("_", "/") + ".html";
            string src = await new HttpClient().GetStringAsync(baseurl);

            if (src.Contains("Page not found"))
                return null;

            string showname = src.Extract("rel=\"category tag\">", "</a>");
            string title = StringUtility.RemoveHTMLTags(src.Extract("<span class=\"PostHeader\">", "</span>")).Replace("\n", "").Trim();
            ExtractTitleAndNos(ep, title, showname);

            string date = src.Extract("alt=\"PostDateIcon\"/>", " | ").Replace("\n", "").Replace("th", "").Replace("st", "").Replace("nd", "").Replace("rd", "").Replace("Augu ", "August ");
            ep.ReleaseDate = DateTime.ParseExact(date.Trim(), "MMMM d, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);

            string all = src.Extract("<table class=\"postlinks", "</table>");

            string linkDeb = "<tr><td class=";
            int startP = all.IndexOf(linkDeb) + linkDeb.Length;
            while (startP >= linkDeb.Length)
            {
                int endP = all.IndexOf("</td></tr>", startP);
                string itemP = all.Substring(startP, endP - startP).Trim();
                startP = all.IndexOf(linkDeb, endP) + linkDeb.Length;

                string nfo = itemP.Extract("<a target=\"_blank\" id=\"hovered\"", "</td>");
                string website = nfo.Extract(">", "<");
                int sp = website.IndexOf(" ");
                if (sp > 1)
                    website = website.Remove(sp);
                string url = nfo.Extract("href=\"http://" + URL + "/watch.php?l=", "\">").Replace("/", "_");

                if (!ep.Links.ContainsKey(website))
                    ep.Links.Add(website, new List<string>());
                ep.Links[website].Add(url);
            }
            return ep;
        }

        public async Task<StreamingInfo> StreamAsync(string website, string args)
        {
            string baseurl = "http://" + URL + "/watch.php?l=" + args;
            HttpClient m_Client = new HttpClient(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, baseurl);

            request.Headers.Host = URL;

            //Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.9));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));

            //Accept-Language: fr,fr-fr;q=0.8,en-us;q=0.5,en;q=0.3
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("fr"));
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("fr-fr", 0.8));
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-us", 0.5));
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("en", 0.5));

            //Accept-Encoding: gzip, deflate
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            //Content-Type: application/x-www-form-urlencoded; charset=UTF-8
            //Content-Type: text/html; charset="UTF-8"
            request.Content = new StringContent("l=" + args);
            request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("text/html");

            //User-Agent: Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0
            request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");

            HttpResponseMessage result = await m_Client.SendAsync(request);
            result.EnsureSuccessStatusCode();
            string src = await result.Content.ReadAsStringAsync();

            string url = src.Extract("<a id=\"redirectButton\" class=\"myButton\" href='", "'>");
            string durl = null;

            return new StreamingInfo() { StreamingURL = url, Arguments = args, Website = website, DownloadURL = durl };
        }

        public string ShowURL(string name)
        {
            return "http://" + URL + "/category/" + name;
        }

        public string EpisodeURL(string epId)
        {
            return "http://" + URL + "/" + epId.Replace("_", "/") + ".html";
        }
    }
}