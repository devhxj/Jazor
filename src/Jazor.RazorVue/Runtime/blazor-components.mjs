import { defineComponent, h, onErrorCaptured, ref } from "vue";

function getSlot(slots, name) {
    return slots[name] || slots[name.charAt(0).toLowerCase() + name.slice(1)] || slots.default;
}

function invoke(callback, ...values) {
    if (typeof callback === "function") return callback(...values);
    if (callback && typeof callback.invokeAsync === "function") return callback.invokeAsync(...values);
    if (callback && typeof callback.InvokeAsync === "function") return callback.InvokeAsync(...values);
    return undefined;
}

function attributes(additionalAttributes) {
    return { ...(additionalAttributes || {}) };
}

function hasOwn(value, name) {
    return value != null && Object.prototype.hasOwnProperty.call(value, name);
}

function parseNumber(value, descriptor) {
    if (value === "" || value === null || value === undefined) {
        return descriptor?.nullable
            ? { ok: true, value: null }
            : { ok: false, error: "The value is required." };
    }
    const parsed = Number(value);
    if (!Number.isFinite(parsed)) return { ok: false, error: "The value is not a valid number." };
    if (descriptor?.integer && !Number.isInteger(parsed)) {
        return { ok: false, error: "The value must be a whole number." };
    }
    return { ok: true, value: parsed };
}

function parseBigInt(value, descriptor) {
    if (value === "" || value === null || value === undefined) {
        return descriptor?.nullable
            ? { ok: true, value: null }
            : { ok: false, error: "The value is required." };
    }
    if (!/^[+-]?\d+$/.test(String(value))) return { ok: false, error: "The value is not a valid whole number." };
    try {
        return { ok: true, value: BigInt(value) };
    } catch {
        return { ok: false, error: "The value is not a valid whole number." };
    }
}

function daysBeforeYear(year) {
    const y = year - 1;
    return 365 * y + Math.floor(y / 4) - Math.floor(y / 100) + Math.floor(y / 400);
}

function dateOnlyDayNumber(year, month, day) {
    const monthDays = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
    let days = daysBeforeYear(year);
    for (let index = 1; index < month; index++) {
        days += monthDays[index - 1];
        if (index === 2 && (year % 4 === 0 && (year % 100 !== 0 || year % 400 === 0))) days++;
    }
    return days + day - 1;
}

function parseIsoDate(value, descriptor) {
    if (value === "" || value === null || value === undefined) {
        return descriptor?.nullable
            ? { ok: true, value: null }
            : { ok: false, error: "The value is required." };
    }
    const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(String(value));
    if (!match) return { ok: false, error: "The value is not a valid date." };
    const year = Number(match[1]);
    const month = Number(match[2]);
    const day = Number(match[3]);
    const date = new Date(0);
    date.setHours(0, 0, 0, 0);
    date.setFullYear(year, month - 1, day);
    if (date.getFullYear() !== year || date.getMonth() !== month - 1 || date.getDate() !== day) {
        return { ok: false, error: "The value is not a valid date." };
    }

    // CLR date carriers are deliberately plain data objects at this boundary. Their fields
    // match RuntimeModule.JDateTime/JDateOnly, so normal mapped members keep working without
    // importing a server-only reflection or parsing service into the browser adapter.
    if (descriptor?.kind === "dateonly") {
        return {
            ok: true,
            value: { year, month, day, dayNumber: dateOnlyDayNumber(year, month, day) }
        };
    }
    if (descriptor?.kind === "datetimeoffset") {
        return { ok: true, value: { utcDateTime: new Date(date.getTime()), offsetTicks: 0n, utcSubMillisecondTicks: 0n } };
    }
    return { ok: true, value: { date, kind: 0, subMillisecondTicks: 0n } };
}

function parseEnum(value, descriptor) {
    if (value === "" || value === null || value === undefined) {
        return descriptor?.nullable
            ? { ok: true, value: null }
            : { ok: false, error: "The value is required." };
    }
    const entries = descriptor?.values || {};
    if (hasOwn(entries, value)) return { ok: true, value: entries[value] };
    const numeric = Number(value);
    if (Number.isFinite(numeric) && Object.values(entries).includes(numeric)) return { ok: true, value: numeric };
    if (/^[+-]?\d+$/.test(String(value))) {
        try {
            const bigint = BigInt(value);
            if (Object.values(entries).some(entry => entry === bigint || String(entry) === String(bigint))) {
                return { ok: true, value: bigint };
            }
        } catch {
            // Keep the standard invalid-selection diagnostic below.
        }
    }
    return { ok: false, error: "The selected value is not valid." };
}

