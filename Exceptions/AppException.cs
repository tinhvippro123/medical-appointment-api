namespace MedicalAppointment.API.Exceptions;

public abstract class AppException(ErrorCode errorCode) : Exception(errorCode.Message){
    public ErrorCode ErrorCode { get; } = errorCode;
}