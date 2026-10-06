using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class TestResult
    {
        public int TestResultId{get;set;}
        public string ResultValue{get;set;}=string.Empty;
        public string Flag{get;set;}=string.Empty;
        public string Remarks{get;set;}=string.Empty;
        public DateTime EnteredAt{get;set;}=DateTime.UtcNow;

        public int SpecimenId{get;set;}
        public Specimen Specimen{get;set;}=null!;

        public int TestId{get;set;}
        public Test Test{get;set;}=null!;

        public int? EnteredByLabTechnicianId{get;set;}
        public LabTechnician? EnteredByLabTechnician{get;set;}
    }
}
