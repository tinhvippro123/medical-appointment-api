namespace MedicalAppointment.API.Provider;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verifty(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}