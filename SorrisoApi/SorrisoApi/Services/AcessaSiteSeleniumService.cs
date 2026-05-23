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
                .GoToUrl("http://totem.sorrisodecuritiba.com.br:9810/rs1totem/LoginTotem.aspx?parms=qh1kmJh2Tluczckdz5b0LjyDhmklei7AAZctMlFgCgsWprEYquaNTASRIvhJsbxYyMxPRV1Dp0Cz7U6+ijtjs/nU6t5aT3RcJGufuAyhqVURVcbdwY9O0Kh6dA5rfAKrzZrNjGuJhnN+rWf3xFFQNA==");

            var nomeDoUsuario = driver.FindElement(By.Name("ctl00$ContentPlaceHolder1$txtUsuario"));
            var senha = driver.FindElement(By.Name("ctl00$ContentPlaceHolder1$txtSenha"));
            var entrar = driver.FindElement(By.Name("ctl00$ContentPlaceHolder1$btnLogin"));

            nomeDoUsuario.SendKeys("9849");
            senha.SendKeys("20719&gWi");
            entrar.Submit();

            var trafego = driver.FindElement(By.Id("dtlMenu_lnkImageMenu_6"));
            trafego.Click();

            var escalaProgramada = driver.FindElement(By.Id("dtlMenu_lnkImageMenu_0"));
            escalaProgramada.Click();

            var tabelaEscalaProgramada = driver.FindElement(By.Id("ContentPlaceHolder1_grvEscalaProgramadaRealizada"));
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
                        Data = colunas[1].Text,
                        Dia = colunas[2].Text,
                        Tipo = colunas[3].Text,
                        Local = colunas[6].Text,
                        Equipamento = colunas[8].Text,
                        HoraInicio = colunas[11].Text,
                        HoraFim = colunas[12].Text,
                        Cargo = colunas[12].Text
                    };
                    escala.Add(diaEscala);
                }
            }
            driver.Quit();
            return escala;
        }
    }
}
