namespace formulario_olv.Models
{
    public class OtpViewModel
    {
        public string OtpSessionId { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string? MensajeError { get; set; }
        public string? MensajeExito { get; set; }
        public int IntentosFallidos { get; set; } = 0;
        public DateTime? FechaUltimoReenvio { get; set; }
        public bool PuedeReenviar { get; set; } = true;
        public int SegundosHastaProximoReenvio { get; set; } = 0;
    }
}
