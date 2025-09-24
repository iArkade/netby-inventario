# Sistema de Gestión de Inventarios

## Descripción

Sistema web para gestión de inventarios con arquitectura de microservicios. Permite administrar productos, registrar transacciones de compra/venta, y generar reportes.

### Características

- **Gestión de Productos**: CRUD completo con categorización y control de stock
- **Gestión de Transacciones**: Registro de compras y ventas con actualización automática de inventario
- **Reportes y Filtros**: Análisis avanzado con filtros dinámicos
- **Interfaz Moderna**: Diseño responsivo con notificaciones en tiempo real

### Tecnologías

**Backend:** .NET 8.0, Entity Framework Core, MySQL, ASP.NET Core Web API, Microservicios

**Frontend:** React 19.x, TypeScript, Bootstrap 5.3, Vite

---

## Requisitos

- **.NET 8.0 SDK** - [Descargar](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 18.x+** - [Descargar](https://nodejs.org/)
- **MySQL** (XAMPP recomendado) o MySQL standalone

### Verificar instalaciones:
```bash
dotnet --version  # Debe mostrar 8.0.x
node --version    # Debe mostrar v18.x.x+
npm --version     # Debe mostrar 9.x.x+
```

---

## Instalación

### 1. Configurar Base de Datos

#### Con XAMPP:
```powershell
# Iniciar Apache y MySQL en XAMPP
# Ejecutar el script en PowerShell en la raiz de proyecto:
Get-Content ".\gestion_inventario.sql" | & "C:\xampp\mysql\bin\mysql.exe" -u root -p
```

#### Sin XAMPP (MySQL standalone):
```bash
# Windows PowerShell:
Get-Content ".\gestion_inventario.sql" | mysql -u root -p

# Linux/Mac Terminal:
mysql -u root -p < gestion_inventario.sql
```

### 2. Ejecutar Backend

#### Terminal 1 - Microservicio de Productos:
```bash
cd backend/Productos
dotnet restore
dotnet run
# Disponible en: https://localhost:7001
```

#### Terminal 2 - Microservicio de Transacciones:
```bash
cd backend/Transacciones
dotnet restore
dotnet run
# Disponible en: https://localhost:7002
```

### 3. Ejecutar Frontend

```bash
cd frontend/inventario-front
npm install
npm run dev
# Disponible en: http://localhost:5173
```

### 4. Configurar Variables de Entorno

Crear `.env` en el directorio del frontend:
```env
VITE_PRODUCTOS_API_URL=https://localhost:7001
VITE_TRANSACCIONES_API_URL=https://localhost:7002
VITE_ENV=development
```

---

## Verificación

- **Productos API**: `https://localhost:7001/swagger`
- **Transacciones API**: `https://localhost:7002/swagger`
- **Frontend**: `http://localhost:5173`

---

## Problemas Comunes

#### Error de certificado SSL:
```bash
dotnet dev-certs https --trust
```

#### Error de CORS:
- Verificar que las URLs del frontend están configuradas en `appsettings.json`
- Reiniciar los servicios del backend

#### Error de conexión MySQL:
- Verificar que MySQL esté corriendo
- Comprobar credenciales en las cadenas de conexión

---

## Autor

**Isaac Velasco**
- GitHub: [iArkade](https://github.com/iArkade)
- Email: isaacvelasco2000@gmail.com