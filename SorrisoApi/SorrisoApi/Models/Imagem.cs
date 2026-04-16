namespace SorrisoApi.Models
{
    public class Imagem
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Texto { get; set; }
        public string tipo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
