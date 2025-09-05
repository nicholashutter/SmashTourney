import path from "path"; 
import { defineConfig } from 'vite';
import { configDefaults } from 'vitest/config';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';



export default defineConfig({
  plugins:
    [
      react(),
      tailwindcss(),
    ],
    resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  test:
  {
    include: ['tests/**/*.test.ts'],
    exclude: [...configDefaults.exclude],
  },
})
