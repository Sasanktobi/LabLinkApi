using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class AvailabilitySlot
    {
        public int AvailabilitySlotId{get;set;}
        public DateTime SlotDate{get;set;}
        public string StartTime{get;set;}=string.Empty;
        public string EndTime{get;set;}=string.Empty;
        public bool IsBooked{get;set;}=false;
        public string ProviderType{get;set;}=string.Empty;
        public DateTime CreatedAt{get;set;}=DateTime.UtcNow;

        // Concurrency token: two simultaneous bookings of the same slot make the second SaveChanges fail.
        public byte[] RowVersion{get;set;}=Array.Empty<byte>();

        public int UserId{get;set;}
        public User User{get;set;}=null!;

        public ICollection<Appointment> Appointments{get;set;}=new List<Appointment>();
    }
}
