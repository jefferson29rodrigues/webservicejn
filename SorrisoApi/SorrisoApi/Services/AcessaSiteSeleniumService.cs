using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SorrisoApi.Services
{
    public class AcessaSiteSeleniumService
    {
        public IWebDriver driver;

        public AcessaSiteSeleniumService()
        {
            driver = new ChromeDriver();
        }

        public void testeAcesso()
        {
            driver
                .Navigate()
                .GoToUrl("https://siteparaacessarcomseleniumeconsultarescala.com.br");

            var nomeDoUsuario = driver.FindElement(By.Name("usuario"));
            var senha = driver.FindElement(By.Name("senha"));
            var entrar = driver.FindElement(By.Name("login"));

            nomeDoUsuario.SendKeys("usuario");
            senha.SendKeys("senha");
            entrar.Submit();

            var trafego = driver.FindElement(By.Id("botaoTrafego"));
            trafego.Click();

            var escalaProgramada = driver.FindElement(By.Id("botaoEscala"));
            escalaProgramada.Click();

            var tabelaEscalaProgramada = driver.FindElement(By.Id("tabelaEscala")).Text;
            Console.WriteLine(tabelaEscalaProgramada);
        }
    }
}
