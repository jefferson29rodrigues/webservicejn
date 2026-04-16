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

        public ProcessarImagemService(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "tessdata");
        }

        public string ExtractText(Stream imageStream)
        {
            try
            {
                using var engine = new TesseractEngine(_dataPath, _language, EngineMode.Default);

                using var ms = new MemoryStream();
                imageStream.CopyTo(ms);

                using var img = Pix.LoadFromMemory(ms.ToArray());

                using var page = engine.Process(img);

                var text = page.GetText();
                return text;
            }
            catch (Exception ex)
            {
                return $"Erro ao processar OCR: {ex.Message}";
            }
        }
    }
}
