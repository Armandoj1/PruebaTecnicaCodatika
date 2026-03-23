# BookRadar

Aplicación web desarrollada en **ASP.NET Core MVC (.NET 8)** que permite buscar libros por autor usando la API pública de [Open Library](https://openlibrary.org), guardar el historial de búsquedas en **SQL Server** y consultarlo en cualquier momento.

> Prueba Técnica — Desarrollador Fullstack Semi Senior  
> Stack: C# · .NET 8 · Razor · Entity Framework Core · SQL Server · Bootstrap 5

---

## Tabla de contenidos

- [Requisitos previos](#requisitos-previos)
- [Pasos para ejecutar el proyecto](#pasos-para-ejecutar-el-proyecto)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Base de datos](#base-de-datos)
- [Decisiones de diseño](#decisiones-de-diseño)
- [Extras implementados](#extras-implementados)
- [Mejoras pendientes](#mejoras-pendientes)

---

## Requisitos previos

| Herramienta | Versión mínima | Notas |
|---|---|---|
| .NET SDK | 8.0 | https://dotnet.microsoft.com/download/dotnet/8.0 |
| SQL Server | 2019 | Express, Developer o superior |
| SQL Server Management Studio | Cualquiera | Para ejecutar el script de base de datos |
| Visual Studio | 2022 | Con carga de trabajo **ASP.NET y desarrollo web** |
| Git | Cualquiera | Para clonar el repositorio |

---

## Pasos para ejecutar el proyecto

### 1. Clonar el repositorio

```bash
git clone https://github.com/Armandoj1/BookRadar.git
cd BookRadar
```

### 2. Crear la base de datos

1. Abre **SQL Server Management Studio**.
2. Conéctate a tu instancia local de SQL Server.
3. Ve a **Archivo → Abrir → Archivo...** y selecciona `SQL/create_tables.sql`.
4. Ejecuta el script completo con **F5**.

El script crea automáticamente:
- La base de datos `BookRadarDB`
- La tabla `HistorialBusquedas` con sus columnas, constraints e índices
- El stored procedure `SP_InsertarLibro` (con lógica anti-duplicado de 1 minuto)
- El stored procedure `SP_ObtenerHistorial`

### 3. Configurar la cadena de conexión

Abre `appsettings.json` en la raíz del proyecto y edita `DefaultConnection` según tu entorno:

**Autenticación Windows (recomendado para entornos locales):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BookRadarDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Autenticación SQL (usuario y contraseña):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BookRadarDB;User Id=sa;Password=TuPassword;TrustServerCertificate=True;"
  }
}
```

> Si tu instancia de SQL Server tiene un nombre específico como `DESKTOP-ABC\SQLEXPRESS`, reemplaza `localhost` por ese nombre.

### 4. Restaurar paquetes NuGet

```bash
dotnet restore
```

O en Visual Studio: clic derecho sobre la solución → **Restaurar paquetes NuGet**.

> Si aparecen errores de versión de paquetes, instálalos manualmente:
> ```bash
> dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0
> dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0
> dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0
> ```

### 5. Compilar el proyecto

```bash
dotnet build
```

Debe compilar sin errores antes de continuar.

### 6. Ejecutar el proyecto

**Desde la terminal:**
```bash
dotnet run
```

**Desde Visual Studio:**  
Presiona `F5` o el botón **Ejecutar**.

### 7. Abrir en el navegador

Navega a la URL que indique la consola, por ejemplo:

```
https://localhost:7215
```

La ruta raíz redirige automáticamente al formulario de búsqueda en `/Books`.

---

## Estructura del proyecto

```
BookRadar/
├── Controllers/
│   └── BooksController.cs        # Orquesta solicitudes HTTP, delega a servicios
├── Data/
│   └── AppDbContext.cs            # Contexto de EF Core
├── Models/
│   ├── HistorialBusqueda.cs       # Entidad mapeada a SQL Server via EF Core
│   ├── LibroViewModel.cs          # ViewModel con Data Annotations para validación
│   └── OpenLibraryModels.cs       # DTOs para deserializar la respuesta JSON de la API
├── Services/
│   ├── IOpenLibraryService.cs     # Interfaz del servicio de API
│   ├── OpenLibraryService.cs      # Implementación: HttpClient → Open Library con fields específicos
│   ├── IHistorialService.cs       # Interfaz del servicio de historial
│   └── HistorialService.cs        # Implementación: EF Core + Stored Procedures
├── Views/
│   └── Books/
│       ├── Index.cshtml           # Formulario con validación JS + server-side y skeleton loader
│       ├── Results.cshtml         # Tabla de resultados con filtro por año y ordenamiento
│       └── History.cshtml         # Historial paginado con exportación CSV y limpieza
├── SQL/
│   └── create_tables.sql          # Script completo: BD, tabla, índices y Stored Procedures
├── appsettings.json               # Configuración y cadena de conexión
└── Program.cs                     # Registro de servicios y middleware
```

---

## Base de datos

### Tabla: HistorialBusquedas

| Columna | Tipo | Nullable | Descripción |
|---|---|---|---|
| Id | INT IDENTITY(1,1) | NO | Clave primaria autoincremental |
| Autor | NVARCHAR(200) | NO | Nombre del autor buscado |
| Titulo | NVARCHAR(500) | NO | Título del libro guardado |
| AnioPublicacion | INT | SÍ | Primer año de publicación según la API |
| Editorial | NVARCHAR(300) | SÍ | Primera editorial de la lista retornada por la API |
| FechaConsulta | DATETIME | NO | Fecha y hora de la búsqueda. Default: GETDATE() |

### Stored Procedures

**`SP_InsertarLibro`** — recibe `@Autor`, `@Titulo`, `@AnioPublicacion`, `@Editorial` y `@Insertado OUTPUT`. Verifica con `DATEDIFF(SECOND, ...)` si el mismo autor fue buscado hace menos de 1 minuto. Si no hay duplicado inserta y retorna `@Insertado = 1`, si hay duplicado retorna `@Insertado = 0` sin insertar.

**`SP_ObtenerHistorial`** — sin parámetros. Retorna todos los registros de `HistorialBusquedas` ordenados por `FechaConsulta DESC`.

---

## Decisiones de diseño

### Arquitectura: MVC con capa de servicios

Se optó por el patrón MVC estándar de ASP.NET Core sin capas adicionales. La capa de servicios (`Services/`) desacopla la lógica de negocio del controlador y facilita el mantenimiento. Agregar Clean Architecture o CQRS hubiera sido sobreingeniería para el alcance definido en la prueba.

### Colores y paleta visual

Se eligió una paleta **oscura para los elementos de navegación y cabeceras** (negro `#212529`) con fondo de contenido claro (`#f8f9fa`). Esta combinación crea contraste fuerte entre la estructura de la página y el contenido, guiando la atención del usuario hacia los datos. El azul Bootstrap (`#0d6efd`) se reserva para acciones primarias y elementos interactivos, siguiendo el principio de color funcional — el usuario asocia el azul con "hacer algo". Los badges grises para años y azules para autores diferencian visualmente tipos de dato distintos dentro de la misma tabla.

### Tipografía y maquetado

Se usó **Segoe UI** como fuente principal por coherencia con el ecosistema Windows donde corre la aplicación, y por su alta legibilidad en tamaños pequeños dentro de tablas densas. El maquetado usa el sistema de grid de Bootstrap 12 columnas — el formulario ocupa 6 columnas centradas para focalizar la atención, y las tablas ocupan el ancho completo para aprovechar el espacio horizontal en la lectura de datos.

### UX: feedback inmediato al usuario

El **skeleton loader** oculta el formulario al enviar y muestra una animación que imita la estructura de la tabla mientras la API responde, eliminando la percepción de pantalla congelada. El botón se deshabilita durante la búsqueda para evitar doble envío. El campo autor muestra un contador de caracteres en tiempo real para que el usuario sepa el límite antes de llegar a él.

### Filtro por año: slider dual

Escribir un año manualmente es propenso a errores de tipeo. El slider dual detecta automáticamente el rango mínimo y máximo de años presentes en los resultados actuales y actualiza el contador de registros visibles en tiempo real, todo client-side sin llamadas adicionales al servidor.

### Consumo de la API con campos explícitos

La API de Open Library no retorna `publisher` en su respuesta por defecto. Se agregó el parámetro `&fields=title,first_publish_year,publisher` a la URL para solicitarle exactamente los campos necesarios, reduciendo el tamaño de la respuesta y garantizando que la editorial esté disponible para mostrar y persistir.

### Guardado limitado a 5 libros por búsqueda

La API puede retornar hasta 20 resultados por consulta. Guardar todos generaría un historial inflado con filas repetidas del mismo autor. Se optó por persistir solo los primeros 5 libros de cada búsqueda — suficiente para trazabilidad sin degradar la legibilidad del historial.

### Stored Procedures para insert, EF Core para lectura

El insert usa `SP_InsertarLibro` que centraliza la lógica anti-duplicado directamente en SQL Server. La lectura del historial y la limpieza usan EF Core directamente. Esta combinación demuestra ambos enfoques en la misma aplicación tal como pide la prueba.

### Validación en dos capas

Se implementó validación **frontend con JavaScript** (campo vacío, longitud, doble envío) y validación **server-side con Data Annotations** (`[Required]`, `[MaxLength]`, `[MinLength]`) verificada con `ModelState.IsValid` en el controlador. La validación JS mejora la experiencia; la server-side garantiza integridad aunque se deshabilite el JS en el navegador.

### HttpClient registrado con `AddHttpClient<>`

Se registró `OpenLibraryService` con `AddHttpClient<IOpenLibraryService, OpenLibraryService>()` en lugar de instanciar `HttpClient` directamente, gestionando correctamente el ciclo de vida del cliente y evitando el agotamiento de sockets.

---

## Extras implementados

- ✅ **Anti-duplicado < 1 minuto** — `SP_InsertarLibro` con `DATEDIFF(SECOND, ...) < 60`
- ✅ **Entity Framework Core** — `AppDbContext` con configuración fluente, consulta del historial y limpieza
- ✅ **Stored Procedures** — `SP_InsertarLibro` y `SP_ObtenerHistorial`
- ✅ **Bootstrap 5 + Bootstrap Icons** — interfaz responsiva y cohesiva
- ✅ **Validación frontend JS** — campo vacío, doble envío, contador de caracteres
- ✅ **Validación server-side** — Data Annotations + `ModelState.IsValid` en el controlador
- ✅ **Skeleton loader** — feedback visual durante la llamada a la API
- ✅ **Campos explícitos en la API** — parámetro `&fields=` para traer `publisher` correctamente
- ✅ **Guardado limitado a 5 libros por búsqueda** — historial legible sin registros inflados
- ✅ **Filtro por año con slider dual** — rango interactivo client-side con años reales
- ✅ **Ordenamiento por columnas** — clic en encabezados, detecta numérico vs texto
- ✅ **Paginación en historial** — 10 registros por página con controles de navegación
- ✅ **Exportación CSV** — descarga del historial completo sin librerías externas
- ✅ **Limpiar historial** — botón con confirmación que elimina todos los registros vía EF Core

---

## Mejoras pendientes

**1. Caché de respuestas de la API**  
Implementar `IMemoryCache` para guardar temporalmente los resultados por autor y reducir llamadas a Open Library. Con un TTL de 30 minutos se evitarían consultas repetidas al mismo autor en sesiones cortas.

**2. Paginación server-side en el historial**  
La paginación actual carga todos los registros en memoria en cada petición. Migrar a `Skip/Take` en EF Core con parámetros de página reduciría la carga de datos cuando el volumen de registros crezca significativamente.

**3. Búsqueda por título**  
Open Library soporta `?title=` como parámetro alternativo. Agregar un selector de tipo de búsqueda (por autor / por título) ampliaría el alcance sin cambiar la arquitectura existente.

**4. Tests unitarios**  
Agregar tests para `OpenLibraryService` mockeando el `HttpClient` con `MockHttpMessageHandler`, y tests para `HistorialService` usando la base de datos InMemory de EF Core para validar el comportamiento del anti-duplicado.

**5. Autenticación de usuarios**  
Integrar ASP.NET Core Identity para que cada usuario tenga su propio historial de búsquedas aislado, con registro, login y gestión de sesión.

---

## Autor

**José Armando Rodríguez Tapia**  
Desarrollador Full Stack
[github.com/Armandoj1](https://github.com/Armandoj1)
