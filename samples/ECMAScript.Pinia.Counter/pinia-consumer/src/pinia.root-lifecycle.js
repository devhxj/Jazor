import {
  ClearConfiguredActivePinia,
  CreateConfiguredPinia,
  CreatePiniaInstallationApp
} from "host/app.mjs";
import {
  disposePinia,
  getActivePinia,
  setActivePinia
} from "pinia";

const disposedRoots = new WeakSet();

export function createManagedPiniaRoot() {
  const previousPinia = getActivePinia();
  const pinia = CreateConfiguredPinia();
  const app = CreatePiniaInstallationApp(pinia);
  let disposed = false;

  const activate = () => {
    if (disposed) {
      return getActivePinia();
    }

    setActivePinia(pinia);
    return getActivePinia();
  };

  activate();

  return {
    app,
    pinia,
    previousPinia,
    activate,
    get activePinia() {
      return getActivePinia();
    },
    dispose() {
      if (disposed) {
        return;
      }

      disposed = true;
      const activePinia = getActivePinia();
      disposePinia(pinia);
      disposedRoots.add(pinia);

      if (activePinia === pinia) {
        if (previousPinia != null && !disposedRoots.has(previousPinia)) {
          setActivePinia(previousPinia);
        } else {
          ClearConfiguredActivePinia();
        }
      }
    }
  };
}
