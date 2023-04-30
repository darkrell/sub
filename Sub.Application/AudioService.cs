using CSCore.SoundIn;
using CSCore;
using CSCore.Streams;
using CSCore.Codecs.WAV;

namespace Sub.ApplicationLayer;

public class AudioService : IDisposable
{
    private readonly WasapiLoopbackCapture _wasapiCapture = new();
    private readonly SoundInSource _soundInSource;
    private readonly IWaveSource _source;
    private readonly IParseService _parseService;

    private readonly WaveWriter _waveWriter;

    public AudioService(IParseService parseService)
    {
        _parseService = parseService;
        _wasapiCapture.Initialize();
        _soundInSource = new SoundInSource(_wasapiCapture, _wasapiCapture.WaveFormat.BytesPerSecond);
        _source = _soundInSource
            .ChangeSampleRate(16000)
            .ToSampleSource()
            .ToWaveSource(16).ToMono();

        byte[] buffer = new byte[_source.WaveFormat.BytesPerSecond];
        _soundInSource.DataAvailable += async (s, e) =>
        {
            int read = _source.Read(buffer, 0, buffer.Length);
            var copy = new byte[read];
            buffer[..read].CopyTo(copy, 0);
            await _parseService.PushBlock(copy);
        };
    }

    public void Start() => _wasapiCapture.Start();
    public void Stop() => _wasapiCapture.Stop();
    public void Dispose()
    {
        _source.Dispose();
        _soundInSource.Dispose();
        _wasapiCapture.Dispose();

        _waveWriter.Dispose();
    }
}
public class DeepGramService
{
    public async Task Push()
    {

    }
}
