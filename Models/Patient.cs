using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class Patient
    {
        public int PatientId{get;set;}
        public DateTime DateOfBirth{get;set;}
        public string Gender{get;set;}=string.Empty;
        public string BloodGroup{get;set;}=string.Empty;
        public string Address{get;set;}=string.Empty;
        public string City{get;set;}=string.Empty;
        public string State{get;set;}=string.Empty;
        public string PostalCode{get;set;}=string.Empty;
        public DateTime CreatedAt{get;set;}=DateTime.UtcNow;

        public int UserId{get;set;}
        public User User{get;set;}=null!;

        public ICollection<Appointment> Appointments{get;set;}=new List<Appointment>();
    }
}
