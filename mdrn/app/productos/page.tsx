import { prisma } from "@/lib/prisma";

export default async function ProductosPage() {
  const productos = await prisma.producto.findMany({
    include: { categoria: true },
  });

  return (
    <main className="p-8">
      <h1 className="text-2xl font-bold mb-6">Nuestros Productos</h1>
      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6">
        {productos.map((producto) => (
          <div key={producto.id} className="border rounded-lg p-4">
            <p className="text-sm text-gray-500">{producto.categoria.nombre}</p>
            <h2 className="text-lg font-semibold">{producto.nombre}</h2>
            <p className="text-gray-600">{producto.descripcion}</p>
            <p className="mt-2 font-bold">₡{producto.precio.toString()}</p>
            <p className="text-sm text-gray-500">Stock: {producto.stock}</p>
          </div>
        ))}
      </div>
    </main>
  );
}