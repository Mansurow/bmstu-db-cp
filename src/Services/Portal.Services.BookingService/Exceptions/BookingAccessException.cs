using System.Runtime.Serialization;

namespace Portal.Services.BookingService.Exceptions;

[Serializable]
public class BookingAccessException: Exception
{
    public BookingAccessException() { }
    public BookingAccessException(string message) : base(message) { }
    public BookingAccessException(string message, Exception ex) : base(message, ex) { }
    protected BookingAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}