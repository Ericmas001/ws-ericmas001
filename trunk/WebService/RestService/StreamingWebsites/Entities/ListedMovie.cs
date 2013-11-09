using System;

namespace RestService.StreamingWebsites.Entities
{
    public class ListedMovie : IComparable<ListedMovie>, IListedStreamingItem
    {
        private string m_Name;
        private string m_Title;

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

        public int CompareTo(ListedMovie other)
        {
            return m_Title.CompareTo(other.Title);
        }
    }
}