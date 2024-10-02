using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimals_api.Dominio.Enums;

namespace minimals_api.Dominio.ModelViews
{
    public record AdministradorModelView
    {
        public int Id { get; set; }
        public string Email { get; set; } = default!;
        public string Perfil { get; set; } = default!;
    }
}