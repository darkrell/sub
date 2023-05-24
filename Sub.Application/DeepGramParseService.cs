using Deepgram;
using Deepgram.Transcription;
using System.Diagnostics;
using System.Net.WebSockets;

namespace Sub.ApplicationLayer;

public class DeepGramParseService : IDisposable, IParseService
{
    private readonly ITokenContainer _tokenContainer;
    private readonly SemaphoreSlim _semaphoreSlim = new(1);
    private ILiveTranscriptionClient? _client;

    public event Action<string> OnError = delegate { };
    public event Action<MsgPackage> OnMessage = delegate { };
    public DeepGramParseService(ITokenContainer tokenContainer)
    {
        _tokenContainer = tokenContainer;
        _tokenContainer.OnTokenChange += InitializeFromEvent;
    }
    private void InitializeFromEvent(string token)
        => _ = Initialize();
    private async Task Initialize()
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            if (_client != null && _client.State() == WebSocketState.Open) return; 
            Debug.WriteLine("Reconnection");

            await (_client?.StopConnectionAsync() ?? Task.CompletedTask);
            _client?.Dispose();

            var credentials = new Credentials(await _tokenContainer.GetToken());
            var deepgramClient = new DeepgramClient(credentials);

            _client = deepgramClient.CreateLiveTranscriptionClient();
            _client.TranscriptReceived += Receive;
            _client.ConnectionError += OnErrorFromDeepgram;
            _client.ConnectionClosed += OnClosedFromDeepgram;
            await _client.StartConnectionAsync(new()
            {
                Punctuate = true,
                InterimResults = true,
                SampleRate = 16000,
                Encoding = Deepgram.Common.AudioEncoding.Linear16
            });
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private void OnClosedFromDeepgram(object? sender, ConnectionClosedEventArgs e) => OnError.Invoke($"Connection closed");

    private void OnErrorFromDeepgram(object? sender, ConnectionErrorEventArgs e) => OnError.Invoke(e.Exception.Message);

    public async Task PushBlock(byte[] block)
    {
        if (_client == null) await Initialize();
        var state = _client?.State();
        switch (state)
        {
            case WebSocketState.None:
            case WebSocketState.CloseSent:
            case WebSocketState.CloseReceived:
            case WebSocketState.Closed:
            case WebSocketState.Aborted:
            default:
                OnError.Invoke($"Connection refused");
                await Initialize();
                return;
            case WebSocketState.Connecting:
                return;
            case WebSocketState.Open:
                break;
        }
        await _semaphoreSlim.WaitAsync();
        try
        {
            _client.SendData(block);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private void Receive(object? sender, TranscriptReceivedEventArgs e) => OnMessage.Invoke(new() { Message = e.Transcript?.Channel?.Alternatives.FirstOrDefault()?.Transcript ?? "", IsFinal = e.Transcript?.IsFinal ?? false });

    public void Dispose() => _client?.Dispose();
}
