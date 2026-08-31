import { GetNoneSignal } from "System/Threading/CancellationTokenModule.js";
const PreventedKey = "__jazorNavigationPrevented";
export function CreateLocationChangingContext(targetLocation, historyEntryState, isNavigationIntercepted, cancellationToken) {
  return {
    targetLocation: targetLocation,
    historyEntryState: historyEntryState,
    isNavigationIntercepted: isNavigationIntercepted,
    cancellationToken: cancellationToken,
    __jazorNavigationPrevented: false
  };
}
export function IsNavigationPrevented(context) {
  let prevented = context["__jazorNavigationPrevented"];
  return typeof prevented === "boolean" && prevented;
}
/*jazor:clr-member Microsoft.AspNetCore.Components.Routing.LocationChangingContext.LocationChangingContext()*/
export function createDefault() {
  return {
    targetLocation: null,
    historyEntryState: null,
    isNavigationIntercepted: false,
    cancellationToken: GetNoneSignal(),
    __jazorNavigationPrevented: false
  };
}
/*jazor:clr-member Microsoft.AspNetCore.Components.Routing.LocationChangingContext.PreventNavigation()*/
export function preventNavigation(instance) {
  Reflect.set(instance, "__jazorNavigationPrevented", true);
}
