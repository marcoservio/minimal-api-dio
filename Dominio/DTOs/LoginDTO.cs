using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace minimals_api.Dominio.DTOs
{
    public class LoginDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }
}