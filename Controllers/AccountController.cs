using BE.Dtos.Account;
using BE.Model;
using BE.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]

    
    public class AccountController : ControllerBase
    {
        private readonly HospitalManagementDbContext _dbContext;
        private readonly AccountService _accountService;
        public AccountController (HospitalManagementDbContext dbContext, AccountService accountService)
        {
            _dbContext = dbContext;
            _accountService = accountService;

            // SeedAdminUser(dbContext);
        }

        // private void SeedAdminUser(HospitalManagementDbContext dbContext)
        // {
        //     // 1. Kiểm tra xem Role 'Admin' đã tồn tại chưa
        //     var adminRole = dbContext.Roles.FirstOrDefault(r => r.Name == "Admin");
        //     if (adminRole == null)
        //     {
        //         adminRole = new Role { Name = "Admin" };
        //         dbContext.Roles.Add(adminRole);
        //         dbContext.SaveChanges();
        //     }

        //     // 2. Kiểm tra xem có User nào mang Role Admin chưa
        //     bool hasAdmin = dbContext.UserRoles.Any(ur => ur.RoleId == adminRole.RoleId);

        //     if (!hasAdmin)
        //     {
        //         // 3. Tạo User gốc (Base User)
        //         var adminUser = new User
        //         {
        //             UserName = "admin",
        //             FullName = "System Administrator",
        //             Password = BCrypt.Net.BCrypt.HashPassword("admin"), // Đừng quên Hash mật khẩu!
        //             Email = "admin@medicare.com",
        //             IsActive = true,
        //             CreatedAt = DateTime.Now
        //         };
        //         dbContext.Users.Add(adminUser);
        //         dbContext.SaveChanges();

        //         // 4. Gán quyền Admin cho User vừa tạo
        //         var userRole = new UserRole
        //         {
        //             UserId = adminUser.UserId,
        //             RoleId = adminRole.RoleId
        //         };
        //         dbContext.UserRoles.Add(userRole);
        //         dbContext.SaveChanges();
        //     }
        // }

        [HttpPost("register")]
        public async Task<IActionResult> createAccount(AccountRegisterDto newaccount)
        {
            try
            {
                bool result = await _accountService.AddPatientAccount(newaccount);

                if (!result)
                {
                    return BadRequest(new {message = "Username or Email has already existed!"});
                }
                return Ok(new {message = "Đăng ký thành công! Vui lòng kiểm tra Email để xác nhận tài khoản."});
            }
            catch (Exception ex)
            {
                return BadRequest(new {message = ex.Message});
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> loginAccount(AccountLoginDto _loginAccount)
        {
            try
            {
                var res = await _accountService.CheckLogin(_loginAccount);

                if(res is null)
                {
                    return BadRequest(new {message = "Username or password incorrect."});
                }
                return Ok(res);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new {message = ex.Message});
            }
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
        {
            bool verified = await _accountService.VerifyEmailAsync(email, token);
            if (!verified)
            {
                return BadRequest(new { message = "Mã xác minh không hợp lệ hoặc tài khoản không tồn tại." });
            }
            return Ok(new { message = "Xác minh email thành công! Bạn có thể đăng nhập ngay bây giờ." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            bool result = await _accountService.ForgotPasswordAsync(dto.Email);
            if (!result)
            {
                return BadRequest(new { message = "Email không tồn tại trong hệ thống." });
            }
            return Ok(new { message = "Link đặt lại mật khẩu đã được gửi vào Gmail của bạn. Vui lòng kiểm tra hộp thư!" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            bool result = await _accountService.ResetPasswordAsync(dto.Email, dto.Token, dto.NewPassword);
            if (!result)
            {
                return BadRequest(new { message = "Mã xác nhận không hợp lệ hoặc đã hết hạn (15 phút)." });
            }
            return Ok(new { message = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập với mật khẩu mới." });
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto dto)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Không thể xác thực danh tính." });
            }

            bool success = await _accountService.UpdateProfileAsync(userId, dto);
            if (!success)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }
            return Ok(new { message = "Cập nhật thông tin thành công." });
        }
    }
}
