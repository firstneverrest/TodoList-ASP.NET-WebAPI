using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace TodoApi.Models
{
    public partial class Activity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; }
        public string Name { get; set; }
        public DateTime When { get; set; }
    }
}
