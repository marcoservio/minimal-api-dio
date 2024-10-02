using minimals_api.Dominio.Entitdades;
using minimals_api.Dominio.Interfaces;
using minimals_api.Infraestrutura.Db;

namespace minimals_api.Dominio.Servicos
{
    public class VeiculoService : IVeiculoService
    {
        private readonly DbContexto _context;

        public VeiculoService(DbContexto context)
        {
            _context = context;
        }

        public void Apagar(Veiculo veiculo)
        {
            _context.Veiculos.Remove(veiculo);
            _context.SaveChanges();
        }

        public void Atualizar(Veiculo veiculo)
        {
            _context.Veiculos.Update(veiculo);
            _context.SaveChanges();
        }

        public Veiculo? BuscarPorId(int id)
        {
            return _context.Veiculos.FirstOrDefault(v => v.Id == id);
        }

        public void Incluir(Veiculo veiculo)
        {
            _context.Veiculos.Add(veiculo);
            _context.SaveChanges();
        }

        public List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null)
        {
            var query = _context.Veiculos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
                query.Where(v => v.Nome.Contains(nome, StringComparison.InvariantCultureIgnoreCase));

            if (!string.IsNullOrWhiteSpace(marca))
                query.Where(v => v.Marca.Contains(marca, StringComparison.InvariantCultureIgnoreCase));

            int itensPorPagina = 10;

            if (pagina.HasValue)
                query = query.Skip((pagina.Value - 1) * itensPorPagina).Take(itensPorPagina);

            return query.ToList();
        }
    }
}