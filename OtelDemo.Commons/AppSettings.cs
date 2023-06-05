namespace OtelDemo.Commons
{
    public class AppSettings
    {
        public string AppName { get; set; }
        public string EnvironmentName { get; set; }
        public string OtlpEndpoint { get; set; }
        public string RedisEndpoint { get; set; }
    }
}