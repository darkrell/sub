namespace Sub.ApplicationLayer;

public interface ITokenContainer
{

    event Action<string> OnTokenChange;

    Task<bool> SetToken(string token);
    Task<string> GetToken();
}