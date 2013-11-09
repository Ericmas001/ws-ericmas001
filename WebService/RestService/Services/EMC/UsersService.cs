using EricUtility2011.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace RestService.Services.Emc
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class UsersService
    {
        private SqlServerConnector Connector = new SqlServerConnector("TURNSOL.arvixe.com", "emc2", "emc.webservice", "Emc42FTW");

        [WebGet(UriTemplate = "Connect/{user}/{pass}")]
        public string Connect(string user, string pass)
        {
            try
            {
                Dictionary<string, object> p = Connector.ExecuteSP("ericmas001.SPUserConnect", new List<SPParam>
                {
                        new SPParam(new SqlParameter("@username", SqlDbType.VarChar, 50),user),
                        new SPParam(new SqlParameter("@password", SqlDbType.VarChar, 50),pass),
                        new SPParam(new SqlParameter("@ok", SqlDbType.Bit),ParamDir.Output),
                        new SPParam(new SqlParameter("@info", SqlDbType.VarChar, 100),ParamDir.Output),
                        new SPParam(new SqlParameter("@validUntil", SqlDbType.DateTimeOffset),ParamDir.Output),
                }).Parameters;

                if ((bool)p["@ok"])
                    return JsonConvert.SerializeObject(new { success = true, token = (String)p["@info"], until = (DateTimeOffset)p["@validUntil"] }, new IsoDateTimeConverter());
                else
                    return JsonConvert.SerializeObject(new { success = false, problem = (String)p["@info"] });
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new { success = false, problem = e.ToString() });
            }
        }

        [WebGet(UriTemplate = "Register/{user}/{pass}/{email}")]
        public string Register(string user, string pass, string email)
        {
            try
            {
                Dictionary<string, object> p = Connector.ExecuteSP("ericmas001.SPUserRegister", new List<SPParam>
                {
                        new SPParam(new SqlParameter("@username", SqlDbType.VarChar, 50),user),
                        new SPParam(new SqlParameter("@password", SqlDbType.VarChar, 50),pass),
                        new SPParam(new SqlParameter("@email", SqlDbType.VarChar, 100),email),
                        new SPParam(new SqlParameter("@ok", SqlDbType.Bit),ParamDir.Output),
                        new SPParam(new SqlParameter("@info", SqlDbType.VarChar, 100),ParamDir.Output),
                }).Parameters;

                if ((bool)p["@ok"])
                    return JsonConvert.SerializeObject(new { success = true });
                else
                    return JsonConvert.SerializeObject(new { success = false, problem = (String)p["@info"] });
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new { success = false, problem = e.ToString() });
            }
        }

        [WebGet(UriTemplate = "ChangeInfo/{user}/{token}/{email}")]
        public string ChangeInfo(string user, string token, string email)
        {
            try
            {
                Dictionary<string, object> p = Connector.ExecuteSP("ericmas001.SPUserChangeInfo", new List<SPParam>
                {
                        new SPParam(new SqlParameter("@username", SqlDbType.VarChar, 50),user),
                        new SPParam(new SqlParameter("@session", SqlDbType.VarChar, 32),token),
                        new SPParam(new SqlParameter("@email", SqlDbType.VarChar, 100),email),
                        new SPParam(new SqlParameter("@ok", SqlDbType.Bit),ParamDir.Output),
                        new SPParam(new SqlParameter("@info", SqlDbType.VarChar, 100),ParamDir.Output),
                        new SPParam(new SqlParameter("@validUntil", SqlDbType.DateTimeOffset),ParamDir.Output),
                }).Parameters;

                if ((bool)p["@ok"])
                    return JsonConvert.SerializeObject(new { success = true, token = (String)p["@info"], until = (DateTimeOffset)p["@validUntil"] }, new IsoDateTimeConverter());
                else
                    return JsonConvert.SerializeObject(new { success = false, problem = (String)p["@info"] });
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new { success = false, problem = e.ToString() });
            }
        }

        [WebGet(UriTemplate = "ChangePassword/{user}/{token}/{password}")]
        public string ChangePassword(string user, string token, string password)
        {
            try
            {
                Dictionary<string, object> p = Connector.ExecuteSP("ericmas001.SPUserChangePassword", new List<SPParam>
                {
                        new SPParam(new SqlParameter("@username", SqlDbType.VarChar, 50),user),
                        new SPParam(new SqlParameter("@session", SqlDbType.VarChar, 32),token),
                        new SPParam(new SqlParameter("@password", SqlDbType.VarChar, 50),password),
                        new SPParam(new SqlParameter("@ok", SqlDbType.Bit),ParamDir.Output),
                        new SPParam(new SqlParameter("@info", SqlDbType.VarChar, 100),ParamDir.Output),
                        new SPParam(new SqlParameter("@validUntil", SqlDbType.DateTimeOffset),ParamDir.Output),
                }).Parameters;

                if ((bool)p["@ok"])
                    return JsonConvert.SerializeObject(new { success = true, token = (String)p["@info"], until = (DateTimeOffset)p["@validUntil"] }, new IsoDateTimeConverter());
                else
                    return JsonConvert.SerializeObject(new { success = false, problem = (String)p["@info"] });
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new { success = false, problem = e.ToString() });
            }
        }

        [WebGet(UriTemplate = "Me/{user}/{token}")]
        public string Me(string user, string token)
        {
            try
            {
                SPResult res = Connector.SelectRowsSP("ericmas001.SPUserGetInfo", new List<SPParam>
                {
                        new SPParam(new SqlParameter("@username", SqlDbType.VarChar, 50),user),
                        new SPParam(new SqlParameter("@session", SqlDbType.VarChar, 32),token),
                        new SPParam(new SqlParameter("@ok", SqlDbType.Bit),ParamDir.Output),
                        new SPParam(new SqlParameter("@info", SqlDbType.VarChar, 100),ParamDir.Output),
                        new SPParam(new SqlParameter("@validUntil", SqlDbType.DateTimeOffset),ParamDir.Output),
                });

                Dictionary<string, object> p = res.Parameters;

                if ((bool)p["@ok"])
                {
                    Dictionary<string, object> r = res.QueryResults[0];
                    return JsonConvert.SerializeObject(new { success = true, username = (String)r["username"], email = (String)r["email"], token = (String)p["@info"], until = (DateTimeOffset)p["@validUntil"] }, new IsoDateTimeConverter());
                }
                else
                    return JsonConvert.SerializeObject(new { success = false, problem = (String)p["@info"] });
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new { success = false, problem = e.ToString() });
            }
        }
    }
}