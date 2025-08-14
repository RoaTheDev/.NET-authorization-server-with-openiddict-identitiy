namespace Authorization.Application.Dto.Request;

public record TwoFactorSetupDto
{
    public string SecretKey { get; init; } = string.Empty;
    public string QrCodeUrl { get; init; } = string.Empty;
    public string[] RecoveryCodes { get; init; } = [];
}