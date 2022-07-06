namespace GateWayPermissionApi.Extensions
{
    public class IdentityServerOptions
    {
        public string IP { get; set; }

        public string Port { get; set; }

        public string IdentityScheme { get; set; }

        public List<ApIResourceOptions> Resources { get; set; } 
    }
}