// Entrevista OLV - JavaScript

document.addEventListener('DOMContentLoaded', function () {
    // Inicializar lógica GPS
    initGPS();

    // Inicializar campos condicionales
    initConditionalFields();

    // Inicializar colapso de secciones
    initSectionCollapse();
});

// ==========================================
// GPS - Obtención de coordenadas
// ==========================================

function initGPS() {
    const btnGPS = document.getElementById('btnObtenerGPS');
    const inputGPS = document.getElementById('CoordenadasGPS');
    const statusGPS = document.getElementById('gpsStatus');

    if (!btnGPS || !inputGPS) return;

    btnGPS.addEventListener('click', function () {
        if (!navigator.geolocation) {
            showGPSError('Tu navegador no soporta geolocalización');
            return;
        }

        // Mostrar estado de carga
        btnGPS.classList.add('loading');
        btnGPS.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Obteniendo...';
        statusGPS.textContent = 'Obteniendo ubicación...';
        statusGPS.className = 'form-text text-muted';

        navigator.geolocation.getCurrentPosition(
            function (position) {
                // Éxito
                const lat = position.coords.latitude.toFixed(6);
                const lng = position.coords.longitude.toFixed(6);
                const accuracy = position.coords.accuracy.toFixed(0);

                inputGPS.value = `${lat}, ${lng}`;

                btnGPS.classList.remove('loading');
                btnGPS.classList.add('success');
                btnGPS.innerHTML = '<span class="gps-icon">&#10003;</span> Obtenido';

                statusGPS.textContent = `Precisión: ±${accuracy}m`;
                statusGPS.className = 'form-text text-success';

                // Restaurar botón después de 3 segundos
                setTimeout(function () {
                    btnGPS.classList.remove('success');
                    btnGPS.innerHTML = '<span class="gps-icon">&#128205;</span> Obtener';
                }, 3000);
            },
            function (error) {
                // Error
                let mensaje = 'Error al obtener ubicación';

                switch (error.code) {
                    case error.PERMISSION_DENIED:
                        mensaje = 'Permiso de ubicación denegado';
                        break;
                    case error.POSITION_UNAVAILABLE:
                        mensaje = 'Ubicación no disponible';
                        break;
                    case error.TIMEOUT:
                        mensaje = 'Tiempo de espera agotado';
                        break;
                }

                showGPSError(mensaje);
            },
            {
                enableHighAccuracy: true,
                timeout: 15000,
                maximumAge: 0
            }
        );
    });

    function showGPSError(mensaje) {
        btnGPS.classList.remove('loading');
        btnGPS.classList.add('error');
        btnGPS.innerHTML = '<span class="gps-icon">&#10007;</span> Error';

        statusGPS.textContent = mensaje;
        statusGPS.className = 'form-text text-danger';

        setTimeout(function () {
            btnGPS.classList.remove('error');
            btnGPS.innerHTML = '<span class="gps-icon">&#128205;</span> Obtener';
        }, 3000);
    }
}

// ==========================================
// Utilidad: Obtener texto de opción seleccionada
// ==========================================

function getSelectedText(selectElement) {
    if (!selectElement || selectElement.selectedIndex < 0) return '';
    return selectElement.options[selectElement.selectedIndex].text.toLowerCase().trim();
}

function textContains(text, keywords) {
    const lowerText = text.toLowerCase();
    return keywords.some(keyword => lowerText.includes(keyword.toLowerCase()));
}

// ==========================================
// Campos Condicionales
// ==========================================

