using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SorrisoApi.Models.DTOs;
using SorrisoApi.Settings;
using System.Diagnostics;

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

            if (string.IsNullOrWhiteSpace(login.CPD) || string.IsNullOrWhiteSpace(login.Senha))
            {
                throw new ArgumentException("Credenciais inválidas.");
            }

            if (string.IsNullOrWhiteSpace(_settings.TargetUrl))
            {
                throw new InvalidOperationException("TargetUrl não configurada.");
            }

            var chromeOptions = new ChromeOptions();

            var ambiente = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (ambiente == "Production")
            {
                chromeOptions.AddArgument("--headless=new");
                chromeOptions.AddArgument("--no-sandbox");
                chromeOptions.AddArgument("--disable-setuid-sandbox");
                chromeOptions.AddArgument("--disable-dev-shm-usage");
                chromeOptions.AddArgument("--disable-gpu");
                chromeOptions.AddArgument("--no-zygote");
                chromeOptions.AddArgument("--window-size=1280,720");
                chromeOptions.AddArgument("--disable-extensions");
                chromeOptions.AddArgument("--disable-background-networking");
                chromeOptions.AddArgument("--disable-sync");
                chromeOptions.AddArgument("--mute-audio");
                chromeOptions.AddArgument("--blink-settings=imagesEnabled=false");
                chromeOptions.AddArgument("--user-data-dir=/tmp/chrome-data");
                chromeOptions.AddArgument("--disk-cache-dir=/tmp/chrome-cache");
            }

            var tempoTotal = Stopwatch.StartNew();

            using var driver = new ChromeDriver(chromeOptions);

            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);

            try
            {
                var etapa = Stopwatch.StartNew();

                driver.Navigate().GoToUrl(_settings.TargetUrl);

                _logger.LogInformation("Tempo abrir site: {Tempo} ms", etapa.ElapsedMilliseconds);

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                etapa.Restart();

                var nomeDoUsuario = wait.Until(d => d.FindElement(By.Name(_settings.SelectorUsuario)));
                var senha = driver.FindElement(By.Name(_settings.SelectorSenha));
                var entrar = driver.FindElement(By.Name(_settings.SelectorLoginBtn));

                nomeDoUsuario.SendKeys(login.CPD);
                senha.SendKeys(login.Senha);
                entrar.Submit();

                var tituloPagina = driver.Title;
                if (tituloPagina == "Login - Radsystem")
                {
                    var botaoOk = driver.FindElement(By.ClassName("swal-button--confirm"));
                    botaoOk.Click();
                    driver.Close();
                    throw new UnauthorizedAccessException("Usuário ou senha inválidos");
                }

                // <title> Login - Radsystem </title>
                // if (class="swal-button swal-button--confirm")
                // <title> Radsystem </ title >
                // <div class="swal-text" style>Colaborador não encontrado.</div>
                // <div class="swal-text" style>Senha inválida. Verifique.</div>


                _logger.LogInformation("Tempo login: {Tempo} ms", etapa.ElapsedMilliseconds);
                etapa.Restart();

                var trafego = wait.Until(d => d.FindElement(By.Id(_settings.SelectorTrafego)));
                trafego.Click();

                _logger.LogInformation("Tempo abrir tráfego: {Tempo} ms", etapa.ElapsedMilliseconds);
                etapa.Restart();

                var escalaProgramada = wait.Until(d => d.FindElement(By.Id(_settings.SelectorEscalaPro)));
                escalaProgramada.Click();

                _logger.LogInformation("Tempo abrir escala: {Tempo} ms", etapa.ElapsedMilliseconds);

                var tabelaEscalaProgramada = wait.Until(d => d.FindElement(By.Id(_settings.SelectorTabela)));
                var linhas = tabelaEscalaProgramada.FindElements(By.TagName("tr"));

                _logger.LogInformation("Quantidade de linhas encontradas: {Qtd}", linhas.Count);
                etapa.Restart();

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

                _logger.LogInformation("Tempo processamento completo: {Tempo} ms", etapa.ElapsedMilliseconds);
                _logger.LogInformation("Quantidade de registros extraídos: {Qtd}", escala.Count);
                _logger.LogInformation("Tempo total requisição Selenium: {Tempo} ms", tempoTotal.ElapsedMilliseconds);

                return escala;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "CPD ou senha inválidos");
                throw;
            }
            catch (WebDriverTimeoutException ex)
            {
                _logger.LogWarning(ex, "Timeout Selenium.");
                throw new TimeoutException("Tempo excedido ao acessar sistema.");
            }
            catch (NoSuchElementException ex)
            {
                _logger.LogWarning(ex, "Elemento não encontrado.");
                throw new InvalidOperationException("Erro ao localizar dados da escala.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro Selenium.");
                throw new Exception("Erro ao acessar sistema.");
            }
        }
    }
}