## Ejercicio de Q10 - Pedidos - Prueba tecnica

El proyecto incluye los siguientes servicios:

- **SQL Server**: almacenamiento de pedidos e inventario.
- **RabbitMQ**: comunicación asíncrona entre la API y el worker.
- **Orders.API**: API REST desarrollada en ASP.NET Core 10.
- **Inventory.Worker**: procesamiento de reserva y rechazo de inventario en ASP.NET Core 10.
- **Frontend**: aplicación Angular 22 servida mediante Nginx.

# Diseño frontend

Para este caso en el registor de Pedido, me tome el atrevimiento de crear una lista desplegable para seleccionar los productos con el SKU, debido a que me parecio una manera mas natural de realizar un pedido sobre lo existente.


# Cómo se manejó la idempotencia

Cada evento `OrderCreated` contiene un `EventId` único. Inventory.Worker registra este identificador en la tabla `InboundOrder`, donde `EventId` es la llave primaria.

Cuando llega un evento:

1. Inventory.Worker consulta si el `EventId` ya está registrado.
2. Si no existe, registra la solicitud con estado `Pending` y almacena si existe disponibilidad.
3. Si está `Pending` y tiene disponibilidad, descuenta el stock y cambia el registro a `Reserved`.
4. Si está `Pending` y no tiene disponibilidad, cambia el registro a `Rejected`.
5. Si el evento vuelve a llegar y ya está `Reserved` o `Rejected`, no se vuelve a modificar el stock.

De esta manera, un evento duplicado que ya terminó su procesamiento no produce un segundo descuento.

Como trade-off, la actualización del stock y el cambio de estado de `InboundOrder` se realizan en operaciones separadas. Una caída exactamente después de guardar el descuento y antes de marcar el evento como `Reserved` podría permitir que una reentrega vuelva a procesarlo. En una solución de producción ambas operaciones deberían ejecutarse dentro de una misma transacción.

# Arquitectura

La solución se organizó como un monorepositorio compuesto por dos aplicaciones backend independientes y una aplicación frontend:
* Orders.API: administra la creación y el estado de los pedidos.
* Inventory.Worker: procesa las solicitudes de inventario.
* Aplicación Angular: permite crear pedidos y consultar sus estados.
* RabbitMQ: desacopla el procesamiento de pedidos e inventario.
* SQL Server: persiste pedidos, inventario y eventos recibidos.

Esta separación permite ejecutar y desplegar cada aplicación de manera independiente, aunque para simplificar la prueba ambos servicios backend utilizan la misma base de datos.

## Arquitectura por capas

Dentro de cada servicio backend se utilizó una arquitectura por capas orientada a separar responsabilidades. No se buscó implementar Clean Architecture de manera estricta, sino mantener una estructura sencilla y proporcional al alcance de la prueba.
### Controllers

Los controladores son responsables únicamente de recibir solicitudes HTTP, delegar el caso de uso al servicio correspondiente y construir la respuesta HTTP.
No contienen validaciones de negocio ni acceso directo a Entity Framework.
### DTOs

Los DTO representan los contratos de entrada y salida de la API. Permiten evitar que el contrato HTTP dependa directamente de las entidades persistidas en la base de datos.
### Validators

Las validaciones se encuentran separadas de los controladores y repositorios. Esto permite concentrar las reglas de entrada, por ejemplo:
* Nombre del cliente obligatorio.
* SKU obligatorio y existente.
* Cantidad entre 1 y 100.
La decisión evita duplicar validaciones y permite probarlas independientemente.

### Transforms

Los transformadores convierten los DTO en entidades del dominio. Así, OrderService no necesita conocer los detalles de construcción de un Pedido, y los cambios en el contrato HTTP no se propagan directamente a la persistencia.

### Services

Los servicios contienen la coordinación de los casos de uso.
Por ejemplo, OrderService:
1. Valida la solicitud.
2. Comprueba que el SKU exista.
3. Transforma el DTO.
4. Persiste el pedido como Pending.
5. Solicita la publicación de OrderCreated.
La lógica se mantiene fuera del controlador para que el mismo caso de uso pueda reutilizarse y probarse sin depender directamente de HTTP.

