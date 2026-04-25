# Plataforma de Creditos

Aplicacion ASP.NET Core MVC con Identity, EF Core SQLite, catalogo de solicitudes, registro con validaciones, sesion, cache distribuida y panel para analista.

## Requisitos

- .NET SDK 9.0
- SQLite
- Redis opcional en local

## Ejecucion local

1. Restaurar dependencias:

```bash
dotnet restore
```

2. Ejecutar la aplicacion:

```bash
dotnet run
```

3. Abrir la URL local que muestre la consola.

La aplicacion aplica migraciones y datos semilla automaticamente al iniciar.

## Migraciones

La migracion actual incluida en el proyecto es la inicial del esquema con Identity y dominio.

Si necesitas crear una nueva migracion:

```bash
dotnet ef migrations add NombreMigracion
dotnet ef database update
```

## Variables de entorno

Minimas para produccion y Render:

- `ASPNETCORE_ENVIRONMENT=Production`
- `ASPNETCORE_URLS=http://0.0.0.0:${PORT}`
- `ConnectionStrings__DefaultConnection=Data Source=/var/data/creditos.db`
- `Redis__ConnectionString=<cadena de conexion redis>`

Notas:

- Si `Redis__ConnectionString` no esta configurada, la aplicacion usa cache distribuida en memoria.
- Para SQLite en Render se recomienda montar un disco persistente y apuntar la base a una ruta persistente como `/var/data/creditos.db`.

## Usuarios de prueba

Semilla incluida:

- Analista
  - Usuario: `Cristopher vilchz`
  - Email: `cristopher_vilchez@usmp.pe`
  - Password: `Analista@2026`

- Cliente 1
  - Usuario: `cliente.uno`
  - Email: `cliente1@creditos.local`
  - Password: `Cliente@2026`

- Cliente 2
  - Usuario: `cliente.dos`
  - Email: `cliente2@creditos.local`
  - Password: `Cliente@2026`

Adicionalmente, cada usuario nuevo registrado desde la UI crea automaticamente su perfil de cliente activo con sus ingresos mensuales.

## Funcionalidades implementadas

- Registro de usuarios con creacion automatica de cliente
- Catalogo de solicitudes con filtros por estado, monto y fechas
- Registro de solicitudes con validaciones server-side
- Sesion para mostrar la ultima solicitud visitada
- Cache distribuida por usuario del listado de solicitudes
- Panel de analista para aprobar o rechazar solicitudes pendientes

## Despliegue en Render

1. Crear un nuevo `Web Service` en Render conectado al repositorio.
2. Seleccionar `Docker` como entorno de despliegue.
3. Configurar las variables de entorno:
   - `ASPNETCORE_ENVIRONMENT`
   - `ASPNETCORE_URLS`
   - `ConnectionStrings__DefaultConnection`
   - `Redis__ConnectionString`
4. Si usaras SQLite en produccion, agregar un disco persistente y usar una ruta de base dentro de ese disco.
5. Desplegar el servicio.

Archivo de apoyo incluido:

- `render.yaml`

## Verificacion sugerida online

- Registro de usuario cliente
- Login
- Registro de solicitud
- Validacion de monto maximo
- Visualizacion de ultima solicitud visitada
- Panel del analista con aprobacion y rechazo
- Revalidacion del listado tras invalidacion de cache

## URL de Render

Pendiente de completar una vez desplegado en Render.
