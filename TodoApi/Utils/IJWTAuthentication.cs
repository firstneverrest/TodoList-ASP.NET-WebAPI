using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoApi.Utils
{
    public interface IJWTAuthentication
    {
        string GenerateJwtToken(string userid);
        string ValidateJwtToken(string token);
    }
}