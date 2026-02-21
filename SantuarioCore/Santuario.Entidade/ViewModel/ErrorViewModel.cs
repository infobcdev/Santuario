namespace Santuario.Entidade.ViewModels
{
    public class ErrorViewModel
    {
        public int StatusCode { get; set; }
        public string? Mensagem { get; set; }
        public string? RequestId { get; set; }
        public string? OriginalPath { get; set; }

        public bool ShowRequestId => !string.IsNullOrWhiteSpace(RequestId);
    }
}