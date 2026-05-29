namespace SorrisoApi.Models.DTOs
{
    public class ApiResponseDTO<T>
    {
        public bool Success { get; set; }

        public string ErrorMessage { get; set; }

        public T Data { get; set; }
    }
}
