using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class LabTechnician
    {
        public int LabTechnicianId{get;set;}
        public string Department{get;set;}=string.Empty;
        public string ShiftTiming{get;set;}=string.Empty;
        public DateTime CreatedAt{get;set;}=DateTime.UtcNow;

        public int UserId{get;set;}
        public User User{get;set;}=null!;

        public ICollection<TestResult> TestResultsEntered{get;set;}=new List<TestResult>();
    }
}
