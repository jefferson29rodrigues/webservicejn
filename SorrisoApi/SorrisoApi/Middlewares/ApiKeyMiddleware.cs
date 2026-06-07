namespace SorrisoApi.Middlewares
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string APIKEYNAME = "X-Api-Key";

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key não fornecida");
                return;
            }

            var apiKey = configuration.GetValue<string>("ApiKeySettings:ApiKey");
            if (string.IsNullOrEmpty(apiKey) || !CompararApiKey(apiKey, extractedApiKey!))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key inválida");
                return;
            }

            await _next(context);
        }

        private static bool CompararApiKey(string chaveEsperada, string chaveRecebida)
        {
            var bytesEsperados = System.Text.Encoding.UTF8.GetBytes(chaveEsperada);
            var bytesRecebidos = System.Text.Encoding.UTF8.GetBytes(chaveRecebida);
            return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(bytesEsperados, bytesRecebidos);
        }
    }
}
