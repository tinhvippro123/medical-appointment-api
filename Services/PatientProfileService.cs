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
        ValidateRequest(request.FullName, request.DateOfBirth, request.Gender, request.Relationship,
            request.Ethnicity, request.Occupation, request.IdentityCardNumber, request.HealthInsuranceNumber);

        var profile = new PatientProfile
        {
            UserId = userId,
            FullName = request.FullName.Trim(),
            DateOfBirth = request.DateOfBirth,
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
        ValidateRequest(request.FullName, request.DateOfBirth, request.Gender, request.Relationship,
            request.Ethnicity, request.Occupation, request.IdentityCardNumber, request.HealthInsuranceNumber);

        var profile = await _context.PatientProfiles
            .Where(p => p.Id == id && p.UserId == userId)
            .FirstOrDefaultAsync() ?? throw new NotFoundException(ErrorCode.RESOURCE_NOT_FOUND);

        profile.FullName = request.FullName.Trim();
        profile.DateOfBirth = request.DateOfBirth;
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
            throw new BadRequestException(new ErrorCode("PROFILE_IN_USE", "Hồ sơ đang được sử dụng trong giỏ hàng hoặc đơn hàng, không thể xóa!"));
        }

        _context.PatientProfiles.Remove(profile);
        await _context.SaveChangesAsync();
    }

    private static void ValidateRequest(
        string fullName, DateTime dateOfBirth, string gender, string relationship,
        string? ethnicity, string? occupation, string? identityCardNumber, string? healthInsuranceNumber)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new BadRequestException(new ErrorCode("INVALID_FULL_NAME", "Họ tên không được để trống!"));
        
        if (string.IsNullOrWhiteSpace(gender))
            throw new BadRequestException(new ErrorCode("INVALID_GENDER", "Giới tính không được để trống!"));

        if (string.IsNullOrWhiteSpace(relationship))
            throw new BadRequestException(new ErrorCode("INVALID_RELATIONSHIP", "Mối quan hệ không được để trống!"));

        if (fullName.Trim().Length > 100)
            throw new BadRequestException(new ErrorCode("MAX_LENGTH_EXCEEDED", "Họ tên không được vượt quá 100 ký tự!"));

        if (gender.Trim().Length > 10)
            throw new BadRequestException(new ErrorCode("MAX_LENGTH_EXCEEDED", "Giới tính không được vượt quá 10 ký tự!"));

        if (relationship.Trim().Length > 50)
            throw new BadRequestException(new ErrorCode("MAX_LENGTH_EXCEEDED", "Mối quan hệ không được vượt quá 50 ký tự!"));

        if (!string.IsNullOrWhiteSpace(ethnicity) && ethnicity.Trim().Length > 50)
            throw new BadRequestException(new ErrorCode("MAX_LENGTH_EXCEEDED", "Dân tộc không được vượt quá 50 ký tự!"));

        if (!string.IsNullOrWhiteSpace(occupation) && occupation.Trim().Length > 100)
            throw new BadRequestException(new ErrorCode("MAX_LENGTH_EXCEEDED", "Nghề nghiệp không được vượt quá 100 ký tự!"));

        if (!string.IsNullOrWhiteSpace(identityCardNumber) && identityCardNumber.Trim().Length > 20)
            throw new BadRequestException(new ErrorCode("MAX_LENGTH_EXCEEDED", "CCCD/CMND không được vượt quá 20 ký tự!"));

        if (!string.IsNullOrWhiteSpace(healthInsuranceNumber) && healthInsuranceNumber.Trim().Length > 50)
            throw new BadRequestException(new ErrorCode("MAX_LENGTH_EXCEEDED", "Số BHYT không được vượt quá 50 ký tự!"));

        if (dateOfBirth > DateTime.UtcNow)
            throw new BadRequestException(new ErrorCode("INVALID_DOB", "Ngày sinh không được ở tương lai!"));
            
        if (dateOfBirth < new DateTime(1900, 1, 1))
            throw new BadRequestException(new ErrorCode("INVALID_DOB", "Ngày sinh không được trước năm 1900!"));
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
