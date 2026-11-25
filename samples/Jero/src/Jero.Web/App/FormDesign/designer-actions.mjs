import { createApp } from "vue"
import ElementPlus from "element-plus"

export async function loadDesigner() {
    const { VFormDesigner, default: plugin } = await import("./runtime/designer.mjs")
    return {
        mount() {
            const host = document.getElementById("jero-vform-designer")
            const app = createApp(VFormDesigner)
            app.use(ElementPlus)
            app.use(plugin)
            const previousResize = window.onresize
            let designerResize
            try {
                app.mount(host)
            } finally {
                designerResize = window.onresize
            }
            return () => {
                app.unmount()
                // VForm leaves its global resize hook installed after unmount.
                if (window.onresize === designerResize) window.onresize = previousResize
            }
        },
    }
}
