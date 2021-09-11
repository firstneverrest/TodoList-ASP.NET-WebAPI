using System;
using System.Collections.Generic;

#nullable disable

namespace TodoApi.Models
{
    public partial class User
    {
        public string Id { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
    }
}
