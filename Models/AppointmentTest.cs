using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class AppointmentTest
    {
        public int AppointmentTestId{get;set;}
        public decimal PriceAtOrder{get;set;}
        public string Status{get;set;}=string.Empty;
        public DateTime CreatedAt{get;set;}=DateTime.UtcNow;

        public int AppointmentId{get;set;}
        public Appointment Appointment{get;set;}=null!;

        public int TestId{get;set;}
        public Test Test{get;set;}=null!;

        public int? TestPanelId{get;set;}
        public TestPanel? TestPanel{get;set;}
    }
}
