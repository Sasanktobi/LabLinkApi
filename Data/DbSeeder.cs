using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Constants;
using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Backend.Data
{
    // Seeds the first Admin account (there is no other way to create one) and, optionally, a starter test catalogue.
    // Roles themselves are seeded by the migration (HasData).
    public static class DbSeeder
    {
        public static async Task SeedAsync(LabLinkDbContext context, IPasswordHasher<User> passwordHasher, IConfiguration configuration, ILogger logger)
        {
            if (!await context.Database.CanConnectAsync())
            {
                logger.LogWarning("Database is not reachable; skipping seeding.");
                return;
            }

            var pending=await context.Database.GetPendingMigrationsAsync();
            if (pending.Any())
            {
                logger.LogWarning("Database has pending migrations ({Migrations}); run 'dotnet ef database update' first. Skipping seeding.",
                    string.Join(", ", pending));
                return;
            }

            await SeedAdminAsync(context, passwordHasher, configuration, logger);

            if (configuration.GetValue<bool>("Seed:SampleCatalogue"))
            {
                await SeedCatalogueAsync(context, logger);
            }
        }

        private static async Task SeedAdminAsync(LabLinkDbContext context, IPasswordHasher<User> passwordHasher, IConfiguration configuration, ILogger logger)
        {
            if (await context.Users.AnyAsync(u=>u.Role.Name==RoleNames.Admin))
            {
                return;
            }

            var email=configuration["Seed:Admin:Email"];
            var password=configuration["Seed:Admin:Password"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("No Admin user exists and Seed:Admin:Email / Seed:Admin:Password are not configured.");
                return;
            }

            var role=await context.Roles.FirstAsync(r=>r.Name==RoleNames.Admin);
            var admin=new User
            {
                FirstName=configuration["Seed:Admin:FirstName"] ?? "System",
                LastName=configuration["Seed:Admin:LastName"] ?? "Admin",
                Email=email,
                PhoneNo=configuration["Seed:Admin:PhoneNo"] ?? string.Empty,
                RoleId=role.RoleId,
                IsActive=true,
                Admin=new Admin()
            };
            admin.PasswordHash=passwordHasher.HashPassword(admin, password);

            context.Users.Add(admin);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded Admin user {Email}.", email);
        }

        private static async Task SeedCatalogueAsync(LabLinkDbContext context, ILogger logger)
        {
            if (await context.Tests.AnyAsync())
            {
                return;
            }

            Test T(string code, string name, string sample, decimal price, string low, string high, string unit, int tat=24)
            {
                return new Test { Code=code, Name=name, SampleType=sample, Price=price, ReferenceRangeLow=low, ReferenceRangeHigh=high, Unit=unit, TurnaroundHours=tat };
            }

            var tests=new List<Test>
            {
                T("HB", "Haemoglobin", "Blood", 150, "13", "17", "g/dL", 6),
                T("WBC", "Total Leucocyte Count", "Blood", 150, "4000", "11000", "cells/uL", 6),
                T("PLT", "Platelet Count", "Blood", 150, "150000", "410000", "cells/uL", 6),
                T("RBC", "Red Blood Cell Count", "Blood", 150, "4.5", "5.5", "million/uL", 6),
                T("FBS", "Fasting Blood Sugar", "Blood", 120, "70", "100", "mg/dL", 6),
                T("HBA1C", "HbA1c", "Blood", 550, "4", "5.6", "%", 24),
                T("TCHOL", "Total Cholesterol", "Blood", 250, "", "200", "mg/dL", 12),
                T("TG", "Triglycerides", "Blood", 250, "", "150", "mg/dL", 12),
                T("HDL", "HDL Cholesterol", "Blood", 250, "40", "", "mg/dL", 12),
                T("LDL", "LDL Cholesterol", "Blood", 250, "", "100", "mg/dL", 12),
                T("TSH", "Thyroid Stimulating Hormone", "Blood", 400, "0.4", "4.0", "uIU/mL", 24),
                T("CREAT", "Serum Creatinine", "Blood", 200, "0.7", "1.3", "mg/dL", 12),
                T("URINE-RE", "Urine Routine Examination", "Urine", 180, "", "", "", 12)
            };
            context.Tests.AddRange(tests);

            TestPanel P(string name, string description, decimal price, params string[] codes)
            {
                return new TestPanel
                {
                    Name=name,
                    Description=description,
                    Price=price,
                    TestPanelTests=tests.Where(t=>codes.Contains(t.Code)).Select(t=>new TestPanelTest { Test=t }).ToList()
                };
            }

            context.TestPanels.AddRange(
                P("Complete Blood Count", "Haemoglobin, WBC, platelets and RBC.", 450, "HB", "WBC", "PLT", "RBC"),
                P("Lipid Profile", "Cholesterol, triglycerides, HDL and LDL.", 800, "TCHOL", "TG", "HDL", "LDL"),
                P("Diabetes Screening", "Fasting blood sugar and HbA1c.", 600, "FBS", "HBA1C"));

            await context.SaveChangesAsync();
            logger.LogInformation("Seeded sample test catalogue ({Count} tests).", tests.Count);
        }
    }
}
