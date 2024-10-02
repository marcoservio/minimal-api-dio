using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace minimals_api.Dominio.ModelViews
{
    public record AdmLogadoModelView
    {
        public string Email { get; set; } = string.Empty;
        public string Perfil { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}