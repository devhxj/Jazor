export const RENDER_CONTEXT_PROTOCOL_VERSION = 1;

const protocolLabel = `render-context v${RENDER_CONTEXT_PROTOCOL_VERSION}`;

function fail(message) {
    throw new Error(`${protocolLabel}: ${message}`);
}

function appendNormalized(target, value) {
    if (value === null || value === undefined) {
        return;
    }

    if (Array.isArray(value)) {
        for (const item of value) {
            appendNormalized(target, item);
        }
        return;
    }

    target.push(value);
}

function normalizeAttributeName(name) {
    if (typeof name !== "string") {
        fail("addAttribute requires an attribute name string");
    }

    let normalized = name;
    if (normalized.startsWith("@")) {
        normalized = normalized.slice(1);
    }

    if (/^on[a-z]/.test(normalized)) {
        return `on${normalized[2].toUpperCase()}${normalized.slice(3)}`;
    }

    return normalized;
}

function isEventAttributeName(name) {
    return typeof name === "string" && /^on[A-Z]/.test(name);
}

function normalizeComponentParameterName(name) {
    if (typeof name !== "string" || name.trim().length === 0) {
        fail("addComponentParameter requires a non-empty parameter name");
    }

    // Official Razor SG emits component parameter names as the C# public
    // property identifier (for example Title / OnChanged), while generated
    // child modules declare/read lower-camel runtime props via
    // Util.GetConfigOrSymbolName (title / onChanged).
    return `${name[0].toLowerCase()}${name.slice(1)}`;
}

function normalizeComponentParameterRuntimeName(name, parameterNameMap) {
    if (parameterNameMap !== null &&
        Object.prototype.hasOwnProperty.call(parameterNameMap, name)) {
        return parameterNameMap[name];
    }

    return normalizeComponentParameterName(name);
}

function normalizeComponentSlotName(name) {
    if (name === "ChildContent") {
        return "default";
    }

    return normalizeComponentParameterName(name);
}

function normalizeComponentSlotRuntimeName(name, parameterNameMap) {
    if (parameterNameMap !== null &&
        Object.prototype.hasOwnProperty.call(parameterNameMap, name)) {
        return parameterNameMap[name];
    }

    return normalizeComponentSlotName(name);
}

function readAttributeEntries(attributes, operation) {
    if (attributes === null || attributes === undefined) {
        return [];
    }

    if (typeof attributes !== "object" && typeof attributes !== "function") {
        fail(`${operation} requires an iterable or object attribute collection`);
    }

    if (typeof attributes[Symbol.iterator] === "function") {
        const entries = [];
        for (const entry of attributes) {
            if (Array.isArray(entry) && entry.length >= 2) {
                entries.push([entry[0], entry[1]]);
                continue;
            }

            if (entry !== null && typeof entry === "object") {
                if ("key" in entry && "value" in entry) {
                    entries.push([entry.key, entry.value]);
                    continue;
                }

                if ("Key" in entry && "Value" in entry) {
                    entries.push([entry.Key, entry.Value]);
                    continue;
                }
            }

            fail(`${operation} requires each iterable item to be a key/value pair`);
        }

        return entries;
    }

    return Object.entries(attributes);
}

function readAttributeFrame(frame) {
    if (frame === null || frame === undefined || typeof frame !== "object") {
        fail("addAttributeFrame requires a RenderTreeFrame-like object");
    }

    const name = frame.AttributeName ?? frame.attributeName ?? frame.name;
    const value = frame.AttributeValue ?? frame.attributeValue ?? frame.value;
    return [name, value];
}

function applyFrameAttribute(frame, name, value, operation) {
    if (typeof name !== "string" || name.trim().length === 0) {
        fail(`${operation} requires non-empty string attribute names`);
    }

    frame.props ??= {};
    const runtimeName = frame.kind === "element"
        ? normalizeAttributeName(name)
        : normalizeComponentParameterRuntimeName(name, frame.parameterNameMap);
    frame.props[runtimeName] = value;
    frame.lastAttributeName = runtimeName;
}

function wrapEventHandler(handler, modifiers) {
    if (typeof handler !== "function") {
        return handler;
    }

    return (event, ...args) => {
        if (modifiers.preventDefault) {
            event?.preventDefault?.();
        }
        if (modifiers.stopPropagation) {
            event?.stopPropagation?.();
        }

        return handler(event, ...args);
    };
}

