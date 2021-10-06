using System;

namespace TodoApi.DTOs 
{
    public class ActivityDTO 
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public DateTime When { get; set; } 
    }
}