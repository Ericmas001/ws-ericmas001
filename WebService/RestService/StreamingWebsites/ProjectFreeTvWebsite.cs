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
    public class ProjectFreeTvWebsite : ITvWebsite
    {
        public static string NAME { get { return "free-tv-video-online.me"; } }
        public static string URL { get { return "www.free-tv-video-online.me"; } }

        private async Task<IEnumerable<ListedTvShow>> AvailableShowsAsync(params string[] keywords)
        {
            List<ListedTvShow> availables = new List<ListedTvShow>();
            string src = await new HttpClient().GetStringAsync("http://" + URL + "/internet/");
            string allShows = src.Extract("<table width=\"100%\" border=\"0\" cellpadding=\"0\" cellspacing=\"1\" align=\"center\" bgcolor=\"#FFFFFF\">", "<!-- Start of the latest link tables -->");
            string itemp = "class=\"mnlcategorylist\"";
            int start = allShows.IndexOf(itemp) + itemp.Length;
            while (start >= itemp.Length)
            {
                int end = allShows.IndexOf("</tr>", start);
                end = end == -1 ? allShows.Length - 1 : end;
                string item = allShows.Substring(start, end - start);

                ListedTvShow entry = new ListedTvShow();
                entry.Name = item.Extract( "><a href=\"", "/\">");
                entry.Title = item.Extract( "<b>", "</b>");
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

            string baseurl = "http://" + URL + "/internet/" + name;
            string src = await new HttpClient().GetStringAsync(baseurl);

            if (src.Contains("Project Free TV Disclaimer"))
                return null;
            show.Title = src.Extract("<h1 style=\"margin:0; font-size: 20px;\">", "</h1>");
            if (show.Title == null)
                return null;
            Dictionary<int, string> seasons = new Dictionary<int, string>();
            string allSeasons = src.Extract(" Categories</td>", "<!-- Start of the latest link tables -->");
            string seasDeb = "mnlcategorylist\">";
            int startS = allSeasons.IndexOf(seasDeb) + seasDeb.Length;
            while (startS >= seasDeb.Length)
            {
                int endS = allSeasons.IndexOf("</td>", startS);
                string itemS = allSeasons.Substring(startS, endS - startS).Trim();
                string noSt = itemS.Extract( "<b>Season ", "</b>");
                int no = -1;
                if (!int.TryParse(noSt, out no))
                    no = -1;
                if (no != -1)
                {
                    seasons.Add(no, itemS.Extract( "<a href=\"", ".html\">"));
                    show.Episodes.Add(no, new List<ListedEpisode>());
                }
                startS = allSeasons.IndexOf(seasDeb, endS) + seasDeb.Length;
            }
            int[] noS = full ? seasons.Keys.ToArray() : (new int[] { seasons.Keys.Max() });
            show.IsComplete = (noS.Length == seasons.Keys.Count);
            foreach (int no in noS)
            {
                List<ListedEpisode> episodes = (List<ListedEpisode>)show.Episodes[no];
                string srcS = await new HttpClient().GetStringAsync(baseurl + "/" + seasons[no] + ".html");
                string allEps = srcS.Extract( "<!-- Start of middle column -->", "<!-- End of the middle_column -->");
                string epDeb = "<td class=\"episode\">";
                int startE = allEps.IndexOf(epDeb) + epDeb.Length;
                while (startE >= epDeb.Length)
                {
                    ListedEpisode ep = new ListedEpisode();
                    int endE = allEps.IndexOf("<a class=\"info\"", startE);
                    if (endE == -1)
                        break;
                    string itemS = allEps.Substring(startE, endE - startE).Trim();
                    ep.Name = show.Name + "-" + seasons[no] + "-" + itemS.Extract( "<a name=\"", "\"></a>");
                    string eTitle = itemS.Extract( "</a><b>", "</b></td>");
                    string noEnd = ". ";
                    int iNoEnd = eTitle.IndexOf(noEnd);
                    ep.Title = iNoEnd >= 0 ? eTitle.Substring(iNoEnd + noEnd.Length) : eTitle;
                    string nfo = itemS.Extract( "<td class=\"mnllinklist\" align=\"right\">", "/div>");
                    ep.NoSeason = int.Parse(nfo.Extract( ">S", "E"));
                    ep.NoEpisode = int.Parse(nfo.Extract( ep.NoSeason + "E", "&nbsp;"));
                    string dt = nfo.Extract( "Air Date: ", "<");
                    ep.ReleaseDate = DateTime.ParseExact(dt, "dd MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
                    episodes.Add(ep);
                    startE = allEps.IndexOf(epDeb, endE) + epDeb.Length;
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
            string[] info = epId.Split('-');
            string baseurl = "http://" + URL + "/internet/" + info[0] + "/" + info[1] + ".html";
            string src = await new HttpClient().GetStringAsync(baseurl);

            if (src.Contains("Project Free TV Disclaimer"))
                return null;

            string all = StringUtility.Extract(src.Replace("</td></tr><tr class=\"3\" bgcolor=\"#E3E3E3\" >", "</td></tr></table>"), "<a name=\"" + info[2] + "\">", "</td></tr></table>");

            ep.Title = all.Extract( "</a><b>", "</b");
            string nfo = all.Extract( "<td class=\"mnllinklist\" align=\"right\">", "/div>");
            ep.NoSeason = int.Parse(nfo.Extract( ">S", "E"));
            ep.NoEpisode = int.Parse(nfo.Extract( ep.NoSeason + "E", "&nbsp;"));
            string dt = nfo.Extract( "Air Date: ", "<");
            ep.ReleaseDate = DateTime.ParseExact(dt, "dd MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);

            string linkDeb = "class=\"mnllinklist dotted\">";
            int startP = all.IndexOf(linkDeb) + linkDeb.Length;
            while (startP >= linkDeb.Length)
            {
                int endP = all.IndexOf("</a>", startP);
                string itemP = all.Substring(startP, endP - startP).Trim();
                startP = all.IndexOf(linkDeb, endP) + linkDeb.Length;

                string website = itemP.Extract( "Host: ", "<br/>");
                string url = itemP.Extract( "href=\"http://" + URL + "/player/", "\"");
                if (url == null)
                {
                    url = itemP.Extract( "href=\"http://", "\"").Replace("/", "_");
                }
                else
                    url = "php_" + url.Replace(".php?id=", "_id_");

                if (!ep.Links.ContainsKey(website))
                    ep.Links.Add(website, new List<string>());
                ep.Links[website].Add(url);
            }

            return ep;
        }

        public async Task<StreamingInfo> StreamAsync(string website, string args)
        {
            string url = null;
            if (args.StartsWith("php_"))
            {
                string mid = "_id_";
                string page = args.Extract( "php_", mid) + ".php";
                args = args.Substring(args.IndexOf(mid) + mid.Length);
                url = "http://" + URL + "/player/" + page + "?id=" + args;

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
            }
            else
                url = "http://" + args.Replace("_", "/");
            return new StreamingInfo() { StreamingURL = url, Arguments = args, Website = website, DownloadURL = null };
        }

        public string ShowURL(string name)
        {
            return "http://" + URL + "/internet/" + name;
        }

        public string EpisodeURL(string epId)
        {
            string[] info = epId.Split('-');
            return "http://" + URL + "/internet/" + info[0] + "/" + info[1] + ".html#" + info[2];
        }
    }
}