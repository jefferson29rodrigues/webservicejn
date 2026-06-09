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

                // Aponta para /tmp para evitar problemas de permissão
                chromeOptions.AddArgument("--user-data-dir=/tmp/chrome-data");
                chromeOptions.AddArgument("--disk-cache-dir=/tmp/chrome-cache");
            }

            // =====================================================
            // DIAGNÓSTICO TEMPORÁRIO — remover este bloco após
            // identificar e resolver o problema do Chrome no Render
            // =====================================================
            try
            {
                var testProcess = new System.Diagnostics.Process();
                testProcess.StartInfo.FileName = "google-chrome";
                testProcess.StartInfo.Arguments = "--headless=new --no-sandbox --disable-gpu --dump-dom about:blank";
                testProcess.StartInfo.RedirectStandardOutput = true;
                testProcess.StartInfo.RedirectStandardError = true;
                testProcess.StartInfo.UseShellExecute = false;
                testProcess.Start();

                var stdout = testProcess.StandardOutput.ReadToEnd();
                var stderr = testProcess.StandardError.ReadToEnd();
                testProcess.WaitForExit(10000);

                _logger.LogInformation("Chrome teste - exit code: {Code}", testProcess.ExitCode);
                _logger.LogInformation("Chrome teste - stdout: {Out}", stdout[..Math.Min(500, stdout.Length)]);
                _logger.LogWarning("Chrome teste - stderr: {Err}", stderr[..Math.Min(500, stderr.Length)]);
            }
            catch (Exception diagEx)
            {
                _logger.LogError(diagEx, "Chrome teste - falha ao executar processo diretamente.");
            }
            // =====================================================
            // FIM DO DIAGNÓSTICO TEMPORÁRIO
            // =====================================================

            var tempoTotal = Stopwatch.StartNew();

            // =====================================================
            // DIAGNÓSTICO TEMPORÁRIO — substituir por:
            //   using var driver = new ChromeDriver(chromeOptions);
            // após resolver o problema
            // =====================================================
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.EnableVerboseLogging = true;
            driverService.LogPath = "/tmp/chromedriver.log";

            using var driver = new ChromeDriver(driverService, chromeOptions);
            // =====================================================
            // FIM DO DIAGNÓSTICO TEMPORÁRIO
            // =====================================================

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