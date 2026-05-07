import { createConfiguredPinia } from "../../Pinia.Counter.Host/wwwroot/jazor/host/app.mjs";
import {
  createCounterHotHandler,
  createProjectedCounterHotHandler
} from "../../Pinia.Counter.Host/wwwroot/jazor/components/counter-hmr.mjs";

export function createHmrBridge(hot) {
  const pinia = createConfiguredPinia();
  const counterHandler = createCounterHotHandler(hot);
  const projectedCounterHandler = createProjectedCounterHotHandler(hot);

  return {
    pinia,
    counterHandler,
    projectedCounterHandler,
    register() {
      if (!hot?.accept) {
        return false;
      }

      hot.accept(counterHandler);
      hot.accept(projectedCounterHandler);
      return true;
    }
  };
}

if (import.meta.hot) {
  createHmrBridge(import.meta.hot).register();
}
