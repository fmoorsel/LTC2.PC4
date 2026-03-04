import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import swc from 'unplugin-swc'
import { readFileSync } from 'fs'
import { resolve } from 'path'

export default defineConfig({
  plugins: [
    vue(),
    swc.vite({
      jsc: {
        parser: {
          syntax: 'typescript',
          decorators: true,
        },
        transform: {
          decoratorMetadata: true,
          legacyDecorator: true,
        },
        target: 'es2017',
      },
    }),
  ],
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src'),
    },
  },
  server: {
    https: {
      key: readFileSync('./certs/localhost-key.pem'),
      cert: readFileSync('./certs/localhost.pem'),
    },
  },
  base: '',
})