function parseTypedValue(value, descriptor, fallback) {
    if (!descriptor) return { ok: true, value: fallback(value) };
    if (descriptor.kind === "number") return parseNumber(value, descriptor);
    if (descriptor.kind === "bigint") return parseBigInt(value, descriptor);
    if (descriptor.kind === "dateonly" || descriptor.kind === "datetime" || descriptor.kind === "datetimeoffset") {
        return parseIsoDate(value, descriptor);
    }
    if (descriptor.kind === "enum") return parseEnum(value, descriptor);
    if (descriptor.kind === "boolean") return { ok: true, value: !!value };
    return { ok: true, value: value === "" && descriptor.nullable ? null : value };
}

function formatDateValue(value) {
    if (!value) return "";
    if (value.year !== undefined && value.month !== undefined && value.day !== undefined) {
        return `${String(value.year).padStart(4, "0")}-${String(value.month).padStart(2, "0")}-${String(value.day).padStart(2, "0")}`;
    }
    const date = value.date instanceof Date ? value.date : value instanceof Date ? value : null;
    if (!date || Number.isNaN(date.getTime())) return "";
    return `${String(date.getFullYear()).padStart(4, "0")}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")}`;
}

function formatInputValue(value, descriptor) {
    if (value === null || value === undefined) return "";
    if (descriptor?.kind === "dateonly" || descriptor?.kind === "datetime" || descriptor?.kind === "datetimeoffset") {
        return formatDateValue(value);
    }
    if (descriptor?.kind === "bigint") return String(value);
    return String(value);
}

function notifyParseFailure(props, error) {
    // EditContext is optional in the browser adapter. When a host supplies the standard
    // notification seam, preserve the invalid field state instead of silently assigning a
    // string to a numeric/date CLR carrier. Unknown host shapes remain a no-op.
    const context = props?.EditContext;
    if (context && typeof context.notifyFieldChanged === "function") context.notifyFieldChanged(error);
    if (context && typeof context.NotifyValidationStateChanged === "function") context.NotifyValidationStateChanged(error);
    invoke(props?.OnParseError, error);
}

function descriptorFromProps(props, fallbackKind) {
    const descriptor = props?.__jazorValueType;
    if (!descriptor) return fallbackKind ? { kind: fallbackKind } : null;
    if (typeof descriptor === "object") return descriptor;
    try {
        return JSON.parse(String(descriptor));
    } catch {
        return fallbackKind ? { kind: fallbackKind } : null;
    }
}

/** Statically resolved DynamicComponent target. Type token resolution stays in RazorVue lowering. */
export const DynamicComponent = defineComponent({
    name: "JazorBlazorDynamicComponent",
    props: { __jazorComponent: { default: null }, Parameters: { default: null } },
    setup(props) {
        return () => {
            if (!props.__jazorComponent) return null;
            const parameters = props.Parameters || {};
            const declared = props.__jazorComponent.props;
            if (Array.isArray(declared)) {
                for (const name of Object.keys(parameters)) {
                    if (name === "ChildContent" || name.startsWith("on") || declared.includes(name)) continue;
                    throw new Error(`DynamicComponent parameter '${name}' is not declared by the target component.`);
                }
            }
            return h(props.__jazorComponent, parameters);
        };
    },
});

/** Browser ErrorBoundary preserving standard ChildContent/ErrorContent composition. */
export const ErrorBoundary = defineComponent({
    name: "JazorBlazorErrorBoundary",
    setup(props, context) {
        const error = ref(null);
        const recover = () => {
            error.value = null;
        };
        context.expose?.({
            Recover: recover,
            get CurrentException() { return error.value; }
        });
        onErrorCaptured(cause => {
            error.value = cause;
            return false;
        });
        return () => {
            if (error.value) {
                const errorContent = getSlot(context.slots, "ErrorContent");
                return errorContent ? errorContent(error.value) : null;
            }
            const childContent = getSlot(context.slots, "ChildContent");
            return childContent ? childContent() : null;
        };
    },
});

/** Native browser form bridge for EditForm's standard submit and child-content API. */
export const EditForm = defineComponent({
    name: "JazorBlazorEditForm",
    props: {
        Model: { default: null },
        EditContext: { default: null },
        OnSubmit: { default: null },
        OnValidSubmit: { default: null },
        OnInvalidSubmit: { default: null },
        AdditionalAttributes: { default: null },
    },
    setup(props, context) {
        const editContext = props.EditContext || (props.Model ? {
            Model: props.Model,
            validate() {
                if (typeof props.Model?.validate === "function") return props.Model.validate();
                if (typeof props.Model?.Validate === "function") return props.Model.Validate();
                return props.Model?.isValid ?? true;
            }
        } : null);
        const submit = async event => {
            event?.preventDefault?.();
            if (props.OnSubmit) return invoke(props.OnSubmit, editContext);
            let valid = true;
            if (editContext && typeof editContext.validate === "function") valid = await editContext.validate();
            else if (editContext && typeof editContext.Validate === "function") valid = await editContext.Validate();
            else if (editContext && typeof editContext.isValid === "boolean") valid = editContext.isValid;
            return invoke(valid ? props.OnValidSubmit : props.OnInvalidSubmit, editContext);
        };
        return () => {
            const formAttributes = attributes(props.AdditionalAttributes);
            formAttributes.onSubmit = submit;
            const childContent = getSlot(context.slots, "ChildContent");
            return h("form", formAttributes, childContent ? childContent(editContext) : null);
        };
    },
});

