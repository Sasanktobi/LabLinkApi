using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class Receptionist
    {
        public int ReceptionistId{get;set;}
        public string ShiftTiming{get;set;}=string.Empty;
        public string BranchLocation{get;set;}=string.Empty;
        public DateTime CreatedAt{get;set;}=DateTime.UtcNow;

        public int UserId{get;set;}
        public User User{get;set;}=null!;

    }
}