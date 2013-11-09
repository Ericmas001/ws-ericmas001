namespace RestService.StreamingWebsites.Entities
{
    public class StreamingInfo
    {
        private string m_Website;

        public string Website
        {
            get { return m_Website; }
            set { m_Website = value; }
        }

        private string m_Arguments;

        public string Arguments
        {
            get { return m_Arguments; }
            set { m_Arguments = value; }
        }

        private string m_StreamingURL;

        public string StreamingURL
        {
            get { return m_StreamingURL; }
            set { m_StreamingURL = value; }
        }

        private string m_DownloadURL;

        public string DownloadURL
        {
            get { return m_DownloadURL; }
            set { m_DownloadURL = value; }
        }
    }
}