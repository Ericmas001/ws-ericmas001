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
    public class TubePlusWebsite : ITvWebsite, IMovieWebsite
    {
        public static string NAME { get { return "tubeplus.me"; } }
        public static string URL { get { return "www.tubeplus.me"; } }

        private async Task<IEnumerable<T>> AvailableAsync<T>(string baseurl) where T : IListedStreamingItem, new()
        {
            List<T> availables = new List<T>();
            string src = await new HttpClient().GetStringAsync(baseurl);
            string allShows = src.Extract("<div id=\"list_body\">", "<div id=\"list_footer\">");
            string itemp = "<div class=\"list_item";
            int start = allShows.IndexOf(itemp) + itemp.Length;
            while (start >= itemp.Length)
            {
                int end = allShows.IndexOf("</div><div class=\"list_item", start);
                end = end == -1 ? allShows.Length - 1 : end;
                string item = allShows.Substring(start, end - start);

                T entry = new T();
                entry.Name = item.Extract("/player/", "/");
                entry.Title = item.Extract("<b>", "</b>");
                availables.Add(entry);
                start = allShows.IndexOf(itemp, end) + itemp.Length;
            }
            T[] items = availables.ToArray();
            Array.Sort(items);
            return items;
        }

        async Task<IEnumerable<ListedTvShow>> ITvWebsite.SearchAsync(string keywords)
        {
            try
            {
                return await AvailableAsync<ListedTvShow>("http://" + URL + "/search/tv-shows/" + keywords.Replace(" ", "_") + "/");
            }
            catch { return null; }
        }

        async Task<IEnumerable<ListedTvShow>> ITvWebsite.StartsWithAsync(string letter)
        {
            try
            {
                return await AvailableAsync<ListedTvShow>("http://" + URL + "/browse/tv-shows/All_Genres/" + letter + "/");
            }
            catch { return null; }
        }

        async Task<IEnumerable<ListedMovie>> IMovieWebsite.SearchAsync(string keywords)
        {
            try
            {
                return await AvailableAsync<ListedMovie>("http://" + URL + "/search/movies/" + keywords.Replace(" ", "_") + "/");
            }
            catch { return null; }
        }

        async Task<IEnumerable<ListedMovie>> IMovieWebsite.StartsWithAsync(string letter)
        {
            try
            {
                return await AvailableAsync<ListedMovie>("http://" + URL + "/browse/movies/All_Genres/" + letter + "/");
            }
            catch { return null; }
        }

        public async Task<TvShow> ShowAsync(string name, bool full)
        {
            int bidon = 0;
            if (!int.TryParse(name, out bidon))
                return null;
            TvShow show = new TvShow();
            show.Name = name;
            show.IsComplete = true;
            string baseurl = "http://" + URL + "/info/" + name + "/";
            string src = await new HttpClient().GetStringAsync(baseurl);

            if (src.Contains("Movie have been removed"))
                return null;
            show.Title = src.Extract("<title>TUBE+ - \"", "\"");
            if (show.Title == null)
                return null;
            string allSeasons = src.Extract("<div id=\"seasons\">", "</div>");
            string seasDeb = "<a id=";
            int startS = allSeasons.IndexOf(seasDeb) + seasDeb.Length;
            while (startS >= seasDeb.Length)
            {
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
                int no = 0;
                if (!int.TryParse(itemS.Extract( "lseason_", "\""), out no))
                    continue;
                if (!show.Episodes.ContainsKey(no))
                    show.Episodes.Add(no, new List<ListedEpisode>());
                List<ListedEpisode> episodes = (List<ListedEpisode>)show.Episodes[no];

                string epDeb = "<a name=";
                int startE = itemS.IndexOf(epDeb) + epDeb.Length;
                while (startE >= epDeb.Length)
                {
                    ListedEpisode episode = new ListedEpisode();
                    int endE = itemS.IndexOf("</a>", startE);
                    string itemE = itemS.Substring(startE, endE - startE).Trim();

                    episode.Name = itemE.Extract( "/player/", "/");
                    int id = int.Parse(episode.Name);
                    episode.NoEpisode = int.Parse(itemE.Extract( ">Episode ", " -"));
                    episode.NoSeason = no;
                    episode.Title = itemE.Substring(itemE.LastIndexOf(" - ") + 3);

                    episode.ReleaseDate = DateTime.MinValue;
                    if (infos.ContainsKey(id))
                    {
                        episode.ReleaseDate = infos[id];
                        if (episode.ReleaseDate <= DateTime.Now)
                        {
                            episodes.Insert(0, episode);
                            if (show.NoLastSeason == 0)
                            {
                                show.NoLastSeason = episode.NoSeason;
                                show.NoLastEpisode = episode.NoEpisode;
                            }
                        }
                    }
                    startE = itemS.IndexOf(epDeb, endE) + epDeb.Length;
                }
            }
            return show;
        }

        public async Task<Episode> EpisodeAsync(string epId)
        {
            Episode ep = new Episode();
            ep.Name = epId;
            string baseurl = "http://" + URL + "/player/" + epId + "/";
            string src = await new HttpClient().GetStringAsync(baseurl);

            if (src.Contains("Movie have been removed"))
                return null;

            string nfos = src.Extract("<a class=\"none\" href=\"#\">", "</a>");
            string sep = " - ";
            ep.Title = nfos.Extract( sep, sep);
            string nfoNos = nfos.Extract( "  ", sep);
            ep.NoSeason = int.Parse(nfoNos.Extract( "S", "E"));
            ep.NoEpisode = int.Parse(nfoNos.Substring(nfoNos.IndexOf("E") + 1));
            ep.ReleaseDate = DateTime.MinValue;

            string all = src.Extract("<ul id=\"links_list\" class=\"wonline\">", "</ul>");

            string linkDeb = "<li ";
            int startP = all.IndexOf(linkDeb) + linkDeb.Length;
            while (startP >= linkDeb.Length)
            {
                int endP = all.IndexOf("</li>", startP);
                string itemP = all.Substring(startP, endP - startP).Trim();
                startP = all.IndexOf(linkDeb, endP) + linkDeb.Length;

                string website = itemP.Extract( "<span>Host: </span>", "</div>").Replace("\r", "").Replace("\n", "").Replace("\t", "").Trim();
                string url = itemP.Extract( "onclick=\"visited('", "');");

                if (!ep.Links.ContainsKey(website))
                    ep.Links.Add(website, new List<string>());
                ep.Links[website].Add(url);
            }
            return ep;
        }

        public async Task<Movie> MovieAsync(string movieId)
        {
            Movie mov = new Movie();
            mov.Name = movieId;
            string baseurl = "http://" + URL + "/player/" + movieId + "/";
            string src = await new HttpClient().GetStringAsync(baseurl);

            if (src.Contains("Movie have been removed"))
                return null;

            string nfos = StringUtility.RemoveBBCodeTags(src.Extract("<a class=\"none\" href=\"#\">", "</a>"));
            mov.Title = nfos.Extract(" ", " - ");

            string all = src.Extract("<ul id=\"links_list\" class=\"wonline\">", "</ul>");

            string linkDeb = "<li ";
            int startP = all.IndexOf(linkDeb) + linkDeb.Length;
            while (startP >= linkDeb.Length)
            {
                int endP = all.IndexOf("</li>", startP);
                string itemP = all.Substring(startP, endP - startP).Trim();
                startP = all.IndexOf(linkDeb, endP) + linkDeb.Length;

                string website = itemP.Extract("<span>Host: </span>", "</div>").Replace("\r", "").Replace("\n", "").Replace("\t", "").Trim();
                string url = itemP.Extract("onclick=\"visited('", "');");

                if (!mov.Links.ContainsKey(website))
                    mov.Links.Add(website, new List<string>());
                mov.Links[website].Add(url);
            }
            return mov;
        }

        public async Task<StreamingInfo> StreamAsync(string website, string args)
        {
            string url = TubePlusWebsite.ObtainURL(website, args);
            return url == null ? null : new StreamingInfo() { StreamingURL = url, Arguments = args, Website = website, DownloadURL = null };
        }

        public string ShowURL(string name)
        {
            return "http://" + URL + "/info/" + name + "/";
        }

        public string EpisodeURL(string epId)
        {
            return "http://" + URL + "/player/" + epId + "/";
        }

        public string MovieURL(string movieId)
        {
            return "http://" + URL + "/player/" + movieId + "/";
        }


        public static string ObtainURL(string website, string args)
        {
            string url = null;
            switch (website)
            {
                case "youtube": url = "http://www.youtube.com/v/" + args + "&hl=en_US&fs=1&rel=0&color1=0x7E9687&color2=0x65786C&hd=1"; break;
                case "stagevu.com": url = "http://stagevu.com/embed?width=655&height=500&background=000&uid=" + args; break;
                case "movshare.net": url = "http://www.movshare.net/embed/" + args + "/?width=653&height=362"; break;
                case "vidbux.com": url = "http://www.vidbux.com/embed-" + args + "-width-653-height-400.html"; break;
                case "videoweed.com":
                case "videoweed.es": url = "http://embed.videoweed.es/embed.php?v=" + args + "&width=653&height=525"; break;
                case "putlocker.com": url = "http://www.putlocker.com/file/" + args; break;
                case "sockshare.com": url = "http://www.sockshare.com/file" + args; break;
                case "videobb.com": url = "http://videobb.com/e/" + args; break;
                case "divxden.com":
                case "vidxden.com": url = "http://www.vidxden.com/embed-" + args + ".html"; break;
                case "tudou": url = "http://www.tudou.com/v/" + args + "/v.swf"; break;
                case "novamov.com": url = "http://embed.novamov.com/embed.php?width=653&height=525&px=1&v=" + args; break;
                case "divxstage.eu": url = "http://embed.divxstage.eu/embed.php?&width=653&height=438&v=" + args; break;
                case "youku": url = "http://player.youku.com/player.php/sid/" + args + "=/v.swf"; break;
                case "smotri.com": url = "http://pics.smotri.com/scrubber_custom8.swf?file=" + args + "&bufferTime=3&autoStart=false&str_lang=rus&xmlsource=http%3A%2F%2Fpics%2Esmotri%2Ecom%2Fcskins%2Fblue%2Fskin%5Fcolor%2Exml&xmldatasource=http%3A%2F%2Fpics%2Esmotri%2Ecom%2Fcskins%2Fblue%2Fskin%5Fng%2Exml"; break;
                case "videozer.com": url = "http://www.videozer.com/embed/" + args; break;
                case "veevr.com": url = "http://veevr.com/embed/" + args + "?w=653&h=370"; break;
                case "ovfile.com": url = "http://ovfile.com/embed-" + args + "-650x325.html"; break;
                case "gorillavid.com":
                case "gorillavid.in": url = "http://gorillavid.in/" + args; break;
                case "zalaa.com": url = "http://www.zalaa.com/embed-" + args + ".html"; break;
                case "filebox.com": url = "http://www.filebox.com/embed-" + args + "-650x450.html"; break;
                case "muchshare.net": url = "http://muchshare.net/embed-" + args + ".html"; break;
                case "uploadc.com": url = "http://www.uploadc.com/embed-" + args + ".html"; break;
                case "moviezer.com": url = "http://moviezer.com/e/" + args; break;
                case "stream2k.com": url = null; break;
                case "vidhog.com": url = "http://www.vidhog.com/embed-" + args + ".html"; break;
                case "hostingbulk.com": url = "http://hostingbulk.com/embed-" + args + "-650x350.html"; break;
                case "ufliq.com": url = "http://www.ufliq.com/embed-" + args + "-650x400.html"; break;
                case "xvidstage.com": url = "http://xvidstage.com/embed-" + args + ".html"; break;
                case "videopp.com": url = "http://videopp.com/player.php?code=" + args; break;
                case "nowvideo.eu": url = "http://embed.nowvideo.eu/embed.php?v=" + args + "&width=650&height=510"; break;
                case "watchfreeinhd.com": url = "http://www.watchfreeinhd.com/embed/" + args; break;
                case "flashx.tv": url = "http://play.flashx.tv/player/embed.php?hash=" + args + "&width=650&height=410&autoplay=no"; break;
                case "vreer.com": url = "http://vreer.com/embed-" + args + "-650x400.html"; break;
                case "nosvideo.com": url = "http://nosvideo.com/embed/" + args + "/650x370"; break;
                case "divxbase.com": url = "http://www.divxbase.com/embed-" + args + "-650x440.html"; break;
                case "daclips.in": url = "http://daclips.in/embed-" + args + "-650x350.html"; break;
                case "movpod.in": url = "http://movpod.in/embed-" + args + "-650x350.html"; break;
                case "movreel.com": url = "http://movreel.com/embed/" + args; break;
                case "180upload.com": url = "http://180upload.com/embed-" + args + "-650x370.html"; break;
                case "mooshare.biz": url = "http://mooshare.biz/embed-" + args + "-650x400.html"; break;
                case "vidbull.com": url = "http://vidbull.com/embed-" + args + "-650x328.html"; break;
                case "glumbouploads.com": url = "http://glumbouploads.com/embed-" + args + "-650x390.html"; break;
                case "modovideo.com": url = "http://www.modovideo.com/frame.php?v=" + args; break;
                case "allmyvideos.net": url = "http://allmyvideos.net/embed-" + args + "-650x360.html"; break;
                case "vidup.me": url = "http://vidup.me/embed-" + args + "-650x350.html"; break;
                case "videobam.com": url = "http://videobam.com/widget/" + args + "/custom/650"; break;
                case "xvidstream.net": url = "http://xvidstream.net/embed-" + args + ".html"; break;
            }
            return url;
        }
    }
}