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

namespace Backend.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository patientRepository;
        private readonly ICurrentUserService currentUser;
        private readonly IAuditService auditService;

        public PatientService(IPatientRepository _patientRepository, ICurrentUserService _currentUser, IAuditService _auditService)
        {
            patientRepository=_patientRepository;
            currentUser=_currentUser;
            auditService=_auditService;
        }

        public async Task<IEnumerable<PatientResponseDto>> SearchAsync(string? search)
        {
            var patients=await patientRepository.SearchAsync(search);
            return patients.Select(DtoMapper.ToDto).ToList();
        }

        public async Task<PatientResponseDto> GetByIdAsync(int patientId)
        {
            var patient=await LoadAsync(patientId);
            return DtoMapper.ToDto(patient);
        }

        public async Task<PatientResponseDto> GetMineAsync()
        {
            var patient=await LoadMineAsync();
            return DtoMapper.ToDto(patient);
        }

        public async Task<PatientResponseDto> UpdateAsync(int patientId, PatientUpdateDto dto)
        {
            var patient=await LoadAsync(patientId);
            return await ApplyUpdateAsync(patient, dto);
        }

        public async Task<PatientResponseDto> UpdateMineAsync(PatientUpdateDto dto)
        {
            var patient=await LoadMineAsync();
            return await ApplyUpdateAsync(patient, dto);
        }

        private async Task<PatientResponseDto> ApplyUpdateAsync(Patient patient, PatientUpdateDto dto)
        {
            if (dto.DateOfBirth.Date > DateTime.Today)
            {
                throw new BadRequestException("Date of birth cannot be in the future.");
            }

            patient.DateOfBirth=dto.DateOfBirth.Date;
            patient.Gender=dto.Gender;
            patient.BloodGroup=dto.BloodGroup;
            patient.Address=dto.Address;
            patient.City=dto.City;
            patient.State=dto.State;
            patient.PostalCode=dto.PostalCode;

            await patientRepository.SaveChangesAsync();
            await auditService.LogAsync(nameof(Patient), patient.PatientId, AuditActions.Update, "Updated patient profile.");
            return DtoMapper.ToDto(patient);
        }

        private async Task<Patient> LoadAsync(int patientId)
        {
            var patient=await patientRepository.GetByIdAsync(patientId)
                ?? throw new NotFoundException($"Patient {patientId} was not found.");

            // Patients may only see themselves; everyone else reaching here is staff (enforced by the controller).
            if (currentUser.IsInRole(RoleNames.Patient) && patient.UserId != currentUser.UserId)
            {
                throw new ForbiddenException("You can only access your own patient record.");
            }

            return patient;
        }

        private async Task<Patient> LoadMineAsync()
        {
            var userId=currentUser.RequireUserId();
            return await patientRepository.GetByUserIdAsync(userId)
                ?? throw new NotFoundException("No patient profile is linked to this account.");
        }
    }
}
