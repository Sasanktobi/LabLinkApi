using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs;
using Backend.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly LabLinkDbContext context;
        public DashboardRepository(LabLinkDbContext _context)
        {
            context=_context;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync(DateTime today)
        {
            var date=today.Date;
            var tomorrow=date.AddDays(1);

            // Sequential awaits: a DbContext does not support concurrent queries.
            var summary=new DashboardSummaryDto
            {
                AppointmentsToday=await context.Appointments
                    .CountAsync(a=>a.ScheduleDate==date && a.Status!=AppointmentStatuses.Cancelled),
                PendingCollection=await context.Appointments
                    .CountAsync(a=>a.Status==AppointmentStatuses.Booked || a.Status==AppointmentStatuses.Confirmed),
                SpecimensAwaitingReceipt=await context.Specimens
                    .CountAsync(s=>s.Status==SpecimenStatuses.Collected),
                AppointmentsInProgress=await context.Appointments
                    .CountAsync(a=>a.Status==AppointmentStatuses.SampleCollected || a.Status==AppointmentStatuses.InProgress),
                ReportsPendingValidation=await context.Reports
                    .CountAsync(r=>r.Status==ReportStatuses.PendingValidation),
                ReportsReadyToRelease=await context.Reports
                    .CountAsync(r=>r.Status==ReportStatuses.Validated),
                ReportsReleasedToday=await context.Reports
                    .CountAsync(r=>r.ReleasedAt>=date && r.ReleasedAt<tomorrow),
                TotalPatients=await context.Patients.CountAsync(),
                RevenueToday=await context.AppointmentTests
                    .Where(at=>at.Appointment.ScheduleDate==date
                        && at.Appointment.Status!=AppointmentStatuses.Cancelled
                        && at.Status!=AppointmentTestStatuses.Cancelled)
                    .SumAsync(at=>(decimal?)at.PriceAtOrder) ?? 0m
            };

            return summary;
        }
    }
}