function initConditionalFields() {
    // 4. Domicilio verificado - Configuración especial
    setupDomicilioVerificado();

    // 4b. Motivo no verificado - Otro
    setupConditionalByText('motivoNoVerificado', {
        showWhen: ['otro'],
        fields: ['campoMotivoOtro']
    });

    // 6. Cumple presentaciones
    setupConditionalByText('cumplePresentaciones', {
        showWhen: ['no'],
        fields: ['campoPresentacionesIncumplidas']
    });

    // 7. Tratamiento obligatorio (mostrar si NO es "ninguno" y hay selección)
    setupConditionalByText('tratamientoObligatorio', {
        hideWhen: ['ninguno', 'seleccione'],
        showWhenNotEmpty: true,
        fields: ['campoAsisteTratamiento']
    });

    // 8. Programa de reeducación
    setupConditionalByText('programaReeducacion', {
        showWhen: ['sí', 'si'],
        fields: ['campoCualPrograma', 'campoAsistePrograma']
    });

    // 9. Restricción contacto víctimas
    setupConditionalByText('restriccionVictimas', {
        showWhen: ['sí', 'si'],
        fields: ['campoRestriccionDetalles']
    });

    // 9b. Incumplimiento detectado
    setupConditionalByText('incumplimientoDetectado', {
        showWhen: ['sí', 'si'],
        fields: ['campoIncumplimientoEspecificar']
    });

    // 10. Tipo monitoreo - Otro (checkbox con clase)
    setupCheckboxConditionalByClass('tipo-monitoreo-otro', 'campoMonitoreoOtro');

    // 12. Grupo conviviente (mostrar si NO es "solo" y hay selección)
    setupConditionalByText('grupoConviviente', {
        hideWhen: ['solo', 'seleccione'],
        showWhenNotEmpty: true,
        fields: ['campoPersonasHogar']
    });

    // 12b. Relación convivientes
    setupConditionalByText('relacionConvivientes', {
        showWhen: ['conflictiva'],
        fields: ['campoObservacionesRelacion']
    });

    // 14. Situación laboral (mostrar si NO es "desempleado" y hay selección)
    setupConditionalByText('situacionLaboral', {
        hideWhen: ['desempleado', 'seleccione'],
        showWhenNotEmpty: true,
        fields: ['campoDetallesLaboral']
    });

    // 16. Actividad educativa
    setupConditionalByText('actividadEducativa', {
        showWhen: ['sí', 'si'],
        fields: ['campoCualEducativa']
    });

    // 17. Consumo sustancias
    setupConditionalByText('consumoSustancias', {
        showWhen: ['sí', 'si'],
        fields: ['campoSustancias', 'campoTratamientoConsumo']
    });

    // 18a. Sustancias - Otro (checkbox con clase)
    setupCheckboxConditionalByClass('sustancia-otro', 'campoOtraSustancia');

    // 19. Tiene Obra Social
    setupConditionalByText('tieneObraSocial', {
        showWhen: ['sí', 'si'],
        fields: ['campoObservacionObraSocial']
    });

    // 20. Tratamiento de salud
    setupConditionalByText('tratamientoSalud', {
        showWhen: ['sí', 'si'],
        fields: ['campoCualTratamientoSalud']
    });

    // 20a. Tiene enfermedad
    setupConditionalByText('tieneEnfermedad', {
        showWhen: ['sí', 'si'],
        fields: ['campoCualEnfermedad']
    });

    // 21. Riesgo evidente - Otro (checkbox con clase)
    setupCheckboxConditionalByClass('riesgo-otro', 'campoOtroRiesgo');

    // 23. Cierre
    setupConditionalByText('cierre', {
        showWhen: ['otro'],
        fields: ['campoCierreOtro']
    });
}

// ==========================================
// Domicilio Verificado - Lógica especial
// ==========================================

