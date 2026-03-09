using CaseItau.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics.CodeAnalysis;

namespace CaseItau.API.Filters
{
    [ExcludeFromCodeCoverage]
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is DomainException domainEx)
            {
                _logger.LogWarning("Business rule violation: {Message}", domainEx.Message);

                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Title = "Business rule violation.",
                    Detail = domainEx.Message
                };

                context.Result = new ObjectResult(problemDetails)
                {
                    StatusCode = StatusCodes.Status422UnprocessableEntity
                };

                context.ExceptionHandled = true;
                return;
            }

            _logger.LogError(context.Exception, "Unhandled exception occurred.");

            var unexpectedProblem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = context.Exception.Message
            };

            context.Result = new ObjectResult(unexpectedProblem)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };

            context.ExceptionHandled = true;
        }
    }
}
