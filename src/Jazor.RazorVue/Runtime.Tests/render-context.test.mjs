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

test("event modifiers wrap matching DOM event handlers", () => {
    const { createContext } = createHarness();
    const received = [];
    const context = createContext();

    context.openElement("form");
    context.addAttribute("onsubmit", (event, value) => {
        received.push([event.kind, value]);
        return "handled";
    });
    context.addEventPreventDefaultAttribute("onsubmit", true);
    context.addEventStopPropagationAttribute("@onsubmit", true);
    context.closeElement();

    const calls = [];
    const event = {
        kind: "submit",
        preventDefault: () => calls.push("prevent"),
        stopPropagation: () => calls.push("stop")
    };
    const result = context.finish();

    assert.equal(result.props.onSubmit(event, 42), "handled");
    assert.deepEqual(calls, ["prevent", "stop"]);
    assert.deepEqual(received, [["submit", 42]]);
});

test("false event modifiers do not wrap handlers", () => {
    const { createContext } = createHarness();
    const handler = () => "handled";
    const context = createContext();

    context.openElement("button");
    context.addAttribute("onclick", handler);
    context.addEventPreventDefaultAttribute("onclick", false);
    context.addEventStopPropagationAttribute("onclick", false);
    context.closeElement();

    const result = context.finish();
    assert.equal(result.props.onClick, handler);
});

