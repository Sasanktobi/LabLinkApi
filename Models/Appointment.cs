using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class Appointment
    {
        public int AppointmentId{get;set;}
        public DateTime ScheduleDate{get;set;}
        public string ScheduleTimeSlot{get;set;}=string.Empty;
        public string AppointmentType{get;set;}=string.Empty;
        public string Status{get;set;}=string.Empty;
        public string HomeAddress{get;set;}=string.Empty;
        public string BookedBy{get;set;}=string.Empty;
        public DateTime CreatedAt{get;set;}=DateTime.UtcNow;

        public int PatientId{get;set;}
        public Patient Patient{get;set;}=null!;

        public int? DoctorId{get;set;}
        public Doctor? Doctor{get;set;}

        public int? PhlebotomistId{get;set;}
        public Phlebotomist? Phlebotomist{get;set;}

        public int? AvailabilitySlotId{get;set;}
        public AvailabilitySlot? AvailabilitySlot{get;set;}

        public int? BookedByUserId{get;set;}
        public User? BookedByUser{get;set;}

        public ICollection<AppointmentTest> AppointmentTests{get;set;}=new List<AppointmentTest>();
        public ICollection<Specimen> Specimens{get;set;}=new List<Specimen>();
        public Report? Report{get;set;}
    }
}
