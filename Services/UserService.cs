using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Constants;
using Backend.DTOs;
using Backend.Exceptions;
using Backend.Helpers;
using Backend.IRepositories;
using Backend.IServices;
using Backend.Models;
using Microsoft.AspNetCore.Identity;

namespace Backend.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        private readonly IRoleRepository roleRepository;
        private readonly IStaffRepository staffRepository;
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly IAuditService auditService;

        public UserService(IUserRepository _userRepository, IRoleRepository _roleRepository, IStaffRepository _staffRepository,
            IPasswordHasher<User> _passwordHasher, IAuditService _auditService)
        {
            userRepository=_userRepository;
            roleRepository=_roleRepository;
            staffRepository=_staffRepository;
            passwordHasher=_passwordHasher;
            auditService=_auditService;
        }

        public async Task<UserResponseDto> RegisterAsync(StaffCreateDto dto)
        {
            var role=await roleRepository.GetByIdAsync(dto.RoleId)
                ?? throw new BadRequestException($"Role {dto.RoleId} does not exist.");

            if (role.Name == RoleNames.Patient)
            {
                throw new BadRequestException("Patients are registered through the patient registration endpoint.");
            }

            if (await userRepository.CheckEmailExistsAsync(dto.Email))
            {
                throw new InvalidOperationException($"A user with email '{dto.Email}' already exists.");
            }

            var user=NewUser(dto.FirstName, dto.LastName, dto.Email, dto.PhoneNo, role.RoleId);
            await AttachStaffProfileAsync(user, role.Name, dto);
            user.PasswordHash=passwordHasher.HashPassword(user, dto.Password);

            // The profile is a navigation property, so user and profile are inserted in one SaveChanges.
            var created=await userRepository.CreateUserAsync(user);
            await auditService.LogAsync(nameof(User), created.UserId, AuditActions.Create, $"Created {role.Name} account {created.Email}.");

            var withProfiles=await userRepository.GetUserWithProfilesAsync(created.UserId);
            return MapToResponse(withProfiles ?? created);
        }

        public async Task<UserResponseDto> RegisterPatientAsync(PatientRegisterDto dto)
        {
            var role=await roleRepository.GetByNameAsync(RoleNames.Patient)
                ?? throw new InvalidOperationException("Patient role is missing; apply the database migrations.");

            if (await userRepository.CheckEmailExistsAsync(dto.Email))
            {
                throw new InvalidOperationException($"A user with email '{dto.Email}' already exists.");
            }

            if (dto.DateOfBirth.Date > DateTime.Today)
            {
                throw new BadRequestException("Date of birth cannot be in the future.");
            }

            var user=NewUser(dto.FirstName, dto.LastName, dto.Email, dto.PhoneNo, role.RoleId);
            user.Patient=new Patient
            {
                DateOfBirth=dto.DateOfBirth.Date,
                Gender=dto.Gender,
                BloodGroup=dto.BloodGroup,
                Address=dto.Address,
                City=dto.City,
                State=dto.State,
                PostalCode=dto.PostalCode
            };
            user.PasswordHash=passwordHasher.HashPassword(user, dto.Password);

            var created=await userRepository.CreateUserAsync(user);
            await auditService.LogAsync(nameof(Patient), created.Patient!.PatientId, AuditActions.Create, $"Registered patient {created.Email}.");

            var withProfiles=await userRepository.GetUserWithProfilesAsync(created.UserId);
            return MapToResponse(withProfiles ?? created);
        }

        public async Task<UserResponseDto?> GetByIdAsync(int id)
        {
            var user=await userRepository.GetUserWithProfilesAsync(id);
            return user == null ? null : MapToResponse(user);
        }

        public async Task<UserResponseDto?> UpdateAsync(int id, UserUpdateDto dto)
        {
            var user = new User
            {
                FirstName=dto.FirstName,
                LastName=dto.LastName,
                Email=dto.Email,
                PhoneNo=dto.PhoneNo
            };

            var updated=await userRepository.UpdateUserAsync(id, user);
            if (updated == null)
            {
                return null;
            }

            await auditService.LogAsync(nameof(User), id, AuditActions.Update, "Updated account details.");
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var deactivated=await userRepository.DeleteUserAsync(id);
            if (deactivated != null)
            {
                await auditService.LogAsync(nameof(User), id, AuditActions.Delete, "Deactivated account.");
            }
            return deactivated != null;
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var activated=await userRepository.ActivateUserAsync(id);
            if (activated != null)
            {
                await auditService.LogAsync(nameof(User), id, AuditActions.Update, "Reactivated account.");
            }
            return activated != null;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllActiveAsync()
        {
            var users=await userRepository.GetAllActiveAsync();
            return users.Select(MapToResponse).ToList();
        }

        public async Task<IEnumerable<UserResponseDto>> GetByRoleIdAsync(int roleId)
        {
            var users=await userRepository.GetByRoleIdAsync(roleId);
            return users.Select(MapToResponse).ToList();
        }

        public async Task<UserResponseDto?> AuthenticateAsync(LoginDto dto)
        {
            var user=await userRepository.GetByEmailWithRoleAsync(dto.Email);
            if (user == null || !user.IsActive)
            {
                return null;
            }

            var result=passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                return null;
            }

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                var rehashed=passwordHasher.HashPassword(user, dto.Password);
                await userRepository.UpdatePasswordAsync(user.UserId, rehashed);
            }

            return MapToResponse(user);
        }

        public async Task<bool> ChangePasswordAsync(int id, ChangePasswordDto dto)
        {
            var user=await userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            var result=passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword);
            if (result == PasswordVerificationResult.Failed)
            {
                return false;
            }

            var newHash=passwordHasher.HashPassword(user, dto.NewPassword);
            var changed=await userRepository.UpdatePasswordAsync(id, newHash);
            if (changed)
            {
                await auditService.LogAsync(nameof(User), id, AuditActions.Update, "Changed password.");
            }
            return changed;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await userRepository.CheckEmailExistsAsync(email);
        }

        private static User NewUser(string firstName, string lastName, string email, string phoneNo, int roleId)
        {
            return new User
            {
                FirstName=firstName,
                LastName=lastName,
                Email=email,
                PhoneNo=phoneNo,
                RoleId=roleId,
                IsActive=true,
                CreatedAt=DateTime.UtcNow
            };
        }

        private async Task AttachStaffProfileAsync(User user, string roleName, StaffCreateDto dto)
        {
            switch (roleName)
            {
                case RoleNames.Admin:
                    user.Admin=new Admin();
                    break;

                case RoleNames.Doctor:
                case RoleNames.Pathologist:
                    var license=Require(dto.LicenseNumber, nameof(dto.LicenseNumber), roleName);
                    if (await staffRepository.LicenseNumberExistsAsync(roleName, license, null))
                    {
                        throw new ConflictException($"License number '{license}' is already registered.");
                    }

                    if (roleName == RoleNames.Doctor)
                    {
                        user.Doctor=new Doctor
                        {
                            LicenseNumber=license,
                            Specialization=dto.Specialization ?? string.Empty,
                            Qualification=dto.Qualification ?? string.Empty,
                            YearsOfExperience=dto.YearsOfExperience ?? 0,
                            ClinicAddress=dto.ClinicAddress ?? string.Empty
                        };
                    }
                    else
                    {
                        user.Pathologist=new Pathologist
                        {
                            LicenseNumber=license,
                            Specialization=dto.Specialization ?? string.Empty,
                            Qualification=dto.Qualification ?? string.Empty
                        };
                    }
                    break;

                case RoleNames.LabTechnician:
                    user.LabTechnician=new LabTechnician
                    {
                        Department=dto.Department ?? string.Empty,
                        ShiftTiming=dto.ShiftTiming ?? string.Empty
                    };
                    break;

                case RoleNames.Phlebotomist:
                    user.Phlebotomist=new Phlebotomist
                    {
                        ServiceZone=Require(dto.ServiceZone, nameof(dto.ServiceZone), roleName),
                        IsAvailable=true
                    };
                    break;

                case RoleNames.Receptionist:
                    user.Receptionist=new Receptionist
                    {
                        ShiftTiming=dto.ShiftTiming ?? string.Empty,
                        BranchLocation=dto.BranchLocation ?? string.Empty
                    };
                    break;

                default:
                    throw new BadRequestException($"Unsupported role '{roleName}'.");
            }
        }

        private static string Require(string? value, string field, string roleName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new BadRequestException($"{field} is required for a {roleName}.");
            }
            return value.Trim();
        }

        private static UserResponseDto MapToResponse(User user)
        {
            return new UserResponseDto
            {
                UserId=user.UserId,
                FirstName=user.FirstName,
                LastName=user.LastName,
                Email=user.Email,
                PhoneNo=user.PhoneNo,
                IsActive=user.IsActive,
                CreatedAt=user.CreatedAt,
                RoleId=user.RoleId,
                RoleName=user.Role?.Name ?? string.Empty,
                ProfileId=DtoMapper.ProfileIdOf(user)
            };
        }
    }
}
