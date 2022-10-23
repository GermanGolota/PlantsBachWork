namespace Plants.Infrastructure.Config
{
    public class ConnectionConfig
    {
        public string DbConnectionTemplate { get; set; }
        public string EventStoreConnection { get; set; }
    }
}
