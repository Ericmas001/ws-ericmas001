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
    public class VioozMovieWebsite : IMovieWebsite
    {
        public static string NAME { get { return "viooz.co"; } }
        public static string URL { get { return "viooz.co"; } }

        private async Task<IEnumerable<ListedMovie>> AvailableMovieAsync(string baseurl)
        {
            List<ListedMovie> availables = new List<ListedMovie>();
            string src = await new HttpClient().GetStringAsync(baseurl);

            int max = 1;

            string pages = src.Extract("<div align=\"left\" class=\"pagination\"", "</div>");
            if (pages != null)
            {
                int lio = pages.LastIndexOf("<a href=\"http://" + URL + "/page/");
                if (lio > -1)
                {
                    if (pages.Substring(lio).Contains(">&#8594;<"))
                    {
                        pages = pages.Remove(lio);
                        lio = pages.LastIndexOf("<a href=\"http://" + URL + "/page/");
                    }
                    max = int.Parse(pages.Substring(lio).Extract("/page/", "/"));
                }
            }

            for (int i = 0; i < max; ++i)
            {
                if (i > 0)
                    src = await new HttpClient().GetStringAsync(baseurl.Replace(URL, URL + "/page/" + (i + 1)));

                string allShows = src.Extract("<div id=\"list\" class=\"films\">", "<div style=\"text-align: center; margin-top: 22px;\">");
                string itemp = "<div id=\"film_";
                int start = allShows.IndexOf(itemp) + itemp.Length;
                while (start >= itemp.Length)
                {
                    int end = allShows.IndexOf("class=\"button_launch_film_img\" />", start);
                    end = end == -1 ? allShows.Length - 1 : end;
                    string item = allShows.Substring(start, end - start);

                    ListedMovie entry = new ListedMovie();
                    entry.Name = item.Extract("<span class=\"title_list\"><a href=\"http://" + URL + "/movies/", ".html");
                    entry.Title = item.Extract("<h3 class=\"title_grid\" title=\"", "\"");
                    availables.Add(entry);
                    start = allShows.IndexOf(itemp, end) + itemp.Length;
                }
            }
            ListedMovie[] items = availables.ToArray();
            Array.Sort(items);
            return items;
        }

        async Task<IEnumerable<ListedMovie>> IMovieWebsite.SearchAsync(string keywords)
        {
            try
            {
                return await AvailableMovieAsync("http://" + URL + "/search?q=" + keywords.Replace(" ", "+") + "&s=t");
            }
            catch { return null; }
        }

        async Task<IEnumerable<ListedMovie>> IMovieWebsite.StartsWithAsync(string letter)
        {
            try
            {
                return await AvailableMovieAsync("http://" + URL + "/title/" + letter);
            }
            catch { return null; }
        }

        public async Task<Movie> MovieAsync(string movieId)
        {
            Movie mov = new Movie();
            mov.Name = movieId;
            string baseurl = "http://" + URL + "/movies/" + movieId + ".html";
            string src = await new HttpClient().GetStringAsync(baseurl);

            mov.Title = src.Extract("<title>Watch ", " Online for Free - Viooz</title>");

            mov.Links.Add(NAME, new List<string>() { mov.Name });
            return mov;
        }

        public async Task<StreamingInfo> StreamAsync(string website, string args)
        {
            return new StreamingInfo() { StreamingURL = "http://" + URL + "/movies/" + args + ".html", Arguments = args, Website = website, DownloadURL = null };
        }

        public string MovieURL(string movieId)
        {
            return "http://" + URL + "/movies/" + movieId + ".html";
        }

    }
}