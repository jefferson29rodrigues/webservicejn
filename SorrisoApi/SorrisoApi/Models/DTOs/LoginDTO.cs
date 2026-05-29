using System.ComponentModel.DataAnnotations;

namespace SorrisoApi.Models.DTOs
{
    public class LoginDTO
    {
        [Required]
        [MaxLength(20)]
        [RegularExpression(@"^[0-9]+$")]
        public string CPD { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Senha { get; set; } = string.Empty;
    }
}
