import { _b7c36408f0f172e9 } from "System/StringModule.js";
let Values = new WeakMap;
/*jazor:clr-member Microsoft.AspNetCore.Components.ChangeEventArgs.captureChangeEvent*/
export function captureChangeEvent(event) {
  let input;
  if (event == null)
    throw new Error("ArgumentNullException: event is null.");
  let target = event.target;
  let value;
  if (target instanceof HTMLInputElement && (input = target, true)) {
    if (_b7c36408f0f172e9(input.type, "file", 5))
      throw new Error("NotSupportedException: file input changes require InputFileChangeEventArgs.");
    value = _b7c36408f0f172e9(input.type, "checkbox", 5) ? input.checked : input.value;
  }
  else {
    let textArea;
    if (target instanceof HTMLTextAreaElement && (textArea = target, true)) {
      value = textArea.value;
    }
    else {
      let select;
      if (target instanceof HTMLSelectElement && (select = target, true)) {
        if (!select.multiple) {
          value = select.value;
        }
        else {
          let selectedValues = new Array;
          let selectedOptions = select.selectedOptions;
          for (let index = 0; index < selectedOptions.length; index++) {
            let __trycast$1a3c83e0aefc2b5b5842df76;
            let option = (__trycast$1a3c83e0aefc2b5b5842df76 = selectedOptions.item(index), __trycast$1a3c83e0aefc2b5b5842df76 instanceof HTMLOptionElement ? __trycast$1a3c83e0aefc2b5b5842df76 : null);
            if (option == null)
              throw new Error("InvalidOperationException: selected option is not an HTMLOptionElement.");
            selectedValues.push(option.value);
          }
          value = selectedValues;
        }
      }
      else {
        throw new Error("NotSupportedException: ChangeEventArgs requires an input, textarea, or select target.");
      }
    }
  }
  Values.set(event, value);
  return event;
}
/*jazor:clr-member Microsoft.AspNetCore.Components.ChangeEventArgs.Value.get*/
export function getChangeEventValue(event) {
  if (event == null)
    throw new Error("ArgumentNullException: event is null.");
  if (!Values.has(event))
    throw new Error("InvalidOperationException: ChangeEventArgs.Value was read before change capture.");
  return Values.get(event);
}