test("multiple attributes normalize DOM events and component parameter names", () => {
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

test("single attributes apply to components with component parameter naming", () => {
    const { createContext } = createHarness();
    const Child = { name: "Child" };
    const context = createContext();

    context.openComponent(Child);
    assert.equal(context.addAttribute("Title", "Hello"), context);
    assert.equal(context.addAttribute("onReady", "handler"), context);
    context.closeComponent();

    assert.deepEqual(context.finish().props, {
        title: "Hello",
        onReady: "handler"
    });
});

test("attribute frame replay applies frame attribute names and values", () => {
    const { createContext } = createHarness();
    const context = createContext();

    context.openElement("input");
    assert.equal(context.addAttributeFrame({ AttributeName: "onchange", AttributeValue: "handler" }), context);
    assert.equal(context.addAttributeFrame({ attributeName: "class", attributeValue: "field" }), context);
    context.closeElement();

    assert.deepEqual(context.finish().props, {
        onChange: "handler",
        class: "field"
    });

    const Child = { name: "Child" };
    const component = createContext();
    component.openComponent(Child);
    assert.equal(component.addAttributeFrame({ AttributeName: "Title", AttributeValue: "Hello" }), component);
    component.closeComponent();

    assert.deepEqual(component.finish().props, {
        title: "Hello"
    });
});

test("setAttributeValue updates the current frame's most recent attribute", () => {
    const { createContext } = createHarness();
    const element = createContext();

    element.openElement("input");
    assert.equal(element.addAttribute("value", "before"), element);
    assert.equal(element.setAttributeValue("after"), element);
    element.closeElement();

    assert.deepEqual(element.finish().props, { value: "after" });

    const Child = { name: "Child" };
    const component = createContext();
    component.openComponent(Child);
    component.addMultipleAttributes({ Title: "before" });
    assert.equal(component.setAttributeValue("after"), component);
    component.closeComponent();

    assert.deepEqual(component.finish().props, { title: "after" });
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

test("named events validate enclosing element metadata without changing VNode props", () => {
    const { createContext } = createHarness();
    const context = createContext();

    context.openElement("form");
    assert.equal(context.addNamedEvent("onsubmit", "checkout"), context);
    context.closeElement();

    assert.deepEqual(context.finish(), {
        name: "form",
        props: null,
        children: []
    });
});

test("element reference capture materializes as Vue ref callback", () => {
    const { createContext } = createHarness();
    const captured = [];
    const context = createContext();

    context.openElement("input");
    assert.equal(context.addElementReferenceCapture((value) => captured.push(value)), context);
    context.closeElement();

    const result = context.finish();
    assert.equal(typeof result.props.ref, "function");

    const input = { tagName: "INPUT" };
    result.props.ref(input);
    assert.deepEqual(captured, [input]);
});

test("component reference capture materializes as Vue ref callback", () => {
    const { createContext } = createHarness();
    const captured = [];
    const Child = { name: "Child" };
    const context = createContext();

    context.openComponent(Child);
    assert.equal(context.addComponentReferenceCapture((value) => captured.push(value)), context);
    context.closeComponent();

    const result = context.finish();
    assert.equal(result.name, Child);
    assert.equal(typeof result.props.ref, "function");

    const instance = { id: "child" };
    result.props.ref(instance);
    assert.deepEqual(captured, [instance]);
});

test("component render mode is accepted as current component metadata", () => {
    const { createContext } = createHarness();
    const Child = { name: "Child" };
    const mode = { name: "interactive" };
    const context = createContext();

    context.openComponent(Child);
    assert.equal(context.addComponentRenderMode(mode), context);
    context.addComponentParameter("Title", "Hello");
    context.closeComponent();

    assert.deepEqual(context.finish().props, { title: "Hello" });
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

test("component attributes apply descriptor parameter name map when provided", () => {
    const { createContext } = createHarness();
    const Child = { name: "Child" };
    const context = createContext();

    context.openComponent(Child, {
        Value: "modelValue",
        ValueChanged: "onUpdate:modelValue"
    });
    context.addAttribute("Value", "before");
    context.setAttributeValue("ready");
    context.addAttributeFrame({ AttributeName: "ValueChanged", AttributeValue: "handler" });
    context.addMultipleAttributes({ Title: "Fallback" });
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

test("RenderFragment<T> component slot materializes as Vue scoped slot", () => {
    const { createContext } = createHarness();
    const Child = { name: "Child" };
    const context = createContext();
    const fragmentFactory = (value) => (builder) => {
        builder.openElement("h1");
        builder.addContent(value);
        builder.closeElement();
    };

    context.openComponent(Child);
    context.addComponentParameter("Title", "Hello");
    context.addComponentScopedSlot("Header", fragmentFactory);
    context.closeComponent();

    const result = context.finish();
    assert.equal(result.name, Child);
    assert.deepEqual(result.props, { title: "Hello" });
    assert.equal(typeof result.children.header, "function");
    assert.deepEqual(result.children.header("Scoped header"), {
        name: "h1",
        props: null,
        children: ["Scoped header"]
    });
});

test("component slot names use descriptor name map", () => {
    const { createContext } = createHarness();
    const Child = { name: "Child" };
    const context = createContext();
    const fragmentFactory = (value) => (builder) => {
        builder.openElement("span");
        builder.addContent(value);
        builder.closeElement();
    };

    context.openComponent(Child, { TitleContent: "title" });
    context.addComponentScopedSlot("TitleContent", fragmentFactory);
    context.closeComponent();

    const result = context.finish();
    assert.equal(typeof result.children.title, "function");
    assert.equal(result.children.titleContent, undefined);
    assert.deepEqual(result.children.title("Mapped title"), {
        name: "span",
        props: null,
        children: ["Mapped title"]
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

test("getFrames returns a snapshot and clear resets the context", () => {
    const { createContext } = createHarness();
    const context = createContext();

    context.openElement("div");
    context.addContent("before");

    const snapshot = context.getFrames();
    assert.equal(snapshot.frames.length, 1);
    assert.equal(snapshot.frames[0].name, "div");
    snapshot.frames.length = 0;
    assert.equal(context.getFrames().frames.length, 1);

    assert.equal(context.clear(), context);
    context.addContent("after");
    assert.equal(context.finish(), "after");
    assert.equal(context.clear(), context);
    assert.equal(context.finish(), null);
});

test("dispose is a no-op protocol method", () => {
    const { createContext } = createHarness();
    const context = createContext();

    assert.equal(context.dispose(), context);
    context.addContent("ready");
    assert.equal(context.finish(), "ready");
    assert.equal(context.dispose(), context);
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
        /render-context v1: addAttribute requires an open element or component/
    );
    assert.throws(
        () => createContext().addComponentParameter("Title", "x"),
        /render-context v1: addComponentParameter requires an open component/
    );
    assert.throws(
        () => createContext().setKey("orphan"),
        /render-context v1: setKey requires an open element or component/
    );
    assert.throws(
        () => createContext().setAttributeValue("orphan"),
        /render-context v1: setAttributeValue requires an open element or component/
    );

    const noAttribute = createContext();
    noAttribute.openElement("input");
    assert.throws(
        () => noAttribute.setAttributeValue("orphan"),
        /render-context v1: setAttributeValue requires a previous attribute/
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

test("element metadata operations reject calls after child content starts", () => {
    const { createContext } = createHarness();

    const assertLateElementOperation = (operation, pattern) => {
        const context = createContext();
        context.openElement("button");
        context.addContent("child");
        assert.throws(operation(context), pattern);
    };

    assertLateElementOperation(
        (context) => () => context.addAttributeFrame({ AttributeName: "id", AttributeValue: "late" }),
        /render-context v1: addAttributeFrame cannot run after child content has started for <button>/
    );
    assertLateElementOperation(
        (context) => () => context.addMultipleAttributes({ id: "late" }),
        /render-context v1: addMultipleAttributes cannot run after child content has started for <button>/
    );
    assertLateElementOperation(
        (context) => () => context.setAttributeValue("late"),
        /render-context v1: setAttributeValue cannot run after child content has started for <button>/
    );
    assertLateElementOperation(
        (context) => () => context.addEventPreventDefaultAttribute("onclick", true),
        /render-context v1: addEventPreventDefaultAttribute cannot run after child content has started for <button>/
    );
    assertLateElementOperation(
        (context) => () => context.addEventStopPropagationAttribute("onclick", true),
        /render-context v1: addEventStopPropagationAttribute cannot run after child content has started for <button>/
    );
    assertLateElementOperation(
        (context) => () => context.addNamedEvent("onclick", "clicked"),
        /render-context v1: addNamedEvent cannot run after child content has started for <button>/
    );
    assertLateElementOperation(
        (context) => () => context.addElementReferenceCapture(() => {}),
        /render-context v1: addElementReferenceCapture cannot run after child content has started for <button>/
    );
});

test("component metadata operations reject calls after child content starts", () => {
    const { createContext } = createHarness();
    const Child = { name: "Child" };

    const assertLateComponentOperation = (operation, pattern) => {
        const context = createContext();
        context.openComponent(Child);
        context.addContent("child");
        assert.throws(operation(context), pattern);
    };

    assertLateComponentOperation(
        (context) => () => context.addAttribute("Title", "late"),
        /render-context v1: addAttribute cannot run after child content has started for the current component/
    );
    assertLateComponentOperation(
        (context) => () => context.addAttributeFrame({ AttributeName: "Title", AttributeValue: "late" }),
        /render-context v1: addAttributeFrame cannot run after child content has started for the current component/
    );
    assertLateComponentOperation(
        (context) => () => context.addMultipleAttributes({ Title: "late" }),
        /render-context v1: addMultipleAttributes cannot run after child content has started for the current component/
    );
    assertLateComponentOperation(
        (context) => () => context.addComponentParameter("Title", "late"),
        /render-context v1: addComponentParameter cannot run after child content has started for the current component/
    );
    assertLateComponentOperation(
        (context) => () => context.addComponentSlot("Header", () => {}),
        /render-context v1: addComponentSlot cannot run after child content has started for the current component/
    );
    assertLateComponentOperation(
        (context) => () => context.addComponentScopedSlot("Header", () => () => {}),
        /render-context v1: addComponentScopedSlot cannot run after child content has started for the current component/
    );
    assertLateComponentOperation(
        (context) => () => context.setKey("late"),
        /render-context v1: setKey cannot run after child content has started for the current component/
    );
    assertLateComponentOperation(
        (context) => () => context.setAttributeValue("late"),
        /render-context v1: setAttributeValue cannot run after child content has started for the current component/
    );
    assertLateComponentOperation(
        (context) => () => context.addComponentReferenceCapture(() => {}),
        /render-context v1: addComponentReferenceCapture cannot run after child content has started for the current component/
    );
    assertLateComponentOperation(
        (context) => () => context.addComponentRenderMode({}),
        /render-context v1: addComponentRenderMode cannot run after child content has started for the current component/
    );
});

test("metadata inputs reject invalid protocol values", () => {
    const { createContext } = createHarness();

    const element = createContext();
    element.openElement("button");
    assert.throws(
        () => element.addAttributeFrame(null),
        /render-context v1: addAttributeFrame requires a RenderTreeFrame-like object/
    );
    assert.throws(
        () => element.addAttributeFrame({ AttributeName: "", AttributeValue: "x" }),
        /render-context v1: addAttributeFrame requires non-empty string attribute names/
    );
    assert.throws(
        () => element.addMultipleAttributes(42),
        /render-context v1: addMultipleAttributes requires an iterable or object attribute collection/
    );
    assert.throws(
        () => element.addMultipleAttributes([["id"]]),
        /render-context v1: addMultipleAttributes requires each iterable item to be a key\/value pair/
    );
    assert.throws(
        () => element.addNamedEvent("", "clicked"),
        /render-context v1: addNamedEvent requires a non-empty event type string/
    );
    assert.throws(
        () => element.addNamedEvent("onclick", ""),
        /render-context v1: addNamedEvent requires a non-empty assigned name string/
    );
    assert.throws(
        () => element.addElementReferenceCapture("not a function"),
        /render-context v1: addElementReferenceCapture requires a reference capture function or nullish value/
    );

    const component = createContext();
    component.openComponent({ name: "Child" });
    assert.throws(
        () => component.addComponentParameter("", "x"),
        /render-context v1: addComponentParameter requires a non-empty parameter name/
    );
    assert.throws(
        () => component.addComponentSlot("Header", "not a function"),
        /render-context v1: addComponentSlot Header requires a RenderFragment function or nullish value/
    );
    assert.throws(
        () => component.addComponentScopedSlot("Header", "not a function"),
        /render-context v1: addComponentScopedSlot Header requires a RenderFragment function or nullish value/
    );
    assert.throws(
        () => component.addComponentReferenceCapture("not a function"),
        /render-context v1: addComponentReferenceCapture requires a reference capture function or nullish value/
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
