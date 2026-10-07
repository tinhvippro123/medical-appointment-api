namespace MedicalAppointment.API.Exceptions;

public class BadRequestException : AppException{
    public BadRequestException(ErrorCode errorCode) : base(errorCode){}
}   