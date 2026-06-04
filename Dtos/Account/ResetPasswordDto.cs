namespace BE.Dtos.Account
{
    public record ResetPasswordDto(string Email, string Token, string NewPassword);
}
