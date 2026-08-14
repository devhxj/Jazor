import { C as e, Jt as t } from "./lib-Bttd6u5E.js";
//#region src/useNestedProp.js
function n({ defaultConfig: n, userConfig: r }) {
	if (!Object.keys(r || {}).length) return n;
	let i = t({
		defaultConfig: n,
		userConfig: r
	});
	return e(i);
}
//#endregion
export { n as t };
