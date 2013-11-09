using RestService.StreamingWebsites.Entities;
using EricUtility;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace RestService.StreamingWebsites
{
    public class LookizMovieWebsite : IMovieWebsite
    {
        public static string NAME { get { return "lookiz.ws"; } }
        public static string URL { get { return "www.lookiz.ws"; } }

        private async Task<IEnumerable<ListedMovie>> AvailableMoviesAsync(string baseurl)
        {
            List<ListedMovie> availables = new List<ListedMovie>();
            string src = await new HttpClient().GetStringAsync(baseurl);
            string pages = src.Extract("<div class='pagination'>", "</div>");
            int max = 1;
            int lio = pages.LastIndexOf("<a href=\"/recherche/");
            if (lio > -1)
            {
                if (pages.Substring(lio).Contains("title=\"Suivant\""))
                {
                    pages = pages.Remove(lio);
                    lio = pages.LastIndexOf("<a href=\"/recherche/");
                }
                max = int.Parse(pages.Substring(lio).Extract("page,", "\""));
            }


            for (int i = 0; i < max; ++i)
            {
                if (i > 0)
                    src = await new HttpClient().GetStringAsync(baseurl + "-page," + (i + 1));
                string allShows = src.Extract("<div id='liste' class='films'>", "<div id='contenu_bottom'>");
                string itemp = "<div id='film_";
                int start = allShows.IndexOf(itemp) + itemp.Length;
                while (start >= itemp.Length)
                {
                    int end = allShows.IndexOf("<div class='boutons'>", start);
                    end = end == -1 ? allShows.Length - 1 : end;
                    string item = allShows.Substring(start, end - start);

                    ListedMovie entry = new ListedMovie();
                    entry.Name = item.Extract("-streaming/", "'>");
                    entry.Title = item.Extract("<h3 class='titre_detail'>", "</h3>");
                    availables.Add(entry);
                    start = allShows.IndexOf(itemp, end) + itemp.Length;
                }
            }
            ListedMovie[] items = availables.ToArray();
            Array.Sort(items);
            return items;
        }

        public async Task<IEnumerable<ListedMovie>> SearchAsync(string keywords)
        {
            try
            {
                return await AvailableMoviesAsync("http://" + URL + "/recherche/tf-" + keywords.Replace(" ", "+"));
            }
            catch { return null; }
        }

        public async Task<IEnumerable<ListedMovie>> StartsWithAsync(string letter)
        {
            try
            {
                return await AvailableMoviesAsync("http://" + URL + "/films/titre/" + letter);
            }
            catch { return null; }
        }

        public async Task<Movie> MovieAsync(string movieId)
        {
            Movie mov = new Movie();
            mov.Name = movieId;
            string baseurl = "http://" + URL + "/films/movie/" + movieId;
            string src = await new HttpClient().GetStringAsync(baseurl);

            //if (src.Contains("Movie have been removed"))
            //    return null;
            mov.Title = src.Extract( "<span style=\"margin-left:5px;margin-top:-15px\">", "</span>");

            string all = src.Extract("<div class='boxed bloc_plateforme'>", "<div class='bloc_social'>");

            string linkDeb = "<a class='lien_plateforme' ";
            int startP = all.IndexOf(linkDeb) + linkDeb.Length;
            while (startP >= linkDeb.Length)
            {
                int endP = all.IndexOf("</a>", startP);
                string itemP = all.Substring(startP, endP - startP).Trim();
                startP = all.IndexOf(linkDeb, endP) + linkDeb.Length;

                string website = WebsiteConvert(itemP.Extract("alt='", "'"));
                string url = itemP.Extract("href='", "'");

                if (!mov.Links.ContainsKey(website))
                    mov.Links.Add(website, new List<string>());
                mov.Links[website].Add(url);
            }
            return mov;
        }

        public string WebsiteConvert(string website)
        {
            switch (website)
            {
                case "purevid": return "purevid.com";
                case "youwatch": return "youwatch.org";
            }
            return website;
        }

        public async Task<StreamingInfo> StreamAsync(string website, string args)
        {
            string baseurl = "http://" + URL + "/films/link/" + args;
            string src = await new HttpClient().GetStringAsync(baseurl);
            string url = WebsiteGetUrl(website,src);
            return url == null ? null : new StreamingInfo() { StreamingURL = url, Arguments = args, Website = website, DownloadURL = null };
        }

        public string WebsiteGetUrl(string website, string src)
        {
            switch (website)
            {
                case "purevid.com": return src.Extract("<iframe width=\"755px\" height=\"422px\" frameBorder=\"0\" src=\"", "\">");
                case "youwatch.org": return src.Extract("<IFRAME SRC=\"", "\"");
            }
            return null;
        }

        public string MovieURL(string movieId)
        {
            return "http://" + URL + "/films/stream/" + movieId;
        }
    }
}