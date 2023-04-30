namespace Sub.ApplicationLayer;

public record MsgPackage
{
    public string Message { get; set; }
    public bool IsFinal { get; set; }
}