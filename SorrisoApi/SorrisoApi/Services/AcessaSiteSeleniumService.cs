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
            var options = new ChromeOptions();
            var ambiente = Environment.GetEnvironmentVariable("ASNETCORE_ENVIRONMENT");
            if (ambiente == "Production")
            {
                options.AddArgument("--headless");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage"); // Evita problemas de falta de memória no Linux
            }
            driver = new ChromeDriver(options);
        }

        public async Task<List<DiaEscalaDTO>> ConsultarEscalaProgramada(LoginDTO login)
        {
            string urlTarget = Environment.GetEnvironmentVariable("TARGET_URL") ?? "http://siteparaacessarcomselenium.com.br";
            string selectorUsuario = Environment.GetEnvironmentVariable("SELECTOR_USER") ?? "Usuario";
            string selectorSenha = Environment.GetEnvironmentVariable("SELECTOR_PASS") ?? "Senha";
            string selectorLoginBtn = Environment.GetEnvironmentVariable("SELECTOR_LOGIN_BTN") ?? "Login";
            string selectorTrafego = Environment.GetEnvironmentVariable("SELECTOR_TRAFEGO") ?? "trafego";
            string selectorEscalaPro = Environment.GetEnvironmentVariable("SELECTOR_ESCALAPRO") ?? "escalapro";
            string selectorTabela = Environment.GetEnvironmentVariable("SELECTOR_TABELA") ?? "tabela";

            driver.Navigate().GoToUrl(urlTarget);

            var nomeDoUsuario = driver.FindElement(By.Name(selectorUsuario));
            var senha = driver.FindElement(By.Name(selectorSenha));
            var entrar = driver.FindElement(By.Name(selectorLoginBtn));

            nomeDoUsuario.SendKeys(login.CPD);
            senha.SendKeys(login.Senha);
            entrar.Submit();

            var trafego = driver.FindElement(By.Id(selectorTrafego));
            trafego.Click();

            var escalaProgramada = driver.FindElement(By.Id(selectorEscalaPro));
            escalaProgramada.Click();

            var tabelaEscalaProgramada = driver.FindElement(By.Id(selectorTabela));
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