function setupDomicilioVerificado() {
    const selectDomicilio = document.getElementById('domicilioVerificado');
    if (!selectDomicilio) return;

    const campoObservaciones = document.getElementById('campoObservacionesDomicilio');
    const campoNombreVerificado = document.getElementById('campoNombrePersonaVerificado');
    const campoMotivoNoVerificado = document.getElementById('campoMotivoNoVerificado');
    const campoNombreNoVerificado = document.getElementById('campoNombrePersonaNoVerificado');
    const alertaNoVerificado = document.getElementById('alertaNoVerificado');
    const seccionesCompletas = document.querySelectorAll('.seccion-entrevista-completa');
    const inputsNombrePersona = document.querySelectorAll('.nombre-persona-input');

    function updateVisibility() {
        const text = getSelectedText(selectDomicilio);

        // Ocultar todos los campos condicionales primero
        [campoObservaciones, campoNombreVerificado, campoMotivoNoVerificado,
         campoNombreNoVerificado, alertaNoVerificado].forEach(campo => {
            if (campo) campo.classList.remove('visible');
        });

        if (text.includes('verificado') && !text.includes('no verificado')) {
            // Domicilio verificado
            if (campoObservaciones) campoObservaciones.classList.add('visible');
            if (campoNombreVerificado) campoNombreVerificado.classList.add('visible');

            seccionesCompletas.forEach(seccion => {
                seccion.style.display = '';
            });

        } else if (text.includes('no verificado')) {
            // Domicilio NO verificado
            if (campoMotivoNoVerificado) campoMotivoNoVerificado.classList.add('visible');
            if (campoNombreNoVerificado) campoNombreNoVerificado.classList.add('visible');
            if (alertaNoVerificado) alertaNoVerificado.classList.add('visible');

            seccionesCompletas.forEach(seccion => {
                seccion.style.display = 'none';
            });

        } else {
            // Sin selección
            seccionesCompletas.forEach(seccion => {
                seccion.style.display = '';
            });
        }
    }

    // Sincronizar los inputs de nombre persona
    inputsNombrePersona.forEach(input => {
        input.addEventListener('input', function() {
            const value = this.value;
            inputsNombrePersona.forEach(otherInput => {
                if (otherInput !== this) {
                    otherInput.value = value;
                }
            });
        });
    });

    selectDomicilio.addEventListener('change', updateVisibility);
    updateVisibility();
}

// ==========================================
// Setup condicional basado en texto
// ==========================================

function setupConditionalByText(selectId, options) {
    const selectElement = document.getElementById(selectId);
    if (!selectElement) return;

    const { showWhen, hideWhen, showWhenNotEmpty, fields } = options;

    function updateVisibility() {
        const text = getSelectedText(selectElement);
        const hasValue = selectElement.value && selectElement.value !== '';
        let shouldShow = false;

        if (showWhen) {
            // Mostrar cuando el texto contiene alguna de las palabras clave
            shouldShow = showWhen.some(keyword => text.includes(keyword.toLowerCase()));
        } else if (hideWhen && showWhenNotEmpty) {
            // Mostrar cuando hay valor Y el texto NO contiene las palabras clave
            const shouldHide = hideWhen.some(keyword => text.includes(keyword.toLowerCase()));
            shouldShow = hasValue && !shouldHide;
        }

        fields.forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (field) {
                if (shouldShow) {
                    field.classList.add('visible');
                } else {
                    field.classList.remove('visible');
                }
            }
        });
    }

    selectElement.addEventListener('change', updateVisibility);
    updateVisibility();
}

// ==========================================
// Setup condicional para checkboxes por clase
// ==========================================

function setupCheckboxConditionalByClass(checkboxClass, fieldId) {
    const checkboxes = document.querySelectorAll('.' + checkboxClass);
    const field = document.getElementById(fieldId);

    if (checkboxes.length === 0 || !field) return;

    function updateVisibility() {
        let anyChecked = false;
        checkboxes.forEach(cb => {
            if (cb.checked) anyChecked = true;
        });

        if (anyChecked) {
            field.classList.add('visible');
        } else {
            field.classList.remove('visible');
        }
    }

    checkboxes.forEach(cb => {
        cb.addEventListener('change', updateVisibility);
    });

    updateVisibility();
}

// ==========================================
// Colapso de Secciones
// ==========================================

function initSectionCollapse() {
    const sectionHeaders = document.querySelectorAll('.seccion-header');

    sectionHeaders.forEach(header => {
        const targetId = header.getAttribute('data-bs-target');
        const target = document.querySelector(targetId);

        if (target) {
            target.addEventListener('show.bs.collapse', function () {
                header.setAttribute('aria-expanded', 'true');
            });

            target.addEventListener('hide.bs.collapse', function () {
                header.setAttribute('aria-expanded', 'false');
            });

            if (target.classList.contains('show')) {
                header.setAttribute('aria-expanded', 'true');
            } else {
                header.setAttribute('aria-expanded', 'false');
            }
        }
    });
}

// ==========================================
// Validación del formulario
// ==========================================

function validateForm() {
    const form = document.getElementById('entrevistaForm');
    if (!form) return true;
    return true;
}
