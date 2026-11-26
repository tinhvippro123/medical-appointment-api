namespace MedicalAppointment.API.Provider;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verifty(string password, string hash);
}