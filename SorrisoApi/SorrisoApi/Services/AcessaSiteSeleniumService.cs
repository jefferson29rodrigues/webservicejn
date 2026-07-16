using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SorrisoApi.Models.DTOs;
using SorrisoApi.Settings;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;

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

        public async Task<List<DiaEscalaDTO>> GetEscalaProgramada(LoginDTO login)
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

            var tempoTotalEscala = Stopwatch.StartNew();

            using var driver = new ChromeDriver(chromeOptions);

            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);

            try
            {
                var etapaEscala = Stopwatch.StartNew();

                driver.Navigate().GoToUrl(_settings.TargetUrl);

                _logger.LogInformation("Tempo abrir site: {Tempo} ms", etapaEscala.ElapsedMilliseconds);

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                etapaEscala.Restart();

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


                _logger.LogInformation("Tempo login: {Tempo} ms", etapaEscala.ElapsedMilliseconds);
                etapaEscala.Restart();

                var trafego = wait.Until(d => d.FindElement(By.Id(_settings.SelectorTrafego)));
                trafego.Click();

                _logger.LogInformation("Tempo abrir tráfego: {Tempo} ms", etapaEscala.ElapsedMilliseconds);
                etapaEscala.Restart();

                var escalaProgramada = wait.Until(d => d.FindElement(By.Id(_settings.SelectorEscalaPro)));
                escalaProgramada.Click();

                _logger.LogInformation("Tempo abrir escala: {Tempo} ms", etapaEscala.ElapsedMilliseconds);

                var tabelaEscalaProgramada = wait.Until(d => d.FindElement(By.Id(_settings.SelectorTabela)));
                var linhas = tabelaEscalaProgramada.FindElements(By.TagName("tr"));

                _logger.LogInformation("Quantidade de linhas encontradas: {Qtd}", linhas.Count);
                etapaEscala.Restart();

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

                _logger.LogInformation("Tempo processamento completo: {Tempo} ms", etapaEscala.ElapsedMilliseconds);
                _logger.LogInformation("Quantidade de registros extraídos: {Qtd}", escala.Count);
                _logger.LogInformation("Tempo total requisição Selenium: {Tempo} ms", tempoTotalEscala.ElapsedMilliseconds);

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
    
        public async Task<List<MensagemDTO>> GetMensagens(LoginDTO login)
        {
            if (login == null)
            {
                throw new ArgumentNullException("Login inválido");
            }

            if (string.IsNullOrWhiteSpace(login.CPD) || string.IsNullOrWhiteSpace(login.Senha))
            {
                throw new ArgumentException("Credenciais inválidas");
            }

            if (string.IsNullOrWhiteSpace(_settings.TargetUrl))
            {
                throw new InvalidOperationException("TargetUrl não configurada");
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
                chromeOptions.AddArgument("--mute audio");
                chromeOptions.AddArgument("--blink-settings=imagesEnabled=false");
                chromeOptions.AddArgument("--user-data-dir=/tmp/chrome-data");
                chromeOptions.AddArgument("--disk-cache-dir=/tmp/chrome-cache");
            }

            var tempoTotalMensagens = Stopwatch.StartNew();

            using var driver = new ChromeDriver(chromeOptions);

            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);

            try
            {
                var etapaMensagens = Stopwatch.StartNew();

                driver.Navigate().GoToUrl(_settings.TargetUrl);

                _logger.LogInformation("Tempo abrir site: {Tempo} ms", etapaMensagens.ElapsedMilliseconds);

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                etapaMensagens.Restart();

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

                var botaoMensagens = driver.FindElement(By.Id(_settings.SelectorMensagens));
                botaoMensagens.Click();

                var mensagens = new List<MensagemDTO>();

                // Aguarda a tabela aparecer
                var tabela = wait.Until(d => d.FindElement(By.Id("ContentPlaceHolder1_grvMensagens")));

                // Pega somente as linhas de dados (ignora o cabeçalho)
                var linhas = tabela.FindElements(By.XPath(".//tbody/tr[position()>1]"));

                // Guarda as URLs das 10 mensagens mais recentes
                var urlsMensagens = new List<string>();

                foreach (var linha in linhas.Take(10))
                {
                    var onclick = linha.GetAttribute("onclick");

                    var match = Regex.Match(onclick, @"'([^']*)'");

                    if (match.Success)
                    {
                        urlsMensagens.Add(match.Groups[1].Value);
                    }
                }

                // Guarda a URL da página da lista
                var urlLista = driver.Url;

                // Percorre cada mensagem
                foreach (var url in urlsMensagens)
                {
                    var uri = new Uri(new Uri(urlLista), url);

                    // Abre a mensagem
                    driver.Navigate().GoToUrl(uri);

                    // Aguarda carregar a página da mensagem
                    wait.Until(d => d.FindElement(By.Id("ContentPlaceHolder1_rtfMensagem"))); // ajuste para um elemento da tela

                    string id = uri.Query.Substring("?id=".Length);
                    var remetente = driver.FindElement(By.Id("ContentPlaceHolder1_txtRemetente")).GetAttribute("value");
                    var destinatario = driver.FindElement(By.Id("ContentPlaceHolder1_txtDestinatario")).GetAttribute("value");
                    var assunto = driver.FindElement(By.Id("ContentPlaceHolder1_txtAssunto")).GetAttribute("value");
                    var dataEnvio = DateTime.ParseExact
                    (
                        driver.FindElement(By.Id("ContentPlaceHolder1_txtDtEnvio")).GetAttribute("value")!,
                        "dd/MM/yyyy HH:mm:ss",
                        CultureInfo.InvariantCulture
                    );
                    var dataRecebimento = DateTime.ParseExact
                    (
                        driver.FindElement(By.Id("ContentPlaceHolder1_txtDtRecebimento")).GetAttribute("value"),
                        "dd/MM/yyyy HH:mm:ss",
                        CultureInfo.InvariantCulture
                    );
                    var conteudo = driver.FindElement(By.Id("ContentPlaceHolder1_rtfMensagem")).Text;

                    var mensagem = new MensagemDTO
                    {
                        Id = id,
                        Remetente = remetente,
                        Destinatario = destinatario,
                        Assunto = assunto,
                        DataEnvio = dataEnvio,
                        DataRecebimento = dataRecebimento,
                        Conteudo = conteudo,
                    };

                    mensagens.Add(mensagem);

                    // Volta para a lista
                    driver.Navigate().GoToUrl(urlLista);

                    wait.Until(d => d.FindElement(By.Id("ContentPlaceHolder1_grvMensagens")));
                }

                return mensagens;
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