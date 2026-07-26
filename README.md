## Ejercicio de Q10 - Pedidos - Prueba tecnica

El proyecto incluye los siguientes servicios:

- **SQL Server**: almacenamiento de pedidos e inventario.
- **RabbitMQ**: comunicación asíncrona entre la API y el worker.
- **Orders.API**: API REST desarrollada en ASP.NET Core.
- **Inventory.Worker**: procesamiento de reserva y rechazo de inventario.
- **Frontend**: aplicación Angular 22 servida mediante Nginx.

# Diseño frontend

Para este caso en el registor de Pedido, me tome el atrevimiento de crear una lista desplegable para seleccionar los productos con el SKU, debido a que me parecio una manera mas natural de realizar un pedido sobre lo existente.




### Requisitos

Antes de ejecutar el proyecto, es necesario tener instalado:

- Docker
- Docker Compose

Puedes comprobar la instalación con:

```bash
docker --version
docker compose version
```

---

## Arquitectura x64: configuración predeterminada

El archivo `docker-compose.yml` está configurado por defecto para ejecutar **SQL Server 2022** en servidores o computadores con arquitectura `x86_64`/`amd64`.

Esta es la configuración que debe utilizarse normalmente en:

- Servidores Linux x64.
- Máquinas con procesadores Intel.
- Máquinas con procesadores AMD.
- Plataformas de despliegue como Dokploy sobre servidores x64.

Ejecuta para este tipo de equipos asi

```bash
docker compose up -d --build
```

---

## Equipos ARM y Apple Silicon

La imagen estándar de SQL Server 2022 puede presentar problemas al ejecutarse mediante emulación en equipos ARM, como los Mac con procesadores Apple Silicon M1, M2, M3, M4, M5.

Para ejecutar el proyecto localmente en uno de estos equipos, ejecuta el siguiente comando:

```bash
docker compose -f docker-compose.dev.yml up -d --build

```
Docker Compose construirá y levantará los servicios respetando el siguiente orden:

```text
SQL Server ─────┐
                ├── Orders.API ─── Frontend
RabbitMQ ───────┤
                └── Inventory.Worker
```

SQL Server y RabbitMQ cuentan con comprobaciones de estado. La API y el worker no iniciarán hasta que ambos servicios se encuentren disponibles.

---

## Migraciones y datos iniciales

`Orders.API` ejecuta automáticamente las migraciones de Entity Framework Core durante su inicio:


En una base de datos nueva, este proceso:

1. Crea la base de datos `Q10Pedidos`.
2. Crea la tabla `Pedidos`.
3. Crea la tabla `Stock`.
4. Ejecuta el seed inicial de inventario.
5. Registra las migraciones aplicadas en `__EFMigrationsHistory`.

Los datos iniciales de inventario son:

| SKU | Cantidad disponible |
|---|---:|
| SKU001 | 10 |
| SKU002 | 5 |
| SKU003 | 20 |
| SKU004 | 40 |
| SKU005 | 2 |

---

## Verificar los contenedores

Para comprobar el estado de los servicios:

```bash
docker compose ps
```

El resultado esperado es similar a:

```text
q10-sqlserver          Up (healthy)
q10-rabbitmq           Up (healthy)
q10-orders-api         Up
q10-inventory-worker   Up
q10-frontend           Up
```

## Acceso a los servicios

Una vez iniciado el proyecto, estarán disponibles las siguientes direcciones:

| Servicio | Dirección |
|---|---|
| Aplicación web | `http://localhost:4200` |
| API directa | `http://localhost:8080` |
| RabbitMQ Management | `http://localhost:15672` |
| SQL Server | `localhost:1433` |

Credenciales de RabbitMQ:

```text
Usuario: guest
Contraseña: guest
```

---

## Proxy entre Angular y la API

La configuración de producción de Angular utiliza una ruta relativa:

```typescript
export const environment = {
  production: true,
  apiUrl: '/api'
};
```

Nginx recibe las peticiones realizadas a `/api` y las reenvía internamente hacia `Orders.API`:

```nginx
location /api/ {
    proxy_pass http://orders-api:8080;

    proxy_http_version 1.1;

    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
}
```

El flujo de una petición es:

```text
Angular
  ↓ /api/Orders
Nginx
  ↓ http://orders-api:8080/api/Orders
Orders.API
```

El nombre `orders-api` solamente se resuelve dentro de la red de Docker. No debe utilizarse como URL en el código Angular, ya que la aplicación Angular se ejecuta en el navegador del usuario.

---

## Probar la comunicación completa

Para probar el acceso a la API pasando por Angular y Nginx:

```bash
curl -v http://localhost:4200/api/Orders
```

Una respuesta correcta debe incluir:

```text
HTTP/1.1 200 OK
```

También se puede consultar directamente la API:

```bash
curl -v http://localhost:8080/api/Orders
```

---

## Detener el proyecto

Para eliminar también la base de datos y los datos persistentes de RabbitMQ:

```bash
docker compose down -v
```

> Advertencia: la opción `-v` elimina los volúmenes. La base de datos será creada nuevamente y las migraciones se ejecutarán desde cero en el siguiente inicio.

---

# Cómo manejar los fallos

## Si Inventory no responde
El comportamiento razonable es que el pedido permanezca **Pending** . No recomiendo rechazarlo automáticamente por timeout: Inventory podría haber descontado stock y haberse perdido solamente la respuesta.
Con colas durables, mensajes persistentes y Outbox:
* El pedido continúa Pending.
* RabbitMQ conserva OrderCreated.
* Cuando Inventory vuelve, procesa el mensaje.
* La interfaz sigue consultando hasta recibir Confirmed o Rejected.
* Un Pending demasiado antiguo puede mostrarse como “demorado” y generar una alerta operativa.

# Que haría con mas tiempo

* Implementaría WebSockets
* Implementaria Kubernetes , actualmente lo desplegare en dokploy sobre servidores x64 personal
expuesto con cloudflare.
