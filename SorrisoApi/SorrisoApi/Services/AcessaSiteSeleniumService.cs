using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SorrisoApi.Models.DTOs;

namespace SorrisoApi.Services
{
    public class AcessaSiteSeleniumService
    {
        public IWebDriver driver;

        public AcessaSiteSeleniumService()
        {
            driver = new ChromeDriver();
        }

        public async Task<List<DiaEscalaDTO>> ConsultarEscalaProgramada()
        {
            driver
                .Navigate()
                .GoToUrl("https://acessaSiteComSelenium123.com.br");

            var nomeDoUsuario = driver.FindElement(By.Name("Usuario"));
            var senha = driver.FindElement(By.Name("Senha"));
            var entrar = driver.FindElement(By.Name("Login"));

            nomeDoUsuario.SendKeys("usuario");
            senha.SendKeys("senha");
            entrar.Submit();

            var trafego = driver.FindElement(By.Id("botaoTrafego"));
            trafego.Click();

            var escalaProgramada = driver.FindElement(By.Id("botaoEscala"));
            escalaProgramada.Click();

            var tabelaEscalaProgramada = driver.FindElement(By.Id("tabela"));
            var linhas = tabelaEscalaProgramada.FindElements(By.TagName("tr"));
            //var tabela = tabelaEscalaProgramada.GetAttribute("outerHTML");
            
            var escala = new List<DiaEscalaDTO>();

            foreach (var linha in linhas)
            {
                var colunas = linha.FindElements(By.TagName("td"));
                if (colunas.Count > 1)
                {
                    var diaEscala = new DiaEscalaDTO
                    {
                        Dia = colunas[0].Text,
                        HoraInicio = colunas[1].Text,
                        HoraFim = colunas[2].Text,
                        Local = colunas[3].Text
                    };
                    escala.Add(diaEscala);
                }
            }
            return escala;
        }
    }
}
