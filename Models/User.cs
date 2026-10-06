using System;
using System.Collections.Generic;
using System.Linq;

namespace Backend.Models
{
    public class User
    {
        public int UserId{get;set;}
        public string FirstName{get;set;}=String.Empty;
        public string LastName{get;set;}=String.Empty;
        public DateTime CreatedAt{get;set;}=DateTime.UtcNow;
        public bool IsActive{get;set;}=true;
        public string PhoneNo{get;set;}=String.Empty;
        public string Email{get;set;}=string.Empty;
        public string PasswordHash{get;set;}=String.Empty;


        public int RoleId{get;set;}
        public Role Role{get;set;}=null!;

        public Admin? Admin{get;set;}
        public Patient? Patient{get;set;}
        public Doctor? Doctor{get;set;}
        public Pathologist? Pathologist{get;set;}
        public LabTechnician? LabTechnician{get;set;}
        public Phlebotomist? Phlebotomist{get;set;}
        public Receptionist? Receptionist{get;set;}

        public ICollection<AvailabilitySlot> AvailabilitySlots{get;set;}=new List<AvailabilitySlot>();
        public ICollection<Appointment> AppointmentsBooked{get;set;}=new List<Appointment>();
        public ICollection<Specimen> SpecimensCollected{get;set;}=new List<Specimen>();
        public ICollection<AuditLog> AuditLogs{get;set;}=new List<AuditLog>();
    }
}
