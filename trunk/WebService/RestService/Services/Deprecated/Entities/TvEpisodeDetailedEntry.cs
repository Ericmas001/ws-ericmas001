using System.Collections.Generic;

namespace RestService.Services.Deprecated.Entities
{
    public class TvEpisodeDetailedEntry : TvEpisodeEntry
    {
        private int m_SeasonNo;

        public int SeasonNo
        {
            get { return m_SeasonNo; }
            set { m_SeasonNo = value; }
        }

        private string m_Description;

        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }

        private string m_ShowTitle;

        public string ShowTitle
        {
            get { return m_ShowTitle; }
            set { m_ShowTitle = value; }
        }

        private List<TvWebsiteEntry> m_Links;

        public List<TvWebsiteEntry> Links
        {
            get { return m_Links; }
            set { m_Links = value; }
        }
    }
}