using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class Phlebotomist
    {
        public int PhlebotomistId{get;set;}
        public string ServiceZone{get;set;}=string.Empty;
        public bool IsAvailable{get;set;}=true;
        public DateTime CreatedAt{get;set;}=DateTime.UtcNow;

        public int UserId{get;set;}
        public User User{get;set;}=null!;

        public ICollection<Appointment> Appointments{get;set;}=new List<Appointment>();
    }
}
