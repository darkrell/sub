using Sub.ApplicationLayer;

namespace Sub.Data;

public class TokenContainer : ITokenContainer
{
    private readonly ITokenValidator _tokenValidator;

    public TokenContainer(ITokenValidator tokenValidator)
    {
        _tokenValidator = tokenValidator;
        SecureStorage.Default.GetAsync("speech-to-text-token")
            .ContinueWith(async x => _token = await x);
    }

    private string _token;
    public event Action<string> OnTokenChange = delegate { };

    public async Task<string> GetToken()
    {
        _token ??= await SecureStorage.Default.GetAsync("speech-to-text-token");
        return _token;
    }

    public async Task<bool> SetToken(string token)
    {
        if (token == null || !await _tokenValidator.Validate(token)) return false;

        _token = token;
        OnTokenChange(token);
        await SecureStorage.Default.SetAsync("speech-to-text-token", token);

        return true;
    }
}