namespace Sub.ApplicationLayer;

public interface ITokenValidator
{
    Task<bool> Validate(string token);
}