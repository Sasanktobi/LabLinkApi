using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class Report
    {
        public int ReportId{get;set;}
        public string PdfFilepath{get;set;}=string.Empty;
        public string Status{get;set;}=string.Empty;
        public DateTime? ValidatedAt{get;set;}
        public DateTime? ReleasedAt{get;set;}
        public string PathologistRemarks{get;set;}=string.Empty;
        public DateTime CreatedAt{get;set;}=DateTime.UtcNow;

        public int AppointmentId{get;set;}
        public Appointment Appointment{get;set;}=null!;

        public int? ValidatedByPathologistId{get;set;}
        public Pathologist? ValidatedByPathologist{get;set;}
    }
}
