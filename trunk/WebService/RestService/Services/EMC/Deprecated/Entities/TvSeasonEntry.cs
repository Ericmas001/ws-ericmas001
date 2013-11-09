using System;
using System.Collections.Generic;

namespace RestService.Services.Emc.Deprecated.Entities
{
    public class TvSeasonEntry : IComparable<TvSeasonEntry>
    {
        private int m_SeasonNo;

        public int SeasonNo
        {
            get { return m_SeasonNo; }
            set { m_SeasonNo = value; }
        }

        private int m_NbEpisodes;

        public int NbEpisodes
        {
            get { return m_NbEpisodes; }
            set { m_NbEpisodes = value; }
        }

        private string m_SeasonName;

        public string SeasonName
        {
            get { return m_SeasonName; }
            set { m_SeasonName = value; }
        }

        private List<TvEpisodeEntry> m_Episodes = new List<TvEpisodeEntry>();

        public List<TvEpisodeEntry> Episodes
        {
            get { return m_Episodes; }
            set { m_Episodes = value; }
        }

        #region IComparable<TvSeasonEntry> Members

        public int CompareTo(TvSeasonEntry other)
        {
            return this.SeasonNo.CompareTo(other.SeasonNo);
        }

        #endregion IComparable<TvSeasonEntry> Members
    }
}