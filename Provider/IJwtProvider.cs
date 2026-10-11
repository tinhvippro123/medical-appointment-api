using MedicalAppointment.API.Entities;

namespace MedicalAppointment.API.Provider;

public interface IJwtProvider
{
    string GenerateToken(User user);
}