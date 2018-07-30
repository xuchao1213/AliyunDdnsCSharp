namespace AliyunDdnsCSharp
{
    public sealed class Configuration
    {
        #region static internal class Singleton
        private Configuration(){}
        private class IntanceHolder
        {
            public static readonly Configuration Instance = new Configuration();
            static IntanceHolder(){}
        }
        public static Configuration GetIns()
        {
            return IntanceHolder.Instance;
        }
        #endregion

        public string ServiceName { get; private set; } = "AliyunDdns";
        public string ServiceDisplayName { get; private set; } = "AliyunDdns";
        public string ServiceDescription { get; private set; } = "采用阿里云API实现的DDNS客户端";




    }
}
