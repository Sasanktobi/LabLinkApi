using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOs;
using Backend.IRepositories;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly LabLinkDbContext context;
        public AppointmentRepository(LabLinkDbContext _context)
        {
            context=_context;
        }

        public async Task<Appointment?> GetWithDetailsAsync(int id)
        {
            return await WithDetails()
                .FirstOrDefaultAsync(a=>a.AppointmentId==id);
        }

        public async Task<(List<Appointment> Items, int TotalCount)> SearchAsync(AppointmentFilterDto filter)
        {
            var query=context.Appointments.AsQueryable();

            if (filter.PatientId.HasValue)
            {
                query=query.Where(a=>a.PatientId==filter.PatientId.Value);
            }

            if (filter.DoctorId.HasValue)
            {
                query=query.Where(a=>a.DoctorId==filter.DoctorId.Value);
            }

            if (filter.PhlebotomistId.HasValue)
            {
                query=query.Where(a=>a.PhlebotomistId==filter.PhlebotomistId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query=query.Where(a=>a.Status==filter.Status);
            }

            if (!string.IsNullOrWhiteSpace(filter.AppointmentType))
            {
                query=query.Where(a=>a.AppointmentType==filter.AppointmentType);
            }

            if (filter.FromDate.HasValue)
            {
                query=query.Where(a=>a.ScheduleDate>=filter.FromDate.Value.Date);
            }

            if (filter.ToDate.HasValue)
            {
                query=query.Where(a=>a.ScheduleDate<=filter.ToDate.Value.Date);
            }

            var total=await query.CountAsync();

            var ids=await query
                .OrderByDescending(a=>a.ScheduleDate).ThenByDescending(a=>a.AppointmentId)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(a=>a.AppointmentId)
                .ToListAsync();

            // Load the page with details in a second query so paging is applied before the includes.
            var items=await WithDetails()
                .AsNoTracking()
                .Where(a=>ids.Contains(a.AppointmentId))
                .ToListAsync();

            items=items.OrderBy(a=>ids.IndexOf(a.AppointmentId)).ToList();
            return (items, total);
        }

        public async Task<Appointment> AddAsync(Appointment appointment)
        {
            await context.Appointments.AddAsync(appointment);
            await context.SaveChangesAsync();
            return appointment;
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }

        private IQueryable<Appointment> WithDetails()
        {
            return context.Appointments
                .Include(a=>a.Patient).ThenInclude(p=>p.User)
                .Include(a=>a.Doctor).ThenInclude(d=>d!.User)
                .Include(a=>a.Phlebotomist).ThenInclude(p=>p!.User)
                .Include(a=>a.AvailabilitySlot)
                .Include(a=>a.AppointmentTests).ThenInclude(at=>at.Test)
                .Include(a=>a.AppointmentTests).ThenInclude(at=>at.TestPanel)
                .Include(a=>a.Specimens)
                .Include(a=>a.Report)
                .AsSplitQuery();
        }
    }
}
