import { listAll } from "@webref/events";
import { addToNestedMap } from "../utils/map.js";
export async function getInterfaceToEventMap() {
    const all = await listAll();
    const map = new Map();
    for (const item of Object.values(all)) {
        const { targets } = item;
        for (const target of targets) {
            addToNestedMap(map, target.target, item.type, item.interface);
            for (const path of target.bubblingPath ?? []) {
                addToNestedMap(map, path, item.type, item.interface);
            }
        }
    }
    return map;
}
//# sourceMappingURL=events.js.map