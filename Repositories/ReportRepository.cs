using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.IRepositories;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly LabLinkDbContext context;
        public ReportRepository(LabLinkDbContext _context)
        {
            context=_context;
        }

        public async Task<Report?> GetByIdAsync(int id)
        {
            return await WithDetails().FirstOrDefaultAsync(r=>r.ReportId==id);
        }

        public async Task<Report?> GetByAppointmentIdAsync(int appointmentId)
        {
            return await WithDetails().FirstOrDefaultAsync(r=>r.AppointmentId==appointmentId);
        }

        public async Task<IEnumerable<Report>> SearchAsync(string? status, int? patientId, int? doctorId)
        {
            var query=context.Reports
                .Include(r=>r.Appointment).ThenInclude(a=>a.Patient).ThenInclude(p=>p.User)
                .Include(r=>r.Appointment).ThenInclude(a=>a.Doctor).ThenInclude(d=>d!.User)
                .Include(r=>r.ValidatedByPathologist).ThenInclude(p=>p!.User)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query=query.Where(r=>r.Status==status);
            }

            if (patientId.HasValue)
            {
                query=query.Where(r=>r.Appointment.PatientId==patientId.Value);
            }

            if (doctorId.HasValue)
            {
                query=query.Where(r=>r.Appointment.DoctorId==doctorId.Value);
            }

            return await query.OrderByDescending(r=>r.CreatedAt).Take(500).ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }

        private IQueryable<Report> WithDetails()
        {
            return context.Reports
                .Include(r=>r.Appointment).ThenInclude(a=>a.Patient).ThenInclude(p=>p.User)
                .Include(r=>r.Appointment).ThenInclude(a=>a.Doctor).ThenInclude(d=>d!.User)
                .Include(r=>r.Appointment).ThenInclude(a=>a.AppointmentTests)
                .Include(r=>r.ValidatedByPathologist).ThenInclude(p=>p!.User)
                .AsSplitQuery();
        }
    }
}
