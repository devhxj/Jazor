import { DecimalModule } from "System/DecimalModule.js";
import { DoubleModule } from "System/DoubleModule.js";
import { SingleModule } from "System/SingleModule.js";
function compareCore(left, right) {
  if (isNaN(left))
    return isNaN(right) ? 0 : -1;
  if (isNaN(right))
    return 1;
  if (left < right)
    return -1;
  if (left > right)
    return 1;
  return 0;
}
function maxMagnitudeCore(x, y) {
  if (isNaN(x) || isNaN(y))
    return Number.NaN;
  let absX = Math.abs(x);
  let absY = Math.abs(y);
  if (absX > absY)
    return x;
  if (absX < absY)
    return y;
  return Math.max(x, y);
}
function minMagnitudeCore(x, y) {
  if (isNaN(x) || isNaN(y))
    return Number.NaN;
  let absX = Math.abs(x);
  let absY = Math.abs(y);
  if (absX < absY)
    return x;
  if (absX > absY)
    return y;
  return Math.min(x, y);
}
export function _4dcadff583296186(x) {
  return { sin: Math.sin(x), cos: Math.cos(x) };
}
export function _eab3564b2663dff6(value) {
  return DecimalModule._e85678b4de2283e8(value);
}
export function _84cbc0eaf2d899af(d) {
  return DecimalModule._84028a6e79626057(d);
}
export function _735e24a467fce432(value, min, max) {
  return DecimalModule._e886400fbfdbdaaa(value, min, max);
}
export function _b12193a7b6647a82(d) {
  return DecimalModule._518facaaeeb29ead(d);
}
export function _68326de2fcd99278(val1, val2) {
  return DecimalModule._872018e11335480a(val1, val2);
}
export function _7922e74207558715(x, y) {
  return maxMagnitudeCore(x, y);
}
export function _87f14d6593efd87f(val1, val2) {
  return DecimalModule._ceb21f954af742e7(val1, val2);
}
export function _44776725ec896ede(x, y) {
  return minMagnitudeCore(x, y);
}
export function _257741f3e4260d82(d) {
  return DecimalModule._4a816369b59f1ca3(d);
}
export function _10e883cf6d89b70c(d, decimals) {
  return DecimalModule._bc3a974d51c694ab(d, decimals);
}
export function _584a7b2219b578fa(d, mode) {
  return DecimalModule._a334f7e82122cfc2(d, mode);
}
export function _b955eff4c2d1fa63(d, decimals, mode) {
  return DecimalModule._09ee3a4652dbe73c(d, decimals, mode);
}
export function _8d626104a531d041(value) {
  return DecimalModule._ed803cf9c8c052f1(value);
}
export function _9a554cfca79bdc59(value) {
  return DoubleModule.signCore(value);
}
export function _c0668680ba7ef96e(value) {
  return SingleModule.signCore(value);
}
export function _abd9211e1e7514b4(d) {
  return DecimalModule._be8b149ea0e1d76b(d);
}
export const MathModule = {
  compareCore,
  maxMagnitudeCore,
  minMagnitudeCore,
  _4dcadff583296186,
  _eab3564b2663dff6,
  _84cbc0eaf2d899af,
  _735e24a467fce432,
  _b12193a7b6647a82,
  _68326de2fcd99278,
  _7922e74207558715,
  _87f14d6593efd87f,
  _44776725ec896ede,
  _257741f3e4260d82,
  _10e883cf6d89b70c,
  _584a7b2219b578fa,
  _b955eff4c2d1fa63,
  _8d626104a531d041,
  _9a554cfca79bdc59,
  _c0668680ba7ef96e,
  _abd9211e1e7514b4
};
