using System;

namespace RestService.Services.Emc.Deprecated.Entities
{
    public class ScheduleEntry : IComparable<ScheduleEntry>
    {
        private string m_Url;

        public string Url
        {
            get { return m_Url; }
            set { m_Url = value; }
        }

        private string m_ShowName;

        public string ShowName
        {
            get { return m_ShowName; }
            set { m_ShowName = value; }
        }

        private string m_ShowTitle;

        public string ShowTitle
        {
            get { return m_ShowTitle; }
            set { m_ShowTitle = value; }
        }

        private int m_Season;

        public int Season
        {
            get { return m_Season; }
            set { m_Season = value; }
        }

        private int m_Episode;

        public int Episode
        {
            get { return m_Episode; }
            set { m_Episode = value; }
        }

        #region IComparable<ScheduleEntry> Members

        public int CompareTo(ScheduleEntry other)
        {
            return this.ShowTitle.CompareTo(other.ShowTitle);
        }

        #endregion IComparable<ScheduleEntry> Members
    }
}