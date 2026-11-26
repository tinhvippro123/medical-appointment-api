using MedicalAppointment.API.Entities;
using MedicalAppointment.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointment.API.Middlewares;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Hệ thống có lỗi: {Message}", exception.Message);
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
            UnauthorizedAccessException => new ErrorDetails
            {
                StatusCode = StatusCodes.Status403Forbidden,
                ErrorCode = ErrorCode.FORBIDDEN.Code,
                Message = ErrorCode.FORBIDDEN.Message
            },
            DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx) => new ErrorDetails
            {
                StatusCode = StatusCodes.Status409Conflict,
                ErrorCode = ErrorCode.DUPLICATE_RESOURCE.Code,
                Message = ErrorCode.DUPLICATE_RESOURCE.Message
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

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        Exception? current = ex.InnerException;
        while (current != null)
        {
            if (current is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
            {
                return true;
            }
            current = current.InnerException;
        }
        return false;
    }
}
