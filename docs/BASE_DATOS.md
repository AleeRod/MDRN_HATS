# Base de datos de MDRN_HATS

## Alcance y estado

La API usa PostgreSQL en Supabase mediante EF Core y Npgsql. El esquema de la
aplicación está en `public`; Supabase también administra sus propios esquemas
(por ejemplo `auth` y `storage`), que no forman parte del modelo de esta API.
La autenticación de la aplicación usa ASP.NET Identity, no Supabase Auth.

El modelo persistente se define principalmente en
[`AppDbContext`](../backend/Infrastructure/Persistence/AppDbContext.cs), las
entidades de `backend/Domain/Entities` y las configuraciones de EF Core en
`backend/Infrastructure/Persistence/Configurations`.

Al 17 de noviembre de 2026 se verificó la conexión de la API con Supabase y se
aplicaron las dos migraciones listadas abajo. Al iniciar, la API vuelve a
comprobar migraciones con `Database.MigrateAsync`; si ya están aplicadas, no
las repite. EF Core registra su historial en `__EFMigrationsHistory`.

## Tablas de la aplicación

### Identidad y autenticación

ASP.NET Identity crea las tablas `AspNet*`. Las claves de usuario y rol son
UUID (`Guid`).

| Tabla | Propósito |
| --- | --- |
| `AspNetUsers` | Cuentas de clientes y administradores. Añade `DisplayName` y `CreatedAtUtc` a los campos de Identity. Incluye correo, nombre de usuario, hash de contraseña, confirmación, bloqueo y datos de seguridad. Las contraseñas no se guardan en texto plano. |
| `AspNetRoles` | Catálogo de roles, por ejemplo `Admin` y `Customer`. |
| `AspNetUserRoles` | Relación muchos-a-muchos entre usuarios y roles. |
| `AspNetUserClaims` | Claims adicionales asociados a un usuario. |
| `AspNetRoleClaims` | Claims asociados a un rol. |
| `AspNetUserLogins` | Vínculos a proveedores de inicio de sesión externos, si se agregan. |
| `AspNetUserTokens` | Tokens administrados por Identity para proveedores o funciones de Identity. No es la tabla de refresh tokens propia de la API. |
| `RefreshTokens` | Refresh tokens de la API. Guarda el hash SHA-256, vencimiento, revocación y el identificador del token que lo reemplazó; no guarda el refresh token en texto plano. |

`__EFMigrationsHistory` es una tabla técnica creada y mantenida por EF Core:
registra qué migraciones se han aplicado y la versión de EF que las aplicó. No
contiene datos de tienda.

Al iniciar la API, el código crea los roles `Admin` y `Customer` si aún no existen.
La cuenta administradora inicial solo se crea si se configuran `Admin:Email` y
`Admin:Password`.

### Catálogo

| Tabla | Propósito y datos importantes |
| --- | --- |
| `Products` | Producto base: nombre, descripción, precio base (`numeric(12,2)`), fechas de creación/actualización y `Active`. `Active=false` permite desactivarlo sin borrarlo. Puede pertenecer a un `Drop`; el vínculo es opcional. |
| `ProductVariants` | Variantes vendibles de un producto: color, talla, SKU y stock actual. Cada variante pertenece a un producto; el SKU tiene índice único. El stock vive aquí, no en `Products`. |
| `ProductImages` | URLs de imágenes, texto alternativo y orden de presentación. No almacena los archivos de imagen; solo sus URLs. |
| `Drops` | Colecciones o lanzamientos: nombre, descripción, slug, colaborador, banner, colores de tema, periodo opcional y estado activo. El slug es único. |
| `PickupPoints` | Puntos de recogida: nombre, dirección, instrucciones opcionales y estado activo. Las órdenes guardan además una copia histórica del nombre y dirección elegidos. |

`Products.Name` admite hasta 200 caracteres; `Description`, 4000. Los SKU
admiten 100 caracteres y son únicos. Las imágenes se ordenan por
`(ProductId, SortOrder)`.

