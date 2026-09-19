/*jazor:clr-member static Microsoft.AspNetCore.Components.ElementReferenceExtensions.FocusAsync(Microsoft.AspNetCore.Components.ElementReference)*/
export function focusAsync(elementReference) {
  EnsureConfigured(elementReference);
  elementReference.focus();
  return Promise.resolve();
}
/*jazor:clr-member static Microsoft.AspNetCore.Components.ElementReferenceExtensions.FocusAsync(Microsoft.AspNetCore.Components.ElementReference, bool)*/
export function focusAsyncWithOptions(elementReference, preventScroll) {
  EnsureConfigured(elementReference);
  let focus = Reflect.get(elementReference, "focus");
  Reflect.apply(focus, elementReference, [{ preventScroll: preventScroll }]);
  return Promise.resolve();
}
function EnsureConfigured(elementReference) {
  if (elementReference == null)
    throw new Error("InvalidOperationException: ElementReference has not been configured correctly.");
}
