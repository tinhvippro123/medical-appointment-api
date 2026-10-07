namespace MedicalAppointment.API.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(ErrorCode errorCode) : base(errorCode) { }
}
