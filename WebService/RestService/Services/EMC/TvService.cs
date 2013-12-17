using EricUtility2011.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestService.Data.Emc2;
using RestService.StreamingWebsites;
using RestService.StreamingWebsites.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace RestService.Services.Emc
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class TvService
    {
        private static Dictionary<string, Dictionary<string, ITvWebsite>> m_Supported = new Dictionary<string, Dictionary<string, ITvWebsite>>()
        {
            {
                "en",new Dictionary<string, ITvWebsite>
                    {
                        {TubePlusWebsite.NAME,new TubePlusWebsite()},
                        {WatchSeriesOnlineTvWebsite.NAME,new WatchSeriesOnlineTvWebsite()},
                        {ProjectFreeTvWebsite.NAME,new ProjectFreeTvWebsite()},
                        {PrimeWireWebsite.NAME,new PrimeWireWebsite()},
                        {WatchSeriesTvWebsite.NAME,new WatchSeriesTvWebsite()},
                    }
            },
        };

        [WebGet(UriTemplate = "Supported")]
        public string Supported()
        {
            Dictionary<string, IEnumerable<string>> sortedDic = new Dictionary<string, IEnumerable<string>>();
            string[] lang = m_Supported.Keys.ToArray();
            Array.Sort(lang);
            foreach (string l in lang)
            {
                string[] websites = m_Supported[l].Keys.ToArray();
                Array.Sort(websites);
                sortedDic.Add(l, websites);
            }
            return JsonConvert.SerializeObject(sortedDic);
        }

        private void BuildWebsiteList(string lang, Dictionary<string, object> websites, string website)
        {
            if (website == "all")
                m_Supported[lang].Keys.ToList().ForEach(x => websites.Add(x, null));
            else if (website.StartsWith("some_"))
            {
                string[] somes = website.Split('_');
                somes.Skip(1).ToList().ForEach(x => websites.Add(x, null));
            }
            else
                websites.Add(website, null);
        }

        [WebGet(UriTemplate = "Search/{lang}/{website}/{keywords}")]
        public string Search(string lang, string website, string keywords)
        {
            Dictionary<string, object> websites = new Dictionary<string, object>();
            BuildWebsiteList(lang, websites, website);

            Parallel.ForEach(websites.Keys, site => websites[site] = (!m_Supported.ContainsKey(lang) || !m_Supported[lang].ContainsKey(site)) ? null : m_Supported[lang][site].SearchAsync(keywords).Result);
            return JsonConvert.SerializeObject(websites);
        }

        [WebGet(UriTemplate = "Letter/{lang}/{website}/{letter}")]
        public string Letter(string lang, string website, string letter)
        {
            Dictionary<string, object> websites = new Dictionary<string, object>();
            BuildWebsiteList(lang, websites, website);

            Parallel.ForEach(websites.Keys, site => websites[site] = (!m_Supported.ContainsKey(lang) || !m_Supported[lang].ContainsKey(site)) ? null : m_Supported[lang][site].StartsWithAsync(letter).Result);
            return JsonConvert.SerializeObject(websites);
        }

        [WebGet(UriTemplate = "Show/{lang}/{website}/{showId}")]
        public string Show(string lang, string website, string showId)
        {
            if (!m_Supported.ContainsKey(lang) || !m_Supported[lang].ContainsKey(website))
                return null;

            TvShow show = m_Supported[lang][website].ShowAsync(showId, false).Result;
            if (show != null)
                Database.TvShowUpdateLastEpisode(show, website, showId);
            return JsonConvert.SerializeObject(show ?? new TvShow());
        }

        [WebGet(UriTemplate = "ShowFull/{lang}/{website}/{showId}")]
        public string ShowFull(string lang, string website, string showId)
        {
            if (!m_Supported.ContainsKey(lang) || !m_Supported[lang].ContainsKey(website))
                return null;
            TvShow show = m_Supported[lang][website].ShowAsync(showId, true).Result;
            if (show != null)
                Database.TvShowUpdateLastEpisode(show, website, showId);
            return JsonConvert.SerializeObject(m_Supported[lang][website].ShowAsync(showId, true).Result ?? new TvShow());
        }

        [WebGet(UriTemplate = "ShowURL/{lang}/{website}/{showId}")]
        public string ShowURL(string lang, string website, string showId)
        {
            if (!m_Supported.ContainsKey(lang) || !m_Supported[lang].ContainsKey(website))
                return null;
            return JsonConvert.SerializeObject(m_Supported[lang][website].ShowURL(showId));
        }

        [WebGet(UriTemplate = "Episode/{lang}/{website}/{epId}")]
        public string Episode(string lang, string website, string epId)
        {
            if (!m_Supported.ContainsKey(lang) || !m_Supported[lang].ContainsKey(website))
                return null;
            return JsonConvert.SerializeObject(m_Supported[lang][website].EpisodeAsync(epId).Result ?? new Episode());
        }

        [WebGet(UriTemplate = "EpisodeURL/{lang}/{website}/{epId}")]
        public string EpisodeURL(string lang, string website, string epId)
        {
            if (!m_Supported.ContainsKey(lang) || !m_Supported[lang].ContainsKey(website))
                return null;
            return JsonConvert.SerializeObject(m_Supported[lang][website].EpisodeURL(epId));
        }

        [WebGet(UriTemplate = "Stream/{lang}/{website}/{streamWebsite}/{args}")]
        public string Stream(string lang, string website, string streamWebsite, string args)
        {
            if (!m_Supported.ContainsKey(lang) || !m_Supported[lang].ContainsKey(website))
                return null;
            StreamingInfo info = m_Supported[lang][website].StreamAsync(streamWebsite, args).Result;
            return JsonConvert.SerializeObject(info ?? new StreamingInfo() { Website = streamWebsite, Arguments = args });
        }

        [WebGet(UriTemplate = "Favs/{user}/{token}")]
        public string Favs(string user, string token)
        {
            SPResult rAll = Database.UserGetAllFavTvShows(user, token);

            Dictionary<string, object> p = rAll.Parameters;

            if ((bool)p["@ok"])
                return JsonConvert.SerializeObject(new { success = true, username = user, shows = rAll.QueryResults, token = (String)p["@info"], until = (DateTimeOffset)p["@validUntil"] }, new IsoDateTimeConverter());
            else
                return JsonConvert.SerializeObject(new { success = false, problem = (String)p["@info"] });
        }

        [WebGet(UriTemplate = "AddFav/{user}/{token}/{lang}/{website}/{showname}/{showtitle}/{lastseason}/{lastepisode}")]
        public string AddFav(string user, string token, string lang, string website, string showname, string showtitle, string lastseason, string lastepisode)
        {
            Dictionary<string, object> p = Database.UserAddFavTvShow(user, token, lang, website, showname, showtitle, lastseason, lastepisode);

            if ((bool)p["@ok"])
                return JsonConvert.SerializeObject(new { success = true });
            else
                return JsonConvert.SerializeObject(new { success = false, problem = (String)p["@info"] });
        }

        [WebGet(UriTemplate = "DelFav/{user}/{token}/{lang}/{website}/{showname}")]
        public string DelFav(string user, string token, string lang, string website, string showname)
        {
            Dictionary<string, object> p = Database.UserDelFavTvShow(user, token, website, lang, showname);

            if ((bool)p["@ok"])
                return JsonConvert.SerializeObject(new { success = true });
            else
                return JsonConvert.SerializeObject(new { success = false, problem = (String)p["@info"] });
        }

        [WebGet(UriTemplate = "LastViewed/{user}/{token}/{lang}/{website}/{showname}/{lastviewedseason}/{lastviewedepisode}")]
        public string LastViewed(string user, string token, string lang, string website, string showname, string lastviewedseason, string lastviewedepisode)
        {
            Dictionary<string, object> p = Database.UserSetLastViewedTvEpisode(user, token, website, lang, showname, lastviewedseason, lastviewedepisode);

            if ((bool)p["@ok"])
                return JsonConvert.SerializeObject(new { success = true });
            else
                return JsonConvert.SerializeObject(new { success = false, problem = (String)p["@info"] });
        }
    }
}