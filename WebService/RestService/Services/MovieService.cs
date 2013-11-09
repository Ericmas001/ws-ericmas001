using RestService.StreamingWebsites;
using RestService.StreamingWebsites.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace RestService.Services
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class MovieService
    {
        private static Dictionary<string, Dictionary<string, IMovieWebsite>> m_Supported = new Dictionary<string, Dictionary<string, IMovieWebsite>>()
        {
            {
                "en",new Dictionary<string, IMovieWebsite>
                    {
                        {TubePlusWebsite.NAME,new TubePlusWebsite()},
                        {PrimeWireWebsite.NAME,new PrimeWireWebsite()},
                        {VioozMovieWebsite.NAME,new VioozMovieWebsite()},
                    }
            },
            {
                "fr",new Dictionary<string, IMovieWebsite>
                    {
                        {LookizMovieWebsite.NAME,new LookizMovieWebsite()},
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

        [WebGet(UriTemplate = "Movie/{lang}/{website}/{movieId}")]
        public string Movie(string lang, string website, string movieId)
        {
            if (!m_Supported.ContainsKey(lang) || !m_Supported[lang].ContainsKey(website))
                return null;
            return JsonConvert.SerializeObject(m_Supported[lang][website].MovieAsync(movieId).Result ?? new Movie());
        }

        [WebGet(UriTemplate = "MovieURL/{lang}/{website}/{movieId}")]
        public string MovieURL(string lang, string website, string movieId)
        {
            if (!m_Supported.ContainsKey(lang) || !m_Supported[lang].ContainsKey(website))
                return null;
            return JsonConvert.SerializeObject(m_Supported[lang][website].MovieURL(movieId));
        }

        [WebGet(UriTemplate = "Stream/{lang}/{website}/{streamWebsite}/{args}")]
        public string Stream(string lang, string website, string streamWebsite, string args)
        {
            if (!m_Supported.ContainsKey(lang) || !m_Supported[lang].ContainsKey(website))
                return null;
            StreamingInfo info = m_Supported[lang][website].StreamAsync(streamWebsite, args).Result;
            return JsonConvert.SerializeObject(info ?? new StreamingInfo() { Website = streamWebsite, Arguments = args });
        }
    }
}