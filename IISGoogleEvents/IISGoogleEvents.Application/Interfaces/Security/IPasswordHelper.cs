namespace IISGoogleEvents.Application.Interfaces.Security;

public interface IPasswordHelper
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
