namespace TechfinCase.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "TechfinCase";
    public string Audience { get; set; } = "TechfinCase";
    public string Key { get; set; } = string.Empty;
}
