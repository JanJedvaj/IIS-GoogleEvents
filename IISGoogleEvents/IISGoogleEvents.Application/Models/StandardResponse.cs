namespace IISGoogleEvents.Application.Models;

public enum ResultStatus
{
    Ok,
    Created,
    BadRequest,
    NotFound,
    Unauthorized,
    Forbidden,
    Conflict,
    InternalError
}

/// <summary>
/// Uniform envelope returned by every service, so that REST, GraphQL and SOAP
/// can each translate one shape into their own error conventions.
/// </summary>
public class StandardResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public ResultStatus Status { get; set; }
    public string? Message { get; set; }
    public IEnumerable<string>? Errors { get; set; }

    public static StandardResponse<T> Create(
        ResultStatus status,
        T? data = default,
        string? message = null,
        IEnumerable<string>? errors = null)
    {
        return new StandardResponse<T>
        {
            Status = status,
            Success = status is ResultStatus.Ok or ResultStatus.Created,
            Data = data,
            Message = message,
            Errors = errors
        };
    }
}
