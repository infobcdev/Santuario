using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;

namespace Santuario.Entidade.Helpers
{
    public class LogManager
    {
        private readonly IHostEnvironment _env;

        public LogManager(IHostEnvironment env)
        {
            _env = env;
        }

        public void LogError(Exception ex, HttpContext context, string? extra = null)
        {
            try
            {
                var logsDir = Path.Combine(_env.ContentRootPath, "Logs");
                Directory.CreateDirectory(logsDir);

                var file = Path.Combine(logsDir, $"erro_{DateTime.Now:dd-MM-yyyy}.txt");

                var sb = new StringBuilder();
                sb.AppendLine("==================================================");
                sb.AppendLine($"Data: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.GetCultureInfo("pt-BR"))}");
                sb.AppendLine($"RequestId: {context.TraceIdentifier}");
                sb.AppendLine($"Method: {context.Request.Method}");
                sb.AppendLine($"Path: {context.Request.Path}{context.Request.QueryString}");
                sb.AppendLine($"IP: {context.Connection.RemoteIpAddress}");
                sb.AppendLine($"UserAgent: {context.Request.Headers["User-Agent"]}");
                if (!string.IsNullOrWhiteSpace(extra))
                    sb.AppendLine($"Extra: {extra}");

                sb.AppendLine("---- Exception ----");
                sb.AppendLine(ex.ToString());
                sb.AppendLine();

                File.AppendAllText(file, sb.ToString(), Encoding.UTF8);
            }
            catch
            {
            }
        }
    }
}