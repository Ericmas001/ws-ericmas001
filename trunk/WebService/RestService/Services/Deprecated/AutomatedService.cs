using EricUtility2011.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace RestService.Services.Deprecated
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class AutomatedService
    {
        //Using https://www.setcronjob.com to execute !
        private SqlServerConnector Connector = new SqlServerConnector("TURNSOL.arvixe.com", "emc", "emc.webservice", "Emc42FTW");

        [WebGet(UriTemplate = "UpdateLastEpisodes")]
        public string UpdateLastEpisodes()
        {
            SqlConnection myConnection = Connector.GetConnection();
            List<Dictionary<string, object>> results = Connector.SelectRows(myConnection, "select * from ericmas001.TLastEpisodes", new Dictionary<string, object>());

            //WatchSeriesService service =  new WatchSeriesService();
            TubePlusService service = new TubePlusService();
            List<object> changes = new List<object>();
            foreach (Dictionary<string, object> result in results)
            {
                string show = result["showname"].ToString();
                int saved_lastSeason = String.IsNullOrEmpty(result["lastSeason"].ToString()) ? 0 : (int)result["lastSeason"];
                int saved_lastEpisode = String.IsNullOrEmpty(result["lastEpisode"].ToString()) ? 0 : (int)result["lastEpisode"];
                string res = service.GetShow(show);
                if (res == null)
                {
                    changes.Add(new { showname = show, error = "show not found" });
                    continue;
                }
                JObject r = JsonConvert.DeserializeObject<dynamic>(res);
                JArray seasons = (JArray)r["Seasons"];
                JObject lastSeason = (JObject)seasons[seasons.Count - 1];
                JArray episodes = (JArray)lastSeason["Episodes"];
                JObject lastEpisode = (JObject)episodes[episodes.Count - 1];

                int lastSeasonNo = (int)lastSeason["SeasonNo"];
                int lastEpisodeNo = (int)lastEpisode["EpisodeNo"];

                bool changeToMake = (lastSeasonNo != saved_lastSeason || lastEpisodeNo != saved_lastEpisode);
                changes.Add(new { showname = show, lastSeason = lastSeasonNo, lastEpisode = lastEpisodeNo, changed = changeToMake });

                if (changeToMake)
                    Connector.Execute(myConnection, "update ericmas001.TLastEpisodes set lastSeason = @Season, lastEpisode = @Episode where showname = @Show", new Dictionary<string, object>() { { "Show", show }, { "Season", lastSeasonNo }, { "Episode", lastEpisodeNo } });
            }
            myConnection.Close();
            return JsonConvert.SerializeObject(changes);
        }
    }
}