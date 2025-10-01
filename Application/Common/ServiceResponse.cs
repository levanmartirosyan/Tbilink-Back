namespace Tbilink_Back.Models
{
    public class ServiceResponse<T>
    {
        public T Data { get; set; }
        public bool IsSuccess { get; set; } = false;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
