namespace MedicalAppointment.API.Exceptions;

public class NotFoundException(ErrorCode errorCode) : AppException(errorCode);
