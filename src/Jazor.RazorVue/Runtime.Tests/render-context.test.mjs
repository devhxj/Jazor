import assert from "node:assert/strict";
import test from "node:test";

import {
    RENDER_CONTEXT_PROTOCOL_VERSION,
    createRenderContextCore
} from "../Runtime/render-context-core.mjs";

const Fragment = Symbol("Fragment");

function createHarness() {
    const calls = [];
    const staticCalls = [];
    const h = (name, props, children) => {
        const vnode = { name, props, children };
        calls.push(vnode);
        return vnode;
    };
    const createStaticVNode = (html, rootCount) => {
        const vnode = { kind: "static", html, rootCount };
        staticCalls.push(vnode);
        return vnode;
    };

    return {
        calls,
        staticCalls,
        createContext: () => createRenderContextCore(h, Fragment, createStaticVNode)
    };
}

test("render-context exposes protocol version 1", () => {
    assert.equal(RENDER_CONTEXT_PROTOCOL_VERSION, 1);
});

test("finish normalizes empty, single, and multiple roots", () => {
    const { createContext } = createHarness();
    assert.equal(createContext().finish(), null);

    const single = createContext();
    single.addContent("only");
    assert.equal(single.finish(), "only");

    const multiple = createContext();
    multiple.addContent("first");
    multiple.addContent("second");
    assert.deepEqual(multiple.finish(), {
        name: Fragment,
        props: null,
        children: ["first", "second"]
    });
});

test("elements preserve attribute and content order", () => {
    const { calls, createContext } = createHarness();
    const context = createContext();

    assert.equal(context.openElement("button"), context);
    assert.equal(context.addAttribute("type", "button"), context);
    assert.equal(context.addAttribute("disabled", false), context);
    assert.equal(context.addContent("Count: "), context);
    assert.equal(context.addContent(0), context);
    assert.equal(context.closeElement(), context);

    assert.deepEqual(context.finish(), {
        name: "button",
        props: { type: "button", disabled: false },
        children: ["Count: ", 0]
    });
    assert.deepEqual(calls.map(({ name }) => name), ["button"]);
});

test("event attributes are normalized to Vue handler props", () => {
    const { createContext } = createHarness();
    const click = () => {};
    const change = () => {};
    const context = createContext();

    context.openElement("button");
    context.addAttribute("onclick", click);
    context.addAttribute("@onchange", change);
    context.closeElement();

    const result = context.finish();
    assert.deepEqual(result.props, {
        onClick: click,
        onChange: change
    });
    assert.equal(result.props.onclick, undefined);
    assert.equal(result.props["@onchange"], undefined);
});

test("multiple attributes normalize DOM events but preserve component parameter names", () => {
    const { createContext } = createHarness();
    const click = () => {};
    const change = () => {};
    const confirm = () => {};

    const element = createContext();
    element.openElement("button");
    assert.equal(element.addMultipleAttributes([
        ["onclick", click],
        { key: "@onchange", value: change },
        { Key: "class", Value: "primary" }
    ]), element);
    element.closeElement();

    assert.deepEqual(element.finish().props, {
        onClick: click,
        onChange: change,
        class: "primary"
    });

    const Child = { name: "Child" };
    const component = createContext();
    component.openComponent(Child);
    assert.equal(component.addMultipleAttributes({
        onclick: click,
        onConfirm: confirm
    }), component);
    component.closeComponent();

    assert.deepEqual(component.finish().props, {
        onclick: click,
        onConfirm: confirm
    });
});

test("setKey writes the Vue key prop on the current element", () => {
    const { createContext } = createHarness();
    const context = createContext();

    context.openElement("li");
    assert.equal(context.setKey(42), context);
    context.addContent("Item 42");
    context.closeElement();

    const result = context.finish();
    assert.deepEqual(result.props, { key: 42 });
    assert.deepEqual(result.children, ["Item 42"]);
});

test("setUpdatesAttributeName validates DOM bind hint without changing VNode props", () => {
    const { createContext } = createHarness();
    const change = () => {};
    const context = createContext();

    context.openElement("input");
    assert.equal(context.addAttribute("value", "ready"), context);
    assert.equal(context.addAttribute("onchange", change), context);
    assert.equal(context.setUpdatesAttributeName("value"), context);
    context.closeElement();

    assert.deepEqual(context.finish(), {
        name: "input",
        props: {
            value: "ready",
            onChange: change
        },
        children: []
    });
});

test("regions flatten children without introducing a DOM element", () => {
    const { createContext } = createHarness();
    const context = createContext();

    context.openRegion();
    context.addContent("first");
    context.addContent("second");
    context.closeRegion();

    assert.deepEqual(context.finish(), {
        name: Fragment,
        props: null,
        children: ["first", "second"]
    });
});

test("constant markup uses createStaticVNode", () => {
    const { staticCalls, createContext } = createHarness();
    const context = createContext();

    assert.equal(context.addMarkupContent("<strong>raw</strong>"), context);
    const result = context.finish();

    assert.deepEqual(result, {
        kind: "static",
        html: "<strong>raw</strong>",
        rootCount: 1
    });
    assert.deepEqual(staticCalls, [result]);
});

test("openComponent lowers to h(component, props) with component parameters", () => {
    const { calls, createContext } = createHarness();
    const Child = { name: "Child" };
    const context = createContext();

    assert.equal(context.openComponent(Child), context);
    assert.equal(context.addComponentParameter("Title", "Hello"), context);
    assert.equal(context.addComponentParameter("OnValueChanged", "handler"), context);
    assert.equal(context.setKey("child-1"), context);
    assert.equal(context.closeComponent(), context);

    const result = context.finish();
    assert.deepEqual(result, {
        name: Child,
        props: { title: "Hello", onValueChanged: "handler", key: "child-1" },
        children: undefined
    });
    assert.deepEqual(calls, [result]);
});