function wrapDomBindHandler(handler, attributeName) {
    if (typeof handler !== "function") {
        return handler;
    }

    return (eventOrValue, ...args) => {
        const value = eventOrValue !== null &&
            eventOrValue !== undefined &&
            typeof eventOrValue === "object" &&
            eventOrValue.target !== null &&
            eventOrValue.target !== undefined &&
            attributeName in eventOrValue.target
            ? eventOrValue.target[attributeName]
            : eventOrValue;

        return handler(value, ...args);
    };
}

function applyElementDomBind(frame) {
    if (frame.updatesEventName === null || frame.props === null) {
        return;
    }

    if (!Object.prototype.hasOwnProperty.call(frame.props, frame.updatesEventName)) {
        return;
    }

    frame.props[frame.updatesEventName] = wrapDomBindHandler(
        frame.props[frame.updatesEventName],
        frame.updatesAttributeName);
}

function applyElementEventModifiers(frame) {
    if (frame.eventModifiers === null || frame.props === null) {
        return;
    }

    for (const [eventName, modifiers] of Object.entries(frame.eventModifiers)) {
        if (!Object.prototype.hasOwnProperty.call(frame.props, eventName)) {
            continue;
        }

        frame.props[eventName] = wrapEventHandler(frame.props[eventName], modifiers);
    }
}

function appendReferenceCapture(frame, action, operation) {
    if (action === null || action === undefined) {
        return;
    }
    if (typeof action !== "function") {
        fail(`${operation} requires a reference capture function or nullish value`);
    }

    frame.referenceCaptures ??= [];
    frame.referenceCaptures.push(action);
}

function validateNonEmptyString(value, operation, parameterName) {
    if (typeof value !== "string" || value.trim().length === 0) {
        fail(`${operation} requires a non-empty ${parameterName} string`);
    }
}

function applyReferenceCaptures(frame) {
    if (frame.referenceCaptures === null) {
        return;
    }

    frame.props ??= {};
    frame.props.ref = (value) => {
        for (const action of frame.referenceCaptures) {
            action(value);
        }
    };
}

function countRootNodes(html) {
    if (typeof html !== "string" || html.length === 0) {
        return 1;
    }

    // Vue createStaticVNode expects a positive root-node count. For constant
    // markup emitted by Razor SG this is usually one static fragment root.
    return 1;
}

