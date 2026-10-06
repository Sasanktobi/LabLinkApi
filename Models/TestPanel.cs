using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class TestPanel
    {
        public int TestPanelId{get;set;}
        public string Name{get;set;}=string.Empty;
        public string Description{get;set;}=string.Empty;
        public decimal Price{get;set;}
        public bool IsActive{get;set;}=true;
        public DateTime CreatedAt{get;set;}=DateTime.UtcNow;

        public ICollection<TestPanelTest> TestPanelTests{get;set;}=new List<TestPanelTest>();
        public ICollection<AppointmentTest> AppointmentTests{get;set;}=new List<AppointmentTest>();
    }
}
