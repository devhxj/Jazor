//#region src/devtools/utils.ts
function getQueryState(query) {
	if (query.state.fetchStatus === "fetching") return 0;
	if (query.state.fetchStatus === "paused") return 4;
	if (!query.getObserversCount()) return 3;
	if (query.isStale()) return 2;
	return 1;
}
function getQueryStateLabel(query) {
	const queryState = getQueryState(query);
	if (queryState === 0) return "fetching";
	if (queryState === 4) return "paused";
	if (queryState === 2) return "stale";
	if (queryState === 3) return "inactive";
	return "fresh";
}
function getQueryStatusFg(query) {
	if (getQueryState(query) === 2) return 0;
	return 16777215;
}
function getQueryStatusBg(query) {
	const queryState = getQueryState(query);
	if (queryState === 0) return 27647;
	if (queryState === 4) return 9193963;
	if (queryState === 2) return 16757248;
	if (queryState === 3) return 4148832;
	return 33575;
}
const queryHashSort = (a, b) => a.queryHash.localeCompare(b.queryHash);
const dateSort = (a, b) => a.state.dataUpdatedAt < b.state.dataUpdatedAt ? 1 : -1;
const statusAndDateSort = (a, b) => {
	if (getQueryState(a) === getQueryState(b)) return dateSort(a, b);
	return getQueryState(a) > getQueryState(b) ? 1 : -1;
};
const sortFns = {
	"Status > Last Updated": statusAndDateSort,
	"Query Hash": queryHashSort,
	"Last Updated": dateSort
};
//#endregion
export { getQueryState, getQueryStateLabel, getQueryStatusBg, getQueryStatusFg, sortFns };

//# sourceMappingURL=utils.js.map