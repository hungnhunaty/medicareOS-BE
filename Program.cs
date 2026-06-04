using System.Text;
using BE.Hubs;
using BE.Model;
using BE.Services;
using BE.Services.JWT;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HospitalManagementDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Constr"));
});

builder.Services.AddSignalR();

// builder.Services.AddCors(opt =>
// {
//     opt.AddPolicy("CorsPolicy", policy =>
//     {
//         policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200");
//     });
// });
////////
builder.WebHost.UseUrls(
    "http://0.0.0.0:5265",
    "https://0.0.0.0:7073"
);
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin();
    });
    opt.AddPolicy("SignalRCorsPolicy", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true);
    });
});
/////////
builder.Services.AddControllers();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWTConfig:Issuer"],
        ValidAudience = builder.Configuration["JWTConfig:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTConfig:SecretKey"]!)),
        RoleClaimType = "Role"
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/queueHub") || path.StartsWithSegments("/queueNotificationHub")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddScoped<TokenProvider>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<DbInitializerAdmin>();
builder.Services.AddScoped<AdminDashboardService>();
builder.Services.AddScoped<AdminStaffService>();
builder.Services.AddScoped<AdminPatientService>();
builder.Services.AddScoped<AdminMedicalServiceService>();
builder.Services.AddScoped<AdminMedicationService>();
builder.Services.AddScoped<AdminFinanceService>();
builder.Services.AddScoped<DoctorPortalService>();
builder.Services.AddScoped<StaffPortalService>();
builder.Services.AddScoped<PatientPortalService>();
builder.Services.AddScoped<QueueService>();builder.Services.AddScoped<QueueQrService>();
builder.Services.AddScoped<MedicalExaminationService>();
builder.Services.AddScoped<PrescriptionService>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<PatientService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<HospitalManagementDbContext>();
        
        // Đồng bộ lịch sử migration đầu tiên nếu các bảng đã tồn tại sẵn trong DB để tránh lỗi tạo bảng trùng lặp
        await context.Database.ExecuteSqlRawAsync(
            "IF OBJECT_ID(N'[Department]') IS NOT NULL " +
            "BEGIN " +
            "    IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL " +
            "    BEGIN " +
            "        CREATE TABLE [__EFMigrationsHistory] ( " +
            "            [MigrationId] nvarchar(150) NOT NULL, " +
            "            [ProductVersion] nvarchar(32) NOT NULL, " +
            "            CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId]) " +
            "        ); " +
            "    END; " +
            "    IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260514155632_AddClinicAndQueue') " +
            "    BEGIN " +
            "        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) " +
            "        VALUES ('20260514155632_AddClinicAndQueue', '8.0.0'); " +
            "    END; " +
            "END;");

        await context.Database.MigrateAsync();

        var initializer = scope.ServiceProvider.GetRequiredService<DbInitializerAdmin>();
        await initializer.SeedAdminDataAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Lỗi Migrate hoặc Seeding dữ liệu Admin: " + ex.Message);
    }
}

app.UseCors("CorsPolicy");

app.UseWebSockets();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<QueueHub>("/queueHub").RequireCors("SignalRCorsPolicy");
app.MapHub<QueueNotificationHub>("/queueNotificationHub").RequireCors("SignalRCorsPolicy");
// DbInitializer.Seed(app);
app.Run();
