using System;
using System.Threading.Tasks;
using Backend.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionHandlingMiddleware> logger;

        public ExceptionHandlingMiddleware(RequestDelegate _next, ILogger<ExceptionHandlingMiddleware> _logger)
        {
            next=_next;
            logger=_logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await next(httpContext);
            }
            catch (Exception ex)
            {
                var (status, title)=ex switch
                {
                    DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Conflict"),
                    DbUpdateException dbEx when IsUniqueViolation(dbEx) => (StatusCodes.Status409Conflict, "Conflict"),
                    NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
                    BadRequestException => (StatusCodes.Status400BadRequest, "Bad Request"),
                    ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
                    ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
                    // UserRepository/UserService signal duplicate emails this way.
                    InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
                    _ => (StatusCodes.Status500InternalServerError, "Server Error")
                };

                if (status == StatusCodes.Status500InternalServerError)
                {
                    logger.LogError(ex, "Unhandled exception for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
                }

                if (httpContext.Response.HasStarted)
                {
                    throw;
                }

                var problem=new ProblemDetails
                {
                    Status=status,
                    Title=title,
                    // Never leak internal exception messages to clients.
                    Detail=ex switch
                    {
                        _ when status == StatusCodes.Status500InternalServerError => "An unexpected error occurred.",
                        DbUpdateConcurrencyException => "The record was changed by someone else (e.g. the slot was just booked). Reload and try again.",
                        DbUpdateException => "A record with the same unique value already exists.",
                        _ => ex.Message
                    },
                    Instance=httpContext.Request.Path
                };

                httpContext.Response.Clear();
                httpContext.Response.StatusCode=status;
                await httpContext.Response.WriteAsJsonAsync(problem, (System.Text.Json.JsonSerializerOptions?)null, "application/problem+json");
            }
        }

        // SQL Server: 2601 = duplicate key in unique index, 2627 = unique constraint violation.
        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627);
        }
    }
}