export function createRenderContextCore(h, Fragment, createStaticVNode) {
    if (typeof h !== "function") {
        fail("createRenderContext requires h to be a function");
    }
    if (Fragment === null || Fragment === undefined) {
        fail("createRenderContext requires a Fragment value");
    }
    if (typeof createStaticVNode !== "function") {
        fail("createRenderContext requires createStaticVNode to be a function");
    }

    const roots = [];
    const frames = [];
    let finished = false;

    function assertActive(operation) {
        if (finished) {
            fail(`${operation} cannot run after finish`);
        }
    }

    function currentElementFrame(operation) {
        const frame = frames.at(-1);
        if (frame === undefined || frame.kind !== "element") {
            fail(`${operation} requires an open element`);
        }
        return frame;
    }

    function currentComponentFrame(operation) {
        const frame = frames.at(-1);
        if (frame === undefined || frame.kind !== "component") {
            fail(`${operation} requires an open component`);
        }
        return frame;
    }

    function currentPropFrame(operation) {
        const frame = frames.at(-1);
        if (frame === undefined || (frame.kind !== "element" && frame.kind !== "component")) {
            fail(`${operation} requires an open element or component`);
        }
        return frame;
    }

    function addRenderFragmentSlot(frame, name, fragment, operation, scoped = false) {
        const slotName = normalizeComponentSlotRuntimeName(name, frame.parameterNameMap);
        if (fragment !== null && fragment !== undefined && typeof fragment !== "function") {
            fail(`${operation} ${name} requires a RenderFragment function or nullish value`);
        }

        if (fragment === null || fragment === undefined) {
            return;
        }

        frame.slotFragments ??= {};
        if (Object.prototype.hasOwnProperty.call(frame.slotFragments, slotName)) {
            fail(`${operation} cannot set slot ${name} more than once for the current component`);
        }

        frame.slotFragments[slotName] = { fragment, scoped };
    }

    function appendToParent(value) {
        const parent = frames.at(-1);
        if (parent === undefined) {
            appendNormalized(roots, value);
            return;
        }

        parent.childrenStarted = true;
        appendNormalized(parent.children, value);
    }

    function describeOpenPath() {
        return frames
            .map((frame) => {
                if (frame.kind === "element") {
                    return `<${frame.name}>`;
                }
                if (frame.kind === "component") {
                    return `<component>`;
                }
                return "<region>";
            })
            .join(" > ");
    }

    const context = {
        openElement(name) {
            assertActive("openElement");
            if (typeof name !== "string" || name.trim().length === 0) {
                fail("openElement requires a non-empty element name");
            }

            const parent = frames.at(-1);
            if (parent !== undefined) {
                parent.childrenStarted = true;
            }
            frames.push({
                kind: "element",
                name,
                props: null,
                children: [],
                childrenStarted: false,
                updatesAttributeName: null,
                eventModifiers: null,
                namedEvents: null,
                referenceCaptures: null,
                lastAttributeName: null,
                updatesEventName: null
            });
            return context;
        },

        openRegion() {
            assertActive("openRegion");
            const parent = frames.at(-1);
            if (parent !== undefined) {
                parent.childrenStarted = true;
            }
            frames.push({
                kind: "region",
                children: [],
                childrenStarted: false
            });
            return context;
        },

        openComponent(componentType, parameterNameMap = null) {
            assertActive("openComponent");
            if (componentType === null || componentType === undefined) {
                fail("openComponent requires a component type");
            }

            const parent = frames.at(-1);
            if (parent !== undefined) {
                parent.childrenStarted = true;
            }
            frames.push({
                kind: "component",
                type: componentType,
                parameterNameMap,
                props: null,
                children: [],
                childrenStarted: false,
                slotFragments: null,
                updatesAttributeName: null,
                referenceCaptures: null,
                lastAttributeName: null
            });
            return context;
        },

        addAttribute(name, value) {
            assertActive("addAttribute");
            const frame = currentPropFrame("addAttribute");
            if (frame.childrenStarted) {
                const target = frame.kind === "element" ? `<${frame.name}>` : "the current component";
                fail(`addAttribute cannot run after child content has started for ${target}`);
            }

            frame.props ??= {};
            applyFrameAttribute(frame, name, value, "addAttribute");
            return context;
        },

        addEventPreventDefaultAttribute(eventName, value) {
            assertActive("addEventPreventDefaultAttribute");
            const frame = currentElementFrame("addEventPreventDefaultAttribute");
            if (frame.childrenStarted) {
                fail(`addEventPreventDefaultAttribute cannot run after child content has started for <${frame.name}>`);
            }
            if (!value) {
                return context;
            }

            const runtimeName = normalizeAttributeName(eventName);
            frame.eventModifiers ??= {};
            frame.eventModifiers[runtimeName] ??= {};
            frame.eventModifiers[runtimeName].preventDefault = true;
            return context;
        },

        addEventStopPropagationAttribute(eventName, value) {
            assertActive("addEventStopPropagationAttribute");
            const frame = currentElementFrame("addEventStopPropagationAttribute");
            if (frame.childrenStarted) {
                fail(`addEventStopPropagationAttribute cannot run after child content has started for <${frame.name}>`);
            }
            if (!value) {
                return context;
            }

            const runtimeName = normalizeAttributeName(eventName);
            frame.eventModifiers ??= {};
            frame.eventModifiers[runtimeName] ??= {};
            frame.eventModifiers[runtimeName].stopPropagation = true;
            return context;
        },

        addAttributeFrame(attributeFrame) {
            assertActive("addAttributeFrame");
            const frame = currentPropFrame("addAttributeFrame");
            if (frame.childrenStarted) {
                const target = frame.kind === "element" ? `<${frame.name}>` : "the current component";
                fail(`addAttributeFrame cannot run after child content has started for ${target}`);
            }

            const [name, value] = readAttributeFrame(attributeFrame);
            applyFrameAttribute(frame, name, value, "addAttributeFrame");
            return context;
        },

        addMultipleAttributes(attributes) {
            assertActive("addMultipleAttributes");
            const frame = currentPropFrame("addMultipleAttributes");
            if (frame.childrenStarted) {
                const target = frame.kind === "element" ? `<${frame.name}>` : "the current component";
                fail(`addMultipleAttributes cannot run after child content has started for ${target}`);
            }

            for (const [name, value] of readAttributeEntries(attributes, "addMultipleAttributes")) {
                applyFrameAttribute(frame, name, value, "addMultipleAttributes");
            }
            return context;
        },

        addComponentParameter(name, value) {
            assertActive("addComponentParameter");
            const frame = currentComponentFrame("addComponentParameter");
            if (frame.childrenStarted) {
                fail("addComponentParameter cannot run after child content has started for the current component");
            }

            // ChildContent is the Razor default-slot contract: keep it off props and
            // materialize as Vue slots.default when the component frame closes.
            if (name === "ChildContent") {
                addRenderFragmentSlot(frame, name, value, "addComponentParameter");
                return context;
            }

            frame.props ??= {};
            frame.props[normalizeComponentParameterRuntimeName(name, frame.parameterNameMap)] = value;
            return context;
        },

        addComponentSlot(name, fragment) {
            assertActive("addComponentSlot");
            const frame = currentComponentFrame("addComponentSlot");
            if (frame.childrenStarted) {
                fail("addComponentSlot cannot run after child content has started for the current component");
            }

            addRenderFragmentSlot(frame, name, fragment, "addComponentSlot");
            return context;
        },

        addComponentScopedSlot(name, fragmentFactory) {
            assertActive("addComponentScopedSlot");
            const frame = currentComponentFrame("addComponentScopedSlot");
            if (frame.childrenStarted) {
                fail("addComponentScopedSlot cannot run after child content has started for the current component");
            }

            addRenderFragmentSlot(frame, name, fragmentFactory, "addComponentScopedSlot", true);
            return context;
        },

        setKey(value) {
            assertActive("setKey");
            const frame = currentPropFrame("setKey");
            if (frame.childrenStarted) {
                const target = frame.kind === "element" ? `<${frame.name}>` : "the current component";
                fail(`setKey cannot run after child content has started for ${target}`);
            }

            frame.props ??= {};
            frame.props.key = value;
            return context;
        },

        setUpdatesAttributeName(name) {
            assertActive("setUpdatesAttributeName");
            const frame = currentPropFrame("setUpdatesAttributeName");
            if (frame.childrenStarted) {
                const target = frame.kind === "element" ? `<${frame.name}>` : "the current component";
                fail(`setUpdatesAttributeName cannot run after child content has started for ${target}`);
            }
            if (typeof name !== "string" || name.trim().length === 0) {
                fail("setUpdatesAttributeName requires a non-empty attribute name");
            }

            frame.updatesAttributeName = name;
            if (frame.kind === "element" && isEventAttributeName(frame.lastAttributeName)) {
                frame.updatesEventName = frame.lastAttributeName;
            }
            return context;
        },

        setAttributeValue(value) {
            assertActive("setAttributeValue");
            const frame = currentPropFrame("setAttributeValue");
            if (frame.childrenStarted) {
                const target = frame.kind === "element" ? `<${frame.name}>` : "the current component";
                fail(`setAttributeValue cannot run after child content has started for ${target}`);
            }
            if (frame.lastAttributeName === null) {
                fail("setAttributeValue requires a previous attribute on the current element or component");
            }

            frame.props ??= {};
            frame.props[frame.lastAttributeName] = value;
            return context;
        },

        addNamedEvent(eventType, assignedName) {
            assertActive("addNamedEvent");
            const frame = currentElementFrame("addNamedEvent");
            if (frame.childrenStarted) {
                fail(`addNamedEvent cannot run after child content has started for <${frame.name}>`);
            }

            validateNonEmptyString(eventType, "addNamedEvent", "event type");
            validateNonEmptyString(assignedName, "addNamedEvent", "assigned name");
            frame.namedEvents ??= {};
            frame.namedEvents[normalizeAttributeName(eventType)] = assignedName;
            return context;
        },

        addElementReferenceCapture(action) {
            assertActive("addElementReferenceCapture");
            const frame = currentElementFrame("addElementReferenceCapture");
            if (frame.childrenStarted) {
                fail(`addElementReferenceCapture cannot run after child content has started for <${frame.name}>`);
            }

            appendReferenceCapture(frame, action, "addElementReferenceCapture");
            return context;
        },

        addComponentReferenceCapture(action) {
            assertActive("addComponentReferenceCapture");
            const frame = currentComponentFrame("addComponentReferenceCapture");
            if (frame.childrenStarted) {
                fail("addComponentReferenceCapture cannot run after child content has started for the current component");
            }

            appendReferenceCapture(frame, action, "addComponentReferenceCapture");
            return context;
        },

        addComponentRenderMode(renderMode) {
            assertActive("addComponentRenderMode");
            const frame = currentComponentFrame("addComponentRenderMode");
            if (frame.childrenStarted) {
                fail("addComponentRenderMode cannot run after child content has started for the current component");
            }

            frame.renderMode = renderMode;
            return context;
        },

        addContent(value) {
            assertActive("addContent");
            appendToParent(value);
            return context;
        },

        addMarkupContent(html) {
            assertActive("addMarkupContent");
            if (html !== null && html !== undefined && typeof html !== "string") {
                fail("addMarkupContent requires a string or nullish markup value");
            }

            const markup = html ?? "";
            appendToParent(createStaticVNode(markup, countRootNodes(markup)));
            return context;
        },

        closeElement() {
            assertActive("closeElement");
            const frame = frames.at(-1);
            if (frame === undefined || frame.kind !== "element") {
                fail("closeElement cannot close a frame because no element is open");
            }

            frames.pop();
            applyElementDomBind(frame);
            applyElementEventModifiers(frame);
            applyReferenceCaptures(frame);
            const vnode = h(frame.name, frame.props, frame.children);
            appendToParent(vnode);
            return context;
        },

        closeRegion() {
            assertActive("closeRegion");
            const frame = frames.at(-1);
            if (frame === undefined || frame.kind !== "region") {
                fail("closeRegion cannot close a frame because no region is open");
            }

            frames.pop();
            const vnode = frame.children.length === 0
                ? null
                : frame.children.length === 1
                    ? frame.children[0]
                    : h(Fragment, null, frame.children);
            appendToParent(vnode);
            return context;
        },

        closeComponent() {
            assertActive("closeComponent");
            const frame = frames.at(-1);
            if (frame === undefined || frame.kind !== "component") {
                fail("closeComponent cannot close a frame because no component is open");
            }

            frames.pop();
            let children;
            if (frame.slotFragments !== null) {
                // RenderFragment protocol -> Vue slot functions that return VNodes.
                children = {};
                for (const [slotName, slot] of Object.entries(frame.slotFragments)) {
                    children[slotName] = (...slotArgs) => {
                        const nested = createRenderContextCore(h, Fragment, createStaticVNode);
                        if (slot.scoped) {
                            const scopedFragment = slot.fragment(...slotArgs);
                            if (scopedFragment !== null && scopedFragment !== undefined) {
                                if (typeof scopedFragment !== "function") {
                                    fail(`scoped slot ${slotName} requires RenderFragment<T> to return a RenderFragment function or nullish value`);
                                }
                                scopedFragment(nested);
                            }
                        } else {
                            slot.fragment(nested);
                        }
                        return nested.finish();
                    };
                }
            } else {
                children = frame.children.length === 0
                    ? undefined
                    : frame.children.length === 1
                        ? frame.children[0]
                        : frame.children;
            }
            applyReferenceCaptures(frame);
            const vnode = children === undefined
                ? h(frame.type, frame.props)
                : h(frame.type, frame.props, children);
            appendToParent(vnode);
            return context;
        },

        clear() {
            roots.length = 0;
            frames.length = 0;
            finished = false;
            return context;
        },

        getFrames() {
            return {
                roots: roots.slice(),
                frames: frames.map((frame) => ({ ...frame })),
                finished
            };
        },

        dispose() {
            return context;
        },

        finish() {
            assertActive("finish");
            if (frames.length !== 0) {
                const count = frames.length;
                const noun = count === 1 ? "frame" : "frames";
                fail(`finish requires ${count} open ${noun} to be closed; open path: ${describeOpenPath()}`);
            }

            const result = roots.length === 0
                ? null
                : roots.length === 1
                    ? roots[0]
                    : h(Fragment, null, roots);
            finished = true;
            return result;
        }
    };

    return context;
}
