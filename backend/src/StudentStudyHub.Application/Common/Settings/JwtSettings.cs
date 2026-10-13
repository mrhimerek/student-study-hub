namespace StudentStudyHub.Application.Common.Settings;

/// <summary>
/// Bound from configuration ("Jwt" section in local.settings.json / Azure App Settings,
/// with the Secret itself sourced from Azure Key Vault in production).
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "StudentStudyHub";
    public string Audience { get; set; } = "StudentStudyHub.Client";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 30;
}
