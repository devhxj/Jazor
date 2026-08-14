//#region src/useLocale.js
function e(e) {
	return String(e || "").toLowerCase();
}
function t(e) {
	let t = (t, n, r) => {
		let i = new Intl.DateTimeFormat(e, { [n]: r });
		return Array.from({ length: t }, (e, t) => {
			let r = n === "month" ? new Date(2025, t, 1) : new Date(2025, 0, 5 + t);
			return i.format(r);
		});
	};
	return {
		months: t(12, "month", "long"),
		shortMonths: t(12, "month", "short"),
		days: t(7, "weekday", "long"),
		shortDays: t(7, "weekday", "short")
	};
}
var n = /* @__PURE__ */ new Map();
async function r(r) {
	let i = e(r) || "en";
	return n.has(i) || n.set(i, t(i)), {
		key: i,
		data: n.get(i)
	};
}
//#endregion
//#region src/useDateTime.js
function i({ useUTC: e = !1, min: t = void 0, max: n = void 0, locale: r = {
	months: [],
	shortMonths: [],
	days: [],
	shortDays: []
}, januaryAsYear: i = !1 }) {
	let a = [
		2,
		4,
		6,
		9,
		11
	], o = [
		0,
		31,
		59,
		90,
		120,
		151,
		181,
		212,
		243,
		273,
		304,
		334
	];
	function s(e) {
		return e % 12;
	}
	function c(e) {
		return typeof e == "number" || !isNaN(d(e));
	}
	function l(t) {
		let n = Date.parse(t);
		if (isNaN(n)) return t;
		if (!e) return n;
		let r = String(t).replace(/([+-]\d{2}:\d{2}|Z)$/, "");
		return Date.parse(r + "Z");
	}
	function u(e) {
		return new Date(e);
	}
	function d(e) {
		return l(isNaN(Date.parse(e)) ? Date.parse(e.replace(/-/g, "/").replace(/[a-z]+/gi, " ")) : e);
	}
	function f(e) {
		return Date.parse(e.replace(/-/g, "/").replace(/[a-z]+/gi, " "));
	}
	function p(t, n) {
		let a = e, o = RegExp(`\\\\(${(/* @__PURE__ */ "yyyy.yy.y.MMMM.MMM.MM.M.dddd.ddd.dd.d.HH.H.hh.h.mm.m.ss.s.fff.ff.f.TT.T.tt.t.K".split(".")).join("|")})`, "g"), s = [];
		n = n.replace(o, (e, t) => (s.push(t), `{{${s.length - 1}}}`));
		let c = ["\0", ...r.months], l = ["", ...r.shortMonths], u = ["", ...r.days], d = ["", ...r.shortDays];
		function f(e, t = 2) {
			let n = String(e);
			for (; n.length < t;) n = "0" + n;
			return n;
		}
		let p = a ? t.getUTCFullYear() : t.getFullYear();
		n = n.replace(/(^|[^\\])yyyy+/g, `$1${p}`).replace(/(^|[^\\])yy/g, `$1${String(p).substr(2, 2)}`).replace(/(^|[^\\])y/g, `$1${p}`);
		let m = (a ? t.getUTCMonth() : t.getMonth()) + 1;
		i && m === 1 && (c[1] = String(p), l[1] = String(p)), n = n.replace(/(^|[^\\])MMMM+/g, `$1${c[0]}`).replace(/(^|[^\\])MMM/g, `$1${l[0]}`).replace(/(^|[^\\])MM/g, `$1${f(m)}`).replace(/(^|[^\\])M/g, `$1${m}`);
		let h = a ? t.getUTCDate() : t.getDate();
		n = n.replace(/(^|[^\\])dddd+/g, `$1${u[0]}`).replace(/(^|[^\\])ddd/g, `$1${d[0]}`).replace(/(^|[^\\])dd/g, `$1${f(h)}`).replace(/(^|[^\\])d/g, `$1${h}`);
		let g = a ? t.getUTCHours() : t.getHours();
		n = n.replace(/(^|[^\\])HH+/g, `$1${f(g)}`).replace(/(^|[^\\])H/g, `$1${g}`);
		let _ = g > 12 ? g - 12 : g === 0 ? 12 : g;
		n = n.replace(/(^|[^\\])hh+/g, `$1${f(_)}`).replace(/(^|[^\\])h/g, `$1${_}`);
		let v = a ? t.getUTCMinutes() : t.getMinutes();
		n = n.replace(/(^|[^\\])mm+/g, `$1${f(v)}`).replace(/(^|[^\\])m/g, `$1${v}`);
		let y = a ? t.getUTCSeconds() : t.getSeconds();
		n = n.replace(/(^|[^\\])ss+/g, `$1${f(y)}`).replace(/(^|[^\\])s/g, `$1${y}`);
		let b = a ? t.getUTCMilliseconds() : t.getMilliseconds();
		n = n.replace(/(^|[^\\])fff+/g, `$1${f(b, 3)}`), b = Math.round(b / 10), n = n.replace(/(^|[^\\])ff/g, `$1${f(b)}`), b = Math.round(b / 10), n = n.replace(/(^|[^\\])f/g, `$1${b}`);
		let x = g < 12 ? "AM" : "PM";
		n = n.replace(/(^|[^\\])TT+/g, `$1${x}`).replace(/(^|[^\\])T/g, `$1${x.charAt(0)}`).replace(/(^|[^\\])tt+/g, `$1${x.toLowerCase()}`).replace(/(^|[^\\])t/g, `$1${x.toLowerCase().charAt(0)}`);
		let S = -t.getTimezoneOffset(), C = a || !S ? "Z" : S > 0 ? "+" : "-";
		if (!a) {
			S = Math.abs(S);
			let e = Math.floor(S / 60), t = S % 60;
			C += f(e) + ":" + f(t);
		}
		n = n.replace(/(^|[^\\])K/g, `$1${C}`);
		let w = (a ? t.getUTCDay() : t.getDay()) + 1;
		return n = n.replace(new RegExp(u[0], "g"), u[w]).replace(new RegExp(d[0], "g"), d[w]).replace(new RegExp(c[0], "g"), c[m]).replace(new RegExp(l[0], "g"), l[m]), n = n.replace(/{{(\d+)}}/g, (e, t) => s[+t]), n;
	}
	function m(e, r) {
		t !== void 0 && (e = t), n !== void 0 && (r = n);
		let i = u(e), a = u(r), o = p(i, "yyyy MM dd HH mm ss fff").split(" "), s = p(a, "yyyy MM dd HH mm ss fff").split(" ");
		return {
			minMillisecond: parseInt(o[6], 10),
			maxMillisecond: parseInt(s[6], 10),
			minSecond: parseInt(o[5], 10),
			maxSecond: parseInt(s[5], 10),
			minMinute: parseInt(o[4], 10),
			maxMinute: parseInt(s[4], 10),
			minHour: parseInt(o[3], 10),
			maxHour: parseInt(s[3], 10),
			minDate: parseInt(o[2], 10),
			maxDate: parseInt(s[2], 10),
			minMonth: parseInt(o[1], 10) - 1,
			maxMonth: parseInt(s[1], 10) - 1,
			minYear: parseInt(o[0], 10),
			maxYear: parseInt(s[0], 10)
		};
	}
	function h(e) {
		return e % 4 == 0 && e % 100 != 0 || e % 400 == 0;
	}
	function g(e, t, n) {
		return y(e, t) - n;
	}
	function _(e) {
		return h(e) ? 366 : 365;
	}
	function v(e, t, n) {
		let r = o[t] + n;
		return t > 1 && h(e) && (r += 1), r;
	}
	function y(e, t) {
		return e = s(e), a.includes(e) ? e === 2 ? h(t) ? 29 : 28 : 30 : 31;
	}
	return {
		isValidDate: c,
		getTimeStamp: l,
		getDate: u,
		parseDate: d,
		parseDateWithTimezone: f,
		formatDate: p,
		getTimeUnitsfromTimestamp: m,
		isLeapYear: h,
		calculateLastDaysOfMonth: g,
		determineDaysOfYear: _,
		determineRemainingDaysOfYear: v,
		determineDaysOfMonths: y
	};
}
//#endregion
//#region src/useTimeLabels.js
var a = 86400;
async function o({ values: e, maxDatapoints: t, formatter: n, start: o, end: s }) {
	let c = [];
	if (!n.enable || e.length === 0) {
		for (let t = o; t < s; t += 1) c.push({
			text: String(e[t] ?? t),
			absoluteIndex: t
		});
		return c;
	}
	let l = e.map((e) => {
		let t = new Date(e).getTime();
		return isNaN(t) ? e : t;
	}).slice(o, s);
	if (l.length === 0) return [];
	let u = l[0], d = l[l.length - 1], f;
	try {
		({data: f} = await r(n.locale));
	} catch {
		({data: f} = await r("en"));
	}
	let p = i({
		useUTC: n.useUTC,
		min: u,
		max: d,
		locale: f,
		januaryAsYear: n.januaryAsYear
	}), m = (d - u) / (1e3 * a), h = m * 24, g = h * 60, _ = g * 60, v;
	switch (!0) {
		case m / 365 > 5:
			v = "years";
			break;
		case m > 800:
			v = "half_year";
			break;
		case m > 180:
			v = "months";
			break;
		case m > 90:
			v = "months_fortnight";
			break;
		case m > 60:
			v = "months_days";
			break;
		case m > 30:
			v = "week_days";
			break;
		case m > 2:
			v = "days";
			break;
		case h > 2.4:
			v = "hours";
			break;
		case g > 15:
			v = "minutes_fives";
			break;
		case g > 5:
			v = "minutes";
			break;
		case g > 1:
			v = "seconds_tens";
			break;
		case _ > 20:
			v = "seconds_fives";
			break;
		default: v = "seconds";
	}
	let y;
	y = v === "years" ? "year" : [
		"half_year",
		"months",
		"months_fortnight",
		"months_days",
		"week_days"
	].includes(v) ? "month" : v === "days" ? "day" : v === "hours" ? "hour" : ["minutes_fives", "minutes"].includes(v) ? "minute" : "second";
	let b = n.options[y] ?? n.options.hour, x = n.options.year, S = n.options.month, C = n.options.day;
	return l.forEach((e, t) => {
		let r = new Date(e), i;
		switch (y) {
			case "year":
				i = p.formatDate(r, x);
				break;
			case "month": {
				let e = n.useUTC ? r.getUTCMonth() : r.getMonth();
				i = n.januaryAsYear && e === 0 ? p.formatDate(r, x) : p.formatDate(r, S);
				break;
			}
			case "day":
				i = p.formatDate(r, C);
				break;
			default: {
				let e = n.useUTC ? r.getUTCHours() : r.getHours(), t = n.useUTC ? r.getUTCMinutes() : r.getMinutes();
				i = e === 0 && t === 0 ? p.formatDate(r, C) : p.formatDate(r, b);
			}
		}
		c.push({
			text: i,
			absoluteIndex: o + t
		});
	}), c;
}
//#endregion
export { i as n, r, o as t };
