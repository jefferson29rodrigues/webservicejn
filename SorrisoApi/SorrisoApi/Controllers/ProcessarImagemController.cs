using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SorrisoApi.Models;
using SorrisoApi.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SorrisoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("fixed")]
    public class ProcessarImagemController : ControllerBase
    {
        private readonly IOcrService _processarImagemService;

        public ProcessarImagemController(IOcrService processarImagemService)
        {
            _processarImagemService = processarImagemService;
        }

        // GET: api/<ProcessarImagemController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Hello World", "Jeff!" };
        }

        // POST api/<ProcessarImagemController>
        [HttpPost]
        public async Task<IActionResult> Post(IFormFile imagem)
        {
            using var stream = imagem.OpenReadStream();

            var extractedText = _processarImagemService.ExtractText(stream);

            //var dadosExtraidos = new
            //{
            //    Nome = ParseNome(extractedText),
            //    Data = ParseData(extractedText)
            //};

            var dadosExtraidos = new
            {
                TextoBruto = extractedText
            };

            return Ok(dadosExtraidos);
        }
    }
}
