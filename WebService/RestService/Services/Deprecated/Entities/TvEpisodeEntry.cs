using System;

namespace RestService.Services.Deprecated.Entities
{
    public class TvEpisodeEntry : IComparable<TvEpisodeEntry>
    {
        private int m_EpisodeNo;

        public int EpisodeNo
        {
            get { return m_EpisodeNo; }
            set { m_EpisodeNo = value; }
        }

        private int m_EpisodeId;

        public int EpisodeId
        {
            get { return m_EpisodeId; }
            set { m_EpisodeId = value; }
        }

        private string m_EpisodeName;

        public string EpisodeName
        {
            get { return m_EpisodeName.Replace(':', ';'); }
            set { m_EpisodeName = value; }
        }

        private string m_EpisodeTitle;

        public string EpisodeTitle
        {
            get { return m_EpisodeTitle; }
            set { m_EpisodeTitle = value; }
        }

        private DateTime m_ReleaseDate;

        public DateTime ReleaseDate
        {
            get { return m_ReleaseDate; }
            set { m_ReleaseDate = value; }
        }

        #region IComparable<TvSeasonEntry> Members

        public int CompareTo(TvEpisodeEntry other)
        {
            return this.EpisodeNo.CompareTo(other.EpisodeNo);
        }

        #endregion IComparable<TvSeasonEntry> Members
    }
}