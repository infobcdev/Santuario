using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Image = SixLabors.ImageSharp.Image;

namespace Santuario.Negocio.Helpers
{
    public static class ImagemUploadHelper
    {
        public const long DefaultMaxSizeBytes = 2 * 1024 * 1024; // 2MB
        private static readonly string[] DefaultAllowedExt = new[] { ".png", ".jpg", ".jpeg" };

        /// <summary>
        /// Garante a pasta física (wwwroot/{pastaPublica}) e retorna o caminho físico completo.
        /// </summary>
        public static string EnsureFolder(string webRootPath, string pastaPublica)
        {
            var pastaFisica = Path.Combine(webRootPath, pastaPublica);
            if (!Directory.Exists(pastaFisica))
                Directory.CreateDirectory(pastaFisica);

            return pastaFisica;
        }

        /// <summary>
        /// Salva imagem redimensionada em wwwroot/{pastaPublica} e devolve URL pública e caminhos físicos.
        /// - currentImagemUrl: URL já salva no banco (ex: "/Carrossel/abc.jpg")
        /// - pastaPublica: "Carrossel" ou "Sobre"
        /// </summary>
        public static async Task<(bool ok, string? erro, string? imagemUrlNova, string? caminhoFisicoNovo, string? caminhoFisicoAntigo)>
            SalvarOuSubstituirAsync(
                IFormFile arquivoImagem,
                string webRootPath,
                string pastaPublica,
                string tituloBase,
                int targetW,
                int targetH,
                string? currentImagemUrl = null,
                long maxSizeBytes = DefaultMaxSizeBytes,
                string[]? allowedExt = null
            )
        {
            try
            {
                if (arquivoImagem == null || arquivoImagem.Length == 0)
                    return (false, "Selecione uma imagem (PNG ou JPG).", null, null, null);

                if (arquivoImagem.Length > maxSizeBytes)
                    return (false, $"A imagem é muito grande. Máximo de {maxSizeBytes / 1024 / 1024} MB.", null, null, null);

                allowedExt ??= DefaultAllowedExt;

                var ext = Path.GetExtension(arquivoImagem.FileName).ToLowerInvariant();
                if (!allowedExt.Contains(ext))
                    return (false, "Formato inválido. Use PNG ou JPG.", null, null, null);

                // garante pasta
                var pastaFisica = EnsureFolder(webRootPath, pastaPublica);

                // nome único
                var safeTitle = SanitizeFileStem(tituloBase);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var guid = Guid.NewGuid().ToString("N");
                var nomeArquivo = $"{safeTitle}_{guid}_{timestamp}{ext}";

                var caminhoFisicoNovo = Path.Combine(pastaFisica, nomeArquivo);

                // salva redimensionada
                await SaveResizedImageAsync(arquivoImagem, caminhoFisicoNovo, ext, targetW, targetH);

                // url pública
                var imagemUrlNova = $"/{pastaPublica}/{nomeArquivo}";

                // caminho físico antigo (baseado na URL atual)
                string? caminhoFisicoAntigo = null;
                if (!string.IsNullOrWhiteSpace(currentImagemUrl))
                {
                    var oldFileName = Path.GetFileName(currentImagemUrl);
                    if (!string.IsNullOrWhiteSpace(oldFileName))
                        caminhoFisicoAntigo = Path.Combine(pastaFisica, oldFileName);
                }

                return (true, null, imagemUrlNova, caminhoFisicoNovo, caminhoFisicoAntigo);
            }
            catch
            {
                return (false, "Não foi possível processar a imagem.", null, null, null);
            }
        }

        public static void TryDeleteFile(string? path)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
                // silencioso
            }
        }

        private static string SanitizeFileStem(string input)
        {
            var s = string.Concat((input ?? "")
                .Trim()
                .Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_'));

            return string.IsNullOrWhiteSpace(s) ? "img" : s;
        }

        private static async Task SaveResizedImageAsync(IFormFile file, string outputPath, string ext, int targetW, int targetH)
        {
            await using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync(stream);

            image.Mutate(x =>
                x.Resize(new ResizeOptions
                {
                    Size = new Size(targetW, targetH),
                    Mode = ResizeMode.Crop,
                    Position = AnchorPositionMode.Center
                })
            );

            if (ext == ".png")
            {
                await image.SaveAsync(outputPath, new PngEncoder());
            }
            else
            {
                await image.SaveAsync(outputPath, new JpegEncoder { Quality = 90 });
            }
        }
    }
}