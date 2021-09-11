using System;
using System.Collections.Generic;

#nullable disable

namespace TodoApi.Models
{
    public partial class Activity
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public DateTime When { get; set; }
    }
}
