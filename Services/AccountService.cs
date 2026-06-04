using System;
using BE.Dtos.Account;
using BE.Model;
using BE.Services.JWT;
using Microsoft.EntityFrameworkCore;

namespace BE.Services;

public class AccountService{
    private readonly HospitalManagementDbContext _dbContext;
    private readonly TokenProvider _tokenProvider;
    private readonly EmailService _emailService;
    public AccountService(HospitalManagementDbContext dbContext, TokenProvider tokenProvider, EmailService emailService)
    {
        _dbContext = dbContext;
        _tokenProvider = tokenProvider;
        _emailService = emailService;
    }

    public async Task<bool> AddPatientAccount(AccountRegisterDto newaccount)
    {
        if (string.IsNullOrEmpty(newaccount.Phone))
        {
            throw new ArgumentException("Số điện thoại là bắt buộc!");
        }

        bool accountExists = await _dbContext.Users.AnyAsync(u => 
            u.UserName == newaccount.UserName || 
            (!string.IsNullOrEmpty(newaccount.Email) && u.Email == newaccount.Email)
        );
        if (accountExists)
        {
            return false;
        }

        var newUser = new User
        {
            UserName = newaccount.UserName,
            Password = BCrypt.Net.BCrypt.HashPassword(newaccount.Password),
            FullName = newaccount.FullName,
            DateOfBirth = newaccount.DateOfBirth,                
            Phone = newaccount.Phone,
            Email = newaccount.Email,
            Address = newaccount.Address,
            Gender = newaccount.Gender,    

            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            IsEmailVerified = true, // Kích hoạt ngay lập tức để không phiền người dùng
            VerificationToken = null,

            UserRoles = new List<UserRole>
            {
                new UserRole
                {
                    RoleId = 4 //User
                }
            }
        };
        try 
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();

            var newPatient = new Patient
            {
                UserId = newUser.UserId,
                PatientCode = GeneratePatientCode(newUser.UserId)
            };

            _dbContext.Patients.Add(newPatient);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return true;
        }
        catch (Exception) 
        {
            return false;
        }
    }

    private string GeneratePatientCode(int userId)
    {
        return $"PT{DateTime.Now.Year}{userId:D5}";
    }

    public async Task<LoginResponseDto?> CheckLogin(AccountLoginDto accountLoginDto)
    {
        var curAccount = await _dbContext.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
        .FirstOrDefaultAsync(x => x.UserName == accountLoginDto.UserName);

        if(curAccount is null || !BCrypt.Net.BCrypt.Verify(accountLoginDto.Password, curAccount.Password))
        {
            return null;
        }

        bool isPatient = await _dbContext.Patients.AnyAsync(p => p.UserId == curAccount.UserId);
        if (isPatient && curAccount.IsEmailVerified == false)
        {
            throw new InvalidOperationException("Tài khoản chưa được kích hoạt. Vui lòng xác thực qua email đã nhận!");
        }

        string token = _tokenProvider.Create(curAccount);

        return new LoginResponseDto
        (
            curAccount.UserId,
            curAccount.FullName,
            curAccount.UserRoles.Select(x => x.Role.Name).ToList(),
            await GetUserType(curAccount.UserId),
            token
        );
    }

    private async Task<string> GetUserType(int userId)
    {
        var isAdmin = await _dbContext.UserRoles.AnyAsync(u => u.UserId == userId && u.Role.Name == "Admin");
        if(isAdmin) return "Admin"
;        if(await _dbContext.Doctors.AnyAsync(d => d.UserId == userId)) return "Doctor";
        if(await _dbContext.Staffs.AnyAsync(s => s.UserId == userId)) return "Staff"; 
        if(await _dbContext.Patients.AnyAsync(p => p.UserId == userId)) return "Patient"; 

       return "Guest";
    }

    public async Task<bool> VerifyEmailAsync(string email, string token)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email && u.VerificationToken == token);
        if (user == null) return false;

        user.IsEmailVerified = true;
        user.VerificationToken = null;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return false;

        var resetToken = Guid.NewGuid().ToString();
        user.PasswordResetToken = resetToken;
        user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);
        await _dbContext.SaveChangesAsync();

        var resetLink = $"http://localhost:4200/reset-password?token={resetToken}&email={email}";
        var emailBody = $@"
            <h3>Medicare - Yêu cầu đặt lại mật khẩu</h3>
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
            <p>Vui lòng bấm vào liên kết dưới đây để thay đổi mật khẩu (liên kết có hiệu lực trong 15 phút):</p>
            <p><a href='{resetLink}' target='_blank' style='display:inline-block;padding:10px 20px;background-color:#e11d48;color:white;text-decoration:none;border-radius:6px;font-weight:bold;'>Đặt lại mật khẩu</a></p>
            <p>Nếu bạn không yêu cầu đổi mật khẩu, vui lòng bỏ qua email này.</p>
            <p>Đường dẫn trực tiếp: {resetLink}</p>
            <br/>
            <p>Thân ái,</p>
            <p>Medicare System</p>";

        _ = Task.Run(() => _emailService.SendEmailAsync(email, "Đặt lại mật khẩu tài khoản Medicare", emailBody));
        return true;
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email && u.PasswordResetToken == token);
        if (user == null) return false;

        if (user.ResetTokenExpiry == null || user.ResetTokenExpiry < DateTime.UtcNow)
        {
            return false;
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.ResetTokenExpiry = null;
        await _dbContext.SaveChangesAsync();
        return true;
    }
    public async Task<bool> UpdateProfileAsync(int userId, ProfileUpdateDto dto)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) return false;

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.Phone = dto.Phone;
        user.Gender = dto.Gender;
        user.DateOfBirth = dto.Dob;
        user.Address = dto.Address;

        await _dbContext.SaveChangesAsync();
        return true;
    }
}
