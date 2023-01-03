namespace Plants.Domain.Projection;

[Serializable]
public class RepositoryException : Exception
{
    public RepositoryErrorCode ErrorCode { get; }
    public RepositoryException() { }

    public RepositoryException(string message, RepositoryErrorCode errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    public RepositoryException(string message, Exception inner, RepositoryErrorCode errorCode) : base(message, inner)
    {
        ErrorCode = errorCode;
    }
}

public enum RepositoryErrorCode
{
    AlreadyExists, NotFound, Other
}