using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SorrisoApi.Services;

namespace SorrisoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("fixed")]
    public class ProcessarImagemController : ControllerBase
    {
        private readonly IOcrService _processarImagemService;

        private const long TAMANHO_MAXIMO_ARQUIVO = 5_000_000; // 5 MB

        private static readonly string[] TiposPermitidos =
        {
            "image/png",
            "image/jpeg",
            "image/jpg"
        };

        public ProcessarImagemController(
            IOcrService processarImagemService)
        {
            _processarImagemService = processarImagemService;
        }

        [HttpPost]
        [RequestSizeLimit(TAMANHO_MAXIMO_ARQUIVO)]
        public IActionResult Post([FromForm] IFormFile imagem)
        {
            try
            {
                if (imagem == null)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Imagem não enviada."
                    });
                }

                if (imagem.Length == 0)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Imagem inválida."
                    });
                }

                if (imagem.Length > TAMANHO_MAXIMO_ARQUIVO)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Imagem excede o tamanho permitido."
                    });
                }

                if (!TiposPermitidos.Contains(imagem.ContentType.ToLower()))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Formato de imagem não permitido."
                    });
                }

                using var stream = imagem.OpenReadStream();
                var textoExtraido = _processarImagemService.ExtractText(stream);

                return Ok(new
                {
                    Success = true,
                    TextoBruto = textoExtraido
                });
            }
            catch
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Erro ao processar imagem."
                });
            }
        }
    }
}