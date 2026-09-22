import { inject, onUnmounted, provide, reactive } from "vue";

/**
 * Host-owned authentication snapshot shared by SSR and hydration.
 * The state object is deliberately closed: endpoint adapters may publish only the
 * four statuses understood by JazorAuthenticationStatus.
 */
export const authenticationProviderKey = "jazor:auth-state";
export const authenticationEnvelopeSchema = "jazor-auth-state";
export const authenticationEnvelopeVersion = 1;
export const authenticationStatuses = Object.freeze([
    "Anonymous",
    "Authenticated",
    "Expired",
    "Forbidden",
]);

function validateSnapshot(value) {
    if (!value || typeof value !== "object") {
        throw new Error("Jazor authentication state must be an object.");
    }
    if (!authenticationStatuses.includes(value.status)) {
        throw new Error(`Jazor authentication state status '${String(value.status)}' is not supported.`);
    }
    if (value.subject !== undefined && value.subject !== null && typeof value.subject !== "string") {
        throw new Error("Jazor authentication state subject must be a string or null.");
    }
    if (value.claims !== undefined && value.claims !== null) {
        if (typeof value.claims !== "object" || Array.isArray(value.claims)) {
            throw new Error("Jazor authentication state claims must be a string-array map.");
        }
        for (const [name, values] of Object.entries(value.claims)) {
            if (!name || !Array.isArray(values) || values.some(item => typeof item !== "string")) {
                throw new Error("Jazor authentication state claims must map names to string arrays.");
            }
        }
    }
    return {
        status: value.status,
        subject: value.subject ?? null,
        claims: value.claims ? Object.fromEntries(
            Object.entries(value.claims).map(([name, values]) => [name, [...values]])) : {},
    };
}

function applySnapshot(target, snapshot) {
    target.status = snapshot.status;
    target.subject = snapshot.subject;
    target.claims = snapshot.claims;
}

function validateEnvelope(value) {
    if (!value || typeof value !== "object" || value.schema !== authenticationEnvelopeSchema ||
        value.version !== authenticationEnvelopeVersion) {
        throw new Error(`Jazor authentication endpoint envelope must use ${authenticationEnvelopeSchema} v${authenticationEnvelopeVersion}.`);
    }
    return { schema: value.schema, version: value.version, state: validateSnapshot(value.state) };
}

/**
 * Creates the browser authentication provider. `refresh` and `signOut` are explicit
 * endpoint adapters supplied by the application; this module never stores tokens or
 * treats an HTTP response as an authorization decision by itself.
 */
export function createAuthenticationProvider(initialState, options = {}) {
    const state = reactive(validateSnapshot(initialState ?? { status: "Anonymous" }));
    const listeners = new Set();
    let requestVersion = 0;
    let disposed = false;
    let lastError = null;

    const notify = () => {
        for (const listener of [...listeners]) listener(state);
    };

    const publish = next => {
        if (disposed) return state;
        const snapshot = validateSnapshot(next);
        applySnapshot(state, snapshot);
        lastError = null;
        notify();
        return state;
    };

    const invokeEndpoint = async (name, fallback, argument) => {
        const endpoint = options[name];
        if (typeof endpoint !== "function") {
            if (fallback) return publish(fallback());
            const error = new Error(`Jazor authentication endpoint '${name}' is not configured.`);
            lastError = error;
            notify();
            throw error;
        }
        const version = ++requestVersion;
        try {
            const next = validateEnvelope(await endpoint(state, argument)).state;
            if (version !== requestVersion || disposed) return state;
            return publish(next);
        } catch (error) {
            if (version === requestVersion && !disposed) {
                lastError = error;
                notify();
            }
            throw error;
        }
    };

    const provider = {
        get status() { return state.status; },
        get subject() { return state.subject; },
        get claims() { return state.claims; },
        get error() { return lastError; },
        snapshot: () => ({
            status: state.status,
            subject: state.subject,
            claims: Object.fromEntries(Object.entries(state.claims).map(([name, values]) => [name, [...values]])),
        }),
        clearError: () => { lastError = null; notify(); },
        setState: publish,
        setAnonymous: () => publish({ status: "Anonymous" }),
        setAuthenticated: (subject, claims = {}) => publish({ status: "Authenticated", subject, claims }),
        markExpired: () => publish({ ...state, status: "Expired" }),
        markForbidden: () => publish({ ...state, status: "Forbidden" }),
        signIn: credentials => invokeEndpoint("signIn", () => {
            throw new Error("Jazor authentication endpoint 'signIn' is not configured.");
        }, credentials),
        refresh: () => invokeEndpoint("refresh"),
        signOut: () => invokeEndpoint("signOut", () => ({ status: "Anonymous" })),
        subscribe: listener => {
            if (typeof listener !== "function") throw new TypeError("Authentication listener must be a function.");
            listeners.add(listener);
            return () => listeners.delete(listener);
        },
        dispose: () => {
            disposed = true;
            requestVersion++;
            listeners.clear();
        },
    };

    provide(authenticationProviderKey, provider);
    onUnmounted?.(provider.dispose);
    return provider;
}

export function installAuthenticationProvider(initialState, options) {
    return createAuthenticationProvider(initialState, options);
}

/**
 * Resolves the SSR-provided snapshot and creates the browser provider in a
 * component setup scope. The app-level SSR envelope supplies only the closed
 * snapshot; endpoint adapters remain application-owned.
 */
export function useAuthenticationProvider(initialState, options) {
    const resolved = initialState ?? inject(authenticationProviderKey, null) ?? { status: "Anonymous" };
    return createAuthenticationProvider(resolved, options);
}

export { validateSnapshot as validateAuthenticationSnapshot, validateEnvelope as validateAuthenticationEnvelope };
