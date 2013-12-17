using EricUtility2011.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestService.Data.Emc2;
using System;
using System.Collections.Generic;
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
        [WebGet(UriTemplate = "Connect/{user}/{pass}")]
        public string Connect(string user, string pass)
        {
            Dictionary<string, object> p = Database.UserConnect(user, pass);

            if ((bool)p["@ok"])
                return JsonConvert.SerializeObject(new { success = true, token = (String)p["@info"], until = (DateTimeOffset)p["@validUntil"] }, new IsoDateTimeConverter());
            else
                return JsonConvert.SerializeObject(new { success = false, problem = (String)p["@info"] });
        }

        [WebGet(UriTemplate = "Register/{user}/{pass}/{email}")]
        public string Register(string user, string pass, string email)
        {
            Dictionary<string, object> p = Database.UserRegister(user, pass, email);

            if ((bool)p["@ok"])
                return JsonConvert.SerializeObject(new { success = true });
            else
                return JsonConvert.SerializeObject(new { success = false, problem = (String)p["@info"] });
        }

        [WebGet(UriTemplate = "ChangeInfo/{user}/{token}/{email}")]
        public string ChangeInfo(string user, string token, string email)
        {
            Dictionary<string, object> p = Database.UserChangeInfo(user, token, email);

            if ((bool)p["@ok"])
                return JsonConvert.SerializeObject(new { success = true, token = (String)p["@info"], until = (DateTimeOffset)p["@validUntil"] }, new IsoDateTimeConverter());
            else
                return JsonConvert.SerializeObject(new { success = false, problem = (String)p["@info"] });
        }

        [WebGet(UriTemplate = "ChangePassword/{user}/{token}/{password}")]
        public string ChangePassword(string user, string token, string password)
        {
            Dictionary<string, object> p = Database.UserChangePassword(user, token, password);

            if ((bool)p["@ok"])
                return JsonConvert.SerializeObject(new { success = true, token = (String)p["@info"], until = (DateTimeOffset)p["@validUntil"] }, new IsoDateTimeConverter());
            else
                return JsonConvert.SerializeObject(new { success = false, problem = (String)p["@info"] });
        }

        [WebGet(UriTemplate = "Me/{user}/{token}")]
        public string Me(string user, string token)
        {
            SPResult res = Database.UserGetProfile(user, token);
            Dictionary<string, object> p = res.Parameters;

            if ((bool)p["@ok"])
            {
                Dictionary<string, object> r = res.QueryResults[0];
                return JsonConvert.SerializeObject(new { success = true, username = (String)r["username"], email = (String)r["email"], token = (String)p["@info"], until = (DateTimeOffset)p["@validUntil"] }, new IsoDateTimeConverter());
            }
            else
                return JsonConvert.SerializeObject(new { success = false, problem = (String)p["@info"] });
        }
    }
}