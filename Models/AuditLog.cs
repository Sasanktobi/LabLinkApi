using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class AuditLog
    {
        public int AuditLogId{get;set;}
        public string EntityName{get;set;}=string.Empty;
        public int EntityId{get;set;}
        public string Action{get;set;}=string.Empty;
        public string Details{get;set;}=string.Empty;
        public DateTime TimeStamp{get;set;}=DateTime.UtcNow;

        public int? UserId{get;set;}
        public User? User{get;set;}
    }
}
