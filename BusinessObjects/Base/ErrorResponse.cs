namespace BusinessObjects.Base;

public class ErrorResponse
{
    public string ErrorCode { get; set; }
    public List<string> ErrorMessages { get; set; }

    public ErrorResponse()
    {
        ErrorMessages = new List<string>();
    }

    public ErrorResponse(string errorCode, string errorMessage)
    {
        ErrorCode = errorCode;
        ErrorMessages = new List<string> { errorMessage };
    }

    public void AddError(string errorMessage)
    {
        ErrorMessages.Add(errorMessage);
    }
}