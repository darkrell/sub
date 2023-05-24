using Deepgram;
using System.Net.WebSockets;

namespace Sub.ApplicationLayer;

public class DeepGramTokenValidator : ITokenValidator
{
    public async Task<bool> Validate(string token)
    {
        if (token == null) return false;

        var credentials = new Credentials(token);
        var deepgramClient = new DeepgramClient(credentials);

        using var client = deepgramClient.CreateLiveTranscriptionClient();
        await client.StartConnectionAsync(new());

        var result = client.State() == WebSocketState.Open;
        await client.StopConnectionAsync();
        return result;
    }
}
