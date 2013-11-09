using RestService.Services.Emc.Deprecated.Entities;
using EricUtility;
using EricUtility.Networking.Gathering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace RestService.Services.Emc.Deprecated
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class TvScheduleService
    {
        [WebGet(UriTemplate = "AvailableSchedule")]
        public string AvailableSchedule()
        {
            List<KeyValuePair<string, string>> availables = new List<KeyValuePair<string, string>>();
            for (int i = -1; i < 6; ++i)
            {
                string id = i.ToString();
                DateTime d = DateTime.Now + new TimeSpan(i, 0, 0, 0);
                TimeZoneInfo tzi = TimeZoneInfo.Local;
                TimeZoneInfo tziw = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                TimeSpan tziU = tzi.BaseUtcOffset;
                TimeSpan tziwU = tziw.BaseUtcOffset;
                d = d - (tziU - tziwU);

                string label = d.ToString("dddd, MMMM dd", CultureInfo.InvariantCulture);// String.Format("{0:dddd}, {0:MMMM} {0:dd}", d);

                availables.Add(new KeyValuePair<string, string>(id, label));
            }
            return JsonConvert.SerializeObject(availables);
        }

        [WebGet(UriTemplate = "GetSchedule/{id}")]
        public string GetSchedule(string id)
        {
            string src = GatheringUtility.GetPageSource("http://watchseries.li/tvschedule/" + id);
            string content = src.Extract("<h2 class=\"listbig\">", "<!-- end of right -->");
            string lineDebut = "<a href=\"";
            string lineFin = "</a>";
            int index = content.IndexOf(lineDebut);
            Dictionary<string, ScheduleEntry> items = new Dictionary<string, ScheduleEntry>();
            List<ScheduleEntry> itemList = new List<ScheduleEntry>();
            while (index != -1)
            {
                ScheduleEntry entry = new ScheduleEntry();
                int deb = index + lineDebut.Length;
                string line = content.Substring(deb, content.IndexOf(lineFin, deb) - deb);

                entry.Url = line.Remove(line.IndexOf("\""));

                string info = line.Substring(line.IndexOf(">") + 1);
                string splitter = " - ";
                entry.ShowName = info.Remove(info.IndexOf(splitter)).Trim();
                int titleDeb = info.IndexOf(splitter) + splitter.Length;
                int numberDeb = info.LastIndexOf("(");
                entry.ShowTitle = info.Substring(titleDeb, numberDeb - titleDeb).Trim();

                string number = StringUtility.Extract(info.Substring(numberDeb), "(", ")");
                string[] nbs;
                if (number.StartsWith("S"))
                {
                    nbs = number.Substring(1).Split('-');
                    nbs[1] = "99";
                }
                else
                    nbs = number.Split('x');
                if (nbs.Length != 2)
                {
                    nbs = new string[2];
                    nbs[0] = "0";
                    nbs[1] = "99";
                }
                entry.Season = int.Parse(nbs[0]);
                entry.Episode = int.Parse(nbs[1]);
                items.Add(entry.ShowName + number, entry);
                index = content.IndexOf(lineDebut, index + 1);
            }

            string[] shows = new string[items.Count];
            items.Keys.CopyTo(shows, 0);
            Array.Sort(shows);

            foreach (string s in shows)
                itemList.Add(items[s]);

            return JsonConvert.SerializeObject(itemList);
        }
    }
}