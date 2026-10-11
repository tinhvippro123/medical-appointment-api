using MedicalAppointment.API.Data;
using MedicalAppointment.API.DTOs;
using MedicalAppointment.API.Entities;
using MedicalAppointment.API.Exceptions;
using MedicalAppointment.API.Provider;
using MedicalAppointment.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointment.API.Services;

public class AuthService(ApplicationDbContext context, IJwtProvider jwtProvider, IPasswordHasher passwordHasher) : IAuthService
{
    public async Task Register(RegisterRequest request)
    {
        var isExist = await context.Users.AnyAsync(u => u.Phone == request.Phone);
        if (isExist)
        {
            throw new BadRequestException(ErrorCode.PHONE_ALREADY_EXISTS);
        }

        var patientRole = await context.Roles.FirstOrDefaultAsync(r => r.Key == "PATIENT") ?? throw new Exception("Role PATIENT không tồn tại trong DB.");
        // if (patientRole == null)
        // {
        //     throw new Exception("Role PATIENT không tồn tại trong DB.");
        // }

        var user = new User
        {
            FullName = request.FullName,
            Phone = request.Phone,
            Password = passwordHasher.Hash(request.Password),
            RoleId = patientRole.Id,
            IsActive = true
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

    }
    public async Task<string> Login(LoginRequest request)
    {
        var user = await context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Phone == request.Phone);
        if (user == null || !passwordHasher.Verifty(request.Password, user.Password))
        {
            throw new BadRequestException(ErrorCode.INVALID_CREDENTIALS);
        }

        if (!user.IsActive)
        {
            throw new BadRequestException(ErrorCode.ACCOUNT_DISABLED);
        }

        return jwtProvider.GenerateToken(user);
    }

 
}