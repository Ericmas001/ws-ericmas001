using System;
using System.Collections.Generic;

namespace RestService.Services.Deprecated.Entities
{
    public class TvShowDetailedEntry : TvShowEntry
    {
        private DateTime m_ReleaseDate;

        public DateTime ReleaseDate
        {
            get { return m_ReleaseDate; }
            set { m_ReleaseDate = value; }
        }

        private string m_Genre;

        public string Genre
        {
            get { return m_Genre; }
            set { m_Genre = value; }
        }

        private string m_Status;

        public string Status
        {
            get { return m_Status; }
            set { m_Status = value; }
        }

        private string m_Network;

        public string Network
        {
            get { return m_Network; }
            set { m_Network = value; }
        }

        private string m_Imdb;

        public string Imdb
        {
            get { return m_Imdb; }
            set { m_Imdb = value; }
        }

        private string m_Description;

        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }

        private int m_NbEpisodes;

        public int NbEpisodes
        {
            get { return m_NbEpisodes; }
            set { m_NbEpisodes = value; }
        }

        private string m_RssFeed;

        public string RssFeed
        {
            get { return m_RssFeed; }
            set { m_RssFeed = value; }
        }

        private string m_Logo;

        public string Logo
        {
            get { return m_Logo; }
            set { m_Logo = value; }
        }

        private List<TvSeasonEntry> m_Seasons = new List<TvSeasonEntry>();

        public List<TvSeasonEntry> Seasons
        {
            get { return m_Seasons; }
            set { m_Seasons = value; }
        }
    }
}