Los slugs de `Drops` deben tener formato de minúsculas, números y guiones
(`^[a-z0-9-]+$`). Los colores de tema, cuando se especifican, deben tener formato
hexadecimal `#RRGGBB`. Estas reglas también están protegidas por restricciones
`CHECK` en PostgreSQL.

### Órdenes y pagos

| Tabla | Propósito y datos importantes |
| --- | --- |
| `Orders` | Encabezado del pedido: estado, fecha, total (`numeric(12,2)`), método de entrega y datos de contacto del comprador. `UserId` es opcional para permitir compras de invitados. Conserva dirección, instrucciones o datos históricos del punto de recogida. |
| `OrderItems` | Renglones del pedido: producto, variante opcional, cantidad y datos históricos del artículo (`ProductName`, `VariantDescription`, `UnitPrice`). Guardar el precio unitario aquí conserva el precio al momento de comprar, aunque luego cambie el catálogo. |
| `Payments` | Intentos o registros de pago asociados al pedido: importe, proveedor, referencia del proveedor, estado, comprobante opcional y revisión opcional por un usuario. No debe guardar número de tarjeta, CVV ni otros datos de tarjeta. |

Los estados de orden se almacenan como texto: `Pending`, `Paid`, `Shipped`,
`Delivered` o `Cancelled`. Los métodos de entrega también se guardan como
texto: `HomeDelivery`, `PickupPoint` o `WhatsAppCoordination`.

En entrega a domicilio, `Orders` guarda `DeliveryAddress` e
`DeliveryInstructions`. Para recogida, `PickupPointId` apunta al punto actual,
mientras `PickupPointNameSnapshot` y `PickupPointAddressSnapshot` preservan lo
que vio el cliente cuando hizo el pedido.

## Relaciones y reglas de borrado

```text
AspNetUsers (0..1 por orden) ── (0..* por usuario) Orders
AspNetUsers 1 ── 0..* RefreshTokens
AspNetUsers 1 ── 0..* Payments (como revisor)
AspNetRoles * ── * AspNetUsers (por AspNetUserRoles)

Drops 1 ── 0..* Products
Products 1 ── 0..* ProductVariants
Products 1 ── 0..* ProductImages
Products 1 ── 0..* OrderItems
ProductVariants 1 ── 0..* OrderItems (vínculo opcional)

PickupPoints 1 ── 0..* Orders (vínculo opcional)
Orders 1 ── 0..* OrderItems
Orders 1 ── 0..* Payments
```

- Eliminar un usuario elimina sus `RefreshTokens`; las órdenes asociadas no se
  eliminan en cascada.
- Eliminar un producto elimina sus variantes e imágenes en cascada. Un producto
  referenciado por una línea de pedido no se puede borrar físicamente.
- El vínculo de una línea de pedido con una variante puede quedar nulo si esa
  variante se elimina; la descripción y el precio históricos permanecen en la
  línea.
- Eliminar una orden elimina sus líneas y pagos en cascada.
- No se permite eliminar físicamente un `Drop` o `PickupPoint` todavía
  referenciado por un producto u orden.

## Índices e integridad

Índices relevantes definidos por las migraciones/configuración:

- Únicos: nombre normalizado de rol, nombre de usuario normalizado, SKU,
  slug de `Drops` y hash de refresh token.
- Búsqueda/relaciones: correo normalizado, `Products.Active`,
  `ProductVariants.ProductId`, `(ProductImages.ProductId, SortOrder)`,
  `(Orders.UserId, Orders.CreatedAtUtc)`, `OrderItems.OrderId`,
  `OrderItems.ProductId`, `Payments.OrderId` y
  `(RefreshTokens.UserId, RefreshTokens.ExpiresAtUtc)`.
- `Drops` tiene índice compuesto por estado y fechas; `PickupPoints` por estado
  activo.

