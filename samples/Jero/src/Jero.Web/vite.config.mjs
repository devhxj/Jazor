import { defineConfig } from 'vite'

export default defineConfig({
    base: './',
    resolve: {
        dedupe: ['vue'],
    },
    build: { target: 'esnext' },
})
