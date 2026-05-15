namespace formulario_olv.Models
{
    /// <summary>
    /// Legajo asignado para entrevista (página de búsqueda)
    /// </summary>
    public class LegajoAsignadoViewModel
    {
        public int IdLegajo { get; set; }
        public short TipoLegajoId { get; set; }
        public string NumeroLegajo { get; set; } = string.Empty;
        public string NombreApellido { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string NivelRiesgo { get; set; } = string.Empty;
        public string Prioridad { get; set; } = string.Empty;
        public DateTime? FechaUltimaEntrevista { get; set; }
    }

    /// <summary>
    /// Página de búsqueda
    /// </summary>
    public class BusquedaViewModel
    {
        public string? FiltroBusqueda { get; set; }
        public List<LegajoAsignadoViewModel> LegajosAsignados { get; set; } = new();
    }

    /// <summary>
    /// Resumen de entrevista para el historial
    /// </summary>
    public class EntrevistaHistorialViewModel
    {
        public int Id { get; set; }
        public int NumeroEntrevista { get; set; }
        public DateTime FechaHora { get; set; }
        public string? Domicilio { get; set; }
        public bool? DomicilioVerificado { get; set; }
        public string? Cierre { get; set; }
        public string UsuarioCreacion { get; set; } = string.Empty;
    }

    /// <summary>
    /// Página de historial del legajo
    /// </summary>
    public class HistorialLegajoViewModel
    {
        // Ficha de identificación
        public int IdLegajo { get; set; }
        public short TipoLegajoId { get; set; }
        public string NumeroLegajo { get; set; } = string.Empty;
        public string NombreApellido { get; set; } = string.Empty;
        public string DNI { get; set; } = string.Empty;
        public string Delito { get; set; } = string.Empty;
        public string NivelRiesgo { get; set; } = string.Empty;
        public string ModalidadTutela { get; set; } = string.Empty;
        public string PeriodicidadEntrevistas { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;

        // Historial de entrevistas
        public List<EntrevistaHistorialViewModel> Entrevistas { get; set; } = new();
    }
}
