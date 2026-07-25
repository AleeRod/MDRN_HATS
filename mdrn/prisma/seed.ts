import { PrismaClient } from "../app/generated/prisma/client";
import { PrismaPg } from "@prisma/adapter-pg";

const adapter = new PrismaPg({ connectionString: process.env.DATABASE_URL });
const prisma = new PrismaClient({ adapter });

async function main() {
  const electronica = await prisma.categoria.create({
    data: { nombre: "Electronica" },
  });
  const ropa = await prisma.categoria.create({
    data: { nombre: "Ropa" },
  });

  await prisma.producto.createMany({
    data: [
      {
        nombre: "Audifonos Bluetooth",
        descripcion: "Audifonos inalambricos",
        precio: 45000,
        stock: 20,
        categoriaId: electronica.id,
      },
      {
        nombre: "Teclado",
        descripcion: "Teclado para PC",
        precio: 20000,
        stock: 10,
        categoriaId: electronica.id,
      },
      {
        nombre: "Camiseta",
        descripcion: "Camisa Basica S",
        precio: 3500,
        stock: 100,
        categoriaId: ropa.id,
      },
    ],
  });

  console.log("Seed completado con exito");
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });