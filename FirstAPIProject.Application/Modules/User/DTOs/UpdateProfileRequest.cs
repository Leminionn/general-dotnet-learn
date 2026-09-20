namespace FirstAPIProject.Application.Modules.User.DTOs
{
    public record UpdateProfileRequest(
        string? UserName,
        string? PhoneNumber);
}
