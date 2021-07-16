namespace OpenFL.Cloud
{
    internal class HTTPSettings
    {
        public string HostProtocol;
        public string HostName;
        public string XOriginAllow;

        public HTTPSettings()
        {
            HostProtocol = "http";
            HostName = "localhost:8080";
        }

    }
}