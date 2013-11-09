using System;

namespace RestService.StreamingWebsites.Entities
{
    public class ListedEpisode : IComparable<ListedEpisode>, IListedStreamingItem
    {
        private int m_NoSeason;
        private int m_NoEpisode;
        private string m_Name;
        private string m_Title;
        private DateTime m_ReleaseDate;

        public int NoSeason
        {
            get { return m_NoSeason; }
            set { m_NoSeason = value; }
        }

        public int NoEpisode
        {
            get { return m_NoEpisode; }
            set { m_NoEpisode = value; }
        }

        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public string Title
        {
            get { return m_Title; }
            set { m_Title = value; }
        }

        public DateTime ReleaseDate
        {
            get { return m_ReleaseDate; }
            set { m_ReleaseDate = value; }
        }

        public int CompareTo(ListedEpisode other)
        {
            int res = NoSeason.CompareTo(other.NoSeason);
            if (res == 0)
                res = NoEpisode.CompareTo(other.NoEpisode);
            if (res == 0)
                res = ReleaseDate.CompareTo(other.ReleaseDate);
            return res;
        }
    }
}