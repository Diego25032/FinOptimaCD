# FinOptima: por qué todavía no se ve como un producto para vender

Revisión de noviembre de 2026 sobre el código de `FinOptima10`.

## En una frase

El código está ordenado y bien separado en capas. El problema no es la calidad
interna, sino que **todo lo que ve un comprador dice "proyecto de clase"**: la
documentación, la instalación, la forma de distribuirlo y la falta de modelo de
negocio.

## Lo que ya está bien (y hay que mostrar)

- Capas limpias (Entidades → Datos → Lógica → Vista/Bot) con contratos e inyección por constructor.
- Seguridad seria: PBKDF2 con 100.000 iteraciones, bloqueo por intentos y aislamiento de datos por sesión.
- Una idea diferenciadora: **método de los 7 sobres + bot de Telegram que reparte el sueldo solo**. Ese es el gancho comercial.
- 151 pruebas automáticas sobre la lógica.

## Los 8 motivos por los que no "vende"

### 1. Se presenta como trabajo académico
- El README dedica la mitad a SOLID y GRASP y termina con *"El código está pensado para alguien que está aprendiendo C#: sin LINQ, sin async, sin var…"*. Un comprador o inversionista lee eso como "ejercicio de universidad".
- Hay diagramas de clases, pero no hay capturas de pantalla, una propuesta de valor, precios ni una página de producto.

**Qué hacer:** mover la parte técnica a `docs/ARQUITECTURA.md` y dejar un README de producto: problema → solución → 3 capturas → cómo instalar → planes.

### 2. Instalarlo exige ser programador
Hoy el usuario necesita el .NET 10 SDK, Docker Desktop, levantar PostgreSQL, ejecutar SQL de migraciones a mano, configurar variables con `setx` y correr scripts de PowerShell. Ningún cliente final hace eso.

**Qué hacer:**
- Publicar como ejecutable *self-contained* con un instalador (Velopack, MSIX o Inno Setup) que además se actualice solo.
- Para uso personal, reemplazar Docker + PostgreSQL por una base local embebida (SQLite). Los repositorios ya están detrás de interfaces, así que es un cambio acotado en `FinOptima.Datos`.
- Aplicar las migraciones automáticamente al iniciar.

### 3. Solo existe en Windows de escritorio
WinForms limita el mercado a PCs con Windows. Las apps de finanzas personales hoy se usan sobre todo en el celular. El bot de Telegram ayuda, pero no reemplaza una app.

**Qué hacer:** separar una **API** (ASP.NET Core) que exponga la capa Lógica y construir encima una versión web/móvil. El escritorio puede quedar como edición "offline".

### 4. El bot es de un solo usuario y se conecta directo a la base
- `FinOptima.Bot` abre conexión directa a PostgreSQL, inicia sesión con el correo y la contraseña del usuario guardados en variables de entorno y responde a **un solo chat fijo**.
- Para vender, cada cliente tendría que montar su propio bot, su token y su base.

**Qué hacer:** un solo bot oficial, conectado a la API. Cada usuario vincula su cuenta desde la app (botón "Conectar Telegram" que genera un enlace `t.me/FinOptimaBot?start=<código>`). Así el bot es una función del producto, no una instalación aparte.

### 5. Credenciales y datos sensibles poco cuidados
- Usuario y contraseña de base por defecto (`finoptima`/`finoptima`) en `docker-compose.yml` y en el código de conexión.
- La contraseña de la cuenta del bot vive en texto plano en las variables de Windows.
- El respaldo JSON con todas las finanzas se guarda sin cifrar.

**Qué hacer:** generar credenciales en la instalación, usar el almacén seguro de Windows (DPAPI) para secretos, cifrar los respaldos con una clave del usuario y conexión TLS cuando la base no es local.

### 6. Licencias: no se podía vender tal como estaba
- PDFsharp 6.5 es **AGPL**: obliga a publicar el código fuente completo de cualquier aplicación que lo use y se distribuya. Vender FinOptima cerrado con esa versión no era legal sin pagar la licencia comercial (≈ 750 €). Fuente: pdfsharp.com/Pricing.
- **Ya corregido:** el proyecto ahora usa PDFsharp 6.2.4, que es MIT.
- Falta un archivo `LICENSE` propio, términos de uso y política de privacidad. Al manejar datos financieros de personas en Colombia aplica la Ley 1581 de 2012 (habeas data).

### 7. Detalles que bajan la percepción de calidad
- Textos sin tildes en la interfaz ("Mas", "Analisis mensual", "Distribucion").
- Tamaños fijos en píxeles: con el escalado de Windows al 125 % o 150 % (habitual en portátiles) los textos se cortan. **Ya corregido en el Panel y la barra "Más"**; falta aplicar el mismo criterio al resto de módulos y formularios (o activar `PerMonitorV2` con `AutoScaleMode.Dpi`).
- El PDF parecía una página de texto plano y el Excel mostraba `#####`. **Ya corregidos.**
- El ZIP incluye carpetas `bin/`, `obj/` y `.vs/` (archivos de compilación y del editor). Un entregable profesional no las lleva.
- Las pruebas son una consola propia; lo esperado es xUnit/NUnit con integración continua (GitHub Actions) y una insignia de "pruebas pasando".
- No hay número de versión visible, notas de versión ni registro de errores (logs) para soporte.

### 8. No hay modelo de negocio dentro del producto
No existe forma de cobrar: ni planes, ni periodo de prueba, ni activación de licencia, ni medición de uso, ni actualizaciones automáticas, ni un recorrido de bienvenida para el primer uso.

**Qué hacer:** definir un plan gratis (sobres + registros) y uno Pro (bot, reportes, simuladores, respaldos en la nube), con prueba de 14 días.

## Plan sugerido

| Plazo | Acciones |
| --- | --- |
| **1–2 semanas** (impacto inmediato) | README de producto con capturas · tildes en toda la interfaz · `LICENSE`, términos y privacidad · quitar `bin/obj/.vs` del entregable · número de versión en la ventana · instalador *self-contained* |
| **1–2 meses** | SQLite para modo personal con migraciones automáticas · DPAPI para secretos y respaldos cifrados · escalado DPI en todos los módulos · pruebas en xUnit + CI · registro de errores |
| **3–6 meses** | API ASP.NET Core sobre la capa Lógica · bot multiusuario con vinculación por enlace · versión web/móvil · planes, prueba gratis y cobro |

## Cambios ya incluidos en esta entrega

- Bot de Telegram manejado con botones (teclado fijo + botones en cada mensaje).
- Excel rehecho: hoja de resumen, tablas con filtros y totales, montos y fechas bien formateados, sin `#####`.
- PDF rediseñado: portada de marca, indicadores, gráficos, tablas y numeración de páginas; funciona en cualquier equipo.
- PDFsharp pasado de 6.5 (AGPL) a 6.2.4 (MIT).
- Botón "Más" convertido en una barra que se despliega debajo de la navegación.
- Panel de inicio reorganizado en tres filas que llenan la pantalla y respetan el escalado de Windows.
