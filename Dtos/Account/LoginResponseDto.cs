using System.Collections.Generic;

namespace BE.Dtos.Account;

public record class LoginResponseDto
(
    int UserID,
    string FullName,
    List<string> Roles,
    string UserType,
    string Token
);
