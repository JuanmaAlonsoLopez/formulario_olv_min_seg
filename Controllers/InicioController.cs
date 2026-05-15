using formulario_olv.Models;
using formulario_olv.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace formulario_olv.Controllers
{
    [Authorize]
    public class InicioController : Controller
    {
        private readonly ILogger<InicioController> _logger;
        private readonly ApiClient _apiClient;

        public InicioController(ILogger<InicioController> logger, ApiClient apiClient)
        {
            _logger = logger;
            _apiClient = apiClient;
        }

        // GET: /Inicio o /
        public async Task<IActionResult> Index(string? busqueda)
        {
            var model = new BusquedaViewModel
            {
                FiltroBusqueda = busqueda,
                LegajosAsignados = new List<LegajoAsignadoViewModel>()
            };

            // Cargar asignaciones pendientes del usuario logueado
            try
            {
                var usuario = User.Identity?.Name;
                if (!string.IsNullOrEmpty(usuario))
                {
                    var asignaciones = await _apiClient.ObtenerAsignacionesPendientesAsync(usuario);
                    model.LegajosAsignados = asignaciones.Select(a => new LegajoAsignadoViewModel
                    {
                        IdLegajo = a.IdLegajo,
                        TipoLegajoId = a.TipoLegajoId,
                        NumeroLegajo = $"{a.IdLegajo}",
                        NombreApellido = a.NombreCompleto ?? "",
                        Direccion = a.Domicilio ?? "",
                        NivelRiesgo = a.NivelRiesgo ?? "Sin evaluar",
                        Prioridad = a.Prioridad,
                        FechaUltimaEntrevista = a.FechaUltimaEntrevista
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar asignaciones pendientes");
            }

            return View(model);
        }

        // GET: /Inicio/Foto/{idLegajo}
        [Route("Inicio/Foto/{idLegajo:int}")]
        [ResponseCache(Duration = 300)]
        public async Task<IActionResult> Foto(int idLegajo)
        {
            _logger.LogInformation("Frontend: Solicitando foto para legajo: {IdLegajo}", idLegajo);
            var fotoBytes = await _apiClient.ObtenerFotoPersonaAsync(idLegajo);
            if (fotoBytes == null || fotoBytes.Length == 0)
            {
                _logger.LogWarning("Frontend: No se obtuvo foto para legajo: {IdLegajo}", idLegajo);
                return NotFound();
            }
            _logger.LogInformation("Frontend: Foto obtenida para legajo {IdLegajo}, tamaño: {Size} bytes", idLegajo, fotoBytes.Length);
            return File(fotoBytes, "image/jpeg");
        }

        // GET: /Inicio/BuscarPersonas?termino=xxx
        [HttpGet]
        public async Task<IActionResult> BuscarPersonas(string termino)
        {
            _logger.LogInformation("Frontend: Recibida búsqueda con término: '{Termino}'", termino);

            if (string.IsNullOrWhiteSpace(termino) || termino.Length < 2)
            {
                _logger.LogInformation("Frontend: Término muy corto, devolviendo lista vacía");
                return Json(new List<object>());
            }

            try
            {
                _logger.LogInformation("Frontend: Llamando a API para buscar: '{Termino}'", termino);
                var personas = await _apiClient.BuscarPersonasAsync(termino, 10);
                _logger.LogInformation("Frontend: API devolvió {Count} resultados", personas.Count);

                var resultados = personas.Select(p => new
                {
                    idLegajo = p.IdLegajo,
                    tipoLegajoId = p.TipoLegajoId,
                    numeroLegajo = p.NumeroLegajo,
                    nombreCompleto = p.NombreCompleto,
                    dni = p.Dni,
                    domicilio = p.Domicilio,
                    texto = $"{p.NombreCompleto} - {p.NumeroLegajo}"
                }).ToList();

                return Json(resultados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Frontend: Error al buscar personas");
                return Json(new List<object>());
            }
        }

        // GET: /Inicio/Busqueda/{idLegajo}/{tipoLegajoId}
        [Route("Inicio/Busqueda/{idLegajo:int}/{tipoLegajoId:int}")]
        public async Task<IActionResult> Busqueda(int idLegajo, short tipoLegajoId)
        {
            // Obtener datos de la persona desde la API
            var persona = await _apiClient.ObtenerPersonaPorLegajoAsync(idLegajo, tipoLegajoId);

            if (persona == null)
            {
                return NotFound();
            }

            var legajo = new HistorialLegajoViewModel
            {
                IdLegajo = persona.IdLegajo,
                TipoLegajoId = persona.TipoLegajoId,
                NumeroLegajo = persona.NumeroLegajo,
                NombreApellido = persona.NombreCompleto,
                DNI = persona.Dni ?? "",
                Direccion = persona.Domicilio ?? "",
                Delito = persona.Delito ?? "",
                PeriodicidadEntrevistas = persona.PeriodicidadEntrevistas ?? "",
                NivelRiesgo = persona.NivelRiesgo ?? "Sin evaluar",
                ModalidadTutela = persona.ModalidadTutela ?? ""
            };

            // Obtener historial de entrevistas desde la API
            var entrevistasApi = await _apiClient.ObtenerEntrevistasPorLegajoAsync(idLegajo, tipoLegajoId);

            legajo.Entrevistas = entrevistasApi.Select(e => new EntrevistaHistorialViewModel
            {
                Id = e.Id,
                NumeroEntrevista = e.NumeroEntrevista,
                FechaHora = e.FechaHora,
                DomicilioVerificado = e.DomicilioVerificado,
                Domicilio = persona.Domicilio ?? "",
                Cierre = e.Cierre,
                UsuarioCreacion = e.UsuarioCreacion
            }).ToList();

            return View(legajo);
        }

        // GET: /Inicio/VerEntrevista/{id}
        [Route("Inicio/VerEntrevista/{id:int}")]
        public async Task<IActionResult> VerEntrevista(int id)
        {
            var entrevista = await _apiClient.ObtenerEntrevistaAsync(id);

            if (entrevista == null)
            {
                return NotFound();
            }

            // Obtener datos de la persona para la ficha
            var persona = await _apiClient.ObtenerPersonaPorLegajoAsync(entrevista.IdLegajo, entrevista.TipoLegajoId);

            // Mapear a EntrevistaViewModel para reutilizar la vista
            var model = MapearEntrevistaAViewModel(entrevista, persona);

            // Obtener catálogos para mostrar los valores
            var catalogos = await _apiClient.ObtenerCatalogosAsync();
            ViewBag.Catalogos = catalogos;
            ViewBag.SoloLectura = true;

            return View("~/Views/Entrevista/Index.cshtml", model);
        }

        private EntrevistaViewModel MapearEntrevistaAViewModel(EntrevistaResponse entrevista, PersonaBusquedaResponse? persona)
        {
            return new EntrevistaViewModel
            {
                // Identificadores del legajo
                IdLegajo = entrevista.IdLegajo,
                TipoLegajoId = entrevista.TipoLegajoId,

                // Ficha de identificación
                NombreApellido = persona?.NombreCompleto ?? "",
                DNI = persona?.Dni ?? "",
                Legajo = persona?.NumeroLegajo ?? "",
                Delito = persona?.Delito ?? "",
                NivelRiesgo = persona?.NivelRiesgo ?? "Sin evaluar",
                ModalidadTutela = persona?.ModalidadTutela ?? "",
                PeriodicidadEntrevistas = persona?.PeriodicidadEntrevistas ?? "",
                Direccion = persona?.Domicilio ?? "",

                // Datos de entrevista
                NumeroEntrevista = entrevista.NumeroEntrevista,
                FechaHora = entrevista.FechaHora,
                CoordenadasGPS = entrevista.CoordenadasGPS,
                DomicilioVerificado = entrevista.DomicilioVerificado == true ? "Verificado" : "NoVerificado",
                ObservacionesDomicilio = entrevista.ObservacionesDomicilio,
                MotivoNoVerificado = entrevista.MotivoNoVerificado?.Id.ToString(),
                MotivoNoVerificadoOtro = entrevista.MotivoNoVerificadoOtro,
                NombrePersonaAtendio = entrevista.NombrePersonaAtendio,
                CumplePresentaciones = entrevista.CumplePresentaciones == true ? "Si" : "No",
                PresentacionesIncumplidas = entrevista.MedidasJudicialesIncumplidas?.Select(m => m.Id.ToString()).ToList() ?? new List<string>(),
                TratamientoObligatorio = entrevista.TratamientoObligatorio?.Id.ToString(),
                AsisteTratamiento = entrevista.AsisteTratamiento == true ? "Si" : "No",
                ProgramaReeducacion = entrevista.ProgramaReeducacion == true ? "Si" : "No",
                CualProgramaReeducacion = entrevista.CualProgramaReeducacion,
                AsistePrograma = entrevista.AsistePrograma == true ? "Si" : "No",
                RestriccionContactoVictimas = entrevista.RestriccionContactoVictimas == true ? "Si" : "No",
                IncumplimientoDetectado = entrevista.IncumplimientoDetectado == true ? "Si" : "No",
                IncumplimientoEspecificar = entrevista.IncumplimientoEspecificar,
                TipoMonitoreo = entrevista.TiposMonitoreo?.Select(m => m.Id.ToString()).ToList() ?? new List<string>(),
                TipoMonitoreoOtro = entrevista.TipoMonitoreoOtro,
                TutorDomiciliario = entrevista.TutorDomiciliario?.Id.ToString(),
                GrupoConviviente = entrevista.GrupoConviviente?.Id.ToString(),
                AdultosEnHogar = entrevista.AdultosEnHogar,
                NinosEnHogar = entrevista.NinosEnHogar,
                RelacionConvivientes = entrevista.RelacionConvivientes?.Id.ToString(),
                ObservacionesRelacion = entrevista.ObservacionesRelacion,
                FamiliaresACargo = entrevista.FamiliaresACargo?.Id.ToString(),
                SituacionLaboral = entrevista.SituacionLaboral?.Id.ToString(),
                Rubro = entrevista.Rubro?.Id.ToString(),
                DomicilioLaboral = entrevista.DomicilioLaboral,
                TiempoTrabajando = entrevista.TiempoTrabajando,
                ActividadEducativa = entrevista.ActividadEducativa == true ? "Si" : "No",
                CualActividadEducativa = entrevista.CualActividadEducativa,
                ConsumoSustancias = entrevista.ConsumoSustancias == true ? "Si" : "No",
                CualesSustancias = entrevista.SustanciasConsumidas?.Select(s => s.Id.ToString()).ToList() ?? new List<string>(),
                OtraSustancia = entrevista.OtraSustancia,
                TratamientoConsumo = entrevista.TratamientoConsumo == true ? "Si" : "No",
                TieneObraSocial = entrevista.TieneObraSocial == true ? "Si" : "No",
                ObservacionObraSocial = entrevista.ObservacionObraSocial,
                TratamientoSalud = entrevista.TratamientoSalud == true ? "Si" : "No",
                CualTratamientoSalud = entrevista.CualTratamientoSalud,
                TieneEnfermedad = entrevista.TieneEnfermedad == true ? "Si" : "No",
                CualEnfermedad = entrevista.CualEnfermedad,
                RiesgoEvidente = entrevista.RiesgosEvidentes?.Select(r => r.Id.ToString()).ToList() ?? new List<string>(),
                OtroRiesgo = entrevista.OtroRiesgo,
                ObservacionesGenerales = entrevista.ObservacionesGenerales,
                Cierre = entrevista.CierreEntrevista?.Id.ToString(),
                CierreOtro = entrevista.CierreOtro
            };
        }
    }
}
