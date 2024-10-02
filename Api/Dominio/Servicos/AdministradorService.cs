using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimals_api.Dominio.DTOs;
using minimals_api.Dominio.Entitdades;
using minimals_api.Dominio.Interfaces;
using minimals_api.Infraestrutura.Db;

namespace minimals_api.Dominio.Servicos
{
    public class AdministradorService : IAdministradorService
    {
        private readonly DbContexto _context;

        public AdministradorService(DbContexto context)
        {
            _context = context;
        }

        public Administrador BuscarPorId(int id)
        {
            return _context.Administradores.FirstOrDefault(a => a.Id == id)!;
        }

        public Administrador Incluir(Administrador administrador)
        {
            _context.Administradores.Add(administrador);
            _context.SaveChanges();

            return administrador;
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            var adm = _context.Administradores.FirstOrDefault(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha);

            if (adm != null)
                return adm;

            return null;
        }

        public List<Administrador> Todos(int? pagina)
        {
            var query = _context.Administradores.AsQueryable();

            int itensPorPagina = 10;

            if (pagina.HasValue)
                query = query.Skip((pagina.Value - 1) * itensPorPagina).Take(itensPorPagina);

            return query.ToList();
        }
    }
}