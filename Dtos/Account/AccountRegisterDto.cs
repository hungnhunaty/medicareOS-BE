using System;

namespace BE.Dtos.Account;

// Khớp hoàn toàn với Model, chuẩn PascalCase và hỗ trợ Nullable
public record AccountRegisterDto(
    string UserName, 
    string Password, 
    string FullName,
    string? Email,
    string Phone,
    DateOnly DateOfBirth,
    string? Address, 
    string? Gender
);
