namespace FirstAPIProject.Application.Modules.Auth.DTOs
{
    public record ChangePasswordRequest(
        string CurrentPassword,
        string NewPassword);

    public record ForgotPasswordRequest(
        string Email);

    public record ForgotPasswordResponse(
        string Message,
        string? ResetToken);

    public record ResetPasswordRequest(
        string Email,
        string Token,
        string NewPassword);
}
