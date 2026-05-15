using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace formulario_olv.Services
{
    /// <summary>
    /// Cliente HTTP para consumir la API de OLV
    /// </summary>
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger; 
            _httpContextAccessor = httpContextAccessor;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var token = _httpContextAccessor.HttpContext?.User?.FindFirst("jwt_token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // ==========================================
        // AUTENTICACIÓN
        // ==========================================

        //public async Task<LoginApiResponse?> LoginAsync(string usuario, string password)
        //{
        //    try
        //    {
        //        var request = new LoginApiRequest { Usuario = usuario, Password = password };
        //        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, _jsonOptions);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var jsonStr = await response.Content.ReadAsStringAsync();
        //            _logger.LogInformation("ApiClient.LoginAsync: Raw response = {Response}", jsonStr);

        //            var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<LoginApiResponse>>(jsonStr, _jsonOptions);
        //            var data = result?.Data;

        //            _logger.LogInformation("ApiClient.LoginAsync: Deserialized data = RequiereOtp:{RequiereOtp}, OtpSessionId:{OtpSessionId}",
        //                data?.RequiereOtp, data?.OtpSessionId);

        //            return data;
        //        }

        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al autenticar usuario");
        //        return null;
        //    }
        //}

        public async Task<LoginApiResponse?> LoginAsync(string usuario, string password)
        {
            try
            {
                var request = new LoginApiRequest { Usuario = usuario, Password = password };
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, _jsonOptions);

                // 1. Logueamos el estado HTTP exacto para saber qué responde la API
                _logger.LogInformation("ApiClient.LoginAsync: HTTP Status = {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var jsonStr = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("ApiClient.LoginAsync: Raw response = '{Response}'", jsonStr);

                    // 2. PREVENCIÓN DEL CRASH: Si la cadena está vacía, no intentamos deserializar
                    if (string.IsNullOrWhiteSpace(jsonStr))
                    {
                        _logger.LogWarning("ApiClient.LoginAsync: La API devolvió éxito ({Status}) pero el cuerpo está vacío.", response.StatusCode);
                        return null;
                    }

                    var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<LoginApiResponse>>(jsonStr, _jsonOptions);
                    return result?.Data;
                }
                else
                {
                    var errorStr = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("ApiClient.LoginAsync: Fallo HTTP {Status}. Detalle: {Error}", response.StatusCode, errorStr);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al autenticar usuario");
                return null;
            }
        }

        public async Task<VerificarOtpResponse?> VerifyOtpAsync(string otpSessionId, string codigo)
        {
            try
            {
                var request = new VerificarOtpRequest { OtpSessionId = otpSessionId, Codigo = codigo };
                var response = await _httpClient.PostAsJsonAsync("api/auth/verify-otp", request, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<VerificarOtpResponse>>(_jsonOptions);
                    return result?.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar código OTP");
                return null;
            }
        }

        public async Task<bool> ReenviarCodigoOtpAsync(string sessionId)
        {
            try
            {
                var request = new ReenviarOtpRequest { SessionId = sessionId };
                var response = await _httpClient.PostAsJsonAsync("api/auth/resend-otp", request, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(_jsonOptions);
                    return result?.Success ?? false;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reenviar código OTP");
                return false;
            }
        }

        // ==========================================
        // CATÁLOGOS
        // ==========================================

        public async Task<CatalogosResponse?> ObtenerCatalogosAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<CatalogosResponse>>(
                    "api/catalogos", _jsonOptions);
                return response?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener catálogos");
                return null;
            }
        }

        // ==========================================
        // ENTREVISTAS
        // ==========================================

        public async Task<EntrevistaResponse?> ObtenerEntrevistaAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<EntrevistaResponse>>(
                    $"api/entrevistas/{id}", _jsonOptions);
                return response?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener entrevista {Id}", id);
                return null;
            }
        }

        public async Task<List<EntrevistaResumenResponse>> ObtenerEntrevistasPorLegajoAsync(int idLegajo, short tipoLegajoId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<EntrevistaResumenResponse>>>(
                    $"api/entrevistas/legajo/{idLegajo}/tipo/{tipoLegajoId}", _jsonOptions);
                return response?.Data ?? new List<EntrevistaResumenResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener entrevistas del legajo {Legajo}", idLegajo);
                return new List<EntrevistaResumenResponse>();
            }
        }

        public async Task<int> ObtenerSiguienteNumeroEntrevistaAsync(int idLegajo, short tipoLegajoId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<int>>(
                    $"api/entrevistas/legajo/{idLegajo}/tipo/{tipoLegajoId}/siguiente-numero", _jsonOptions);
                return response?.Data ?? 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener siguiente número de entrevista");
                return 1;
            }
        }

        public async Task<EntrevistaResponse?> CrearEntrevistaAsync(CrearEntrevistaRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/entrevistas", request, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<EntrevistaResponse>>(_jsonOptions);
                    return result?.Data;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al crear entrevista: {Error}", error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear entrevista");
                return null;
            }
        }

        public async Task<EntrevistaResponse?> ActualizarEntrevistaAsync(int id, ActualizarEntrevistaRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/entrevistas/{id}", request, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<EntrevistaResponse>>(_jsonOptions);
                    return result?.Data;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error al actualizar entrevista: {Error}", error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar entrevista {Id}", id);
                return null;
            }
        }

        public async Task<bool> EliminarEntrevistaAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/entrevistas/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar entrevista {Id}", id);
                return false;
            }
        }

        // ==========================================
        // PERSONAS (Búsqueda)
        // ==========================================

        public async Task<List<PersonaBusquedaResponse>> BuscarPersonasAsync(string termino, int limite = 10)
        {
            try
            {
                var url = $"api/personas/buscar?termino={Uri.EscapeDataString(termino)}&limite={limite}";
                _logger.LogInformation("ApiClient: Llamando a {Url}", url);

                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<PersonaBusquedaResponse>>>(url, _jsonOptions);

                _logger.LogInformation("ApiClient: Respuesta recibida, Success={Success}, Data count={Count}",
                    response?.Success, response?.Data?.Count ?? 0);

                return response?.Data ?? new List<PersonaBusquedaResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApiClient: Error al buscar personas con término: {Termino}", termino);
                return new List<PersonaBusquedaResponse>();
            }
        }

        public async Task<byte[]?> ObtenerFotoPersonaAsync(int idLegajo)
        {
            try
            {
                var url = $"api/personas/{idLegajo}/foto";
                _logger.LogInformation("ApiClient: Solicitando foto en {Url}", url);
                var response = await _httpClient.GetAsync(url);
                _logger.LogInformation("ApiClient: Respuesta foto legajo {IdLegajo}: StatusCode={StatusCode}", idLegajo, response.StatusCode);
                if (response.IsSuccessStatusCode)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    _logger.LogInformation("ApiClient: Foto recibida, tamaño: {Size} bytes", bytes.Length);
                    return bytes;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener foto de persona con legajo: {IdLegajo}", idLegajo);
                return null;
            }
        }

        // ==========================================
        // ASIGNACIONES
        // ==========================================

        public async Task<List<AsignacionPendienteResponse>> ObtenerAsignacionesPendientesAsync(string usuario)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<AsignacionPendienteResponse>>>(
                    $"api/asignaciones/usuario/{Uri.EscapeDataString(usuario)}", _jsonOptions);
                return response?.Data ?? new List<AsignacionPendienteResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener asignaciones pendientes para usuario: {Usuario}", usuario);
                return new List<AsignacionPendienteResponse>();
            }
        }

        public async Task<PersonaBusquedaResponse?> ObtenerPersonaPorLegajoAsync(int idLegajo, short tipoLegajoId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<PersonaBusquedaResponse>>(
                    $"api/personas/{idLegajo}/tipo/{tipoLegajoId}", _jsonOptions);
                return response?.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener persona con legajo: {IdLegajo}", idLegajo);
                return null;
            }
        }
    }

    // ==========================================
    // DTOs (para deserializar respuestas de la API)
    // ==========================================

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class CatalogoDto
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CatalogosResponse
    {
        public List<CatalogoDto> MotivosNoVerificado { get; set; } = new();
        public List<CatalogoDto> MedidasJudiciales { get; set; } = new();
        public List<CatalogoDto> Tratamientos { get; set; } = new();
        public List<CatalogoDto> TiposMonitoreo { get; set; } = new();
        public List<CatalogoDto> EstadosTutor { get; set; } = new();
        public List<CatalogoDto> GruposConviviente { get; set; } = new();
        public List<CatalogoDto> RelacionesConviviente { get; set; } = new();
        public List<CatalogoDto> FamiliaresCargo { get; set; } = new();
        public List<CatalogoDto> SituacionesLaborales { get; set; } = new();
        public List<CatalogoDto> RubrosLaborales { get; set; } = new();
        public List<CatalogoDto> Sustancias { get; set; } = new();
        public List<CatalogoDto> RiesgosEvidentes { get; set; } = new();
        public List<CatalogoDto> CierresEntrevista { get; set; } = new();
    }

    public class CrearEntrevistaRequest
    {
        public int IdLegajo { get; set; }
        public short TipoLegajoId { get; set; }
        public string? CoordenadasGPS { get; set; }
        public bool? DomicilioVerificado { get; set; }
        public string? ObservacionesDomicilio { get; set; }
        public int? IdMotivoNoVerificado { get; set; }
        public string? MotivoNoVerificadoOtro { get; set; }
        public string? NombrePersonaAtendio { get; set; }
        public bool? CumplePresentaciones { get; set; }
        public List<int> MedidasJudicialesIncumplidas { get; set; } = new();
        public int? IdTratamientoObligatorio { get; set; }
        public bool? AsisteTratamiento { get; set; }
        public bool? ProgramaReeducacion { get; set; }
        public string? CualProgramaReeducacion { get; set; }
        public bool? AsistePrograma { get; set; }
        public bool? RestriccionContactoVictimas { get; set; }
        public bool? IncumplimientoDetectado { get; set; }
        public string? IncumplimientoEspecificar { get; set; }
        public List<int> TiposMonitoreo { get; set; } = new();
        public string? TipoMonitoreoOtro { get; set; }
        public int? IdTutorDomiciliario { get; set; }
        public int? IdGrupoConviviente { get; set; }
        public int? AdultosEnHogar { get; set; }
        public int? NinosEnHogar { get; set; }
        public int? IdRelacionConvivientes { get; set; }
        public string? ObservacionesRelacion { get; set; }
        public int? IdFamiliaresACargo { get; set; }
        public int? IdSituacionLaboral { get; set; }
        public int? IdRubro { get; set; }
        public string? DomicilioLaboral { get; set; }
        public int? TiempoTrabajando { get; set; }
        public bool? ActividadEducativa { get; set; }
        public string? CualActividadEducativa { get; set; }
        public bool? ConsumoSustancias { get; set; }
        public List<int> SustanciasConsumidas { get; set; } = new();
        public string? OtraSustancia { get; set; }
        public bool? TratamientoConsumo { get; set; }
        public bool? TieneObraSocial { get; set; }
        public string? ObservacionObraSocial { get; set; }
        public bool? TratamientoSalud { get; set; }
        public string? CualTratamientoSalud { get; set; }
        public bool? TieneEnfermedad { get; set; }
        public string? CualEnfermedad { get; set; }
        public List<int> RiesgosEvidentes { get; set; } = new();
        public string? OtroRiesgo { get; set; }
        public string? ObservacionesGenerales { get; set; }
        public int? IdCierreEntrevista { get; set; }
        public string? CierreOtro { get; set; }
        public string UsuarioCreacion { get; set; } = string.Empty;
    }

    public class ActualizarEntrevistaRequest : CrearEntrevistaRequest
    {
        public string? UsuarioModificacion { get; set; }
    }

    public class EntrevistaResponse
    {
        public int Id { get; set; }
        public int IdLegajo { get; set; }
        public short TipoLegajoId { get; set; }
        public int NumeroEntrevista { get; set; }
        public DateTime FechaHora { get; set; }
        public string? CoordenadasGPS { get; set; }
        public bool? DomicilioVerificado { get; set; }
        public string? ObservacionesDomicilio { get; set; }
        public CatalogoDto? MotivoNoVerificado { get; set; }
        public string? MotivoNoVerificadoOtro { get; set; }
        public string? NombrePersonaAtendio { get; set; }
        public bool? CumplePresentaciones { get; set; }
        public List<CatalogoDto> MedidasJudicialesIncumplidas { get; set; } = new();
        public CatalogoDto? TratamientoObligatorio { get; set; }
        public bool? AsisteTratamiento { get; set; }
        public bool? ProgramaReeducacion { get; set; }
        public string? CualProgramaReeducacion { get; set; }
        public bool? AsistePrograma { get; set; }
        public bool? RestriccionContactoVictimas { get; set; }
        public bool? IncumplimientoDetectado { get; set; }
        public string? IncumplimientoEspecificar { get; set; }
        public List<CatalogoDto> TiposMonitoreo { get; set; } = new();
        public string? TipoMonitoreoOtro { get; set; }
        public CatalogoDto? TutorDomiciliario { get; set; }
        public CatalogoDto? GrupoConviviente { get; set; }
        public int? AdultosEnHogar { get; set; }
        public int? NinosEnHogar { get; set; }
        public CatalogoDto? RelacionConvivientes { get; set; }
        public string? ObservacionesRelacion { get; set; }
        public CatalogoDto? FamiliaresACargo { get; set; }
        public CatalogoDto? SituacionLaboral { get; set; }
        public CatalogoDto? Rubro { get; set; }
        public string? DomicilioLaboral { get; set; }
        public int? TiempoTrabajando { get; set; }
        public bool? ActividadEducativa { get; set; }
        public string? CualActividadEducativa { get; set; }
        public bool? ConsumoSustancias { get; set; }
        public List<CatalogoDto> SustanciasConsumidas { get; set; } = new();
        public string? OtraSustancia { get; set; }
        public bool? TratamientoConsumo { get; set; }
        public bool? TieneObraSocial { get; set; }
        public string? ObservacionObraSocial { get; set; }
        public bool? TratamientoSalud { get; set; }
        public string? CualTratamientoSalud { get; set; }
        public bool? TieneEnfermedad { get; set; }
        public string? CualEnfermedad { get; set; }
        public List<CatalogoDto> RiesgosEvidentes { get; set; } = new();
        public string? OtroRiesgo { get; set; }
        public string? ObservacionesGenerales { get; set; }
        public CatalogoDto? CierreEntrevista { get; set; }
        public string? CierreOtro { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string UsuarioCreacion { get; set; } = string.Empty;
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }
    }

    public class EntrevistaResumenResponse
    {
        public int Id { get; set; }
        public int IdLegajo { get; set; }
        public int NumeroEntrevista { get; set; }
        public DateTime FechaHora { get; set; }
        public bool? DomicilioVerificado { get; set; }
        public string? Cierre { get; set; }
        public string UsuarioCreacion { get; set; } = string.Empty;
    }

    public class PersonaBusquedaResponse
    {
        public int IdLegajo { get; set; }
        public short TipoLegajoId { get; set; }
        public string NumeroLegajo { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Dni { get; set; }
        public string? Domicilio { get; set; }
        public string? Delito { get; set; }
        public string? PeriodicidadEntrevistas { get; set; }
        public string? ModalidadTutela { get; set; }
        public string? NivelRiesgo { get; set; }
    }

    public class AsignacionPendienteResponse
    {
        public int Id { get; set; }
        public int IdLegajo { get; set; }
        public short TipoLegajoId { get; set; }
        public string? NombreCompleto { get; set; }
        public string? NroDocumento { get; set; }
        public string? Domicilio { get; set; }
        public string? NivelRiesgo { get; set; }
        public string Prioridad { get; set; } = string.Empty;
        public int IdPrioridad { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public DateTime? FechaUltimaEntrevista { get; set; }
        public string? Observaciones { get; set; }
    }

    public class LoginApiRequest
    {
        public string Usuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginApiResponse
    {
        public string Usuario { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;

        [JsonPropertyName("requiereOtp")]
        public bool RequiereOtp { get; set; }

        [JsonPropertyName("otpSessionId")]
        public string OtpSessionId { get; set; } = string.Empty;
    }

    public class VerificarOtpRequest
    {
        [JsonPropertyName("otpSessionId")]
        public string OtpSessionId { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
    }

    public class VerificarOtpResponse
    {
        public string Usuario { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class ReenviarOtpRequest
    {
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; } = string.Empty;
    }
}
