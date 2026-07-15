namespace SorrisoApi.Models.DTOs
{
    public class MensagemDTO
    {
        public string Id {  get; set; }
        public string Remetente { get; set; }
        public string Destinatario { get; set; }
        public string? Assunto { get; set; }
        public string Conteudo { get; set; }
        public DateTime DataEnvio { get; set; }
        public DateTime DataRecebimento { get; set; }
        public bool Lida { get; set; } = true;
        public DateTime DataLeitura { get; set; }
    }
}
