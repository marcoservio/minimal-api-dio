using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using minimals_api.Dominio.DTOs;
using minimals_api.Dominio.Entitdades;
using minimals_api.Dominio.Enums;
using minimals_api.Dominio.Servicos;
using minimals_api.Infraestrutura.Db;

namespace Test.Domain.Servicos
{
    [TestClass]
    public class AdministradorServiceTest
    {
        private DbContexto CriarContextoDeTeste()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            var context = new DbContexto(configuration);

            context.Database.ExecuteSqlRaw("TRUNCATE TABLE administradores");

            return context;
        }

        [TestMethod]
        public void TestandoSalvarAdministrador()
        {
            var context = CriarContextoDeTeste();

            var adm = new Administrador();

            adm.Email = "teste@teste.com";
            adm.Senha = "teste";
            adm.Perfil = nameof(Perfil.Adm);

            var service = new AdministradorService(context);

            service.Incluir(adm);

            Assert.AreEqual(1, service.Todos(1).Count());
        }

        [TestMethod]
        public void TestandoBuscarAdministradorPorId()
        {
            var context = CriarContextoDeTeste();

            var adm = new Administrador();

            adm.Email = "teste@teste.com";
            adm.Senha = "teste";
            adm.Perfil = nameof(Perfil.Adm);

            var service = new AdministradorService(context);

            service.Incluir(adm);
            var admBuscado = service.BuscarPorId(adm.Id);

            Assert.AreEqual(1, admBuscado.Id);
        }

        [TestMethod]
        public void TestandoLogin()
        {
            var context = CriarContextoDeTeste();

            var adm = new Administrador();

            adm.Email = "teste@teste.com";
            adm.Senha = "teste";
            adm.Perfil = nameof(Perfil.Adm);

            var service = new AdministradorService(context);

            service.Incluir(adm);

            var usuarioLogado = service.Login(new LoginDTO { Email = adm.Email, Senha = adm.Senha });

            Assert.IsNotNull(usuarioLogado);
        }
    }
}