### Repositories

Los repositorios encapsulan el acceso a Entity Framework Core. Los servicios expresan operaciones del negocio —buscar pedido, consultar stock, actualizar estado— sin implementar directamente consultas SQL o manipular el contexto.
Esta separación añade algunas interfaces y clases, pero facilita:
* Sustituir o simular la persistencia en pruebas.
* Mantener las consultas concentradas.
* Evitar que la lógica de negocio dependa de Entity Framework.
* Cambiar detalles de persistencia sin modificar controladores o consumidores.

### Consumers and Messaging

La integración con RabbitMQ se mantiene separada de los servicios mediante consumidores y publishers.
Los consumidores se responsabilizan de:
* Recibir y deserializar mensajes.
* Delegar el procesamiento a servicios.
* Realizar Ack o Nack.
Los servicios se ocupan de la lógica de inventario o de las transiciones de estado. Esta separación evita mezclar detalles de RabbitMQ con reglas de negocio.

### Middleware

El manejo de excepciones HTTP está centralizado en un middleware. Esto evita repetir bloques try/catch en cada controlador y mantiene un formato uniforme de errores para el frontend.

### Inyección de dependencias

Las dependencias se registran en Program.cs y se consumen a través de interfaces:
* Controller → Service → Repository → DbContext
* Consumer   → Service → Repository → DbContext
Por ejemplo, el controlador depende de IOrderService, no de OrderService, y el servicio depende de interfaces de repositorio y mensajería.
Esta decisión reduce el acoplamiento y facilita sustituir implementaciones durante las pruebas.

### Comunicación asíncrona

Se eligió RabbitMQ porque la reserva de inventario no necesita ocurrir dentro de la misma solicitud HTTP y lo sugieren en la prueba.
El flujo es:
Orders API
    │ guarda Pedido = Pending
    ▼
OrderCreated
    │
    ▼
Inventory Worker
    │
    ├── StockReserved
    └── StockRejected
            │
            ▼
## Orders API actualiza el pedido

Esto introduce consistencia eventual: el POST devuelve inicialmente un pedido Pending y, después de procesar los eventos, cambia a Confirmed o Rejected.
La ventaja es que Orders no depende de que Inventory responda de manera síncrona. La desventaja es que deben contemplarse mensajes duplicados, fallos de publicación y pedidos que permanezcan Pending.

## Persistencia compartida

Para mantener la solución proporcional al tiempo de la prueba, ambos servicios usan una instancia compartida de SQL Server.

### Ventajas:

Un solo contenedor de datos.
Migraciones y seed sencillos.
Menor complejidad operativa.
Orders puede validar fácilmente que el SKU exista.

### Trade-off:

Orders e Inventory quedan acoplados al mismo esquema.
No es un aislamiento completo de microservicios.
Un cambio de esquema puede afectar a ambos servicios.
Con más tiempo, cada servicio podría ser dueño de su base de datos o esquema y compartir información exclusivamente mediante eventos. Para esta prueba se priorizó una solución fácil de ejecutar y explicar.

## Contratos de eventos

Cada servicio mantiene sus propias clases de mensajes. Esto evita crear una librería compartida que acople las compilaciones de ambos proyectos.
El trade-off es que los contratos pueden desincronizarse. En una solución mayor utilizaría:
Un paquete versionado de contratos.
Validación de esquemas.
Versionado explícito de eventos.
Para el alcance actual, mantener contratos pequeños y equivalentes resulta suficiente.

## Frontend

El frontend separa:
Componentes visuales.
Servicios HTTP.
Modelos.
Configuración por ambientes.
Se eligió polling cada cinco segundos porque el enunciado lo acepta y reduce la complejidad frente a SignalR. El trade-off es que las actualizaciones no son instantáneas y se generan solicitudes periódicas, pero para el volumen esperado es una decisión proporcional.

# Test
* Idempotencia
* Transiciones de estado
* Validación de negocio

Se puede probar asi
```bash
dotnet test Inventory.Worker.Tests/Inventory.Worker.Tests.csproj
```

```bash
dotnet test Orders.API.Tests/Orders.API.Tests.csproj
```

## Docker y configuración

