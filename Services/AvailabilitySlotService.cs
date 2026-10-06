using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class AvailabilitySlotService : IAvailabilitySlotService
    {
        private readonly IAvailabilitySlotRepository slotRepository;
        private readonly IUserRepository userRepository;
        private readonly ICurrentUserService currentUser;
        private readonly IAuditService auditService;

        public AvailabilitySlotService(IAvailabilitySlotRepository _slotRepository, IUserRepository _userRepository,
            ICurrentUserService _currentUser, IAuditService _auditService)
        {
            slotRepository=_slotRepository;
            userRepository=_userRepository;
            currentUser=_currentUser;
            auditService=_auditService;
        }

        public async Task<IEnumerable<SlotResponseDto>> SearchAsync(DateTime? fromDate, DateTime? toDate, string? providerType, int? userId, bool availableOnly)
        {
            var slots=await slotRepository.SearchAsync(fromDate, toDate, providerType, userId, availableOnly);
            return slots.Select(DtoMapper.ToDto).ToList();
        }

        public async Task<SlotResponseDto> GetByIdAsync(int id)
        {
            var slot=await slotRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"Slot {id} was not found.");
            return DtoMapper.ToDto(slot);
        }

        public async Task<SlotResponseDto> CreateAsync(SlotCreateDto dto)
        {
            var start=ParseTime(dto.StartTime);
            var end=ParseTime(dto.EndTime);
            var created=await CreateSlotsAsync(dto, new List<(TimeOnly, TimeOnly)> { (start, end) });
            return created.Single();
        }

        public async Task<IEnumerable<SlotResponseDto>> CreateBulkAsync(SlotBulkCreateDto dto)
        {
            var start=ParseTime(dto.StartTime);
            var end=ParseTime(dto.EndTime);
            if (end <= start)
            {
                throw new BadRequestException("EndTime must be after StartTime.");
            }

            var ranges=new List<(TimeOnly, TimeOnly)>();
            var cursor=start;
            while (cursor.AddMinutes(dto.DurationMinutes) <= end && cursor.AddMinutes(dto.DurationMinutes) > cursor)
            {
                var next=cursor.AddMinutes(dto.DurationMinutes);
                ranges.Add((cursor, next));
                cursor=next;
            }

            if (ranges.Count == 0)
            {
                throw new BadRequestException("The time window is shorter than one slot.");
            }

            return await CreateSlotsAsync(dto, ranges);
        }

        public async Task DeleteAsync(int id)
        {
            var slot=await slotRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"Slot {id} was not found.");

            if (!CanManageSlotsFor(slot.UserId))
            {
                throw new ForbiddenException("You can only remove your own slots.");
            }

            if (slot.IsBooked)
            {
                throw new ConflictException("A booked slot cannot be removed; cancel or reschedule the appointment first.");
            }

            await slotRepository.DeleteAsync(slot);
            await auditService.LogAsync(nameof(AvailabilitySlot), id, AuditActions.Delete, $"Removed slot {slot.SlotDate:yyyy-MM-dd} {slot.StartTime}-{slot.EndTime}.");
        }

        private async Task<List<SlotResponseDto>> CreateSlotsAsync(SlotCreateDto dto, List<(TimeOnly Start, TimeOnly End)> ranges)
        {
            var ownerId=dto.UserId ?? currentUser.RequireUserId();
            if (!CanManageSlotsFor(ownerId))
            {
                throw new ForbiddenException("You can only create slots for yourself.");
            }

            var owner=await userRepository.GetUserByIdAsync(ownerId)
                ?? throw new NotFoundException($"User {ownerId} was not found.");

            if (!owner.IsActive)
            {
                throw new BadRequestException("Slots cannot be created for an inactive user.");
            }

            var providerType=owner.Role.Name switch
            {
                RoleNames.Phlebotomist => ProviderTypes.Phlebotomist,
                RoleNames.LabTechnician or RoleNames.Receptionist => ProviderTypes.Lab,
                _ => throw new BadRequestException("Slots can only belong to a Phlebotomist, LabTechnician or Receptionist.")
            };

            var date=dto.SlotDate.Date;
            if (date < DateTime.Today)
            {
                throw new BadRequestException("Slots cannot be created in the past.");
            }

            var existing=(await slotRepository.GetForUserOnDateAsync(ownerId, date))
                .Select(s=>(Start: ParseTime(s.StartTime), End: ParseTime(s.EndTime)))
                .ToList();

            var slots=new List<AvailabilitySlot>();
            foreach (var (start, end) in ranges)
            {
                if (end <= start)
                {
                    throw new BadRequestException("EndTime must be after StartTime.");
                }

                if (existing.Any(e=>start < e.End && e.Start < end))
                {
                    throw new ConflictException($"Slot {Format(start)}-{Format(end)} overlaps an existing slot on {date:yyyy-MM-dd}.");
                }

                existing.Add((start, end));
                slots.Add(new AvailabilitySlot
                {
                    UserId=ownerId,
                    SlotDate=date,
                    StartTime=Format(start),
                    EndTime=Format(end),
                    ProviderType=providerType,
                    IsBooked=false
                });
            }

            await slotRepository.AddRangeAsync(slots);
            await auditService.LogAsync(nameof(AvailabilitySlot), slots[0].AvailabilitySlotId, AuditActions.Create,
                $"Created {slots.Count} slot(s) for user {ownerId} on {date:yyyy-MM-dd}.");

            foreach (var slot in slots)
            {
                slot.User=owner;
            }
            return slots.Select(DtoMapper.ToDto).ToList();
        }

        private bool CanManageSlotsFor(int ownerUserId)
        {
            return currentUser.IsInRole(RoleNames.Admin, RoleNames.Receptionist) || currentUser.UserId == ownerUserId;
        }

        private static TimeOnly ParseTime(string value)
        {
            if (!TimeOnly.TryParseExact(value, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
            {
                throw new BadRequestException($"'{value}' is not a valid time; use HH:mm.");
            }
            return time;
        }

        private static string Format(TimeOnly time)
        {
            return time.ToString("HH:mm", CultureInfo.InvariantCulture);
        }
    }
}
