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

namespace Backend.Services
{
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository staffRepository;
        private readonly IUserRepository userRepository;
        private readonly ICurrentUserService currentUser;
        private readonly IAuditService auditService;

        public StaffService(IStaffRepository _staffRepository, IUserRepository _userRepository, ICurrentUserService _currentUser, IAuditService _auditService)
        {
            staffRepository=_staffRepository;
            userRepository=_userRepository;
            currentUser=_currentUser;
            auditService=_auditService;
        }

        public async Task<IEnumerable<StaffResponseDto>> GetStaffAsync(string? roleName, string? search, bool activeOnly)
        {
            if (!string.IsNullOrWhiteSpace(roleName) && !RoleNames.All.Contains(roleName))
            {
                throw new BadRequestException($"Unknown role '{roleName}'.");
            }

            var users=await staffRepository.GetStaffUsersAsync(roleName, search, activeOnly);
            return users.Select(DtoMapper.ToStaffDto).ToList();
        }

        public async Task<StaffResponseDto> GetByUserIdAsync(int userId)
        {
            var user=await LoadStaffAsync(userId);
            return DtoMapper.ToStaffDto(user);
        }

        public async Task<StaffResponseDto> UpdateProfileAsync(int userId, StaffProfileUpdateDto dto)
        {
            var user=await LoadStaffAsync(userId);

            if (!currentUser.IsInRole(RoleNames.Admin) && currentUser.UserId != userId)
            {
                throw new ForbiddenException("You can only update your own profile.");
            }

            if (user.Doctor != null)
            {
                await ApplyLicenseAsync(RoleNames.Doctor, dto.LicenseNumber, userId, l=>user.Doctor.LicenseNumber=l);
                user.Doctor.Specialization=dto.Specialization ?? user.Doctor.Specialization;
                user.Doctor.Qualification=dto.Qualification ?? user.Doctor.Qualification;
                user.Doctor.YearsOfExperience=dto.YearsOfExperience ?? user.Doctor.YearsOfExperience;
                user.Doctor.ClinicAddress=dto.ClinicAddress ?? user.Doctor.ClinicAddress;
            }
            else if (user.Pathologist != null)
            {
                await ApplyLicenseAsync(RoleNames.Pathologist, dto.LicenseNumber, userId, l=>user.Pathologist.LicenseNumber=l);
                user.Pathologist.Specialization=dto.Specialization ?? user.Pathologist.Specialization;
                user.Pathologist.Qualification=dto.Qualification ?? user.Pathologist.Qualification;
            }
            else if (user.LabTechnician != null)
            {
                user.LabTechnician.Department=dto.Department ?? user.LabTechnician.Department;
                user.LabTechnician.ShiftTiming=dto.ShiftTiming ?? user.LabTechnician.ShiftTiming;
            }
            else if (user.Phlebotomist != null)
            {
                user.Phlebotomist.ServiceZone=dto.ServiceZone ?? user.Phlebotomist.ServiceZone;
                user.Phlebotomist.IsAvailable=dto.IsAvailable ?? user.Phlebotomist.IsAvailable;
            }
            else if (user.Receptionist != null)
            {
                user.Receptionist.ShiftTiming=dto.ShiftTiming ?? user.Receptionist.ShiftTiming;
                user.Receptionist.BranchLocation=dto.BranchLocation ?? user.Receptionist.BranchLocation;
            }

            await staffRepository.SaveChangesAsync();
            await auditService.LogAsync(nameof(User), userId, AuditActions.Update, $"Updated {user.Role.Name} profile.");
            return DtoMapper.ToStaffDto(user);
        }

        public async Task<StaffResponseDto> SetMyAvailabilityAsync(bool isAvailable)
        {
            var userId=currentUser.RequireUserId();
            var phlebotomist=await staffRepository.GetPhlebotomistByUserIdAsync(userId)
                ?? throw new NotFoundException("No phlebotomist profile is linked to this account.");

            phlebotomist.IsAvailable=isAvailable;
            await staffRepository.SaveChangesAsync();
            await auditService.LogAsync(nameof(Phlebotomist), phlebotomist.PhlebotomistId, AuditActions.Update, $"Availability set to {isAvailable}.");

            return await GetByUserIdAsync(userId);
        }

        private async Task ApplyLicenseAsync(string roleName, string? licenseNumber, int userId, System.Action<string> apply)
        {
            if (string.IsNullOrWhiteSpace(licenseNumber))
            {
                return;
            }

            var license=licenseNumber.Trim();
            if (await staffRepository.LicenseNumberExistsAsync(roleName, license, userId))
            {
                throw new ConflictException($"License number '{license}' is already registered.");
            }
            apply(license);
        }

        private async Task<User> LoadStaffAsync(int userId)
        {
            var user=await userRepository.GetUserWithProfilesAsync(userId);
            if (user == null || user.Role.Name == RoleNames.Patient)
            {
                throw new NotFoundException($"Staff member {userId} was not found.");
            }
            return user;
        }
    }
}
