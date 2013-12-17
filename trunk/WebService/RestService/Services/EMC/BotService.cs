using EricUtility;
using EricUtility2011.Data;
using Newtonsoft.Json;
using RestService.Data.Emc2;
using RestService.StreamingWebsites.Entities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace RestService.Services.Emc
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class BotService
    {
        private SqlServerConnector Connector = new SqlServerConnector("TURNSOL.arvixe.com", "emc2", "emc.webservice", "Emc42FTW");

        [WebGet(UriTemplate = "TvUpdate")]
        public string TvUpdate()
        {
            SqlConnection myConnection = Connector.GetConnection();
            SPResult rAll = Connector.SelectRowsSP(myConnection, "ericmas001.SPTvAllShows");
            TvService tv = new TvService();
            List<object> changes = new List<object>();
            foreach (Dictionary<string, object> r in rAll.QueryResults)
            {
                string website = (string)r["website"];
                string lang = "en";
                string realwebsite = website;
                if (website.StartsWith("|"))
                {
                    lang = website.Extract("|", "|");
                    realwebsite = website.Replace("|" + lang + "|", "");
                }
                string name = (string)r["showname"];
                string title = (string)r["showtitle"];
                int lastS = (int)r["lastSeason"];
                int lastE = (int)r["lastEpisode"];

                string res = tv.Show(lang, realwebsite, name);
                if (res == null)
                {
                    changes.Add(new { showname = name, website = website, info = "Show not found" });
                    continue;
                }

                TvShow show = JsonConvert.DeserializeObject<TvShow>(res);
                if (lastS == show.NoLastSeason && lastE == show.NoLastEpisode)
                {
                    changes.Add(new { showname = name, website = website, info = "Already Up to Date" });
                    continue;
                }

                Dictionary<string, object> p = Database.TvShowUpdateLastEpisode(show, website, name);
                if (!(bool)p["@ok"])
                    return JsonConvert.SerializeObject(new { success = false, problem = (String)p["@info"] });
                changes.Add(new { showname = name, website = website, info = (String)p["@info"] });
            }
            if (myConnection != null)
                myConnection.Close();
            return JsonConvert.SerializeObject(new { success = true, log = changes });
        }
    }
}