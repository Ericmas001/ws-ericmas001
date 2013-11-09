using EricUtility;
using EricUtility2011.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace RestService.Services.Emc.Deprecated
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class UserService
    {
        private SqlServerConnector Connector = new SqlServerConnector("TURNSOL.arvixe.com", "emc", "emc.webservice", "Emc42FTW");

        [WebGet(UriTemplate = "Connect/{user}/{pass}")]
        public string Connect(string user, string pass)
        {
            string error = null;
            bool ok = false;
            SqlConnection myConnection = Connector.GetConnection();
            try
            {
                Dictionary<string, object> result = Connector.SelectOneRow(myConnection, "select * from ericmas001.TUser where username = @User", new Dictionary<string, object>() { { "User", user } });
                if (result.ContainsKey("password") && pass.Trim() == result["password"].ToString().Trim())
                    ok = true;
                if (!ok)
                    error = "User or Password is incorrect";
            }
            catch (Exception e)
            {
                error = "Read Error: " + e.ToString();
            }

            if (ok)
            {
                string t = StringUtility.GetMd5Hash(user + ";" + DateTime.Now.ToString());
                DateTime d = DateTime.Now + TimeSpan.FromMinutes(5);
                DateTime validUntil = new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second, DateTimeKind.Local);
                Connector.Execute(myConnection, "update ericmas001.TUser set lastToken = @Token, tokenValidUntil = @Valid where username = @User", new Dictionary<string, object>() { { "Token", t }, { "Valid", validUntil }, { "User", user } });

                if (myConnection != null)
                    myConnection.Close();
                return JsonConvert.SerializeObject(new { success = ok, token = t, until = validUntil }, new IsoDateTimeConverter());
            }
            else
            {
                if (myConnection != null)
                    myConnection.Close();
                return JsonConvert.SerializeObject(new { success = ok, problem = error });
            }
        }

        [WebGet(UriTemplate = "Register/{user}/{pass}")]
        public string Register(string user, string pass)
        {
            string error = null;
            bool ok = false;
            SqlConnection myConnection = Connector.GetConnection();
            try
            {
                Dictionary<string, object> result = Connector.SelectOneRow(myConnection, "select * from ericmas001.TUser where username = @User", new Dictionary<string, object>() { { "User", user } });
                if (result.Count == 0)
                    ok = true;
                if (!ok)
                    error = "User already exist";
            }
            catch (Exception e)
            {
                error = "Read Error: " + e.ToString();
            }

            if (ok)
            {
                Connector.Execute(myConnection, "insert into ericmas001.TUser (username,password) values (@User,@Password)", new Dictionary<string, object>() { { "User", user }, { "Password", pass } });
                if (myConnection != null)
                    myConnection.Close();
                return JsonConvert.SerializeObject(new { success = ok });
            }
            else
            {
                if (myConnection != null)
                    myConnection.Close();
                return JsonConvert.SerializeObject(new { success = ok, problem = error });
            }
        }

        private KeyValuePair<string, DateTime> GetUsernameFromToken(SqlConnection myConnection, string token)
        {
            if (myConnection != null)
            {
                Dictionary<string, object> result = Connector.SelectOneRow(myConnection, "select * from ericmas001.TUser where lastToken = @Token and tokenValidUntil >= @ValidUntil", new Dictionary<string, object>() { { "Token", token }, { "ValidUntil", DateTime.Now } });
                if (result.ContainsKey("username"))
                {
                    string user = result["username"].ToString();
                    DateTime d = DateTime.Now + TimeSpan.FromMinutes(5);
                    DateTime validUntil = new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second);
                    Connector.Execute(myConnection, "update ericmas001.TUser set tokenValidUntil = @Valid where username = @User", new Dictionary<string, object>() { { "Valid", validUntil }, { "User", user } });
                    return new KeyValuePair<string, DateTime>(user, validUntil);
                }
            }
            return new KeyValuePair<string, DateTime>(null, DateTime.MinValue);
        }

        [WebGet(UriTemplate = "GetFavs/{token}")]
        public string GetFavs(string token)
        {
            SqlConnection myConnection = Connector.GetConnection();
            KeyValuePair<string, DateTime> tokenInfo = GetUsernameFromToken(myConnection, token);
            string user = tokenInfo.Key;
            if (user == null)
            {
                if (myConnection != null)
                    myConnection.Close();
                return JsonConvert.SerializeObject(new { success = false, problem = "Invalid Token" });
            }
            else
            {
                List<Dictionary<string, object>> results = Connector.SelectRows(myConnection, "select f.username, f.showname, f.lastViewedSeason, f.lastViewedEpisode, s.showtitle, e.lastSeason, e.lastEpisode from ericmas001.TFavoriteShows f LEFT OUTER JOIN ericmas001.TShows s ON f.showname = s.showname LEFT OUTER JOIN ericmas001.TLastEpisodes e ON f.showname = e.showname where f.username = @User order by f.showname", new Dictionary<string, object>() { { "User", user } });
                if (myConnection != null)
                    myConnection.Close();
                return JsonConvert.SerializeObject(new { success = true, favorites = results, token = token, until = tokenInfo.Value }, new IsoDateTimeConverter());
            }
        }

        [WebGet(UriTemplate = "AddFav/{token}/{showname}")]
        public string AddFav(string token, string showname)
        {
            SqlConnection myConnection = Connector.GetConnection();
            KeyValuePair<string, DateTime> tokenInfo = GetUsernameFromToken(myConnection, token);
            string user = tokenInfo.Key;
            if (user == null)
            {
                if (myConnection != null)
                    myConnection.Close();
                return JsonConvert.SerializeObject(new { success = false, problem = "Invalid Token" });
            }
            else
            {
                Dictionary<string, object> result = Connector.SelectOneRow(myConnection, "select * from ericmas001.TFavoriteShows where username = @User and showname = @Show", new Dictionary<string, object>() { { "User", user }, { "Show", showname } });
                if (result.Count == 0)
                {
                    Connector.Execute(myConnection, "insert into ericmas001.TFavoriteShows (username,showname) values (@User,@Show)", new Dictionary<string, object>() { { "User", user }, { "Show", showname } });
                    Dictionary<string, object> show = Connector.SelectOneRow(myConnection, "select * from ericmas001.TLastEpisodes where showname = @Show", new Dictionary<string, object>() { { "Show", showname } });
                    if (show.Count == 0)
                        Connector.Execute(myConnection, "insert into ericmas001.TLastEpisodes (showname) values (@Show)", new Dictionary<string, object>() { { "Show", showname } });
                    show = Connector.SelectOneRow(myConnection, "select * from ericmas001.TShows where showname = @Show", new Dictionary<string, object>() { { "Show", showname } });
                    if (show.Count == 0)
                    {
                        WatchSeriesService service = new WatchSeriesService();
                        JObject r = JsonConvert.DeserializeObject<dynamic>(service.GetShow(showname));
                        Connector.Execute(myConnection, "insert into ericmas001.TShows (showname, showtitle) values (@Show, @Title)", new Dictionary<string, object>() { { "Show", showname }, { "Title", r["ShowTitle"].ToString() } });
                    }

                    myConnection.Close();
                    return JsonConvert.SerializeObject(new { success = true, token = token, until = tokenInfo.Value }, new IsoDateTimeConverter());
                }
                else
                {
                    myConnection.Close();
                    return JsonConvert.SerializeObject(new { success = false, problem = "Favorite already existing", token = token, until = tokenInfo.Value }, new IsoDateTimeConverter());
                }
            }
        }

        [WebGet(UriTemplate = "DelFav/{token}/{showname}")]
        public string DelFav(string token, string showname)
        {
            SqlConnection myConnection = Connector.GetConnection();
            KeyValuePair<string, DateTime> tokenInfo = GetUsernameFromToken(myConnection, token);
            string user = tokenInfo.Key;
            if (user == null)
            {
                if (myConnection != null)
                    myConnection.Close();
                return JsonConvert.SerializeObject(new { success = false, problem = "Invalid Token" });
            }
            else
            {
                Dictionary<string, object> result = Connector.SelectOneRow(myConnection, "select * from ericmas001.TFavoriteShows where username = @User and showname = @Show", new Dictionary<string, object>() { { "User", user }, { "Show", showname } });
                if (result.Count != 0)
                {
                    Connector.Execute(myConnection, "delete from ericmas001.TFavoriteShows where username = @User and showname = @Show", new Dictionary<string, object>() { { "User", user }, { "Show", showname } });
                    myConnection.Close();
                    return JsonConvert.SerializeObject(new { success = true, token = token, until = tokenInfo.Value }, new IsoDateTimeConverter());
                }
                else
                {
                    myConnection.Close();
                    return JsonConvert.SerializeObject(new { success = false, problem = "Non-existant favorite", token = token, until = tokenInfo.Value }, new IsoDateTimeConverter());
                }
            }
        }

        [WebGet(UriTemplate = "SetLastViewed/{token}/{showname}/{lastViewedSeason}/{lastViewedEpisode}")]
        public string SetLastViewed(string token, string showname, string lastViewedSeason, string lastViewedEpisode)
        {
            SqlConnection myConnection = Connector.GetConnection();
            KeyValuePair<string, DateTime> tokenInfo = GetUsernameFromToken(myConnection, token);
            string user = tokenInfo.Key;
            if (user == null)
            {
                if (myConnection != null)
                    myConnection.Close();
                return JsonConvert.SerializeObject(new { success = false, problem = "Invalid Token" });
            }
            else
            {
                Dictionary<string, object> result = Connector.SelectOneRow(myConnection, "select * from ericmas001.TFavoriteShows where username = @User and showname = @Show", new Dictionary<string, object>() { { "User", user }, { "Show", showname } });
                if (result.Count != 0)
                {
                    Connector.Execute(myConnection, "update ericmas001.TFavoriteShows set lastViewedSeason = @Season, lastViewedEpisode = @Episode where username = @User and showname = @Show", new Dictionary<string, object>() { { "User", user }, { "Show", showname }, { "Season", lastViewedSeason }, { "Episode", lastViewedEpisode } });
                    myConnection.Close();
                    return JsonConvert.SerializeObject(new { success = true, token = token, until = tokenInfo.Value }, new IsoDateTimeConverter());
                }
                else
                {
                    myConnection.Close();
                    return JsonConvert.SerializeObject(new { success = false, problem = "Non-existant favorite", token = token, until = tokenInfo.Value }, new IsoDateTimeConverter());
                }
            }
        }
    }
}