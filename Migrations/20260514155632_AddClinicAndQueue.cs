using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BE.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicAndQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Department",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Departme__B2079BEDADDDB927", x => x.DepartmentId);
                });

            migrationBuilder.CreateTable(
                name: "Medications",
                columns: table => new
                {
                    MedicationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Ingredient = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CurentPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Medicati__62EC1AFA1A13840B", x => x.MedicationId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Roles__8AFACE1A1FFA2060", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    ServiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Services__C51BB00A3B1236B0", x => x.ServiceId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Password = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__1788CC4C36FD5F7F", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Clinic",
                columns: table => new
                {
                    ClinicId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clinic", x => x.ClinicId);
                    table.ForeignKey(
                        name: "FK_Clinic_Department",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "DepartmentId");
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PatientCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    BloodType = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    Allergies = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FamilyMedicalHistory = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Patients__1788CC4CCDD88B0A", x => x.UserId);
                    table.ForeignKey(
                        name: "FK__Patients__UserId__35BCFE0A",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Staffs",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EmployeeCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    BaseSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Staffs__1788CC4CE3829BDD", x => x.UserId);
                    table.ForeignKey(
                        name: "FK__Staffs__Departme__3A81B327",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "DepartmentId");
                    table.ForeignKey(
                        name: "FK__Staffs__UserId__398D8EEE",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserRole__3214EC0724C9533F", x => x.Id);
                    table.ForeignKey(
                        name: "FK__UserRoles__RoleI__31EC6D26",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId");
                    table.ForeignKey(
                        name: "FK__UserRoles__UserI__30F848ED",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Specialty = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LicenseNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    CommissionRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Doctors__1788CC4C38250ED5", x => x.UserId);
                    table.ForeignKey(
                        name: "FK__Doctors__UserId__3D5E1FD2",
                        column: x => x.UserId,
                        principalTable: "Staffs",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Payrolls",
                columns: table => new
                {
                    PayrollId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    PayPeriod = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Allowances = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0m),
                    Bonuses = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0m),
                    Deductions = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0m),
                    NetPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Payrolls__99DFC67230D1A5E0", x => x.PayrollId);
                    table.ForeignKey(
                        name: "FK__Payrolls__StaffI__59FA5E80",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "MedicalExaminations",
                columns: table => new
                {
                    MedicalExaminationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    VisitDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Symptoms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Diagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TreatmentPlan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BloodPressure = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    HeartRate = table.Column<int>(type: "int", nullable: true),
                    Temperature = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Height = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    ClinicId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MedicalE__60CB94EC9DB8D944", x => x.MedicalExaminationId);
                    table.ForeignKey(
                        name: "FK_MedicalExamination_Clinic",
                        column: x => x.ClinicId,
                        principalTable: "Clinic",
                        principalColumn: "ClinicId");
                    table.ForeignKey(
                        name: "FK__MedicalEx__Docto__46E78A0C",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK__MedicalEx__Patie__45F365D3",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    InvoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicalExaminationId = table.Column<int>(type: "int", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    CashierId = table.Column<int>(type: "int", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Invoices__D796AAB5B4865AF0", x => x.InvoiceId);
                    table.ForeignKey(
                        name: "FK__Invoices__Cashie__628FA481",
                        column: x => x.CashierId,
                        principalTable: "Staffs",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK__Invoices__Medica__60A75C0F",
                        column: x => x.MedicalExaminationId,
                        principalTable: "MedicalExaminations",
                        principalColumn: "MedicalExaminationId");
                    table.ForeignKey(
                        name: "FK__Invoices__Patien__619B8048",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    PrescriptionsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicalExaminationId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Status = table.Column<int>(type: "int", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Prescrip__A2F31069879F8761", x => x.PrescriptionsId);
                    table.ForeignKey(
                        name: "FK__Prescript__Medic__5165187F",
                        column: x => x.MedicalExaminationId,
                        principalTable: "MedicalExaminations",
                        principalColumn: "MedicalExaminationId");
                });

            migrationBuilder.CreateTable(
                name: "Queue",
                columns: table => new
                {
                    QueueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicId = table.Column<int>(type: "int", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    MedicalExaminationId = table.Column<int>(type: "int", nullable: true),
                    QueueNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Queue", x => x.QueueId);
                    table.ForeignKey(
                        name: "FK_Queue_Clinic",
                        column: x => x.ClinicId,
                        principalTable: "Clinic",
                        principalColumn: "ClinicId");
                    table.ForeignKey(
                        name: "FK_Queue_MedicalExamination",
                        column: x => x.MedicalExaminationId,
                        principalTable: "MedicalExaminations",
                        principalColumn: "MedicalExaminationId");
                    table.ForeignKey(
                        name: "FK_Queue_Patient",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ServiceDetails",
                columns: table => new
                {
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    MedicalExaminationId = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ServiceD__131709449F09BC51", x => new { x.ServiceId, x.MedicalExaminationId });
                    table.ForeignKey(
                        name: "FK__ServiceDe__Medic__4D94879B",
                        column: x => x.MedicalExaminationId,
                        principalTable: "MedicalExaminations",
                        principalColumn: "MedicalExaminationId");
                    table.ForeignKey(
                        name: "FK__ServiceDe__Servi__4CA06362",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId");
                });

            migrationBuilder.CreateTable(
                name: "InvoiceDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReferenceId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__InvoiceD__3214EC078829BCC8", x => x.Id);
                    table.ForeignKey(
                        name: "FK__InvoiceDe__Invoi__6754599E",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "InvoiceId");
                });

            migrationBuilder.CreateTable(
                name: "PrescriptionDetails",
                columns: table => new
                {
                    PrescriptionId = table.Column<int>(type: "int", nullable: false),
                    MedicationId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Prescrip__F63DC99D1532F43C", x => new { x.PrescriptionId, x.MedicationId });
                    table.ForeignKey(
                        name: "FK__Prescript__Medic__571DF1D5",
                        column: x => x.MedicationId,
                        principalTable: "Medications",
                        principalColumn: "MedicationId");
                    table.ForeignKey(
                        name: "FK__Prescript__Presc__5629CD9C",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "PrescriptionsId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clinic_DepartmentId",
                table: "Clinic",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetails_InvoiceId",
                table: "InvoiceDetails",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CashierId",
                table: "Invoices",
                column: "CashierId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_MedicalExaminationId",
                table: "Invoices",
                column: "MedicalExaminationId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PatientId",
                table: "Invoices",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalExaminations_ClinicId",
                table: "MedicalExaminations",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalExaminations_DoctorId",
                table: "MedicalExaminations",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalExaminations_PatientId",
                table: "MedicalExaminations",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "UQ__Patients__B9C66DFED32204E5",
                table: "Patients",
                column: "PatientCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_StaffId",
                table: "Payrolls",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionDetails_MedicationId",
                table: "PrescriptionDetails",
                column: "MedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_MedicalExaminationId",
                table: "Prescriptions",
                column: "MedicalExaminationId");

            migrationBuilder.CreateIndex(
                name: "IX_Queue_ClinicId",
                table: "Queue",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_Queue_MedicalExaminationId",
                table: "Queue",
                column: "MedicalExaminationId");

            migrationBuilder.CreateIndex(
                name: "IX_Queue_PatientId",
                table: "Queue",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDetails_MedicalExaminationId",
                table: "ServiceDetails",
                column: "MedicalExaminationId");

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_DepartmentId",
                table: "Staffs",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "UQ__Staffs__1F642548A20E1F16",
                table: "Staffs",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceDetails");

            migrationBuilder.DropTable(
                name: "Payrolls");

            migrationBuilder.DropTable(
                name: "PrescriptionDetails");

            migrationBuilder.DropTable(
                name: "Queue");

            migrationBuilder.DropTable(
                name: "ServiceDetails");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Medications");

            migrationBuilder.DropTable(
                name: "Prescriptions");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "MedicalExaminations");

            migrationBuilder.DropTable(
                name: "Clinic");

            migrationBuilder.DropTable(
                name: "Doctors");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "Staffs");

            migrationBuilder.DropTable(
                name: "Department");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
