namespace VGT.Galaxy.Backend.Services.SignalManagement.Domain.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
    {
        Errors = errors;
    }
}