test("openComponent applies descriptor parameter name map when provided", () => {
    const { createContext } = createHarness();
    const Child = { name: "Child" };
    const context = createContext();

    context.openComponent(Child, {
        Value: "modelValue",
        ValueChanged: "onUpdate:modelValue"
    });
    context.addComponentParameter("Value", "ready");
    context.addComponentParameter("ValueChanged", "handler");
    context.addComponentParameter("Title", "Fallback");
    context.closeComponent();

    const result = context.finish();
    assert.deepEqual(result.props, {
        modelValue: "ready",
        "onUpdate:modelValue": "handler",
        title: "Fallback"
    });
});

test("ChildContent parameter materializes as Vue default slot", () => {
    const { createContext } = createHarness();
    const Child = { name: "Child" };
    const context = createContext();
    const fragment = (builder) => {
        builder.openElement("span");
        builder.addContent("slot body");
        builder.closeElement();
    };

    context.openComponent(Child);
    context.addComponentParameter("Title", "Hello");
    context.addComponentParameter("ChildContent", fragment);
    context.closeComponent();

    const result = context.finish();
    assert.equal(result.name, Child);
    assert.deepEqual(result.props, { title: "Hello" });
    assert.equal(typeof result.children.default, "function");
    assert.deepEqual(result.children.default(), {
        name: "span",
        props: null,
        children: ["slot body"]
    });
});

test("named RenderFragment component slot materializes as Vue named slot", () => {
    const { createContext } = createHarness();
    const Child = { name: "Child" };
    const context = createContext();
    const fragment = (builder) => {
        builder.openElement("h1");
        builder.addContent("header body");
        builder.closeElement();
    };

    context.openComponent(Child);
    context.addComponentParameter("Title", "Hello");
    context.addComponentSlot("Header", fragment);
    context.closeComponent();

    const result = context.finish();
    assert.equal(result.name, Child);
    assert.deepEqual(result.props, { title: "Hello" });
    assert.equal(typeof result.children.header, "function");
    assert.deepEqual(result.children.header(), {
        name: "h1",
        props: null,
        children: ["header body"]
    });
});

test("nested elements become children of the current frame", () => {
    const { calls, createContext } = createHarness();
    const context = createContext();

    context.openElement("section");
    context.openElement("span");
    context.addContent("nested");
    context.closeElement();
    context.closeElement();

    const result = context.finish();
    assert.equal(result.name, "section");
    assert.deepEqual(result.children, [calls[0]]);
    assert.deepEqual(calls.map(({ name }) => name), ["span", "section"]);
});

test("content recursively flattens arrays, ignores nullish values, and preserves false", () => {
    const { createContext } = createHarness();
    const context = createContext();

    context.addContent([null, "a", [undefined, false, [0]], "b"]);

    assert.deepEqual(context.finish(), {
        name: Fragment,
        props: null,
        children: ["a", false, 0, "b"]
    });
});

test("frame imbalance and invalid operations fail with protocol context", () => {
    const { createContext } = createHarness();

    assert.throws(
        () => createContext().closeElement(),
        /render-context v1: closeElement cannot close a frame because no element is open/
    );
    assert.throws(
        () => createContext().closeRegion(),
        /render-context v1: closeRegion cannot close a frame because no region is open/
    );
    assert.throws(
        () => createContext().closeComponent(),
        /render-context v1: closeComponent cannot close a frame because no component is open/
    );
    assert.throws(
        () => createContext().addAttribute("id", "root"),
        /render-context v1: addAttribute requires an open element/
    );
    assert.throws(
        () => createContext().addComponentParameter("Title", "x"),
        /render-context v1: addComponentParameter requires an open component/
    );
    assert.throws(
        () => createContext().setKey("orphan"),
        /render-context v1: setKey requires an open element or component/
    );

    const unclosed = createContext();
    unclosed.openElement("main");
    assert.throws(
        () => unclosed.finish(),
        /render-context v1: finish requires 1 open frame to be closed.*main/
    );

    const unclosedRegion = createContext();
    unclosedRegion.openRegion();
    assert.throws(
        () => unclosedRegion.finish(),
        /render-context v1: finish requires 1 open frame to be closed.*region/
    );
});

test("attributes cannot be added after child content and a finished context is sealed", () => {
    const { createContext } = createHarness();
    const context = createContext();

    context.openElement("div");
    context.addContent("child");
    assert.throws(
        () => context.addAttribute("id", "late"),
        /render-context v1: addAttribute cannot run after child content has started for <div>/
    );
    assert.throws(
        () => context.setKey("late"),
        /render-context v1: setKey cannot run after child content has started for <div>/
    );
    context.closeElement();
    context.finish();

    assert.throws(
        () => context.addContent("late"),
        /render-context v1: addContent cannot run after finish/
    );
    assert.throws(
        () => context.finish(),
        /render-context v1: finish cannot run after finish/
    );
});

test("factory and element inputs reject invalid protocol calls", () => {
    assert.throws(
        () => createRenderContextCore(null, Fragment, () => ({})),
        /render-context v1: createRenderContext requires h to be a function/
    );
    assert.throws(
        () => createRenderContextCore(() => ({}), null, () => ({})),
        /render-context v1: createRenderContext requires a Fragment value/
    );
    assert.throws(
        () => createRenderContextCore(() => ({}), Fragment, null),
        /render-context v1: createRenderContext requires createStaticVNode to be a function/
    );

    const { createContext } = createHarness();
    assert.throws(
        () => createContext().openElement(""),
        /render-context v1: openElement requires a non-empty element name/
    );
    assert.throws(
        () => createContext().openElement(null),
        /render-context v1: openElement requires a non-empty element name/
    );
});
