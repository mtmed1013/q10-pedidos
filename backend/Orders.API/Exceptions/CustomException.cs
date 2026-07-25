using System.Net;

namespace Orders.API.Exceptions;

public class CustomException : Exception
{
    public int Code { get; }

    public CustomException(int code, string message)
        : base(message)
    {
        Code = code;
    }
}