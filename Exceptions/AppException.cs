namespace MedicalAppointment.API.Exceptions;

public abstract class AppException : Exception{
    public ErrorCode ErrorCode { get; }

    protected AppException(ErrorCode errorCode) : base(errorCode.Message){
        ErrorCode = errorCode;
    }
}