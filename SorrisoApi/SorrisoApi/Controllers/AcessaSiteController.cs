using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SorrisoApi.Models.DTOs;
using SorrisoApi.Services;

namespace SorrisoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcessaSiteController : ControllerBase
    {
        private readonly AcessaSiteSeleniumService _acessaSiteService;

        public AcessaSiteController(AcessaSiteSeleniumService acessaSiteService)
        {
            _acessaSiteService = acessaSiteService;
        }

        [HttpGet("escalaProgramada")]
        public async Task<IActionResult> ObterEscalaProgramada()
        {
            var escala = await _acessaSiteService.ConsultarEscalaProgramada();
            return Ok(escala);
        }
    }
}
