using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SorrisoApi.Models.DTOs;
using SorrisoApi.Settings;

namespace SorrisoApi.Services
{
    public class AcessaSiteSeleniumService
    {
        private readonly SeleniumSettings _settings;

        public AcessaSiteSeleniumService(IOptions<SeleniumSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<List<DiaEscalaDTO>> ConsultarEscalaProgramada(LoginDTO login)
        {
            var options = new ChromeOptions();

            var ambiente = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (ambiente == "Production")
            {
                options.AddArgument("--headless=new");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--blink-settings=imagesEnabled=false");
            }

            using var driver = new ChromeDriver(options);

            driver.Navigate().GoToUrl(_settings.TargetUrl);

            var nomeDoUsuario = driver.FindElement(By.Name(_settings.SelectorUsuario));
            var senha = driver.FindElement(By.Name(_settings.SelectorSenha));
            var entrar = driver.FindElement(By.Name(_settings.SelectorLoginBtn));

            nomeDoUsuario.SendKeys(login.CPD);
            senha.SendKeys(login.Senha);
            entrar.Submit();

            var trafego = driver.FindElement(By.Id(_settings.SelectorTrafego));
            trafego.Click();

            var escalaProgramada = driver.FindElement(By.Id(_settings.SelectorEscalaPro));
            escalaProgramada.Click();

            var tabelaEscalaProgramada = driver.FindElement(By.Id(_settings.SelectorTabela));
            var linhas = tabelaEscalaProgramada.FindElements(By.TagName("tr"));

            var escala = new List<DiaEscalaDTO>();

            foreach (var linha in linhas)
            {
                var colunas = linha.FindElements(By.TagName("td"));

                if (colunas.Count > 15)
                {
                    escala.Add(new DiaEscalaDTO
                    {
                        Data = colunas[1].Text,
                        Dia = colunas[2].Text,
                        Tipo = colunas[3].Text,
                        Local = colunas[6].Text,
                        Equipamento = colunas[8].Text,
                        HoraInicio = colunas[11].Text,
                        HoraFim = colunas[12].Text,
                        Cargo = colunas[15].Text
                    });
                }
            }

            return escala;
        }
    }
}
