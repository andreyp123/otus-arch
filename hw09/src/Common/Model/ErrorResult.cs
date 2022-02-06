namespace Common.Model
{
    public class ErrorResult
    {
        public string? ErrorId { get; set; }
        public string? Message { get; set; }

        public static ErrorResult FromException(Exception ex)
        {
            return new ErrorResult
            {
                ErrorId = ex.GetType().Name,
                Message = ex.Message
            };
        }
    }
}
