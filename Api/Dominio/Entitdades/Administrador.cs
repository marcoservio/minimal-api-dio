using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace minimals_api.Dominio.Entitdades
{
    public class Administrador
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [StringLength(50)]
        [Required]
        public string Senha { get; set; } = string.Empty;

        [StringLength(10)]
        [Required]
        public string Perfil { get; set; } = string.Empty;
    }
}