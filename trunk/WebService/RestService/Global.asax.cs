using RestService.Services;
using RestService.Services.Deprecated;
using System;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;

namespace RestService
{
    public class Global : HttpApplication
    {
        private void Application_Start(object sender, EventArgs e)
        {
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            const string OLD = "old/";
            const string EMC = "emc/";
            const string LABS = "labs/";

            // EricMediaCenter
            RouteTable.Routes.Add(new ServiceRoute(EMC + "Tv", new WebServiceHostFactory(), typeof(TvService)));
            RouteTable.Routes.Add(new ServiceRoute(EMC + "Movie", new WebServiceHostFactory(), typeof(MovieService)));
            RouteTable.Routes.Add(new ServiceRoute(EMC + "Users", new WebServiceHostFactory(), typeof(UsersService)));
            RouteTable.Routes.Add(new ServiceRoute(EMC + "Bot", new WebServiceHostFactory(), typeof(BotService)));
            // Old Services => Soon to be deleted, not used by anybody!
            RouteTable.Routes.Add(new ServiceRoute(EMC + OLD + "TvSchedule", new WebServiceHostFactory(), typeof(TvScheduleService)));
            RouteTable.Routes.Add(new ServiceRoute(EMC + OLD + "VideoParsing", new WebServiceHostFactory(), typeof(VideoParsingService)));
            RouteTable.Routes.Add(new ServiceRoute(EMC + OLD + "User", new WebServiceHostFactory(), typeof(UserService)));
            RouteTable.Routes.Add(new ServiceRoute(EMC + OLD + "WatchSeries", new WebServiceHostFactory(), typeof(WatchSeriesService)));
            RouteTable.Routes.Add(new ServiceRoute(EMC + OLD + "TubePlus", new WebServiceHostFactory(), typeof(TubePlusService)));
            RouteTable.Routes.Add(new ServiceRoute(EMC + OLD + "Automated", new WebServiceHostFactory(), typeof(AutomatedService)));
            RouteTable.Routes.Add(new ServiceRoute(EMC + OLD + "EpGuide", new WebServiceHostFactory(), typeof(EpGuideService)));
            RouteTable.Routes.Add(new ServiceRoute(EMC + OLD + "TvRage", new WebServiceHostFactory(), typeof(TvRageService)));

            // EricLabs
            RouteTable.Routes.Add(new ServiceRoute(LABS + "Time", new WebServiceHostFactory(), typeof(TimeService)));
        }
    }
}