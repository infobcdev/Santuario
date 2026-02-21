using Microsoft.AspNetCore.Builder;

namespace Santuario.Entidade.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseSantuarioExceptionHandling(this IApplicationBuilder app)
            => app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}