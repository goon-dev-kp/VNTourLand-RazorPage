using Common.DTO;

namespace BLL.Services.Implement
{
    public class ResponseDTO<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
       
    }
}