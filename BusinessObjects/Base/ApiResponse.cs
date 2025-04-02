namespace BusinessObjects.Base
{

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public ErrorResponse Errors { get; set; }

        public ApiResponse()
        {
            Success = true;
            Message = string.Empty;
            Errors = null;
        }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Operation completed successfully")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message,
                Errors = null
            };
        }

        public static ApiResponse<T> ErrorResponse(string message, ErrorResponse errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Data = default,
                Message = message,
                Errors = errors
            };
        }
    }
}

   
    
   
