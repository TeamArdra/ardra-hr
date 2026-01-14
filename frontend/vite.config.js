import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react({
      babel: {
        plugins: [['babel-plugin-react-compiler']],
      },
    }),
    tailwindcss(),
  ],
  server: {
    port: 3000,      // Specify your desired port
    strictPort: true // Optional: if true, Vite will exit if the port is already in use
    ,
    proxy: {
      // Proxy API requests to the FastAPI backend during development
      '/api': 'http://localhost:5285'
    }
  }
})
