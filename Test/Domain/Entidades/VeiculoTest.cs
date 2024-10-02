using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using minimals_api.Dominio.Entitdades;

namespace Test.Domain.Entidades
{
    [TestClass]
    public class VeiculoTest
    {
        [TestMethod]
        public void TestarGetSetPropriedades()
        {
            var veiculo = new Veiculo();

            veiculo.Id = 1;
            veiculo.Marca = "Fiat";
            veiculo.Nome = "Stilo";
            veiculo.Ano = 2003;

            Assert.AreEqual(1, veiculo.Id);
            Assert.AreEqual("Fiat", veiculo.Marca);
            Assert.AreEqual("Stilo", veiculo.Nome);
            Assert.AreEqual(2003, veiculo.Ano);
        }
    }
}