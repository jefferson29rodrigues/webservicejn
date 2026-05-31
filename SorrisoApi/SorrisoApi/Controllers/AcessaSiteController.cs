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
        private readonly ILogger<AcessaSiteController> _logger;

        public AcessaSiteController(AcessaSiteSeleniumService acessaSiteService, ILogger<AcessaSiteController> logger)
        {
            _acessaSiteService = acessaSiteService;
            _logger = logger;
        }

        [HttpPost("escalaProgramada")]
        public async Task<IActionResult> ObterEscalaProgramada([FromBody] LoginDTO login)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDTO<object>
                    {
                        Success = false,
                        ErrorMessage = "Dados inválidos."
                    });
                }

                var escala = await _acessaSiteService.ConsultarEscalaProgramada(login);

                return Ok(new ApiResponseDTO<List<DiaEscalaDTO>>
                {
                    Success = true,
                    Data = escala
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponseDTO<object>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar escala.");

                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    ErrorMessage = "Erro interno ao processar requisição."
                });
            }
        }
    }
}