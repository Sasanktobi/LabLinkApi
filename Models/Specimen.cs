using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class Specimen
    {
        public int SpecimenId{get;set;}
        public string SpecimenName{get;set;}=string.Empty;
        public DateTime CollectedAt{get;set;}
        public string Status { get; set; } = string.Empty;
         public string RejectionReason { get; set; } = string.Empty;
         public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int AppointmentId{get;set;}
        public Appointment Appointment{get;set;}=null!;

        public int? CollectedByUserId{get;set;}
        public User? CollectedByUser{get;set;}

        public ICollection<TestResult> TestResults{get;set;}=new List<TestResult>();
    }
}
