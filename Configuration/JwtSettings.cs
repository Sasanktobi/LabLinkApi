namespace Backend.Configuration
{
    public class JwtSettings
    {
        public const string SectionName="Jwt";

        public string Issuer{get;set;}=string.Empty;
        public string Audience{get;set;}=string.Empty;

        // HMAC-SHA256 signing key; must be at least 32 characters. Keep it out of source control in production.
        public string Key{get;set;}=string.Empty;

        public int ExpiryMinutes{get;set;}=120;
    }

    public class StorageSettings
    {
        public const string SectionName="Storage";

        // Relative paths are resolved against the content root.
        public string ReportsPath{get;set;}="Storage/Reports";
    }
}
