using System.ComponentModel.DataAnnotations;

namespace formulario_olv.Models
{
    public class EntrevistaViewModel
    {
        // ==========================================
        // IDENTIFICADORES DEL LEGAJO
        // ==========================================
        public int IdLegajo { get; set; }
        public short TipoLegajoId { get; set; }

        // ==========================================
        // FICHA DE IDENTIFICACIÓN (Solo lectura)
        // ==========================================
        public string NombreApellido { get; set; } = string.Empty;
        public string DNI { get; set; } = string.Empty;
        public string Legajo { get; set; } = string.Empty;
        public string Delito { get; set; } = string.Empty;
        public string NivelRiesgo { get; set; } = string.Empty;
        public string ModalidadTutela { get; set; } = string.Empty;
        public string PeriodicidadEntrevistas { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;

        // ==========================================
        // FORMULARIO DE ENTREVISTA
        // ==========================================

        // --- Datos de Entrevista (1-3) ---

        // 1. Número de entrevista (auto/readonly)
        [Display(Name = "Número de entrevista")]
        public int NumeroEntrevista { get; set; }

        // 2. Fecha y hora (auto)
        [Display(Name = "Fecha y hora")]
        public DateTime FechaHora { get; set; } = DateTime.Now;

        // 3. Coordenadas GPS
        [Display(Name = "Coordenadas GPS")]
        public string? CoordenadasGPS { get; set; }

        // --- Verificación Domicilio (4-5) ---

        // 4. Domicilio verificado
        [Display(Name = "Domicilio")]
        public string? DomicilioVerificado { get; set; }

        // 4a. Observaciones domicilio (si verificado)
        [Display(Name = "Observaciones del domicilio")]
        public string? ObservacionesDomicilio { get; set; }

        // 4b. Motivo no verificado
        [Display(Name = "Motivo no verificado")]
        public string? MotivoNoVerificado { get; set; }

        // 4b. Especificar otro motivo
        [Display(Name = "Especificar otro motivo")]
        public string? MotivoNoVerificadoOtro { get; set; }

        // 5. Nombre persona que atendió
        [Display(Name = "Nombre de persona que atendió")]
        public string? NombrePersonaAtendio { get; set; }

        // --- Situación Judicial (6-10) ---

        // 6. Cumple presentaciones
        [Display(Name = "Cumple presentaciones")]
        public string? CumplePresentaciones { get; set; }

        // 6a. Cuáles incumple (multiselect)
        [Display(Name = "¿Cuáles incumple?")]
        public List<string> PresentacionesIncumplidas { get; set; } = new List<string>();

        // 7. Tratamiento obligatorio
        [Display(Name = "Tratamiento obligatorio")]
        public string? TratamientoObligatorio { get; set; }

        // 7a. Asiste al tratamiento
        [Display(Name = "¿Asiste al tratamiento?")]
        public string? AsisteTratamiento { get; set; }

        // 8. Programa de reeducación
        [Display(Name = "Programa de reeducación")]
        public string? ProgramaReeducacion { get; set; }

        // 8a. ¿Cuál programa?
        [Display(Name = "¿Cuál programa?")]
        public string? CualProgramaReeducacion { get; set; }

        // 8b. Asiste al programa
        [Display(Name = "¿Asiste al programa?")]
        public string? AsistePrograma { get; set; }

        // 9. Restricción contacto víctimas
        [Display(Name = "Restricción de contacto con víctimas")]
        public string? RestriccionContactoVictimas { get; set; }

        // 9b. Incumplimiento detectado
        [Display(Name = "Incumplimiento detectado")]
        public string? IncumplimientoDetectado { get; set; }

        // 9b. Especificar incumplimiento
        [Display(Name = "Especificar incumplimiento")]
        public string? IncumplimientoEspecificar { get; set; }

        // 10. Tipo de monitoreo (multiselect)
        [Display(Name = "Tipo de monitoreo")]
        public List<string> TipoMonitoreo { get; set; } = new List<string>();

        // 10. Otro tipo de monitoreo
        [Display(Name = "Otro tipo de monitoreo")]
        public string? TipoMonitoreoOtro { get; set; }

        // --- Situación Habitacional (11-13) ---

        // 11. Tutor domiciliario
        [Display(Name = "Tutor domiciliario")]
        public string? TutorDomiciliario { get; set; }

        // 12. Grupo conviviente
        [Display(Name = "Grupo conviviente")]
        public string? GrupoConviviente { get; set; }

        // 12a. Adultos en hogar
        [Display(Name = "Adultos en el hogar")]
        public int? AdultosEnHogar { get; set; }

        // 12a. Niños en hogar
        [Display(Name = "Niños en el hogar")]
        public int? NinosEnHogar { get; set; }

        // 12b. Relación convivientes
        [Display(Name = "Relación con convivientes")]
        public string? RelacionConvivientes { get; set; }

        // 12c. Observaciones relación (si conflictiva)
        [Display(Name = "Observaciones sobre la relación")]
        public string? ObservacionesRelacion { get; set; }

        // 13. Familiares a cargo
        [Display(Name = "Familiares a cargo")]
        public string? FamiliaresACargo { get; set; }

        // --- Situación Laboral (14-16) ---

        // 14. Situación laboral
        [Display(Name = "Situación laboral")]
        public string? SituacionLaboral { get; set; }

        // 15a. Rubro
        [Display(Name = "Rubro")]
        public string? Rubro { get; set; }

        // 15b. Domicilio laboral
        [Display(Name = "Domicilio laboral")]
        public string? DomicilioLaboral { get; set; }

        // 15c. Tiempo trabajando (meses)
        [Display(Name = "Tiempo trabajando (meses)")]
        public int? TiempoTrabajando { get; set; }

        // 16. Actividad educativa
        [Display(Name = "Actividad educativa")]
        public string? ActividadEducativa { get; set; }

        // 16. ¿Cuál actividad educativa?
        [Display(Name = "¿Cuál actividad educativa?")]
        public string? CualActividadEducativa { get; set; }

        // --- Salud (17-21) ---

        // 17. Consumo de sustancias
        [Display(Name = "Consumo de sustancias")]
        public string? ConsumoSustancias { get; set; }

        // 18a. ¿Cuáles sustancias? (multiselect)
        [Display(Name = "¿Cuáles sustancias?")]
        public List<string> CualesSustancias { get; set; } = new List<string>();

        // 18a. Otra sustancia
        [Display(Name = "Otra sustancia")]
        public string? OtraSustancia { get; set; }

        // 18b. Tratamiento consumo
        [Display(Name = "Tratamiento por consumo")]
        public string? TratamientoConsumo { get; set; }

        // 19. Tiene Obra Social
        [Display(Name = "Tiene Obra Social")]
        public string? TieneObraSocial { get; set; }

        [Display(Name = "Observaciones Obra Social")]
        public string? ObservacionObraSocial { get; set; }

        // 20. Tratamiento de salud
        [Display(Name = "Tratamiento de salud")]
        public string? TratamientoSalud { get; set; }

        // 20. ¿Cuál tratamiento de salud?
        [Display(Name = "¿Cuál tratamiento?")]
        public string? CualTratamientoSalud { get; set; }

        // 20a. Tiene enfermedad
        [Display(Name = "Tiene enfermedad")]
        public string? TieneEnfermedad { get; set; }

        // 20a. ¿Cuál enfermedad?
        [Display(Name = "¿Cuál enfermedad?")]
        public string? CualEnfermedad { get; set; }

        // 21. Riesgo evidente (multiselect)
        [Display(Name = "Riesgo evidente")]
        public List<string> RiesgoEvidente { get; set; } = new List<string>();

        // 21. Otro riesgo
        [Display(Name = "Otro riesgo")]
        public string? OtroRiesgo { get; set; }

        // --- Cierre (22-23) ---

        // 22. Observaciones generales
        [Display(Name = "Observaciones generales")]
        public string? ObservacionesGenerales { get; set; }

        // 23. Cierre
        [Display(Name = "Cierre")]
        public string? Cierre { get; set; }

        [Display(Name = "Especificar otro cierre")]
        public string? CierreOtro { get; set; }
    }
}
