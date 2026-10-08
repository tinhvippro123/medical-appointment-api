using MedicalAppointment.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using MedicalAppointment.API.Entities;

namespace MedicalAppointment.API.Middlewares;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError("Hệ thống có lỗi {exception.Message}",exception.Message);
        var errorResponse = exception switch
        {
            BadRequestException badRequestException => new ErrorDetails
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ErrorCode = badRequestException.ErrorCode.Code,
                Message = badRequestException.Message
            },
            NotFoundException notFoundException => new ErrorDetails
            {
                StatusCode = StatusCodes.Status404NotFound,
                ErrorCode = notFoundException.ErrorCode.Code,
                Message = notFoundException.Message
            },
            AppException appException => new ErrorDetails
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ErrorCode = appException.ErrorCode.Code,
                Message = appException.ErrorCode.Message
            },
            _ => new ErrorDetails
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                ErrorCode = ErrorCode.INTERNAL_SERVER_ERROR.Code,
                Message = ErrorCode.INTERNAL_SERVER_ERROR.Message
            }
        };

        httpContext.Response.StatusCode = errorResponse.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);
        return true;
    }
}
