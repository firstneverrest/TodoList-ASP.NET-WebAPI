using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApi.Utils
{
    public class Account
    {
        public string userid { get; set; }
        public string password { get; set; }
    }
}
