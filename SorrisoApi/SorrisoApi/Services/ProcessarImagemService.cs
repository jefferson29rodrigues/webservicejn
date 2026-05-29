using Tesseract;

namespace SorrisoApi.Services
{
    public interface IOcrService
    {
        string ExtractText(Stream imageStream);
    }

    public class ProcessarImagemService : IOcrService
    {
        private readonly string _dataPath;
        private readonly string _language = "por";
        private readonly ILogger<ProcessarImagemService> _logger;

        public ProcessarImagemService(IWebHostEnvironment env, ILogger<ProcessarImagemService> logger)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "tessdata");
            _logger = logger;
        }

        public string ExtractText(Stream imageStream)
        {
            if (imageStream == null || !imageStream.CanRead)
            {
                throw new ArgumentException("Imagem inválida.");
            }

            try
            {
                using var engine = new TesseractEngine(_dataPath, _language, EngineMode.Default);

                using var ms = new MemoryStream();
                imageStream.CopyTo(ms);

                using var img = Pix.LoadFromMemory(ms.ToArray());

                using var page = engine.Process(img);
                var text = page.GetText()?.Trim();

                return text ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar OCR.");
                throw new Exception("Erro ao processar imagem.");
            }
        }
    }
}