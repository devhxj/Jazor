using System.Text.Json;

namespace Jazor.AspNetCore.Dev;

internal static class JazorDevelopmentClientScriptFactory
{
    public static string Build(string webSocketPath, string pathBaseExpression, bool suppressReloadOnReconnect)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(webSocketPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(pathBaseExpression);

        var serializedWebSocketPath = JsonSerializer.Serialize(webSocketPath);
        var serializedPathBaseExpression = JsonSerializer.Serialize(pathBaseExpression);
        var reloadOnReconnect = suppressReloadOnReconnect ? "false" : "true";
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
        let socket;
        let reconnectTimer;
        let heartbeatTimer;
        let hasConnected = false;
        let currentServerInstanceId = null;
        let currentReloadSequence = 0;
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
        function handleConnected(payload) {
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
          if (payload?.type === "connected") {
            handleConnected(payload);
            return;
          }
          if (payload?.type === "reload" || payload?.type === "full-reload") {
            currentReloadSequence = Number.isFinite(payload?.reloadSequence)
              ? Math.max(currentReloadSequence, payload.reloadSequence)
              : currentReloadSequence + 1;
            reloadPage();
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
              sendMessage({ type: "ready" });
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
}
