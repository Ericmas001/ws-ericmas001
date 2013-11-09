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
    public class PrimeWireWebsite : ITvWebsite, IMovieWebsite
    {
        public static string NAME { get { return "primewire.ag"; } }
        public static string URL { get { return "www.primewire.ag"; } }

        private async Task<IEnumerable<T>> AvailableAsync<T>(CookieContainer cookies, string baseurl) where T : IListedStreamingItem, new()
        {
            List<T> availables = new List<T>();
            string src = await new HttpClient(new HttpClientHandler() { CookieContainer = cookies }).GetStringAsync(baseurl);
            int max = 1;

            if (src.Contains("<div class=\"pagination\">"))
            {
                string pages = src.Extract("<div class=\"pagination\">", "</div>");
                pages = pages.Substring(pages.LastIndexOf("<a href="));
                max = int.Parse(pages.Extract("&page=", "\""));
            }

            for (int i = 0; i < max; ++i)
            {
                if (i > 0)
                    src = await new HttpClient(new HttpClientHandler() { CookieContainer = cookies }).GetStringAsync(baseurl + "&page=" + (i + 1));
                string allShows = "<div class=\"index_item index_item_ie\">" + src.Extract("<div class=\"index_item index_item_ie\">", "<div class=\"col2\">");
                string itemp = "<div class=\"index_item index_item_ie\">";
                int start = allShows.IndexOf(itemp) + itemp.Length;
                while (start >= itemp.Length)
                {
                    int end = allShows.IndexOf("</div></div>", start);
                    end = end == -1 ? allShows.Length - 1 : end;
                    string item = allShows.Substring(start, end - start);

                    T entry = new T();
                    entry.Name = (typeof(T) == typeof(ListedTvShow) ? "tv-" : "") + item.Extract("<a href=\"/" + (typeof(T) == typeof(ListedTvShow) ? "watch-" : ""), "\"");
                    entry.Title = item.Extract(" title=\"Watch ", "\"").Trim();
                    availables.Add(entry);
                    start = allShows.IndexOf(itemp, end) + itemp.Length;
                }
            }
            T[] items = availables.ToArray();
            Array.Sort(items);
            return items;
        }

        async Task<IEnumerable<ListedTvShow>> ITvWebsite.SearchAsync(string keywords)
        {
            try
            {
                CookieContainer cookies = new CookieContainer();

                string res = await new HttpClient(new HttpClientHandler() { CookieContainer = cookies }).GetStringAsync("http://" + URL + "/");
                string key = res.Extract("<input type=\"hidden\" name=\"key\" value=\"", "\"");
                return await AvailableAsync<ListedTvShow>(cookies, "http://" + URL + "/index.php?search_section=2&search_keywords=" + keywords.Replace(" ", "+") + "&key=" + key);
            }
            catch { return null; }
        }

        Task<IEnumerable<ListedTvShow>> ITvWebsite.StartsWithAsync(string letter)
        {
            return null;
        }


        async Task<IEnumerable<ListedMovie>> IMovieWebsite.SearchAsync(string keywords)
        {
            try
            {
                CookieContainer cookies = new CookieContainer();

                string res = await new HttpClient(new HttpClientHandler() { CookieContainer = cookies }).GetStringAsync("http://" + URL + "/");
                string key = res.Extract("<input type=\"hidden\" name=\"key\" value=\"", "\"");
                return await AvailableAsync<ListedMovie>(cookies, "http://" + URL + "/index.php?search_section=1&search_keywords=" + keywords.Replace(" ", "+") + "&key=" + key);
            }
            catch { return null; }
        }

        Task<IEnumerable<ListedMovie>> IMovieWebsite.StartsWithAsync(string letter)
        {
            return null;
        }

        public async Task<TvShow> ShowAsync(string name, bool full)
        {
            CookieContainer cookies = new CookieContainer();
            TvShow show = new TvShow();
            show.Name = name;
            show.IsComplete = true;
            string baseurl = "http://" + URL + "/" + name;
            string src = await new HttpClient(new HttpClientHandler() { CookieContainer = cookies }).GetStringAsync(baseurl);

            show.Title = src.Extract("<a href=\"/" + name + "\">", "</a>");
            if (String.IsNullOrEmpty(show.Title))
                return null;

            string allSeasons = src.Extract(show.Title + " Links", "<div class=\"download_link\">");
            string seasDeb = "<h2><a href=\"";
            int startS = allSeasons.IndexOf(seasDeb) + seasDeb.Length;
            while (startS >= seasDeb.Length)
            {
                int endS = allSeasons.IndexOf("<h2><a href=\"", startS);
                if (endS == -1)
                    endS = allSeasons.IndexOf("<div class=\"clearer\"></div>", startS);
                string itemS = allSeasons.Substring(startS, endS - startS).Trim();

                startS = allSeasons.IndexOf(seasDeb, endS) + seasDeb.Length;
                int no = 0;
                if (!int.TryParse(itemS.Extract( "/season-", "\""), out no))
                    continue;
                if (!show.Episodes.ContainsKey(no))
                    show.Episodes.Add(no, new List<ListedEpisode>());
                List<ListedEpisode> episodes = (List<ListedEpisode>)show.Episodes[no];

                string epDeb = "<div class=\"tv_episode_item\">";
                int startE = itemS.IndexOf(epDeb) + epDeb.Length;
                while (startE >= epDeb.Length)
                {
                    ListedEpisode episode = new ListedEpisode();
                    int endE = itemS.IndexOf("</div>", startE);
                    string itemE = itemS.Substring(startE, endE - startE).Trim();

                    episode.Name = HttpUtility.UrlEncode(itemE.Extract( "<a href=\"/", "\"")).Replace("%", ".");
                    episode.NoEpisode = int.Parse(itemE.Extract( "-episode-", "\""));
                    episode.NoSeason = no;

                    episode.ReleaseDate = DateTime.MinValue;
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
            string baseurl = "http://" + URL + "/" + epIdClean;
            string src = await new HttpClient().GetStringAsync(baseurl);

            if (src.Contains("Doesn't look like there are any links"))
                return null;

            string title = src.Extract("<td width=\"70\"><strong>Title:</strong></td>", "</tr>");
            ep.Title = title.Extract( "<td>", "</td>");
            ep.NoSeason = int.Parse(epIdClean.Extract( "/season-", "-"));
            ep.NoEpisode = int.Parse(epIdClean.Substring(epIdClean.IndexOf("-episode-") + 9));
            string date = src.Extract("<td width=\"70\"><strong>Air Date:</strong></td>", "</tr>");
            ep.ReleaseDate = DateTime.ParseExact(date.Extract( "<td>", "</td>"), "MMMM d, yyyy", CultureInfo.InvariantCulture);

            string showName = epIdClean.Remove(epIdClean.IndexOf("/"));
            string showTitle = src.Extract("<a href=\"/" + showName + "\">", "</a>");

            string all = src.Extract(showTitle + " Links", "<div class=\"download_link\">");

            string linkDeb = "<span class=quality_dvd>";
            int startP = all.IndexOf(linkDeb) + linkDeb.Length;
            while (startP >= linkDeb.Length)
            {
                int endP = all.IndexOf("</table>", startP);
                string itemP = all.Substring(startP, endP - startP).Trim();
                startP = all.IndexOf(linkDeb, endP) + linkDeb.Length;

                string website = itemP.Extract( "<script type=\"text/javascript\">document.writeln('", "');</script>");
                string url = itemP.Extract( "<a href=\"/external.php?", "\"");

                if (!ep.Links.ContainsKey(website))
                    ep.Links.Add(website, new List<string>());
                ep.Links[website].Add(HttpUtility.UrlEncode(url).Replace("%", "."));
            }
            return ep;
        }

        public async Task<Movie> MovieAsync(string movieId)
        {
            CookieContainer cookies = new CookieContainer();
            Movie mov = new Movie();
            mov.Name = movieId;
            string baseurl = "http://" + URL + "/" + movieId + "/";
            string src = await new HttpClient(new HttpClientHandler() { CookieContainer = cookies }).GetStringAsync(baseurl);

            if (src.Contains("Doesn't look like there are any links"))
                return null;

            mov.Title = src.Extract("<a href=\"/" + movieId + "\">", "</a>");

            string all = src.Extract(mov.Title + " Links", "<div class=\"download_link\">");

            string linkDeb = "<span class=quality_dvd>";
            int startP = all.IndexOf(linkDeb) + linkDeb.Length;
            while (startP >= linkDeb.Length)
            {
                int endP = all.IndexOf("</table>", startP);
                string itemP = all.Substring(startP, endP - startP).Trim();
                startP = all.IndexOf(linkDeb, endP) + linkDeb.Length;

                string website = itemP.Extract("<script type=\"text/javascript\">document.writeln('", "');</script>");
                string url = itemP.Extract("<a href=\"/external.php?", "\"");

                if (!mov.Links.ContainsKey(website))
                    mov.Links.Add(website, new List<string>());
                mov.Links[website].Add(HttpUtility.UrlEncode(url).Replace("%", "."));
            }
            return mov;
        }

        public async Task<StreamingInfo> StreamAsync(string website, string args)
        {
            string url = "http://" + URL + "/external.php?" + HttpUtility.UrlDecode(args.Replace(".", "%"));
            string srcUrl = await new HttpClient().GetStringAsync(url);
            if (srcUrl.Contains("frame_header.php?hello=&title="))
                url = srcUrl.Extract( "</frameset><noframes>", "</noframes>");
            else
                url = GatheringUtility.GetPageUrl(url, new CookieContainer());
            return url == null ? null : new StreamingInfo() { StreamingURL = url, Arguments = args, Website = website, DownloadURL = null };
        }

        public string ShowURL(string name)
        {
            return "http://" + URL + "/" + name;
        }

        public string EpisodeURL(string epId)
        {
            return "http://" + URL + "/" + HttpUtility.UrlDecode(epId.Replace(".", "%"));
        }

        public string MovieURL(string movieId)
        {
            return "http://" + URL + "/" + movieId + "/";
        }
    }
}