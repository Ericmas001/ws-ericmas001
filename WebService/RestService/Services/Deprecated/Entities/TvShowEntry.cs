using System;

namespace RestService.Services.Deprecated.Entities
{
    public class TvShowEntry : IComparable<TvShowEntry>
    {
        private string m_ShowName;

        public string ShowName
        {
            get { return m_ShowName.Replace(':', ';'); }
            set { m_ShowName = value; }
        }

        private string m_ShowTitle;

        public string ShowTitle
        {
            get { return m_ShowTitle; }
            set { m_ShowTitle = value; }
        }

        private int m_ReleaseYear;

        public int ReleaseYear
        {
            get { return m_ReleaseYear; }
            set { m_ReleaseYear = value; }
        }

        #region IComparable<ScheduleEntry> Members

        public int CompareTo(TvShowEntry other)
        {
            return this.ShowTitle.CompareTo(other.ShowTitle);
        }

        #endregion IComparable<ScheduleEntry> Members
    }
}