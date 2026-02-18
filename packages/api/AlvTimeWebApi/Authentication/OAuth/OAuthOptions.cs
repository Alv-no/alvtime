namespace AlvTimeWebApi.Authentication.OAuth
{
    public class OAuthOptions
    {
        public string Instance { get; set; }

        public string Domain { get; set; }

        public string TenantId { get; set; }

        public string IssuerId { get; set; }

        public string ClientId { get; set; }
        public string GraphClientSecret { get; set; }
        public string AuthCodeFlowSecret { get; set; }
    }
}

