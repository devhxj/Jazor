import { GetObjectHashCode } from "System/RuntimeModule.js";
/*jazor:clr-member virtual object.GetHashCode()*/
export function _97891de43f43ceb4(instance) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  let type = typeof instance;
  if (type === "object" || type === "function") {
    let customHashCode = Reflect.get(instance, "getHashCode");
    if (typeof customHashCode === "function")
      return Reflect.apply(customHashCode, instance, []);
  }
  return GetObjectHashCode(instance);
}
