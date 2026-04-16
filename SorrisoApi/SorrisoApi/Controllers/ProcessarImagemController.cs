using Microsoft.AspNetCore.Mvc;
using SorrisoApi.Models;
using SorrisoApi.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SorrisoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            return new string[] { "value1", "value2" };
        }

        // GET api/<ProcessarImagemController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
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

        // PUT api/<ProcessarImagemController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ProcessarImagemController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
