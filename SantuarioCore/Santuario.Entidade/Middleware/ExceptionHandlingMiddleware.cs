using Microsoft.AspNetCore.Http;
using Santuario.Entidade.Helpers;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Santuario.Entidade.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        private static bool IsAjax(HttpContext ctx)
            => string.Equals(ctx.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

        public async Task Invoke(HttpContext context, LogManager log)
        {
            try
            {
                await _next(context);

                // ⚠️ Se a resposta for redirect para Login em chamada AJAX -> vira 401 (pra JS tratar certo)
                if (context.Response.StatusCode == (int)HttpStatusCode.Redirect &&
                    IsAjax(context) &&
                    context.Response.Headers["Location"].ToString().Contains("/Login", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.Headers.Remove("Location");
                }
            }
            // ====== CLIENTE ABORTOU (fechou navegador / caiu net) ======
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                if (!context.Response.HasStarted)
                    context.Response.StatusCode = 499; // Client Closed Request
            }
            catch (TaskCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                if (!context.Response.HasStarted)
                    context.Response.StatusCode = 499;
            }
            // ====== CANCELAMENTO/TIMEOUT INTERNO (não foi o cliente) ======
            catch (OperationCanceledException ex)
            {
                log.LogError(ex, context, "timeout interno");

                if (IsAjax(context))
                {
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync("{\"success\":false,\"message\":\"A operação demorou mais que o esperado. Tente novamente.\"}");
                    }
                }
                else
                {
                    RedirectToError(context, 504);
                }
            }
            // ====== DEMAIS EXCEÇÕES ======
            catch (Exception ex)
            {
                log.LogError(ex, context);

                if (IsAjax(context))
                {
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync("{\"success\":false,\"message\":\"Ocorreu um erro interno. Tente novamente em instantes.\"}");
                    }
                }
                else
                {
                    RedirectToError(context, 500);
                }
            }
        }

        private static void RedirectToError(HttpContext context, int code)
        {
            if (context.Response.HasStarted) return;

            var originalPath = (context.Request.Path + context.Request.QueryString).ToString();
            var url = $"/Error/{code}"
                      + $"?originalPath={Uri.EscapeDataString(originalPath)}"
                      + $"&requestId={Uri.EscapeDataString(context.TraceIdentifier)}";

            context.Response.Redirect(url);
        }
    }
}