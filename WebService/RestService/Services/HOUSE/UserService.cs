using DuProprioLib.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace RestService.Services.House
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class UserService
    {
        [WebGet(UriTemplate = "Favs/{user}/{pass}")]
        public string Favs(string user, string pass)
        {
            SessionInfo m_Session = new SessionInfo(user, pass);
            IEnumerable<HouseSummary> res = m_Session.GetFavs().Result;
            return res == null ? "" : JsonConvert.SerializeObject(res);
        }

        [WebGet(UriTemplate = "House/{noAnnonce}")]
        public string House(string noAnnonce)
        {
            HouseDetailInfo res = GetHouseInfo(noAnnonce).Result;
            return res == null ? "" : JsonConvert.SerializeObject(res);
        }

        public async Task<HouseDetailInfo> GetHouseInfo(string noAnnonce)
        {
            // Get Page
            return new HouseDetailInfo(await new HttpClient().GetStringAsync("http://duproprio.com/" + noAnnonce));
        }
    }
}