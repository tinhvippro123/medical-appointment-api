namespace MedicalAppointment.API.Exceptions;

public class BadRequestException(ErrorCode errorCode) : AppException(errorCode);