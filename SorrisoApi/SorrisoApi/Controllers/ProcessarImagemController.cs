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
        private readonly ILogger<ProcessarImagemController> _logger;
        private const long TAMANHO_MAXIMO_ARQUIVO = 5_000_000; // 5 MB

        private static readonly string[] TiposPermitidos =
        {
            "image/png",
            "image/jpeg",
            "image/jpg"
        };

        private static readonly string[] ExtensoesPermitidas =
        {
            ".png",
            ".jpg",
            ".jpeg"
        };

        public ProcessarImagemController(IOcrService processarImagemService, ILogger<ProcessarImagemController> logger)
        {
            _processarImagemService = processarImagemService;
            _logger = logger;
        }

        [HttpPost]
        [RequestSizeLimit(TAMANHO_MAXIMO_ARQUIVO)]
        public IActionResult Post([FromForm] IFormFile imagem)
        {
            try
            {
                if (imagem is null)
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

                var extensao = Path.GetExtension(imagem.FileName);

                if (!ExtensoesPermitidas.Contains(extensao, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Extensão do arquivo não permitida."
                    });
                }

                if (!TiposPermitidos.Contains(imagem.ContentType, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Formato de imagem não permitido."
                    });
                }

                using var stream = imagem.OpenReadStream();

                if (!ArquivoEhImagemValida(stream))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "O conteudo não é uma imagem válida."
                    });
                }

                stream.Position = 0;

                var textoExtraido = _processarImagemService.ExtractText(stream);

                return Ok(new
                {
                    Success = true,
                    TextoBruto = textoExtraido
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar imagem enviada para OCR.");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Erro ao processar imagem."
                });
            }
        }

        private static bool ArquivoEhImagemValida(Stream stream)
        {
            if (!stream.CanRead)
            {
                return false;
            }

            long posicaoOriginal = 0;

            if (stream.CanSeek)
            {
                posicaoOriginal = stream.Position;
            }

            try
            {
                var header = new byte[4];
                var bytesLidos = stream.Read(header, 0, header.Length);

                if (bytesLidos < 4)
                {
                    return false;
                }

                bool isPng = header[0] == 0x89
                          && header[1] == 0x50
                          && header[2] == 0x4E
                          && header[3] == 0x47;

                bool isJpeg = header[0] == 0xFF
                           && header[1] == 0xD8
                           && header[2] == 0xFF;

                return isPng || isJpeg;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = posicaoOriginal;
                }
            }
        }
    }
}