namespace BE.Dtos.Account;

public record class AccountLoginDto(
    string? UserName,
    string? Password
);
