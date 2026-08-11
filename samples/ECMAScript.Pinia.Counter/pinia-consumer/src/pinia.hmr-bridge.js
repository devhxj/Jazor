import { CreateConfiguredPinia } from "host/app.mjs";
import {
  CreateCounterHotHandler,
  CreateProjectedCounterHotHandler
} from "components/counter-hmr.mjs";

export function createHmrBridge(hot) {
  const pinia = CreateConfiguredPinia();
  const counterHandler = CreateCounterHotHandler(hot);
  const projectedCounterHandler = CreateProjectedCounterHotHandler(hot);

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
