namespace GateWayPermissionApi.Extensions
{
    public class IdentityServerOptions
    {
        public string IP { get; set; }

        public string Port { get; set; }

        public string IdentityScheme { get; set; }

        public List<APIResource> Resources { get; set; }
    }

    public class APIResource
    {
        public string Key { get; set; }

        public string Name { get; set; }
    }
}