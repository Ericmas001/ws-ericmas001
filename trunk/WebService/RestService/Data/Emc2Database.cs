using EricUtility2011.Data;
using RestService.StreamingWebsites.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace RestService.Data
{
    public static class Emc2Database
    {
        private static SqlServerConnector Connector = new SqlServerConnector("TURNSOL.arvixe.com", "emc2", "emc.webservice", "Emc42FTW");

        private static string GetWebsiteAndLang(string website, string lang)
        {
            if (lang != "en")
                return "|" + lang + "|" + website;
            return website;
        }

        public static Dictionary<string, object> TvShowUpdateLastEpisode(TvShow show, string website, string name)
        {
            try
            {
                Dictionary<string, object> p = Connector.ExecuteSP("ericmas001.SPTvLastEpisode", new List<SPParam>
                    {
                        new SPParam(new SqlParameter("@website", SqlDbType.VarChar, 50),website),
                        new SPParam(new SqlParameter("@name", SqlDbType.VarChar, 50),name),
                        new SPParam(new SqlParameter("@lastSeason", SqlDbType.Int),show.NoLastSeason),
                        new SPParam(new SqlParameter("@lastEpisode", SqlDbType.Int),show.NoLastEpisode),
                        new SPParam(new SqlParameter("@ok", SqlDbType.Bit),ParamDir.Output),
                        new SPParam(new SqlParameter("@info", SqlDbType.VarChar, 100),ParamDir.Output),
                    }).Parameters;
                return p;
            }
            catch (Exception e)
            {
                Dictionary<string, object> p = new Dictionary<string, object>();
                p.Add("@ok", false);
                p.Add("@info", e.ToString());
                return p;
            }
        }

        public static SPResult UserGetAllFavTvShows(string user, string token)
        {
            try
            {
                return Connector.SelectRowsSP("ericmas001.SPUserAllFavs", new List<SPParam>
                {
                        new SPParam(new SqlParameter("@username", SqlDbType.VarChar, 50),user),
                        new SPParam(new SqlParameter("@session", SqlDbType.VarChar, 32),token),
                        new SPParam(new SqlParameter("@ok", SqlDbType.Bit),ParamDir.Output),
                        new SPParam(new SqlParameter("@info", SqlDbType.VarChar, 100),ParamDir.Output),
                        new SPParam(new SqlParameter("@validUntil", SqlDbType.DateTimeOffset),ParamDir.Output),
                });
            }
            catch (Exception e)
            {
                SPResult spr = new SPResult();
                spr.Parameters.Add("@ok", false);
                spr.Parameters.Add("@info", e.ToString());
                return spr;
            }
        }

        public static Dictionary<string, object> UserSetLastViewedTvEpisode(string user, string token, string website, string lang, string showname, string lastviewedseason, string lastviewedepisode)
        {
            try
            {
                return Connector.ExecuteSP("ericmas001.SPFavLastViewed", new List<SPParam>
                {
                        new SPParam(new SqlParameter("@username", SqlDbType.VarChar, 50),user),
                        new SPParam(new SqlParameter("@session", SqlDbType.VarChar, 32),token),
                        new SPParam(new SqlParameter("@website", SqlDbType.VarChar, 50),GetWebsiteAndLang(website,lang)),
                        new SPParam(new SqlParameter("@name", SqlDbType.VarChar, 50),showname),
                        new SPParam(new SqlParameter("@lastViewedSeason", SqlDbType.Int),int.Parse(lastviewedseason)),
                        new SPParam(new SqlParameter("@lastViewedEpisode", SqlDbType.Int),int.Parse(lastviewedepisode)),
                        new SPParam(new SqlParameter("@ok", SqlDbType.Bit),ParamDir.Output),
                        new SPParam(new SqlParameter("@info", SqlDbType.VarChar, 100),ParamDir.Output),
                        new SPParam(new SqlParameter("@validUntil", SqlDbType.DateTimeOffset),ParamDir.Output),
                }).Parameters;
            }
            catch (Exception e)
            {
                Dictionary<string, object> p = new Dictionary<string, object>();
                p.Add("@ok", false);
                p.Add("@info", e.ToString());
                return p;
            }
        }

        public static Dictionary<string, object> UserDelFavTvShow(string user, string token, string website, string lang, string showname)
        {
            try
            {
                return Connector.ExecuteSP("ericmas001.SPFavDelShow", new List<SPParam>
                {
                        new SPParam(new SqlParameter("@username", SqlDbType.VarChar, 50),user),
                        new SPParam(new SqlParameter("@session", SqlDbType.VarChar, 32),token),
                        new SPParam(new SqlParameter("@website", SqlDbType.VarChar, 50),GetWebsiteAndLang(website,lang)),
                        new SPParam(new SqlParameter("@name", SqlDbType.VarChar, 50),showname),
                        new SPParam(new SqlParameter("@ok", SqlDbType.Bit),ParamDir.Output),
                        new SPParam(new SqlParameter("@info", SqlDbType.VarChar, 100),ParamDir.Output),
                        new SPParam(new SqlParameter("@validUntil", SqlDbType.DateTimeOffset),ParamDir.Output),
                }).Parameters;
            }
            catch (Exception e)
            {
                Dictionary<string, object> p = new Dictionary<string, object>();
                p.Add("@ok", false);
                p.Add("@info", e.ToString());
                return p;
            }
        }

        public static Dictionary<string, object> UserAddFavTvShow(string user, string token, string lang, string website, string showname, string showtitle, string lastseason, string lastepisode)
        {
            try
            {
                return Connector.ExecuteSP("ericmas001.SPFavAddShow", new List<SPParam>
                {
                        new SPParam(new SqlParameter("@username", SqlDbType.VarChar, 50),user),
                        new SPParam(new SqlParameter("@session", SqlDbType.VarChar, 32),token),
                        new SPParam(new SqlParameter("@website", SqlDbType.VarChar, 50),GetWebsiteAndLang(website,lang)),
                        new SPParam(new SqlParameter("@name", SqlDbType.VarChar, 50),showname),
                        new SPParam(new SqlParameter("@title", SqlDbType.VarChar, 100),showtitle),
                        new SPParam(new SqlParameter("@lastSeason", SqlDbType.Int),int.Parse(lastseason)),
                        new SPParam(new SqlParameter("@lastEpisode", SqlDbType.Int),int.Parse(lastepisode)),
                        new SPParam(new SqlParameter("@ok", SqlDbType.Bit),ParamDir.Output),
                        new SPParam(new SqlParameter("@info", SqlDbType.VarChar, 100),ParamDir.Output),
                        new SPParam(new SqlParameter("@validUntil", SqlDbType.DateTimeOffset),ParamDir.Output),
                }).Parameters;
            }
            catch (Exception e)
            {
                Dictionary<string, object> p = new Dictionary<string, object>();
                p.Add("@ok", false);
                p.Add("@info", e.ToString());
                return p;
            }
        }

        public static Dictionary<string, object> UserConnect(string user, string pass)
        {
            try
            {
                return Connector.ExecuteSP("ericmas001.SPUserConnect", new List<SPParam>
                {
                        new SPParam(new SqlParameter("@username", SqlDbType.VarChar, 50),user),
                        new SPParam(new SqlParameter("@password", SqlDbType.VarChar, 50),pass),
                        new SPParam(new SqlParameter("@ok", SqlDbType.Bit),ParamDir.Output),
                        new SPParam(new SqlParameter("@info", SqlDbType.VarChar, 100),ParamDir.Output),
                        new SPParam(new SqlParameter("@validUntil", SqlDbType.DateTimeOffset),ParamDir.Output),
                }).Parameters;
            }
            catch (Exception e)
            {
                Dictionary<string, object> p = new Dictionary<string, object>();
                p.Add("@ok", false);
                p.Add("@info", e.ToString());
                return p;
            }
        }

        public static Dictionary<string, object> UserRegister(string user, string pass, string email)
        {
            try
            {
                return Connector.ExecuteSP("ericmas001.SPUserRegister", new List<SPParam>
                {
                        new SPParam(new SqlParameter("@username", SqlDbType.VarChar, 50),user),
                        new SPParam(new SqlParameter("@password", SqlDbType.VarChar, 50),pass),
                        new SPParam(new SqlParameter("@email", SqlDbType.VarChar, 100),email),
                        new SPParam(new SqlParameter("@ok", SqlDbType.Bit),ParamDir.Output),
                        new SPParam(new SqlParameter("@info", SqlDbType.VarChar, 100),ParamDir.Output),
                }).Parameters;
            }
            catch (Exception e)
            {
                Dictionary<string, object> p = new Dictionary<string, object>();
                p.Add("@ok", false);
                p.Add("@info", e.ToString());
                return p;
            }
        }

        public static Dictionary<string, object> UserChangeInfo(string user, string token, string email)
        {
            try
            {
                return Connector.ExecuteSP("ericmas001.SPUserChangeInfo", new List<SPParam>
                {
                        new SPParam(new SqlParameter("@username", SqlDbType.VarChar, 50),user),
                        new SPParam(new SqlParameter("@session", SqlDbType.VarChar, 32),token),
                        new SPParam(new SqlParameter("@email", SqlDbType.VarChar, 100),email),
                        new SPParam(new SqlParameter("@ok", SqlDbType.Bit),ParamDir.Output),
                        new SPParam(new SqlParameter("@info", SqlDbType.VarChar, 100),ParamDir.Output),
                        new SPParam(new SqlParameter("@validUntil", SqlDbType.DateTimeOffset),ParamDir.Output),
                }).Parameters;
            }
            catch (Exception e)
            {
                Dictionary<string, object> p = new Dictionary<string, object>();
                p.Add("@ok", false);
                p.Add("@info", e.ToString());
                return p;
            }
        }

        public static Dictionary<string, object> UserChangePassword(string user, string token, string password)
        {
            try
            {
                return Connector.ExecuteSP("ericmas001.SPUserChangePassword", new List<SPParam>
                {
                        new SPParam(new SqlParameter("@username", SqlDbType.VarChar, 50),user),
                        new SPParam(new SqlParameter("@session", SqlDbType.VarChar, 32),token),
                        new SPParam(new SqlParameter("@password", SqlDbType.VarChar, 50),password),
                        new SPParam(new SqlParameter("@ok", SqlDbType.Bit),ParamDir.Output),
                        new SPParam(new SqlParameter("@info", SqlDbType.VarChar, 100),ParamDir.Output),
                        new SPParam(new SqlParameter("@validUntil", SqlDbType.DateTimeOffset),ParamDir.Output),
                }).Parameters;
            }
            catch (Exception e)
            {
                Dictionary<string, object> p = new Dictionary<string, object>();
                p.Add("@ok", false);
                p.Add("@info", e.ToString());
                return p;
            }
        }

        public static SPResult UserGetProfile(string user, string token)
        {
            try
            {
                return Connector.SelectRowsSP("ericmas001.SPUserGetInfo", new List<SPParam>
                {
                        new SPParam(new SqlParameter("@username", SqlDbType.VarChar, 50),user),
                        new SPParam(new SqlParameter("@session", SqlDbType.VarChar, 32),token),
                        new SPParam(new SqlParameter("@ok", SqlDbType.Bit),ParamDir.Output),
                        new SPParam(new SqlParameter("@info", SqlDbType.VarChar, 100),ParamDir.Output),
                        new SPParam(new SqlParameter("@validUntil", SqlDbType.DateTimeOffset),ParamDir.Output),
                });
            }
            catch (Exception e)
            {
                SPResult spr = new SPResult();
                spr.Parameters.Add("@ok", false);
                spr.Parameters.Add("@info", e.ToString());
                return spr;
            }
        }
    }
}