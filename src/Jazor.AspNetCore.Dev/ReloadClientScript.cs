using System.Text.Json;

namespace Jazor.AspNetCore.Dev;

/// <summary>Builds the browser reload client while preserving its stable JavaScript ABI.</summary>
internal static class ReloadClientScript
{
    public static string Build(string webSocketPath, string pathBaseExpression, bool suppressReloadOnReconnect)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(webSocketPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(pathBaseExpression);

        var serializedWebSocketPath = SerializeJavaScriptString(webSocketPath);
        var serializedPathBaseExpression = SerializeJavaScriptString(pathBaseExpression);
        var reloadOnReconnect = suppressReloadOnReconnect ? "false" : "true";
        // The generated script is consumed directly by browsers. Keep exported names, JSON fields,
        // and protocol message strings stable even when the CLR-side implementation is renamed.
        return $$"""
        const socketProtocol = location.protocol === "https:" ? "wss" : "ws";
        const pathBaseExpression = {{serializedPathBaseExpression}};
        const normalizePathBase = value => {
          if (!value || value === "/") {
            return "";
          }
          return value.endsWith("/") ? value.slice(0, -1) : value;
        };
        const resolvePathBase = () => {
          const documentElement = document.documentElement;
          const configuredValue = documentElement ? (documentElement.getAttribute(pathBaseExpression) || "") : "";
          return normalizePathBase(configuredValue);
        };
        const pathBase = resolvePathBase();
        const socketPath = {{serializedWebSocketPath}};
        const socketUrl = `${socketProtocol}://${location.host}${pathBase}${socketPath}`;
        const reloadOnReconnect = {{reloadOnReconnect}};
        const protocolVersion = {{ReloadHub.ProtocolVersion}};
        const moduleUpdateCapability = "module-update";
        let socket;
        let reconnectTimer;
        let heartbeatTimer;
        let hasConnected = false;
        let currentServerInstanceId = null;
        let currentReloadSequence = 0;
        let resolveTransportReady;
        const transportReady = new Promise(resolve => {
          resolveTransportReady = resolve;
        });
        const hmrHandlers = new Map();
        const vueComponents = new Map();
        function sendMessage(payload) {
          if (!socket || socket.readyState !== WebSocket.OPEN) {
            return;
          }
          socket.send(JSON.stringify(payload));
        }
        function stopHeartbeat() {
          if (!heartbeatTimer) {
            return;
          }
          clearInterval(heartbeatTimer);
          heartbeatTimer = undefined;
        }
        function startHeartbeat() {
          stopHeartbeat();
          heartbeatTimer = setInterval(() => {
            sendMessage({ type: "heartbeat" });
          }, 15000);
        }
        function scheduleReconnect() {
          if (reconnectTimer) {
            return;
          }
          stopHeartbeat();
          reconnectTimer = setTimeout(() => {
            reconnectTimer = undefined;
            connect();
          }, 2000);
        }
        function reloadPage() {
          location.reload();
        }
        function hasCompatibleProtocol(payload) {
          return Number.isInteger(payload?.protocolVersion) && payload.protocolVersion === protocolVersion;
        }
        function reloadForProtocolMismatch(payload) {
          console.error("Jazor reload protocol mismatch. Refreshing the page.", {
            expectedProtocolVersion: protocolVersion,
            receivedProtocolVersion: payload?.protocolVersion,
            messageType: payload?.type
          });
          reloadPage();
        }
        function normalizeModuleUpdates(payload) {
          if (!Array.isArray(payload?.moduleUpdates)) {
            return [];
          }
          return payload.moduleUpdates.filter(update =>
            update &&
            typeof update.path === "string" &&
            typeof update.url === "string" &&
            typeof update.componentId === "string" &&
            typeof update.moduleId === "string" &&
            typeof update.descriptorHash === "string" &&
            typeof update.templateHash === "string" &&
            typeof update.logicHash === "string" &&
            typeof update.boundaryKind === "string");
        }
        function createModuleUpdateDetail(payload) {
          const changedPaths = Array.isArray(payload?.changedPaths)
            ? payload.changedPaths.filter(path => typeof path === "string")
            : [];
          return {
            changedPaths,
            moduleUpdates: normalizeModuleUpdates(payload),
            reason: typeof payload?.reason === "string" ? payload.reason : null,
            reloadSequence: Number.isFinite(payload?.reloadSequence) ? payload.reloadSequence : null,
            serverInstanceId: typeof payload?.serverInstanceId === "string" ? payload.serverInstanceId : null
          };
        }
        function dispatchModuleUpdate(detail) {
          return window.dispatchEvent(new CustomEvent("jazor:module-update", {
            cancelable: true,
            detail
          })) === false;
        }
        function registerVueComponent(moduleId, component) {
          if (typeof moduleId !== "string" || moduleId.length === 0 || (typeof component !== "object" && typeof component !== "function") || component === null) {
            throw new TypeError("JazorHmr.registerVueComponent requires a module id and Vue component.");
          }
          const runtime = globalThis.__VUE_HMR_RUNTIME__;
          if (!runtime || typeof runtime.createRecord !== "function") {
            return false;
          }
          component.__hmrId = moduleId;
          runtime.createRecord(moduleId, component);
          vueComponents.set(moduleId, component);
          return true;
        }
        function reloadVueComponent(moduleId, component) {
          const runtime = globalThis.__VUE_HMR_RUNTIME__;
          if (!runtime || typeof runtime.reload !== "function") {
            return false;
          }
          runtime.reload(moduleId, component);
          return true;
        }
        async function applyRegisteredModuleUpdates(detail) {
          if (detail.moduleUpdates.length === 0) {
            return false;
          }
          for (const update of detail.moduleUpdates) {
            const handlers = hmrHandlers.get(update.moduleId);
            const previousVueComponent = vueComponents.get(update.moduleId);
            if ((!handlers || handlers.size === 0) && !previousVueComponent) {
              return false;
            }
            try {
              const moduleUrl = new URL(update.url, location.href);
              moduleUrl.searchParams.set("__jazor_hmr", String(detail.reloadSequence ?? Date.now()));
              const module = await import(moduleUrl.href);
              if (previousVueComponent) {
                const replacementVueComponent = vueComponents.get(update.moduleId);
                if (!replacementVueComponent || replacementVueComponent === previousVueComponent || !reloadVueComponent(update.moduleId, replacementVueComponent)) {
                  return false;
                }
              }
              if (handlers) {
                for (const handler of Array.from(handlers)) {
                  if (await handler({ module, update, reason: detail.reason, reloadSequence: detail.reloadSequence }) === false) {
                    return false;
                  }
                }
              }
            } catch (error) {
              console.error("Jazor module update failed.", error);
              return false;
            }
          }
          return true;
        }
        async function acceptModuleUpdate(payload) {
          const detail = createModuleUpdateDetail(payload);
          if (dispatchModuleUpdate(detail)) {
            return true;
          }
          return await applyRegisteredModuleUpdates(detail);
        }
        Object.defineProperty(window, "JazorHmr", {
          configurable: true,
          enumerable: false,
          value: Object.freeze({
            ready: transportReady,
            registerVueComponent(moduleId, component) {
              return registerVueComponent(moduleId, component);
            },
            accept(moduleId, handler) {
              if (typeof moduleId !== "string" || moduleId.length === 0 || typeof handler !== "function") {
                throw new TypeError("JazorHmr.accept requires a module id and handler.");
              }
              let handlers = hmrHandlers.get(moduleId);
              if (!handlers) {
                handlers = new Set();
                hmrHandlers.set(moduleId, handlers);
              }
              handlers.add(handler);
              return () => {
                handlers.delete(handler);
                if (handlers.size === 0) {
                  hmrHandlers.delete(moduleId);
                }
              };
            }
          })
        });
        function handleConnected(payload) {
          if (!hasCompatibleProtocol(payload)) {
            reloadForProtocolMismatch(payload);
            return;
          }
          const nextServerInstanceId = typeof payload?.serverInstanceId === "string" ? payload.serverInstanceId : null;
          const nextReloadSequence = Number.isFinite(payload?.reloadSequence) ? payload.reloadSequence : 0;
          const shouldReloadAfterReconnect = hasConnected && (
            (currentServerInstanceId && nextServerInstanceId && currentServerInstanceId !== nextServerInstanceId) ||
            nextReloadSequence > currentReloadSequence
          );
          currentServerInstanceId = nextServerInstanceId;
          currentReloadSequence = nextReloadSequence;
          if (reloadOnReconnect && shouldReloadAfterReconnect) {
            reloadPage();
            return;
          }
          hasConnected = true;
        }
        function handleSocketMessage(event) {
          let payload;
          try {
            payload = JSON.parse(event.data);
          } catch {
            payload = { type: event.data };
          }
          if (!hasCompatibleProtocol(payload)) {
            reloadForProtocolMismatch(payload);
            return;
          }
          if (payload?.type === "connected") {
            handleConnected(payload);
            return;
          }
          if (payload?.type !== "reload" &&
              payload?.type !== "full-reload" &&
              payload?.type !== "module-update") {
            console.warn("Jazor reload client received an unknown message type.", payload?.type);
            reloadPage();
            return;
          }
          if (payload?.type === "reload" || payload?.type === "full-reload") {
            currentReloadSequence = Number.isFinite(payload?.reloadSequence)
              ? Math.max(currentReloadSequence, payload.reloadSequence)
              : currentReloadSequence + 1;
            reloadPage();
            return;
          }
          if (payload?.type === "module-update") {
            currentReloadSequence = Number.isFinite(payload?.reloadSequence)
              ? Math.max(currentReloadSequence, payload.reloadSequence)
              : currentReloadSequence + 1;
            void acceptModuleUpdate(payload).then(accepted => {
              if (!accepted) {
                reloadPage();
              }
            });
          }
        }
        function connect() {
          try {
            socket = new WebSocket(socketUrl);
            socket.addEventListener("open", () => {
              if (reconnectTimer) {
                clearTimeout(reconnectTimer);
                reconnectTimer = undefined;
              }
              sendMessage({ type: "ready", protocolVersion, capabilities: [moduleUpdateCapability] });
              resolveTransportReady();
              startHeartbeat();
            });
            socket.addEventListener("message", handleSocketMessage);
            socket.addEventListener("close", () => {
              stopHeartbeat();
              scheduleReconnect();
            });
            socket.addEventListener("error", () => {
              stopHeartbeat();
              socket?.close();
            });
          } catch {
            scheduleReconnect();
          }
        }
        connect();
        export {};
        """;
    }

    private static string SerializeJavaScriptString(string value)
        => "\"" + JsonEncodedText.Encode(value).ToString() + "\"";
}
