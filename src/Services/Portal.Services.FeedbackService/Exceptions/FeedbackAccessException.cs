using System.Runtime.Serialization;

namespace Portal.Services.FeedbackService.Exceptions;

[Serializable]
public class FeedbackAccessException: Exception
{
    public FeedbackAccessException() { }
    public FeedbackAccessException(string message) : base(message) { }
    public FeedbackAccessException(string message, Exception ex) : base(message, ex) { }
    protected FeedbackAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}