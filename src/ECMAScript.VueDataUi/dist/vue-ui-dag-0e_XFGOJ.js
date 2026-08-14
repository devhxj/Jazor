import { t as e } from "./rolldown-runtime-Dy4uBu1J.js";
import { Jt as t, K as n, S as r, gt as i, jt as a, q as o, t as s, tt as c } from "./lib-Bttd6u5E.js";
import { n as l, t as u } from "./useHints-Dq_w2E8B.js";
import { t as d } from "./useConfig-DlNpz6P8.js";
import { t as f } from "./usePrinter-DN5bYhTG.js";
import { n as p, t as m } from "./BaseScanner-DZvpgOjM.js";
import { t as h } from "./useNestedProp-vPNvh7rV.js";
import { t as g } from "./useThemeCheck-C43Tcqmk.js";
import { t as ee } from "./useChartExport-DNiwdPmb.js";
import { t as te } from "./img-Bnokohej.js";
import { n as ne } from "./Title-BE3qg9xl.js";
import { t as re } from "./_plugin-vue_export-helper-B3ysoDQm.js";
import { l as ie, t as ae } from "./useResponsive-ZtArZtUf.js";
import { t as oe } from "./A11yDataTable-DdRsVULz.js";
import { t as se } from "./useUserOptionState-DK-_1ddE.js";
import { t as ce } from "./useChartAccessibility-DYqac8yF.js";
import { t as le } from "./usePanZoom-CYU3B4T3.js";
import { t as ue } from "./BaseZoomControls-BZvCnZEi.js";
import { t as de } from "./vue_ui_dag-TsJ_azQq.js";
import { Fragment as fe, Teleport as pe, Transition as me, computed as _, createBlock as v, createCommentVNode as y, createElementBlock as b, createElementVNode as x, createSlots as he, createTextVNode as ge, createVNode as _e, defineAsyncComponent as ve, guardReactiveProps as S, mergeProps as ye, nextTick as be, normalizeClass as xe, normalizeProps as C, normalizeStyle as Se, onBeforeUnmount as Ce, onMounted as we, openBlock as w, ref as T, renderList as Te, renderSlot as E, toDisplayString as Ee, toRefs as De, unref as D, watch as Oe, watchEffect as ke, withCtx as O, withModifiers as Ae } from "vue";
//#region src/DAG/graph.js
var je = "\0", Me = "\0", Ne = "", k = class {
	_isDirected = !0;
	_isMultigraph = !1;
	_isCompound = !1;
	_label;
	_defaultNodeLabelFn = () => void 0;
	_defaultEdgeLabelFn = () => void 0;
	_nodes = {};
	_in = {};
	_preds = {};
	_out = {};
	_sucs = {};
	_edgeObjs = {};
	_edgeLabels = {};
	_nodeCount = 0;
	_edgeCount = 0;
	_parent;
	_children;
	constructor(e) {
		e && (this._isDirected = !Object.hasOwn(e, "directed") || e.directed, this._isMultigraph = Object.hasOwn(e, "multigraph") ? e.multigraph : !1, this._isCompound = Object.hasOwn(e, "compound") ? e.compound : !1), this._isCompound && (this._parent = {}, this._children = {}, this._children[Me] = {});
	}
	isDirected() {
		return this._isDirected;
	}
	isMultigraph() {
		return this._isMultigraph;
	}
	isCompound() {
		return this._isCompound;
	}
	setGraph(e) {
		return this._label = e, this;
	}
	graph() {
		return this._label;
	}
	setDefaultNodeLabel(e) {
		return this._defaultNodeLabelFn = e, typeof e != "function" && (this._defaultNodeLabelFn = () => e), this;
	}
	nodeCount() {
		return this._nodeCount;
	}
	nodes() {
		return Object.keys(this._nodes);
	}
	sources() {
		let e = this;
		return this.nodes().filter((t) => Object.keys(e._in[t]).length === 0);
	}
	sinks() {
		let e = this;
		return this.nodes().filter((t) => Object.keys(e._out[t]).length === 0);
	}
	setNodes(e, t) {
		let n = arguments, r = this;
		return e.forEach((e) => {
			n.length > 1 ? r.setNode(e, t) : r.setNode(e);
		}), this;
	}
	setNode(e, t) {
		return Object.hasOwn(this._nodes, e) ? (arguments.length > 1 && (this._nodes[e] = t), this) : (this._nodes[e] = arguments.length > 1 ? t : this._defaultNodeLabelFn(e), this._isCompound && (this._parent[e] = Me, this._children[e] = {}, this._children[Me][e] = !0), this._in[e] = {}, this._preds[e] = {}, this._out[e] = {}, this._sucs[e] = {}, ++this._nodeCount, this);
	}
	node(e) {
		return this._nodes[e];
	}
	hasNode(e) {
		return Object.hasOwn(this._nodes, e);
	}
	removeNode(e) {
		let t = this;
		if (Object.hasOwn(this._nodes, e)) {
			let n = (e) => t.removeEdge(t._edgeObjs[e]);
			delete this._nodes[e], this._isCompound && (this._removeFromParentsChildList(e), delete this._parent[e], this.children(e).forEach((e) => {
				t.setParent(e);
			}), delete this._children[e]), Object.keys(this._in[e]).forEach(n), delete this._in[e], delete this._preds[e], Object.keys(this._out[e]).forEach(n), delete this._out[e], delete this._sucs[e], --this._nodeCount;
		}
		return this;
	}
	setParent(e, t) {
		if (!this._isCompound) throw Error("Cannot set parent in a non-compound graph");
		if (t === void 0) t = Me;
		else {
			t += "";
			for (let n = t; n !== void 0; n = this.parent(n)) if (n === e) throw Error("Setting " + t + " as parent of " + e + " would create a cycle");
			this.setNode(t);
		}
		return this.setNode(e), this._removeFromParentsChildList(e), this._parent[e] = t, this._children[t][e] = !0, this;
	}
	_removeFromParentsChildList(e) {
		delete this._children[this._parent[e]][e];
	}
	parent(e) {
		if (this._isCompound) {
			let t = this._parent[e];
			if (t !== Me) return t;
		}
	}
	children(e = Me) {
		if (this._isCompound) {
			let t = this._children[e];
			if (t) return Object.keys(t);
		} else if (e === Me) return this.nodes();
		else if (this.hasNode(e)) return [];
	}
	predecessors(e) {
		let t = this._preds[e];
		if (t) return Object.keys(t);
	}
	successors(e) {
		let t = this._sucs[e];
		if (t) return Object.keys(t);
	}
	neighbors(e) {
		let t = this.predecessors(e);
		if (t) {
			let n = new Set(t);
			for (let t of this.successors(e)) n.add(t);
			return Array.from(n.values());
		}
	}
	isLeaf(e) {
		let t;
		return t = this.isDirected() ? this.successors(e) : this.neighbors(e), t.length === 0;
	}
	filterNodes(e) {
		let t = new this.constructor({
			directed: this._isDirected,
			multigraph: this._isMultigraph,
			compound: this._isCompound
		});
		t.setGraph(this.graph());
		let n = this;
		Object.entries(this._nodes).forEach(([n, r]) => {
			e(n) && t.setNode(n, r);
		}), Object.values(this._edgeObjs).forEach((e) => {
			t.hasNode(e.v) && t.hasNode(e.w) && t.setEdge(e, n.edge(e));
		});
		let r = {};
		function i(e) {
			let a = n.parent(e);
			return a === void 0 || t.hasNode(a) ? (r[e] = a, a) : a in r ? r[a] : i(a);
		}
		return this._isCompound && t.nodes().forEach((e) => t.setParent(e, i(e))), t;
	}
	setDefaultEdgeLabel(e) {
		return this._defaultEdgeLabelFn = e, typeof e != "function" && (this._defaultEdgeLabelFn = () => e), this;
	}
	edgeCount() {
		return this._edgeCount;
	}
	edges() {
		return Object.values(this._edgeObjs);
	}
	setPath(e, t) {
		let n = this, r = arguments;
		return e.reduce((e, i) => (r.length > 1 ? n.setEdge(e, i, t) : n.setEdge(e, i), i)), this;
	}
	setEdge() {
		let e, t, n, r, i = !1, a = arguments[0];
		typeof a == "object" && a && "v" in a ? (e = a.v, t = a.w, n = a.name, arguments.length === 2 && (r = arguments[1], i = !0)) : (e = a, t = arguments[1], n = arguments[3], arguments.length > 2 && (r = arguments[2], i = !0)), e = "" + e, t = "" + t, n !== void 0 && (n = "" + n);
		let o = j(this._isDirected, e, t, n);
		if (Object.hasOwn(this._edgeLabels, o)) return i && (this._edgeLabels[o] = r), this;
		if (n !== void 0 && !this._isMultigraph) throw Error("Cannot set a named edge when isMultigraph = false");
		this.setNode(e), this.setNode(t), this._edgeLabels[o] = i ? r : this._defaultEdgeLabelFn(e, t, n);
		let s = M(this._isDirected, e, t, n);
		return e = s.v, t = s.w, Object.freeze(s), this._edgeObjs[o] = s, Pe(this._preds[t], e), Pe(this._sucs[e], t), this._in[t][o] = s, this._out[e][o] = s, this._edgeCount++, this;
	}
	edge(e, t, n) {
		let r = arguments.length === 1 ? N(this._isDirected, arguments[0]) : j(this._isDirected, e, t, n);
		return this._edgeLabels[r];
	}
	edgeAsObj() {
		let e = this.edge(...arguments);
		return typeof e == "object" ? e : { label: e };
	}
	hasEdge(e, t, n) {
		let r = arguments.length === 1 ? N(this._isDirected, arguments[0]) : j(this._isDirected, e, t, n);
		return Object.hasOwn(this._edgeLabels, r);
	}
	removeEdge(e, t, n) {
		let r = arguments.length === 1 ? N(this._isDirected, arguments[0]) : j(this._isDirected, e, t, n), i = this._edgeObjs[r];
		return i && (e = i.v, t = i.w, delete this._edgeLabels[r], delete this._edgeObjs[r], A(this._preds[t], e), A(this._sucs[e], t), delete this._in[t][r], delete this._out[e][r], this._edgeCount--), this;
	}
	inEdges(e, t) {
		let n = this._in[e];
		if (n) {
			let e = Object.values(n);
			return t ? e.filter((e) => e.v === t) : e;
		}
	}
	outEdges(e, t) {
		let n = this._out[e];
		if (n) {
			let e = Object.values(n);
			return t ? e.filter((e) => e.w === t) : e;
		}
	}
	nodeEdges(e, t) {
		let n = this.inEdges(e, t);
		if (n) return n.concat(this.outEdges(e, t));
	}
};
function Pe(e, t) {
	e[t] ? e[t]++ : e[t] = 1;
}
function A(e, t) {
	--e[t] || delete e[t];
}
function j(e, t, n, r) {
	let i = "" + t, a = "" + n;
	if (!e && i > a) {
		let e = i;
		i = a, a = e;
	}
	return i + Ne + a + Ne + (r === void 0 ? je : r);
}
function M(e, t, n, r) {
	let i = "" + t, a = "" + n;
	if (!e && i > a) {
		let e = i;
		i = a, a = e;
	}
	let o = {
		v: i,
		w: a
	};
	return r && (o.name = r), o;
}
function N(e, t) {
	return j(e, t.v, t.w, t.name);
}
//#endregion
//#region src/DAG/data/list.js
var Fe = class {
	constructor() {
		let e = {};
		e._next = e._prev = e, this._sentinel = e;
	}
	dequeue() {
		let e = this._sentinel, t = e._prev;
		if (t !== e) return Ie(t), t;
	}
	enqueue(e) {
		let t = this._sentinel;
		e._prev && e._next && Ie(e), e._next = t._next, t._next._prev = e, t._next = e, e._prev = t;
	}
	toString() {
		let e = [], t = this._sentinel, n = t._prev;
		for (; n !== t;) e.push(JSON.stringify(n, Le)), n = n._prev;
		return "[" + e.join(", ") + "]";
	}
};
function Ie(e) {
	e._prev._next = e._next, e._next._prev = e._prev, delete e._next, delete e._prev;
}
function Le(e, t) {
	if (e !== "_next" && e !== "_prev") return t;
}
//#endregion
//#region src/DAG/greedy-fas.js
var Re = () => 1;
function ze(e, t) {
	if (e.nodeCount() <= 1) return [];
	let n = P(e, t || Re);
	return Be(n.graph, n.buckets, n.zeroIndex).flatMap((t) => e.outEdges(t.v, t.w));
}
function Be(e, t, n) {
	let r = [], i = t[t.length - 1], a = t[0], o;
	for (; e.nodeCount();) {
		for (; o = a.dequeue();) Ve(e, t, n, o);
		for (; o = i.dequeue();) Ve(e, t, n, o);
		if (e.nodeCount()) {
			for (let i = t.length - 2; i > 0; --i) if (o = t[i].dequeue(), o) {
				r = r.concat(Ve(e, t, n, o, !0));
				break;
			}
		}
	}
	return r;
}
function Ve(e, t, n, r, i) {
	let a = i ? [] : void 0;
	return e.inEdges(r.v).forEach((r) => {
		let o = e.edge(r), s = e.node(r.v);
		i && a.push({
			v: r.v,
			w: r.w
		}), s.out -= o, F(t, n, s);
	}), e.outEdges(r.v).forEach((r) => {
		let i = e.edge(r), a = e.node(r.w);
		a.in -= i, F(t, n, a);
	}), e.removeNode(r.v), a;
}
function P(e, t) {
	let n = new k(), r = 0, i = 0;
	e.nodes().forEach((e) => {
		n.setNode(e, {
			v: e,
			in: 0,
			out: 0
		});
	}), e.edges().forEach((e) => {
		let a = n.edge(e.v, e.w) || 0, o = t(e), s = a + o;
		n.setEdge(e.v, e.w, s), i = Math.max(i, n.node(e.v).out += o), r = Math.max(r, n.node(e.w).in += o);
	});
	let a = I(i + r + 3).map(() => new Fe()), o = r + 1;
	return n.nodes().forEach((e) => {
		F(a, o, n.node(e));
	}), {
		graph: n,
		buckets: a,
		zeroIndex: o
	};
}
function F(e, t, n) {
	n.out ? n.in ? e[n.out - n.in + t].enqueue(n) : e[e.length - 1].enqueue(n) : e[0].enqueue(n);
}
function I(e) {
	let t = [];
	for (let n = 0; n < e; n++) t.push(n);
	return t;
}
//#endregion
//#region src/DAG/util.js
function L(e, t, n, r) {
	let i = r;
	for (; e.hasNode(i);) i = tt(r);
	return n.dummy = t, e.setNode(i, n), i;
}
function He(e) {
	let t = new k().setGraph(e.graph());
	return e.nodes().forEach((n) => {
		t.setNode(n, e.node(n));
	}), e.edges().forEach((n) => {
		let r = t.edge(n.v, n.w) || {
			weight: 0,
			minlen: 1
		}, i = e.edge(n);
		t.setEdge(n.v, n.w, {
			weight: r.weight + i.weight,
			minlen: Math.max(r.minlen, i.minlen)
		});
	}), t;
}
function Ue(e) {
	let t = new k({ multigraph: e.isMultigraph() }).setGraph(e.graph());
	return e.nodes().forEach((n) => {
		e.children(n).length || t.setNode(n, e.node(n));
	}), e.edges().forEach((n) => {
		t.setEdge(n, e.edge(n));
	}), t;
}
function We(e) {
	let t = e.nodes().map((t) => {
		let n = {};
		return e.outEdges(t).forEach((t) => {
			n[t.w] = (n[t.w] || 0) + e.edge(t).weight;
		}), n;
	});
	return W(e.nodes(), t);
}
function R(e) {
	let t = e.nodes().map((t) => {
		let n = {};
		return e.inEdges(t).forEach((t) => {
			n[t.v] = (n[t.v] || 0) + e.edge(t).weight;
		}), n;
	});
	return W(e.nodes(), t);
}
function Ge(e, t) {
	let n = e.x, r = e.y, i = t.x - n, a = t.y - r, o = e.width / 2, s = e.height / 2;
	if (!i && !a) throw Error("Not possible to find intersection inside of the rectangle");
	let c, l;
	return Math.abs(a) * o > Math.abs(i) * s ? (a < 0 && (s = -s), c = s * i / a, l = s) : (i < 0 && (o = -o), c = o, l = o * a / i), {
		x: n + c,
		y: r + l
	};
}
function Ke(e) {
	let t = nt(V(e) + 1).map(() => []);
	return e.nodes().forEach((n) => {
		let r = e.node(n), i = r.rank;
		i !== void 0 && (t[i][r.order] = n);
	}), t;
}
function qe(e) {
	let t = e.nodes().map((t) => {
		let n = e.node(t).rank;
		return n === void 0 ? Number.MAX_VALUE : n;
	}), n = B(Math.min, t);
	e.nodes().forEach((t) => {
		let r = e.node(t);
		Object.hasOwn(r, "rank") && (r.rank -= n);
	});
}
function Je(e) {
	let t = e.nodes().map((t) => e.node(t).rank).filter((e) => e !== void 0), n = B(Math.min, t), r = [];
	e.nodes().forEach((t) => {
		let i = e.node(t).rank - n;
		r[i] || (r[i] = []), r[i].push(t);
	});
	let i = 0, a = e.graph().nodeRankFactor;
	Array.from(r).forEach((t, n) => {
		t === void 0 && n % a !== 0 ? --i : t !== void 0 && i && t.forEach((t) => {
			e.node(t).rank += i;
		});
	});
}
function z(e, t, n, r) {
	let i = {
		width: 0,
		height: 0
	};
	return arguments.length >= 4 && (i.rank = n, i.order = r), L(e, "border", i, t);
}
var Ye = 65535;
function Xe(e, t = Ye) {
	let n = [];
	for (let r = 0; r < e.length; r += t) {
		let i = e.slice(r, r + t);
		n.push(i);
	}
	return n;
}
function B(e, t) {
	if (t.length > Ye) {
		let n = Xe(t);
		return e.apply(null, n.map((t) => e.apply(null, t)));
	}
	return e.apply(null, t);
}
function V(e) {
	let t = e.nodes().map((t) => {
		let n = e.node(t).rank;
		return n === void 0 ? Number.MIN_VALUE : n;
	});
	return B(Math.max, t);
}
function Ze(e, t) {
	let n = {
		lhs: [],
		rhs: []
	};
	return e.forEach((e) => {
		t(e) ? n.lhs.push(e) : n.rhs.push(e);
	}), n;
}
function Qe(e, t) {
	let n = Date.now();
	try {
		return t();
	} finally {
		console.log(e + " time: " + (Date.now() - n) + "ms");
	}
}
function $e(e, t) {
	return t();
}
var et = 0;
function tt(e) {
	let t = ++et;
	return e + String(t);
}
function nt(e, t, n = 1) {
	t ?? (t = e, e = 0);
	let r = (e) => e < t;
	n < 0 && (r = (e) => t < e);
	let i = [];
	for (let t = e; r(t); t += n) i.push(t);
	return i;
}
function H(e, t) {
	let n = {};
	for (let r of t) e[r] !== void 0 && (n[r] = e[r]);
	return n;
}
function U(e, t) {
	let n = t;
	if (typeof t == "string") {
		let e = t;
		n = (t) => t[e];
	}
	return Object.entries(e).reduce((e, [t, r]) => (e[t] = n(r, t), e), {});
}
function W(e, t) {
	return e.reduce((e, n, r) => (e[n] = t[r], e), {});
}
var rt = {
	addBorderNode: z,
	addDummyNode: L,
	applyWithChunking: B,
	asNonCompoundGraph: Ue,
	buildLayerMatrix: Ke,
	intersectRect: Ge,
	mapValues: U,
	maxRank: V,
	normalizeRanks: qe,
	notime: $e,
	partition: Ze,
	pick: H,
	predecessorWeights: R,
	range: nt,
	removeEmptyRanks: Je,
	simplify: He,
	successorWeights: We,
	time: Qe,
	uniqueId: tt,
	zipObject: W
};
//#endregion
//#region src/DAG/acyclic.js
function it(e) {
	(e.graph().acyclicer === "greedy" ? ze(e, t(e)) : G(e)).forEach((t) => {
		let n = e.edge(t);
		e.removeEdge(t), n.forwardName = t.name, n.reversed = !0, e.setEdge(t.w, t.v, n, tt("rev"));
	});
	function t(e) {
		return (t) => e.edge(t).weight;
	}
}
function at(e) {
	e.edges().forEach((t) => {
		let n = e.edge(t);
		if (n.reversed) {
			e.removeEdge(t);
			let r = n.forwardName;
			delete n.reversed, delete n.forwardName, e.setEdge(t.w, t.v, n, r);
		}
	});
}
function G(e) {
	let t = [], n = {}, r = {};
	function i(a) {
		Object.hasOwn(r, a) || (r[a] = !0, n[a] = !0, e.outEdges(a).forEach((e) => {
			Object.hasOwn(n, e.w) ? t.push(e) : i(e.w);
		}), delete n[a]);
	}
	return e.nodes().forEach(i), t;
}
//#endregion
//#region src/DAG/normalize.js
function ot(e) {
	e.graph().dummyChains = [], e.edges().forEach((t) => st(e, t));
}
function st(e, t) {
	let n = t.v, r = e.node(n).rank, i = t.w, a = e.node(i).rank, o = t.name, s = e.edge(t), c = s.labelRank;
	if (a === r + 1) return;
	e.removeEdge(t);
	let l, u, d;
	for (d = 0, ++r; r < a; ++d, ++r) s.points = [], u = {
		width: 0,
		height: 0,
		edgeLabel: s,
		edgeObj: t,
		rank: r
	}, l = L(e, "edge", u, "_d"), r === c && (u.width = s.width, u.height = s.height, u.dummy = "edge-label", u.labelpos = s.labelpos), e.setEdge(n, l, { weight: s.weight }, o), d === 0 && e.graph().dummyChains.push(l), n = l;
	e.setEdge(n, i, { weight: s.weight }, o);
}
function ct(e) {
	e.graph().dummyChains.forEach((t) => {
		let n = e.node(t), r = n.edgeLabel;
		e.setEdge(n.edgeObj, r);
		let i;
		for (; n.dummy;) i = e.successors(t)[0], e.removeNode(t), r.points.push({
			x: n.x,
			y: n.y
		}), n.dummy === "edge-label" && (r.x = n.x, r.y = n.y, r.width = n.width, r.height = n.height), t = i, n = e.node(t);
	});
}
//#endregion
//#region src/DAG/rank/util.js
function lt(e) {
	function t(n) {
		let r = e.node(n);
		if (r && Object.prototype.hasOwnProperty.call(r, "rank")) return r.rank;
		let i = e.outEdges(n) || [];
		if (!i.length) return r && (r.rank = 0), 0;
		let a = i.map((n) => e.node(n.w) ? t(n.w) - e.edge(n).minlen : Infinity), o = B(Math.min, a);
		return o === Infinity && (o = 0), r && (r.rank = o), o;
	}
	(e.sources() || []).forEach(t);
}
function ut(e, t) {
	return e.node(t.w).rank - e.node(t.v).rank - e.edge(t).minlen;
}
//#endregion
//#region src/DAG/rank/feasible-tree.js
function dt(e) {
	ft(e);
	let t = new k();
	e.nodes().forEach((e) => {
		t.setNode(e, {});
	});
	let n = e.nodes();
	if (!n.length) return t;
	let r = n[0], i = /* @__PURE__ */ new Set([r]);
	for (; i.size < n.length;) {
		let r = pt(e, i);
		if (!r) {
			let e = n.find((e) => !i.has(e));
			i.add(e), t.setNode(e, {});
			continue;
		}
		let { edgeObject: a, delta: o, attachFrom: s, attachTo: c } = r;
		mt(e, i, s, o), t.setEdge(a.v, a.w, {}), i.add(c);
	}
	return t;
}
function ft(e) {
	e.nodes().forEach((t) => {
		let n = e.node(t) || {};
		Object.prototype.hasOwnProperty.call(n, "rank") || (n.rank = 0, e.setNode(t, n));
	});
}
function pt(e, t) {
	let n = null;
	return e.edges().forEach((r) => {
		let i = t.has(r.v);
		if (i === t.has(r.w)) return;
		let a = ut(e, r), o = Math.abs(a);
		if (!n || o < n.absoluteSlack) {
			let e = i ? r.v : r.w, t = i ? r.w : r.v;
			n = {
				edgeObject: r,
				delta: i ? a : -a,
				attachFrom: e,
				attachTo: t,
				absoluteSlack: o
			};
		}
	}), n;
}
function mt(e, t, n, r) {
	r && e.nodes().forEach((n) => {
		if (!t.has(n)) {
			let t = e.node(n);
			t.rank += r;
		}
	});
}
//#endregion
//#region src/DAG/rank/network-simplex.js
function ht(e) {
	let t = He(e);
	return lt(t), t.nodes().forEach((n) => {
		let r = t.node(n), i = e.node(n) || {};
		i.rank = r.rank, e.setNode(n, i);
	}), e;
}
function gt(e, t) {}
function _t(e, t) {}
function vt(e, t, n) {
	return 0;
}
function yt(e) {
	return null;
}
function bt(e, t) {
	return null;
}
ht.initLowLimValues = gt, ht.initCutValues = _t, ht.calcCutValue = vt, ht.leaveEdge = yt, ht.enterEdge = bt;
//#endregion
//#region src/DAG/rank/index.js
var xt = lt;
function St(e) {
	let t = e.graph().ranker;
	if (t instanceof Function) return t(e);
	switch (e.graph().ranker) {
		case "network-simplex":
			K(e);
			break;
		case "tight-tree":
			wt(e);
			break;
		case "longest-path":
			Ct(e);
			break;
		case "none": break;
		default: K(e);
	}
}
var Ct = xt;
function wt(e) {
	xt(e), dt(e);
}
function K(e) {
	ht(e);
}
//#endregion
//#region src/DAG/parent-dummy-chains.js
function Tt(e) {
	let t = q(e);
	e.graph().dummyChains.forEach((n) => {
		let r = e.node(n), i = r.edgeObj, a = Et(e, t, i.v, i.w), o = a.path, s = a.lca, c = 0, l = o[c], u = !0, d = n;
		for (; d !== i.w;) {
			if (r = e.node(d), u) {
				for (; (l = o[c]) !== s && e.node(l).maxRank < r.rank;) c++;
				l === s && (u = !1);
			}
			if (!u) {
				for (; c < o.length - 1 && e.node(l = o[c + 1]).minRank <= r.rank;) c++;
				l = o[c];
			}
			e.setParent(d, l), d = e.successors(d)[0];
		}
	});
}
function Et(e, t, n, r) {
	let i = [], a = [], o = Math.min(t[n].low, t[r].low), s = Math.max(t[n].lim, t[r].lim), c, l;
	c = n;
	do
		c = e.parent(c), i.push(c);
	while (c && (t[c].low > o || s > t[c].lim));
	for (l = c, c = r; (c = e.parent(c)) !== l;) a.push(c);
	return {
		path: i.concat(a.reverse()),
		lca: l
	};
}
function q(e) {
	let t = {}, n = 0;
	function r(i) {
		let a = n;
		e.children(i).forEach(r), t[i] = {
			low: a,
			lim: n++
		};
	}
	return e.children().forEach(r), t;
}
//#endregion
//#region src/DAG/nested-graph.js
function Dt(e) {
	let t = rt.addDummyNode(e, "root", {}, "_root"), n = kt(e), r = Object.values(n), i = rt.applyWithChunking(Math.max, r) - 1, a = 2 * i + 1;
	e.graph().nestingRoot = t, e.edges().forEach((t) => {
		e.edge(t).minlen *= a;
	});
	let o = At(e) + 1;
	e.children().forEach((r) => {
		Ot(e, t, a, o, i, n, r);
	}), e.graph().nodeRankFactor = a;
}
function Ot(e, t, n, r, i, a, o) {
	let s = e.children(o);
	if (!s.length) {
		o !== t && e.setEdge(t, o, {
			weight: 0,
			minlen: n
		});
		return;
	}
	let c = rt.addBorderNode(e, "_bt"), l = rt.addBorderNode(e, "_bb"), u = e.node(o);
	e.setParent(c, o), u.borderTop = c, e.setParent(l, o), u.borderBottom = l, s.forEach((s) => {
		Ot(e, t, n, r, i, a, s);
		let u = e.node(s), d = u.borderTop ? u.borderTop : s, f = u.borderBottom ? u.borderBottom : s, p = u.borderTop ? r : 2 * r, m = d === f ? i - a[o] + 1 : 1;
		e.setEdge(c, d, {
			weight: p,
			minlen: m,
			nestingEdge: !0
		}), e.setEdge(f, l, {
			weight: p,
			minlen: m,
			nestingEdge: !0
		});
	}), e.parent(o) || e.setEdge(t, c, {
		weight: 0,
		minlen: i + a[o]
	});
}
function kt(e) {
	let t = {};
	function n(r, i) {
		let a = e.children(r);
		a && a.length && a.forEach((e) => n(e, i + 1)), t[r] = i;
	}
	return e.children().forEach((e) => n(e, 1)), t;
}
function At(e) {
	return e.edges().reduce((t, n) => t + e.edge(n).weight, 0);
}
function jt(e) {
	let t = e.graph();
	e.removeNode(t.nestingRoot), delete t.nestingRoot, e.edges().forEach((t) => {
		e.edge(t).nestingEdge && e.removeEdge(t);
	});
}
//#endregion
//#region src/DAG/add-border-segments.js
function Mt(e) {
	function t(n) {
		let r = e.children(n), i = e.node(n);
		if (r.length && r.forEach(t), Object.hasOwn(i, "minRank")) {
			i.borderLeft = [], i.borderRight = [];
			for (let t = i.minRank, r = i.maxRank + 1; t < r; ++t) Nt(e, "borderLeft", "_bl", n, i, t), Nt(e, "borderRight", "_br", n, i, t);
		}
	}
	e.children().forEach(t);
}
function Nt(e, t, n, r, i, a) {
	let o = {
		width: 0,
		height: 0,
		rank: a,
		borderType: t
	}, s = i[t][a - 1], c = L(e, "border", o, n);
	i[t][a] = c, e.setParent(c, r), s && e.setEdge(s, c, { weight: 1 });
}
//#endregion
//#region src/DAG/coordinate-system.js
function Pt(e) {
	let t = e.graph().rankdir.toLowerCase();
	(t === "lr" || t === "rl") && It(e);
}
function Ft(e) {
	let t = e.graph().rankdir.toLowerCase();
	(t === "bt" || t === "rl") && Lt(e), (t === "lr" || t === "rl") && (zt(e), It(e));
}
function It(e) {
	e.nodes().forEach((t) => {
		J(e.node(t));
	}), e.edges().forEach((t) => {
		J(e.edge(t));
	});
}
function J(e) {
	let t = e.width;
	e.width = e.height, e.height = t;
}
function Lt(e) {
	e.nodes().forEach((t) => {
		Rt(e.node(t));
	}), e.edges().forEach((t) => {
		let n = e.edge(t);
		n.points.forEach(Rt), Object.hasOwn(n, "y") && Rt(n);
	});
}
function Rt(e) {
	e.y = -e.y;
}
function zt(e) {
	e.nodes().forEach((t) => {
		Bt(e.node(t));
	}), e.edges().forEach((t) => {
		let n = e.edge(t);
		n.points.forEach(Bt), Object.hasOwn(n, "x") && Bt(n);
	});
}
function Bt(e) {
	let t = e.x;
	e.x = e.y, e.y = t;
}
var Vt = {
	adjust: Pt,
	undo: Ft
};
//#endregion
//#region src/DAG/order/init-order.js
function Ht(e) {
	let t = {}, n = e.nodes().filter((t) => e.children(t).length === 0), r = n.map((t) => e.node(t).rank), i = nt(B(Math.max, r) + 1).map(() => []);
	function a(n) {
		if (t[n]) return;
		t[n] = !0;
		let r = e.node(n);
		i[r.rank].push(n), e.successors(n).forEach(a);
	}
	return n.sort((t, n) => e.node(t).rank - e.node(n).rank).forEach(a), i;
}
//#endregion
//#region src/DAG/order/cross-count.js
function Ut(e, t) {
	let n = 0;
	for (let r = 1; r < t.length; ++r) n += Wt(e, t[r - 1], t[r]);
	return n;
}
function Wt(e, t, n) {
	let r = W(n, n.map((e, t) => t)), i = t.flatMap((t) => e.outEdges(t).map((t) => ({
		pos: r[t.w],
		weight: e.edge(t).weight
	})).sort((e, t) => e.pos - t.pos)), a = 1;
	for (; a < n.length;) a <<= 1;
	let o = 2 * a - 1;
	--a;
	let s = Array(o).fill(0), c = 0;
	return i.forEach((e) => {
		let t = e.pos + a;
		s[t] += e.weight;
		let n = 0;
		for (; t > 0;) t % 2 && (n += s[t + 1]), t = t - 1 >> 1, s[t] += e.weight;
		c += e.weight * n;
	}), c;
}
//#endregion
//#region src/DAG/order/barycenter.js
function Y(e, t = []) {
	return t.map((t) => {
		let n = e.inEdges(t);
		if (!n.length) return { v: t };
		let r = n.reduce((t, n) => {
			let r = e.edge(n), i = e.node(n.v);
			return {
				sum: t.sum + r.weight * i.order,
				weight: t.weight + r.weight
			};
		}, {
			sum: 0,
			weight: 0
		});
		return {
			v: t,
			barycenter: r.sum / r.weight,
			weight: r.weight
		};
	});
}
//#endregion
//#region src/DAG/order/resolve-conflicts.js
function Gt(e, t) {
	let n = {};
	return e.forEach((e, t) => {
		let r = n[e.v] = {
			indegree: 0,
			in: [],
			out: [],
			vs: [e.v],
			i: t
		};
		e.barycenter !== void 0 && (r.barycenter = e.barycenter, r.weight = e.weight);
	}), t.edges().forEach((e) => {
		let t = n[e.v], r = n[e.w];
		t !== void 0 && r !== void 0 && (r.indegree++, t.out.push(r));
	}), X(Object.values(n).filter((e) => !e.indegree));
}
function X(e) {
	let t = [];
	function n(e) {
		return (t) => {
			t.merged || (t.barycenter === void 0 || e.barycenter === void 0 || t.barycenter >= e.barycenter) && Kt(e, t);
		};
	}
	function r(t) {
		return (n) => {
			n.in.push(t), --n.indegree === 0 && e.push(n);
		};
	}
	for (; e.length;) {
		let i = e.pop();
		t.push(i), i.in.reverse().forEach(n(i)), i.out.forEach(r(i));
	}
	return t.filter((e) => !e.merged).map((e) => H(e, [
		"vs",
		"i",
		"barycenter",
		"weight"
	]));
}
function Kt(e, t) {
	let n = 0, r = 0;
	e.weight && (n += e.barycenter * e.weight, r += e.weight), t.weight && (n += t.barycenter * t.weight, r += t.weight), e.vs = t.vs.concat(e.vs), e.barycenter = n / r, e.weight = r, e.i = Math.min(t.i, e.i), t.merged = !0;
}
//#endregion
//#region src/DAG/order/sort.js
function qt(e, t) {
	let n = Ze(e, (e) => Object.hasOwn(e, "barycenter")), r = n.lhs, i = n.rhs.sort((e, t) => t.i - e.i), a = [], o = 0, s = 0, c = 0;
	r.sort(Yt(!!t)), c = Jt(a, i, c), r.forEach((e) => {
		c += e.vs.length, a.push(e.vs), o += e.barycenter * e.weight, s += e.weight, c = Jt(a, i, c);
	});
	let l = { vs: a.flat(!0) };
	return s && (l.barycenter = o / s, l.weight = s), l;
}
function Jt(e, t, n) {
	let r;
	for (; t.length && (r = t[t.length - 1]).i <= n;) t.pop(), e.push(r.vs), n++;
	return n;
}
function Yt(e) {
	return (t, n) => t.barycenter < n.barycenter ? -1 : t.barycenter > n.barycenter ? 1 : e ? n.i - t.i : t.i - n.i;
}
//#endregion
//#region src/DAG/order/sort-subgraph.js
function Xt(e, t, n, r) {
	let i = e.children(t), a = e.node(t), o = a ? a.borderLeft : void 0, s = a ? a.borderRight : void 0, c = {};
	o && (i = i.filter((e) => e !== o && e !== s));
	let l = Y(e, i);
	l.forEach((t) => {
		if (e.children(t.v).length) {
			let i = Xt(e, t.v, n, r);
			c[t.v] = i, Object.hasOwn(i, "barycenter") && Qt(t, i);
		}
	});
	let u = Gt(l, n);
	Zt(u, c);
	let d = qt(u, r);
	if (o && (d.vs = [
		o,
		d.vs,
		s
	].flat(!0), e.predecessors(o).length)) {
		let t = e.node(e.predecessors(o)[0]), n = e.node(e.predecessors(s)[0]);
		Object.hasOwn(d, "barycenter") || (d.barycenter = 0, d.weight = 0), d.barycenter = (d.barycenter * d.weight + t.order + n.order) / (d.weight + 2), d.weight += 2;
	}
	return d;
}
function Zt(e, t) {
	e.forEach((e) => {
		e.vs = e.vs.flatMap((e) => t[e] ? t[e].vs : e);
	});
}
function Qt(e, t) {
	e.barycenter === void 0 ? (e.barycenter = t.barycenter, e.weight = t.weight) : (e.barycenter = (e.barycenter * e.weight + t.barycenter * t.weight) / (e.weight + t.weight), e.weight += t.weight);
}
//#endregion
//#region src/DAG/order/build-layer-graph.js
function $t(e, t, n, r) {
	r ||= e.nodes();
	let i = en(e), a = new k({ compound: !0 }).setGraph({ root: i }).setDefaultNodeLabel((t) => e.node(t));
	return r.forEach((r) => {
		let o = e.node(r), s = e.parent(r);
		(o.rank === t || o.minRank <= t && t <= o.maxRank) && (a.setNode(r), a.setParent(r, s || i), e[n](r).forEach((t) => {
			let n = t.v === r ? t.w : t.v, i = a.edge(n, r), o = i ? i.weight : 0;
			a.setEdge(n, r, { weight: e.edge(t).weight + o });
		}), Object.hasOwn(o, "minRank") && a.setNode(r, {
			borderLeft: o.borderLeft[t],
			borderRight: o.borderRight[t]
		}));
	}), a;
}
function en(e) {
	let t;
	for (; e.hasNode(t = tt("_root")););
	return t;
}
//#endregion
//#region src/DAG/order/add-subgraph-constraints.js
function tn(e, t, n) {
	let r = {}, i;
	n.forEach((n) => {
		let a = e.parent(n), o, s;
		for (; a;) {
			if (o = e.parent(a), o ? (s = r[o], r[o] = a) : (s = i, i = a), s && s !== a) {
				t.setEdge(s, a);
				return;
			}
			a = o;
		}
	});
}
//#endregion
//#region src/DAG/order/index.js
function Z(e, t = {}) {
	if (typeof t.customOrder == "function") {
		t.customOrder(e, Z);
		return;
	}
	let n = V(e), r = nn(e, nt(1, n + 1), "inEdges"), i = nn(e, nt(n - 1, -1, -1), "outEdges"), a = Ht(e);
	if (an(e, a), t.disableOptimalOrderHeuristic) return;
	let o = Infinity, s, c = t.constraints || [];
	for (let t = 0, n = 0; n < 4; ++t, ++n) {
		rn(t % 2 == 0 ? r : i, t % 4 >= 2, c), a = Ke(e);
		let l = Ut(e, a);
		l < o ? (o = l, n = 0, s = structuredClone(a)) : l === o && (s = structuredClone(a));
	}
	an(e, s);
}
function nn(e, t, n) {
	let r = /* @__PURE__ */ new Map();
	function i(e, t) {
		r.has(e) || r.set(e, []), r.get(e).push(t);
	}
	for (let t of e.nodes()) {
		let n = e.node(t);
		if (typeof n.rank == "number" && i(n.rank, t), typeof n.minRank == "number" && typeof n.maxRank == "number") for (let e = n.minRank; e <= n.maxRank; e++) e !== n.rank && i(e, t);
	}
	return t.map((t) => $t(e, t, n, r.get(t) || []));
}
function rn(e, t, n) {
	let r = new k();
	e.forEach((e) => {
		n.forEach((e) => r.setEdge(e.left, e.right));
		let i = e.graph().root, a = Xt(e, i, r, t);
		a.vs.forEach((t, n) => {
			e.node(t).order = n;
		}), tn(e, r, a.vs);
	});
}
function an(e, t) {
	Object.values(t).forEach((t) => {
		t.forEach((t, n) => {
			e.node(t).order = n;
		});
	});
}
//#endregion
//#region src/DAG/position/bk.js
function Q(e, t) {
	let n = {};
	function r(t, r) {
		let i = 0, a = 0, o = t.length, s = r[r.length - 1];
		return r.forEach((t, c) => {
			let l = sn(e, t), u = l ? e.node(l).order : o;
			(l || t === s) && (r.slice(a, c + 1).forEach((t) => {
				e.predecessors(t).forEach((r) => {
					let a = e.node(r), o = a.order;
					(o < i || u < o) && !(a.dummy && e.node(t).dummy) && cn(n, r, t);
				});
			}), a = c + 1, i = u);
		}), r;
	}
	return t.length && t.reduce(r), n;
}
function on(e, t) {
	let n = {};
	function r(t, r, i, a, o) {
		let s;
		nt(r, i).forEach((r) => {
			s = t[r], e.node(s).dummy && e.predecessors(s).forEach((t) => {
				let r = e.node(t);
				r.dummy && (r.order < a || r.order > o) && cn(n, t, s);
			});
		});
	}
	function i(t, n) {
		let i = -1, a, o = 0;
		return n.forEach((s, c) => {
			if (e.node(s).dummy === "border") {
				let t = e.predecessors(s);
				t.length && (a = e.node(t[0]).order, r(n, o, c, i, a), o = c, i = a);
			}
			r(n, o, n.length, a, t.length);
		}), n;
	}
	return t.length && t.reduce(i), n;
}
function sn(e, t) {
	if (e.node(t).dummy) return e.predecessors(t).find((t) => e.node(t).dummy);
}
function cn(e, t, n) {
	if (t > n) {
		let e = t;
		t = n, n = e;
	}
	let r = e[t];
	r || (e[t] = r = {}), r[n] = !0;
}
function ln(e, t, n) {
	if (t > n) {
		let e = t;
		t = n, n = e;
	}
	return !!e[t] && Object.hasOwn(e[t], n);
}
function un(e, t, n, r) {
	let i = {}, a = {}, o = {};
	return t.forEach((e) => {
		e.forEach((e, t) => {
			i[e] = e, a[e] = e, o[e] = t;
		});
	}), t.forEach((e) => {
		let t = -1;
		e.forEach((e) => {
			let s = r(e);
			if (s.length) {
				s = s.sort((e, t) => o[e] - o[t]);
				let r = (s.length - 1) / 2;
				for (let c = Math.floor(r), l = Math.ceil(r); c <= l; ++c) {
					let r = s[c];
					a[e] === e && t < o[r] && !ln(n, e, r) && (a[r] = e, a[e] = i[e] = i[r], t = o[r]);
				}
			}
		});
	}), {
		root: i,
		align: a
	};
}
function dn(e, t, n, r, i) {
	let a = {}, o = fn(e, t, n, i), s = i ? "borderLeft" : "borderRight";
	function c(e, t) {
		let n = o.nodes().slice(), r = {}, i = n.pop();
		for (; i;) {
			if (r[i]) e(i);
			else {
				r[i] = !0, n.push(i);
				for (let e of t(i)) n.push(e);
			}
			i = n.pop();
		}
	}
	function l(e) {
		a[e] = o.inEdges(e).reduce((e, t) => Math.max(e, a[t.v] + o.edge(t)), 0);
	}
	function u(t) {
		let n = o.outEdges(t).reduce((e, t) => Math.min(e, a[t.w] - o.edge(t)), Infinity), r = e.node(t);
		n !== Infinity && r.borderType !== s && (a[t] = Math.max(a[t], n));
	}
	return c(l, o.predecessors.bind(o)), c(u, o.successors.bind(o)), Object.keys(r).forEach((e) => {
		a[e] = a[n[e]];
	}), a;
}
function fn(e, t, n, r) {
	let i = new e.constructor(), a = e.graph(), o = _n(a.nodesep, a.edgesep, r);
	return t.forEach((t) => {
		let r;
		t.forEach((t) => {
			let a = n[t];
			if (i.setNode(a), r) {
				let s = n[r], c = i.edge(s, a);
				i.setEdge(s, a, Math.max(o(e, t, r), c || 0));
			}
			r = t;
		});
	}), i;
}
function pn(e, t) {
	return Object.values(t).reduce((t, n) => {
		let r = -Infinity, i = Infinity;
		Object.entries(n).forEach(([t, n]) => {
			let a = vn(e, t) / 2;
			r = Math.max(n + a, r), i = Math.min(n - a, i);
		});
		let a = r - i;
		return a < t[0] && (t = [a, n]), t;
	}, [Infinity, null])[1];
}
function mn(e, t) {
	let n = Object.values(t), r = B(Math.min, n), i = B(Math.max, n);
	["u", "d"].forEach((n) => {
		["l", "r"].forEach((a) => {
			let o = n + a, s = e[o];
			if (s === t) return;
			let c = Object.values(s), l = r - B(Math.min, c);
			a !== "l" && (l = i - B(Math.max, c)), l && (s = U(s, (e) => e + l), e[o] = s);
		});
	});
}
function hn(e, t) {
	return U(e.ul, (n, r) => {
		if (t) return e[t.toLowerCase()][r];
		let i = Object.values(e).map((e) => e[r]).sort((e, t) => e - t);
		return (i[1] + i[2]) / 2;
	});
}
function gn(e) {
	let t = Ke(e), n = Object.assign(Q(e, t), on(e, t)), r = {}, i;
	return ["u", "d"].forEach((a) => {
		i = a === "u" ? t : Object.values(t).reverse(), ["l", "r"].forEach((t) => {
			let o = i;
			t === "r" && (o = o.map((e) => Object.values(e).reverse()));
			let s = (a === "u" ? e.predecessors : e.successors).bind(e), c = un(e, o, n, s), l = dn(e, o, c.root, c.align, t === "r");
			t === "r" && (l = U(l, (e) => -e)), r[a + t] = l;
		});
	}), mn(r, pn(e, r)), hn(r, e.graph().align);
}
function _n(e, t, n) {
	return (r, i, a) => {
		let o = r.node(i), s = r.node(a), c = 0, l;
		if (c += o.width / 2, Object.hasOwn(o, "labelpos")) switch (o.labelpos.toLowerCase()) {
			case "l":
				l = -o.width / 2;
				break;
			case "r": l = o.width / 2;
		}
		if (l && (c += n ? l : -l), l = 0, c += (o.dummy ? t : e) / 2, c += (s.dummy ? t : e) / 2, c += s.width / 2, Object.hasOwn(s, "labelpos")) switch (s.labelpos.toLowerCase()) {
			case "l":
				l = s.width / 2;
				break;
			case "r": l = -s.width / 2;
		}
		return l && (c += n ? l : -l), l = 0, c;
	};
}
function vn(e, t) {
	return e.node(t).width;
}
//#endregion
//#region src/DAG/position/index.js
function yn(e) {
	let t = Ue(e);
	bn(t);
	let n = gn(t);
	Object.entries(n).forEach(([t, n]) => {
		e.node(t).x = n;
	});
}
function bn(e) {
	let t = Ke(e), n = e.graph().ranksep, r = 0;
	t.forEach((t) => {
		let i = t.reduce((t, n) => {
			let r = e.node(n).height;
			return t > r ? t : r;
		}, 0);
		t.forEach((t) => {
			e.node(t).y = r + i / 2;
		}), r += i + n;
	});
}
//#endregion
//#region src/DAG/layout.js
function xn(e, t = {}) {
	let n = t.debugTiming ? Qe : $e;
	return n("layout", () => {
		let r = n("  buildLayoutGraph", () => jn(e));
		return n("  runLayout", () => $(r, n, t)), n("  updateInputGraph", () => Sn(e, r)), r;
	});
}
function $(e, t, n) {
	t("    makeSpaceForEdgeLabels", () => Mn(e)), t("    removeSelfEdges", () => Hn(e)), t("    acyclic", () => it(e)), t("    nestingGraph.run", () => Dt(e)), t("    rank", () => St(Ue(e))), t("    injectEdgeLabelProxies", () => Nn(e)), t("    removeEmptyRanks", () => Je(e)), t("    nestingGraph.cleanup", () => jt(e)), t("    normalizeRanks", () => qe(e)), t("    assignRankMinMax", () => Pn(e)), t("    removeEdgeLabelProxies", () => Fn(e)), t("    normalize.run", () => ot(e)), t("    parentDummyChains", () => Tt(e)), t("    addBorderSegments", () => Mt(e)), t("    order", () => Z(e, n)), t("    insertSelfEdges", () => Un(e)), t("    adjustCoordinateSystem", () => Vt.adjust(e)), t("    position", () => yn(e)), t("    positionSelfEdges", () => Wn(e)), t("    removeBorderNodes", () => Vn(e)), t("    normalize.undo", () => ct(e)), t("    fixupEdgeLabelCoords", () => zn(e)), t("    undoCoordinateSystem", () => Vt.undo(e)), t("    translateGraph", () => In(e)), t("    assignNodeIntersects", () => Rn(e)), t("    reversePoints", () => Bn(e)), t("    acyclic.undo", () => at(e));
}
function Sn(e, t) {
	e.nodes().forEach((n) => {
		let r = e.node(n), i = t.node(n);
		r && (r.x = i.x, r.y = i.y, r.order = i.order, r.rank = i.rank, t.children(n).length && (r.width = i.width, r.height = i.height));
	}), e.edges().forEach((n) => {
		let r = e.edge(n), i = t.edge(n);
		r.points = i.points, Object.hasOwn(i, "x") && (r.x = i.x, r.y = i.y);
	});
	let n = t.graph(), r = e.graph();
	r.width = n.width, r.height = n.height;
}
var Cn = [
	"nodesep",
	"edgesep",
	"ranksep",
	"marginx",
	"marginy"
], wn = {
	ranksep: 50,
	edgesep: 20,
	nodesep: 50,
	rankdir: "tb"
}, Tn = [
	"acyclicer",
	"ranker",
	"rankdir",
	"align"
], En = [
	"width",
	"height",
	"rank"
], Dn = {
	width: 0,
	height: 0
}, On = [
	"minlen",
	"weight",
	"width",
	"height",
	"labeloffset"
], kn = {
	minlen: 1,
	weight: 1,
	width: 0,
	height: 0,
	labeloffset: 10,
	labelpos: "r"
}, An = ["labelpos", "arrowshape"];
function jn(e) {
	let t = Kn(e.graph()), n = {
		...wn,
		...Gn(t, Cn),
		...H(t, Tn)
	}, r = new k({
		multigraph: !0,
		compound: !0
	});
	return r.setGraph(n), e.nodes().forEach((t) => {
		let n = Gn(Kn(e.node(t)), En);
		Object.keys(Dn).forEach((e) => {
			n[e] === void 0 && (n[e] = Dn[e]);
		}), r.setNode(t, n), r.setParent(t, e.parent(t));
	}), e.edges().forEach((t) => {
		let n = Kn(e.edge(t));
		r.setEdge(t, {
			...kn,
			...Gn(n, On),
			...H(n, An)
		});
	}), r;
}
function Mn(e) {
	let t = e.graph();
	t.ranksep /= 2, e.edges().forEach((n) => {
		let r = e.edge(n);
		r.minlen *= 2, r.labelpos.toLowerCase() !== "c" && (t.rankdir === "TB" || t.rankdir === "BT" ? r.width += r.labeloffset : r.height += r.labeloffset);
	});
}
function Nn(e) {
	e.edges().forEach((t) => {
		let n = e.edge(t);
		if (n.width && n.height) {
			let n = e.node(t.v);
			L(e, "edge-proxy", {
				rank: (e.node(t.w).rank - n.rank) / 2 + n.rank,
				e: t
			}, "_ep");
		}
	});
}
function Pn(e) {
	let t = 0;
	e.nodes().forEach((n) => {
		let r = e.node(n);
		r.borderTop && (r.minRank = e.node(r.borderTop).rank, r.maxRank = e.node(r.borderBottom).rank, t = Math.max(t, r.maxRank));
	}), e.graph().maxRank = t;
}
function Fn(e) {
	e.nodes().forEach((t) => {
		let n = e.node(t);
		n.dummy === "edge-proxy" && (e.edge(n.e).labelRank = n.rank, e.removeNode(t));
	});
}
function In(e) {
	let t = Infinity, n = 0, r = Infinity, i = 0, a = e.graph(), o = a.marginx || 0, s = a.marginy || 0;
	function c(e) {
		let a = e.x, o = e.y, s = e.width, c = e.height;
		t = Math.min(t, a - s / 2), n = Math.max(n, a + s / 2), r = Math.min(r, o - c / 2), i = Math.max(i, o + c / 2);
	}
	e.nodes().forEach((t) => c(e.node(t))), e.edges().forEach((t) => {
		let n = e.edge(t);
		Object.hasOwn(n, "x") && c(n);
	}), t -= o, r -= s, e.nodes().forEach((n) => {
		let i = e.node(n);
		i.x -= t, i.y -= r;
	}), e.edges().forEach((n) => {
		let i = e.edge(n);
		i.points.forEach((e) => {
			e.x -= t, e.y -= r;
		}), Object.hasOwn(i, "x") && (i.x -= t), Object.hasOwn(i, "y") && (i.y -= r);
	}), a.width = n - t + o, a.height = i - r + s;
}
function Ln(e, t, n) {
	if (!n || !t) return e;
	let r = t.x - e.x, i = t.y - e.y, a = Math.sqrt(r * r + i * i);
	if (!a || a <= n) return e;
	let o = n / a;
	return {
		x: e.x + r * o,
		y: e.y + i * o
	};
}
function Rn(e) {
	e.edges().forEach((t) => {
		let n = e.edge(t), r = e.node(t.v), i = e.node(t.w), a, o;
		!n.points || !n.points.length ? (n.points = [], a = i, o = r) : (a = n.points[0], o = n.points[n.points.length - 1]);
		let s = Ge(r, a), c = Ge(i, o), l = n.points.length ? n.points[0] : a, u = n.points.length ? n.points[n.points.length - 1] : o, d = n.arrowshape, f = d === "normal" || d === "vee", p = !!n.reversed, m = s, h = c;
		f && (p ? m = Ln(s, l, 4) : h = Ln(c, u, 4)), n.points.unshift(m), n.points.push(h);
	});
}
function zn(e) {
	e.edges().forEach((t) => {
		let n = e.edge(t);
		if (Object.hasOwn(n, "x")) switch ((n.labelpos === "l" || n.labelpos === "r") && (n.width -= n.labeloffset), n.labelpos) {
			case "l":
				n.x -= n.width / 2 + n.labeloffset;
				break;
			case "r": n.x += n.width / 2 + n.labeloffset;
		}
	});
}
function Bn(e) {
	e.edges().forEach((t) => {
		let n = e.edge(t);
		n.reversed && n.points.reverse();
	});
}
function Vn(e) {
	e.nodes().forEach((t) => {
		if (e.children(t).length) {
			let n = e.node(t), r = e.node(n.borderTop), i = e.node(n.borderBottom), a = e.node(n.borderLeft[n.borderLeft.length - 1]), o = e.node(n.borderRight[n.borderRight.length - 1]);
			n.width = Math.abs(o.x - a.x), n.height = Math.abs(i.y - r.y), n.x = a.x + n.width / 2, n.y = r.y + n.height / 2;
		}
	}), e.nodes().forEach((t) => {
		e.node(t).dummy === "border" && e.removeNode(t);
	});
}
function Hn(e) {
	e.edges().forEach((t) => {
		if (t.v === t.w) {
			let n = e.node(t.v);
			n.selfEdges ||= [], n.selfEdges.push({
				e: t,
				label: e.edge(t)
			}), e.removeEdge(t);
		}
	});
}
function Un(e) {
	Ke(e).forEach((t) => {
		let n = 0;
		t.forEach((t, r) => {
			let i = e.node(t);
			i.order = r + n, (i.selfEdges || []).forEach((t) => {
				L(e, "selfedge", {
					width: t.label.width,
					height: t.label.height,
					rank: i.rank,
					order: r + ++n,
					e: t.e,
					label: t.label
				}, "_se");
			}), delete i.selfEdges;
		});
	});
}
function Wn(e) {
	e.nodes().forEach((t) => {
		let n = e.node(t);
		if (n.dummy === "selfedge") {
			let r = e.node(n.e.v), i = r.x + r.width / 2, a = r.y, o = n.x - i, s = r.height / 2;
			e.setEdge(n.e, n.label), e.removeNode(t), n.label.points = [
				{
					x: i + 2 * o / 3,
					y: a - s
				},
				{
					x: i + 5 * o / 6,
					y: a - s
				},
				{
					x: i + o,
					y: a
				},
				{
					x: i + 5 * o / 6,
					y: a + s
				},
				{
					x: i + 2 * o / 3,
					y: a + s
				}
			], n.label.x = n.x, n.label.y = n.y;
		}
	});
}
function Gn(e, t) {
	return U(H(e, t), Number);
}
function Kn(e) {
	let t = {};
	return e && Object.entries(e).forEach(([e, n]) => {
		let r = e;
		typeof r == "string" && (r = r.toLowerCase()), t[r] = n;
	}), t;
}
//#endregion
//#region src/useDag.js
var qn = {
	rankDirection: "TB",
	nodeSeparation: 50,
	rankSeparation: 50,
	edgeSeparation: 10,
	align: void 0,
	nodeWidth: 100,
	nodeHeight: 40,
	curvedEdges: !1,
	padding: 20,
	arrowShape: "normal",
	arrowSize: 10
};
function Jn(e) {
	return e.length ? e.map((e, t) => `${t === 0 ? "M" : "L"} ${e.x} ${e.y}`).join(" ") : "";
}
function Yn(e) {
	if (!e.length) return "";
	if (e.length === 1) {
		let t = e[0];
		return `M ${t.x} ${t.y}`;
	}
	if (e.length === 2) return `M ${e[0].x} ${e[0].y} L ${e[1].x} ${e[1].y}`;
	let [t] = e, n = `M ${t.x} ${t.y}`;
	for (let t = 1; t < e.length - 1; t += 1) {
		e[t - 1];
		let r = e[t], i = e[t + 1], a = r.x, o = r.y, s = (r.x + i.x) / 2, c = (r.y + i.y) / 2;
		n += ` Q ${a} ${o} ${s} ${c}`;
	}
	let r = e[e.length - 1];
	return n += ` L ${r.x} ${r.y}`, n;
}
function Xn(e) {
	let { nodes: t, edges: n, configuration: r } = e, a = T(null), s = T(null), c = `dag-arrow-${o()}`;
	function l(e, t, n) {
		s.value = null, a.value = null;
		let r = {
			...qn,
			...n
		}, l = new k({
			multigraph: !0,
			compound: !0
		});
		l.setGraph({
			rankdir: r.rankDirection,
			nodesep: r.nodeSeparation,
			ranksep: r.rankSeparation,
			edgesep: r.edgeSeparation,
			align: r.align
		}), e.forEach((e) => {
			l.setNode(e.id, {
				label: e.label,
				width: e.width ?? r.nodeWidth,
				height: e.height ?? r.nodeHeight
			});
		}), t.forEach((e) => {
			l.setEdge(e.from, e.to, {
				weight: e.weight ?? 1,
				minlen: e.minLength ?? 1,
				arrowShape: r.arrowShape ?? "normal"
			});
		}), xn(l);
		let u = e.map((e) => {
			let t = l.node(e.id);
			return {
				id: e.id,
				label: e.label,
				x: t.x,
				y: t.y,
				width: t.width,
				height: t.height,
				original: e
			};
		}), d = l.edges().map((e) => {
			let n = l.edge(e), i = n.points || [];
			if (!i.length) return null;
			let a = r.curvedEdges ? Yn(i) : Jn(i), s = r.arrowShape === "undirected" ? null : `url(#${c})`, u = t.find((t) => t?.from === e.v && t?.to === e.w);
			return {
				id: `${e.v}->${e.w}->${o()}`,
				from: e.v,
				to: e.w,
				points: i,
				pathData: a,
				markerEnd: s,
				original: {
					...u,
					...n
				}
			};
		}).filter(Boolean), f = r.padding;
		if (!u.length) {
			a.value = {
				nodes: [],
				edges: d,
				viewBox: "0 0 0 0",
				arrowShape: r.arrowShape,
				arrowSize: r.arrowSize
			};
			return;
		}
		let p = u.flatMap((e) => [e.x - e.width / 2, e.x + e.width / 2]), m = u.flatMap((e) => [e.y - e.height / 2, e.y + e.height / 2]), h = Math.min(...p) - f, g = Math.max(...p) + f, ee = Math.min(...m) - f, te = Math.max(...m) + f;
		a.value = {
			nodes: u,
			edges: d.map((e) => ({
				...e,
				midpoint: e.pathData ? i(e.pathData) : {
					x: 0,
					y: 0
				}
			})),
			viewBox: `${h} ${ee} ${g - h} ${te - ee}`,
			arrowShape: r.arrowShape,
			arrowSize: r.arrowSize
		};
	}
	return ke(() => {
		try {
			l(D(t) || [], D(n) || [], D(r) || {});
		} catch (e) {
			console.error("[useDag] layout error:", e), s.value = e, a.value = null;
		}
	}), {
		layoutData: a,
		lastError: s,
		arrowMarkerIdentifier: c,
		recomputeLayout: () => {
			l(D(t) || [], D(n) || [], D(r) || {});
		}
	};
}
//#endregion
//#region src/components/vue-ui-dag.vue
var Zn = /* @__PURE__ */ e({ default: () => jr }), Qn = ["id"], $n = ["id"], er = {
	key: 1,
	class: "dag-chart-error"
}, tr = { style: { position: "relative" } }, nr = [
	"viewBox",
	"xmlns",
	"aria-describedby"
], rr = { key: 0 }, ir = [
	"id",
	"width",
	"height"
], ar = [
	"cx",
	"cy",
	"r",
	"fill"
], or = [
	"x",
	"y",
	"width",
	"height",
	"fill"
], sr = { key: 2 }, cr = [
	"id",
	"markerWidth",
	"markerHeight",
	"refX",
	"refY"
], lr = [
	"d",
	"fill",
	"stroke"
], ur = [
	"d",
	"fill",
	"stroke"
], dr = { class: "vue-ui-dag-edges" }, fr = [
	"data-a11y-midpoint-id",
	"aria-label",
	"onMouseenter"
], pr = { class: "vue-ui-dag-nodes" }, mr = [
	"onClick",
	"onMouseenter",
	"onMouseleave"
], hr = ["data-a11y-node-id", "aria-label"], gr = [
	"x",
	"y",
	"width",
	"height"
], _r = { class: "vue-ui-dag-edges" }, vr = [
	"d",
	"stroke-width",
	"marker-end"
], yr = { class: "vue-ui-dag-node-labels" }, br = [
	"onClick",
	"onMouseenter",
	"onMouseleave"
], xr = [
	"x",
	"y",
	"font-size",
	"fill",
	"font-weight"
], Sr = [
	"x",
	"y",
	"font-size",
	"fill",
	"font-weight",
	"innerHTML"
], Cr = { key: 1 }, wr = {
	key: 1,
	style: {
		position: "absolute",
		top: "100%",
		left: "0",
		width: "100%"
	},
	"data-dom-to-png-ignore": "",
	"aria-hidden": "true"
}, Tr = {
	key: 6,
	class: "vue-data-ui-watermark"
}, Er = ["data-position"], Dr = { key: 0 }, Or = ["data-position"], kr = { key: 0 }, Ar = 1.5, jr = /*#__PURE__*/ re({
	__name: "vue-ui-dag",
	props: {
		dataset: {
			type: Object,
			default() {
				return {
					nodes: [],
					edges: []
				};
			}
		},
		config: {
			type: Object,
			default() {
				return {};
			}
		}
	},
	emits: [
		"onNodeClick",
		"onMidpointEnter",
		"onMidpointLeave",
		"copyAlt",
		"rotate"
	],
	setup(e, { expose: i, emit: re }) {
		let ke = ve(() => import("./PenAndPaper-DAE-tnEQ.js").then((e) => e.t)), je = ve(() => import("./UserOptions-Dt98of5H.js").then((e) => e.n)), Me = ve(() => import("./PackageVersion-CtRPcMPr.js").then((e) => e.t)), { vue_ui_dag: Ne } = d(), { isThemeValid: k, warnInvalidTheme: Pe } = g(), A = e, j = re, M = T(null), N = T(o()), Fe = T(null), Ie = T(null), Le = T(null), Re = T(0), ze = T(0), Be = T(!1), Ve = T(!1), P = T(null), F = T(null), I = T(null), L = T(null), He = T(null), Ue = T("pointer"), We = T(!1), R = T(ut());
		l({
			config: () => R.value,
			dataset: () => A.dataset,
			component: "VueUiDag",
			rules: [u.noHint]
		});
		let Ge = _(() => R.value.userOptions.useCursorPointer), Ke = T(R.value.style.chart.width), qe = T(R.value.style.chart.height), Je = T({
			x: 0,
			y: 0
		}), z = T(null), Ye = T(null), Xe = T({
			left: "0px",
			top: "0px"
		}), B = T("top"), V = T(!1), Ze = T({
			x: 0,
			y: 0
		}), Qe = T({
			x: 0,
			y: 0
		}), $e = T(null), et = T(null), tt = T({
			left: "0px",
			top: "0px"
		}), nt = T("top"), H = T(!1), U = T(!1), { svgRef: W } = ce({ config: R.value.style.chart.title }), { userOptionsVisible: rt, setUserOptionsVisibility: it, keepUserOptionState: at } = se({ config: R.value }), G = T(R.value.style.chart.layout.rankDirection), ot = _(() => t({
			defaultConfig: {
				userOptions: { show: !1 },
				style: { chart: {
					backgroundColor: "#99999930",
					nodes: {
						stroke: "#CCCCCC",
						backgroundColor: "#DDDDDD50"
					},
					edges: { stroke: "#CCCCCC" },
					midpoints: {
						stroke: "#CCCCCC",
						fill: "#CCCCCC"
					}
				} }
			},
			userConfig: R.value.skeletonConfig ?? {}
		})), { loading: st, FINAL_DATASET: ct, manualLoading: lt } = p({
			...De(A),
			FINAL_CONFIG: R,
			prepareConfig: ut,
			skeletonDataset: A.config?.skeletonDataset ?? {
				nodes: [
					{
						id: "A",
						label: ""
					},
					{
						id: "B",
						label: ""
					},
					{
						id: "C",
						label: ""
					}
				],
				edges: [{
					from: "A",
					to: "B"
				}, {
					from: "A",
					to: "C"
				}]
			},
			skeletonConfig: t({
				defaultConfig: R.value,
				userConfig: ot.value
			})
		});
		function ut() {
			let e = h({
				userConfig: A.config,
				defaultConfig: Ne
			}), t = e.theme;
			if (!t) return e;
			if (!k.value(e)) return Pe(e), e;
			let n = h({
				userConfig: de[t] || A.config,
				defaultConfig: e
			});
			return h({
				userConfig: A.config,
				defaultConfig: n
			});
		}
		let dt = _(() => R.value.debug);
		we(async () => {
			a(A.dataset) && (c({
				componentName: "VueUiDag",
				type: "dataset",
				debug: dt.value
			}), Ve.value = !1, lt.value = !0), (!A.dataset.nodes || A.dataset.nodes.length === 0) && (c({
				componentName: "VueUiDag",
				type: "datasetAttributeEmpty",
				property: "nodes",
				index: 0,
				debug: dt.value
			}), Ve.value = !1, lt.value = !0), Ve.value = !0, await be(), Xt();
		}), Oe(() => A.config, async (e) => {
			st.value || (R.value = ut()), rt.value = !R.value.userOptions.showOnChartHover, Re.value += 1, G.value = R.value.style.chart.layout.rankDirection, Ke.value = R.value.style.chart.width, qe.value = R.value.style.chart.height, Ot.value = R.value.style.chart.zoom.active, await be(), Xt();
		}, { deep: !0 });
		let { isPrinting: ft, isImaging: pt, generatePdf: mt, generateImage: ht } = f({
			elementId: `dag_${N.value}`,
			fileName: R.value.style.chart.title.text || "vue-ui-dag",
			options: R.value.userOptions.print
		}), gt = _(() => R.value.style.chart.backgroundColor), _t = _(() => R.value.style.chart.title), { isCallbackImaging: vt, isCallbackSvg: yt, generateSvg: bt, onGenerateImage: xt } = ee({
			svg: W,
			title: _t,
			legend: null,
			legendItems: null,
			backgroundColor: gt,
			getSvgCallback: () => R.value.userOptions.callbacks.svg,
			generateImage: ht
		});
		function St() {
			Be.value = !0, it(!0);
		}
		function Ct() {
			it(!1), Be.value = !1;
		}
		function wt({ tooltipRef: e, isVisibleRef: t, anchorRef: n, styleRef: r, placementRef: i, offsetRef: a, margin: o = 24 }) {
			return function() {
				let s = e.value;
				if (!s || !t.value) return;
				let c = s.getBoundingClientRect(), l = window.innerWidth, u = window.innerHeight, d = n.value.x, f = n.value.y, p = a?.value?.x ?? 0, m = a?.value?.y ?? 0, h = f - m - c.height - o, g = d - c.width / 2, ee = "top";
				if (h < o) {
					let e = f + m + o;
					e + c.height <= u - o ? (h = e, ee = "bottom") : (h = f - c.height / 2, h < o && (h = o), h + c.height > u - o && (h = u - c.height - o), ee = "center");
				}
				g < o && (g = o), g + c.width > l - o && (g = l - c.width - o);
				let te = g <= o, ne = g + c.width >= l - o;
				if ((te || ne) && ee !== "center") {
					let e, t, n = d - p - o;
					l - (d + p) - o >= n ? (e = "right", t = d + p + o) : (e = "left", t = d - p - o - c.width), t >= o && t + c.width <= l - o && (g = t, h = f - c.height / 2, h < o && (h = o), h + c.height > u - o && (h = u - c.height - o), ee = e);
				}
				i.value = ee, r.value = {
					left: `${g}px`,
					top: `${h}px`
				};
			};
		}
		let { layoutData: K, lastError: Tt, arrowMarkerIdentifier: Et } = Xn({
			nodes: _(() => ct.value.nodes.map((e) => ({
				...e,
				backgroundColor: e.backgroundColor ? r(e.backgroundColor) : R.value.style.chart.nodes.backgroundColor,
				color: e.color ? r(e.color) : R.value.style.chart.nodes.labels.color
			}))),
			edges: _(() => ct.value.edges),
			configuration: _(() => ({
				...R.value.style.chart.layout,
				rankDirection: G.value
			}))
		});
		function q(e) {
			return K.value ? K.value.nodes.find((t) => t.id === e) : null;
		}
		_(() => {
			if (!K.value) return [];
			let e = R.value.style.chart.edges.stroke, t = /* @__PURE__ */ new Set();
			return K.value.edges.forEach((n) => {
				t.add({
					id: n.id,
					from: n.from,
					to: n.to,
					color: n.original?.color || e
				});
			}), Array.from(t);
		});
		let Dt = _(() => {
			let e = Ke.value, t = qe.value, n = Number(e), r = Number(t), i = Number.isFinite(n) && n > 0, a = Number.isFinite(r) && r > 0;
			return !i && !a ? null : {
				width: i ? n : null,
				height: a ? r : null
			};
		}), Ot = T(R.value.style.chart.zoom.active), { viewBox: kt, resetZoom: At, setInitialViewBox: jt, scale: Mt, zoomByFactor: Nt } = le(W, {
			x: 0,
			y: 0,
			width: 100,
			height: 100
		}, 1, Ot, () => {
			V.value = !1;
		});
		function Pt() {
			Ot.value = !Ot.value;
		}
		function Ft() {
			let e = K.value && K.value.viewBox;
			if (!e) return;
			let t = String(e).split(" ").map(Number);
			if (t.length !== 4) return;
			let [n, r, i, a] = t;
			if (!Number.isFinite(n) || !Number.isFinite(r) || !Number.isFinite(i) || !Number.isFinite(a)) return;
			let o = i, s = a, c = n, l = r, u = Dt.value;
			u && (u.width !== null && (o = u.width), u.height !== null && (s = u.height), c = n - (o - i) / 2, l = r - (s - a) / 2), jt({
				x: c,
				y: l,
				width: o,
				height: s
			}, { overwriteCurrentIfNotZoomed: !0 });
		}
		Oe(() => K.value && K.value.viewBox, () => {
			Ft();
		}, { immediate: !0 }), Oe(() => Dt.value, () => {
			Ft();
		}), Oe(() => U.value, (e) => {
			Ot.value = !e;
		});
		let It = _(() => {
			let e = kt.value;
			return e ? `${e.x} ${e.y} ${e.width} ${e.height}` : "0 0 0 0";
		}), J = T(!1);
		function Lt(e) {
			J.value = e, ze.value += 1;
		}
		function Rt() {
			U.value = !U.value;
		}
		function zt() {
			Nt(Ar, !0);
		}
		function Bt() {
			Nt(1 / Ar, !0);
		}
		let Vt = [
			"TB",
			"RL",
			"BT",
			"LR"
		];
		function Ht() {
			G.value = Vt[(Vt.indexOf(G.value) + 1) % Vt.length], At(), j("rotate", G.value);
		}
		let Ut = wt({
			tooltipRef: Ye,
			isVisibleRef: H,
			anchorRef: Je,
			styleRef: Xe,
			placementRef: B,
			isNode: !1
		}), Wt = wt({
			tooltipRef: et,
			isVisibleRef: V,
			anchorRef: Ze,
			styleRef: tt,
			placementRef: nt,
			offsetRef: Qe
		}), Y = T(null);
		async function Gt(e) {
			j("onMidpointEnter", e);
			let t = W.value;
			if (!t || !e?.midpoint) return;
			let n = t.createSVGPoint();
			n.x = e.midpoint.x, n.y = e.midpoint.y;
			let r = t.getScreenCTM();
			if (!r) return;
			let i = n.matrixTransform(r);
			Je.value = {
				x: i.x,
				y: i.y
			}, z.value = e, H.value = !0, R.value.style.chart.midpoints.selectedEdge.animated === !0 && (Y.value = e.id, Z()), await be(), Ut();
		}
		function X() {
			H.value = !1, z.value = null, j("onMidpointLeave"), R.value.style.chart.midpoints.selectedEdge.animated === !0 && (Y.value = null, Z());
		}
		async function Kt(e) {
			if (j("onNodeClick", e), !R.value.style.chart.nodes.tooltip.showOnClick) return;
			let t = W.value;
			if (!t) return;
			let n = t.createSVGPoint();
			n.x = e.x, n.y = e.y;
			let r = t.getScreenCTM();
			if (!r) return;
			let i = n.matrixTransform(r), a = R.value.style.chart.layout.nodeWidth, o = R.value.style.chart.layout.nodeHeight, s = r.a, c = r.d, l = a * s, u = o * c;
			Qe.value = {
				x: l / 2,
				y: u / 2
			}, Ze.value = {
				x: i.x,
				y: i.y
			}, $e.value = e, V.value = !0, await be(), Wt();
		}
		function qt() {
			V.value = !1, $e.value = null;
		}
		function Jt(e) {
			if (!(V.value || H.value)) return;
			let t = et.value;
			if (t && t.contains(e.target)) return;
			let n = Ye.value;
			if (n && n.contains(e.target)) return;
			let r = W.value;
			if (r && r.contains(e.target)) {
				let t = e.target.closest(".vue-ui-dag-node"), n = e.target.closest(".vue-ui-dag-edge-midpoint");
				if (t || n) return;
			}
			qt(), X(), We.value || On();
		}
		function Yt(e) {
			e.key === "Escape" && (V.value && qt(), H.value && X());
		}
		we(() => {
			document.addEventListener("mousedown", Jt), document.addEventListener("keydown", Yt);
		}), Ce(() => {
			document.removeEventListener("mousedown", Jt), document.removeEventListener("keydown", Yt), P.value && (F.value && P.value.unobserve(F.value), P.value.disconnect());
		});
		function Xt() {
			if (!R.value.responsive) {
				P.value && (F.value && P.value.unobserve(F.value), P.value.disconnect(), P.value = null, F.value = null);
				return;
			}
			let e = ie(() => {
				if (!M.value) return;
				let { width: e, height: t } = ae({
					chart: M.value,
					title: R.value.style.chart.title.text ? Fe.value : null,
					legend: R.value.style.chart.controls.show ? Le.value?.$el : null,
					source: Ie.value
				});
				requestAnimationFrame(() => {
					Ke.value = Math.max(.1, e), qe.value = Math.max(.1, t - 12);
				});
			});
			P.value && (F.value && P.value.unobserve(F.value), P.value.disconnect()), P.value = new ResizeObserver(e), F.value = M.value ? M.value.parentNode : null, F.value && P.value.observe(F.value), e();
		}
		function Zt(e, t = {}) {
			let { direction: n = -1, mode: r = "oneLapNearest", dasharray: i = null } = t;
			if (!e || typeof e.getTotalLength != "function") return dt.value && console.warn("VueUiDag @getIdealDashoffsetDelta: invalid path element", e), 0;
			let a = e.getTotalLength(), o = d(i ?? e.getAttribute("stroke-dasharray") ?? (typeof getComputedStyle == "function" ? getComputedStyle(e).strokeDasharray : ""));
			if (!Number.isFinite(o) || o <= 0) return n * a;
			let s = Math.max(1, Math.round(a / o)), c = Math.max(1, Math.ceil(a / o)), l = Math.max(1, Math.floor(a / o)), u;
			return u = r === "pattern" ? o : r === "oneLapCeil" ? c * o : r === "oneLapFloor" ? l * o : s * o, n * u;
			function d(e) {
				if (!e || e === "none") return NaN;
				let t = String(e).replace(/,/g, " ").trim().split(/\s+/).map((e) => Number.parseFloat(e)).filter((e) => Number.isFinite(e));
				if (!t.length) return NaN;
				let n = t.reduce((e, t) => e + t, 0);
				return t.length % 2 == 1 ? n * 2 : n;
			}
		}
		let Qt = T(/* @__PURE__ */ new Map()), $t = T(/* @__PURE__ */ new Map());
		function en(e) {
			return function(t) {
				t ? Qt.value.set(e, t) : Qt.value.delete(e);
			};
		}
		function tn() {
			$t.value.forEach((e) => {
				try {
					e.cancel();
				} catch {}
			}), $t.value.clear();
		}
		function Z() {
			tn();
			let e = K.value?.edges ?? [];
			if (!e.length) return;
			let t = R.value.style.chart.edges.animations, n = Number(t.referenceDistance) > 0 ? Number(t.referenceDistance) : 24;
			e.forEach((e) => {
				let r = R.value.style.chart.midpoints.selectedEdge.animated === !0 && Y.value != null && e.id === Y.value, i = !!e?.original?.animated || !!e?.animated || r, a = Qt.value.get(e.id);
				if (!a) return;
				if (!i) {
					a.style.strokeDasharray = "0", a.style.strokeDashoffset = "0";
					return;
				}
				let o = e?.original?.dasharray ?? t.dasharray;
				a.style.strokeDasharray = String(o), a.style.strokeDashoffset = "0";
				let s = ![void 0, null].includes(e?.original?.animationDirection), c = ![void 0, null].includes(t.animationDirection), l = Zt(a, {
					direction: s ? Number(e.original.animationDirection) : c ? Number(t.animationDirection) : -1,
					mode: "oneLapNearest",
					dasharray: String(o)
				}), u = e?.original?.animationDurationMs ?? t.animationDurationMs ?? 1e3, d = Number(u), f = Number.isFinite(d) && d > 0 ? n / d : n / 1e3, p = Math.max(1, Math.round(Math.abs(l) / Math.max(1e-9, f))), m = a.animate([{ strokeDashoffset: 0 }, { strokeDashoffset: l }], {
					duration: p,
					iterations: Infinity,
					easing: "linear"
				});
				$t.value.set(e.id, m);
			});
		}
		Oe(() => K.value && K.value.edges, async () => {
			await be(), Z();
		}, {
			deep: !0,
			immediate: !0
		}), Ce(() => {
			tn();
		});
		async function nn({ scale: e = 2 } = {}) {
			if (!M.value) return;
			let { width: t, height: n } = M.value.getBoundingClientRect(), r = t / n, { imageUri: i, base64: a } = await te({
				domElement: M.value,
				base64: !0,
				img: !0,
				scale: e
			});
			return {
				imageUri: i,
				base64: a,
				title: R.value.style.chart.title.text ?? "vue-ui-dag",
				width: t,
				height: n,
				aspectRatio: r
			};
		}
		let rn = _(() => {
			let e = Number(R.value.style.chart.layout.nodeHeight);
			return Number.isFinite(e) && e > 0 ? e / R.value.style.chart.backgroundPattern.spacingRatio : 12;
		}), an = _(() => rn.value * (R.value.style.chart.backgroundPattern.dotRadiusRatio / 100)), Q = T(null), on = null, sn = 0;
		function cn(e) {
			on &&= (clearTimeout(on), null), Q.value !== e && (Q.value = e, (R.value.style.chart.nodes.selected.downstreamEdges.animated === !0 || R.value.style.chart.nodes.selected.upstreamEdges.animated === !0) && Z());
		}
		function ln(e) {
			Ue.value = "pointer", Y.value = null, cn(e);
		}
		async function un(e) {
			Ue.value = "pointer", Q.value = null, await Gt(e);
		}
		function dn(e) {
			let t = ++sn;
			on && clearTimeout(on), on = setTimeout(() => {
				t === sn && Q.value === e && (Q.value = null, (R.value.style.chart.nodes.selected.downstreamEdges.animated === !0 || R.value.style.chart.nodes.selected.upstreamEdges.animated === !0) && Z());
			}, 20);
		}
		function fn(e) {
			let t = e.from === Q.value, n = e.to === Q.value, r = L.value === "midpoint" && I.value === e.id, i = Y.value === e.id || z.value?.id === e.id || r, a = e.original.color ?? R.value.style.chart.edges.stroke;
			return i && R.value.style.chart.midpoints.selectedEdge.stroke != null ? a = R.value.style.chart.midpoints.selectedEdge.stroke : t && R.value.style.chart.nodes.selected.downstreamEdges.stroke != null ? a = R.value.style.chart.nodes.selected.downstreamEdges.stroke : n && R.value.style.chart.nodes.selected.upstreamEdges.stroke != null && (a = R.value.style.chart.nodes.selected.upstreamEdges.stroke), e.animated = t && R.value.style.chart.nodes.selected.downstreamEdges.animated === !0 || n && R.value.style.chart.nodes.selected.upstreamEdges.animated === !0 || r && R.value.style.chart.midpoints.selectedEdge.animated === !0 ? !0 : (K.value?.edges.find((t) => t.id === e.id))?.original?.animated ?? !1, {
				d: e.pathData,
				fill: "none",
				stroke: a,
				"stroke-width": R.value.style.chart.edges.strokeWidth * (i || e.from === Q.value ? 2 : 1),
				"stroke-linecap": "round",
				"stroke-linejoin": "round"
			};
		}
		function pn(e) {
			return L.value === "node" && I.value === e;
		}
		function mn(e) {
			let t = Q.value === e.id, n = L.value === "node" && I.value === e.id, r = t || n, i = r && R.value.style.chart.nodes.selected.backgroundColor != null ? R.value.style.chart.nodes.selected.backgroundColor : e.original.backgroundColor, a = r && R.value.style.chart.nodes.selected.stroke != null ? R.value.style.chart.nodes.selected.stroke : R.value.style.chart.nodes.stroke, o = r && R.value.style.chart.nodes.selected.strokeWidth != null ? R.value.style.chart.nodes.selected.strokeWidth : R.value.style.chart.nodes.strokeWidth;
			return {
				x: e.x - e.width / 2,
				y: e.y - e.height / 2,
				width: e.width,
				height: e.height,
				rx: R.value.style.chart.nodes.borderRadius,
				fill: i,
				stroke: a,
				"stroke-width": o
			};
		}
		function hn(e) {
			let t = e.from === Q.value, n = e.to === Q.value, r = L.value === "midpoint" && I.value === e.id, i = Y.value === e.id || z.value?.id === e.id || r, a = e.original.color ?? R.value.style.chart.edges.stroke;
			return i && R.value.style.chart.midpoints.selectedEdge.stroke != null ? a = R.value.style.chart.midpoints.selectedEdge.stroke : t && R.value.style.chart.nodes.selected.downstreamEdges.stroke != null ? a = R.value.style.chart.nodes.selected.downstreamEdges.stroke : n && R.value.style.chart.nodes.selected.upstreamEdges.stroke != null && (a = R.value.style.chart.nodes.selected.upstreamEdges.stroke), {
				cx: e?.midpoint?.x,
				cy: e?.midpoint?.y,
				r: R.value.style.chart.midpoints.radius,
				fill: r ? R.value.style.chart.midpoints.selectedEdge.stroke ?? R.value.style.chart.midpoints.fill : R.value.style.chart.midpoints.fill,
				stroke: a,
				"stroke-width": R.value.style.chart.edges.strokeWidth * (i || e.from === Q.value ? 2 : 1)
			};
		}
		function gn(e) {
			let t = e.from === Q.value, n = e.to === Q.value, r = L.value === "midpoint" && I.value === e.id, i = Y.value === e.id || z.value?.id === e.id || r, a = e.color ?? e.original?.color ?? R.value.style.chart.edges.stroke;
			return i && R.value.style.chart.midpoints.selectedEdge.stroke != null ? a = R.value.style.chart.midpoints.selectedEdge.stroke : t && R.value.style.chart.nodes.selected.downstreamEdges.stroke != null ? a = R.value.style.chart.nodes.selected.downstreamEdges.stroke : n && R.value.style.chart.nodes.selected.upstreamEdges.stroke != null && (a = R.value.style.chart.nodes.selected.upstreamEdges.stroke), a;
		}
		function _n(e) {
			return `${Et}-${String(e).replace(/[^a-zA-Z0-9_-]/g, "_")}`;
		}
		function vn() {
			return K.value;
		}
		async function yn() {
			if (j("copyAlt", {
				config: R.value,
				dataset: ct.value
			}), !R.value.userOptions.callbacks.altCopy) {
				console.warn("Vue Data UI - A callback must be set for `altCopy` in userOptions.");
				return;
			}
			await Promise.resolve(R.value.userOptions.callbacks.altCopy({
				config: R.value,
				dataset: ct.value
			}));
		}
		let bn = _(() => (K.value?.nodes ?? []).map((e) => ({
			id: e.id,
			type: "node",
			x: e.x,
			y: e.y,
			label: e.label ?? e.id,
			raw: e
		}))), xn = _(() => R.value.style.chart.midpoints.show ? (K.value?.edges ?? []).filter((e) => e?.midpoint).map((e) => ({
			id: e.id,
			type: "midpoint",
			x: e.midpoint.x,
			y: e.midpoint.y,
			label: `${q(e.from)?.label ?? e.from} → ${q(e.to)?.label ?? e.to}`,
			raw: e
		})) : []), $ = _(() => [...bn.value, ...xn.value].sort((e, t) => e.x === t.x ? e.y - t.y : e.x - t.x)), Sn = _(() => {
			let e = /* @__PURE__ */ new Map();
			return $.value.forEach((t, n) => {
				e.set(`${t.type}:${t.id}`, n);
			}), e;
		}), Cn = _(() => {
			let e = K.value?.nodes ?? [], t = K.value?.edges ?? [];
			return {
				headers: [
					R.value.a11y?.translations?.node ?? "Node",
					R.value.a11y?.translations?.parents ?? "Parents",
					R.value.a11y?.translations?.children ?? "Children"
				],
				rows: e.map((e) => {
					let n = t.filter((t) => t.to === e.id).map((e) => q(e.from)?.label ?? e.from).join(", "), r = t.filter((t) => t.from === e.id).map((e) => q(e.to)?.label ?? e.to).join(", ");
					return [
						e.label ?? e.id,
						n || "—",
						r || "—"
					];
				})
			};
		});
		function wn(e) {
			return `${e.type}:${e.id}`;
		}
		function Tn() {
			return I.value == null || L.value == null ? null : $.value.find((e) => e.id === I.value && e.type === L.value) ?? null;
		}
		async function En(e) {
			if (e) {
				if (e.type === "node") {
					X();
					return;
				}
				e.type === "midpoint" && (qt(), await Gt(e.raw));
			}
		}
		function Dn(e) {
			if (e) {
				if (I.value = e.id, L.value = e.type, He.value = Sn.value.get(wn(e)) ?? null, e.type === "node") {
					cn(e.id), Y.value = null;
					return;
				}
				e.type === "midpoint" && (Q.value = null, Y.value = e.id, R.value.style.chart.midpoints.selectedEdge.animated === !0 && Z());
			}
		}
		function On() {
			I.value = null, L.value = null, He.value = null, V.value || (Q.value = null), H.value || (Y.value = null, R.value.style.chart.midpoints.selectedEdge.animated === !0 && Z());
		}
		function kn() {
			We.value = !0, On();
		}
		function An() {
			We.value = !1, qt(), X(), On();
		}
		function jn(e, t, n) {
			let r = t.x - e.x, i = t.y - e.y;
			return n === "right" && r <= 0 || n === "left" && r >= 0 || n === "down" && i <= 0 || n === "up" && i >= 0 ? Infinity : n === "right" || n === "left" ? Math.abs(r) * 1e3 + Math.abs(i) : Math.abs(i) * 1e3 + Math.abs(r);
		}
		function Mn(e, t) {
			let n = $.value.filter((t) => wn(t) !== wn(e));
			if (!n.length) return null;
			let r = null, i = Infinity;
			return n.forEach((n) => {
				let a = jn(e, n, t);
				a < i && (i = a, r = n);
			}), r || (t === "right" || t === "down" ? $.value[0] ?? null : $.value[$.value.length - 1] ?? null);
		}
		async function Nn(e) {
			e && (Ue.value = "keyboard", e.type === "node" && (X(), await Kt(e.raw)));
		}
		async function Pn(e) {
			if (!W.value || U.value || document.activeElement !== W.value || !$.value.length) return;
			let t = e.key === "ArrowLeft", n = e.key === "ArrowRight", r = e.key === "ArrowUp", i = e.key === "ArrowDown", a = e.key === "Enter" || e.key === " ", o = e.key === "Escape";
			if (!t && !n && !r && !i && !a && !o) return;
			if (e.preventDefault(), e.stopPropagation(), o) {
				qt(), X(), On();
				return;
			}
			let s = Tn();
			if (a) {
				if (!s) return;
				s.type === "node" && await Nn(s);
				return;
			}
			if (!s) {
				let e = $.value[0];
				if (!e) return;
				Dn(e), await En(e);
				return;
			}
			let c = null;
			n ? c = Mn(s, "right") : t ? c = Mn(s, "left") : i ? c = Mn(s, "down") : r && (c = Mn(s, "up")), c && (qt(), X(), Dn(c), await En(c));
		}
		return i({
			getData: vn,
			getImage: nn,
			generatePdf: mt,
			generateSvg: bt,
			generateImage: ht,
			toggleAnnotator: Rt,
			toggleFullscreen: Lt,
			zoomIn: zt,
			zoomOut: Bt,
			resetZoom: At,
			switchDirection: Ht,
			copyAlt: yn
		}), (e, t) => (w(), b("div", {
			class: xe(`vue-data-ui-component vue-ui-dag ${J.value ? "vue-data-ui-wrapper-fullscreen" : ""} ${R.value.responsive ? "vue-ui-dag-responsive" : ""}`),
			id: `dag_${N.value}`,
			ref_key: "dagChart",
			ref: M,
			style: Se({
				fontFamily: R.value.style.fontFamily,
				backgroundColor: R.value.style.chart.backgroundColor,
				padding: "0.5rem"
			}),
			onMouseenter: St,
			onMouseleave: Ct
		}, [
			x("div", {
				id: `chart-instructions-${N.value}`,
				class: "sr-only"
			}, [x("p", null, Ee(R.value.a11y.translations.keyboardNavigation), 1)], 8, $n),
			Cn.value?.rows?.length ? (w(), v(oe, {
				key: 0,
				uid: N.value,
				head: Cn.value.headers,
				body: Cn.value.rows,
				notice: R.value.a11y.translations.tableAvailable,
				caption: R.value.a11y.translations.tableCaption
			}, null, 8, [
				"uid",
				"head",
				"body",
				"notice",
				"caption"
			])) : y("", !0),
			D(Tt) ? (w(), b("div", er, Ee(String(D(Tt))), 1)) : y("", !0),
			R.value.userOptions.buttons.annotator ? (w(), v(D(ke), {
				key: 2,
				svgRef: D(W),
				backgroundColor: R.value.style.chart.backgroundColor,
				color: R.value.style.chart.color,
				active: U.value,
				isCursorPointer: Ge.value,
				onClose: Rt
			}, {
				"annotator-action-close": O(() => [E(e.$slots, "annotator-action-close", {}, void 0, !0)]),
				"annotator-action-color": O(({ color: t }) => [E(e.$slots, "annotator-action-color", C(S({ color: t })), void 0, !0)]),
				"annotator-action-draw": O(({ mode: t }) => [E(e.$slots, "annotator-action-draw", C(S({ mode: t })), void 0, !0)]),
				"annotator-action-undo": O(({ disabled: t }) => [E(e.$slots, "annotator-action-undo", C(S({ disabled: t })), void 0, !0)]),
				"annotator-action-redo": O(({ disabled: t }) => [E(e.$slots, "annotator-action-redo", C(S({ disabled: t })), void 0, !0)]),
				"annotator-action-delete": O(({ disabled: t }) => [E(e.$slots, "annotator-action-delete", C(S({ disabled: t })), void 0, !0)]),
				_: 3
			}, 8, [
				"svgRef",
				"backgroundColor",
				"color",
				"active",
				"isCursorPointer"
			])) : y("", !0),
			R.value.userOptions.show && Ve.value && (D(at) || D(rt)) ? (w(), v(D(je), {
				ref: "userOptionsRef",
				key: `user_option_${ze.value}`,
				backgroundColor: R.value.style.chart.backgroundColor,
				color: R.value.style.chart.color,
				isPrinting: D(ft),
				isImaging: D(pt),
				uid: N.value,
				hasTooltip: !1,
				hasTable: !1,
				hasXls: !1,
				hasLabel: !1,
				hasPdf: R.value.userOptions.buttons.pdf,
				hasImg: R.value.userOptions.buttons.img,
				hasSvg: R.value.userOptions.buttons.svg,
				hasFullscreen: R.value.userOptions.buttons.fullscreen,
				hasAltCopy: R.value.userOptions.buttons.altCopy,
				isFullscreen: J.value,
				chartElement: M.value,
				position: R.value.userOptions.position,
				titles: { ...R.value.userOptions.buttonTitles },
				hasAnnotator: R.value.userOptions.buttons.annotator,
				isAnnotation: U.value,
				callbacks: R.value.userOptions.callbacks,
				printScale: R.value.userOptions.print.scale,
				hasZoom: R.value.userOptions.buttons.zoom,
				isZoom: Ot.value,
				isCursorPointer: Ge.value,
				onToggleFullscreen: Lt,
				onGeneratePdf: D(mt),
				onGenerateImage: D(xt),
				onGenerateSvg: D(bt),
				onToggleAnnotator: Rt,
				onToggleZoom: Pt,
				onCopyAlt: yn,
				style: Se({ visibility: D(at) ? D(rt) ? "visible" : "hidden" : "visible" })
			}, he({ _: 2 }, [
				e.$slots.menuIcon ? {
					name: "menuIcon",
					fn: O(({ isOpen: t, color: n }) => [E(e.$slots, "menuIcon", C(S({
						isOpen: t,
						color: n
					})), void 0, !0)]),
					key: "0"
				} : void 0,
				e.$slots.optionPdf ? {
					name: "optionPdf",
					fn: O(() => [E(e.$slots, "optionPdf", {}, void 0, !0)]),
					key: "1"
				} : void 0,
				e.$slots.optionImg ? {
					name: "optionImg",
					fn: O(() => [E(e.$slots, "optionImg", {}, void 0, !0)]),
					key: "2"
				} : void 0,
				e.$slots.optionSvg ? {
					name: "optionSvg",
					fn: O(() => [E(e.$slots, "optionSvg", {}, void 0, !0)]),
					key: "3"
				} : void 0,
				e.$slots.optionFullscreen ? {
					name: "optionFullscreen",
					fn: O(({ toggleFullscreen: t, isFullscreen: n }) => [E(e.$slots, "optionFullscreen", C(S({
						toggleFullscreen: t,
						isFullscreen: n
					})), void 0, !0)]),
					key: "4"
				} : void 0,
				e.$slots.optionAnnotator ? {
					name: "optionAnnotator",
					fn: O(({ toggleAnnotator: t, isAnnotator: n }) => [E(e.$slots, "optionAnnotator", C(S({
						toggleAnnotator: t,
						isAnnotator: n
					})), void 0, !0)]),
					key: "5"
				} : void 0,
				e.$slots.optionZoom ? {
					name: "optionZoom",
					fn: O(({ toggleZoom: t, isZoomLocked: n }) => [E(e.$slots, "optionZoom", C(S({
						toggleZoom: t,
						isZoomLocked: n
					})), void 0, !0)]),
					key: "6"
				} : void 0,
				e.$slots.optionAltCopy ? {
					name: "optionAltCopy",
					fn: O(({ altCopy: t }) => [E(e.$slots, "optionAltCopy", C(S({ altCopy: t })), void 0, !0)]),
					key: "7"
				} : void 0,
				e.$slots["custom-menu-before"] ? {
					name: "custom-menu-before",
					fn: O(() => [E(e.$slots, "custom-menu-before", {}, void 0, !0)]),
					key: "8"
				} : void 0,
				e.$slots["custom-menu-after"] ? {
					name: "custom-menu-after",
					fn: O(() => [E(e.$slots, "custom-menu-after", {}, void 0, !0)]),
					key: "9"
				} : void 0
			]), 1032, [
				"backgroundColor",
				"color",
				"isPrinting",
				"isImaging",
				"uid",
				"hasPdf",
				"hasImg",
				"hasSvg",
				"hasFullscreen",
				"hasAltCopy",
				"isFullscreen",
				"chartElement",
				"position",
				"titles",
				"hasAnnotator",
				"isAnnotation",
				"callbacks",
				"printScale",
				"hasZoom",
				"isZoom",
				"isCursorPointer",
				"onGeneratePdf",
				"onGenerateImage",
				"onGenerateSvg",
				"style"
			])) : y("", !0),
			R.value.style.chart.title.text ? (w(), b("div", {
				key: 4,
				ref_key: "chartTitle",
				ref: Fe,
				style: "width:100%;background:transparent;"
			}, [(w(), v(ne, {
				key: `title_${Re.value}`,
				config: {
					title: {
						cy: "dag-title",
						...R.value.style.chart.title
					},
					subtitle: {
						cy: "dag-subtitle",
						...R.value.style.chart.title.subtitle
					}
				}
			}, null, 8, ["config"]))], 512)) : y("", !0),
			R.value.style.chart.controls.position === "top" && !D(st) && R.value.style.chart.controls.show ? (w(), v(ue, {
				key: 5,
				ref_key: "zoomControls",
				ref: Le,
				config: R.value,
				scale: D(Mt),
				isFullscreen: J.value,
				withDirection: "",
				onZoomIn: zt,
				onZoomOut: Bt,
				onResetZoom: t[0] ||= () => D(At)(!0),
				onSwitchDirection: Ht
			}, null, 8, [
				"config",
				"scale",
				"isFullscreen"
			])) : y("", !0),
			x("div", tr, [D(K) ? (w(), b("svg", {
				key: 0,
				ref_key: "svgRef",
				ref: W,
				class: xe({
					"vue-ui-dag-svg": !0,
					"vue-data-ui-loading": D(st)
				}),
				viewBox: It.value,
				xmlns: D(s),
				style: Se({
					backgroundColor: R.value.style.chart.backgroundColor,
					height: "100%",
					width: "100%"
				}),
				tabindex: "0",
				"aria-describedby": `chart-instructions-${N.value}`,
				onFocus: kn,
				onBlur: An,
				onKeydown: Pn
			}, [
				_e(D(Me)),
				R.value.style.chart.backgroundPattern.show ? (w(), b("defs", rr, [x("pattern", {
					id: `dag_bg_pattern_${N.value}`,
					patternUnits: "userSpaceOnUse",
					width: rn.value,
					height: rn.value
				}, [E(e.$slots, "background-pattern", C(S({
					x: rn.value / 2,
					y: rn.value / 2,
					color: R.value.style.chart.backgroundPattern.dotColor
				})), () => [x("circle", {
					cx: rn.value / 2,
					cy: rn.value / 2,
					r: an.value,
					fill: R.value.style.chart.backgroundPattern.dotColor
				}, null, 8, ar)], !0)], 8, ir)])) : y("", !0),
				R.value.style.chart.backgroundPattern.show ? (w(), b("rect", {
					key: 1,
					x: D(kt)?.x ?? 0,
					y: D(kt)?.y ?? 0,
					width: D(kt)?.width ?? 0,
					height: D(kt)?.height ?? 0,
					fill: `url(#dag_bg_pattern_${N.value})`,
					style: Se({
						pointerEvents: "none",
						opacity: R.value.style.chart.backgroundPattern.opacity
					})
				}, null, 12, or)) : y("", !0),
				D(K).arrowShape === "undirected" ? y("", !0) : (w(), b("defs", sr, [(w(!0), b(fe, null, Te(D(K).edges, (e) => (w(), b("marker", {
					key: `marker_${e.id}`,
					id: _n(e.id),
					markerWidth: D(K).arrowSize,
					markerHeight: D(K).arrowSize,
					refX: D(K).arrowSize - 3,
					refY: D(K).arrowSize / 2,
					orient: "auto",
					markerUnits: "strokeWidth"
				}, [D(K).arrowShape === "normal" ? (w(), b("path", {
					key: 0,
					d: `M 0 0 L ${D(K).arrowSize} ${D(K).arrowSize / 2} L 0 ${D(K).arrowSize} Z`,
					fill: gn(e),
					stroke: gn(e),
					"stroke-width": "0",
					style: { transition: "stroke 0.2s ease-in-out,\n                                        fill 0.2s ease-in-out,\n                                        stroke-width 0.2s ease-in-out" }
				}, null, 8, lr)) : (w(), b("path", {
					key: 1,
					d: `M 0 0 L ${D(K).arrowSize} ${D(K).arrowSize / 2} L 0 ${D(K).arrowSize} L ${D(K).arrowSize / 3} ${D(K).arrowSize / 2} Z`,
					fill: gn(e),
					stroke: gn(e),
					"stroke-width": "0",
					style: { transition: "stroke 0.2s ease-in-out,\n                                        fill 0.2s ease-in-out,\n                                        stroke-width 0.2s ease-in-out" }
				}, null, 8, ur))], 8, cr))), 128))])),
				x("g", dr, [(w(!0), b(fe, null, Te(D(K).edges, (e) => (w(), b(fe, { key: e.id }, [x("path", ye({
					"data-cy-edge": "",
					ref_for: !0,
					ref: en(e.id)
				}, { ref_for: !0 }, fn(e), { style: {
					"pointer-events": "none",
					transition: "stroke-width 0.2s ease-in-out,\n                                    stroke 0.2s ease-in-out"
				} }), null, 16), R.value.style.chart.midpoints.show ? (w(), b("circle", ye({
					key: 0,
					"data-cy-midpoint": "",
					class: "vue-ui-dag-edge-midpoint",
					"data-a11y-midpoint-id": e.id
				}, { ref_for: !0 }, hn(e), {
					"aria-label": `${q(e.from)?.label ?? e.from} to ${q(e.to)?.label ?? e.to}`,
					style: { transition: "stroke-width 0.2s ease-in-out,\n                                    stroke 0.2s ease-in-out,\n                                    fill 0.2s ease-in-out" },
					onMouseenter: (t) => un(e),
					onMouseleave: X
				}), null, 16, fr)) : y("", !0)], 64))), 128))]),
				x("g", pr, [(w(!0), b(fe, null, Te(D(K).nodes, (t) => (w(), b("g", {
					key: t.id,
					class: "vue-ui-dag-node",
					onClick: Ae((e) => R.value.style.chart.nodes.tooltip.showOnClick && Kt(t), ["stop"]),
					onMouseenter: (e) => ln(t.id),
					onMouseleave: (e) => dn(t.id)
				}, [e.$slots.node ? y("", !0) : (w(), b("rect", ye({
					key: 0,
					"data-cy-node": ""
				}, { ref_for: !0 }, mn(t), {
					"data-a11y-node-id": t.id,
					"aria-label": `${t.label ?? t.id}`,
					style: {
						cursor: R.value.style.chart.nodes.tooltip.showOnClick && Ge.value ? "pointer" : "default",
						transition: "stroke 0.2s ease-in-out, stroke-width 0.2s ease-in-out, fill 0.2s ease-in-out"
					}
				}), null, 16, hr)), e.$slots.node ? (w(), b("foreignObject", {
					key: 1,
					x: t.x - t.width / 2,
					y: t.y - t.height / 2,
					width: t.width,
					height: t.height,
					style: { overflow: "visible" }
				}, [E(e.$slots, "node", ye({ ref_for: !0 }, {
					node: t,
					orientation: G.value
				}), void 0, !0)], 8, gr)) : y("", !0)], 40, mr))), 128))]),
				x("g", _r, [(w(!0), b(fe, null, Te(D(K).edges, (e) => (w(), b("path", {
					key: e.id,
					d: e.pathData,
					fill: "none",
					stroke: "transparent",
					"stroke-width": R.value.style.chart.edges.strokeWidth * (e.from === Q.value || e.id === z.value?.id ? 1.3 : 1),
					"stroke-linecap": "round",
					"stroke-linejoin": "round",
					"marker-end": D(K).arrowShape === "undirected" ? null : `url(#${_n(e.id)})`,
					style: {
						"pointer-events": "none",
						transition: "stroke-width 0.2s ease-in-out,\n                                stroke 0.2s ease-in-out"
					}
				}, null, 8, vr))), 128))]),
				x("g", yr, [(w(!0), b(fe, null, Te(D(K).nodes, (t) => (w(), b("g", {
					key: t.id,
					onClick: Ae((e) => R.value.style.chart.nodes.tooltip.showOnClick && Kt(t), ["stop"]),
					onMouseenter: (e) => cn(t.id),
					onMouseleave: (e) => dn(t.id)
				}, [e.$slots["free-node-label"] ? y("", !0) : (w(), b(fe, { key: 0 }, [e.$slots["node-label"] ? (w(), b("text", {
					key: 0,
					x: t.x,
					y: t.y + R.value.style.chart.nodes.labels.fontSize / 3,
					"text-anchor": "middle",
					"font-size": R.value.style.chart.nodes.labels.fontSize,
					fill: Q.value === t.id && R.value.style.chart.nodes.selected.labelColor != null ? R.value.style.chart.nodes.selected.labelColor : t.original.color,
					"font-weight": R.value.style.chart.nodes.labels.bold ? "bold" : "normal",
					style: { transition: "fill 0.2s ease-in-out" }
				}, [E(e.$slots, "node-label", ye({ ref_for: !0 }, {
					node: t,
					orientation: G.value
				}), () => [ge(Ee(t.label), 1)], !0)], 8, xr)) : !e.$slots["free-node-label"] && !e.$slots.node ? (w(), b("text", {
					key: 1,
					"data-cy-node-label": "",
					x: t.x,
					y: t.y + R.value.style.chart.nodes.labels.fontSize / 3,
					"text-anchor": "middle",
					"font-size": R.value.style.chart.nodes.labels.fontSize,
					fill: (Q.value === t.id || pn(t.id)) && R.value.style.chart.nodes.selected.labelColor != null ? R.value.style.chart.nodes.selected.labelColor : t.original.color,
					"font-weight": R.value.style.chart.nodes.labels.bold ? "bold" : "normal",
					style: { transition: "fill 0.2s ease-in-out" },
					innerHTML: D(n)({
						content: t.label,
						fontSize: R.value.style.chart.nodes.labels.fontSize,
						fontWeight: R.value.style.chart.nodes.labels.bold ? "bold" : "normal",
						fill: (Q.value === t.id || pn(t.id)) && R.value.style.chart.nodes.selected.labelColor != null ? R.value.style.chart.nodes.selected.labelColor : t.original.color,
						x: t.x,
						y: t.y,
						autoOffset: !0
					})
				}, null, 8, Sr)) : y("", !0)], 64)), e.$slots["free-node-label"] ? (w(), b("g", Cr, [E(e.$slots, "free-node-label", ye({ ref_for: !0 }, {
					node: t,
					layoutData: D(K),
					orientation: G.value
				}), void 0, !0)])) : y("", !0)], 40, br))), 128))]),
				E(e.$slots, "svg", { svg: {
					drawingArea: D(kt),
					data: D(K),
					orientation: G.value,
					isPrintingImg: D(ft) || D(pt) || D(vt),
					isPrintingSvg: D(yt)
				} }, void 0, !0)
			], 46, nr)) : y("", !0), e.$slots.hint ? (w(), b("div", wr, [E(e.$slots, "hint", C(S({
				hint: R.value.a11y.translations.keyboardNavigation,
				isVisible: We.value
			})), void 0, !0)])) : y("", !0)]),
			e.$slots.watermark ? (w(), b("div", Tr, [E(e.$slots, "watermark", C(S({ isPrinting: D(ft) || D(pt) || D(vt) || D(yt) })), void 0, !0)])) : y("", !0),
			_e(me, { name: "fade" }, {
				default: O(() => [H.value ? (w(), v(pe, {
					key: 0,
					to: J.value ? M.value : "body"
				}, [x("div", {
					"data-cy-tooltip-midpoint": "",
					ref_key: "tooltipRef",
					ref: Ye,
					class: "vue-ui-dag-tooltip",
					style: Se({
						...Xe.value,
						maxWidth: R.value.style.chart.midpoints.tooltip.maxWidth,
						"--vue-data-ui-dag-tooltip-background": R.value.style.chart.midpoints.tooltip.backgroundColor,
						"--vue-data-ui-dag-tooltip-color": R.value.style.chart.midpoints.tooltip.color
					}),
					"data-position": B.value
				}, [E(e.$slots, "tooltip-midpoint", C(S({
					edge: z.value,
					layoutData: D(K)
				})), () => [z.value ? (w(), b("div", Dr, Ee(q(z.value.from)?.label ?? z.value.from) + " → " + Ee(q(z.value.to)?.label ?? z.value.to), 1)) : y("", !0)], !0)], 12, Er)], 8, ["to"])) : y("", !0)]),
				_: 3
			}),
			_e(me, { name: "fade" }, {
				default: O(() => [V.value ? (w(), v(pe, {
					key: 0,
					to: J.value ? M.value : "body"
				}, [x("div", {
					"data-cy-tooltip-node": "",
					ref_key: "nodeTooltipRef",
					ref: et,
					class: "vue-ui-dag-node-tooltip",
					style: Se({
						maxWidth: R.value.style.chart.nodes.tooltip.maxWidth,
						left: tt.value.left,
						top: tt.value.top,
						"--vue-data-ui-dag-node-tooltip-background": R.value.style.chart.nodes.tooltip.backgroundColor,
						"--vue-data-ui-dag-node-tooltip-color": R.value.style.chart.nodes.tooltip.color
					}),
					"data-position": nt.value
				}, [E(e.$slots, "tooltip-node", C(S({
					node: $e.value,
					layoutData: D(K)
				})), () => [$e.value ? (w(), b("div", kr, Ee($e.value.label), 1)) : y("", !0)], !0)], 12, Or)], 8, ["to"])) : y("", !0)]),
				_: 3
			}),
			R.value.style.chart.controls.position === "bottom" && !D(st) && R.value.style.chart.controls.show ? (w(), v(ue, {
				key: 7,
				ref_key: "zoomControls",
				ref: Le,
				config: R.value,
				scale: D(Mt),
				isFullscreen: J.value,
				withDirection: "",
				onZoomIn: zt,
				onZoomOut: Bt,
				onResetZoom: t[1] ||= () => D(At)(!0),
				onSwitchDirection: Ht
			}, null, 8, [
				"config",
				"scale",
				"isFullscreen"
			])) : y("", !0),
			e.$slots.source ? (w(), b("div", {
				key: 8,
				ref_key: "source",
				ref: Ie,
				dir: "auto"
			}, [E(e.$slots, "source", {}, void 0, !0)], 512)) : y("", !0),
			E(e.$slots, "skeleton", {}, () => [D(st) ? (w(), v(m, { key: 0 })) : y("", !0)], !0)
		], 46, Qn));
	}
}, [["__scopeId", "data-v-aed1b7b3"]]);
//#endregion
export { Zn as n, jr as t };
