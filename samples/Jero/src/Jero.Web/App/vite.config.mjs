import { fileURLToPath } from 'node:url'
import { defineConfig } from 'vite'

export default defineConfig({
    base: './',
    resolve: {
        alias: {
            '@jero/runtime': fileURLToPath(new URL('./.jazor/modules/', import.meta.url)),
        },
        dedupe: ['vue'],
    },
    build: { target: 'esnext' },
})
