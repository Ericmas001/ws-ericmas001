using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace RestService.Services.Labs
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class TimeService
    {
        [WebGet(UriTemplate = "CurrentTime")]
        public string CurrentTime()
        {
            //return DateTime.Now.ToString();
            return JsonConvert.SerializeObject(new { value = DateTime.Now }, new IsoDateTimeConverter());
        }
    }
}