export const InputText = defineComponent({
    name: "JazorBlazorInputText",
    props: { Value: { default: null }, ValueChanged: { default: null }, AdditionalAttributes: { default: null } },
    setup(props) {
        return () => {
            const inputAttributes = attributes(props.AdditionalAttributes);
            inputAttributes.value = props.Value ?? "";
            inputAttributes.onChange = event => invoke(props.ValueChanged, event?.target?.value ?? "");
            return h("input", inputAttributes);
        };
    },
});

export const InputTextArea = defineComponent({
    name: "JazorBlazorInputTextArea",
    props: { Value: { default: null }, ValueChanged: { default: null }, AdditionalAttributes: { default: null } },
    setup(props) {
        return () => {
            const inputAttributes = attributes(props.AdditionalAttributes);
            inputAttributes.value = props.Value ?? "";
            inputAttributes.onChange = event => invoke(props.ValueChanged, event?.target?.value ?? "");
            return h("textarea", inputAttributes);
        };
    },
});

export const InputCheckbox = defineComponent({
    name: "JazorBlazorInputCheckbox",
    props: { Value: { default: false }, ValueChanged: { default: null }, AdditionalAttributes: { default: null } },
    setup(props) {
        return () => {
            const inputAttributes = attributes(props.AdditionalAttributes);
            inputAttributes.type = "checkbox";
            inputAttributes.checked = !!props.Value;
            inputAttributes.onChange = event => invoke(props.ValueChanged, !!event?.target?.checked);
            return h("input", inputAttributes);
        };
    },
});

export const InputNumber = defineComponent({
    name: "JazorBlazorInputNumber",
    props: { Value: { default: null }, ValueChanged: { default: null }, AdditionalAttributes: { default: null }, __jazorValueType: { default: null }, OnParseError: { default: null }, EditContext: { default: null } },
    setup(props) {
        return () => {
            const inputAttributes = attributes(props.AdditionalAttributes);
            inputAttributes.type = "number";
            inputAttributes.value = formatInputValue(props.Value, descriptorFromProps(props, "number"));
            inputAttributes.onChange = event => {
                const value = event?.target?.value;
                const parsed = parseTypedValue(value, descriptorFromProps(props, "number"), raw => raw === "" ? null : Number(raw));
                if (!parsed.ok) return notifyParseFailure(props, parsed.error);
                return invoke(props.ValueChanged, parsed.value);
            };
            return h("input", inputAttributes);
        };
    },
});

export const InputDate = defineComponent({
    name: "JazorBlazorInputDate",
    props: { Value: { default: null }, ValueChanged: { default: null }, AdditionalAttributes: { default: null }, __jazorValueType: { default: null }, OnParseError: { default: null }, EditContext: { default: null } },
    setup(props) {
        return () => {
            const inputAttributes = attributes(props.AdditionalAttributes);
            inputAttributes.type = "date";
            const descriptor = descriptorFromProps(props, "datetime");
            inputAttributes.value = formatInputValue(props.Value, descriptor);
            inputAttributes.onChange = event => {
                const parsed = parseTypedValue(event?.target?.value ?? "", descriptor, raw => raw || null);
                if (!parsed.ok) return notifyParseFailure(props, parsed.error);
                return invoke(props.ValueChanged, parsed.value);
            };
            return h("input", inputAttributes);
        };
    },
});

export const InputSelect = defineComponent({
    name: "JazorBlazorInputSelect",
    props: { Value: { default: null }, ValueChanged: { default: null }, AdditionalAttributes: { default: null }, __jazorValueType: { default: null }, OnParseError: { default: null }, EditContext: { default: null } },
    setup(props, context) {
        return () => {
            const inputAttributes = attributes(props.AdditionalAttributes);
            const descriptor = descriptorFromProps(props, null);
            inputAttributes.value = formatInputValue(props.Value, descriptor);
            inputAttributes.onChange = event => {
                const parsed = parseTypedValue(event?.target?.value ?? "", descriptor, raw => raw);
                if (!parsed.ok) return notifyParseFailure(props, parsed.error);
                return invoke(props.ValueChanged, parsed.value);
            };
            const childContent = getSlot(context.slots, "ChildContent");
            return h("select", inputAttributes, childContent ? childContent() : null);
        };
    },
});
