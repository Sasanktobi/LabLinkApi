using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class Pathologist
    {
        public int PathologistId{get;set;}
        public string Specialization{get;set;}=string.Empty;
        public string LicenseNumber{get;set;}=string.Empty;
        public string Qualification{get;set;}=string.Empty;
        public DateTime CreatedAt{get;set;}=DateTime.UtcNow;

        public int UserId{get;set;}
        public User User{get;set;}=null!;

        public ICollection<Report> ReportsValidated{get;set;}=new List<Report>();
    }
}
