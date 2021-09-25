using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace MVC_F83345
{
    public static class PasswordHash
    {
        public static string PHash(string V)
        {

            return Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(V)));
        }
    }
}