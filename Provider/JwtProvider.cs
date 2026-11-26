using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MedicalAppointment.API.Entities;
using Microsoft.IdentityModel.Tokens;

namespace MedicalAppointment.API.Provider;

public class JwtProvider(IConfiguration config, IHostEnvironment hostEnvironment) : IJwtProvider
{
    public string GenerateToken(User user)
    {
        var claims = new List<Claim>{
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.MobilePhone, user.Phone),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.Key)
        };
        
        var jwtKey = config["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            if (hostEnvironment.IsDevelopment())
            {
                jwtKey = "DevOnly_MedicalApp_LocalSigningKey_2026_ChangeInProduction!";
            }
            else
            {
                throw new InvalidOperationException("Chưa cấu hình khóa bí mật Jwt:Key.");
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}