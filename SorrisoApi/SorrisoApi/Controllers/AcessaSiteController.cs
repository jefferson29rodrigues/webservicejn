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
            try
            {
                var escala = await _acessaSiteService.ConsultarEscalaProgramada(login);
                return Ok(new ApiResponseDTO<List<DiaEscalaDTO>>
                {
                    Success = true,
                    Data = escala
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseDTO<List<DiaEscalaDTO>>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}
