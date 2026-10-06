using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class TestPanelTest
    {
        public int TestPanelId{get;set;}
        public TestPanel TestPanel{get;set;}=null!;

        public int TestId{get;set;}
        public Test Test{get;set;}=null!;
    }
}
