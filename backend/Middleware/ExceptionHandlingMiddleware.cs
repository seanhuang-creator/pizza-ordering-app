
using Microsoft.AspNetCore.Mvc;
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _log;
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> log){ _next=next; _log=log; }
    public async Task Invoke(HttpContext ctx)
    {
        try{ await _next(ctx); }
        catch(Exception ex)
        {
            _log.LogError(ex, "Unhandled");
            ctx.Response.StatusCode=500;
            ctx.Response.ContentType="application/problem+json";
            var pd = new ProblemDetails{ Title="Internal error", Detail="An unexpected error occurred", Status=500 };
            await ctx.Response.WriteAsJsonAsync(pd);
        }
    }
}
