using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class Role
    {
        public int RoleId{get;set;}
        public string Name{get;set;}=string.Empty;
        public ICollection<User> Users{get;set;}=new List<User>();
    }
}