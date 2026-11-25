export function LoadDocx() {
    return Promise.all([
        import('@vue-office/docx/lib/v3/vue-office-docx.mjs'),
        import('@vue-office/docx/lib/v3/index.css'),
    ]).then(([renderer]) => renderer.default)
}

export function LoadExcel() {
    return Promise.all([
        import('@vue-office/excel/lib/v3/vue-office-excel.mjs'),
        import('@vue-office/excel/lib/v3/index.css'),
    ]).then(([renderer]) => renderer.default)
}
