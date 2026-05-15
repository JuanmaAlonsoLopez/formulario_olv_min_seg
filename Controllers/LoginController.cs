using System.Security.Claims;
using formulario_olv.Models;
using formulario_olv.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace formulario_olv.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<LoginController> _logger;

        public LoginController(ApiClient apiClient, ILogger<LoginController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        // GET: /Login
        [HttpGet]
        public IActionResult Index(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return Redirect("~/Inicio/Index");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        // POST: /Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resultado = await _apiClient.LoginAsync(model.Usuario, model.Password);

            if (resultado == null)
            {
                model.ErrorMessage = "Usuario o contraseña incorrectos, o no tiene permisos para acceder.";
                return View(model);
            }

            _logger.LogInformation("LoginController: RequiereOtp={RequiereOtp}, OtpSessionId={OtpSessionId}",
                resultado.RequiereOtp, resultado.OtpSessionId);

            // Si requiere OTP, redirigir a la pantalla de verificación
            if (resultado.RequiereOtp)
            {
                _logger.LogInformation("LoginController: Redirigiendo a Otp con SessionId={SessionId}", resultado.OtpSessionId);
                return RedirectToAction("Otp", new { sessionId = resultado.OtpSessionId });
            }

            // Crear claims y cookie de sesión
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, resultado.Usuario)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            _logger.LogInformation("Usuario {Usuario} inició sesión", resultado.Usuario);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl != "/")
			{
				// Si la URL es válida y no es simplemente la raíz cruda, redirige ahí
				return Redirect(returnUrl);
			}
			// Forzamos la ruta completa
			return Redirect("~/Inicio/Index");
        }

        // GET: /Login/Otp
        [HttpGet]
        public IActionResult Otp(string sessionId)
        {
            _logger.LogInformation("LoginController GET Otp: sessionId={SessionId}", sessionId);

            if (string.IsNullOrEmpty(sessionId))
            {
                _logger.LogWarning("LoginController GET Otp: sessionId vacío, redirigiendo a Index");
                return RedirectToAction("Index");
            }

            return View(new OtpViewModel
            {
                OtpSessionId = sessionId,
                PuedeReenviar = true,
                SegundosHastaProximoReenvio = 0
            });
        }

        // POST: /Login/VerificarOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerificarOtp(OtpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.MensajeError = "Por favor, ingresa un código válido.";
                return View("Otp", model);
            }

            if (string.IsNullOrEmpty(model.OtpSessionId))
            {
                return RedirectToAction("Index");
            }

            var resultado = await _apiClient.VerifyOtpAsync(model.OtpSessionId, model.Codigo);

            if (resultado == null)
            {
                model.MensajeError = "Verificación fallida. Verifica el código e intenta nuevamente.";
                return View("Otp", model);
            }

            // MODIFICACIÓN: Crear claims incluyendo el Token JWT
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, resultado.Usuario),
                new Claim("jwt_token", resultado.Token) // <--- NUEVO: Guardamos el token aquí
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            _logger.LogInformation("Usuario {Usuario} completó 2FA e inició sesión", resultado.Usuario);

            return Redirect("~/Inicio/Index");
        }

        // POST: /Login/ReenviarOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReenviarOtp(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return Json(new { success = false, message = "Sesión inválida." });
            }

            var exito = await _apiClient.ReenviarCodigoOtpAsync(sessionId);

            if (exito)
            {
                _logger.LogInformation("Código OTP reenviado para sesión: {SessionId}", sessionId);
                return Json(new
                {
                    success = true,
                    message = "Se ha reenviado el código de verificación a tu correo."
                });
            }

            return Json(new
            {
                success = false,
                message = "No se pudo reenviar el código. Espera un momento e intenta nuevamente."
            });
        }

        // POST: /Login/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var usuario = User.Identity?.Name;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("Usuario {Usuario} cerró sesión", usuario);
            return RedirectToAction("Index", "Login");
        }
    }
}
