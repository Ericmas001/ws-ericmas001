using System;
using System.Collections.Generic;

namespace RestService.Services.Emc.Deprecated.Entities
{
    public class TvWebsiteEntry : IComparable<TvWebsiteEntry>
    {
        private string m_Name;

        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        private List<int> m_LinkIDs;

        public List<int> LinkIDs
        {
            get { return m_LinkIDs; }
            set { m_LinkIDs = value; }
        }

        #region IComparable<ScheduleEntry> Members

        public int CompareTo(TvWebsiteEntry other)
        {
            return this.Name.CompareTo(other.Name);
        }

        #endregion IComparable<ScheduleEntry> Members
    }
}