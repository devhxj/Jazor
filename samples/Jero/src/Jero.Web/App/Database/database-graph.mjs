import RelationGraph from "relation-graph/vue3"
import { createApp, h, nextTick } from "vue"


const options = {
    defaultJunctionPoint: "border",
    allowShowMiniToolBar: false,
    allowShowZoomMenu: false,
    allowShowRefreshButton: false,
    allowShowDownloadButton: false,
    allowShowFullscreenMenu: false,
    moveToCenterWhenRefresh: false,
    zoomToFitWhenRefresh: false,
    useAnimationWhenRefresh: false,
    disableNodeClickEffect: true,
    disableLineClickEffect: true,
    minCanvasZoom: 10,
    backgroundColor: "#f7f9fc",
}

export function createDatabaseGraph() {
    let app = null
    let component = null
    let previousNodes
    let revision = 0

    async function update(nodes, relations) {
        // A nested mount can run inside the parent app's Vue flush. The graph
        // creates its core in onMounted, after Vue assigns the component ref.
        await nextTick()
        const instance = component?.getInstance()
        if (!instance || nodes === previousNodes) return
        previousNodes = nodes
        const current = ++revision
        instance.stopAutoLayout()
        // Add lines after layout so stale requests cannot target destroyed node anchors.
        await instance.setJsonData({ nodes: nodes.map(node => ({ ...node, nodeShape: 1 })), lines: [] }, true)
        if (current !== revision || nodes.length === 0) return

        instance.addElementLines(relations.map(line => ({
            ...line,
            text: "",
            fromJunctionPoint: "left",
            toJunctionPoint: "lr",
            lineShape: 6,
            lineWidth: 3,
        })))
        instance.moveToCenter()
        instance.zoomToFit()
    }

    return {
        mount(nodes, relations) {
            const host = document.getElementById("database-relation-graph")
            if (!host) return
            // The wrapper owns the component ref and exposes the same public handle as a template ref.
            app = createApp({
                render: () => h(RelationGraph, {
                    options,
                    ref: value => { component = value },
                }),
            })
            app.mount(host)
            return update(nodes, relations)
        },
        update,
        dispose() {
            ++revision
            const instance = component?.getInstance()
            instance?.stopAutoLayout()
            instance?.clearGraph()
            app?.unmount()
            component = null
            app = null
            previousNodes = undefined
        },
        zoom(offset) {
            const instance = component?.getInstance()
            if (instance && instance.options.canvasZoom !== undefined)
                instance.setZoom(instance.options.canvasZoom + offset)
        },
        center() { component?.getInstance().moveToCenter() },
        fit() {
            component?.onGraphResize()
            component?.getInstance().zoomToFit()
        },
        reset() {
            const instance = component?.getInstance()
            instance?.setZoom(100)
            instance?.moveToCenter()
        },
    }
}
