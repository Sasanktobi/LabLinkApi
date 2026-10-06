using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class Test
    {
        public int TestId{get;set;}
        public string Name{get;set;}=string.Empty;
        public string Code{get;set;}=string.Empty;
        public string Description{get;set;}=string.Empty;
        public string SampleType{get;set;}=string.Empty;
        public decimal Price{get;set;}
        public string ReferenceRangeLow { get; set; } = string.Empty;
        public string ReferenceRangeHigh { get; set; } = string.Empty;
        public string Unit{get;set;}=string.Empty;
        public int TurnaroundHours{get;set;}=24;
        public bool IsActive{get;set;}=true;
        public DateTime CreatedAt{get;set;}=DateTime.UtcNow;

        public ICollection<TestPanelTest> TestPanelTests{get;set;}=new List<TestPanelTest>();
        public ICollection<AppointmentTest> AppointmentTests{get;set;}=new List<AppointmentTest>();
        public ICollection<TestResult> TestResults{get;set;}=new List<TestResult>();
    }
}
