using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class Doctor
    {
        public int DoctorId{get;set;}
        public string Specialization{get;set;}=string.Empty;
        public string LicenseNumber{get;set;}=string.Empty;
        public string Qualification{get;set;}=string.Empty;
        public int YearsOfExperience{get;set;}
        public string ClinicAddress{get;set;}=string.Empty;
        public DateTime CreatedAt{get;set;}=DateTime.UtcNow;

        public int UserId{get;set;}
        public User User{get;set;}=null!;

        public ICollection<Appointment> Appointments{get;set;}=new List<Appointment>();
    }
}
