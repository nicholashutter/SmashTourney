import path from "path";
import { defineConfig } from 'vite';
import { configDefaults } from 'vitest/config';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import eslint from "vite-plugin-eslint";



export default defineConfig({
  plugins:
    [
      react(),
      tailwindcss(),
      eslint({ failOnWarning: false })
    ],
  build: {
    chunkSizeWarningLimit: 600,
    rollupOptions: {
      onwarn(warning, warn)
      {
        if (
          typeof warning.message === "string" &&
          warning.message.includes("contains an annotation that Rollup cannot interpret")
        )
        {
          return;
        }
        warn(warning);
      }
    }
  },
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
