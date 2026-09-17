import { getClientKey } from "./utils.js";
import { QueryClient as QueryClient$1 } from "./queryClient.js";
import { setupDevtools } from "./devtools/devtools.js";
import { environmentManager } from "@tanstack/query-core";
import { isVue2 } from "vue-demi";
//#region src/vueQueryPlugin.ts
/**
* Installs a `QueryClient` on the Vue app, making it available to every descendant component through
* `useQueryClient` — the Vue equivalent of React's `QueryClientProvider`, but wired up as an app-level plugin
* instead of a wrapping component.
*
* @example
* ```ts
* import { createApp } from 'vue'
* import { VueQueryPlugin } from '@tanstack/vue-query'
*
* const app = createApp(App)
* app.use(VueQueryPlugin)
* ```
*
* @example
* Pass a `queryClient` you constructed yourself — useful for SSR, where you need a fresh `QueryClient` per
* request, or when the same instance also needs to be used outside of Vue components:
* ```ts
* import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query'
*
* const queryClient = new QueryClient()
* app.use(VueQueryPlugin, { queryClient })
* ```
*
* @example
* Or pass `queryClientConfig` to let the plugin construct the `QueryClient` for you, with your own defaults:
* ```ts
* app.use(VueQueryPlugin, {
*   queryClientConfig: {
*     defaultOptions: { queries: { staleTime: 5 * 1000 } },
*   },
* })
* ```
*/
const VueQueryPlugin = { install: (app, options = {}) => {
	const clientKey = getClientKey(options.queryClientKey);
	let client;
	if ("queryClient" in options && options.queryClient) client = options.queryClient;
	else {
		const clientConfig = "queryClientConfig" in options ? options.queryClientConfig : void 0;
		client = new QueryClient$1(clientConfig);
	}
	if (!environmentManager.isServer()) client.mount();
	let persisterUnmount = () => {};
	if (options.clientPersister) {
		if (client.isRestoring) client.isRestoring.value = true;
		const [unmount, promise] = options.clientPersister(client);
		persisterUnmount = unmount;
		promise.then(() => {
			if (client.isRestoring) client.isRestoring.value = false;
			options.clientPersisterOnSuccess?.(client);
		});
	}
	const cleanup = () => {
		client.unmount();
		persisterUnmount();
	};
	if (app.onUnmount) app.onUnmount(cleanup);
	else {
		const originalUnmount = app.unmount;
		app.unmount = function vueQueryUnmount() {
			cleanup();
			originalUnmount();
		};
	}
	if (isVue2) app.mixin({ beforeCreate() {
		if (!this._provided) {
			const provideCache = {};
			Object.defineProperty(this, "_provided", {
				get: () => provideCache,
				set: (v) => Object.assign(provideCache, v)
			});
		}
		this._provided[clientKey] = client;
		if (process.env.NODE_ENV === "development") {
			if (this === this.$root && options.enableDevtoolsV6Plugin) setupDevtools(this, client);
		}
	} });
	else {
		app.provide(clientKey, client);
		if (process.env.NODE_ENV === "development") {
			if (options.enableDevtoolsV6Plugin) setupDevtools(app, client);
		}
	}
} };
//#endregion
export { VueQueryPlugin };

//# sourceMappingURL=vueQueryPlugin.js.map