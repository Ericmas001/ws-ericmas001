using System;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;

using emc = RestService.Services.Emc;
using emc_old = RestService.Services.Emc.Deprecated;
using labs = RestService.Services.Labs;
using lou = RestService.Services.Lou;
using house = RestService.Services.House;

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
            const string LOU = "lou/";
            const string HOUSE = "duproprio/";

            // EricMediaCenter
            RouteTable.Routes.Add(new ServiceRoute(EMC + "Tv", new WebServiceHostFactory(), typeof(emc.TvService)));
            RouteTable.Routes.Add(new ServiceRoute(EMC + "Movie", new WebServiceHostFactory(), typeof(emc.MovieService)));
            RouteTable.Routes.Add(new ServiceRoute(EMC + "Users", new WebServiceHostFactory(), typeof(emc.UsersService)));
            RouteTable.Routes.Add(new ServiceRoute(EMC + "Bot", new WebServiceHostFactory(), typeof(emc.BotService)));
            // Old Services => Soon to be deleted, not used by anybody!
            RouteTable.Routes.Add(new ServiceRoute(EMC + OLD + "TvSchedule", new WebServiceHostFactory(), typeof(emc_old.TvScheduleService)));
            RouteTable.Routes.Add(new ServiceRoute(EMC + OLD + "VideoParsing", new WebServiceHostFactory(), typeof(emc_old.VideoParsingService)));
            RouteTable.Routes.Add(new ServiceRoute(EMC + OLD + "User", new WebServiceHostFactory(), typeof(emc_old.UserService)));
            RouteTable.Routes.Add(new ServiceRoute(EMC + OLD + "WatchSeries", new WebServiceHostFactory(), typeof(emc_old.WatchSeriesService)));
            RouteTable.Routes.Add(new ServiceRoute(EMC + OLD + "TubePlus", new WebServiceHostFactory(), typeof(emc_old.TubePlusService)));
            RouteTable.Routes.Add(new ServiceRoute(EMC + OLD + "Automated", new WebServiceHostFactory(), typeof(emc_old.AutomatedService)));
            RouteTable.Routes.Add(new ServiceRoute(EMC + OLD + "EpGuide", new WebServiceHostFactory(), typeof(emc_old.EpGuideService)));
            RouteTable.Routes.Add(new ServiceRoute(EMC + OLD + "TvRage", new WebServiceHostFactory(), typeof(emc_old.TvRageService)));

            // LouMapInfo
            RouteTable.Routes.Add(new ServiceRoute(LOU + "User", new WebServiceHostFactory(), typeof(lou.UserService)));

            // DuProprio
            RouteTable.Routes.Add(new ServiceRoute(HOUSE + "User", new WebServiceHostFactory(), typeof(house.UserService)));

            // EricLabs
            RouteTable.Routes.Add(new ServiceRoute(LABS + "Time", new WebServiceHostFactory(), typeof(labs.TimeService)));
        }
    }
}