Cada aplicación tiene su propio Dockerfile y Docker Compose levanta:
* Frontend.
* Orders API.
* Inventory Worker.
* RabbitMQ.
* SQL Server.
La configuración sensible se externalizó mediante variables de entorno. .env contiene la configuración local y no se versiona; .env.example informa qué variables necesita el sistema sin exponer secretos.

## Configuración de variables de entorno

El repositorio contiene un archivo `.env.example` con las variables necesarias para ejecutar el proyecto.

Antes de levantar los contenedores, se debe crear el archivo local `.env`:

```bash
cp .env.example .env
```


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
docker compose -f docker-compose-arm64.yml up -d --build

```
Docker Compose construirá y levantará los servicios respetando el siguiente orden:


```text
SQL Server ───┐
              ├── Orders.API ─── Inventory.Worker
RabbitMQ ─────┘         │
                        └── Frontend
```

SQL Server y RabbitMQ cuentan con comprobaciones de estado. Orders.API espera a que ambos estén disponibles y ejecuta sus migraciones.

Inventory.Worker espera a que Orders.API esté saludable antes de iniciar. Esto garantiza que las migraciones de `Pedidos` y `Stock` se ejecuten antes de la migración de `InboundOrder`.

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

`Inventory.Worker` ejecuta automáticamente las migraciones de Entity Framework durante su inicio.


En una base de datos nueva, este proceso:

1. Crea la tabla `InboundOrder`.

La tabla es la encargada de validar si una solicitud ya fue procesada o no y emitir una respuesta a rabbitMQ
y asi Orders.API pueda tomarla  y procesarla

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

Las credenciales de RabbitMQ deben configurarse en el archivo `.env`. El archivo `.env.example` muestra las variables necesarias sin incluir credenciales reales.



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

# Manejo de fallos

## Si Inventory.Worker no está disponible

Si Orders.API publica correctamente el evento, pero Inventory.Worker no está disponible, el pedido permanece en estado `Pending`.
Mientras RabbitMQ continúe disponible, el evento permanecerá en la cola hasta que Inventory.Worker vuelva a conectarse y pueda procesarlo. 
Cuando el Worker se recupere, el pedido podrá cambiar a `Confirmed` o `Rejected`.
No se rechaza automáticamente un pedido por timeout, porque Inventory podría estar temporalmente fuera de servicio y recuperarse posteriormente.

## Si RabbitMQ está caído cuando Orders.API publica

Orders.API guarda primero el pedido en la base de datos con estado `Pending` y después intenta publicar el evento `OrderCreated`.

Si RabbitMQ está caído durante la publicación:

1. El pedido ya quedó guardado como `Pending`.
2. La publicación genera una excepción.
3. El endpoint responde con un error `500`.
4. Actualmente no existe un mecanismo automático que vuelva a publicar el evento.

Con un poco mas de tiempo podria definir que todo lo pending sin una publicación se pueda reintentar automáticamente.

Por lo tanto, el pedido por ahora puede permanecer en estado `Pending` aunque nunca haya llegado a Inventory.Worker.


## Si Inventory reserva el stock pero no puede publicar el resultado

Inventory.Worker descuenta el stock y posteriormente publica `StockReserved` o `StockRejected`.

Si RabbitMQ falla durante esa publicación, el pedido puede permanecer `Pending` en Orders.API aunque el inventario ya haya sido procesado.

La implementación actual registra el procesamiento en `InboundOrder`, pero no cuenta con un Outbox para recuperar publicaciones de respuesta pendientes. En una solución de producción agregaría un mecanismo de reintentos controlados, Outbox y una cola de mensajes muertos.

# Que haría con mas tiempo

* Implementaría WebSockets
* Implementaria Kubernetes , actualmente lo desplegare en dokploy sobre servidores x64 personal
expuesto con cloudflare.
* Con más tiempo implementaría el patrón Outbox para guardar el pedido y el evento en la misma transacción, y un proceso en segundo plano se encargaría de publicar los eventos pendientes, este proceso lo consulte en IA, solo que no lo implemente porque en mi experiencia me gusta interiorizar lo que estoy haciendo para en caso de implementarlo, sea con mi estructura.
