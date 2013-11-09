using LouMapInfo.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace RestService.Services.Lou
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class UserService
    {
        [WebGet(UriTemplate = "Session/{user}/{pass}")]
        public string Session(string user, string pass)
        {
            return JsonConvert.SerializeObject(new { SessionID = SessionInfo.GetSessionID(SessionInfo.ConnectToLoU(user, pass)) });
        }
    }
}