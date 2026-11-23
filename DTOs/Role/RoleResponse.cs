namespace MedicalAppointment.API.DTOs.Role;

public class RoleResponse
{
    public int Id{set; get;}
    public string Key{set; get;} = string.Empty;
    public string Name{set; get;} = string.Empty;
    public string? Description{set; get;}
    public int UserCount{set; get;}
    public DateTime CreatedAt{set; get;}
    public DateTime? UpdatedAt{set; get;}


}