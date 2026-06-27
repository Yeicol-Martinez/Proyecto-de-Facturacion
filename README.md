# 🧾 FacturaciónApp

Sistema web de facturación desarrollado con **ASP.NET Core MVC** y **Entity Framework Core**, orientado a la gestión de clientes, productos, facturas, solicitudes y usuarios. La aplicación incorpora autenticación mediante **ASP.NET Core Identity** y registra auditorías para mejorar el seguimiento de las operaciones realizadas.

---

## Características principales

- Gestión de clientes.
- Administración de productos.
- Creación y administración de facturas.
- Gestión de solicitudes.
- Administración de usuarios.
- Inicio de sesión y autenticación con ASP.NET Core Identity.
- Registro de auditoría de operaciones.
- Persistencia de datos mediante Entity Framework Core y SQL Server.

---

## Tecnologías utilizadas

- ASP.NET Core MVC (.NET 10)
- C#
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- Razor Views
- HTML5
- CSS3
- Bootstrap
- JavaScript

---

## Estructura del proyecto

```
FacturacionApp
│
├── Controllers
│   ├── AuditoriaController
│   ├── ClienteController
│   ├── CuentaController
│   ├── FacturaController
│   ├── HomeController
│   ├── ProductoController
│   ├── SolicitudController
│   └── UsuarioController
│
├── Models
│   ├── AuditoriaFactura
│   ├── Cliente
│   ├── Factura
│   ├── Producto
│   ├── Solicitud
│   ├── Usuario
│   └── FacturacionContext
│
├── Views
├── Migrations
├── wwwroot
├── Properties
├── Program.cs
├── SeedData.cs
└── appsettings.json
```

---

## Arquitectura

El proyecto sigue el patrón **Modelo–Vista–Controlador (MVC)**.

- **Models:** representan las entidades del sistema y el acceso a datos.
- **Views:** contienen la interfaz de usuario mediante Razor.
- **Controllers:** gestionan la lógica de negocio y las solicitudes HTTP.

---

## Funcionalidades

- Registro e inicio de sesión de usuarios.
- Gestión de clientes.
- Gestión de productos.
- Creación de facturas.
- Administración de solicitudes.
- Registro de auditoría de facturas.
- Navegación mediante controladores MVC.

---

## Requisitos

- Visual Studio 2022 o superior
- .NET 10 SDK
- SQL Server
- Git

---

## Instalación

### 1. Clonar el repositorio

```bash
git clone https://github.com/TU-USUARIO/FacturacionApp.git
```

### 2. Acceder al proyecto

```bash
cd FacturacionApp
```

### 3. Configurar la base de datos

Modificar la cadena de conexión en **appsettings.json**.

Ejemplo:

```json
"ConnectionStrings": {
  "FacturacionDB": "Server=SERVIDOR;Database=FacturacionDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 4. Aplicar las migraciones

```bash
dotnet ef database update
```

### 5. Ejecutar la aplicación

```bash
dotnet run
```

o desde Visual Studio presionando **F5**.

---

## Seguridad

La aplicación implementa **ASP.NET Core Identity** para la autenticación de usuarios y configuración de cookies, garantizando un acceso seguro a las funcionalidades del sistema.

---

## Base de datos

La persistencia de datos se realiza mediante **Entity Framework Core** con **SQL Server**, utilizando migraciones para el control de versiones del esquema de la base de datos.

---

## Autor

Proyecto desarrollado con fines académicos como práctica de desarrollo de aplicaciones web utilizando **ASP.NET Core MVC**, **Entity Framework Core** y **SQL Server**.
Desarrollado por: Yeicol Martinez.
