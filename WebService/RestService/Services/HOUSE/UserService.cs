using DuProprioLib.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace RestService.Services.House
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class UserService
    {
        [WebGet(UriTemplate = "Favs/{user}/{pass}")]
        public string Session(string user, string pass)
        {
            SessionInfo m_Session = new SessionInfo(user, pass);
            IEnumerable<HouseSummary> res = m_Session.GetFavs().Result;
            return res == null ? "" : JsonConvert.SerializeObject(res);
        }
    }
}