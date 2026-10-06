using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class TestUpsertDto
    {
        [Required]
        [MaxLength(150)]
        public string Name{get;set;}=string.Empty;

        [Required]
        [MaxLength(30)]
        public string Code{get;set;}=string.Empty;

        [MaxLength(1000)]
        public string Description{get;set;}=string.Empty;

        // e.g. Blood, Urine, Stool, Swab.
        [Required]
        [MaxLength(50)]
        public string SampleType{get;set;}=string.Empty;

        [Range(0, 1000000)]
        public decimal Price{get;set;}

        // Numeric bounds enable automatic Low/High flagging; leave empty for qualitative tests.
        [MaxLength(50)]
        public string ReferenceRangeLow{get;set;}=string.Empty;

        [MaxLength(50)]
        public string ReferenceRangeHigh{get;set;}=string.Empty;

        [MaxLength(30)]
        public string Unit{get;set;}=string.Empty;

        [Range(1, 720)]
        public int TurnaroundHours{get;set;}=24;
    }

    public class TestResponseDto
    {
        public int TestId{get;set;}
        public string Name{get;set;}=string.Empty;
        public string Code{get;set;}=string.Empty;
        public string Description{get;set;}=string.Empty;
        public string SampleType{get;set;}=string.Empty;
        public decimal Price{get;set;}
        public string ReferenceRangeLow{get;set;}=string.Empty;
        public string ReferenceRangeHigh{get;set;}=string.Empty;
        public string Unit{get;set;}=string.Empty;
        public int TurnaroundHours{get;set;}
        public bool IsActive{get;set;}
    }

    public class TestPanelUpsertDto
    {
        [Required]
        [MaxLength(150)]
        public string Name{get;set;}=string.Empty;

        public string Description{get;set;}=string.Empty;

        [Range(0, 1000000)]
        public decimal Price{get;set;}

        [Required]
        [MinLength(1)]
        public List<int> TestIds{get;set;}=new List<int>();
    }

    public class TestPanelResponseDto
    {
        public int TestPanelId{get;set;}
        public string Name{get;set;}=string.Empty;
        public string Description{get;set;}=string.Empty;
        public decimal Price{get;set;}
        public decimal IndividualTestsTotal{get;set;}
        public bool IsActive{get;set;}
        public List<TestResponseDto> Tests{get;set;}=new List<TestResponseDto>();
    }
}
