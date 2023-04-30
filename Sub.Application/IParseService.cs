namespace Sub.ApplicationLayer;

public interface IParseService
{
    event Action<string> OnError;
    event Action<MsgPackage> OnMessage;

    Task PushBlock(byte[] block);
}