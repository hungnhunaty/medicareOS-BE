using System;
using System.Linq;
using System.Threading.Tasks;
using BE.Model;
using Microsoft.EntityFrameworkCore;

namespace BE.Services;

public class DbInitializerAdmin
{
    private readonly HospitalManagementDbContext _dbContext;

    public DbInitializerAdmin(HospitalManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAdminDataAsync()
    {
        // Xóa role User cũ nếu còn tồn tại trong DB
        var userRoleOld = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (userRoleOld != null)
        {
            var userRolesMapping = await _dbContext.UserRoles.Where(ur => ur.RoleId == userRoleOld.RoleId).ToListAsync();
            _dbContext.UserRoles.RemoveRange(userRolesMapping);
            _dbContext.Roles.Remove(userRoleOld);
            await _dbContext.SaveChangesAsync();
        }

        // 1. Seed Roles
        var defaultRoles = new[] { "Admin", "Doctor", "Staff", "Pharmacist" };
        foreach (var roleName in defaultRoles)
        {
            if (!await _dbContext.Roles.AnyAsync(r => r.Name == roleName))
            {
                _dbContext.Roles.Add(new Role { Name = roleName });
            }
        }
        await _dbContext.SaveChangesAsync();

        // 2. Seed Departments
        var defaultDepts = new[] 
        { 
            "Khoa Khám Bệnh", 
            "Khoa Nội Tổng Hợp", 
            "Khoa Ngoại Tổng Hợp", 
            "Khoa Nhi", 
            "Khoa Chẩn Đoán Hình Ảnh", 
            "Khoa Xét Nghiệm" 
        };
        foreach (var deptName in defaultDepts)
        {
            if (!await _dbContext.Departments.AnyAsync(d => d.Name == deptName))
            {
                _dbContext.Departments.Add(new Department { Name = deptName });
            }
        }
        await _dbContext.SaveChangesAsync();

        // 2b. Seed Clinics (3 per department)
        var departments = await _dbContext.Departments.ToListAsync();
        foreach (var dept in departments)
        {
            if (dept.Name == "Quản trị hệ thống") continue;
            
            var hasClinics = await _dbContext.Clinics.AnyAsync(c => c.DepartmentId == dept.DepartmentId);
            if (!hasClinics)
            {
                for (int i = 1; i <= 3; i++)
                {
                    var clinicName = $"Phòng {dept.Name.Split(' ').Last()} {i:D2}";
                    _dbContext.Clinics.Add(new Clinic
                    {
                        Name = clinicName,
                        DepartmentId = dept.DepartmentId,
                        IsActive = true
                    });
                }
            }
        }
        await _dbContext.SaveChangesAsync();

        // 3. Seed Admin User
        var adminRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole != null && !await _dbContext.Users.AnyAsync(u => u.UserName == "admin"))
        {
            var adminUser = new User
            {
                UserName = "admin",
                FullName = "Quản Trị Viên Hệ Thống",
                Password = BCrypt.Net.BCrypt.HashPassword("admin123"), // Mật khẩu mặc định
                Email = "admin@medicare.vn",
                Phone = "0900111222",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Users.Add(adminUser);
            await _dbContext.SaveChangesAsync();

            _dbContext.UserRoles.Add(new UserRole
            {
                UserId = adminUser.UserId,
                RoleId = adminRole.RoleId
            });
            await _dbContext.SaveChangesAsync();
        }

        // 4. Seed Sample Services
        var sampleServices = new[]
        {
            new { Name = "Khám chuyên khoa Nội", Price = 150000m },
            new { Name = "Khám chuyên khoa Ngoại", Price = 150000m },
            new { Name = "Khám chuyên khoa Nhi", Price = 120000m },
            new { Name = "Siêu âm ổ bụng 4D", Price = 350000m },
            new { Name = "Nội soi thực quản", Price = 750000m },
            new { Name = "Xét nghiệm máu tổng quát", Price = 420000m },
            new { Name = "Chụp X-Quang kỹ thuật số", Price = 200000m }
        };

        foreach (var s in sampleServices)
        {
            if (!await _dbContext.Services.AnyAsync(x => x.Name == s.Name))
            {
                _dbContext.Services.Add(new Service { Name = s.Name, CurrentPrice = s.Price });
            }
        }
        await _dbContext.SaveChangesAsync();

        // 5. Seed Sample Medications
        var sampleMeds = new[]
        {
            new { Name = "Paracetamol 500mg", Ingredient = "Paracetamol", Unit = "Hộp", Price = 40000m, Qty = 1250 },
            new { Name = "Amoxicillin 500mg", Ingredient = "Amoxicillin", Unit = "Hộp", Price = 85000m, Qty = 850 },
            new { Name = "Omeprazole 20mg", Ingredient = "Omeprazole", Unit = "Vỉ", Price = 35000m, Qty = 210 }, // Dưới 300 để test cảnh báo
            new { Name = "Vitamin C 1000mg", Ingredient = "Acid Ascorbic", Unit = "Lọ", Price = 60000m, Qty = 500 },
            new { Name = "Nước muối sinh lý 0.9%", Ingredient = "Natri Clorid", Unit = "Chai", Price = 12000m, Qty = 150 } // Dưới 300
        };

        foreach (var m in sampleMeds)
        {
            if (!await _dbContext.Medications.AnyAsync(x => x.Name == m.Name))
            {
                _dbContext.Medications.Add(new Medication 
                { 
                    Name = m.Name, 
                    Ingredient = m.Ingredient, 
                    Unit = m.Unit, 
                    CurentPrice = m.Price, 
                    Quantity = m.Qty 
                });
            }
        }
        await _dbContext.SaveChangesAsync();

        // 6. Seed Sample Staff / Doctor (nếu chưa có)
        var examDept = await _dbContext.Departments.FirstOrDefaultAsync(d => d.Name == "Khoa Khám Bệnh");
        var doctorRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Doctor");
        if (examDept != null && doctorRole != null && !await _dbContext.Users.AnyAsync(u => u.UserName == "doctor1"))
        {
            var docUser = new User
            {
                UserName = "doctor1",
                FullName = "BS. Nguyễn Trí Thức",
                Password = BCrypt.Net.BCrypt.HashPassword("doctor123"),
                Email = "doctor1@medicare.vn",
                Phone = "0912345678",
                Gender = "Nam",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Users.Add(docUser);
            await _dbContext.SaveChangesAsync();

            _dbContext.UserRoles.Add(new UserRole { UserId = docUser.UserId, RoleId = doctorRole.RoleId });

            var docStaff = new Staff
            {
                UserId = docUser.UserId,
                EmployeeCode = "EMP-DOC-001",
                DepartmentId = examDept.DepartmentId,
                BaseSalary = 18000000m
            };
            _dbContext.Staffs.Add(docStaff);
            await _dbContext.SaveChangesAsync();

            _dbContext.Doctors.Add(new Doctor
            {
                UserId = docUser.UserId,
                Specialty = "Nội khoa",
                LicenseNumber = "CCHN-0019293",
                CommissionRate = 5.0m
            });
            await _dbContext.SaveChangesAsync();
        }

        // 6.5 Seed Sample Staff (Receptionist)
        var staffRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Staff");
        if (examDept != null && staffRole != null && !await _dbContext.Users.AnyAsync(u => u.UserName == "staff1"))
        {
            var stUser = new User
            {
                UserName = "staff1",
                FullName = "Lễ tân Nguyễn Thu Hà",
                Password = BCrypt.Net.BCrypt.HashPassword("staff123"),
                Email = "staff1@medicare.vn",
                Phone = "0922333444",
                Gender = "Nữ",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Users.Add(stUser);
            await _dbContext.SaveChangesAsync();

            _dbContext.UserRoles.Add(new UserRole { UserId = stUser.UserId, RoleId = staffRole.RoleId });

            var stStaff = new Staff
            {
                UserId = stUser.UserId,
                EmployeeCode = "EMP-REC-001",
                DepartmentId = examDept.DepartmentId,
                BaseSalary = 12000000m
            };
            _dbContext.Staffs.Add(stStaff);
            await _dbContext.SaveChangesAsync();
        }

        // 7. Seed Sample Patient
        if (!await _dbContext.Users.AnyAsync(u => u.UserName == "patient1"))
        {
            var patUser = new User
            {
                UserName = "patient1",
                FullName = "Trần Thanh Bình",
                Password = BCrypt.Net.BCrypt.HashPassword("patient123"),
                Email = "binh.tran@gmail.com",
                Phone = "0987654321",
                Gender = "Nữ",
                Address = "Quận 1, TP. HCM",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Users.Add(patUser);
            await _dbContext.SaveChangesAsync();

            var userRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole != null)
            {
                _dbContext.UserRoles.Add(new UserRole { UserId = patUser.UserId, RoleId = userRole.RoleId });
            }

            var patient = new Patient
            {
                UserId = patUser.UserId,
                PatientCode = "BN-2026-00001",
                BloodType = "O+",
                Allergies = "Không có",
                FamilyMedicalHistory = "Khỏe mạnh bình thường"
            };
            _dbContext.Patients.Add(patient);
            await _dbContext.SaveChangesAsync();

            // Thêm 1 lượt khám mẫu (Hàng đợi)
            var activeDoc = await _dbContext.Doctors.FirstOrDefaultAsync();
            if (activeDoc != null && !await _dbContext.MedicalExaminations.AnyAsync(m => m.PatientId == patient.UserId))
            {
                _dbContext.MedicalExaminations.Add(new MedicalExamination
                {
                    PatientId = patient.UserId,
                    DoctorId = activeDoc.UserId,
                    VisitDate = DateTime.UtcNow,
                    Symptoms = "Đau đầu, chóng mặt kéo dài",
                    BloodPressure = "120/80",
                    HeartRate = 75,
                    Temperature = 37.2m,
                    Weight = 58.5m,
                    Height = 1.65m,
                    Status = 0 // Chờ khám
                });
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
