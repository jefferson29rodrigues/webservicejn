using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SorrisoApi.Models.DTOs;
using SorrisoApi.Services;

namespace SorrisoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("fixed")]
    public class AcessaSiteController : ControllerBase
    {
        private readonly AcessaSiteSeleniumService _acessaSiteService;

        public AcessaSiteController(AcessaSiteSeleniumService acessaSiteService)
        {
            _acessaSiteService = acessaSiteService;
        }

        [HttpPost("escalaProgramada")]
        public async Task<IActionResult> ObterEscalaProgramada(LoginDTO login)
        {
            var escala = await _acessaSiteService.ConsultarEscalaProgramada(login);
            return Ok(escala);
        }
    }
}
