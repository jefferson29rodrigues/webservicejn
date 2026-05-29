using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SorrisoApi.Models.DTOs;
using SorrisoApi.Settings;

namespace SorrisoApi.Services
{
    public class AcessaSiteSeleniumService
    {
        private readonly SeleniumSettings _settings;

        private readonly ILogger<AcessaSiteSeleniumService> _logger;

        public AcessaSiteSeleniumService(IOptions<SeleniumSettings> options, ILogger<AcessaSiteSeleniumService> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<List<DiaEscalaDTO>> ConsultarEscalaProgramada(LoginDTO login)
        {
            if (login == null)
            {
                throw new ArgumentException("Login inválido.");
            }

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
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(20);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            try
            {
                driver.Navigate().GoToUrl(_settings.TargetUrl);

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                var nomeDoUsuario = wait.Until(d => d.FindElement(By.Name(_settings.SelectorUsuario)));
                var senha = driver.FindElement(By.Name(_settings.SelectorSenha));
                var entrar = driver.FindElement(By.Name(_settings.SelectorLoginBtn));

                nomeDoUsuario.SendKeys(login.CPD);
                senha.SendKeys(login.Senha);
                entrar.Submit();

                var trafego = wait.Until(d => d.FindElement(By.Id(_settings.SelectorTrafego)));
                trafego.Click();

                var escalaProgramada = wait.Until(d => d.FindElement(By.Id(_settings.SelectorEscalaPro)));
                escalaProgramada.Click();

                var tabelaEscalaProgramada = wait.Until(d => d.FindElement(By.Id(_settings.SelectorTabela)));
                var linhas = tabelaEscalaProgramada.FindElements(By.TagName("tr"));

                var escala = new List<DiaEscalaDTO>();

                foreach (var linha in linhas)
                {
                    var colunas = linha.FindElements(By.TagName("td"));
                    if (colunas.Count > 15)
                    {
                        escala.Add(new DiaEscalaDTO
                        {
                            Data = colunas[1].Text.Trim(),
                            Dia = colunas[2].Text.Trim(),
                            Tipo = colunas[3].Text.Trim(),
                            Local = colunas[6].Text.Trim(),
                            Equipamento = colunas[8].Text.Trim(),
                            HoraInicio = colunas[11].Text.Trim(),
                            HoraFim = colunas[12].Text.Trim(),
                            Cargo = colunas[15].Text.Trim()
                        });
                    }
                }

                return escala;
            }
            catch (WebDriverTimeoutException ex)
            {
                _logger.LogWarning(ex, "Timeout Selenium.");
                throw new Exception("Tempo excedido ao acessar sistema.");
            }
            catch (NoSuchElementException ex)
            {
                _logger.LogWarning(ex, "Elemento não encontrado.");
                throw new Exception("Erro ao localizar dados da escala.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro Selenium.");
                throw new Exception("Erro ao acessar sistema.");
            }
        }
    }
}