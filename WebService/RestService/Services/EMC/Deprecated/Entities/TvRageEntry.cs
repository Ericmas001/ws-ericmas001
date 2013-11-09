using System;
using System.Collections.Generic;

namespace RestService.Services.Emc.Deprecated.Entities
{
    public class TvRageEntry
    {
        public class EpisodeInfo
        {
            private int m_AbsoluteNo;

            public int AbsoluteNo
            {
                get { return m_AbsoluteNo; }
                set { m_AbsoluteNo = value; }
            }

            private int m_RelativeNo;

            public int RelativeNo
            {
                get { return m_RelativeNo; }
                set { m_RelativeNo = value; }
            }

            private int m_Season;

            public int Season
            {
                get { return m_Season; }
                set { m_Season = value; }
            }

            private DateTime m_AirDate;

            public DateTime AirDate
            {
                get { return m_AirDate; }
                set { m_AirDate = value; }
            }

            private int m_EpId;

            public int EpId
            {
                get { return m_EpId; }
                set { m_EpId = value; }
            }

            private string m_Title;

            public string Title
            {
                get { return m_Title; }
                set { m_Title = value; }
            }
        }

        private string m_Title;

        public string Title
        {
            get { return m_Title; }
            set { m_Title = value; }
        }

        private int m_NbSeasons;

        public int NbSeasons
        {
            get { return m_NbSeasons; }
            set { m_NbSeasons = value; }
        }

        private int m_Id;

        public int Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }

        private string m_Url;

        public string Url
        {
            get { return m_Url; }
            set { m_Url = value; }
        }

        private DateTime m_DateStarted;

        public DateTime DateStarted
        {
            get { return m_DateStarted; }
            set { m_DateStarted = value; }
        }

        private DateTime m_DateEnded;

        public DateTime DateEnded
        {
            get { return m_DateEnded; }
            set { m_DateEnded = value; }
        }

        private string m_ImageUrl;

        public string ImageUrl
        {
            get { return m_ImageUrl; }
            set { m_ImageUrl = value; }
        }

        private string m_Country;

        public string Country
        {
            get { return m_Country; }
            set { m_Country = value; }
        }

        private string m_Status;

        public string Status
        {
            get { return m_Status; }
            set { m_Status = value; }
        }

        private string m_Classification;

        public string Classification
        {
            get { return m_Classification; }
            set { m_Classification = value; }
        }

        private IEnumerable<string> m_Genres;

        public IEnumerable<string> Genres
        {
            get { return m_Genres; }
            set { m_Genres = value; }
        }

        private int m_Runtime;

        public int Runtime
        {
            get { return m_Runtime; }
            set { m_Runtime = value; }
        }

        private string m_Network;

        public string Network
        {
            get { return m_Network; }
            set { m_Network = value; }
        }

        private DateTime m_AirTime;

        public DateTime AirTime
        {
            get { return m_AirTime; }
            set { m_AirTime = value; }
        }

        private string m_AirDay;

        public string AirDay
        {
            get { return m_AirDay; }
            set { m_AirDay = value; }
        }

        private string m_TimeZone;

        public string TimeZone
        {
            get { return m_TimeZone; }
            set { m_TimeZone = value; }
        }

        private EpisodeInfo m_LastEpisode;

        public EpisodeInfo LastEpisode
        {
            get { return m_LastEpisode; }
            set { m_LastEpisode = value; }
        }

        private EpisodeInfo m_NextEpisode;

        public EpisodeInfo NextEpisode
        {
            get { return m_NextEpisode; }
            set { m_NextEpisode = value; }
        }
    }
}