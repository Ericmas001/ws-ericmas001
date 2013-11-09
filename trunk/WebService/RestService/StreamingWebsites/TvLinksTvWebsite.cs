using RestService.StreamingWebsites.Entities;
using EricUtility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RestService.StreamingWebsites
{
    public class TvLinksTvWebsite : ITvWebsite
    {
        public static string NAME { get { return "tv-links.eu"; } }
        public static string URL { get { return "www.tvmuse.eu"; } }

        private async Task<IEnumerable<ListedTvShow>> AvailableShowsAsync(params string[] keywords)
        {
            List<ListedTvShow> availables = new List<ListedTvShow>();
            string src = await new HttpClient().GetStringAsync("http://" + URL + "/tv-shows/all_links");
            string allShows = src.Extract("<div class=\"cfix\">", "<div class=\"a_center pt_1\">");
            string itemp = "<li>";
            int start = allShows.IndexOf(itemp) + itemp.Length;
            while (start >= itemp.Length)
            {
                int end = allShows.IndexOf("</li>", start);
                end = end == -1 ? allShows.Length - 1 : end;
                string item = allShows.Substring(start, end - start);

                ListedTvShow entry = new ListedTvShow();
                entry.Name = item.Extract( "<a href=\"/tv-shows/", "/\" ");
                entry.Title = item.Extract( "<span class=\"c1\">", "<");
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

        public async Task<TvShow> ShowAsync(string name, bool full)
        {
            TvShow show = new TvShow();
            show.Name = name;
            show.IsComplete = true;

            string baseurl = "http://" + URL + "/tv-shows/" + name + "/";
            string src = await new HttpClient().GetStringAsync(baseurl);

            if (src.Contains("Page not found!"))
                return null;
            show.Title = src.Extract("<title>Watch ", " online free on Tv-links</title>");
            if (show.Title == null)
                return null;
            show.Title = show.Title.Trim();
            string n = src.Extract("<meta property=\"og:url\" content=\"http://" + URL + "/tv-shows/", "/\"/>");
            if (n.ToLower() != name.ToLower())
                return null;
            string allSeasons = src.Extract("<!--Episodes-->", "<!--End Episodes-->");
            string seasDeb = "<div class=\"bg_imp biggest bold dark clear";
            int startS = allSeasons.IndexOf(seasDeb) + seasDeb.Length;
            while (startS >= seasDeb.Length)
            {
                int endS = allSeasons.IndexOf("</ul>", startS);
                string itemS = allSeasons.Substring(startS, endS - startS).Trim();
                int no = int.Parse(itemS.Extract( ">Season ", "<") ?? "-1");
                startS = allSeasons.IndexOf(seasDeb, endS) + seasDeb.Length;
                if (no == -1)
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
                    startE = itemS.IndexOf(epDeb, endE) + epDeb.Length;
                    if (itemE.Contains("class=\"list cfix gray\""))
                        continue;

                    string noE = itemE.Extract( "/episode_", "/");
                    episode.Name = name + "-" + "season_" + no + "-episode_" + noE;
                    int id = int.Parse(noE);
                    episode.NoEpisode = int.Parse(noE);
                    episode.NoSeason = no;
                    episode.Title = itemE.Extract( "<span class=\"c2\">", "</span>");

                    string date = itemE.Extract( "<span class=\"c3", "</span>");
                    date = date.Substring(date.IndexOf('>') + 1).Replace("&nbsp;", "");
                    episode.ReleaseDate = DateTime.MinValue;
                    if (!String.IsNullOrWhiteSpace(date))
                        episode.ReleaseDate = DateTime.ParseExact(date, "M/d/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
                    episodes.Add(episode);
                }
                if (episodes.Count == 0)
                    show.Episodes.Remove(no);
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
            string baseurl = "http://" + URL + "/tv-shows/" + epId.Replace("-", "/") + "/video-results/";
            string src = await new HttpClient().GetStringAsync(baseurl);

            if (!src.Contains("<ul id=\"table_search\" class=\"mb_1\">"))
                return null;

            string nfos = src.Extract("<div class=\"cfix mb_1\"> <input type=\"text\" value=\"", "\" ");
            string sep = " - ";
            ep.Title = nfos.Substring(nfos.IndexOf(sep) + sep.Length);
            ep.NoSeason = int.Parse(nfos.Extract( " Season ", " Episode "));
            ep.NoEpisode = int.Parse(nfos.Extract( " Episode ", sep));
            ep.ReleaseDate = DateTime.MinValue;

            string all = src.Extract("<ul id=\"table_search\" class=\"mb_1\">", "</ul>");

            string linkDeb = "<li>";
            int startP = all.IndexOf(linkDeb) + linkDeb.Length;
            while (startP >= linkDeb.Length)
            {
                int endP = all.IndexOf("</li>", startP);
                string itemP = all.Substring(startP, endP - startP).Trim();
                startP = all.IndexOf(linkDeb, endP) + linkDeb.Length;

                if (itemP.Contains("<span class=\"dark\">featured result</span>"))
                    continue;

                string website = itemP.Extract( ">Visit ", "</span>");
                string url = itemP.Extract( "return frameLink('", "');");

                if (!ep.Links.ContainsKey(website))
                    ep.Links.Add(website, new List<string>());
                ep.Links[website].Add(url);
            }
            return ep;
        }

        public async Task<StreamingInfo> StreamAsync(string website, string args)
        {
            return new StreamingInfo() { StreamingURL = "http://" + URL + "/gateway.php?data=" + args, Arguments = args, Website = website, DownloadURL = null };
        }

        public string ShowURL(string showId)
        {
            throw new NotImplementedException();
        }

        public string EpisodeURL(string epId)
        {
            throw new NotImplementedException();
        }
    }
}