La base impone tipos, longitudes, claves foráneas, índices únicos y las
restricciones de slug/colores de `Drops`. La validación de producto en la API
también exige precio base mayor que cero y stock no negativo. No hay
restricciones SQL `CHECK` equivalentes para precios, stock, cantidades o
totales; esas reglas deben seguir validándose en el backend, especialmente al
implementar el checkout.

## Migraciones aplicadas

### `20261105194601_InitialCreate`

Crea:

- Las siete tablas de ASP.NET Identity.
- `Products`, `ProductVariants` y `ProductImages`.
- La estructura inicial de `Orders`, `OrderItems` y `Payments`.
- `RefreshTokens`.
- Claves foráneas e índices iniciales, incluidos SKU único y hash de refresh
  token único.

En esta primera versión, una orden requería usuario registrado.

### `20261111065938_AlignDropsAndPickupPoints`

Adapta el modelo para lanzamientos, recogida y compras de invitados:

- Crea `Drops` y `PickupPoints`.
- Agrega `Products.DropId`.
- Permite que `Orders.UserId` sea nulo.
- Agrega a `Orders` datos de comprador, entrega, punto de recogida, copias
  históricas del punto y `Total`.
- Agrega `OrderItems.VariantDescription`.
- Agrega a `Payments` URL de comprobante, fecha de revisión y usuario revisor.
- Añade índices, claves foráneas y restricciones de formato para `Drops`.

El detalle exacto de cada migración está en
[`Migrations`](../backend/Infrastructure/Persistence/Migrations).

## ¿Falta alguna tabla?

Para la conexión con Supabase y las funciones ya implementadas (registro,
autenticación y CRUD de productos), no falta una tabla obligatoria. El esquema
actual también tiene estructuras para órdenes y pagos, pero eso no significa
que el flujo de checkout ya esté implementado.

En el backend actual solo hay controladores para autenticación y productos.
Todavía no hay endpoints/servicios de órdenes, pagos, lanzamientos ni puntos de
recogida. Por tanto, `Orders`, `OrderItems`, `Payments`, `Drops` y
`PickupPoints` son el modelo de persistencia preparado, no un checkout completo.

Tablas que podrían añadirse según las decisiones del negocio:

| Tabla futura | ¿Cuándo hace falta? |
| --- | --- |
| `Categories` y, si un producto puede estar en varias, `ProductCategories` | Si el catálogo necesita categorías como gorras, ropa o accesorios. No hay categoría modelada actualmente. |
| `Carts` y `CartItems` | Si el carrito debe persistir entre dispositivos/sesiones o sobrevivir al cierre del navegador. Si solo vive en el frontend durante una sesión, no es imprescindible. |
| `InventoryMovements` | Recomendable para auditar entradas, reservas, ventas y ajustes de stock. Hoy solo se guarda el stock actual en `ProductVariants`. |
| `Shipments` o `OrderFulfillments` | Si se requieren transportista, guía, tracking, despacho parcial o historial de entregas. La dirección del pedido ya se conserva en `Orders`. |
| `Addresses` | Solo si se quieren guardar varias direcciones reutilizables por cliente. No es necesaria para conservar la dirección histórica de cada orden. |
| `Coupons`/`Discounts`, `Reviews`, `Returns` o `AdminAuditLog` | Funciones adicionales; no son necesarias para el esquema actual y no deben agregarse hasta definir sus reglas. |

No agregues datos de tarjeta a ninguna tabla: el cobro debe procesarse con la
pasarela y almacenarse únicamente referencias/estados necesarios.

## Código relacionado

- Contexto y tablas: [`AppDbContext.cs`](../backend/Infrastructure/Persistence/AppDbContext.cs)
- Entidades: [`backend/Domain/Entities`](../backend/Domain/Entities)
- Mapeos, longitudes e índices:
  [`Configurations`](../backend/Infrastructure/Persistence/Configurations)
- Inicialización de migraciones y roles:
  [`DependencyInjection.cs`](../backend/Infrastructure/DependencyInjection.cs)
- Endpoints actuales:
  [`backend/Api/Controllers`](../backend/Api/Controllers)
