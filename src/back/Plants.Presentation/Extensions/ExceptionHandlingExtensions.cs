using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Plants.Presentation.Extensions;

public static class ExceptionHandlingExtensions
{
    public static void UseCustomErrors(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.Use(DevErrorHandler);
        }
        else
        {
            app.Use(ProdErrorHandler);
        }
    }

    private static Task DevErrorHandler(HttpContext httpContext, Func<Task> next)
    {
        return HandleError(httpContext, true);
    }

    private static Task ProdErrorHandler(HttpContext httpContext, Func<Task> next)
    {
        return HandleError(httpContext, false);
    }
    private static async Task HandleError(HttpContext httpContext, bool includeDetails)
    {
        var exceptionDetails = httpContext.Features.Get<IExceptionHandlerFeature>();
        var exc = exceptionDetails?.Error;

        httpContext.Response.ContentType = "application/problem+json";
        if (exc is not null)
        {
            if (exc is UnauthorizedAccessException)
            {
                await ProcessException(httpContext, exc, includeDetails, 401, exc.Message);
            }
            else
            {
                await ProcessUnexpectedException(httpContext, exc, includeDetails);
            }
        }
    }
    private static Task ProcessExpectedException(HttpContext httpContext, Exception exc, bool includeDetails)
    {
        return ProcessException(httpContext, exc, includeDetails, 400, exc.Message);
    }
    private static Task ProcessUnexpectedException(HttpContext httpContext, Exception exc, bool includeDetails)
    {
        return ProcessException(httpContext, exc, includeDetails, 500, "An error has occured");
    }

    private static Task ProcessException(HttpContext httpContext, Exception exc, bool includeDetails, int statusCode,
        string title)
    {
        string details = null;
        if (includeDetails)
        {
            title += $" {exc.Message}";
            details = exc.ToString();
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = details
        };
        httpContext.Response.StatusCode = statusCode;
        var stream = httpContext.Response.Body;
        return JsonSerializer.SerializeAsync(stream, problem);
    }
}
