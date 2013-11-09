using RestService.Services.Deprecated.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Xml.Linq;

namespace RestService.Services.Deprecated
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class TvRageService
    {
        /* Should use the API:
         *
         * EMC API key: fcrvRd4L80GiZC7uI0nu
         *
         * XML Feeds
         * Search	• http://services.tvrage.com/feeds/search.php?show=SHOWNAME
         * Detailed Search	• http://services.tvrage.com/feeds/full_search.php?show=SHOWNAME
         * Show Info	• http://services.tvrage.com/feeds/showinfo.php?sid=SHOWID
         * Episode List	• http://services.tvrage.com/feeds/episode_list.php?sid=SHOWID
         * Episode Info	• http://services.tvrage.com/feeds/episodeinfo.php?show=Show Name&exact=1&ep=SEASONxEPISODE
         * Show Info + Episode List	• http://services.tvrage.com/feeds/full_show_info.php?sid=SHOWID
         *
         * XML Example For Buffy The Vampire Slayer
         * Search	http://services.tvrage.com/feeds/search.php?show=buffy
         * Detailed Search	http://services.tvrage.com/feeds/full_search.php?show=buffy
         * Show Info	http://services.tvrage.com/feeds/showinfo.php?sid=2930
         * Episode List	http://services.tvrage.com/feeds/episode_list.php?sid=2930
         * Show Info + Episode List	http://services.tvrage.com/feeds/full_show_info.php?sid=2930
         * Episode Info	http://services.tvrage.com/feeds/episodeinfo.php?sid=2930&ep=2x04
         */

        public string value(XElement item, string elem)
        {
            XElement element = item.Element(elem);
            if (element == null) return null;
            return element.Value.Trim();
        }

        [WebGet(UriTemplate = "Search/{id}")]
        public string Search(string id)
        {
            XElement root = XElement.Load("http://services.tvrage.com/feeds/search.php?key=fcrvRd4L80GiZC7uI0nu&show=" + id);
            Dictionary<string, string> results = new Dictionary<string, string>();

            foreach (XElement e in root.Elements("show"))
            {
                string title = value(e, "name");
                string name = value(e, "showid");
                results.Add(name, title);
            }

            return JsonConvert.SerializeObject(results);
        }

        [WebGet(UriTemplate = "GetShow/{id}")]
        public string GetShow(string id)
        {
            //string baseurl = "http://www.tvrage.com/shows/id-" + id + "/";
            XElement root = XElement.Load("http://services.tvrage.com/feeds/full_show_info.php?key=fcrvRd4L80GiZC7uI0nu&sid=" + id);

            TvRageEntry entry = new TvRageEntry();

            entry.Title = value(root, "name");
            entry.NbSeasons = int.Parse(value(root, "totalseasons"));
            entry.Id = int.Parse(value(root, "showid"));
            entry.Url = value(root, "showlink");
            entry.ImageUrl = value(root, "image");
            entry.Country = value(root, "origin_country");
            entry.Status = value(root, "image");
            entry.Classification = value(root, "classification");
            entry.Genres = from item in root.Element("genres").Descendants("genre") select item.Value;
            entry.Runtime = int.Parse(value(root, "runtime"));
            entry.Network = value(root, "network");
            entry.AirDay = value(root, "airday");
            entry.TimeZone = value(root, "timezone");

            string myDate;
            string[] myDateParts;
            string datePattern;

            myDate = value(root, "started");
            if (!String.IsNullOrEmpty(myDate))
            {
                myDateParts = myDate.Trim().Split('/');
                datePattern = myDateParts.Length == 2 ? "MMM/yyyy" : "MMM/dd/yyyy";
                entry.DateStarted = DateTime.ParseExact(myDate, datePattern, CultureInfo.InvariantCulture);
            }
            else
                entry.DateStarted = DateTime.MinValue;

            myDate = value(root, "ended");
            if (!String.IsNullOrEmpty(myDate))
            {
                myDateParts = myDate.Trim().Split('/');
                datePattern = myDateParts.Length == 2 ? "MMM/yyyy" : "MMM/dd/yyyy";
                entry.DateEnded = DateTime.ParseExact(myDate, datePattern, CultureInfo.InvariantCulture);
            }
            else
                entry.DateEnded = DateTime.MaxValue;

            myDate = value(root, "airtime");
            if (!String.IsNullOrEmpty(myDate))
            {
                datePattern = "HH:mm";
                entry.AirTime = DateTime.ParseExact(myDate, datePattern, CultureInfo.InvariantCulture);
            }
            else
                entry.AirTime = DateTime.MaxValue;

            if (root.Element("Episodelist") != null)
            {
                XElement elementLast = (from item in root.Element("Episodelist").Descendants("episode") where item.Element("airdate").Value.CompareTo(DateTime.Now.ToString("yyyy-MM-dd")) <= 0 orderby item.Element("airdate").Value descending select item).FirstOrDefault();
                if (elementLast != null)
                {
                    TvRageEntry.EpisodeInfo info = new TvRageEntry.EpisodeInfo();
                    if (elementLast.Parent.Name.LocalName == "Season")
                    {
                        info.AbsoluteNo = int.Parse(value(elementLast, "epnum"));
                        info.RelativeNo = int.Parse(value(elementLast, "seasonnum"));
                        info.Season = int.Parse(elementLast.Parent.Attribute("no").Value);
                    }
                    else
                    {
                        info.AbsoluteNo = 0;
                        info.RelativeNo = 0;
                        info.Season = int.Parse(value(elementLast, "season"));
                    }
                    info.Title = value(elementLast, "title");
                    string epId = value(elementLast, "link");
                    info.EpId = epId == null ? -1 : int.Parse(epId.Substring(epId.LastIndexOf("/") + 1));
                    DateTime date = DateTime.MinValue;
                    DateTime.TryParseExact(myDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
                    info.AirDate = date;
                    entry.LastEpisode = info;
                }

                XElement elementNext = (from item in root.Element("Episodelist").Descendants("episode") where item.Element("airdate").Value.CompareTo(DateTime.Now.ToString("yyyy-MM-dd")) > 0 orderby item.Element("airdate").Value ascending select item).FirstOrDefault();
                if (elementNext != null)
                {
                    TvRageEntry.EpisodeInfo info = new TvRageEntry.EpisodeInfo();
                    if (elementNext.Parent.Name.LocalName == "Season")
                    {
                        info.AbsoluteNo = int.Parse(value(elementNext, "epnum"));
                        info.RelativeNo = int.Parse(value(elementNext, "seasonnum"));
                        info.Season = int.Parse(elementNext.Parent.Attribute("no").Value);
                    }
                    else
                    {
                        info.AbsoluteNo = 0;
                        info.RelativeNo = 0;
                        info.Season = int.Parse(value(elementNext, "season"));
                    }
                    info.Title = value(elementNext, "title");
                    string epId = value(elementNext, "link");
                    info.EpId = epId == null ? -1 : int.Parse(epId.Substring(epId.LastIndexOf("/") + 1));
                    DateTime date = DateTime.MinValue;
                    DateTime.TryParseExact(myDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
                    info.AirDate = date;
                    entry.NextEpisode = info;
                }
            }
            return JsonConvert.SerializeObject(entry);
        }
    }
}