using formulario_olv.Models;
using formulario_olv.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace formulario_olv.Controllers
{
    [Authorize]
    public class EntrevistaController : Controller
    {
        private readonly ILogger<EntrevistaController> _logger;
        private readonly ApiClient _apiClient;

        public EntrevistaController(ILogger<EntrevistaController> logger, ApiClient apiClient)
        {
            _logger = logger;
            _apiClient = apiClient;
        }

        // GET: Entrevista
        public async Task<IActionResult> Index(int? idLegajo, short? tipoLegajoId)
        {
            // Validar que se proporcionen los parámetros del legajo
            if (!idLegajo.HasValue || !tipoLegajoId.HasValue)
            {
                return RedirectToAction("Index", "Inicio");
            }

            // Obtener catálogos de la API
            var catalogos = await _apiClient.ObtenerCatalogosAsync();
            ViewBag.Catalogos = catalogos;

            // Obtener el siguiente número de entrevista
            int numeroEntrevista = await _apiClient.ObtenerSiguienteNumeroEntrevistaAsync(
                idLegajo.Value, tipoLegajoId.Value);

            // Obtener datos del legajo desde la API
            var persona = await _apiClient.ObtenerPersonaPorLegajoAsync(idLegajo.Value, tipoLegajoId.Value);

            var model = new EntrevistaViewModel
            {
                // Identificadores del legajo
                IdLegajo = idLegajo.Value,
                TipoLegajoId = tipoLegajoId.Value,

                // Datos de ficha
                NombreApellido = persona?.NombreCompleto ?? "",
                DNI = persona?.Dni ?? "",
                Legajo = persona?.NumeroLegajo ?? idLegajo.Value.ToString(),
                Delito = persona?.Delito ?? "",
                NivelRiesgo = persona?.NivelRiesgo ?? "Sin evaluar",
                ModalidadTutela = persona?.ModalidadTutela ?? "",
                PeriodicidadEntrevistas = persona?.PeriodicidadEntrevistas ?? "",
                Direccion = persona?.Domicilio ?? "",

                // Datos iniciales del formulario
                NumeroEntrevista = numeroEntrevista,
                FechaHora = DateTime.Now
            };

            return View(model);
        }

        // POST: Entrevista
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(EntrevistaViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Convertir ViewModel a Request de la API
                var request = MapearViewModelARequest(model);

                // Enviar a la API
                var resultado = await _apiClient.CrearEntrevistaAsync(request);

                if (resultado != null)
                {
                    _logger.LogInformation("Entrevista creada: ID={Id}, Legajo={Legajo}",
                        resultado.Id, model.Legajo);
                    TempData["Success"] = "Entrevista guardada correctamente.";

                    // Redirigir al historial del legajo
                    return RedirectToAction("Busqueda", "Inicio", new {
                        idLegajo = model.IdLegajo,
                        tipoLegajoId = model.TipoLegajoId
                    });
                }
                else
                {
                    _logger.LogError("Error al crear entrevista para legajo: {Legajo}", model.Legajo);
                    ModelState.AddModelError("", "Error al guardar la entrevista. Intente nuevamente.");
                }
            }

            // Si hay errores, recargar catálogos y mostrar formulario
            var catalogos = await _apiClient.ObtenerCatalogosAsync();
            ViewBag.Catalogos = catalogos;
            return View(model);
        }

        // GET: Entrevista/Ver/{id}
        public async Task<IActionResult> Ver(int id)
        {
            var entrevista = await _apiClient.ObtenerEntrevistaAsync(id);

            if (entrevista == null)
            {
                return NotFound();
            }

            var model = MapearResponseAViewModel(entrevista);
            ViewBag.SoloLectura = true;
            return View("Index", model);
        }

        // GET: Entrevista/Historial/{idLegajo}/{tipoLegajoId}
        public async Task<IActionResult> Historial(int idLegajo, short tipoLegajoId)
        {
            var entrevistas = await _apiClient.ObtenerEntrevistasPorLegajoAsync(idLegajo, tipoLegajoId);
            return View(entrevistas);
        }

        private CrearEntrevistaRequest MapearViewModelARequest(EntrevistaViewModel model)
        {
            return new CrearEntrevistaRequest
            {
                IdLegajo = model.IdLegajo,
                TipoLegajoId = model.TipoLegajoId,
                CoordenadasGPS = model.CoordenadasGPS,
                DomicilioVerificado = model.DomicilioVerificado == "Verificado",
                ObservacionesDomicilio = model.ObservacionesDomicilio,
                IdMotivoNoVerificado = ParseInt(model.MotivoNoVerificado),
                MotivoNoVerificadoOtro = model.MotivoNoVerificadoOtro,
                NombrePersonaAtendio = model.NombrePersonaAtendio,
                CumplePresentaciones = model.CumplePresentaciones == "Si",
                MedidasJudicialesIncumplidas = model.PresentacionesIncumplidas?.Select(ParseInt).Where(x => x.HasValue).Select(x => x!.Value).ToList() ?? new List<int>(),
                IdTratamientoObligatorio = ParseInt(model.TratamientoObligatorio),
                AsisteTratamiento = model.AsisteTratamiento == "Si",
                ProgramaReeducacion = model.ProgramaReeducacion == "Si",
                CualProgramaReeducacion = model.CualProgramaReeducacion,
                AsistePrograma = model.AsistePrograma == "Si",
                RestriccionContactoVictimas = model.RestriccionContactoVictimas == "Si",
                IncumplimientoDetectado = model.IncumplimientoDetectado == "Si",
                IncumplimientoEspecificar = model.IncumplimientoEspecificar,
                TiposMonitoreo = model.TipoMonitoreo?.Select(ParseInt).Where(x => x.HasValue).Select(x => x!.Value).ToList() ?? new List<int>(),
                TipoMonitoreoOtro = model.TipoMonitoreoOtro,
                IdTutorDomiciliario = ParseInt(model.TutorDomiciliario),
                IdGrupoConviviente = ParseInt(model.GrupoConviviente),
                AdultosEnHogar = model.AdultosEnHogar,
                NinosEnHogar = model.NinosEnHogar,
                IdRelacionConvivientes = ParseInt(model.RelacionConvivientes),
                ObservacionesRelacion = model.ObservacionesRelacion,
                IdFamiliaresACargo = ParseInt(model.FamiliaresACargo),
                IdSituacionLaboral = ParseInt(model.SituacionLaboral),
                IdRubro = ParseInt(model.Rubro),
                DomicilioLaboral = model.DomicilioLaboral,
                TiempoTrabajando = model.TiempoTrabajando,
                ActividadEducativa = model.ActividadEducativa == "Si",
                CualActividadEducativa = model.CualActividadEducativa,
                ConsumoSustancias = model.ConsumoSustancias == "Si",
                SustanciasConsumidas = model.CualesSustancias?.Select(ParseInt).Where(x => x.HasValue).Select(x => x!.Value).ToList() ?? new List<int>(),
                OtraSustancia = model.OtraSustancia,
                TratamientoConsumo = model.TratamientoConsumo == "Si",
                TieneObraSocial = model.TieneObraSocial == "Si",
                ObservacionObraSocial = model.ObservacionObraSocial,
                TratamientoSalud = model.TratamientoSalud == "Si",
                CualTratamientoSalud = model.CualTratamientoSalud,
                TieneEnfermedad = model.TieneEnfermedad == "Si",
                CualEnfermedad = model.CualEnfermedad,
                RiesgosEvidentes = model.RiesgoEvidente?.Select(ParseInt).Where(x => x.HasValue).Select(x => x!.Value).ToList() ?? new List<int>(),
                OtroRiesgo = model.OtroRiesgo,
                ObservacionesGenerales = model.ObservacionesGenerales,
                IdCierreEntrevista = ParseInt(model.Cierre),
                CierreOtro = model.CierreOtro,
                UsuarioCreacion = User.Identity?.Name ?? "sistema"
            };
        }

        private EntrevistaViewModel MapearResponseAViewModel(EntrevistaResponse response)
        {
            return new EntrevistaViewModel
            {
                NumeroEntrevista = response.NumeroEntrevista,
                FechaHora = response.FechaHora,
                CoordenadasGPS = response.CoordenadasGPS,
                DomicilioVerificado = response.DomicilioVerificado == true ? "Verificado" : "No verificado",
                ObservacionesDomicilio = response.ObservacionesDomicilio,
                MotivoNoVerificado = response.MotivoNoVerificado?.Id.ToString(),
                MotivoNoVerificadoOtro = response.MotivoNoVerificadoOtro,
                NombrePersonaAtendio = response.NombrePersonaAtendio,
                // ... continuar mapeando los demás campos
            };
        }

        private int? ParseInt(string? value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            if (int.TryParse(value, out var result)) return result;
            return null;
        }

    }
}
