using MedicalAppointment.API.Data;
using MedicalAppointment.API.DTOs.PatientProfile;
using MedicalAppointment.API.Entities;
using MedicalAppointment.API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointment.API.Services;

public class PatientProfileService(ApplicationDbContext context) : IPatientProfileService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<PatientProfileResponse>> GetAllByUserIdAsync(int userId)
    {
        return await _context.PatientProfiles
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Id)
            .Select(p => new PatientProfileResponse
            {
                Id = p.Id,
                FullName = p.FullName,
                DateOfBirth = p.DateOfBirth,
                Gender = p.Gender,
                Ethnicity = p.Ethnicity,
                Occupation = p.Occupation,
                Phone = p.Phone,
                Address = p.Address,
                IdentityCardNumber = p.IdentityCardNumber,
                HealthInsuranceNumber = p.HealthInsuranceNumber,
                Relationship = p.Relationship,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<PatientProfileResponse> CreateAsync(int userId, CreatePatientProfileRequest request)
    {
        var profile = new PatientProfile
        {
            UserId = userId,
            FullName = request.FullName.Trim(),
            DateOfBirth = request.DateOfBirth!.Value,
            Gender = request.Gender.Trim(),
            Ethnicity = string.IsNullOrWhiteSpace(request.Ethnicity) ? null : request.Ethnicity.Trim(),
            Occupation = string.IsNullOrWhiteSpace(request.Occupation) ? null : request.Occupation.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim(),
            IdentityCardNumber = string.IsNullOrWhiteSpace(request.IdentityCardNumber) ? null : request.IdentityCardNumber.Trim(),
            HealthInsuranceNumber = string.IsNullOrWhiteSpace(request.HealthInsuranceNumber) ? null : request.HealthInsuranceNumber.Trim(),
            Relationship = request.Relationship.Trim()
        };

        _context.PatientProfiles.Add(profile);
        await _context.SaveChangesAsync();

        return MapToResponse(profile);
    }

    public async Task<PatientProfileResponse> UpdateAsync(int id, int userId, UpdatePatientProfileRequest request)
    {
        var profile = await _context.PatientProfiles
            .Where(p => p.Id == id && p.UserId == userId)
            .FirstOrDefaultAsync() ?? throw new NotFoundException(ErrorCode.RESOURCE_NOT_FOUND);

        profile.FullName = request.FullName.Trim();
        profile.DateOfBirth = request.DateOfBirth!.Value;
        profile.Gender = request.Gender.Trim();
        profile.Ethnicity = string.IsNullOrWhiteSpace(request.Ethnicity) ? null : request.Ethnicity.Trim();
        profile.Occupation = string.IsNullOrWhiteSpace(request.Occupation) ? null : request.Occupation.Trim();
        profile.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        profile.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
        profile.IdentityCardNumber = string.IsNullOrWhiteSpace(request.IdentityCardNumber) ? null : request.IdentityCardNumber.Trim();
        profile.HealthInsuranceNumber = string.IsNullOrWhiteSpace(request.HealthInsuranceNumber) ? null : request.HealthInsuranceNumber.Trim();
        profile.Relationship = request.Relationship.Trim();

        await _context.SaveChangesAsync();

        return MapToResponse(profile);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var profile = await _context.PatientProfiles
            .Where(p => p.Id == id && p.UserId == userId)
            .FirstOrDefaultAsync() ?? throw new NotFoundException(ErrorCode.RESOURCE_NOT_FOUND);

        bool hasCartItems = await _context.CartItems.AnyAsync(c => c.PatientProfileId == id);
        bool hasOrderDetails = await _context.OrderDetails.AnyAsync(o => o.PatientProfileId == id);

        if (hasCartItems || hasOrderDetails)
        {
            throw new BadRequestException(ErrorCode.PROFILE_IN_USE);
        }

        _context.PatientProfiles.Remove(profile);
        await _context.SaveChangesAsync();
    }

    private static PatientProfileResponse MapToResponse(PatientProfile profile)
    {
        return new PatientProfileResponse
        {
            Id = profile.Id,
            FullName = profile.FullName,
            DateOfBirth = profile.DateOfBirth,
            Gender = profile.Gender,
            Ethnicity = profile.Ethnicity,
            Occupation = profile.Occupation,
            Phone = profile.Phone,
            Address = profile.Address,
            IdentityCardNumber = profile.IdentityCardNumber,
            HealthInsuranceNumber = profile.HealthInsuranceNumber,
            Relationship = profile.Relationship,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }
}
