// node_modules/.deno/@vue+shared@3.5.13/node_modules/@vue/shared/dist/shared.esm-bundler.js
// @__NO_SIDE_EFFECTS__
function makeMap(str) {
  const map2 = /* @__PURE__ */ Object.create(null);
  for (const key of str.split(",")) map2[key] = 1;
  return (val) => val in map2;
}
var EMPTY_OBJ = true ? Object.freeze({}) : {};
var EMPTY_ARR = true ? Object.freeze([]) : [];
var NOOP = () => {
};
var NO = () => false;
var isOn = (key) => key.charCodeAt(0) === 111 && key.charCodeAt(1) === 110 && // uppercase letter
(key.charCodeAt(2) > 122 || key.charCodeAt(2) < 97);
var isModelListener = (key) => key.startsWith("onUpdate:");
var extend = Object.assign;
var remove = (arr, el) => {
  const i = arr.indexOf(el);
  if (i > -1) {
    arr.splice(i, 1);
  }
};
var hasOwnProperty = Object.prototype.hasOwnProperty;
var hasOwn = (val, key) => hasOwnProperty.call(val, key);
var isArray = Array.isArray;
var isMap = (val) => toTypeString(val) === "[object Map]";
var isSet = (val) => toTypeString(val) === "[object Set]";
var isFunction = (val) => typeof val === "function";
var isString = (val) => typeof val === "string";
var isSymbol = (val) => typeof val === "symbol";
var isObject = (val) => val !== null && typeof val === "object";
var isPromise = (val) => {
  return (isObject(val) || isFunction(val)) && isFunction(val.then) && isFunction(val.catch);
};
var objectToString = Object.prototype.toString;
var toTypeString = (value) => objectToString.call(value);
var toRawType = (value) => {
  return toTypeString(value).slice(8, -1);
};
var isPlainObject = (val) => toTypeString(val) === "[object Object]";
var isIntegerKey = (key) => isString(key) && key !== "NaN" && key[0] !== "-" && "" + parseInt(key, 10) === key;
var isReservedProp = /* @__PURE__ */ makeMap(
  // the leading comma is intentional so empty string "" is also included
  ",key,ref,ref_for,ref_key,onVnodeBeforeMount,onVnodeMounted,onVnodeBeforeUpdate,onVnodeUpdated,onVnodeBeforeUnmount,onVnodeUnmounted"
);
var isBuiltInDirective = /* @__PURE__ */ makeMap("bind,cloak,else-if,else,for,html,if,model,on,once,pre,show,slot,text,memo");
var cacheStringFunction = (fn) => {
  const cache = /* @__PURE__ */ Object.create(null);
  return (str) => {
    const hit = cache[str];
    return hit || (cache[str] = fn(str));
  };
};
var camelizeRE = /-(\w)/g;
var camelize = cacheStringFunction((str) => {
  return str.replace(camelizeRE, (_, c) => c ? c.toUpperCase() : "");
});
var hyphenateRE = /\B([A-Z])/g;
var hyphenate = cacheStringFunction((str) => str.replace(hyphenateRE, "-$1").toLowerCase());
var capitalize = cacheStringFunction((str) => {
  return str.charAt(0).toUpperCase() + str.slice(1);
});
var toHandlerKey = cacheStringFunction((str) => {
  const s = str ? `on${capitalize(str)}` : ``;
  return s;
});
var hasChanged = (value, oldValue) => !Object.is(value, oldValue);
var invokeArrayFns = (fns, ...arg) => {
  for (let i = 0; i < fns.length; i++) {
    fns[i](...arg);
  }
};
var def = (obj, key, value, writable = false) => {
  Object.defineProperty(obj, key, {
    configurable: true,
    enumerable: false,
    writable,
    value
  });
};
var looseToNumber = (val) => {
  const n = parseFloat(val);
  return isNaN(n) ? val : n;
};
var _globalThis;
var getGlobalThis = () => {
  return _globalThis || (_globalThis = typeof globalThis !== "undefined" ? globalThis : typeof self !== "undefined" ? self : typeof window !== "undefined" ? window : typeof global !== "undefined" ? global : {});
};
function normalizeStyle(value) {
  if (isArray(value)) {
    const res = {};
    for (let i = 0; i < value.length; i++) {
      const item = value[i];
      const normalized = isString(item) ? parseStringStyle(item) : normalizeStyle(item);
      if (normalized) {
        for (const key in normalized) {
          res[key] = normalized[key];
        }
      }
    }
    return res;
  } else if (isString(value) || isObject(value)) {
    return value;
  }
}
var listDelimiterRE = /;(?![^(]*\))/g;
var propertyDelimiterRE = /:([^]+)/;
var styleCommentRE = /\/\*[^]*?\*\//g;
function parseStringStyle(cssText) {
  const ret = {};
  cssText.replace(styleCommentRE, "").split(listDelimiterRE).forEach((item) => {
    if (item) {
      const tmp = item.split(propertyDelimiterRE);
      tmp.length > 1 && (ret[tmp[0].trim()] = tmp[1].trim());
    }
  });
  return ret;
}
function normalizeClass(value) {
  let res = "";
  if (isString(value)) {
    res = value;
  } else if (isArray(value)) {
    for (let i = 0; i < value.length; i++) {
      const normalized = normalizeClass(value[i]);
      if (normalized) {
        res += normalized + " ";
      }
    }
  } else if (isObject(value)) {
    for (const name in value) {
      if (value[name]) {
        res += name + " ";
      }
    }
  }
  return res.trim();
}
var HTML_TAGS = "html,body,base,head,link,meta,style,title,address,article,aside,footer,header,hgroup,h1,h2,h3,h4,h5,h6,nav,section,div,dd,dl,dt,figcaption,figure,picture,hr,img,li,main,ol,p,pre,ul,a,b,abbr,bdi,bdo,br,cite,code,data,dfn,em,i,kbd,mark,q,rp,rt,ruby,s,samp,small,span,strong,sub,sup,time,u,var,wbr,area,audio,map,track,video,embed,object,param,source,canvas,script,noscript,del,ins,caption,col,colgroup,table,thead,tbody,td,th,tr,button,datalist,fieldset,form,input,label,legend,meter,optgroup,option,output,progress,select,textarea,details,dialog,menu,summary,template,blockquote,iframe,tfoot";
var SVG_TAGS = "svg,animate,animateMotion,animateTransform,circle,clipPath,color-profile,defs,desc,discard,ellipse,feBlend,feColorMatrix,feComponentTransfer,feComposite,feConvolveMatrix,feDiffuseLighting,feDisplacementMap,feDistantLight,feDropShadow,feFlood,feFuncA,feFuncB,feFuncG,feFuncR,feGaussianBlur,feImage,feMerge,feMergeNode,feMorphology,feOffset,fePointLight,feSpecularLighting,feSpotLight,feTile,feTurbulence,filter,foreignObject,g,hatch,hatchpath,image,line,linearGradient,marker,mask,mesh,meshgradient,meshpatch,meshrow,metadata,mpath,path,pattern,polygon,polyline,radialGradient,rect,set,solidcolor,stop,switch,symbol,text,textPath,title,tspan,unknown,use,view";
var MATH_TAGS = "annotation,annotation-xml,maction,maligngroup,malignmark,math,menclose,merror,mfenced,mfrac,mfraction,mglyph,mi,mlabeledtr,mlongdiv,mmultiscripts,mn,mo,mover,mpadded,mphantom,mprescripts,mroot,mrow,ms,mscarries,mscarry,msgroup,msline,mspace,msqrt,msrow,mstack,mstyle,msub,msubsup,msup,mtable,mtd,mtext,mtr,munder,munderover,none,semantics";
var isHTMLTag = /* @__PURE__ */ makeMap(HTML_TAGS);
var isSVGTag = /* @__PURE__ */ makeMap(SVG_TAGS);
var isMathMLTag = /* @__PURE__ */ makeMap(MATH_TAGS);
var specialBooleanAttrs = `itemscope,allowfullscreen,formnovalidate,ismap,nomodule,novalidate,readonly`;
var isSpecialBooleanAttr = /* @__PURE__ */ makeMap(specialBooleanAttrs);
var isBooleanAttr = /* @__PURE__ */ makeMap(specialBooleanAttrs + `,async,autofocus,autoplay,controls,default,defer,disabled,hidden,inert,loop,open,required,reversed,scoped,seamless,checked,muted,multiple,selected`);
function includeBooleanAttr(value) {
  return !!value || value === "";
}

// node_modules/.deno/@vue+reactivity@3.5.13/node_modules/@vue/reactivity/dist/reactivity.esm-bundler.js
function warn(msg, ...args) {
  console.warn(`[Vue warn] ${msg}`, ...args);
}
var activeEffectScope;
var EffectScope = class {
  constructor(detached = false) {
    this.detached = detached;
    this._active = true;
    this.effects = [];
    this.cleanups = [];
    this._isPaused = false;
    this.parent = activeEffectScope;
    if (!detached && activeEffectScope) {
      this.index = (activeEffectScope.scopes || (activeEffectScope.scopes = [])).push(this) - 1;
    }
  }
  get active() {
    return this._active;
  }
  pause() {
    if (this._active) {
      this._isPaused = true;
      let i, l;
      if (this.scopes) {
        for (i = 0, l = this.scopes.length; i < l; i++) {
          this.scopes[i].pause();
        }
      }
      for (i = 0, l = this.effects.length; i < l; i++) {
        this.effects[i].pause();
      }
    }
  }
  /**
   * Resumes the effect scope, including all child scopes and effects.
   */
  resume() {
    if (this._active) {
      if (this._isPaused) {
        this._isPaused = false;
        let i, l;
        if (this.scopes) {
          for (i = 0, l = this.scopes.length; i < l; i++) {
            this.scopes[i].resume();
          }
        }
        for (i = 0, l = this.effects.length; i < l; i++) {
          this.effects[i].resume();
        }
      }
    }
  }
  run(fn) {
    if (this._active) {
      const currentEffectScope = activeEffectScope;
      try {
        activeEffectScope = this;
        return fn();
      } finally {
        activeEffectScope = currentEffectScope;
      }
    } else if (true) {
      warn(`cannot run an inactive effect scope.`);
    }
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  on() {
    activeEffectScope = this;
  }
  /**
   * This should only be called on non-detached scopes
   * @internal
   */
  off() {
    activeEffectScope = this.parent;
  }
  stop(fromParent) {
    if (this._active) {
      this._active = false;
      let i, l;
      for (i = 0, l = this.effects.length; i < l; i++) {
        this.effects[i].stop();
      }
      this.effects.length = 0;
      for (i = 0, l = this.cleanups.length; i < l; i++) {
        this.cleanups[i]();
      }
      this.cleanups.length = 0;
      if (this.scopes) {
        for (i = 0, l = this.scopes.length; i < l; i++) {
          this.scopes[i].stop(true);
        }
        this.scopes.length = 0;
      }
      if (!this.detached && this.parent && !fromParent) {
        const last = this.parent.scopes.pop();
        if (last && last !== this) {
          this.parent.scopes[this.index] = last;
          last.index = this.index;
        }
      }
      this.parent = void 0;
    }
  }
};
function effectScope(detached) {
  return new EffectScope(detached);
}
function getCurrentScope() {
  return activeEffectScope;
}
function onScopeDispose(fn, failSilently = false) {
  if (activeEffectScope) {
    activeEffectScope.cleanups.push(fn);
  } else if (!failSilently) {
    warn(`onScopeDispose() is called when there is no active effect scope to be associated with.`);
  }
}
var activeSub;
var pausedQueueEffects = /* @__PURE__ */ new WeakSet();
var ReactiveEffect = class {
  constructor(fn) {
    this.fn = fn;
    this.deps = void 0;
    this.depsTail = void 0;
    this.flags = 1 | 4;
    this.next = void 0;
    this.cleanup = void 0;
    this.scheduler = void 0;
    if (activeEffectScope && activeEffectScope.active) {
      activeEffectScope.effects.push(this);
    }
  }
  pause() {
    this.flags |= 64;
  }
  resume() {
    if (this.flags & 64) {
      this.flags &= ~64;
      if (pausedQueueEffects.has(this)) {
        pausedQueueEffects.delete(this);
        this.trigger();
      }
    }
  }
  /**
   * @internal
   */
  notify() {
    if (this.flags & 2 && !(this.flags & 32)) {
      return;
    }
    if (!(this.flags & 8)) {
      batch(this);
    }
  }
  run() {
    if (!(this.flags & 1)) {
      return this.fn();
    }
    this.flags |= 2;
    cleanupEffect(this);
    prepareDeps(this);
    const prevEffect = activeSub;
    const prevShouldTrack = shouldTrack;
    activeSub = this;
    shouldTrack = true;
    try {
      return this.fn();
    } finally {
      if (activeSub !== this) {
        warn("Active effect was not restored correctly - this is likely a Vue internal bug.");
      }
      cleanupDeps(this);
      activeSub = prevEffect;
      shouldTrack = prevShouldTrack;
      this.flags &= ~2;
    }
  }
  stop() {
    if (this.flags & 1) {
      for (let link = this.deps; link; link = link.nextDep) {
        removeSub(link);
      }
      this.deps = this.depsTail = void 0;
      cleanupEffect(this);
      this.onStop && this.onStop();
      this.flags &= ~1;
    }
  }
  trigger() {
    if (this.flags & 64) {
      pausedQueueEffects.add(this);
    } else if (this.scheduler) {
      this.scheduler();
    } else {
      this.runIfDirty();
    }
  }
  /**
   * @internal
   */
  runIfDirty() {
    if (isDirty(this)) {
      this.run();
    }
  }
  get dirty() {
    return isDirty(this);
  }
};
var batchDepth = 0;
var batchedSub;
var batchedComputed;
function batch(sub, isComputed2 = false) {
  sub.flags |= 8;
  if (isComputed2) {
    sub.next = batchedComputed;
    batchedComputed = sub;
    return;
  }
  sub.next = batchedSub;
  batchedSub = sub;
}
function startBatch() {
  batchDepth++;
}
function endBatch() {
  if (--batchDepth > 0) {
    return;
  }
  if (batchedComputed) {
    let e = batchedComputed;
    batchedComputed = void 0;
    while (e) {
      const next = e.next;
      e.next = void 0;
      e.flags &= ~8;
      e = next;
    }
  }
  let error;
  while (batchedSub) {
    let e = batchedSub;
    batchedSub = void 0;
    while (e) {
      const next = e.next;
      e.next = void 0;
      e.flags &= ~8;
      if (e.flags & 1) {
        try {
          ;
          e.trigger();
        } catch (err) {
          if (!error) error = err;
        }
      }
      e = next;
    }
  }
  if (error) throw error;
}
function prepareDeps(sub) {
  for (let link = sub.deps; link; link = link.nextDep) {
    link.version = -1;
    link.prevActiveLink = link.dep.activeLink;
    link.dep.activeLink = link;
  }
}
function cleanupDeps(sub) {
  let head;
  let tail = sub.depsTail;
  let link = tail;
  while (link) {
    const prev = link.prevDep;
    if (link.version === -1) {
      if (link === tail) tail = prev;
      removeSub(link);
      removeDep(link);
    } else {
      head = link;
    }
    link.dep.activeLink = link.prevActiveLink;
    link.prevActiveLink = void 0;
    link = prev;
  }
  sub.deps = head;
  sub.depsTail = tail;
}
function isDirty(sub) {
  for (let link = sub.deps; link; link = link.nextDep) {
    if (link.dep.version !== link.version || link.dep.computed && (refreshComputed(link.dep.computed) || link.dep.version !== link.version)) {
      return true;
    }
  }
  if (sub._dirty) {
    return true;
  }
  return false;
}
function refreshComputed(computed3) {
  if (computed3.flags & 4 && !(computed3.flags & 16)) {
    return;
  }
  computed3.flags &= ~16;
  if (computed3.globalVersion === globalVersion) {
    return;
  }
  computed3.globalVersion = globalVersion;
  const dep = computed3.dep;
  computed3.flags |= 2;
  if (dep.version > 0 && !computed3.isSSR && computed3.deps && !isDirty(computed3)) {
    computed3.flags &= ~2;
    return;
  }
  const prevSub = activeSub;
  const prevShouldTrack = shouldTrack;
  activeSub = computed3;
  shouldTrack = true;
  try {
    prepareDeps(computed3);
    const value = computed3.fn(computed3._value);
    if (dep.version === 0 || hasChanged(value, computed3._value)) {
      computed3._value = value;
      dep.version++;
    }
  } catch (err) {
    dep.version++;
    throw err;
  } finally {
    activeSub = prevSub;
    shouldTrack = prevShouldTrack;
    cleanupDeps(computed3);
    computed3.flags &= ~2;
  }
}
function removeSub(link, soft = false) {
  const { dep, prevSub, nextSub } = link;
  if (prevSub) {
    prevSub.nextSub = nextSub;
    link.prevSub = void 0;
  }
  if (nextSub) {
    nextSub.prevSub = prevSub;
    link.nextSub = void 0;
  }
  if (dep.subsHead === link) {
    dep.subsHead = nextSub;
  }
  if (dep.subs === link) {
    dep.subs = prevSub;
    if (!prevSub && dep.computed) {
      dep.computed.flags &= ~4;
      for (let l = dep.computed.deps; l; l = l.nextDep) {
        removeSub(l, true);
      }
    }
  }
  if (!soft && !--dep.sc && dep.map) {
    dep.map.delete(dep.key);
  }
}
function removeDep(link) {
  const { prevDep, nextDep } = link;
  if (prevDep) {
    prevDep.nextDep = nextDep;
    link.prevDep = void 0;
  }
  if (nextDep) {
    nextDep.prevDep = prevDep;
    link.nextDep = void 0;
  }
}
var shouldTrack = true;
var trackStack = [];
function pauseTracking() {
  trackStack.push(shouldTrack);
  shouldTrack = false;
}
function resetTracking() {
  const last = trackStack.pop();
  shouldTrack = last === void 0 ? true : last;
}
function cleanupEffect(e) {
  const { cleanup } = e;
  e.cleanup = void 0;
  if (cleanup) {
    const prevSub = activeSub;
    activeSub = void 0;
    try {
      cleanup();
    } finally {
      activeSub = prevSub;
    }
  }
}
var globalVersion = 0;
var Link = class {
  constructor(sub, dep) {
    this.sub = sub;
    this.dep = dep;
    this.version = dep.version;
    this.nextDep = this.prevDep = this.nextSub = this.prevSub = this.prevActiveLink = void 0;
  }
};
var Dep = class {
  constructor(computed3) {
    this.computed = computed3;
    this.version = 0;
    this.activeLink = void 0;
    this.subs = void 0;
    this.map = void 0;
    this.key = void 0;
    this.sc = 0;
    if (true) {
      this.subsHead = void 0;
    }
  }
  track(debugInfo) {
    if (!activeSub || !shouldTrack || activeSub === this.computed) {
      return;
    }
    let link = this.activeLink;
    if (link === void 0 || link.sub !== activeSub) {
      link = this.activeLink = new Link(activeSub, this);
      if (!activeSub.deps) {
        activeSub.deps = activeSub.depsTail = link;
      } else {
        link.prevDep = activeSub.depsTail;
        activeSub.depsTail.nextDep = link;
        activeSub.depsTail = link;
      }
      addSub(link);
    } else if (link.version === -1) {
      link.version = this.version;
      if (link.nextDep) {
        const next = link.nextDep;
        next.prevDep = link.prevDep;
        if (link.prevDep) {
          link.prevDep.nextDep = next;
        }
        link.prevDep = activeSub.depsTail;
        link.nextDep = void 0;
        activeSub.depsTail.nextDep = link;
        activeSub.depsTail = link;
        if (activeSub.deps === link) {
          activeSub.deps = next;
        }
      }
    }
    if (activeSub.onTrack) {
      activeSub.onTrack(extend({
        effect: activeSub
      }, debugInfo));
    }
    return link;
  }
  trigger(debugInfo) {
    this.version++;
    globalVersion++;
    this.notify(debugInfo);
  }
  notify(debugInfo) {
    startBatch();
    try {
      if (true) {
        for (let head = this.subsHead; head; head = head.nextSub) {
          if (head.sub.onTrigger && !(head.sub.flags & 8)) {
            head.sub.onTrigger(extend({
              effect: head.sub
            }, debugInfo));
          }
        }
      }
      for (let link = this.subs; link; link = link.prevSub) {
        if (link.sub.notify()) {
          ;
          link.sub.dep.notify();
        }
      }
    } finally {
      endBatch();
    }
  }
};
function addSub(link) {
  link.dep.sc++;
  if (link.sub.flags & 4) {
    const computed3 = link.dep.computed;
    if (computed3 && !link.dep.subs) {
      computed3.flags |= 4 | 16;
      for (let l = computed3.deps; l; l = l.nextDep) {
        addSub(l);
      }
    }
    const currentTail = link.dep.subs;
    if (currentTail !== link) {
      link.prevSub = currentTail;
      if (currentTail) currentTail.nextSub = link;
    }
    if (link.dep.subsHead === void 0) {
      link.dep.subsHead = link;
    }
    link.dep.subs = link;
  }
}
var targetMap = /* @__PURE__ */ new WeakMap();
var ITERATE_KEY = Symbol(true ? "Object iterate" : "");
var MAP_KEY_ITERATE_KEY = Symbol(true ? "Map keys iterate" : "");
var ARRAY_ITERATE_KEY = Symbol(true ? "Array iterate" : "");
function track(target2, type, key) {
  if (shouldTrack && activeSub) {
    let depsMap = targetMap.get(target2);
    if (!depsMap) {
      targetMap.set(target2, depsMap = /* @__PURE__ */ new Map());
    }
    let dep = depsMap.get(key);
    if (!dep) {
      depsMap.set(key, dep = new Dep());
      dep.map = depsMap;
      dep.key = key;
    }
    if (true) {
      dep.track({
        target: target2,
        type,
        key
      });
    } else {
      dep.track();
    }
  }
}
function trigger(target2, type, key, newValue, oldValue, oldTarget) {
  const depsMap = targetMap.get(target2);
  if (!depsMap) {
    globalVersion++;
    return;
  }
  const run = (dep) => {
    if (dep) {
      if (true) {
        dep.trigger({
          target: target2,
          type,
          key,
          newValue,
          oldValue,
          oldTarget
        });
      } else {
        dep.trigger();
      }
    }
  };
  startBatch();
  if (type === "clear") {
    depsMap.forEach(run);
  } else {
    const targetIsArray = isArray(target2);
    const isArrayIndex = targetIsArray && isIntegerKey(key);
    if (targetIsArray && key === "length") {
      const newLength = Number(newValue);
      depsMap.forEach((dep, key2) => {
        if (key2 === "length" || key2 === ARRAY_ITERATE_KEY || !isSymbol(key2) && key2 >= newLength) {
          run(dep);
        }
      });
    } else {
      if (key !== void 0 || depsMap.has(void 0)) {
        run(depsMap.get(key));
      }
      if (isArrayIndex) {
        run(depsMap.get(ARRAY_ITERATE_KEY));
      }
      switch (type) {
        case "add":
          if (!targetIsArray) {
            run(depsMap.get(ITERATE_KEY));
            if (isMap(target2)) {
              run(depsMap.get(MAP_KEY_ITERATE_KEY));
            }
          } else if (isArrayIndex) {
            run(depsMap.get("length"));
          }
          break;
        case "delete":
          if (!targetIsArray) {
            run(depsMap.get(ITERATE_KEY));
            if (isMap(target2)) {
              run(depsMap.get(MAP_KEY_ITERATE_KEY));
            }
          }
          break;
        case "set":
          if (isMap(target2)) {
            run(depsMap.get(ITERATE_KEY));
          }
          break;
      }
    }
  }
  endBatch();
}
function getDepFromReactive(object, key) {
  const depMap = targetMap.get(object);
  return depMap && depMap.get(key);
}
function reactiveReadArray(array) {
  const raw = toRaw(array);
  if (raw === array) return raw;
  track(raw, "iterate", ARRAY_ITERATE_KEY);
  return isShallow(array) ? raw : raw.map(toReactive);
}
function shallowReadArray(arr) {
  track(arr = toRaw(arr), "iterate", ARRAY_ITERATE_KEY);
  return arr;
}
var arrayInstrumentations = {
  __proto__: null,
  [Symbol.iterator]() {
    return iterator(this, Symbol.iterator, toReactive);
  },
  concat(...args) {
    return reactiveReadArray(this).concat(...args.map((x) => isArray(x) ? reactiveReadArray(x) : x));
  },
  entries() {
    return iterator(this, "entries", (value) => {
      value[1] = toReactive(value[1]);
      return value;
    });
  },
  every(fn, thisArg) {
    return apply(this, "every", fn, thisArg, void 0, arguments);
  },
  filter(fn, thisArg) {
    return apply(this, "filter", fn, thisArg, (v) => v.map(toReactive), arguments);
  },
  find(fn, thisArg) {
    return apply(this, "find", fn, thisArg, toReactive, arguments);
  },
  findIndex(fn, thisArg) {
    return apply(this, "findIndex", fn, thisArg, void 0, arguments);
  },
  findLast(fn, thisArg) {
    return apply(this, "findLast", fn, thisArg, toReactive, arguments);
  },
  findLastIndex(fn, thisArg) {
    return apply(this, "findLastIndex", fn, thisArg, void 0, arguments);
  },
  // flat, flatMap could benefit from ARRAY_ITERATE but are not straight-forward to implement
  forEach(fn, thisArg) {
    return apply(this, "forEach", fn, thisArg, void 0, arguments);
  },
  includes(...args) {
    return searchProxy(this, "includes", args);
  },
  indexOf(...args) {
    return searchProxy(this, "indexOf", args);
  },
  join(separator) {
    return reactiveReadArray(this).join(separator);
  },
  // keys() iterator only reads `length`, no optimisation required
  lastIndexOf(...args) {
    return searchProxy(this, "lastIndexOf", args);
  },
  map(fn, thisArg) {
    return apply(this, "map", fn, thisArg, void 0, arguments);
  },
  pop() {
    return noTracking(this, "pop");
  },
  push(...args) {
    return noTracking(this, "push", args);
  },
  reduce(fn, ...args) {
    return reduce(this, "reduce", fn, args);
  },
  reduceRight(fn, ...args) {
    return reduce(this, "reduceRight", fn, args);
  },
  shift() {
    return noTracking(this, "shift");
  },
  // slice could use ARRAY_ITERATE but also seems to beg for range tracking
  some(fn, thisArg) {
    return apply(this, "some", fn, thisArg, void 0, arguments);
  },
  splice(...args) {
    return noTracking(this, "splice", args);
  },
  toReversed() {
    return reactiveReadArray(this).toReversed();
  },
  toSorted(comparer) {
    return reactiveReadArray(this).toSorted(comparer);
  },
  toSpliced(...args) {
    return reactiveReadArray(this).toSpliced(...args);
  },
  unshift(...args) {
    return noTracking(this, "unshift", args);
  },
  values() {
    return iterator(this, "values", toReactive);
  }
};
function iterator(self2, method, wrapValue) {
  const arr = shallowReadArray(self2);
  const iter = arr[method]();
  if (arr !== self2 && !isShallow(self2)) {
    iter._next = iter.next;
    iter.next = () => {
      const result = iter._next();
      if (result.value) {
        result.value = wrapValue(result.value);
      }
      return result;
    };
  }
  return iter;
}
var arrayProto = Array.prototype;
function apply(self2, method, fn, thisArg, wrappedRetFn, args) {
  const arr = shallowReadArray(self2);
  const needsWrap = arr !== self2 && !isShallow(self2);
  const methodFn = arr[method];
  if (methodFn !== arrayProto[method]) {
    const result2 = methodFn.apply(self2, args);
    return needsWrap ? toReactive(result2) : result2;
  }
  let wrappedFn = fn;
  if (arr !== self2) {
    if (needsWrap) {
      wrappedFn = function(item, index) {
        return fn.call(this, toReactive(item), index, self2);
      };
    } else if (fn.length > 2) {
      wrappedFn = function(item, index) {
        return fn.call(this, item, index, self2);
      };
    }
  }
  const result = methodFn.call(arr, wrappedFn, thisArg);
  return needsWrap && wrappedRetFn ? wrappedRetFn(result) : result;
}
function reduce(self2, method, fn, args) {
  const arr = shallowReadArray(self2);
  let wrappedFn = fn;
  if (arr !== self2) {
    if (!isShallow(self2)) {
      wrappedFn = function(acc, item, index) {
        return fn.call(this, acc, toReactive(item), index, self2);
      };
    } else if (fn.length > 3) {
      wrappedFn = function(acc, item, index) {
        return fn.call(this, acc, item, index, self2);
      };
    }
  }
  return arr[method](wrappedFn, ...args);
}
function searchProxy(self2, method, args) {
  const arr = toRaw(self2);
  track(arr, "iterate", ARRAY_ITERATE_KEY);
  const res = arr[method](...args);
  if ((res === -1 || res === false) && isProxy(args[0])) {
    args[0] = toRaw(args[0]);
    return arr[method](...args);
  }
  return res;
}
function noTracking(self2, method, args = []) {
  pauseTracking();
  startBatch();
  const res = toRaw(self2)[method].apply(self2, args);
  endBatch();
  resetTracking();
  return res;
}
var isNonTrackableKeys = /* @__PURE__ */ makeMap(`__proto__,__v_isRef,__isVue`);
var builtInSymbols = new Set(/* @__PURE__ */ Object.getOwnPropertyNames(Symbol).filter((key) => key !== "arguments" && key !== "caller").map((key) => Symbol[key]).filter(isSymbol));
function hasOwnProperty2(key) {
  if (!isSymbol(key)) key = String(key);
  const obj = toRaw(this);
  track(obj, "has", key);
  return obj.hasOwnProperty(key);
}
var BaseReactiveHandler = class {
  constructor(_isReadonly = false, _isShallow = false) {
    this._isReadonly = _isReadonly;
    this._isShallow = _isShallow;
  }
  get(target2, key, receiver) {
    if (key === "__v_skip") return target2["__v_skip"];
    const isReadonly22 = this._isReadonly, isShallow2 = this._isShallow;
    if (key === "__v_isReactive") {
      return !isReadonly22;
    } else if (key === "__v_isReadonly") {
      return isReadonly22;
    } else if (key === "__v_isShallow") {
      return isShallow2;
    } else if (key === "__v_raw") {
      if (receiver === (isReadonly22 ? isShallow2 ? shallowReadonlyMap : readonlyMap : isShallow2 ? shallowReactiveMap : reactiveMap).get(target2) || // receiver is not the reactive proxy, but has the same prototype
      // this means the receiver is a user proxy of the reactive proxy
      Object.getPrototypeOf(target2) === Object.getPrototypeOf(receiver)) {
        return target2;
      }
      return;
    }
    const targetIsArray = isArray(target2);
    if (!isReadonly22) {
      let fn;
      if (targetIsArray && (fn = arrayInstrumentations[key])) {
        return fn;
      }
      if (key === "hasOwnProperty") {
        return hasOwnProperty2;
      }
    }
    const res = Reflect.get(
      target2,
      key,
      // if this is a proxy wrapping a ref, return methods using the raw ref
      // as receiver so that we don't have to call `toRaw` on the ref in all
      // its class methods
      isRef(target2) ? target2 : receiver
    );
    if (isSymbol(key) ? builtInSymbols.has(key) : isNonTrackableKeys(key)) {
      return res;
    }
    if (!isReadonly22) {
      track(target2, "get", key);
    }
    if (isShallow2) {
      return res;
    }
    if (isRef(res)) {
      return targetIsArray && isIntegerKey(key) ? res : res.value;
    }
    if (isObject(res)) {
      return isReadonly22 ? readonly(res) : reactive(res);
    }
    return res;
  }
};
var MutableReactiveHandler = class extends BaseReactiveHandler {
  constructor(isShallow2 = false) {
    super(false, isShallow2);
  }
  set(target2, key, value, receiver) {
    let oldValue = target2[key];
    if (!this._isShallow) {
      const isOldValueReadonly = isReadonly(oldValue);
      if (!isShallow(value) && !isReadonly(value)) {
        oldValue = toRaw(oldValue);
        value = toRaw(value);
      }
      if (!isArray(target2) && isRef(oldValue) && !isRef(value)) {
        if (isOldValueReadonly) {
          return false;
        } else {
          oldValue.value = value;
          return true;
        }
      }
    }
    const hadKey = isArray(target2) && isIntegerKey(key) ? Number(key) < target2.length : hasOwn(target2, key);
    const result = Reflect.set(target2, key, value, isRef(target2) ? target2 : receiver);
    if (target2 === toRaw(receiver)) {
      if (!hadKey) {
        trigger(target2, "add", key, value);
      } else if (hasChanged(value, oldValue)) {
        trigger(target2, "set", key, value, oldValue);
      }
    }
    return result;
  }
  deleteProperty(target2, key) {
    const hadKey = hasOwn(target2, key);
    const oldValue = target2[key];
    const result = Reflect.deleteProperty(target2, key);
    if (result && hadKey) {
      trigger(target2, "delete", key, void 0, oldValue);
    }
    return result;
  }
  has(target2, key) {
    const result = Reflect.has(target2, key);
    if (!isSymbol(key) || !builtInSymbols.has(key)) {
      track(target2, "has", key);
    }
    return result;
  }
  ownKeys(target2) {
    track(target2, "iterate", isArray(target2) ? "length" : ITERATE_KEY);
    return Reflect.ownKeys(target2);
  }
};
var ReadonlyReactiveHandler = class extends BaseReactiveHandler {
  constructor(isShallow2 = false) {
    super(true, isShallow2);
  }
  set(target2, key) {
    if (true) {
      warn(`Set operation on key "${String(key)}" failed: target is readonly.`, target2);
    }
    return true;
  }
  deleteProperty(target2, key) {
    if (true) {
      warn(`Delete operation on key "${String(key)}" failed: target is readonly.`, target2);
    }
    return true;
  }
};
var mutableHandlers = /* @__PURE__ */ new MutableReactiveHandler();
var readonlyHandlers = /* @__PURE__ */ new ReadonlyReactiveHandler();
var shallowReactiveHandlers = /* @__PURE__ */ new MutableReactiveHandler(true);
var shallowReadonlyHandlers = /* @__PURE__ */ new ReadonlyReactiveHandler(true);
var toShallow = (value) => value;
var getProto = (v) => Reflect.getPrototypeOf(v);
function createIterableMethod(method, isReadonly22, isShallow2) {
  return function(...args) {
    const target2 = this["__v_raw"];
    const rawTarget = toRaw(target2);
    const targetIsMap = isMap(rawTarget);
    const isPair = method === "entries" || method === Symbol.iterator && targetIsMap;
    const isKeyOnly = method === "keys" && targetIsMap;
    const innerIterator = target2[method](...args);
    const wrap = isShallow2 ? toShallow : isReadonly22 ? toReadonly : toReactive;
    !isReadonly22 && track(rawTarget, "iterate", isKeyOnly ? MAP_KEY_ITERATE_KEY : ITERATE_KEY);
    return {
      // iterator protocol
      next() {
        const { value, done } = innerIterator.next();
        return done ? {
          value,
          done
        } : {
          value: isPair ? [
            wrap(value[0]),
            wrap(value[1])
          ] : wrap(value),
          done
        };
      },
      // iterable protocol
      [Symbol.iterator]() {
        return this;
      }
    };
  };
}
function createReadonlyMethod(type) {
  return function(...args) {
    if (true) {
      const key = args[0] ? `on key "${args[0]}" ` : ``;
      warn(`${capitalize(type)} operation ${key}failed: target is readonly.`, toRaw(this));
    }
    return type === "delete" ? false : type === "clear" ? void 0 : this;
  };
}
function createInstrumentations(readonly2, shallow) {
  const instrumentations = {
    get(key) {
      const target2 = this["__v_raw"];
      const rawTarget = toRaw(target2);
      const rawKey = toRaw(key);
      if (!readonly2) {
        if (hasChanged(key, rawKey)) {
          track(rawTarget, "get", key);
        }
        track(rawTarget, "get", rawKey);
      }
      const { has } = getProto(rawTarget);
      const wrap = shallow ? toShallow : readonly2 ? toReadonly : toReactive;
      if (has.call(rawTarget, key)) {
        return wrap(target2.get(key));
      } else if (has.call(rawTarget, rawKey)) {
        return wrap(target2.get(rawKey));
      } else if (target2 !== rawTarget) {
        target2.get(key);
      }
    },
    get size() {
      const target2 = this["__v_raw"];
      !readonly2 && track(toRaw(target2), "iterate", ITERATE_KEY);
      return Reflect.get(target2, "size", target2);
    },
    has(key) {
      const target2 = this["__v_raw"];
      const rawTarget = toRaw(target2);
      const rawKey = toRaw(key);
      if (!readonly2) {
        if (hasChanged(key, rawKey)) {
          track(rawTarget, "has", key);
        }
        track(rawTarget, "has", rawKey);
      }
      return key === rawKey ? target2.has(key) : target2.has(key) || target2.has(rawKey);
    },
    forEach(callback, thisArg) {
      const observed = this;
      const target2 = observed["__v_raw"];
      const rawTarget = toRaw(target2);
      const wrap = shallow ? toShallow : readonly2 ? toReadonly : toReactive;
      !readonly2 && track(rawTarget, "iterate", ITERATE_KEY);
      return target2.forEach((value, key) => {
        return callback.call(thisArg, wrap(value), wrap(key), observed);
      });
    }
  };
  extend(instrumentations, readonly2 ? {
    add: createReadonlyMethod("add"),
    set: createReadonlyMethod("set"),
    delete: createReadonlyMethod("delete"),
    clear: createReadonlyMethod("clear")
  } : {
    add(value) {
      if (!shallow && !isShallow(value) && !isReadonly(value)) {
        value = toRaw(value);
      }
      const target2 = toRaw(this);
      const proto = getProto(target2);
      const hadKey = proto.has.call(target2, value);
      if (!hadKey) {
        target2.add(value);
        trigger(target2, "add", value, value);
      }
      return this;
    },
    set(key, value) {
      if (!shallow && !isShallow(value) && !isReadonly(value)) {
        value = toRaw(value);
      }
      const target2 = toRaw(this);
      const { has, get } = getProto(target2);
      let hadKey = has.call(target2, key);
      if (!hadKey) {
        key = toRaw(key);
        hadKey = has.call(target2, key);
      } else if (true) {
        checkIdentityKeys(target2, has, key);
      }
      const oldValue = get.call(target2, key);
      target2.set(key, value);
      if (!hadKey) {
        trigger(target2, "add", key, value);
      } else if (hasChanged(value, oldValue)) {
        trigger(target2, "set", key, value, oldValue);
      }
      return this;
    },
    delete(key) {
      const target2 = toRaw(this);
      const { has, get } = getProto(target2);
      let hadKey = has.call(target2, key);
      if (!hadKey) {
        key = toRaw(key);
        hadKey = has.call(target2, key);
      } else if (true) {
        checkIdentityKeys(target2, has, key);
      }
      const oldValue = get ? get.call(target2, key) : void 0;
      const result = target2.delete(key);
      if (hadKey) {
        trigger(target2, "delete", key, void 0, oldValue);
      }
      return result;
    },
    clear() {
      const target2 = toRaw(this);
      const hadItems = target2.size !== 0;
      const oldTarget = true ? isMap(target2) ? new Map(target2) : new Set(target2) : void 0;
      const result = target2.clear();
      if (hadItems) {
        trigger(target2, "clear", void 0, void 0, oldTarget);
      }
      return result;
    }
  });
  const iteratorMethods = [
    "keys",
    "values",
    "entries",
    Symbol.iterator
  ];
  iteratorMethods.forEach((method) => {
    instrumentations[method] = createIterableMethod(method, readonly2, shallow);
  });
  return instrumentations;
}
function createInstrumentationGetter(isReadonly22, shallow) {
  const instrumentations = createInstrumentations(isReadonly22, shallow);
  return (target2, key, receiver) => {
    if (key === "__v_isReactive") {
      return !isReadonly22;
    } else if (key === "__v_isReadonly") {
      return isReadonly22;
    } else if (key === "__v_raw") {
      return target2;
    }
    return Reflect.get(hasOwn(instrumentations, key) && key in target2 ? instrumentations : target2, key, receiver);
  };
}
var mutableCollectionHandlers = {
  get: /* @__PURE__ */ createInstrumentationGetter(false, false)
};
var shallowCollectionHandlers = {
  get: /* @__PURE__ */ createInstrumentationGetter(false, true)
};
var readonlyCollectionHandlers = {
  get: /* @__PURE__ */ createInstrumentationGetter(true, false)
};
var shallowReadonlyCollectionHandlers = {
  get: /* @__PURE__ */ createInstrumentationGetter(true, true)
};
function checkIdentityKeys(target2, has, key) {
  const rawKey = toRaw(key);
  if (rawKey !== key && has.call(target2, rawKey)) {
    const type = toRawType(target2);
    warn(`Reactive ${type} contains both the raw and reactive versions of the same object${type === `Map` ? ` as keys` : ``}, which can lead to inconsistencies. Avoid differentiating between the raw and reactive versions of an object and only use the reactive version if possible.`);
  }
}
var reactiveMap = /* @__PURE__ */ new WeakMap();
var shallowReactiveMap = /* @__PURE__ */ new WeakMap();
var readonlyMap = /* @__PURE__ */ new WeakMap();
var shallowReadonlyMap = /* @__PURE__ */ new WeakMap();
function targetTypeMap(rawType) {
  switch (rawType) {
    case "Object":
    case "Array":
      return 1;
    case "Map":
    case "Set":
    case "WeakMap":
    case "WeakSet":
      return 2;
    default:
      return 0;
  }
}
function getTargetType(value) {
  return value["__v_skip"] || !Object.isExtensible(value) ? 0 : targetTypeMap(toRawType(value));
}
function reactive(target2) {
  if (isReadonly(target2)) {
    return target2;
  }
  return createReactiveObject(target2, false, mutableHandlers, mutableCollectionHandlers, reactiveMap);
}
function shallowReactive(target2) {
  return createReactiveObject(target2, false, shallowReactiveHandlers, shallowCollectionHandlers, shallowReactiveMap);
}
function readonly(target2) {
  return createReactiveObject(target2, true, readonlyHandlers, readonlyCollectionHandlers, readonlyMap);
}
function shallowReadonly(target2) {
  return createReactiveObject(target2, true, shallowReadonlyHandlers, shallowReadonlyCollectionHandlers, shallowReadonlyMap);
}
function createReactiveObject(target2, isReadonly22, baseHandlers, collectionHandlers, proxyMap) {
  if (!isObject(target2)) {
    if (true) {
      warn(`value cannot be made ${isReadonly22 ? "readonly" : "reactive"}: ${String(target2)}`);
    }
    return target2;
  }
  if (target2["__v_raw"] && !(isReadonly22 && target2["__v_isReactive"])) {
    return target2;
  }
  const existingProxy = proxyMap.get(target2);
  if (existingProxy) {
    return existingProxy;
  }
  const targetType = getTargetType(target2);
  if (targetType === 0) {
    return target2;
  }
  const proxy = new Proxy(target2, targetType === 2 ? collectionHandlers : baseHandlers);
  proxyMap.set(target2, proxy);
  return proxy;
}
function isReactive(value) {
  if (isReadonly(value)) {
    return isReactive(value["__v_raw"]);
  }
  return !!(value && value["__v_isReactive"]);
}
function isReadonly(value) {
  return !!(value && value["__v_isReadonly"]);
}
function isShallow(value) {
  return !!(value && value["__v_isShallow"]);
}
function isProxy(value) {
  return value ? !!value["__v_raw"] : false;
}
function toRaw(observed) {
  const raw = observed && observed["__v_raw"];
  return raw ? toRaw(raw) : observed;
}
function markRaw(value) {
  if (!hasOwn(value, "__v_skip") && Object.isExtensible(value)) {
    def(value, "__v_skip", true);
  }
  return value;
}
var toReactive = (value) => isObject(value) ? reactive(value) : value;
var toReadonly = (value) => isObject(value) ? readonly(value) : value;
function isRef(r) {
  return r ? r["__v_isRef"] === true : false;
}
function ref(value) {
  return createRef(value, false);
}
function createRef(rawValue, shallow) {
  if (isRef(rawValue)) {
    return rawValue;
  }
  return new RefImpl(rawValue, shallow);
}
var RefImpl = class {
  constructor(value, isShallow2) {
    this.dep = new Dep();
    this["__v_isRef"] = true;
    this["__v_isShallow"] = false;
    this._rawValue = isShallow2 ? value : toRaw(value);
    this._value = isShallow2 ? value : toReactive(value);
    this["__v_isShallow"] = isShallow2;
  }
  get value() {
    if (true) {
      this.dep.track({
        target: this,
        type: "get",
        key: "value"
      });
    } else {
      this.dep.track();
    }
    return this._value;
  }
  set value(newValue) {
    const oldValue = this._rawValue;
    const useDirectValue = this["__v_isShallow"] || isShallow(newValue) || isReadonly(newValue);
    newValue = useDirectValue ? newValue : toRaw(newValue);
    if (hasChanged(newValue, oldValue)) {
      this._rawValue = newValue;
      this._value = useDirectValue ? newValue : toReactive(newValue);
      if (true) {
        this.dep.trigger({
          target: this,
          type: "set",
          key: "value",
          newValue,
          oldValue
        });
      } else {
        this.dep.trigger();
      }
    }
  }
};
function unref(ref2) {
  return isRef(ref2) ? ref2.value : ref2;
}
var shallowUnwrapHandlers = {
  get: (target2, key, receiver) => key === "__v_raw" ? target2 : unref(Reflect.get(target2, key, receiver)),
  set: (target2, key, value, receiver) => {
    const oldValue = target2[key];
    if (isRef(oldValue) && !isRef(value)) {
      oldValue.value = value;
      return true;
    } else {
      return Reflect.set(target2, key, value, receiver);
    }
  }
};
function proxyRefs(objectWithRefs) {
  return isReactive(objectWithRefs) ? objectWithRefs : new Proxy(objectWithRefs, shallowUnwrapHandlers);
}
function toRefs(object) {
  if (!isProxy(object)) {
    warn(`toRefs() expects a reactive object but received a plain one.`);
  }
  const ret = isArray(object) ? new Array(object.length) : {};
  for (const key in object) {
    ret[key] = propertyToRef(object, key);
  }
  return ret;
}
var ObjectRefImpl = class {
  constructor(_object, _key, _defaultValue) {
    this._object = _object;
    this._key = _key;
    this._defaultValue = _defaultValue;
    this["__v_isRef"] = true;
    this._value = void 0;
  }
  get value() {
    const val = this._object[this._key];
    return this._value = val === void 0 ? this._defaultValue : val;
  }
  set value(newVal) {
    this._object[this._key] = newVal;
  }
  get dep() {
    return getDepFromReactive(toRaw(this._object), this._key);
  }
};
var GetterRefImpl = class {
  constructor(_getter) {
    this._getter = _getter;
    this["__v_isRef"] = true;
    this["__v_isReadonly"] = true;
    this._value = void 0;
  }
  get value() {
    return this._value = this._getter();
  }
};
function toRef(source, key, defaultValue) {
  if (isRef(source)) {
    return source;
  } else if (isFunction(source)) {
    return new GetterRefImpl(source);
  } else if (isObject(source) && arguments.length > 1) {
    return propertyToRef(source, key, defaultValue);
  } else {
    return ref(source);
  }
}
function propertyToRef(source, key, defaultValue) {
  const val = source[key];
  return isRef(val) ? val : new ObjectRefImpl(source, key, defaultValue);
}
var ComputedRefImpl = class {
  constructor(fn, setter, isSSR) {
    this.fn = fn;
    this.setter = setter;
    this._value = void 0;
    this.dep = new Dep(this);
    this.__v_isRef = true;
    this.deps = void 0;
    this.depsTail = void 0;
    this.flags = 16;
    this.globalVersion = globalVersion - 1;
    this.next = void 0;
    this.effect = this;
    this["__v_isReadonly"] = !setter;
    this.isSSR = isSSR;
  }
  /**
   * @internal
   */
  notify() {
    this.flags |= 16;
    if (!(this.flags & 8) && // avoid infinite self recursion
    activeSub !== this) {
      batch(this, true);
      return true;
    } else if (true) ;
  }
  get value() {
    const link = true ? this.dep.track({
      target: this,
      type: "get",
      key: "value"
    }) : this.dep.track();
    refreshComputed(this);
    if (link) {
      link.version = this.dep.version;
    }
    return this._value;
  }
  set value(newValue) {
    if (this.setter) {
      this.setter(newValue);
    } else if (true) {
      warn("Write operation failed: computed value is readonly");
    }
  }
};
function computed(getterOrOptions, debugOptions, isSSR = false) {
  let getter;
  let setter;
  if (isFunction(getterOrOptions)) {
    getter = getterOrOptions;
  } else {
    getter = getterOrOptions.get;
    setter = getterOrOptions.set;
  }
  const cRef = new ComputedRefImpl(getter, setter, isSSR);
  if (debugOptions && !isSSR) {
    cRef.onTrack = debugOptions.onTrack;
    cRef.onTrigger = debugOptions.onTrigger;
  }
  return cRef;
}
var INITIAL_WATCHER_VALUE = {};
var cleanupMap = /* @__PURE__ */ new WeakMap();
var activeWatcher = void 0;
function onWatcherCleanup(cleanupFn, failSilently = false, owner = activeWatcher) {
  if (owner) {
    let cleanups = cleanupMap.get(owner);
    if (!cleanups) cleanupMap.set(owner, cleanups = []);
    cleanups.push(cleanupFn);
  } else if (!failSilently) {
    warn(`onWatcherCleanup() was called when there was no active watcher to associate with.`);
  }
}
function watch(source, cb, options = EMPTY_OBJ) {
  const { immediate, deep, once, scheduler, augmentJob, call } = options;
  const warnInvalidSource = (s) => {
    (options.onWarn || warn)(`Invalid watch source: `, s, `A watch source can only be a getter/effect function, a ref, a reactive object, or an array of these types.`);
  };
  const reactiveGetter = (source2) => {
    if (deep) return source2;
    if (isShallow(source2) || deep === false || deep === 0) return traverse(source2, 1);
    return traverse(source2);
  };
  let effect2;
  let getter;
  let cleanup;
  let boundCleanup;
  let forceTrigger = false;
  let isMultiSource = false;
  if (isRef(source)) {
    getter = () => source.value;
    forceTrigger = isShallow(source);
  } else if (isReactive(source)) {
    getter = () => reactiveGetter(source);
    forceTrigger = true;
  } else if (isArray(source)) {
    isMultiSource = true;
    forceTrigger = source.some((s) => isReactive(s) || isShallow(s));
    getter = () => source.map((s) => {
      if (isRef(s)) {
        return s.value;
      } else if (isReactive(s)) {
        return reactiveGetter(s);
      } else if (isFunction(s)) {
        return call ? call(s, 2) : s();
      } else {
        warnInvalidSource(s);
      }
    });
  } else if (isFunction(source)) {
    if (cb) {
      getter = call ? () => call(source, 2) : source;
    } else {
      getter = () => {
        if (cleanup) {
          pauseTracking();
          try {
            cleanup();
          } finally {
            resetTracking();
          }
        }
        const currentEffect = activeWatcher;
        activeWatcher = effect2;
        try {
          return call ? call(source, 3, [
            boundCleanup
          ]) : source(boundCleanup);
        } finally {
          activeWatcher = currentEffect;
        }
      };
    }
  } else {
    getter = NOOP;
    warnInvalidSource(source);
  }
  if (cb && deep) {
    const baseGetter = getter;
    const depth = deep === true ? Infinity : deep;
    getter = () => traverse(baseGetter(), depth);
  }
  const scope = getCurrentScope();
  const watchHandle = () => {
    effect2.stop();
    if (scope && scope.active) {
      remove(scope.effects, effect2);
    }
  };
  if (once && cb) {
    const _cb = cb;
    cb = (...args) => {
      _cb(...args);
      watchHandle();
    };
  }
  let oldValue = isMultiSource ? new Array(source.length).fill(INITIAL_WATCHER_VALUE) : INITIAL_WATCHER_VALUE;
  const job = (immediateFirstRun) => {
    if (!(effect2.flags & 1) || !effect2.dirty && !immediateFirstRun) {
      return;
    }
    if (cb) {
      const newValue = effect2.run();
      if (deep || forceTrigger || (isMultiSource ? newValue.some((v, i) => hasChanged(v, oldValue[i])) : hasChanged(newValue, oldValue))) {
        if (cleanup) {
          cleanup();
        }
        const currentWatcher = activeWatcher;
        activeWatcher = effect2;
        try {
          const args = [
            newValue,
            // pass undefined as the old value when it's changed for the first time
            oldValue === INITIAL_WATCHER_VALUE ? void 0 : isMultiSource && oldValue[0] === INITIAL_WATCHER_VALUE ? [] : oldValue,
            boundCleanup
          ];
          call ? call(cb, 3, args) : (
            // @ts-expect-error
            cb(...args)
          );
          oldValue = newValue;
        } finally {
          activeWatcher = currentWatcher;
        }
      }
    } else {
      effect2.run();
    }
  };
  if (augmentJob) {
    augmentJob(job);
  }
  effect2 = new ReactiveEffect(getter);
  effect2.scheduler = scheduler ? () => scheduler(job, false) : job;
  boundCleanup = (fn) => onWatcherCleanup(fn, false, effect2);
  cleanup = effect2.onStop = () => {
    const cleanups = cleanupMap.get(effect2);
    if (cleanups) {
      if (call) {
        call(cleanups, 4);
      } else {
        for (const cleanup2 of cleanups) cleanup2();
      }
      cleanupMap.delete(effect2);
    }
  };
  if (true) {
    effect2.onTrack = options.onTrack;
    effect2.onTrigger = options.onTrigger;
  }
  if (cb) {
    if (immediate) {
      job(true);
    } else {
      oldValue = effect2.run();
    }
  } else if (scheduler) {
    scheduler(job.bind(null, true), true);
  } else {
    effect2.run();
  }
  watchHandle.pause = effect2.pause.bind(effect2);
  watchHandle.resume = effect2.resume.bind(effect2);
  watchHandle.stop = watchHandle;
  return watchHandle;
}
function traverse(value, depth = Infinity, seen) {
  if (depth <= 0 || !isObject(value) || value["__v_skip"]) {
    return value;
  }
  seen = seen || /* @__PURE__ */ new Set();
  if (seen.has(value)) {
    return value;
  }
  seen.add(value);
  depth--;
  if (isRef(value)) {
    traverse(value.value, depth, seen);
  } else if (isArray(value)) {
    for (let i = 0; i < value.length; i++) {
      traverse(value[i], depth, seen);
    }
  } else if (isSet(value) || isMap(value)) {
    value.forEach((v) => {
      traverse(v, depth, seen);
    });
  } else if (isPlainObject(value)) {
    for (const key in value) {
      traverse(value[key], depth, seen);
    }
    for (const key of Object.getOwnPropertySymbols(value)) {
      if (Object.prototype.propertyIsEnumerable.call(value, key)) {
        traverse(value[key], depth, seen);
      }
    }
  }
  return value;
}

// node_modules/.deno/@vue+runtime-core@3.5.13/node_modules/@vue/runtime-core/dist/runtime-core.esm-bundler.js
var stack = [];
function pushWarningContext(vnode) {
  stack.push(vnode);
}
function popWarningContext() {
  stack.pop();
}
var isWarning = false;
function warn$1(msg, ...args) {
  if (isWarning) return;
  isWarning = true;
  pauseTracking();
  const instance = stack.length ? stack[stack.length - 1].component : null;
  const appWarnHandler = instance && instance.appContext.config.warnHandler;
  const trace = getComponentTrace();
  if (appWarnHandler) {
    callWithErrorHandling(appWarnHandler, instance, 11, [
      // eslint-disable-next-line no-restricted-syntax
      msg + args.map((a) => {
        var _a25, _b25;
        return (_b25 = (_a25 = a.toString) == null ? void 0 : _a25.call(a)) != null ? _b25 : JSON.stringify(a);
      }).join(""),
      instance && instance.proxy,
      trace.map(({ vnode }) => `at <${formatComponentName(instance, vnode.type)}>`).join("\n"),
      trace
    ]);
  } else {
    const warnArgs = [
      `[Vue warn]: ${msg}`,
      ...args
    ];
    if (trace.length && // avoid spamming console during tests
    true) {
      warnArgs.push(`
`, ...formatTrace(trace));
    }
    console.warn(...warnArgs);
  }
  resetTracking();
  isWarning = false;
}
function getComponentTrace() {
  let currentVNode = stack[stack.length - 1];
  if (!currentVNode) {
    return [];
  }
  const normalizedStack = [];
  while (currentVNode) {
    const last = normalizedStack[0];
    if (last && last.vnode === currentVNode) {
      last.recurseCount++;
    } else {
      normalizedStack.push({
        vnode: currentVNode,
        recurseCount: 0
      });
    }
    const parentInstance = currentVNode.component && currentVNode.component.parent;
    currentVNode = parentInstance && parentInstance.vnode;
  }
  return normalizedStack;
}
function formatTrace(trace) {
  const logs = [];
  trace.forEach((entry, i) => {
    logs.push(...i === 0 ? [] : [
      `
`
    ], ...formatTraceEntry(entry));
  });
  return logs;
}
function formatTraceEntry({ vnode, recurseCount }) {
  const postfix = recurseCount > 0 ? `... (${recurseCount} recursive calls)` : ``;
  const isRoot = vnode.component ? vnode.component.parent == null : false;
  const open2 = ` at <${formatComponentName(vnode.component, vnode.type, isRoot)}`;
  const close = `>` + postfix;
  return vnode.props ? [
    open2,
    ...formatProps(vnode.props),
    close
  ] : [
    open2 + close
  ];
}
function formatProps(props) {
  const res = [];
  const keys = Object.keys(props);
  keys.slice(0, 3).forEach((key) => {
    res.push(...formatProp(key, props[key]));
  });
  if (keys.length > 3) {
    res.push(` ...`);
  }
  return res;
}
function formatProp(key, value, raw) {
  if (isString(value)) {
    value = JSON.stringify(value);
    return raw ? value : [
      `${key}=${value}`
    ];
  } else if (typeof value === "number" || typeof value === "boolean" || value == null) {
    return raw ? value : [
      `${key}=${value}`
    ];
  } else if (isRef(value)) {
    value = formatProp(key, toRaw(value.value), true);
    return raw ? value : [
      `${key}=Ref<`,
      value,
      `>`
    ];
  } else if (isFunction(value)) {
    return [
      `${key}=fn${value.name ? `<${value.name}>` : ``}`
    ];
  } else {
    value = toRaw(value);
    return raw ? value : [
      `${key}=`,
      value
    ];
  }
}
var ErrorTypeStrings$1 = {
  ["sp"]: "serverPrefetch hook",
  ["bc"]: "beforeCreate hook",
  ["c"]: "created hook",
  ["bm"]: "beforeMount hook",
  ["m"]: "mounted hook",
  ["bu"]: "beforeUpdate hook",
  ["u"]: "updated",
  ["bum"]: "beforeUnmount hook",
  ["um"]: "unmounted hook",
  ["a"]: "activated hook",
  ["da"]: "deactivated hook",
  ["ec"]: "errorCaptured hook",
  ["rtc"]: "renderTracked hook",
  ["rtg"]: "renderTriggered hook",
  [0]: "setup function",
  [1]: "render function",
  [2]: "watcher getter",
  [3]: "watcher callback",
  [4]: "watcher cleanup function",
  [5]: "native event handler",
  [6]: "component event handler",
  [7]: "vnode hook",
  [8]: "directive hook",
  [9]: "transition hook",
  [10]: "app errorHandler",
  [11]: "app warnHandler",
  [12]: "ref function",
  [13]: "async component loader",
  [14]: "scheduler flush",
  [15]: "component update",
  [16]: "app unmount cleanup function"
};
function callWithErrorHandling(fn, instance, type, args) {
  try {
    return args ? fn(...args) : fn();
  } catch (err) {
    handleError(err, instance, type);
  }
}
function callWithAsyncErrorHandling(fn, instance, type, args) {
  if (isFunction(fn)) {
    const res = callWithErrorHandling(fn, instance, type, args);
    if (res && isPromise(res)) {
      res.catch((err) => {
        handleError(err, instance, type);
      });
    }
    return res;
  }
  if (isArray(fn)) {
    const values = [];
    for (let i = 0; i < fn.length; i++) {
      values.push(callWithAsyncErrorHandling(fn[i], instance, type, args));
    }
    return values;
  } else if (true) {
    warn$1(`Invalid value type passed to callWithAsyncErrorHandling(): ${typeof fn}`);
  }
}
function handleError(err, instance, type, throwInDev = true) {
  const contextVNode = instance ? instance.vnode : null;
  const { errorHandler, throwUnhandledErrorInProduction } = instance && instance.appContext.config || EMPTY_OBJ;
  if (instance) {
    let cur = instance.parent;
    const exposedInstance = instance.proxy;
    const errorInfo = true ? ErrorTypeStrings$1[type] : `https://vuejs.org/error-reference/#runtime-${type}`;
    while (cur) {
      const errorCapturedHooks = cur.ec;
      if (errorCapturedHooks) {
        for (let i = 0; i < errorCapturedHooks.length; i++) {
          if (errorCapturedHooks[i](err, exposedInstance, errorInfo) === false) {
            return;
          }
        }
      }
      cur = cur.parent;
    }
    if (errorHandler) {
      pauseTracking();
      callWithErrorHandling(errorHandler, null, 10, [
        err,
        exposedInstance,
        errorInfo
      ]);
      resetTracking();
      return;
    }
  }
  logError(err, type, contextVNode, throwInDev, throwUnhandledErrorInProduction);
}
function logError(err, type, contextVNode, throwInDev = true, throwInProd = false) {
  if (true) {
    const info = ErrorTypeStrings$1[type];
    if (contextVNode) {
      pushWarningContext(contextVNode);
    }
    warn$1(`Unhandled error${info ? ` during execution of ${info}` : ``}`);
    if (contextVNode) {
      popWarningContext();
    }
    if (throwInDev) {
      throw err;
    } else {
      console.error(err);
    }
  } else if (throwInProd) {
    throw err;
  } else {
    console.error(err);
  }
}
var queue = [];
var flushIndex = -1;
var pendingPostFlushCbs = [];
var activePostFlushCbs = null;
var postFlushIndex = 0;
var resolvedPromise = /* @__PURE__ */ Promise.resolve();
var currentFlushPromise = null;
var RECURSION_LIMIT = 100;
function nextTick(fn) {
  const p2 = currentFlushPromise || resolvedPromise;
  return fn ? p2.then(this ? fn.bind(this) : fn) : p2;
}
function findInsertionIndex(id) {
  let start = flushIndex + 1;
  let end = queue.length;
  while (start < end) {
    const middle = start + end >>> 1;
    const middleJob = queue[middle];
    const middleJobId = getId(middleJob);
    if (middleJobId < id || middleJobId === id && middleJob.flags & 2) {
      start = middle + 1;
    } else {
      end = middle;
    }
  }
  return start;
}
function queueJob(job) {
  if (!(job.flags & 1)) {
    const jobId = getId(job);
    const lastJob = queue[queue.length - 1];
    if (!lastJob || // fast path when the job id is larger than the tail
    !(job.flags & 2) && jobId >= getId(lastJob)) {
      queue.push(job);
    } else {
      queue.splice(findInsertionIndex(jobId), 0, job);
    }
    job.flags |= 1;
    queueFlush();
  }
}
function queueFlush() {
  if (!currentFlushPromise) {
    currentFlushPromise = resolvedPromise.then(flushJobs);
  }
}
function queuePostFlushCb(cb) {
  if (!isArray(cb)) {
    if (activePostFlushCbs && cb.id === -1) {
      activePostFlushCbs.splice(postFlushIndex + 1, 0, cb);
    } else if (!(cb.flags & 1)) {
      pendingPostFlushCbs.push(cb);
      cb.flags |= 1;
    }
  } else {
    pendingPostFlushCbs.push(...cb);
  }
  queueFlush();
}
function flushPreFlushCbs(instance, seen, i = flushIndex + 1) {
  if (true) {
    seen = seen || /* @__PURE__ */ new Map();
  }
  for (; i < queue.length; i++) {
    const cb = queue[i];
    if (cb && cb.flags & 2) {
      if (instance && cb.id !== instance.uid) {
        continue;
      }
      if (checkRecursiveUpdates(seen, cb)) {
        continue;
      }
      queue.splice(i, 1);
      i--;
      if (cb.flags & 4) {
        cb.flags &= ~1;
      }
      cb();
      if (!(cb.flags & 4)) {
        cb.flags &= ~1;
      }
    }
  }
}
function flushPostFlushCbs(seen) {
  if (pendingPostFlushCbs.length) {
    const deduped = [
      ...new Set(pendingPostFlushCbs)
    ].sort((a, b) => getId(a) - getId(b));
    pendingPostFlushCbs.length = 0;
    if (activePostFlushCbs) {
      activePostFlushCbs.push(...deduped);
      return;
    }
    activePostFlushCbs = deduped;
    if (true) {
      seen = seen || /* @__PURE__ */ new Map();
    }
    for (postFlushIndex = 0; postFlushIndex < activePostFlushCbs.length; postFlushIndex++) {
      const cb = activePostFlushCbs[postFlushIndex];
      if (checkRecursiveUpdates(seen, cb)) {
        continue;
      }
      if (cb.flags & 4) {
        cb.flags &= ~1;
      }
      if (!(cb.flags & 8)) cb();
      cb.flags &= ~1;
    }
    activePostFlushCbs = null;
    postFlushIndex = 0;
  }
}
var getId = (job) => job.id == null ? job.flags & 2 ? -1 : Infinity : job.id;
function flushJobs(seen) {
  if (true) {
    seen = seen || /* @__PURE__ */ new Map();
  }
  const check = true ? (job) => checkRecursiveUpdates(seen, job) : NOOP;
  try {
    for (flushIndex = 0; flushIndex < queue.length; flushIndex++) {
      const job = queue[flushIndex];
      if (job && !(job.flags & 8)) {
        if (check(job)) {
          continue;
        }
        if (job.flags & 4) {
          job.flags &= ~1;
        }
        callWithErrorHandling(job, job.i, job.i ? 15 : 14);
        if (!(job.flags & 4)) {
          job.flags &= ~1;
        }
      }
    }
  } finally {
    for (; flushIndex < queue.length; flushIndex++) {
      const job = queue[flushIndex];
      if (job) {
        job.flags &= ~1;
      }
    }
    flushIndex = -1;
    queue.length = 0;
    flushPostFlushCbs(seen);
    currentFlushPromise = null;
    if (queue.length || pendingPostFlushCbs.length) {
      flushJobs(seen);
    }
  }
}
function checkRecursiveUpdates(seen, fn) {
  const count = seen.get(fn) || 0;
  if (count > RECURSION_LIMIT) {
    const instance = fn.i;
    const componentName = instance && getComponentName(instance.type);
    handleError(`Maximum recursive updates exceeded${componentName ? ` in component <${componentName}>` : ``}. This means you have a reactive effect that is mutating its own dependencies and thus recursively triggering itself. Possible sources include component template, render function, updated hook or watcher source function.`, null, 10);
    return true;
  }
  seen.set(fn, count + 1);
  return false;
}
var isHmrUpdating = false;
var hmrDirtyComponents = /* @__PURE__ */ new Map();
if (true) {
  getGlobalThis().__VUE_HMR_RUNTIME__ = {
    createRecord: tryWrap(createRecord),
    rerender: tryWrap(rerender),
    reload: tryWrap(reload)
  };
}
var map = /* @__PURE__ */ new Map();
function registerHMR(instance) {
  const id = instance.type.__hmrId;
  let record = map.get(id);
  if (!record) {
    createRecord(id, instance.type);
    record = map.get(id);
  }
  record.instances.add(instance);
}
function unregisterHMR(instance) {
  map.get(instance.type.__hmrId).instances.delete(instance);
}
function createRecord(id, initialDef) {
  if (map.has(id)) {
    return false;
  }
  map.set(id, {
    initialDef: normalizeClassComponent(initialDef),
    instances: /* @__PURE__ */ new Set()
  });
  return true;
}
function normalizeClassComponent(component8) {
  return isClassComponent(component8) ? component8.__vccOpts : component8;
}
function rerender(id, newRender) {
  const record = map.get(id);
  if (!record) {
    return;
  }
  record.initialDef.render = newRender;
  [
    ...record.instances
  ].forEach((instance) => {
    if (newRender) {
      instance.render = newRender;
      normalizeClassComponent(instance.type).render = newRender;
    }
    instance.renderCache = [];
    isHmrUpdating = true;
    instance.update();
    isHmrUpdating = false;
  });
}
function reload(id, newComp) {
  const record = map.get(id);
  if (!record) return;
  newComp = normalizeClassComponent(newComp);
  updateComponentDef(record.initialDef, newComp);
  const instances = [
    ...record.instances
  ];
  for (let i = 0; i < instances.length; i++) {
    const instance = instances[i];
    const oldComp = normalizeClassComponent(instance.type);
    let dirtyInstances = hmrDirtyComponents.get(oldComp);
    if (!dirtyInstances) {
      if (oldComp !== record.initialDef) {
        updateComponentDef(oldComp, newComp);
      }
      hmrDirtyComponents.set(oldComp, dirtyInstances = /* @__PURE__ */ new Set());
    }
    dirtyInstances.add(instance);
    instance.appContext.propsCache.delete(instance.type);
    instance.appContext.emitsCache.delete(instance.type);
    instance.appContext.optionsCache.delete(instance.type);
    if (instance.ceReload) {
      dirtyInstances.add(instance);
      instance.ceReload(newComp.styles);
      dirtyInstances.delete(instance);
    } else if (instance.parent) {
      queueJob(() => {
        isHmrUpdating = true;
        instance.parent.update();
        isHmrUpdating = false;
        dirtyInstances.delete(instance);
      });
    } else if (instance.appContext.reload) {
      instance.appContext.reload();
    } else if (typeof window !== "undefined") {
      window.location.reload();
    } else {
      console.warn("[HMR] Root or manually mounted instance modified. Full reload required.");
    }
    if (instance.root.ce && instance !== instance.root) {
      instance.root.ce._removeChildStyle(oldComp);
    }
  }
  queuePostFlushCb(() => {
    hmrDirtyComponents.clear();
  });
}
function updateComponentDef(oldComp, newComp) {
  extend(oldComp, newComp);
  for (const key in oldComp) {
    if (key !== "__file" && !(key in newComp)) {
      delete oldComp[key];
    }
  }
}
function tryWrap(fn) {
  return (id, arg) => {
    try {
      return fn(id, arg);
    } catch (e) {
      console.error(e);
      console.warn(`[HMR] Something went wrong during Vue component hot-reload. Full reload required.`);
    }
  };
}
var devtools$1;
var buffer = [];
var devtoolsNotInstalled = false;
function emit$1(event, ...args) {
  if (devtools$1) {
    devtools$1.emit(event, ...args);
  } else if (!devtoolsNotInstalled) {
    buffer.push({
      event,
      args
    });
  }
}
function setDevtoolsHook$1(hook2, target2) {
  var _a25, _b25;
  devtools$1 = hook2;
  if (devtools$1) {
    devtools$1.enabled = true;
    buffer.forEach(({ event, args }) => devtools$1.emit(event, ...args));
    buffer = [];
  } else if (
    // handle late devtools injection - only do this if we are in an actual
    // browser environment to avoid the timer handle stalling test runner exit
    // (#4815)
    typeof window !== "undefined" && // some envs mock window but not fully
    window.HTMLElement && // also exclude jsdom
    // eslint-disable-next-line no-restricted-syntax
    !((_b25 = (_a25 = window.navigator) == null ? void 0 : _a25.userAgent) == null ? void 0 : _b25.includes("jsdom"))
  ) {
    const replay = target2.__VUE_DEVTOOLS_HOOK_REPLAY__ = target2.__VUE_DEVTOOLS_HOOK_REPLAY__ || [];
    replay.push((newHook) => {
      setDevtoolsHook$1(newHook, target2);
    });
    setTimeout(() => {
      if (!devtools$1) {
        target2.__VUE_DEVTOOLS_HOOK_REPLAY__ = null;
        devtoolsNotInstalled = true;
        buffer = [];
      }
    }, 3e3);
  } else {
    devtoolsNotInstalled = true;
    buffer = [];
  }
}
function devtoolsInitApp(app, version2) {
  emit$1("app:init", app, version2, {
    Fragment,
    Text,
    Comment,
    Static
  });
}
function devtoolsUnmountApp(app) {
  emit$1("app:unmount", app);
}
var devtoolsComponentAdded = /* @__PURE__ */ createDevtoolsComponentHook(
  "component:added"
  /* COMPONENT_ADDED */
);
var devtoolsComponentUpdated = /* @__PURE__ */ createDevtoolsComponentHook(
  "component:updated"
  /* COMPONENT_UPDATED */
);
var _devtoolsComponentRemoved = /* @__PURE__ */ createDevtoolsComponentHook(
  "component:removed"
  /* COMPONENT_REMOVED */
);
var devtoolsComponentRemoved = (component8) => {
  if (devtools$1 && typeof devtools$1.cleanupBuffer === "function" && // remove the component if it wasn't buffered
  !devtools$1.cleanupBuffer(component8)) {
    _devtoolsComponentRemoved(component8);
  }
};
// @__NO_SIDE_EFFECTS__
function createDevtoolsComponentHook(hook2) {
  return (component8) => {
    emit$1(hook2, component8.appContext.app, component8.uid, component8.parent ? component8.parent.uid : void 0, component8);
  };
}
var devtoolsPerfStart = /* @__PURE__ */ createDevtoolsPerformanceHook(
  "perf:start"
  /* PERFORMANCE_START */
);
var devtoolsPerfEnd = /* @__PURE__ */ createDevtoolsPerformanceHook(
  "perf:end"
  /* PERFORMANCE_END */
);
function createDevtoolsPerformanceHook(hook2) {
  return (component8, type, time) => {
    emit$1(hook2, component8.appContext.app, component8.uid, component8, type, time);
  };
}
function devtoolsComponentEmit(component8, event, params) {
  emit$1("component:emit", component8.appContext.app, component8, event, params);
}
var currentRenderingInstance = null;
var currentScopeId = null;
function setCurrentRenderingInstance(instance) {
  const prev = currentRenderingInstance;
  currentRenderingInstance = instance;
  currentScopeId = instance && instance.type.__scopeId || null;
  return prev;
}
function withCtx(fn, ctx = currentRenderingInstance, isNonScopedSlot) {
  if (!ctx) return fn;
  if (fn._n) {
    return fn;
  }
  const renderFnWithContext = (...args) => {
    if (renderFnWithContext._d) {
      setBlockTracking(-1);
    }
    const prevInstance = setCurrentRenderingInstance(ctx);
    let res;
    try {
      res = fn(...args);
    } finally {
      setCurrentRenderingInstance(prevInstance);
      if (renderFnWithContext._d) {
        setBlockTracking(1);
      }
    }
    if (true) {
      devtoolsComponentUpdated(ctx);
    }
    return res;
  };
  renderFnWithContext._n = true;
  renderFnWithContext._c = true;
  renderFnWithContext._d = true;
  return renderFnWithContext;
}
function validateDirectiveName(name) {
  if (isBuiltInDirective(name)) {
    warn$1("Do not use built-in directive ids as custom directive id: " + name);
  }
}
function invokeDirectiveHook(vnode, prevVNode, instance, name) {
  const bindings = vnode.dirs;
  const oldBindings = prevVNode && prevVNode.dirs;
  for (let i = 0; i < bindings.length; i++) {
    const binding = bindings[i];
    if (oldBindings) {
      binding.oldValue = oldBindings[i].value;
    }
    let hook2 = binding.dir[name];
    if (hook2) {
      pauseTracking();
      callWithAsyncErrorHandling(hook2, instance, 8, [
        vnode.el,
        binding,
        vnode,
        prevVNode
      ]);
      resetTracking();
    }
  }
}
var TeleportEndKey = Symbol("_vte");
var isTeleport = (type) => type.__isTeleport;
var leaveCbKey = Symbol("_leaveCb");
var enterCbKey = Symbol("_enterCb");
function setTransitionHooks(vnode, hooks2) {
  if (vnode.shapeFlag & 6 && vnode.component) {
    vnode.transition = hooks2;
    setTransitionHooks(vnode.component.subTree, hooks2);
  } else if (vnode.shapeFlag & 128) {
    vnode.ssContent.transition = hooks2.clone(vnode.ssContent);
    vnode.ssFallback.transition = hooks2.clone(vnode.ssFallback);
  } else {
    vnode.transition = hooks2;
  }
}
// @__NO_SIDE_EFFECTS__
function defineComponent(options, extraOptions) {
  return isFunction(options) ? (
    // #8236: extend call and options.name access are considered side-effects
    // by Rollup, so we have to wrap it in a pure-annotated IIFE.
    /* @__PURE__ */ (() => extend({
      name: options.name
    }, extraOptions, {
      setup: options
    }))()
  ) : options;
}
function markAsyncBoundary(instance) {
  instance.ids = [
    instance.ids[0] + instance.ids[2]++ + "-",
    0,
    0
  ];
}
var knownTemplateRefs = /* @__PURE__ */ new WeakSet();
function setRef(rawRef, oldRawRef, parentSuspense, vnode, isUnmount = false) {
  if (isArray(rawRef)) {
    rawRef.forEach((r, i) => setRef(r, oldRawRef && (isArray(oldRawRef) ? oldRawRef[i] : oldRawRef), parentSuspense, vnode, isUnmount));
    return;
  }
  if (isAsyncWrapper(vnode) && !isUnmount) {
    if (vnode.shapeFlag & 512 && vnode.type.__asyncResolved && vnode.component.subTree.component) {
      setRef(rawRef, oldRawRef, parentSuspense, vnode.component.subTree);
    }
    return;
  }
  const refValue = vnode.shapeFlag & 4 ? getComponentPublicInstance(vnode.component) : vnode.el;
  const value = isUnmount ? null : refValue;
  const { i: owner, r: ref2 } = rawRef;
  if (!owner) {
    warn$1(`Missing ref owner context. ref cannot be used on hoisted vnodes. A vnode with ref must be created inside the render function.`);
    return;
  }
  const oldRef = oldRawRef && oldRawRef.r;
  const refs = owner.refs === EMPTY_OBJ ? owner.refs = {} : owner.refs;
  const setupState = owner.setupState;
  const rawSetupState = toRaw(setupState);
  const canSetSetupRef = setupState === EMPTY_OBJ ? () => false : (key) => {
    if (true) {
      if (hasOwn(rawSetupState, key) && !isRef(rawSetupState[key])) {
        warn$1(`Template ref "${key}" used on a non-ref value. It will not work in the production build.`);
      }
      if (knownTemplateRefs.has(rawSetupState[key])) {
        return false;
      }
    }
    return hasOwn(rawSetupState, key);
  };
  if (oldRef != null && oldRef !== ref2) {
    if (isString(oldRef)) {
      refs[oldRef] = null;
      if (canSetSetupRef(oldRef)) {
        setupState[oldRef] = null;
      }
    } else if (isRef(oldRef)) {
      oldRef.value = null;
    }
  }
  if (isFunction(ref2)) {
    callWithErrorHandling(ref2, owner, 12, [
      value,
      refs
    ]);
  } else {
    const _isString = isString(ref2);
    const _isRef = isRef(ref2);
    if (_isString || _isRef) {
      const doSet = () => {
        if (rawRef.f) {
          const existing = _isString ? canSetSetupRef(ref2) ? setupState[ref2] : refs[ref2] : ref2.value;
          if (isUnmount) {
            isArray(existing) && remove(existing, refValue);
          } else {
            if (!isArray(existing)) {
              if (_isString) {
                refs[ref2] = [
                  refValue
                ];
                if (canSetSetupRef(ref2)) {
                  setupState[ref2] = refs[ref2];
                }
              } else {
                ref2.value = [
                  refValue
                ];
                if (rawRef.k) refs[rawRef.k] = ref2.value;
              }
            } else if (!existing.includes(refValue)) {
              existing.push(refValue);
            }
          }
        } else if (_isString) {
          refs[ref2] = value;
          if (canSetSetupRef(ref2)) {
            setupState[ref2] = value;
          }
        } else if (_isRef) {
          ref2.value = value;
          if (rawRef.k) refs[rawRef.k] = value;
        } else if (true) {
          warn$1("Invalid template ref type:", ref2, `(${typeof ref2})`);
        }
      };
      if (value) {
        doSet.id = -1;
        queuePostRenderEffect(doSet, parentSuspense);
      } else {
        doSet();
      }
    } else if (true) {
      warn$1("Invalid template ref type:", ref2, `(${typeof ref2})`);
    }
  }
}
var requestIdleCallback = getGlobalThis().requestIdleCallback || ((cb) => setTimeout(cb, 1));
var cancelIdleCallback = getGlobalThis().cancelIdleCallback || ((id) => clearTimeout(id));
var isAsyncWrapper = (i) => !!i.type.__asyncLoader;
var isKeepAlive = (vnode) => vnode.type.__isKeepAlive;
function onActivated(hook2, target2) {
  registerKeepAliveHook(hook2, "a", target2);
}
function onDeactivated(hook2, target2) {
  registerKeepAliveHook(hook2, "da", target2);
}
function registerKeepAliveHook(hook2, type, target2 = currentInstance) {
  const wrappedHook = hook2.__wdc || (hook2.__wdc = () => {
    let current = target2;
    while (current) {
      if (current.isDeactivated) {
        return;
      }
      current = current.parent;
    }
    return hook2();
  });
  injectHook(type, wrappedHook, target2);
  if (target2) {
    let current = target2.parent;
    while (current && current.parent) {
      if (isKeepAlive(current.parent.vnode)) {
        injectToKeepAliveRoot(wrappedHook, type, target2, current);
      }
      current = current.parent;
    }
  }
}
function injectToKeepAliveRoot(hook2, type, target2, keepAliveRoot) {
  const injected = injectHook(type, hook2, keepAliveRoot, true);
  onUnmounted(() => {
    remove(keepAliveRoot[type], injected);
  }, target2);
}
function injectHook(type, hook2, target2 = currentInstance, prepend = false) {
  if (target2) {
    const hooks2 = target2[type] || (target2[type] = []);
    const wrappedHook = hook2.__weh || (hook2.__weh = (...args) => {
      pauseTracking();
      const reset = setCurrentInstance(target2);
      const res = callWithAsyncErrorHandling(hook2, target2, type, args);
      reset();
      resetTracking();
      return res;
    });
    if (prepend) {
      hooks2.unshift(wrappedHook);
    } else {
      hooks2.push(wrappedHook);
    }
    return wrappedHook;
  } else if (true) {
    const apiName = toHandlerKey(ErrorTypeStrings$1[type].replace(/ hook$/, ""));
    warn$1(`${apiName} is called when there is no active component instance to be associated with. Lifecycle injection APIs can only be used during execution of setup(). If you are using async setup(), make sure to register lifecycle hooks before the first await statement.`);
  }
}
var createHook = (lifecycle) => (hook2, target2 = currentInstance) => {
  if (!isInSSRComponentSetup || lifecycle === "sp") {
    injectHook(lifecycle, (...args) => hook2(...args), target2);
  }
};
var onBeforeMount = createHook("bm");
var onMounted = createHook("m");
var onBeforeUpdate = createHook("bu");
var onUpdated = createHook("u");
var onBeforeUnmount = createHook("bum");
var onUnmounted = createHook("um");
var onServerPrefetch = createHook("sp");
var onRenderTriggered = createHook("rtg");
var onRenderTracked = createHook("rtc");
function onErrorCaptured(hook2, target2 = currentInstance) {
  injectHook("ec", hook2, target2);
}
var NULL_DYNAMIC_COMPONENT = Symbol.for("v-ndc");
var getPublicInstance = (i) => {
  if (!i) return null;
  if (isStatefulComponent(i)) return getComponentPublicInstance(i);
  return getPublicInstance(i.parent);
};
var publicPropertiesMap = (
  // Move PURE marker to new line to workaround compiler discarding it
  // due to type annotation
  /* @__PURE__ */ extend(/* @__PURE__ */ Object.create(null), {
    $: (i) => i,
    $el: (i) => i.vnode.el,
    $data: (i) => i.data,
    $props: (i) => true ? shallowReadonly(i.props) : i.props,
    $attrs: (i) => true ? shallowReadonly(i.attrs) : i.attrs,
    $slots: (i) => true ? shallowReadonly(i.slots) : i.slots,
    $refs: (i) => true ? shallowReadonly(i.refs) : i.refs,
    $parent: (i) => getPublicInstance(i.parent),
    $root: (i) => getPublicInstance(i.root),
    $host: (i) => i.ce,
    $emit: (i) => i.emit,
    $options: (i) => __VUE_OPTIONS_API__ ? resolveMergedOptions(i) : i.type,
    $forceUpdate: (i) => i.f || (i.f = () => {
      queueJob(i.update);
    }),
    $nextTick: (i) => i.n || (i.n = nextTick.bind(i.proxy)),
    $watch: (i) => __VUE_OPTIONS_API__ ? instanceWatch.bind(i) : NOOP
  })
);
var isReservedPrefix = (key) => key === "_" || key === "$";
var hasSetupBinding = (state, key) => state !== EMPTY_OBJ && !state.__isScriptSetup && hasOwn(state, key);
var PublicInstanceProxyHandlers = {
  get({ _: instance }, key) {
    if (key === "__v_skip") {
      return true;
    }
    const { ctx, setupState, data, props, accessCache, type, appContext } = instance;
    if (key === "__isVue") {
      return true;
    }
    let normalizedProps;
    if (key[0] !== "$") {
      const n = accessCache[key];
      if (n !== void 0) {
        switch (n) {
          case 1:
            return setupState[key];
          case 2:
            return data[key];
          case 4:
            return ctx[key];
          case 3:
            return props[key];
        }
      } else if (hasSetupBinding(setupState, key)) {
        accessCache[key] = 1;
        return setupState[key];
      } else if (data !== EMPTY_OBJ && hasOwn(data, key)) {
        accessCache[key] = 2;
        return data[key];
      } else if (
        // only cache other properties when instance has declared (thus stable)
        // props
        (normalizedProps = instance.propsOptions[0]) && hasOwn(normalizedProps, key)
      ) {
        accessCache[key] = 3;
        return props[key];
      } else if (ctx !== EMPTY_OBJ && hasOwn(ctx, key)) {
        accessCache[key] = 4;
        return ctx[key];
      } else if (!__VUE_OPTIONS_API__ || shouldCacheAccess) {
        accessCache[key] = 0;
      }
    }
    const publicGetter = publicPropertiesMap[key];
    let cssModule, globalProperties;
    if (publicGetter) {
      if (key === "$attrs") {
        track(instance.attrs, "get", "");
        markAttrsAccessed();
      } else if (key === "$slots") {
        track(instance, "get", key);
      }
      return publicGetter(instance);
    } else if (
      // css module (injected by vue-loader)
      (cssModule = type.__cssModules) && (cssModule = cssModule[key])
    ) {
      return cssModule;
    } else if (ctx !== EMPTY_OBJ && hasOwn(ctx, key)) {
      accessCache[key] = 4;
      return ctx[key];
    } else if (
      // global properties
      globalProperties = appContext.config.globalProperties, hasOwn(globalProperties, key)
    ) {
      {
        return globalProperties[key];
      }
    } else if (currentRenderingInstance && (!isString(key) || // #1091 avoid internal isRef/isVNode checks on component instance leading
    // to infinite warning loop
    key.indexOf("__v") !== 0)) {
      if (data !== EMPTY_OBJ && isReservedPrefix(key[0]) && hasOwn(data, key)) {
        warn$1(`Property ${JSON.stringify(key)} must be accessed via $data because it starts with a reserved character ("$" or "_") and is not proxied on the render context.`);
      } else if (instance === currentRenderingInstance) {
        warn$1(`Property ${JSON.stringify(key)} was accessed during render but is not defined on instance.`);
      }
    }
  },
  set({ _: instance }, key, value) {
    const { data, setupState, ctx } = instance;
    if (hasSetupBinding(setupState, key)) {
      setupState[key] = value;
      return true;
    } else if (setupState.__isScriptSetup && hasOwn(setupState, key)) {
      warn$1(`Cannot mutate <script setup> binding "${key}" from Options API.`);
      return false;
    } else if (data !== EMPTY_OBJ && hasOwn(data, key)) {
      data[key] = value;
      return true;
    } else if (hasOwn(instance.props, key)) {
      warn$1(`Attempting to mutate prop "${key}". Props are readonly.`);
      return false;
    }
    if (key[0] === "$" && key.slice(1) in instance) {
      warn$1(`Attempting to mutate public property "${key}". Properties starting with $ are reserved and readonly.`);
      return false;
    } else {
      if (key in instance.appContext.config.globalProperties) {
        Object.defineProperty(ctx, key, {
          enumerable: true,
          configurable: true,
          value
        });
      } else {
        ctx[key] = value;
      }
    }
    return true;
  },
  has({ _: { data, setupState, accessCache, ctx, appContext, propsOptions } }, key) {
    let normalizedProps;
    return !!accessCache[key] || data !== EMPTY_OBJ && hasOwn(data, key) || hasSetupBinding(setupState, key) || (normalizedProps = propsOptions[0]) && hasOwn(normalizedProps, key) || hasOwn(ctx, key) || hasOwn(publicPropertiesMap, key) || hasOwn(appContext.config.globalProperties, key);
  },
  defineProperty(target2, key, descriptor) {
    if (descriptor.get != null) {
      target2._.accessCache[key] = 0;
    } else if (hasOwn(descriptor, "value")) {
      this.set(target2, key, descriptor.value, null);
    }
    return Reflect.defineProperty(target2, key, descriptor);
  }
};
if (true) {
  PublicInstanceProxyHandlers.ownKeys = (target2) => {
    warn$1(`Avoid app logic that relies on enumerating keys on a component instance. The keys will be empty in production mode to avoid performance overhead.`);
    return Reflect.ownKeys(target2);
  };
}
function createDevRenderContext(instance) {
  const target2 = {};
  Object.defineProperty(target2, `_`, {
    configurable: true,
    enumerable: false,
    get: () => instance
  });
  Object.keys(publicPropertiesMap).forEach((key) => {
    Object.defineProperty(target2, key, {
      configurable: true,
      enumerable: false,
      get: () => publicPropertiesMap[key](instance),
      // intercepted by the proxy so no need for implementation,
      // but needed to prevent set errors
      set: NOOP
    });
  });
  return target2;
}
function exposePropsOnRenderContext(instance) {
  const { ctx, propsOptions: [propsOptions] } = instance;
  if (propsOptions) {
    Object.keys(propsOptions).forEach((key) => {
      Object.defineProperty(ctx, key, {
        enumerable: true,
        configurable: true,
        get: () => instance.props[key],
        set: NOOP
      });
    });
  }
}
function exposeSetupStateOnRenderContext(instance) {
  const { ctx, setupState } = instance;
  Object.keys(toRaw(setupState)).forEach((key) => {
    if (!setupState.__isScriptSetup) {
      if (isReservedPrefix(key[0])) {
        warn$1(`setup() return property ${JSON.stringify(key)} should not start with "$" or "_" which are reserved prefixes for Vue internals.`);
        return;
      }
      Object.defineProperty(ctx, key, {
        enumerable: true,
        configurable: true,
        get: () => setupState[key],
        set: NOOP
      });
    }
  });
}
function normalizePropsOrEmits(props) {
  return isArray(props) ? props.reduce((normalized, p2) => (normalized[p2] = null, normalized), {}) : props;
}
function createDuplicateChecker() {
  const cache = /* @__PURE__ */ Object.create(null);
  return (type, key) => {
    if (cache[key]) {
      warn$1(`${type} property "${key}" is already defined in ${cache[key]}.`);
    } else {
      cache[key] = type;
    }
  };
}
var shouldCacheAccess = true;
function applyOptions(instance) {
  const options = resolveMergedOptions(instance);
  const publicThis = instance.proxy;
  const ctx = instance.ctx;
  shouldCacheAccess = false;
  if (options.beforeCreate) {
    callHook(options.beforeCreate, instance, "bc");
  }
  const {
    // state
    data: dataOptions,
    computed: computedOptions,
    methods,
    watch: watchOptions,
    provide: provideOptions,
    inject: injectOptions,
    // lifecycle
    created,
    beforeMount,
    mounted,
    beforeUpdate,
    updated,
    activated,
    deactivated,
    beforeDestroy,
    beforeUnmount,
    destroyed,
    unmounted,
    render,
    renderTracked,
    renderTriggered,
    errorCaptured,
    serverPrefetch,
    // public API
    expose,
    inheritAttrs,
    // assets
    components,
    directives,
    filters
  } = options;
  const checkDuplicateProperties = true ? createDuplicateChecker() : null;
  if (true) {
    const [propsOptions] = instance.propsOptions;
    if (propsOptions) {
      for (const key in propsOptions) {
        checkDuplicateProperties("Props", key);
      }
    }
  }
  if (injectOptions) {
    resolveInjections(injectOptions, ctx, checkDuplicateProperties);
  }
  if (methods) {
    for (const key in methods) {
      const methodHandler = methods[key];
      if (isFunction(methodHandler)) {
        if (true) {
          Object.defineProperty(ctx, key, {
            value: methodHandler.bind(publicThis),
            configurable: true,
            enumerable: true,
            writable: true
          });
        } else {
          ctx[key] = methodHandler.bind(publicThis);
        }
        if (true) {
          checkDuplicateProperties("Methods", key);
        }
      } else if (true) {
        warn$1(`Method "${key}" has type "${typeof methodHandler}" in the component definition. Did you reference the function correctly?`);
      }
    }
  }
  if (dataOptions) {
    if (!isFunction(dataOptions)) {
      warn$1(`The data option must be a function. Plain object usage is no longer supported.`);
    }
    const data = dataOptions.call(publicThis, publicThis);
    if (isPromise(data)) {
      warn$1(`data() returned a Promise - note data() cannot be async; If you intend to perform data fetching before component renders, use async setup() + <Suspense>.`);
    }
    if (!isObject(data)) {
      warn$1(`data() should return an object.`);
    } else {
      instance.data = reactive(data);
      if (true) {
        for (const key in data) {
          checkDuplicateProperties("Data", key);
          if (!isReservedPrefix(key[0])) {
            Object.defineProperty(ctx, key, {
              configurable: true,
              enumerable: true,
              get: () => data[key],
              set: NOOP
            });
          }
        }
      }
    }
  }
  shouldCacheAccess = true;
  if (computedOptions) {
    for (const key in computedOptions) {
      const opt = computedOptions[key];
      const get = isFunction(opt) ? opt.bind(publicThis, publicThis) : isFunction(opt.get) ? opt.get.bind(publicThis, publicThis) : NOOP;
      if (get === NOOP) {
        warn$1(`Computed property "${key}" has no getter.`);
      }
      const set = !isFunction(opt) && isFunction(opt.set) ? opt.set.bind(publicThis) : true ? () => {
        warn$1(`Write operation failed: computed property "${key}" is readonly.`);
      } : NOOP;
      const c = computed2({
        get,
        set
      });
      Object.defineProperty(ctx, key, {
        enumerable: true,
        configurable: true,
        get: () => c.value,
        set: (v) => c.value = v
      });
      if (true) {
        checkDuplicateProperties("Computed", key);
      }
    }
  }
  if (watchOptions) {
    for (const key in watchOptions) {
      createWatcher(watchOptions[key], ctx, publicThis, key);
    }
  }
  if (provideOptions) {
    const provides = isFunction(provideOptions) ? provideOptions.call(publicThis) : provideOptions;
    Reflect.ownKeys(provides).forEach((key) => {
      provide(key, provides[key]);
    });
  }
  if (created) {
    callHook(created, instance, "c");
  }
  function registerLifecycleHook(register, hook2) {
    if (isArray(hook2)) {
      hook2.forEach((_hook) => register(_hook.bind(publicThis)));
    } else if (hook2) {
      register(hook2.bind(publicThis));
    }
  }
  registerLifecycleHook(onBeforeMount, beforeMount);
  registerLifecycleHook(onMounted, mounted);
  registerLifecycleHook(onBeforeUpdate, beforeUpdate);
  registerLifecycleHook(onUpdated, updated);
  registerLifecycleHook(onActivated, activated);
  registerLifecycleHook(onDeactivated, deactivated);
  registerLifecycleHook(onErrorCaptured, errorCaptured);
  registerLifecycleHook(onRenderTracked, renderTracked);
  registerLifecycleHook(onRenderTriggered, renderTriggered);
  registerLifecycleHook(onBeforeUnmount, beforeUnmount);
  registerLifecycleHook(onUnmounted, unmounted);
  registerLifecycleHook(onServerPrefetch, serverPrefetch);
  if (isArray(expose)) {
    if (expose.length) {
      const exposed = instance.exposed || (instance.exposed = {});
      expose.forEach((key) => {
        Object.defineProperty(exposed, key, {
          get: () => publicThis[key],
          set: (val) => publicThis[key] = val
        });
      });
    } else if (!instance.exposed) {
      instance.exposed = {};
    }
  }
  if (render && instance.render === NOOP) {
    instance.render = render;
  }
  if (inheritAttrs != null) {
    instance.inheritAttrs = inheritAttrs;
  }
  if (components) instance.components = components;
  if (directives) instance.directives = directives;
  if (serverPrefetch) {
    markAsyncBoundary(instance);
  }
}
function resolveInjections(injectOptions, ctx, checkDuplicateProperties = NOOP) {
  if (isArray(injectOptions)) {
    injectOptions = normalizeInject(injectOptions);
  }
  for (const key in injectOptions) {
    const opt = injectOptions[key];
    let injected;
    if (isObject(opt)) {
      if ("default" in opt) {
        injected = inject(opt.from || key, opt.default, true);
      } else {
        injected = inject(opt.from || key);
      }
    } else {
      injected = inject(opt);
    }
    if (isRef(injected)) {
      Object.defineProperty(ctx, key, {
        enumerable: true,
        configurable: true,
        get: () => injected.value,
        set: (v) => injected.value = v
      });
    } else {
      ctx[key] = injected;
    }
    if (true) {
      checkDuplicateProperties("Inject", key);
    }
  }
}
function callHook(hook2, instance, type) {
  callWithAsyncErrorHandling(isArray(hook2) ? hook2.map((h2) => h2.bind(instance.proxy)) : hook2.bind(instance.proxy), instance, type);
}
function createWatcher(raw, ctx, publicThis, key) {
  let getter = key.includes(".") ? createPathGetter(publicThis, key) : () => publicThis[key];
  if (isString(raw)) {
    const handler = ctx[raw];
    if (isFunction(handler)) {
      {
        watch2(getter, handler);
      }
    } else if (true) {
      warn$1(`Invalid watch handler specified by key "${raw}"`, handler);
    }
  } else if (isFunction(raw)) {
    {
      watch2(getter, raw.bind(publicThis));
    }
  } else if (isObject(raw)) {
    if (isArray(raw)) {
      raw.forEach((r) => createWatcher(r, ctx, publicThis, key));
    } else {
      const handler = isFunction(raw.handler) ? raw.handler.bind(publicThis) : ctx[raw.handler];
      if (isFunction(handler)) {
        watch2(getter, handler, raw);
      } else if (true) {
        warn$1(`Invalid watch handler specified by key "${raw.handler}"`, handler);
      }
    }
  } else if (true) {
    warn$1(`Invalid watch option: "${key}"`, raw);
  }
}
function resolveMergedOptions(instance) {
  const base = instance.type;
  const { mixins, extends: extendsOptions } = base;
  const { mixins: globalMixins, optionsCache: cache, config: { optionMergeStrategies } } = instance.appContext;
  const cached = cache.get(base);
  let resolved;
  if (cached) {
    resolved = cached;
  } else if (!globalMixins.length && !mixins && !extendsOptions) {
    {
      resolved = base;
    }
  } else {
    resolved = {};
    if (globalMixins.length) {
      globalMixins.forEach((m) => mergeOptions(resolved, m, optionMergeStrategies, true));
    }
    mergeOptions(resolved, base, optionMergeStrategies);
  }
  if (isObject(base)) {
    cache.set(base, resolved);
  }
  return resolved;
}
function mergeOptions(to, from, strats, asMixin = false) {
  const { mixins, extends: extendsOptions } = from;
  if (extendsOptions) {
    mergeOptions(to, extendsOptions, strats, true);
  }
  if (mixins) {
    mixins.forEach((m) => mergeOptions(to, m, strats, true));
  }
  for (const key in from) {
    if (asMixin && key === "expose") {
      warn$1(`"expose" option is ignored when declared in mixins or extends. It should only be declared in the base component itself.`);
    } else {
      const strat = internalOptionMergeStrats[key] || strats && strats[key];
      to[key] = strat ? strat(to[key], from[key]) : from[key];
    }
  }
  return to;
}
var internalOptionMergeStrats = {
  data: mergeDataFn,
  props: mergeEmitsOrPropsOptions,
  emits: mergeEmitsOrPropsOptions,
  // objects
  methods: mergeObjectOptions,
  computed: mergeObjectOptions,
  // lifecycle
  beforeCreate: mergeAsArray,
  created: mergeAsArray,
  beforeMount: mergeAsArray,
  mounted: mergeAsArray,
  beforeUpdate: mergeAsArray,
  updated: mergeAsArray,
  beforeDestroy: mergeAsArray,
  beforeUnmount: mergeAsArray,
  destroyed: mergeAsArray,
  unmounted: mergeAsArray,
  activated: mergeAsArray,
  deactivated: mergeAsArray,
  errorCaptured: mergeAsArray,
  serverPrefetch: mergeAsArray,
  // assets
  components: mergeObjectOptions,
  directives: mergeObjectOptions,
  // watch
  watch: mergeWatchOptions,
  // provide / inject
  provide: mergeDataFn,
  inject: mergeInject
};
function mergeDataFn(to, from) {
  if (!from) {
    return to;
  }
  if (!to) {
    return from;
  }
  return function mergedDataFn() {
    return extend(isFunction(to) ? to.call(this, this) : to, isFunction(from) ? from.call(this, this) : from);
  };
}
function mergeInject(to, from) {
  return mergeObjectOptions(normalizeInject(to), normalizeInject(from));
}
function normalizeInject(raw) {
  if (isArray(raw)) {
    const res = {};
    for (let i = 0; i < raw.length; i++) {
      res[raw[i]] = raw[i];
    }
    return res;
  }
  return raw;
}
function mergeAsArray(to, from) {
  return to ? [
    ...new Set([].concat(to, from))
  ] : from;
}
function mergeObjectOptions(to, from) {
  return to ? extend(/* @__PURE__ */ Object.create(null), to, from) : from;
}
function mergeEmitsOrPropsOptions(to, from) {
  if (to) {
    if (isArray(to) && isArray(from)) {
      return [
        .../* @__PURE__ */ new Set([
          ...to,
          ...from
        ])
      ];
    }
    return extend(/* @__PURE__ */ Object.create(null), normalizePropsOrEmits(to), normalizePropsOrEmits(from != null ? from : {}));
  } else {
    return from;
  }
}
function mergeWatchOptions(to, from) {
  if (!to) return from;
  if (!from) return to;
  const merged = extend(/* @__PURE__ */ Object.create(null), to);
  for (const key in from) {
    merged[key] = mergeAsArray(to[key], from[key]);
  }
  return merged;
}
function createAppContext() {
  return {
    app: null,
    config: {
      isNativeTag: NO,
      performance: false,
      globalProperties: {},
      optionMergeStrategies: {},
      errorHandler: void 0,
      warnHandler: void 0,
      compilerOptions: {}
    },
    mixins: [],
    components: {},
    directives: {},
    provides: /* @__PURE__ */ Object.create(null),
    optionsCache: /* @__PURE__ */ new WeakMap(),
    propsCache: /* @__PURE__ */ new WeakMap(),
    emitsCache: /* @__PURE__ */ new WeakMap()
  };
}
var uid$1 = 0;
function createAppAPI(render, hydrate) {
  return function createApp2(rootComponent, rootProps = null) {
    if (!isFunction(rootComponent)) {
      rootComponent = extend({}, rootComponent);
    }
    if (rootProps != null && !isObject(rootProps)) {
      warn$1(`root props passed to app.mount() must be an object.`);
      rootProps = null;
    }
    const context = createAppContext();
    const installedPlugins = /* @__PURE__ */ new WeakSet();
    const pluginCleanupFns = [];
    let isMounted = false;
    const app = context.app = {
      _uid: uid$1++,
      _component: rootComponent,
      _props: rootProps,
      _container: null,
      _context: context,
      _instance: null,
      version,
      get config() {
        return context.config;
      },
      set config(v) {
        if (true) {
          warn$1(`app.config cannot be replaced. Modify individual options instead.`);
        }
      },
      use(plugin, ...options) {
        if (installedPlugins.has(plugin)) {
          warn$1(`Plugin has already been applied to target app.`);
        } else if (plugin && isFunction(plugin.install)) {
          installedPlugins.add(plugin);
          plugin.install(app, ...options);
        } else if (isFunction(plugin)) {
          installedPlugins.add(plugin);
          plugin(app, ...options);
        } else if (true) {
          warn$1(`A plugin must either be a function or an object with an "install" function.`);
        }
        return app;
      },
      mixin(mixin) {
        if (__VUE_OPTIONS_API__) {
          if (!context.mixins.includes(mixin)) {
            context.mixins.push(mixin);
          } else if (true) {
            warn$1("Mixin has already been applied to target app" + (mixin.name ? `: ${mixin.name}` : ""));
          }
        } else if (true) {
          warn$1("Mixins are only available in builds supporting Options API");
        }
        return app;
      },
      component(name, component8) {
        if (true) {
          validateComponentName(name, context.config);
        }
        if (!component8) {
          return context.components[name];
        }
        if (context.components[name]) {
          warn$1(`Component "${name}" has already been registered in target app.`);
        }
        context.components[name] = component8;
        return app;
      },
      directive(name, directive) {
        if (true) {
          validateDirectiveName(name);
        }
        if (!directive) {
          return context.directives[name];
        }
        if (context.directives[name]) {
          warn$1(`Directive "${name}" has already been registered in target app.`);
        }
        context.directives[name] = directive;
        return app;
      },
      mount(rootContainer, isHydrate, namespace) {
        if (!isMounted) {
          if (rootContainer.__vue_app__) {
            warn$1(`There is already an app instance mounted on the host container.
 If you want to mount another app on the same host container, you need to unmount the previous app by calling \`app.unmount()\` first.`);
          }
          const vnode = app._ceVNode || createVNode(rootComponent, rootProps);
          vnode.appContext = context;
          if (namespace === true) {
            namespace = "svg";
          } else if (namespace === false) {
            namespace = void 0;
          }
          if (true) {
            context.reload = () => {
              render(cloneVNode(vnode), rootContainer, namespace);
            };
          }
          if (isHydrate && hydrate) {
            hydrate(vnode, rootContainer);
          } else {
            render(vnode, rootContainer, namespace);
          }
          isMounted = true;
          app._container = rootContainer;
          rootContainer.__vue_app__ = app;
          if (true) {
            app._instance = vnode.component;
            devtoolsInitApp(app, version);
          }
          return getComponentPublicInstance(vnode.component);
        } else if (true) {
          warn$1(`App has already been mounted.
If you want to remount the same app, move your app creation logic into a factory function and create fresh app instances for each mount - e.g. \`const createMyApp = () => createApp(App)\``);
        }
      },
      onUnmount(cleanupFn) {
        if (typeof cleanupFn !== "function") {
          warn$1(`Expected function as first argument to app.onUnmount(), but got ${typeof cleanupFn}`);
        }
        pluginCleanupFns.push(cleanupFn);
      },
      unmount() {
        if (isMounted) {
          callWithAsyncErrorHandling(pluginCleanupFns, app._instance, 16);
          render(null, app._container);
          if (true) {
            app._instance = null;
            devtoolsUnmountApp(app);
          }
          delete app._container.__vue_app__;
        } else if (true) {
          warn$1(`Cannot unmount an app that is not mounted.`);
        }
      },
      provide(key, value) {
        if (key in context.provides) {
          warn$1(`App already provides property with key "${String(key)}". It will be overwritten with the new value.`);
        }
        context.provides[key] = value;
        return app;
      },
      runWithContext(fn) {
        const lastApp = currentApp;
        currentApp = app;
        try {
          return fn();
        } finally {
          currentApp = lastApp;
        }
      }
    };
    return app;
  };
}
var currentApp = null;
function provide(key, value) {
  if (!currentInstance) {
    if (true) {
      warn$1(`provide() can only be used inside setup().`);
    }
  } else {
    let provides = currentInstance.provides;
    const parentProvides = currentInstance.parent && currentInstance.parent.provides;
    if (parentProvides === provides) {
      provides = currentInstance.provides = Object.create(parentProvides);
    }
    provides[key] = value;
  }
}
function inject(key, defaultValue, treatDefaultAsFactory = false) {
  const instance = currentInstance || currentRenderingInstance;
  if (instance || currentApp) {
    const provides = currentApp ? currentApp._context.provides : instance ? instance.parent == null ? instance.vnode.appContext && instance.vnode.appContext.provides : instance.parent.provides : void 0;
    if (provides && key in provides) {
      return provides[key];
    } else if (arguments.length > 1) {
      return treatDefaultAsFactory && isFunction(defaultValue) ? defaultValue.call(instance && instance.proxy) : defaultValue;
    } else if (true) {
      warn$1(`injection "${String(key)}" not found.`);
    }
  } else if (true) {
    warn$1(`inject() can only be used inside setup() or functional components.`);
  }
}
function hasInjectionContext() {
  return !!(currentInstance || currentRenderingInstance || currentApp);
}
var internalObjectProto = {};
var createInternalObject = () => Object.create(internalObjectProto);
var isInternalObject = (obj) => Object.getPrototypeOf(obj) === internalObjectProto;
function initProps(instance, rawProps, isStateful, isSSR = false) {
  const props = {};
  const attrs = createInternalObject();
  instance.propsDefaults = /* @__PURE__ */ Object.create(null);
  setFullProps(instance, rawProps, props, attrs);
  for (const key in instance.propsOptions[0]) {
    if (!(key in props)) {
      props[key] = void 0;
    }
  }
  if (true) {
    validateProps(rawProps || {}, props, instance);
  }
  if (isStateful) {
    instance.props = isSSR ? props : shallowReactive(props);
  } else {
    if (!instance.type.props) {
      instance.props = attrs;
    } else {
      instance.props = props;
    }
  }
  instance.attrs = attrs;
}
function isInHmrContext(instance) {
  while (instance) {
    if (instance.type.__hmrId) return true;
    instance = instance.parent;
  }
}
function updateProps(instance, rawProps, rawPrevProps, optimized) {
  const { props, attrs, vnode: { patchFlag } } = instance;
  const rawCurrentProps = toRaw(props);
  const [options] = instance.propsOptions;
  let hasAttrsChanged = false;
  if (
    // always force full diff in dev
    // - #1942 if hmr is enabled with sfc component
    // - vite#872 non-sfc component used by sfc component
    !isInHmrContext(instance) && (optimized || patchFlag > 0) && !(patchFlag & 16)
  ) {
    if (patchFlag & 8) {
      const propsToUpdate = instance.vnode.dynamicProps;
      for (let i = 0; i < propsToUpdate.length; i++) {
        let key = propsToUpdate[i];
        if (isEmitListener(instance.emitsOptions, key)) {
          continue;
        }
        const value = rawProps[key];
        if (options) {
          if (hasOwn(attrs, key)) {
            if (value !== attrs[key]) {
              attrs[key] = value;
              hasAttrsChanged = true;
            }
          } else {
            const camelizedKey = camelize(key);
            props[camelizedKey] = resolvePropValue(options, rawCurrentProps, camelizedKey, value, instance, false);
          }
        } else {
          if (value !== attrs[key]) {
            attrs[key] = value;
            hasAttrsChanged = true;
          }
        }
      }
    }
  } else {
    if (setFullProps(instance, rawProps, props, attrs)) {
      hasAttrsChanged = true;
    }
    let kebabKey;
    for (const key in rawCurrentProps) {
      if (!rawProps || // for camelCase
      !hasOwn(rawProps, key) && // it's possible the original props was passed in as kebab-case
      // and converted to camelCase (#955)
      ((kebabKey = hyphenate(key)) === key || !hasOwn(rawProps, kebabKey))) {
        if (options) {
          if (rawPrevProps && // for camelCase
          (rawPrevProps[key] !== void 0 || // for kebab-case
          rawPrevProps[kebabKey] !== void 0)) {
            props[key] = resolvePropValue(options, rawCurrentProps, key, void 0, instance, true);
          }
        } else {
          delete props[key];
        }
      }
    }
    if (attrs !== rawCurrentProps) {
      for (const key in attrs) {
        if (!rawProps || !hasOwn(rawProps, key) && true) {
          delete attrs[key];
          hasAttrsChanged = true;
        }
      }
    }
  }
  if (hasAttrsChanged) {
    trigger(instance.attrs, "set", "");
  }
  if (true) {
    validateProps(rawProps || {}, props, instance);
  }
}
function setFullProps(instance, rawProps, props, attrs) {
  const [options, needCastKeys] = instance.propsOptions;
  let hasAttrsChanged = false;
  let rawCastValues;
  if (rawProps) {
    for (let key in rawProps) {
      if (isReservedProp(key)) {
        continue;
      }
      const value = rawProps[key];
      let camelKey;
      if (options && hasOwn(options, camelKey = camelize(key))) {
        if (!needCastKeys || !needCastKeys.includes(camelKey)) {
          props[camelKey] = value;
        } else {
          (rawCastValues || (rawCastValues = {}))[camelKey] = value;
        }
      } else if (!isEmitListener(instance.emitsOptions, key)) {
        if (!(key in attrs) || value !== attrs[key]) {
          attrs[key] = value;
          hasAttrsChanged = true;
        }
      }
    }
  }
  if (needCastKeys) {
    const rawCurrentProps = toRaw(props);
    const castValues = rawCastValues || EMPTY_OBJ;
    for (let i = 0; i < needCastKeys.length; i++) {
      const key = needCastKeys[i];
      props[key] = resolvePropValue(options, rawCurrentProps, key, castValues[key], instance, !hasOwn(castValues, key));
    }
  }
  return hasAttrsChanged;
}
function resolvePropValue(options, props, key, value, instance, isAbsent) {
  const opt = options[key];
  if (opt != null) {
    const hasDefault = hasOwn(opt, "default");
    if (hasDefault && value === void 0) {
      const defaultValue = opt.default;
      if (opt.type !== Function && !opt.skipFactory && isFunction(defaultValue)) {
        const { propsDefaults } = instance;
        if (key in propsDefaults) {
          value = propsDefaults[key];
        } else {
          const reset = setCurrentInstance(instance);
          value = propsDefaults[key] = defaultValue.call(null, props);
          reset();
        }
      } else {
        value = defaultValue;
      }
      if (instance.ce) {
        instance.ce._setProp(key, value);
      }
    }
    if (opt[
      0
      /* shouldCast */
    ]) {
      if (isAbsent && !hasDefault) {
        value = false;
      } else if (opt[
        1
        /* shouldCastTrue */
      ] && (value === "" || value === hyphenate(key))) {
        value = true;
      }
    }
  }
  return value;
}
var mixinPropsCache = /* @__PURE__ */ new WeakMap();
function normalizePropsOptions(comp, appContext, asMixin = false) {
  const cache = __VUE_OPTIONS_API__ && asMixin ? mixinPropsCache : appContext.propsCache;
  const cached = cache.get(comp);
  if (cached) {
    return cached;
  }
  const raw = comp.props;
  const normalized = {};
  const needCastKeys = [];
  let hasExtends = false;
  if (__VUE_OPTIONS_API__ && !isFunction(comp)) {
    const extendProps = (raw2) => {
      hasExtends = true;
      const [props, keys] = normalizePropsOptions(raw2, appContext, true);
      extend(normalized, props);
      if (keys) needCastKeys.push(...keys);
    };
    if (!asMixin && appContext.mixins.length) {
      appContext.mixins.forEach(extendProps);
    }
    if (comp.extends) {
      extendProps(comp.extends);
    }
    if (comp.mixins) {
      comp.mixins.forEach(extendProps);
    }
  }
  if (!raw && !hasExtends) {
    if (isObject(comp)) {
      cache.set(comp, EMPTY_ARR);
    }
    return EMPTY_ARR;
  }
  if (isArray(raw)) {
    for (let i = 0; i < raw.length; i++) {
      if (!isString(raw[i])) {
        warn$1(`props must be strings when using array syntax.`, raw[i]);
      }
      const normalizedKey = camelize(raw[i]);
      if (validatePropName(normalizedKey)) {
        normalized[normalizedKey] = EMPTY_OBJ;
      }
    }
  } else if (raw) {
    if (!isObject(raw)) {
      warn$1(`invalid props options`, raw);
    }
    for (const key in raw) {
      const normalizedKey = camelize(key);
      if (validatePropName(normalizedKey)) {
        const opt = raw[key];
        const prop = normalized[normalizedKey] = isArray(opt) || isFunction(opt) ? {
          type: opt
        } : extend({}, opt);
        const propType = prop.type;
        let shouldCast = false;
        let shouldCastTrue = true;
        if (isArray(propType)) {
          for (let index = 0; index < propType.length; ++index) {
            const type = propType[index];
            const typeName = isFunction(type) && type.name;
            if (typeName === "Boolean") {
              shouldCast = true;
              break;
            } else if (typeName === "String") {
              shouldCastTrue = false;
            }
          }
        } else {
          shouldCast = isFunction(propType) && propType.name === "Boolean";
        }
        prop[
          0
          /* shouldCast */
        ] = shouldCast;
        prop[
          1
          /* shouldCastTrue */
        ] = shouldCastTrue;
        if (shouldCast || hasOwn(prop, "default")) {
          needCastKeys.push(normalizedKey);
        }
      }
    }
  }
  const res = [
    normalized,
    needCastKeys
  ];
  if (isObject(comp)) {
    cache.set(comp, res);
  }
  return res;
}
function validatePropName(key) {
  if (key[0] !== "$" && !isReservedProp(key)) {
    return true;
  } else if (true) {
    warn$1(`Invalid prop name: "${key}" is a reserved property.`);
  }
  return false;
}
function getType(ctor) {
  if (ctor === null) {
    return "null";
  }
  if (typeof ctor === "function") {
    return ctor.name || "";
  } else if (typeof ctor === "object") {
    const name = ctor.constructor && ctor.constructor.name;
    return name || "";
  }
  return "";
}
function validateProps(rawProps, props, instance) {
  const resolvedValues = toRaw(props);
  const options = instance.propsOptions[0];
  const camelizePropsKey = Object.keys(rawProps).map((key) => camelize(key));
  for (const key in options) {
    let opt = options[key];
    if (opt == null) continue;
    validateProp(key, resolvedValues[key], opt, true ? shallowReadonly(resolvedValues) : resolvedValues, !camelizePropsKey.includes(key));
  }
}
function validateProp(name, value, prop, props, isAbsent) {
  const { type, required, validator, skipCheck } = prop;
  if (required && isAbsent) {
    warn$1('Missing required prop: "' + name + '"');
    return;
  }
  if (value == null && !required) {
    return;
  }
  if (type != null && type !== true && !skipCheck) {
    let isValid = false;
    const types = isArray(type) ? type : [
      type
    ];
    const expectedTypes = [];
    for (let i = 0; i < types.length && !isValid; i++) {
      const { valid, expectedType } = assertType(value, types[i]);
      expectedTypes.push(expectedType || "");
      isValid = valid;
    }
    if (!isValid) {
      warn$1(getInvalidTypeMessage(name, value, expectedTypes));
      return;
    }
  }
  if (validator && !validator(value, props)) {
    warn$1('Invalid prop: custom validator check failed for prop "' + name + '".');
  }
}
var isSimpleType = /* @__PURE__ */ makeMap("String,Number,Boolean,Function,Symbol,BigInt");
function assertType(value, type) {
  let valid;
  const expectedType = getType(type);
  if (expectedType === "null") {
    valid = value === null;
  } else if (isSimpleType(expectedType)) {
    const t = typeof value;
    valid = t === expectedType.toLowerCase();
    if (!valid && t === "object") {
      valid = value instanceof type;
    }
  } else if (expectedType === "Object") {
    valid = isObject(value);
  } else if (expectedType === "Array") {
    valid = isArray(value);
  } else {
    valid = value instanceof type;
  }
  return {
    valid,
    expectedType
  };
}
function getInvalidTypeMessage(name, value, expectedTypes) {
  if (expectedTypes.length === 0) {
    return `Prop type [] for prop "${name}" won't match anything. Did you mean to use type Array instead?`;
  }
  let message = `Invalid prop: type check failed for prop "${name}". Expected ${expectedTypes.map(capitalize).join(" | ")}`;
  const expectedType = expectedTypes[0];
  const receivedType = toRawType(value);
  const expectedValue = styleValue(value, expectedType);
  const receivedValue = styleValue(value, receivedType);
  if (expectedTypes.length === 1 && isExplicable(expectedType) && !isBoolean(expectedType, receivedType)) {
    message += ` with value ${expectedValue}`;
  }
  message += `, got ${receivedType} `;
  if (isExplicable(receivedType)) {
    message += `with value ${receivedValue}.`;
  }
  return message;
}
function styleValue(value, type) {
  if (type === "String") {
    return `"${value}"`;
  } else if (type === "Number") {
    return `${Number(value)}`;
  } else {
    return `${value}`;
  }
}
function isExplicable(type) {
  const explicitTypes = [
    "string",
    "number",
    "boolean"
  ];
  return explicitTypes.some((elem) => type.toLowerCase() === elem);
}
function isBoolean(...args) {
  return args.some((elem) => elem.toLowerCase() === "boolean");
}
var isInternalKey = (key) => key[0] === "_" || key === "$stable";
var normalizeSlotValue = (value) => isArray(value) ? value.map(normalizeVNode) : [
  normalizeVNode(value)
];
var normalizeSlot = (key, rawSlot, ctx) => {
  if (rawSlot._n) {
    return rawSlot;
  }
  const normalized = withCtx((...args) => {
    if (currentInstance && (!ctx || ctx.root === currentInstance.root)) {
      warn$1(`Slot "${key}" invoked outside of the render function: this will not track dependencies used in the slot. Invoke the slot function inside the render function instead.`);
    }
    return normalizeSlotValue(rawSlot(...args));
  }, ctx);
  normalized._c = false;
  return normalized;
};
var normalizeObjectSlots = (rawSlots, slots, instance) => {
  const ctx = rawSlots._ctx;
  for (const key in rawSlots) {
    if (isInternalKey(key)) continue;
    const value = rawSlots[key];
    if (isFunction(value)) {
      slots[key] = normalizeSlot(key, value, ctx);
    } else if (value != null) {
      if (true) {
        warn$1(`Non-function value encountered for slot "${key}". Prefer function slots for better performance.`);
      }
      const normalized = normalizeSlotValue(value);
      slots[key] = () => normalized;
    }
  }
};
var normalizeVNodeSlots = (instance, children) => {
  if (!isKeepAlive(instance.vnode) && true) {
    warn$1(`Non-function value encountered for default slot. Prefer function slots for better performance.`);
  }
  const normalized = normalizeSlotValue(children);
  instance.slots.default = () => normalized;
};
var assignSlots = (slots, children, optimized) => {
  for (const key in children) {
    if (optimized || key !== "_") {
      slots[key] = children[key];
    }
  }
};
var initSlots = (instance, children, optimized) => {
  const slots = instance.slots = createInternalObject();
  if (instance.vnode.shapeFlag & 32) {
    const type = children._;
    if (type) {
      assignSlots(slots, children, optimized);
      if (optimized) {
        def(slots, "_", type, true);
      }
    } else {
      normalizeObjectSlots(children, slots);
    }
  } else if (children) {
    normalizeVNodeSlots(instance, children);
  }
};
var updateSlots = (instance, children, optimized) => {
  const { vnode, slots } = instance;
  let needDeletionCheck = true;
  let deletionComparisonTarget = EMPTY_OBJ;
  if (vnode.shapeFlag & 32) {
    const type = children._;
    if (type) {
      if (isHmrUpdating) {
        assignSlots(slots, children, optimized);
        trigger(instance, "set", "$slots");
      } else if (optimized && type === 1) {
        needDeletionCheck = false;
      } else {
        assignSlots(slots, children, optimized);
      }
    } else {
      needDeletionCheck = !children.$stable;
      normalizeObjectSlots(children, slots);
    }
    deletionComparisonTarget = children;
  } else if (children) {
    normalizeVNodeSlots(instance, children);
    deletionComparisonTarget = {
      default: 1
    };
  }
  if (needDeletionCheck) {
    for (const key in slots) {
      if (!isInternalKey(key) && deletionComparisonTarget[key] == null) {
        delete slots[key];
      }
    }
  }
};
var supported;
var perf;
function startMeasure(instance, type) {
  if (instance.appContext.config.performance && isSupported()) {
    perf.mark(`vue-${type}-${instance.uid}`);
  }
  if (true) {
    devtoolsPerfStart(instance, type, isSupported() ? perf.now() : Date.now());
  }
}
function endMeasure(instance, type) {
  if (instance.appContext.config.performance && isSupported()) {
    const startTag = `vue-${type}-${instance.uid}`;
    const endTag = startTag + `:end`;
    perf.mark(endTag);
    perf.measure(`<${formatComponentName(instance, instance.type)}> ${type}`, startTag, endTag);
    perf.clearMarks(startTag);
    perf.clearMarks(endTag);
  }
  if (true) {
    devtoolsPerfEnd(instance, type, isSupported() ? perf.now() : Date.now());
  }
}
function isSupported() {
  if (supported !== void 0) {
    return supported;
  }
  if (typeof window !== "undefined" && window.performance) {
    supported = true;
    perf = window.performance;
  } else {
    supported = false;
  }
  return supported;
}
function initFeatureFlags() {
  const needWarn = [];
  if (typeof __VUE_OPTIONS_API__ !== "boolean") {
    needWarn.push(`__VUE_OPTIONS_API__`);
    getGlobalThis().__VUE_OPTIONS_API__ = true;
  }
  if (typeof __VUE_PROD_DEVTOOLS__ !== "boolean") {
    needWarn.push(`__VUE_PROD_DEVTOOLS__`);
    getGlobalThis().__VUE_PROD_DEVTOOLS__ = false;
  }
  if (typeof __VUE_PROD_HYDRATION_MISMATCH_DETAILS__ !== "boolean") {
    needWarn.push(`__VUE_PROD_HYDRATION_MISMATCH_DETAILS__`);
    getGlobalThis().__VUE_PROD_HYDRATION_MISMATCH_DETAILS__ = false;
  }
  if (needWarn.length) {
    const multi = needWarn.length > 1;
    console.warn(`Feature flag${multi ? `s` : ``} ${needWarn.join(", ")} ${multi ? `are` : `is`} not explicitly defined. You are running the esm-bundler build of Vue, which expects these compile-time feature flags to be globally injected via the bundler config in order to get better tree-shaking in the production bundle.

For more details, see https://link.vuejs.org/feature-flags.`);
  }
}
var queuePostRenderEffect = queueEffectWithSuspense;
function createRenderer(options) {
  return baseCreateRenderer(options);
}
function baseCreateRenderer(options, createHydrationFns) {
  {
    initFeatureFlags();
  }
  const target2 = getGlobalThis();
  target2.__VUE__ = true;
  if (true) {
    setDevtoolsHook$1(target2.__VUE_DEVTOOLS_GLOBAL_HOOK__, target2);
  }
  const { insert: hostInsert, remove: hostRemove, patchProp: hostPatchProp, createElement: hostCreateElement, createText: hostCreateText, createComment: hostCreateComment, setText: hostSetText, setElementText: hostSetElementText, parentNode: hostParentNode, nextSibling: hostNextSibling, setScopeId: hostSetScopeId = NOOP, insertStaticContent: hostInsertStaticContent } = options;
  const patch = (n1, n2, container, anchor = null, parentComponent = null, parentSuspense = null, namespace = void 0, slotScopeIds = null, optimized = isHmrUpdating ? false : !!n2.dynamicChildren) => {
    if (n1 === n2) {
      return;
    }
    if (n1 && !isSameVNodeType(n1, n2)) {
      anchor = getNextHostNode(n1);
      unmount(n1, parentComponent, parentSuspense, true);
      n1 = null;
    }
    if (n2.patchFlag === -2) {
      optimized = false;
      n2.dynamicChildren = null;
    }
    const { type, ref: ref2, shapeFlag } = n2;
    switch (type) {
      case Text:
        processText(n1, n2, container, anchor);
        break;
      case Comment:
        processCommentNode(n1, n2, container, anchor);
        break;
      case Static:
        if (n1 == null) {
          mountStaticNode(n2, container, anchor, namespace);
        } else if (true) {
          patchStaticNode(n1, n2, container, namespace);
        }
        break;
      case Fragment:
        processFragment(n1, n2, container, anchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized);
        break;
      default:
        if (shapeFlag & 1) {
          processElement(n1, n2, container, anchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized);
        } else if (shapeFlag & 6) {
          processComponent(n1, n2, container, anchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized);
        } else if (shapeFlag & 64) {
          type.process(n1, n2, container, anchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized, internals);
        } else if (shapeFlag & 128) {
          type.process(n1, n2, container, anchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized, internals);
        } else if (true) {
          warn$1("Invalid VNode type:", type, `(${typeof type})`);
        }
    }
    if (ref2 != null && parentComponent) {
      setRef(ref2, n1 && n1.ref, parentSuspense, n2 || n1, !n2);
    }
  };
  const processText = (n1, n2, container, anchor) => {
    if (n1 == null) {
      hostInsert(n2.el = hostCreateText(n2.children), container, anchor);
    } else {
      const el = n2.el = n1.el;
      if (n2.children !== n1.children) {
        hostSetText(el, n2.children);
      }
    }
  };
  const processCommentNode = (n1, n2, container, anchor) => {
    if (n1 == null) {
      hostInsert(n2.el = hostCreateComment(n2.children || ""), container, anchor);
    } else {
      n2.el = n1.el;
    }
  };
  const mountStaticNode = (n2, container, anchor, namespace) => {
    [n2.el, n2.anchor] = hostInsertStaticContent(n2.children, container, anchor, namespace, n2.el, n2.anchor);
  };
  const patchStaticNode = (n1, n2, container, namespace) => {
    if (n2.children !== n1.children) {
      const anchor = hostNextSibling(n1.anchor);
      removeStaticNode(n1);
      [n2.el, n2.anchor] = hostInsertStaticContent(n2.children, container, anchor, namespace);
    } else {
      n2.el = n1.el;
      n2.anchor = n1.anchor;
    }
  };
  const moveStaticNode = ({ el, anchor }, container, nextSibling) => {
    let next;
    while (el && el !== anchor) {
      next = hostNextSibling(el);
      hostInsert(el, container, nextSibling);
      el = next;
    }
    hostInsert(anchor, container, nextSibling);
  };
  const removeStaticNode = ({ el, anchor }) => {
    let next;
    while (el && el !== anchor) {
      next = hostNextSibling(el);
      hostRemove(el);
      el = next;
    }
    hostRemove(anchor);
  };
  const processElement = (n1, n2, container, anchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized) => {
    if (n2.type === "svg") {
      namespace = "svg";
    } else if (n2.type === "math") {
      namespace = "mathml";
    }
    if (n1 == null) {
      mountElement(n2, container, anchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized);
    } else {
      patchElement(n1, n2, parentComponent, parentSuspense, namespace, slotScopeIds, optimized);
    }
  };
  const mountElement = (vnode, container, anchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized) => {
    let el;
    let vnodeHook;
    const { props, shapeFlag, transition, dirs } = vnode;
    el = vnode.el = hostCreateElement(vnode.type, namespace, props && props.is, props);
    if (shapeFlag & 8) {
      hostSetElementText(el, vnode.children);
    } else if (shapeFlag & 16) {
      mountChildren(vnode.children, el, null, parentComponent, parentSuspense, resolveChildrenNamespace(vnode, namespace), slotScopeIds, optimized);
    }
    if (dirs) {
      invokeDirectiveHook(vnode, null, parentComponent, "created");
    }
    setScopeId(el, vnode, vnode.scopeId, slotScopeIds, parentComponent);
    if (props) {
      for (const key in props) {
        if (key !== "value" && !isReservedProp(key)) {
          hostPatchProp(el, key, null, props[key], namespace, parentComponent);
        }
      }
      if ("value" in props) {
        hostPatchProp(el, "value", null, props.value, namespace);
      }
      if (vnodeHook = props.onVnodeBeforeMount) {
        invokeVNodeHook(vnodeHook, parentComponent, vnode);
      }
    }
    if (true) {
      def(el, "__vnode", vnode, true);
      def(el, "__vueParentComponent", parentComponent, true);
    }
    if (dirs) {
      invokeDirectiveHook(vnode, null, parentComponent, "beforeMount");
    }
    const needCallTransitionHooks = needTransition(parentSuspense, transition);
    if (needCallTransitionHooks) {
      transition.beforeEnter(el);
    }
    hostInsert(el, container, anchor);
    if ((vnodeHook = props && props.onVnodeMounted) || needCallTransitionHooks || dirs) {
      queuePostRenderEffect(() => {
        vnodeHook && invokeVNodeHook(vnodeHook, parentComponent, vnode);
        needCallTransitionHooks && transition.enter(el);
        dirs && invokeDirectiveHook(vnode, null, parentComponent, "mounted");
      }, parentSuspense);
    }
  };
  const setScopeId = (el, vnode, scopeId, slotScopeIds, parentComponent) => {
    if (scopeId) {
      hostSetScopeId(el, scopeId);
    }
    if (slotScopeIds) {
      for (let i = 0; i < slotScopeIds.length; i++) {
        hostSetScopeId(el, slotScopeIds[i]);
      }
    }
    if (parentComponent) {
      let subTree = parentComponent.subTree;
      if (subTree.patchFlag > 0 && subTree.patchFlag & 2048) {
        subTree = filterSingleRoot(subTree.children) || subTree;
      }
      if (vnode === subTree || isSuspense(subTree.type) && (subTree.ssContent === vnode || subTree.ssFallback === vnode)) {
        const parentVNode = parentComponent.vnode;
        setScopeId(el, parentVNode, parentVNode.scopeId, parentVNode.slotScopeIds, parentComponent.parent);
      }
    }
  };
  const mountChildren = (children, container, anchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized, start = 0) => {
    for (let i = start; i < children.length; i++) {
      const child = children[i] = optimized ? cloneIfMounted(children[i]) : normalizeVNode(children[i]);
      patch(null, child, container, anchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized);
    }
  };
  const patchElement = (n1, n2, parentComponent, parentSuspense, namespace, slotScopeIds, optimized) => {
    const el = n2.el = n1.el;
    if (true) {
      el.__vnode = n2;
    }
    let { patchFlag, dynamicChildren, dirs } = n2;
    patchFlag |= n1.patchFlag & 16;
    const oldProps = n1.props || EMPTY_OBJ;
    const newProps = n2.props || EMPTY_OBJ;
    let vnodeHook;
    parentComponent && toggleRecurse(parentComponent, false);
    if (vnodeHook = newProps.onVnodeBeforeUpdate) {
      invokeVNodeHook(vnodeHook, parentComponent, n2, n1);
    }
    if (dirs) {
      invokeDirectiveHook(n2, n1, parentComponent, "beforeUpdate");
    }
    parentComponent && toggleRecurse(parentComponent, true);
    if (isHmrUpdating) {
      patchFlag = 0;
      optimized = false;
      dynamicChildren = null;
    }
    if (oldProps.innerHTML && newProps.innerHTML == null || oldProps.textContent && newProps.textContent == null) {
      hostSetElementText(el, "");
    }
    if (dynamicChildren) {
      patchBlockChildren(n1.dynamicChildren, dynamicChildren, el, parentComponent, parentSuspense, resolveChildrenNamespace(n2, namespace), slotScopeIds);
      if (true) {
        traverseStaticChildren(n1, n2);
      }
    } else if (!optimized) {
      patchChildren(n1, n2, el, null, parentComponent, parentSuspense, resolveChildrenNamespace(n2, namespace), slotScopeIds, false);
    }
    if (patchFlag > 0) {
      if (patchFlag & 16) {
        patchProps(el, oldProps, newProps, parentComponent, namespace);
      } else {
        if (patchFlag & 2) {
          if (oldProps.class !== newProps.class) {
            hostPatchProp(el, "class", null, newProps.class, namespace);
          }
        }
        if (patchFlag & 4) {
          hostPatchProp(el, "style", oldProps.style, newProps.style, namespace);
        }
        if (patchFlag & 8) {
          const propsToUpdate = n2.dynamicProps;
          for (let i = 0; i < propsToUpdate.length; i++) {
            const key = propsToUpdate[i];
            const prev = oldProps[key];
            const next = newProps[key];
            if (next !== prev || key === "value") {
              hostPatchProp(el, key, prev, next, namespace, parentComponent);
            }
          }
        }
      }
      if (patchFlag & 1) {
        if (n1.children !== n2.children) {
          hostSetElementText(el, n2.children);
        }
      }
    } else if (!optimized && dynamicChildren == null) {
      patchProps(el, oldProps, newProps, parentComponent, namespace);
    }
    if ((vnodeHook = newProps.onVnodeUpdated) || dirs) {
      queuePostRenderEffect(() => {
        vnodeHook && invokeVNodeHook(vnodeHook, parentComponent, n2, n1);
        dirs && invokeDirectiveHook(n2, n1, parentComponent, "updated");
      }, parentSuspense);
    }
  };
  const patchBlockChildren = (oldChildren, newChildren, fallbackContainer, parentComponent, parentSuspense, namespace, slotScopeIds) => {
    for (let i = 0; i < newChildren.length; i++) {
      const oldVNode = oldChildren[i];
      const newVNode = newChildren[i];
      const container = (
        // oldVNode may be an errored async setup() component inside Suspense
        // which will not have a mounted element
        oldVNode.el && // - In the case of a Fragment, we need to provide the actual parent
        // of the Fragment itself so it can move its children.
        (oldVNode.type === Fragment || // - In the case of different nodes, there is going to be a replacement
        // which also requires the correct parent container
        !isSameVNodeType(oldVNode, newVNode) || // - In the case of a component, it could contain anything.
        oldVNode.shapeFlag & (6 | 64)) ? hostParentNode(oldVNode.el) : (
          // In other cases, the parent container is not actually used so we
          // just pass the block element here to avoid a DOM parentNode call.
          fallbackContainer
        )
      );
      patch(oldVNode, newVNode, container, null, parentComponent, parentSuspense, namespace, slotScopeIds, true);
    }
  };
  const patchProps = (el, oldProps, newProps, parentComponent, namespace) => {
    if (oldProps !== newProps) {
      if (oldProps !== EMPTY_OBJ) {
        for (const key in oldProps) {
          if (!isReservedProp(key) && !(key in newProps)) {
            hostPatchProp(el, key, oldProps[key], null, namespace, parentComponent);
          }
        }
      }
      for (const key in newProps) {
        if (isReservedProp(key)) continue;
        const next = newProps[key];
        const prev = oldProps[key];
        if (next !== prev && key !== "value") {
          hostPatchProp(el, key, prev, next, namespace, parentComponent);
        }
      }
      if ("value" in newProps) {
        hostPatchProp(el, "value", oldProps.value, newProps.value, namespace);
      }
    }
  };
  const processFragment = (n1, n2, container, anchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized) => {
    const fragmentStartAnchor = n2.el = n1 ? n1.el : hostCreateText("");
    const fragmentEndAnchor = n2.anchor = n1 ? n1.anchor : hostCreateText("");
    let { patchFlag, dynamicChildren, slotScopeIds: fragmentSlotScopeIds } = n2;
    if (
      // #5523 dev root fragment may inherit directives
      isHmrUpdating || patchFlag & 2048
    ) {
      patchFlag = 0;
      optimized = false;
      dynamicChildren = null;
    }
    if (fragmentSlotScopeIds) {
      slotScopeIds = slotScopeIds ? slotScopeIds.concat(fragmentSlotScopeIds) : fragmentSlotScopeIds;
    }
    if (n1 == null) {
      hostInsert(fragmentStartAnchor, container, anchor);
      hostInsert(fragmentEndAnchor, container, anchor);
      mountChildren(
        // #10007
        // such fragment like `<></>` will be compiled into
        // a fragment which doesn't have a children.
        // In this case fallback to an empty array
        n2.children || [],
        container,
        fragmentEndAnchor,
        parentComponent,
        parentSuspense,
        namespace,
        slotScopeIds,
        optimized
      );
    } else {
      if (patchFlag > 0 && patchFlag & 64 && dynamicChildren && // #2715 the previous fragment could've been a BAILed one as a result
      // of renderSlot() with no valid children
      n1.dynamicChildren) {
        patchBlockChildren(n1.dynamicChildren, dynamicChildren, container, parentComponent, parentSuspense, namespace, slotScopeIds);
        if (true) {
          traverseStaticChildren(n1, n2);
        } else if (
          // #2080 if the stable fragment has a key, it's a <template v-for> that may
          //  get moved around. Make sure all root level vnodes inherit el.
          // #2134 or if it's a component root, it may also get moved around
          // as the component is being moved.
          n2.key != null || parentComponent && n2 === parentComponent.subTree
        ) {
          traverseStaticChildren(n1, n2, true);
        }
      } else {
        patchChildren(n1, n2, container, fragmentEndAnchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized);
      }
    }
  };
  const processComponent = (n1, n2, container, anchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized) => {
    n2.slotScopeIds = slotScopeIds;
    if (n1 == null) {
      if (n2.shapeFlag & 512) {
        parentComponent.ctx.activate(n2, container, anchor, namespace, optimized);
      } else {
        mountComponent(n2, container, anchor, parentComponent, parentSuspense, namespace, optimized);
      }
    } else {
      updateComponent(n1, n2, optimized);
    }
  };
  const mountComponent = (initialVNode, container, anchor, parentComponent, parentSuspense, namespace, optimized) => {
    const instance = initialVNode.component = createComponentInstance(initialVNode, parentComponent, parentSuspense);
    if (instance.type.__hmrId) {
      registerHMR(instance);
    }
    if (true) {
      pushWarningContext(initialVNode);
      startMeasure(instance, `mount`);
    }
    if (isKeepAlive(initialVNode)) {
      instance.ctx.renderer = internals;
    }
    {
      if (true) {
        startMeasure(instance, `init`);
      }
      setupComponent(instance, false, optimized);
      if (true) {
        endMeasure(instance, `init`);
      }
    }
    if (instance.asyncDep) {
      if (isHmrUpdating) initialVNode.el = null;
      parentSuspense && parentSuspense.registerDep(instance, setupRenderEffect, optimized);
      if (!initialVNode.el) {
        const placeholder = instance.subTree = createVNode(Comment);
        processCommentNode(null, placeholder, container, anchor);
      }
    } else {
      setupRenderEffect(instance, initialVNode, container, anchor, parentSuspense, namespace, optimized);
    }
    if (true) {
      popWarningContext();
      endMeasure(instance, `mount`);
    }
  };
  const updateComponent = (n1, n2, optimized) => {
    const instance = n2.component = n1.component;
    if (shouldUpdateComponent(n1, n2, optimized)) {
      if (instance.asyncDep && !instance.asyncResolved) {
        if (true) {
          pushWarningContext(n2);
        }
        updateComponentPreRender(instance, n2, optimized);
        if (true) {
          popWarningContext();
        }
        return;
      } else {
        instance.next = n2;
        instance.update();
      }
    } else {
      n2.el = n1.el;
      instance.vnode = n2;
    }
  };
  const setupRenderEffect = (instance, initialVNode, container, anchor, parentSuspense, namespace, optimized) => {
    const componentUpdateFn = () => {
      if (!instance.isMounted) {
        let vnodeHook;
        const { el, props } = initialVNode;
        const { bm, m, parent, root, type } = instance;
        const isAsyncWrapperVNode = isAsyncWrapper(initialVNode);
        toggleRecurse(instance, false);
        if (bm) {
          invokeArrayFns(bm);
        }
        if (!isAsyncWrapperVNode && (vnodeHook = props && props.onVnodeBeforeMount)) {
          invokeVNodeHook(vnodeHook, parent, initialVNode);
        }
        toggleRecurse(instance, true);
        if (el && hydrateNode) {
          const hydrateSubTree = () => {
            if (true) {
              startMeasure(instance, `render`);
            }
            instance.subTree = renderComponentRoot(instance);
            if (true) {
              endMeasure(instance, `render`);
            }
            if (true) {
              startMeasure(instance, `hydrate`);
            }
            hydrateNode(el, instance.subTree, instance, parentSuspense, null);
            if (true) {
              endMeasure(instance, `hydrate`);
            }
          };
          if (isAsyncWrapperVNode && type.__asyncHydrate) {
            type.__asyncHydrate(el, instance, hydrateSubTree);
          } else {
            hydrateSubTree();
          }
        } else {
          if (root.ce) {
            root.ce._injectChildStyle(type);
          }
          if (true) {
            startMeasure(instance, `render`);
          }
          const subTree = instance.subTree = renderComponentRoot(instance);
          if (true) {
            endMeasure(instance, `render`);
          }
          if (true) {
            startMeasure(instance, `patch`);
          }
          patch(null, subTree, container, anchor, instance, parentSuspense, namespace);
          if (true) {
            endMeasure(instance, `patch`);
          }
          initialVNode.el = subTree.el;
        }
        if (m) {
          queuePostRenderEffect(m, parentSuspense);
        }
        if (!isAsyncWrapperVNode && (vnodeHook = props && props.onVnodeMounted)) {
          const scopedInitialVNode = initialVNode;
          queuePostRenderEffect(() => invokeVNodeHook(vnodeHook, parent, scopedInitialVNode), parentSuspense);
        }
        if (initialVNode.shapeFlag & 256 || parent && isAsyncWrapper(parent.vnode) && parent.vnode.shapeFlag & 256) {
          instance.a && queuePostRenderEffect(instance.a, parentSuspense);
        }
        instance.isMounted = true;
        if (true) {
          devtoolsComponentAdded(instance);
        }
        initialVNode = container = anchor = null;
      } else {
        let { next, bu, u, parent, vnode } = instance;
        {
          const nonHydratedAsyncRoot = locateNonHydratedAsyncRoot(instance);
          if (nonHydratedAsyncRoot) {
            if (next) {
              next.el = vnode.el;
              updateComponentPreRender(instance, next, optimized);
            }
            nonHydratedAsyncRoot.asyncDep.then(() => {
              if (!instance.isUnmounted) {
                componentUpdateFn();
              }
            });
            return;
          }
        }
        let originNext = next;
        let vnodeHook;
        if (true) {
          pushWarningContext(next || instance.vnode);
        }
        toggleRecurse(instance, false);
        if (next) {
          next.el = vnode.el;
          updateComponentPreRender(instance, next, optimized);
        } else {
          next = vnode;
        }
        if (bu) {
          invokeArrayFns(bu);
        }
        if (vnodeHook = next.props && next.props.onVnodeBeforeUpdate) {
          invokeVNodeHook(vnodeHook, parent, next, vnode);
        }
        toggleRecurse(instance, true);
        if (true) {
          startMeasure(instance, `render`);
        }
        const nextTree = renderComponentRoot(instance);
        if (true) {
          endMeasure(instance, `render`);
        }
        const prevTree = instance.subTree;
        instance.subTree = nextTree;
        if (true) {
          startMeasure(instance, `patch`);
        }
        patch(
          prevTree,
          nextTree,
          // parent may have changed if it's in a teleport
          hostParentNode(prevTree.el),
          // anchor may have changed if it's in a fragment
          getNextHostNode(prevTree),
          instance,
          parentSuspense,
          namespace
        );
        if (true) {
          endMeasure(instance, `patch`);
        }
        next.el = nextTree.el;
        if (originNext === null) {
          updateHOCHostEl(instance, nextTree.el);
        }
        if (u) {
          queuePostRenderEffect(u, parentSuspense);
        }
        if (vnodeHook = next.props && next.props.onVnodeUpdated) {
          queuePostRenderEffect(() => invokeVNodeHook(vnodeHook, parent, next, vnode), parentSuspense);
        }
        if (true) {
          devtoolsComponentUpdated(instance);
        }
        if (true) {
          popWarningContext();
        }
      }
    };
    instance.scope.on();
    const effect2 = instance.effect = new ReactiveEffect(componentUpdateFn);
    instance.scope.off();
    const update2 = instance.update = effect2.run.bind(effect2);
    const job = instance.job = effect2.runIfDirty.bind(effect2);
    job.i = instance;
    job.id = instance.uid;
    effect2.scheduler = () => queueJob(job);
    toggleRecurse(instance, true);
    if (true) {
      effect2.onTrack = instance.rtc ? (e) => invokeArrayFns(instance.rtc, e) : void 0;
      effect2.onTrigger = instance.rtg ? (e) => invokeArrayFns(instance.rtg, e) : void 0;
    }
    update2();
  };
  const updateComponentPreRender = (instance, nextVNode, optimized) => {
    nextVNode.component = instance;
    const prevProps = instance.vnode.props;
    instance.vnode = nextVNode;
    instance.next = null;
    updateProps(instance, nextVNode.props, prevProps, optimized);
    updateSlots(instance, nextVNode.children, optimized);
    pauseTracking();
    flushPreFlushCbs(instance);
    resetTracking();
  };
  const patchChildren = (n1, n2, container, anchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized = false) => {
    const c1 = n1 && n1.children;
    const prevShapeFlag = n1 ? n1.shapeFlag : 0;
    const c2 = n2.children;
    const { patchFlag, shapeFlag } = n2;
    if (patchFlag > 0) {
      if (patchFlag & 128) {
        patchKeyedChildren(c1, c2, container, anchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized);
        return;
      } else if (patchFlag & 256) {
        patchUnkeyedChildren(c1, c2, container, anchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized);
        return;
      }
    }
    if (shapeFlag & 8) {
      if (prevShapeFlag & 16) {
        unmountChildren(c1, parentComponent, parentSuspense);
      }
      if (c2 !== c1) {
        hostSetElementText(container, c2);
      }
    } else {
      if (prevShapeFlag & 16) {
        if (shapeFlag & 16) {
          patchKeyedChildren(c1, c2, container, anchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized);
        } else {
          unmountChildren(c1, parentComponent, parentSuspense, true);
        }
      } else {
        if (prevShapeFlag & 8) {
          hostSetElementText(container, "");
        }
        if (shapeFlag & 16) {
          mountChildren(c2, container, anchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized);
        }
      }
    }
  };
  const patchUnkeyedChildren = (c1, c2, container, anchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized) => {
    c1 = c1 || EMPTY_ARR;
    c2 = c2 || EMPTY_ARR;
    const oldLength = c1.length;
    const newLength = c2.length;
    const commonLength = Math.min(oldLength, newLength);
    let i;
    for (i = 0; i < commonLength; i++) {
      const nextChild = c2[i] = optimized ? cloneIfMounted(c2[i]) : normalizeVNode(c2[i]);
      patch(c1[i], nextChild, container, null, parentComponent, parentSuspense, namespace, slotScopeIds, optimized);
    }
    if (oldLength > newLength) {
      unmountChildren(c1, parentComponent, parentSuspense, true, false, commonLength);
    } else {
      mountChildren(c2, container, anchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized, commonLength);
    }
  };
  const patchKeyedChildren = (c1, c2, container, parentAnchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized) => {
    let i = 0;
    const l2 = c2.length;
    let e1 = c1.length - 1;
    let e2 = l2 - 1;
    while (i <= e1 && i <= e2) {
      const n1 = c1[i];
      const n2 = c2[i] = optimized ? cloneIfMounted(c2[i]) : normalizeVNode(c2[i]);
      if (isSameVNodeType(n1, n2)) {
        patch(n1, n2, container, null, parentComponent, parentSuspense, namespace, slotScopeIds, optimized);
      } else {
        break;
      }
      i++;
    }
    while (i <= e1 && i <= e2) {
      const n1 = c1[e1];
      const n2 = c2[e2] = optimized ? cloneIfMounted(c2[e2]) : normalizeVNode(c2[e2]);
      if (isSameVNodeType(n1, n2)) {
        patch(n1, n2, container, null, parentComponent, parentSuspense, namespace, slotScopeIds, optimized);
      } else {
        break;
      }
      e1--;
      e2--;
    }
    if (i > e1) {
      if (i <= e2) {
        const nextPos = e2 + 1;
        const anchor = nextPos < l2 ? c2[nextPos].el : parentAnchor;
        while (i <= e2) {
          patch(null, c2[i] = optimized ? cloneIfMounted(c2[i]) : normalizeVNode(c2[i]), container, anchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized);
          i++;
        }
      }
    } else if (i > e2) {
      while (i <= e1) {
        unmount(c1[i], parentComponent, parentSuspense, true);
        i++;
      }
    } else {
      const s1 = i;
      const s2 = i;
      const keyToNewIndexMap = /* @__PURE__ */ new Map();
      for (i = s2; i <= e2; i++) {
        const nextChild = c2[i] = optimized ? cloneIfMounted(c2[i]) : normalizeVNode(c2[i]);
        if (nextChild.key != null) {
          if (keyToNewIndexMap.has(nextChild.key)) {
            warn$1(`Duplicate keys found during update:`, JSON.stringify(nextChild.key), `Make sure keys are unique.`);
          }
          keyToNewIndexMap.set(nextChild.key, i);
        }
      }
      let j;
      let patched = 0;
      const toBePatched = e2 - s2 + 1;
      let moved = false;
      let maxNewIndexSoFar = 0;
      const newIndexToOldIndexMap = new Array(toBePatched);
      for (i = 0; i < toBePatched; i++) newIndexToOldIndexMap[i] = 0;
      for (i = s1; i <= e1; i++) {
        const prevChild = c1[i];
        if (patched >= toBePatched) {
          unmount(prevChild, parentComponent, parentSuspense, true);
          continue;
        }
        let newIndex;
        if (prevChild.key != null) {
          newIndex = keyToNewIndexMap.get(prevChild.key);
        } else {
          for (j = s2; j <= e2; j++) {
            if (newIndexToOldIndexMap[j - s2] === 0 && isSameVNodeType(prevChild, c2[j])) {
              newIndex = j;
              break;
            }
          }
        }
        if (newIndex === void 0) {
          unmount(prevChild, parentComponent, parentSuspense, true);
        } else {
          newIndexToOldIndexMap[newIndex - s2] = i + 1;
          if (newIndex >= maxNewIndexSoFar) {
            maxNewIndexSoFar = newIndex;
          } else {
            moved = true;
          }
          patch(prevChild, c2[newIndex], container, null, parentComponent, parentSuspense, namespace, slotScopeIds, optimized);
          patched++;
        }
      }
      const increasingNewIndexSequence = moved ? getSequence(newIndexToOldIndexMap) : EMPTY_ARR;
      j = increasingNewIndexSequence.length - 1;
      for (i = toBePatched - 1; i >= 0; i--) {
        const nextIndex = s2 + i;
        const nextChild = c2[nextIndex];
        const anchor = nextIndex + 1 < l2 ? c2[nextIndex + 1].el : parentAnchor;
        if (newIndexToOldIndexMap[i] === 0) {
          patch(null, nextChild, container, anchor, parentComponent, parentSuspense, namespace, slotScopeIds, optimized);
        } else if (moved) {
          if (j < 0 || i !== increasingNewIndexSequence[j]) {
            move(nextChild, container, anchor, 2);
          } else {
            j--;
          }
        }
      }
    }
  };
  const move = (vnode, container, anchor, moveType, parentSuspense = null) => {
    const { el, type, transition, children, shapeFlag } = vnode;
    if (shapeFlag & 6) {
      move(vnode.component.subTree, container, anchor, moveType);
      return;
    }
    if (shapeFlag & 128) {
      vnode.suspense.move(container, anchor, moveType);
      return;
    }
    if (shapeFlag & 64) {
      type.move(vnode, container, anchor, internals);
      return;
    }
    if (type === Fragment) {
      hostInsert(el, container, anchor);
      for (let i = 0; i < children.length; i++) {
        move(children[i], container, anchor, moveType);
      }
      hostInsert(vnode.anchor, container, anchor);
      return;
    }
    if (type === Static) {
      moveStaticNode(vnode, container, anchor);
      return;
    }
    const needTransition2 = moveType !== 2 && shapeFlag & 1 && transition;
    if (needTransition2) {
      if (moveType === 0) {
        transition.beforeEnter(el);
        hostInsert(el, container, anchor);
        queuePostRenderEffect(() => transition.enter(el), parentSuspense);
      } else {
        const { leave, delayLeave, afterLeave } = transition;
        const remove22 = () => hostInsert(el, container, anchor);
        const performLeave = () => {
          leave(el, () => {
            remove22();
            afterLeave && afterLeave();
          });
        };
        if (delayLeave) {
          delayLeave(el, remove22, performLeave);
        } else {
          performLeave();
        }
      }
    } else {
      hostInsert(el, container, anchor);
    }
  };
  const unmount = (vnode, parentComponent, parentSuspense, doRemove = false, optimized = false) => {
    const { type, props, ref: ref2, children, dynamicChildren, shapeFlag, patchFlag, dirs, cacheIndex } = vnode;
    if (patchFlag === -2) {
      optimized = false;
    }
    if (ref2 != null) {
      setRef(ref2, null, parentSuspense, vnode, true);
    }
    if (cacheIndex != null) {
      parentComponent.renderCache[cacheIndex] = void 0;
    }
    if (shapeFlag & 256) {
      parentComponent.ctx.deactivate(vnode);
      return;
    }
    const shouldInvokeDirs = shapeFlag & 1 && dirs;
    const shouldInvokeVnodeHook = !isAsyncWrapper(vnode);
    let vnodeHook;
    if (shouldInvokeVnodeHook && (vnodeHook = props && props.onVnodeBeforeUnmount)) {
      invokeVNodeHook(vnodeHook, parentComponent, vnode);
    }
    if (shapeFlag & 6) {
      unmountComponent(vnode.component, parentSuspense, doRemove);
    } else {
      if (shapeFlag & 128) {
        vnode.suspense.unmount(parentSuspense, doRemove);
        return;
      }
      if (shouldInvokeDirs) {
        invokeDirectiveHook(vnode, null, parentComponent, "beforeUnmount");
      }
      if (shapeFlag & 64) {
        vnode.type.remove(vnode, parentComponent, parentSuspense, internals, doRemove);
      } else if (dynamicChildren && // #5154
      // when v-once is used inside a block, setBlockTracking(-1) marks the
      // parent block with hasOnce: true
      // so that it doesn't take the fast path during unmount - otherwise
      // components nested in v-once are never unmounted.
      !dynamicChildren.hasOnce && // #1153: fast path should not be taken for non-stable (v-for) fragments
      (type !== Fragment || patchFlag > 0 && patchFlag & 64)) {
        unmountChildren(dynamicChildren, parentComponent, parentSuspense, false, true);
      } else if (type === Fragment && patchFlag & (128 | 256) || !optimized && shapeFlag & 16) {
        unmountChildren(children, parentComponent, parentSuspense);
      }
      if (doRemove) {
        remove2(vnode);
      }
    }
    if (shouldInvokeVnodeHook && (vnodeHook = props && props.onVnodeUnmounted) || shouldInvokeDirs) {
      queuePostRenderEffect(() => {
        vnodeHook && invokeVNodeHook(vnodeHook, parentComponent, vnode);
        shouldInvokeDirs && invokeDirectiveHook(vnode, null, parentComponent, "unmounted");
      }, parentSuspense);
    }
  };
  const remove2 = (vnode) => {
    const { type, el, anchor, transition } = vnode;
    if (type === Fragment) {
      if (vnode.patchFlag > 0 && vnode.patchFlag & 2048 && transition && !transition.persisted) {
        vnode.children.forEach((child) => {
          if (child.type === Comment) {
            hostRemove(child.el);
          } else {
            remove2(child);
          }
        });
      } else {
        removeFragment(el, anchor);
      }
      return;
    }
    if (type === Static) {
      removeStaticNode(vnode);
      return;
    }
    const performRemove = () => {
      hostRemove(el);
      if (transition && !transition.persisted && transition.afterLeave) {
        transition.afterLeave();
      }
    };
    if (vnode.shapeFlag & 1 && transition && !transition.persisted) {
      const { leave, delayLeave } = transition;
      const performLeave = () => leave(el, performRemove);
      if (delayLeave) {
        delayLeave(vnode.el, performRemove, performLeave);
      } else {
        performLeave();
      }
    } else {
      performRemove();
    }
  };
  const removeFragment = (cur, end) => {
    let next;
    while (cur !== end) {
      next = hostNextSibling(cur);
      hostRemove(cur);
      cur = next;
    }
    hostRemove(end);
  };
  const unmountComponent = (instance, parentSuspense, doRemove) => {
    if (instance.type.__hmrId) {
      unregisterHMR(instance);
    }
    const { bum, scope, job, subTree, um, m, a } = instance;
    invalidateMount(m);
    invalidateMount(a);
    if (bum) {
      invokeArrayFns(bum);
    }
    scope.stop();
    if (job) {
      job.flags |= 8;
      unmount(subTree, instance, parentSuspense, doRemove);
    }
    if (um) {
      queuePostRenderEffect(um, parentSuspense);
    }
    queuePostRenderEffect(() => {
      instance.isUnmounted = true;
    }, parentSuspense);
    if (parentSuspense && parentSuspense.pendingBranch && !parentSuspense.isUnmounted && instance.asyncDep && !instance.asyncResolved && instance.suspenseId === parentSuspense.pendingId) {
      parentSuspense.deps--;
      if (parentSuspense.deps === 0) {
        parentSuspense.resolve();
      }
    }
    if (true) {
      devtoolsComponentRemoved(instance);
    }
  };
  const unmountChildren = (children, parentComponent, parentSuspense, doRemove = false, optimized = false, start = 0) => {
    for (let i = start; i < children.length; i++) {
      unmount(children[i], parentComponent, parentSuspense, doRemove, optimized);
    }
  };
  const getNextHostNode = (vnode) => {
    if (vnode.shapeFlag & 6) {
      return getNextHostNode(vnode.component.subTree);
    }
    if (vnode.shapeFlag & 128) {
      return vnode.suspense.next();
    }
    const el = hostNextSibling(vnode.anchor || vnode.el);
    const teleportEnd = el && el[TeleportEndKey];
    return teleportEnd ? hostNextSibling(teleportEnd) : el;
  };
  let isFlushing = false;
  const render = (vnode, container, namespace) => {
    if (vnode == null) {
      if (container._vnode) {
        unmount(container._vnode, null, null, true);
      }
    } else {
      patch(container._vnode || null, vnode, container, null, null, null, namespace);
    }
    container._vnode = vnode;
    if (!isFlushing) {
      isFlushing = true;
      flushPreFlushCbs();
      flushPostFlushCbs();
      isFlushing = false;
    }
  };
  const internals = {
    p: patch,
    um: unmount,
    m: move,
    r: remove2,
    mt: mountComponent,
    mc: mountChildren,
    pc: patchChildren,
    pbc: patchBlockChildren,
    n: getNextHostNode,
    o: options
  };
  let hydrate;
  let hydrateNode;
  if (createHydrationFns) {
    [hydrate, hydrateNode] = createHydrationFns(internals);
  }
  return {
    render,
    hydrate,
    createApp: createAppAPI(render, hydrate)
  };
}
function resolveChildrenNamespace({ type, props }, currentNamespace) {
  return currentNamespace === "svg" && type === "foreignObject" || currentNamespace === "mathml" && type === "annotation-xml" && props && props.encoding && props.encoding.includes("html") ? void 0 : currentNamespace;
}
function toggleRecurse({ effect: effect2, job }, allowed) {
  if (allowed) {
    effect2.flags |= 32;
    job.flags |= 4;
  } else {
    effect2.flags &= ~32;
    job.flags &= ~4;
  }
}
function needTransition(parentSuspense, transition) {
  return (!parentSuspense || parentSuspense && !parentSuspense.pendingBranch) && transition && !transition.persisted;
}
function traverseStaticChildren(n1, n2, shallow = false) {
  const ch1 = n1.children;
  const ch2 = n2.children;
  if (isArray(ch1) && isArray(ch2)) {
    for (let i = 0; i < ch1.length; i++) {
      const c1 = ch1[i];
      let c2 = ch2[i];
      if (c2.shapeFlag & 1 && !c2.dynamicChildren) {
        if (c2.patchFlag <= 0 || c2.patchFlag === 32) {
          c2 = ch2[i] = cloneIfMounted(ch2[i]);
          c2.el = c1.el;
        }
        if (!shallow && c2.patchFlag !== -2) traverseStaticChildren(c1, c2);
      }
      if (c2.type === Text) {
        c2.el = c1.el;
      }
      if (c2.type === Comment && !c2.el) {
        c2.el = c1.el;
      }
    }
  }
}
function getSequence(arr) {
  const p2 = arr.slice();
  const result = [
    0
  ];
  let i, j, u, v, c;
  const len = arr.length;
  for (i = 0; i < len; i++) {
    const arrI = arr[i];
    if (arrI !== 0) {
      j = result[result.length - 1];
      if (arr[j] < arrI) {
        p2[i] = j;
        result.push(i);
        continue;
      }
      u = 0;
      v = result.length - 1;
      while (u < v) {
        c = u + v >> 1;
        if (arr[result[c]] < arrI) {
          u = c + 1;
        } else {
          v = c;
        }
      }
      if (arrI < arr[result[u]]) {
        if (u > 0) {
          p2[i] = result[u - 1];
        }
        result[u] = i;
      }
    }
  }
  u = result.length;
  v = result[u - 1];
  while (u-- > 0) {
    result[u] = v;
    v = p2[v];
  }
  return result;
}
function locateNonHydratedAsyncRoot(instance) {
  const subComponent = instance.subTree.component;
  if (subComponent) {
    if (subComponent.asyncDep && !subComponent.asyncResolved) {
      return subComponent;
    } else {
      return locateNonHydratedAsyncRoot(subComponent);
    }
  }
}
function invalidateMount(hooks2) {
  if (hooks2) {
    for (let i = 0; i < hooks2.length; i++) hooks2[i].flags |= 8;
  }
}
var ssrContextKey = Symbol.for("v-scx");
var useSSRContext = () => {
  {
    const ctx = inject(ssrContextKey);
    if (!ctx) {
      warn$1(`Server rendering context not provided. Make sure to only call useSSRContext() conditionally in the server build.`);
    }
    return ctx;
  }
};
function watch2(source, cb, options) {
  if (!isFunction(cb)) {
    warn$1(`\`watch(fn, options?)\` signature has been moved to a separate API. Use \`watchEffect(fn, options?)\` instead. \`watch\` now only supports \`watch(source, cb, options?) signature.`);
  }
  return doWatch(source, cb, options);
}
function doWatch(source, cb, options = EMPTY_OBJ) {
  const { immediate, deep, flush, once } = options;
  if (!cb) {
    if (immediate !== void 0) {
      warn$1(`watch() "immediate" option is only respected when using the watch(source, callback, options?) signature.`);
    }
    if (deep !== void 0) {
      warn$1(`watch() "deep" option is only respected when using the watch(source, callback, options?) signature.`);
    }
    if (once !== void 0) {
      warn$1(`watch() "once" option is only respected when using the watch(source, callback, options?) signature.`);
    }
  }
  const baseWatchOptions = extend({}, options);
  if (true) baseWatchOptions.onWarn = warn$1;
  const runsImmediately = cb && immediate || !cb && flush !== "post";
  let ssrCleanup;
  if (isInSSRComponentSetup) {
    if (flush === "sync") {
      const ctx = useSSRContext();
      ssrCleanup = ctx.__watcherHandles || (ctx.__watcherHandles = []);
    } else if (!runsImmediately) {
      const watchStopHandle = () => {
      };
      watchStopHandle.stop = NOOP;
      watchStopHandle.resume = NOOP;
      watchStopHandle.pause = NOOP;
      return watchStopHandle;
    }
  }
  const instance = currentInstance;
  baseWatchOptions.call = (fn, type, args) => callWithAsyncErrorHandling(fn, instance, type, args);
  let isPre = false;
  if (flush === "post") {
    baseWatchOptions.scheduler = (job) => {
      queuePostRenderEffect(job, instance && instance.suspense);
    };
  } else if (flush !== "sync") {
    isPre = true;
    baseWatchOptions.scheduler = (job, isFirstRun) => {
      if (isFirstRun) {
        job();
      } else {
        queueJob(job);
      }
    };
  }
  baseWatchOptions.augmentJob = (job) => {
    if (cb) {
      job.flags |= 4;
    }
    if (isPre) {
      job.flags |= 2;
      if (instance) {
        job.id = instance.uid;
        job.i = instance;
      }
    }
  };
  const watchHandle = watch(source, cb, baseWatchOptions);
  if (isInSSRComponentSetup) {
    if (ssrCleanup) {
      ssrCleanup.push(watchHandle);
    } else if (runsImmediately) {
      watchHandle();
    }
  }
  return watchHandle;
}
function instanceWatch(source, value, options) {
  const publicThis = this.proxy;
  const getter = isString(source) ? source.includes(".") ? createPathGetter(publicThis, source) : () => publicThis[source] : source.bind(publicThis, publicThis);
  let cb;
  if (isFunction(value)) {
    cb = value;
  } else {
    cb = value.handler;
    options = value;
  }
  const reset = setCurrentInstance(this);
  const res = doWatch(getter, cb.bind(publicThis), options);
  reset();
  return res;
}
function createPathGetter(ctx, path) {
  const segments = path.split(".");
  return () => {
    let cur = ctx;
    for (let i = 0; i < segments.length && cur; i++) {
      cur = cur[segments[i]];
    }
    return cur;
  };
}
var getModelModifiers = (props, modelName) => {
  return modelName === "modelValue" || modelName === "model-value" ? props.modelModifiers : props[`${modelName}Modifiers`] || props[`${camelize(modelName)}Modifiers`] || props[`${hyphenate(modelName)}Modifiers`];
};
function emit(instance, event, ...rawArgs) {
  if (instance.isUnmounted) return;
  const props = instance.vnode.props || EMPTY_OBJ;
  if (true) {
    const { emitsOptions, propsOptions: [propsOptions] } = instance;
    if (emitsOptions) {
      if (!(event in emitsOptions) && true) {
        if (!propsOptions || !(toHandlerKey(camelize(event)) in propsOptions)) {
          warn$1(`Component emitted event "${event}" but it is neither declared in the emits option nor as an "${toHandlerKey(camelize(event))}" prop.`);
        }
      } else {
        const validator = emitsOptions[event];
        if (isFunction(validator)) {
          const isValid = validator(...rawArgs);
          if (!isValid) {
            warn$1(`Invalid event arguments: event validation failed for event "${event}".`);
          }
        }
      }
    }
  }
  let args = rawArgs;
  const isModelListener2 = event.startsWith("update:");
  const modifiers = isModelListener2 && getModelModifiers(props, event.slice(7));
  if (modifiers) {
    if (modifiers.trim) {
      args = rawArgs.map((a) => isString(a) ? a.trim() : a);
    }
    if (modifiers.number) {
      args = rawArgs.map(looseToNumber);
    }
  }
  if (true) {
    devtoolsComponentEmit(instance, event, args);
  }
  if (true) {
    const lowerCaseEvent = event.toLowerCase();
    if (lowerCaseEvent !== event && props[toHandlerKey(lowerCaseEvent)]) {
      warn$1(`Event "${lowerCaseEvent}" is emitted in component ${formatComponentName(instance, instance.type)} but the handler is registered for "${event}". Note that HTML attributes are case-insensitive and you cannot use v-on to listen to camelCase events when using in-DOM templates. You should probably use "${hyphenate(event)}" instead of "${event}".`);
    }
  }
  let handlerName;
  let handler = props[handlerName = toHandlerKey(event)] || // also try camelCase event handler (#2249)
  props[handlerName = toHandlerKey(camelize(event))];
  if (!handler && isModelListener2) {
    handler = props[handlerName = toHandlerKey(hyphenate(event))];
  }
  if (handler) {
    callWithAsyncErrorHandling(handler, instance, 6, args);
  }
  const onceHandler = props[handlerName + `Once`];
  if (onceHandler) {
    if (!instance.emitted) {
      instance.emitted = {};
    } else if (instance.emitted[handlerName]) {
      return;
    }
    instance.emitted[handlerName] = true;
    callWithAsyncErrorHandling(onceHandler, instance, 6, args);
  }
}
function normalizeEmitsOptions(comp, appContext, asMixin = false) {
  const cache = appContext.emitsCache;
  const cached = cache.get(comp);
  if (cached !== void 0) {
    return cached;
  }
  const raw = comp.emits;
  let normalized = {};
  let hasExtends = false;
  if (__VUE_OPTIONS_API__ && !isFunction(comp)) {
    const extendEmits = (raw2) => {
      const normalizedFromExtend = normalizeEmitsOptions(raw2, appContext, true);
      if (normalizedFromExtend) {
        hasExtends = true;
        extend(normalized, normalizedFromExtend);
      }
    };
    if (!asMixin && appContext.mixins.length) {
      appContext.mixins.forEach(extendEmits);
    }
    if (comp.extends) {
      extendEmits(comp.extends);
    }
    if (comp.mixins) {
      comp.mixins.forEach(extendEmits);
    }
  }
  if (!raw && !hasExtends) {
    if (isObject(comp)) {
      cache.set(comp, null);
    }
    return null;
  }
  if (isArray(raw)) {
    raw.forEach((key) => normalized[key] = null);
  } else {
    extend(normalized, raw);
  }
  if (isObject(comp)) {
    cache.set(comp, normalized);
  }
  return normalized;
}
function isEmitListener(options, key) {
  if (!options || !isOn(key)) {
    return false;
  }
  key = key.slice(2).replace(/Once$/, "");
  return hasOwn(options, key[0].toLowerCase() + key.slice(1)) || hasOwn(options, hyphenate(key)) || hasOwn(options, key);
}
var accessedAttrs = false;
function markAttrsAccessed() {
  accessedAttrs = true;
}
function renderComponentRoot(instance) {
  const { type: Component, vnode, proxy, withProxy, propsOptions: [propsOptions], slots, attrs, emit: emit2, render, renderCache, props, data, setupState, ctx, inheritAttrs } = instance;
  const prev = setCurrentRenderingInstance(instance);
  let result;
  let fallthroughAttrs;
  if (true) {
    accessedAttrs = false;
  }
  try {
    if (vnode.shapeFlag & 4) {
      const proxyToUse = withProxy || proxy;
      const thisProxy = setupState.__isScriptSetup ? new Proxy(proxyToUse, {
        get(target2, key, receiver) {
          warn$1(`Property '${String(key)}' was accessed via 'this'. Avoid using 'this' in templates.`);
          return Reflect.get(target2, key, receiver);
        }
      }) : proxyToUse;
      result = normalizeVNode(render.call(thisProxy, proxyToUse, renderCache, true ? shallowReadonly(props) : props, setupState, data, ctx));
      fallthroughAttrs = attrs;
    } else {
      const render2 = Component;
      if (attrs === props) {
        markAttrsAccessed();
      }
      result = normalizeVNode(render2.length > 1 ? render2(true ? shallowReadonly(props) : props, true ? {
        get attrs() {
          markAttrsAccessed();
          return shallowReadonly(attrs);
        },
        slots,
        emit: emit2
      } : {
        attrs,
        slots,
        emit: emit2
      }) : render2(true ? shallowReadonly(props) : props, null));
      fallthroughAttrs = Component.props ? attrs : getFunctionalFallthrough(attrs);
    }
  } catch (err) {
    blockStack.length = 0;
    handleError(err, instance, 1);
    result = createVNode(Comment);
  }
  let root = result;
  let setRoot = void 0;
  if (result.patchFlag > 0 && result.patchFlag & 2048) {
    [root, setRoot] = getChildRoot(result);
  }
  if (fallthroughAttrs && inheritAttrs !== false) {
    const keys = Object.keys(fallthroughAttrs);
    const { shapeFlag } = root;
    if (keys.length) {
      if (shapeFlag & (1 | 6)) {
        if (propsOptions && keys.some(isModelListener)) {
          fallthroughAttrs = filterModelListeners(fallthroughAttrs, propsOptions);
        }
        root = cloneVNode(root, fallthroughAttrs, false, true);
      } else if (!accessedAttrs && root.type !== Comment) {
        const allAttrs = Object.keys(attrs);
        const eventAttrs = [];
        const extraAttrs = [];
        for (let i = 0, l = allAttrs.length; i < l; i++) {
          const key = allAttrs[i];
          if (isOn(key)) {
            if (!isModelListener(key)) {
              eventAttrs.push(key[2].toLowerCase() + key.slice(3));
            }
          } else {
            extraAttrs.push(key);
          }
        }
        if (extraAttrs.length) {
          warn$1(`Extraneous non-props attributes (${extraAttrs.join(", ")}) were passed to component but could not be automatically inherited because component renders fragment or text or teleport root nodes.`);
        }
        if (eventAttrs.length) {
          warn$1(`Extraneous non-emits event listeners (${eventAttrs.join(", ")}) were passed to component but could not be automatically inherited because component renders fragment or text root nodes. If the listener is intended to be a component custom event listener only, declare it using the "emits" option.`);
        }
      }
    }
  }
  if (vnode.dirs) {
    if (!isElementRoot(root)) {
      warn$1(`Runtime directive used on component with non-element root node. The directives will not function as intended.`);
    }
    root = cloneVNode(root, null, false, true);
    root.dirs = root.dirs ? root.dirs.concat(vnode.dirs) : vnode.dirs;
  }
  if (vnode.transition) {
    if (!isElementRoot(root)) {
      warn$1(`Component inside <Transition> renders non-element root node that cannot be animated.`);
    }
    setTransitionHooks(root, vnode.transition);
  }
  if (setRoot) {
    setRoot(root);
  } else {
    result = root;
  }
  setCurrentRenderingInstance(prev);
  return result;
}
var getChildRoot = (vnode) => {
  const rawChildren = vnode.children;
  const dynamicChildren = vnode.dynamicChildren;
  const childRoot = filterSingleRoot(rawChildren, false);
  if (!childRoot) {
    return [
      vnode,
      void 0
    ];
  } else if (childRoot.patchFlag > 0 && childRoot.patchFlag & 2048) {
    return getChildRoot(childRoot);
  }
  const index = rawChildren.indexOf(childRoot);
  const dynamicIndex = dynamicChildren ? dynamicChildren.indexOf(childRoot) : -1;
  const setRoot = (updatedRoot) => {
    rawChildren[index] = updatedRoot;
    if (dynamicChildren) {
      if (dynamicIndex > -1) {
        dynamicChildren[dynamicIndex] = updatedRoot;
      } else if (updatedRoot.patchFlag > 0) {
        vnode.dynamicChildren = [
          ...dynamicChildren,
          updatedRoot
        ];
      }
    }
  };
  return [
    normalizeVNode(childRoot),
    setRoot
  ];
};
function filterSingleRoot(children, recurse = true) {
  let singleRoot;
  for (let i = 0; i < children.length; i++) {
    const child = children[i];
    if (isVNode(child)) {
      if (child.type !== Comment || child.children === "v-if") {
        if (singleRoot) {
          return;
        } else {
          singleRoot = child;
          if (recurse && singleRoot.patchFlag > 0 && singleRoot.patchFlag & 2048) {
            return filterSingleRoot(singleRoot.children);
          }
        }
      }
    } else {
      return;
    }
  }
  return singleRoot;
}
var getFunctionalFallthrough = (attrs) => {
  let res;
  for (const key in attrs) {
    if (key === "class" || key === "style" || isOn(key)) {
      (res || (res = {}))[key] = attrs[key];
    }
  }
  return res;
};
var filterModelListeners = (attrs, props) => {
  const res = {};
  for (const key in attrs) {
    if (!isModelListener(key) || !(key.slice(9) in props)) {
      res[key] = attrs[key];
    }
  }
  return res;
};
var isElementRoot = (vnode) => {
  return vnode.shapeFlag & (6 | 1) || vnode.type === Comment;
};
function shouldUpdateComponent(prevVNode, nextVNode, optimized) {
  const { props: prevProps, children: prevChildren, component: component8 } = prevVNode;
  const { props: nextProps, children: nextChildren, patchFlag } = nextVNode;
  const emits = component8.emitsOptions;
  if ((prevChildren || nextChildren) && isHmrUpdating) {
    return true;
  }
  if (nextVNode.dirs || nextVNode.transition) {
    return true;
  }
  if (optimized && patchFlag >= 0) {
    if (patchFlag & 1024) {
      return true;
    }
    if (patchFlag & 16) {
      if (!prevProps) {
        return !!nextProps;
      }
      return hasPropsChanged(prevProps, nextProps, emits);
    } else if (patchFlag & 8) {
      const dynamicProps = nextVNode.dynamicProps;
      for (let i = 0; i < dynamicProps.length; i++) {
        const key = dynamicProps[i];
        if (nextProps[key] !== prevProps[key] && !isEmitListener(emits, key)) {
          return true;
        }
      }
    }
  } else {
    if (prevChildren || nextChildren) {
      if (!nextChildren || !nextChildren.$stable) {
        return true;
      }
    }
    if (prevProps === nextProps) {
      return false;
    }
    if (!prevProps) {
      return !!nextProps;
    }
    if (!nextProps) {
      return true;
    }
    return hasPropsChanged(prevProps, nextProps, emits);
  }
  return false;
}
function hasPropsChanged(prevProps, nextProps, emitsOptions) {
  const nextKeys = Object.keys(nextProps);
  if (nextKeys.length !== Object.keys(prevProps).length) {
    return true;
  }
  for (let i = 0; i < nextKeys.length; i++) {
    const key = nextKeys[i];
    if (nextProps[key] !== prevProps[key] && !isEmitListener(emitsOptions, key)) {
      return true;
    }
  }
  return false;
}
function updateHOCHostEl({ vnode, parent }, el) {
  while (parent) {
    const root = parent.subTree;
    if (root.suspense && root.suspense.activeBranch === vnode) {
      root.el = vnode.el;
    }
    if (root === vnode) {
      (vnode = parent.vnode).el = el;
      parent = parent.parent;
    } else {
      break;
    }
  }
}
var isSuspense = (type) => type.__isSuspense;
function queueEffectWithSuspense(fn, suspense) {
  if (suspense && suspense.pendingBranch) {
    if (isArray(fn)) {
      suspense.effects.push(...fn);
    } else {
      suspense.effects.push(fn);
    }
  } else {
    queuePostFlushCb(fn);
  }
}
var Fragment = Symbol.for("v-fgt");
var Text = Symbol.for("v-txt");
var Comment = Symbol.for("v-cmt");
var Static = Symbol.for("v-stc");
var blockStack = [];
var currentBlock = null;
var isBlockTreeEnabled = 1;
function setBlockTracking(value, inVOnce = false) {
  isBlockTreeEnabled += value;
  if (value < 0 && currentBlock && inVOnce) {
    currentBlock.hasOnce = true;
  }
}
function isVNode(value) {
  return value ? value.__v_isVNode === true : false;
}
function isSameVNodeType(n1, n2) {
  if (n2.shapeFlag & 6 && n1.component) {
    const dirtyInstances = hmrDirtyComponents.get(n2.type);
    if (dirtyInstances && dirtyInstances.has(n1.component)) {
      n1.shapeFlag &= ~256;
      n2.shapeFlag &= ~512;
      return false;
    }
  }
  return n1.type === n2.type && n1.key === n2.key;
}
var vnodeArgsTransformer;
var createVNodeWithArgsTransform = (...args) => {
  return _createVNode(...vnodeArgsTransformer ? vnodeArgsTransformer(args, currentRenderingInstance) : args);
};
var normalizeKey = ({ key }) => key != null ? key : null;
var normalizeRef = ({ ref: ref2, ref_key, ref_for }) => {
  if (typeof ref2 === "number") {
    ref2 = "" + ref2;
  }
  return ref2 != null ? isString(ref2) || isRef(ref2) || isFunction(ref2) ? {
    i: currentRenderingInstance,
    r: ref2,
    k: ref_key,
    f: !!ref_for
  } : ref2 : null;
};
function createBaseVNode(type, props = null, children = null, patchFlag = 0, dynamicProps = null, shapeFlag = type === Fragment ? 0 : 1, isBlockNode = false, needFullChildrenNormalization = false) {
  const vnode = {
    __v_isVNode: true,
    __v_skip: true,
    type,
    props,
    key: props && normalizeKey(props),
    ref: props && normalizeRef(props),
    scopeId: currentScopeId,
    slotScopeIds: null,
    children,
    component: null,
    suspense: null,
    ssContent: null,
    ssFallback: null,
    dirs: null,
    transition: null,
    el: null,
    anchor: null,
    target: null,
    targetStart: null,
    targetAnchor: null,
    staticCount: 0,
    shapeFlag,
    patchFlag,
    dynamicProps,
    dynamicChildren: null,
    appContext: null,
    ctx: currentRenderingInstance
  };
  if (needFullChildrenNormalization) {
    normalizeChildren(vnode, children);
    if (shapeFlag & 128) {
      type.normalize(vnode);
    }
  } else if (children) {
    vnode.shapeFlag |= isString(children) ? 8 : 16;
  }
  if (vnode.key !== vnode.key) {
    warn$1(`VNode created with invalid key (NaN). VNode type:`, vnode.type);
  }
  if (isBlockTreeEnabled > 0 && // avoid a block node from tracking itself
  !isBlockNode && // has current parent block
  currentBlock && // presence of a patch flag indicates this node needs patching on updates.
  // component nodes also should always be patched, because even if the
  // component doesn't need to update, it needs to persist the instance on to
  // the next vnode so that it can be properly unmounted later.
  (vnode.patchFlag > 0 || shapeFlag & 6) && // the EVENTS flag is only for hydration and if it is the only flag, the
  // vnode should not be considered dynamic due to handler caching.
  vnode.patchFlag !== 32) {
    currentBlock.push(vnode);
  }
  return vnode;
}
var createVNode = true ? createVNodeWithArgsTransform : _createVNode;
function _createVNode(type, props = null, children = null, patchFlag = 0, dynamicProps = null, isBlockNode = false) {
  if (!type || type === NULL_DYNAMIC_COMPONENT) {
    if (!type) {
      warn$1(`Invalid vnode type when creating vnode: ${type}.`);
    }
    type = Comment;
  }
  if (isVNode(type)) {
    const cloned = cloneVNode(type, props, true);
    if (children) {
      normalizeChildren(cloned, children);
    }
    if (isBlockTreeEnabled > 0 && !isBlockNode && currentBlock) {
      if (cloned.shapeFlag & 6) {
        currentBlock[currentBlock.indexOf(type)] = cloned;
      } else {
        currentBlock.push(cloned);
      }
    }
    cloned.patchFlag = -2;
    return cloned;
  }
  if (isClassComponent(type)) {
    type = type.__vccOpts;
  }
  if (props) {
    props = guardReactiveProps(props);
    let { class: klass, style } = props;
    if (klass && !isString(klass)) {
      props.class = normalizeClass(klass);
    }
    if (isObject(style)) {
      if (isProxy(style) && !isArray(style)) {
        style = extend({}, style);
      }
      props.style = normalizeStyle(style);
    }
  }
  const shapeFlag = isString(type) ? 1 : isSuspense(type) ? 128 : isTeleport(type) ? 64 : isObject(type) ? 4 : isFunction(type) ? 2 : 0;
  if (shapeFlag & 4 && isProxy(type)) {
    type = toRaw(type);
    warn$1(`Vue received a Component that was made a reactive object. This can lead to unnecessary performance overhead and should be avoided by marking the component with \`markRaw\` or using \`shallowRef\` instead of \`ref\`.`, `
Component that was made reactive: `, type);
  }
  return createBaseVNode(type, props, children, patchFlag, dynamicProps, shapeFlag, isBlockNode, true);
}
function guardReactiveProps(props) {
  if (!props) return null;
  return isProxy(props) || isInternalObject(props) ? extend({}, props) : props;
}
function cloneVNode(vnode, extraProps, mergeRef = false, cloneTransition = false) {
  const { props, ref: ref2, patchFlag, children, transition } = vnode;
  const mergedProps = extraProps ? mergeProps(props || {}, extraProps) : props;
  const cloned = {
    __v_isVNode: true,
    __v_skip: true,
    type: vnode.type,
    props: mergedProps,
    key: mergedProps && normalizeKey(mergedProps),
    ref: extraProps && extraProps.ref ? (
      // #2078 in the case of <component :is="vnode" ref="extra"/>
      // if the vnode itself already has a ref, cloneVNode will need to merge
      // the refs so the single vnode can be set on multiple refs
      mergeRef && ref2 ? isArray(ref2) ? ref2.concat(normalizeRef(extraProps)) : [
        ref2,
        normalizeRef(extraProps)
      ] : normalizeRef(extraProps)
    ) : ref2,
    scopeId: vnode.scopeId,
    slotScopeIds: vnode.slotScopeIds,
    children: patchFlag === -1 && isArray(children) ? children.map(deepCloneVNode) : children,
    target: vnode.target,
    targetStart: vnode.targetStart,
    targetAnchor: vnode.targetAnchor,
    staticCount: vnode.staticCount,
    shapeFlag: vnode.shapeFlag,
    // if the vnode is cloned with extra props, we can no longer assume its
    // existing patch flag to be reliable and need to add the FULL_PROPS flag.
    // note: preserve flag for fragments since they use the flag for children
    // fast paths only.
    patchFlag: extraProps && vnode.type !== Fragment ? patchFlag === -1 ? 16 : patchFlag | 16 : patchFlag,
    dynamicProps: vnode.dynamicProps,
    dynamicChildren: vnode.dynamicChildren,
    appContext: vnode.appContext,
    dirs: vnode.dirs,
    transition,
    // These should technically only be non-null on mounted VNodes. However,
    // they *should* be copied for kept-alive vnodes. So we just always copy
    // them since them being non-null during a mount doesn't affect the logic as
    // they will simply be overwritten.
    component: vnode.component,
    suspense: vnode.suspense,
    ssContent: vnode.ssContent && cloneVNode(vnode.ssContent),
    ssFallback: vnode.ssFallback && cloneVNode(vnode.ssFallback),
    el: vnode.el,
    anchor: vnode.anchor,
    ctx: vnode.ctx,
    ce: vnode.ce
  };
  if (transition && cloneTransition) {
    setTransitionHooks(cloned, transition.clone(cloned));
  }
  return cloned;
}
function deepCloneVNode(vnode) {
  const cloned = cloneVNode(vnode);
  if (isArray(vnode.children)) {
    cloned.children = vnode.children.map(deepCloneVNode);
  }
  return cloned;
}
function createTextVNode(text = " ", flag = 0) {
  return createVNode(Text, null, text, flag);
}
function normalizeVNode(child) {
  if (child == null || typeof child === "boolean") {
    return createVNode(Comment);
  } else if (isArray(child)) {
    return createVNode(
      Fragment,
      null,
      // #3666, avoid reference pollution when reusing vnode
      child.slice()
    );
  } else if (isVNode(child)) {
    return cloneIfMounted(child);
  } else {
    return createVNode(Text, null, String(child));
  }
}
function cloneIfMounted(child) {
  return child.el === null && child.patchFlag !== -1 || child.memo ? child : cloneVNode(child);
}
function normalizeChildren(vnode, children) {
  let type = 0;
  const { shapeFlag } = vnode;
  if (children == null) {
    children = null;
  } else if (isArray(children)) {
    type = 16;
  } else if (typeof children === "object") {
    if (shapeFlag & (1 | 64)) {
      const slot = children.default;
      if (slot) {
        slot._c && (slot._d = false);
        normalizeChildren(vnode, slot());
        slot._c && (slot._d = true);
      }
      return;
    } else {
      type = 32;
      const slotFlag = children._;
      if (!slotFlag && !isInternalObject(children)) {
        children._ctx = currentRenderingInstance;
      } else if (slotFlag === 3 && currentRenderingInstance) {
        if (currentRenderingInstance.slots._ === 1) {
          children._ = 1;
        } else {
          children._ = 2;
          vnode.patchFlag |= 1024;
        }
      }
    }
  } else if (isFunction(children)) {
    children = {
      default: children,
      _ctx: currentRenderingInstance
    };
    type = 32;
  } else {
    children = String(children);
    if (shapeFlag & 64) {
      type = 16;
      children = [
        createTextVNode(children)
      ];
    } else {
      type = 8;
    }
  }
  vnode.children = children;
  vnode.shapeFlag |= type;
}
function mergeProps(...args) {
  const ret = {};
  for (let i = 0; i < args.length; i++) {
    const toMerge = args[i];
    for (const key in toMerge) {
      if (key === "class") {
        if (ret.class !== toMerge.class) {
          ret.class = normalizeClass([
            ret.class,
            toMerge.class
          ]);
        }
      } else if (key === "style") {
        ret.style = normalizeStyle([
          ret.style,
          toMerge.style
        ]);
      } else if (isOn(key)) {
        const existing = ret[key];
        const incoming = toMerge[key];
        if (incoming && existing !== incoming && !(isArray(existing) && existing.includes(incoming))) {
          ret[key] = existing ? [].concat(existing, incoming) : incoming;
        }
      } else if (key !== "") {
        ret[key] = toMerge[key];
      }
    }
  }
  return ret;
}
function invokeVNodeHook(hook2, instance, vnode, prevVNode = null) {
  callWithAsyncErrorHandling(hook2, instance, 7, [
    vnode,
    prevVNode
  ]);
}
var emptyAppContext = createAppContext();
var uid = 0;
function createComponentInstance(vnode, parent, suspense) {
  const type = vnode.type;
  const appContext = (parent ? parent.appContext : vnode.appContext) || emptyAppContext;
  const instance = {
    uid: uid++,
    vnode,
    type,
    parent,
    appContext,
    root: null,
    // to be immediately set
    next: null,
    subTree: null,
    // will be set synchronously right after creation
    effect: null,
    update: null,
    // will be set synchronously right after creation
    job: null,
    scope: new EffectScope(true),
    render: null,
    proxy: null,
    exposed: null,
    exposeProxy: null,
    withProxy: null,
    provides: parent ? parent.provides : Object.create(appContext.provides),
    ids: parent ? parent.ids : [
      "",
      0,
      0
    ],
    accessCache: null,
    renderCache: [],
    // local resolved assets
    components: null,
    directives: null,
    // resolved props and emits options
    propsOptions: normalizePropsOptions(type, appContext),
    emitsOptions: normalizeEmitsOptions(type, appContext),
    // emit
    emit: null,
    // to be set immediately
    emitted: null,
    // props default value
    propsDefaults: EMPTY_OBJ,
    // inheritAttrs
    inheritAttrs: type.inheritAttrs,
    // state
    ctx: EMPTY_OBJ,
    data: EMPTY_OBJ,
    props: EMPTY_OBJ,
    attrs: EMPTY_OBJ,
    slots: EMPTY_OBJ,
    refs: EMPTY_OBJ,
    setupState: EMPTY_OBJ,
    setupContext: null,
    // suspense related
    suspense,
    suspenseId: suspense ? suspense.pendingId : 0,
    asyncDep: null,
    asyncResolved: false,
    // lifecycle hooks
    // not using enums here because it results in computed properties
    isMounted: false,
    isUnmounted: false,
    isDeactivated: false,
    bc: null,
    c: null,
    bm: null,
    m: null,
    bu: null,
    u: null,
    um: null,
    bum: null,
    da: null,
    a: null,
    rtg: null,
    rtc: null,
    ec: null,
    sp: null
  };
  if (true) {
    instance.ctx = createDevRenderContext(instance);
  } else {
    instance.ctx = {
      _: instance
    };
  }
  instance.root = parent ? parent.root : instance;
  instance.emit = emit.bind(null, instance);
  if (vnode.ce) {
    vnode.ce(instance);
  }
  return instance;
}
var currentInstance = null;
var getCurrentInstance = () => currentInstance || currentRenderingInstance;
var internalSetCurrentInstance;
var setInSSRSetupState;
{
  const g = getGlobalThis();
  const registerGlobalSetter = (key, setter) => {
    let setters;
    if (!(setters = g[key])) setters = g[key] = [];
    setters.push(setter);
    return (v) => {
      if (setters.length > 1) setters.forEach((set) => set(v));
      else setters[0](v);
    };
  };
  internalSetCurrentInstance = registerGlobalSetter(`__VUE_INSTANCE_SETTERS__`, (v) => currentInstance = v);
  setInSSRSetupState = registerGlobalSetter(`__VUE_SSR_SETTERS__`, (v) => isInSSRComponentSetup = v);
}
var setCurrentInstance = (instance) => {
  const prev = currentInstance;
  internalSetCurrentInstance(instance);
  instance.scope.on();
  return () => {
    instance.scope.off();
    internalSetCurrentInstance(prev);
  };
};
var unsetCurrentInstance = () => {
  currentInstance && currentInstance.scope.off();
  internalSetCurrentInstance(null);
};
var isBuiltInTag = /* @__PURE__ */ makeMap("slot,component");
function validateComponentName(name, { isNativeTag }) {
  if (isBuiltInTag(name) || isNativeTag(name)) {
    warn$1("Do not use built-in or reserved HTML elements as component id: " + name);
  }
}
function isStatefulComponent(instance) {
  return instance.vnode.shapeFlag & 4;
}
var isInSSRComponentSetup = false;
function setupComponent(instance, isSSR = false, optimized = false) {
  isSSR && setInSSRSetupState(isSSR);
  const { props, children } = instance.vnode;
  const isStateful = isStatefulComponent(instance);
  initProps(instance, props, isStateful, isSSR);
  initSlots(instance, children, optimized);
  const setupResult = isStateful ? setupStatefulComponent(instance, isSSR) : void 0;
  isSSR && setInSSRSetupState(false);
  return setupResult;
}
function setupStatefulComponent(instance, isSSR) {
  var _a25;
  const Component = instance.type;
  if (true) {
    if (Component.name) {
      validateComponentName(Component.name, instance.appContext.config);
    }
    if (Component.components) {
      const names = Object.keys(Component.components);
      for (let i = 0; i < names.length; i++) {
        validateComponentName(names[i], instance.appContext.config);
      }
    }
    if (Component.directives) {
      const names = Object.keys(Component.directives);
      for (let i = 0; i < names.length; i++) {
        validateDirectiveName(names[i]);
      }
    }
    if (Component.compilerOptions && isRuntimeOnly()) {
      warn$1(`"compilerOptions" is only supported when using a build of Vue that includes the runtime compiler. Since you are using a runtime-only build, the options should be passed via your build tool config instead.`);
    }
  }
  instance.accessCache = /* @__PURE__ */ Object.create(null);
  instance.proxy = new Proxy(instance.ctx, PublicInstanceProxyHandlers);
  if (true) {
    exposePropsOnRenderContext(instance);
  }
  const { setup: setup8 } = Component;
  if (setup8) {
    pauseTracking();
    const setupContext = instance.setupContext = setup8.length > 1 ? createSetupContext(instance) : null;
    const reset = setCurrentInstance(instance);
    const setupResult = callWithErrorHandling(setup8, instance, 0, [
      true ? shallowReadonly(instance.props) : instance.props,
      setupContext
    ]);
    const isAsyncSetup = isPromise(setupResult);
    resetTracking();
    reset();
    if ((isAsyncSetup || instance.sp) && !isAsyncWrapper(instance)) {
      markAsyncBoundary(instance);
    }
    if (isAsyncSetup) {
      setupResult.then(unsetCurrentInstance, unsetCurrentInstance);
      if (isSSR) {
        return setupResult.then((resolvedResult) => {
          handleSetupResult(instance, resolvedResult, isSSR);
        }).catch((e) => {
          handleError(e, instance, 0);
        });
      } else {
        instance.asyncDep = setupResult;
        if (!instance.suspense) {
          const name = (_a25 = Component.name) != null ? _a25 : "Anonymous";
          warn$1(`Component <${name}>: setup function returned a promise, but no <Suspense> boundary was found in the parent component tree. A component with async setup() must be nested in a <Suspense> in order to be rendered.`);
        }
      }
    } else {
      handleSetupResult(instance, setupResult, isSSR);
    }
  } else {
    finishComponentSetup(instance, isSSR);
  }
}
function handleSetupResult(instance, setupResult, isSSR) {
  if (isFunction(setupResult)) {
    if (instance.type.__ssrInlineRender) {
      instance.ssrRender = setupResult;
    } else {
      instance.render = setupResult;
    }
  } else if (isObject(setupResult)) {
    if (isVNode(setupResult)) {
      warn$1(`setup() should not return VNodes directly - return a render function instead.`);
    }
    if (true) {
      instance.devtoolsRawSetupState = setupResult;
    }
    instance.setupState = proxyRefs(setupResult);
    if (true) {
      exposeSetupStateOnRenderContext(instance);
    }
  } else if (setupResult !== void 0) {
    warn$1(`setup() should return an object. Received: ${setupResult === null ? "null" : typeof setupResult}`);
  }
  finishComponentSetup(instance, isSSR);
}
var compile;
var installWithProxy;
var isRuntimeOnly = () => !compile;
function finishComponentSetup(instance, isSSR, skipOptions) {
  const Component = instance.type;
  if (!instance.render) {
    if (!isSSR && compile && !Component.render) {
      const template = Component.template || __VUE_OPTIONS_API__ && resolveMergedOptions(instance).template;
      if (template) {
        if (true) {
          startMeasure(instance, `compile`);
        }
        const { isCustomElement, compilerOptions } = instance.appContext.config;
        const { delimiters, compilerOptions: componentCompilerOptions } = Component;
        const finalCompilerOptions = extend(extend({
          isCustomElement,
          delimiters
        }, compilerOptions), componentCompilerOptions);
        Component.render = compile(template, finalCompilerOptions);
        if (true) {
          endMeasure(instance, `compile`);
        }
      }
    }
    instance.render = Component.render || NOOP;
    if (installWithProxy) {
      installWithProxy(instance);
    }
  }
  if (__VUE_OPTIONS_API__ && true) {
    const reset = setCurrentInstance(instance);
    pauseTracking();
    try {
      applyOptions(instance);
    } finally {
      resetTracking();
      reset();
    }
  }
  if (!Component.render && instance.render === NOOP && !isSSR) {
    if (!compile && Component.template) {
      warn$1(`Component provided template option but runtime compilation is not supported in this build of Vue. Configure your bundler to alias "vue" to "vue/dist/vue.esm-bundler.js".`);
    } else {
      warn$1(`Component is missing template or render function: `, Component);
    }
  }
}
var attrsProxyHandlers = true ? {
  get(target2, key) {
    markAttrsAccessed();
    track(target2, "get", "");
    return target2[key];
  },
  set() {
    warn$1(`setupContext.attrs is readonly.`);
    return false;
  },
  deleteProperty() {
    warn$1(`setupContext.attrs is readonly.`);
    return false;
  }
} : {
  get(target2, key) {
    track(target2, "get", "");
    return target2[key];
  }
};
function getSlotsProxy(instance) {
  return new Proxy(instance.slots, {
    get(target2, key) {
      track(instance, "get", "$slots");
      return target2[key];
    }
  });
}
function createSetupContext(instance) {
  const expose = (exposed) => {
    if (true) {
      if (instance.exposed) {
        warn$1(`expose() should be called only once per setup().`);
      }
      if (exposed != null) {
        let exposedType = typeof exposed;
        if (exposedType === "object") {
          if (isArray(exposed)) {
            exposedType = "array";
          } else if (isRef(exposed)) {
            exposedType = "ref";
          }
        }
        if (exposedType !== "object") {
          warn$1(`expose() should be passed a plain object, received ${exposedType}.`);
        }
      }
    }
    instance.exposed = exposed || {};
  };
  if (true) {
    let attrsProxy;
    let slotsProxy;
    return Object.freeze({
      get attrs() {
        return attrsProxy || (attrsProxy = new Proxy(instance.attrs, attrsProxyHandlers));
      },
      get slots() {
        return slotsProxy || (slotsProxy = getSlotsProxy(instance));
      },
      get emit() {
        return (event, ...args) => instance.emit(event, ...args);
      },
      expose
    });
  } else {
    return {
      attrs: new Proxy(instance.attrs, attrsProxyHandlers),
      slots: instance.slots,
      emit: instance.emit,
      expose
    };
  }
}
function getComponentPublicInstance(instance) {
  if (instance.exposed) {
    return instance.exposeProxy || (instance.exposeProxy = new Proxy(proxyRefs(markRaw(instance.exposed)), {
      get(target2, key) {
        if (key in target2) {
          return target2[key];
        } else if (key in publicPropertiesMap) {
          return publicPropertiesMap[key](instance);
        }
      },
      has(target2, key) {
        return key in target2 || key in publicPropertiesMap;
      }
    }));
  } else {
    return instance.proxy;
  }
}
var classifyRE = /(?:^|[-_])(\w)/g;
var classify = (str) => str.replace(classifyRE, (c) => c.toUpperCase()).replace(/[-_]/g, "");
function getComponentName(Component, includeInferred = true) {
  return isFunction(Component) ? Component.displayName || Component.name : Component.name || includeInferred && Component.__name;
}
function formatComponentName(instance, Component, isRoot = false) {
  let name = getComponentName(Component);
  if (!name && Component.__file) {
    const match = Component.__file.match(/([^/\\]+)\.\w+$/);
    if (match) {
      name = match[1];
    }
  }
  if (!name && instance && instance.parent) {
    const inferFromRegistry = (registry) => {
      for (const key in registry) {
        if (registry[key] === Component) {
          return key;
        }
      }
    };
    name = inferFromRegistry(instance.components || instance.parent.type.components) || inferFromRegistry(instance.appContext.components);
  }
  return name ? classify(name) : isRoot ? `App` : `Anonymous`;
}
function isClassComponent(value) {
  return isFunction(value) && "__vccOpts" in value;
}
var computed2 = (getterOrOptions, debugOptions) => {
  const c = computed(getterOrOptions, debugOptions, isInSSRComponentSetup);
  if (true) {
    const i = getCurrentInstance();
    if (i && i.appContext.config.warnRecursiveComputed) {
      c._warnRecursive = true;
    }
  }
  return c;
};
function h(type, propsOrChildren, children) {
  const l = arguments.length;
  if (l === 2) {
    if (isObject(propsOrChildren) && !isArray(propsOrChildren)) {
      if (isVNode(propsOrChildren)) {
        return createVNode(type, null, [
          propsOrChildren
        ]);
      }
      return createVNode(type, propsOrChildren);
    } else {
      return createVNode(type, null, propsOrChildren);
    }
  } else {
    if (l > 3) {
      children = Array.prototype.slice.call(arguments, 2);
    } else if (l === 3 && isVNode(children)) {
      children = [
        children
      ];
    }
    return createVNode(type, propsOrChildren, children);
  }
}
function initCustomFormatter() {
  if (typeof window === "undefined") {
    return;
  }
  const vueStyle = {
    style: "color:#3ba776"
  };
  const numberStyle = {
    style: "color:#1677ff"
  };
  const stringStyle = {
    style: "color:#f5222d"
  };
  const keywordStyle = {
    style: "color:#eb2f96"
  };
  const formatter = {
    __vue_custom_formatter: true,
    header(obj) {
      if (!isObject(obj)) {
        return null;
      }
      if (obj.__isVue) {
        return [
          "div",
          vueStyle,
          `VueInstance`
        ];
      } else if (isRef(obj)) {
        return [
          "div",
          {},
          [
            "span",
            vueStyle,
            genRefFlag(obj)
          ],
          "<",
          // avoid debugger accessing value affecting behavior
          formatValue("_value" in obj ? obj._value : obj),
          `>`
        ];
      } else if (isReactive(obj)) {
        return [
          "div",
          {},
          [
            "span",
            vueStyle,
            isShallow(obj) ? "ShallowReactive" : "Reactive"
          ],
          "<",
          formatValue(obj),
          `>${isReadonly(obj) ? ` (readonly)` : ``}`
        ];
      } else if (isReadonly(obj)) {
        return [
          "div",
          {},
          [
            "span",
            vueStyle,
            isShallow(obj) ? "ShallowReadonly" : "Readonly"
          ],
          "<",
          formatValue(obj),
          ">"
        ];
      }
      return null;
    },
    hasBody(obj) {
      return obj && obj.__isVue;
    },
    body(obj) {
      if (obj && obj.__isVue) {
        return [
          "div",
          {},
          ...formatInstance(obj.$)
        ];
      }
    }
  };
  function formatInstance(instance) {
    const blocks = [];
    if (instance.type.props && instance.props) {
      blocks.push(createInstanceBlock("props", toRaw(instance.props)));
    }
    if (instance.setupState !== EMPTY_OBJ) {
      blocks.push(createInstanceBlock("setup", instance.setupState));
    }
    if (instance.data !== EMPTY_OBJ) {
      blocks.push(createInstanceBlock("data", toRaw(instance.data)));
    }
    const computed3 = extractKeys(instance, "computed");
    if (computed3) {
      blocks.push(createInstanceBlock("computed", computed3));
    }
    const injected = extractKeys(instance, "inject");
    if (injected) {
      blocks.push(createInstanceBlock("injected", injected));
    }
    blocks.push([
      "div",
      {},
      [
        "span",
        {
          style: keywordStyle.style + ";opacity:0.66"
        },
        "$ (internal): "
      ],
      [
        "object",
        {
          object: instance
        }
      ]
    ]);
    return blocks;
  }
  function createInstanceBlock(type, target2) {
    target2 = extend({}, target2);
    if (!Object.keys(target2).length) {
      return [
        "span",
        {}
      ];
    }
    return [
      "div",
      {
        style: "line-height:1.25em;margin-bottom:0.6em"
      },
      [
        "div",
        {
          style: "color:#476582"
        },
        type
      ],
      [
        "div",
        {
          style: "padding-left:1.25em"
        },
        ...Object.keys(target2).map((key) => {
          return [
            "div",
            {},
            [
              "span",
              keywordStyle,
              key + ": "
            ],
            formatValue(target2[key], false)
          ];
        })
      ]
    ];
  }
  function formatValue(v, asRaw = true) {
    if (typeof v === "number") {
      return [
        "span",
        numberStyle,
        v
      ];
    } else if (typeof v === "string") {
      return [
        "span",
        stringStyle,
        JSON.stringify(v)
      ];
    } else if (typeof v === "boolean") {
      return [
        "span",
        keywordStyle,
        v
      ];
    } else if (isObject(v)) {
      return [
        "object",
        {
          object: asRaw ? toRaw(v) : v
        }
      ];
    } else {
      return [
        "span",
        stringStyle,
        String(v)
      ];
    }
  }
  function extractKeys(instance, type) {
    const Comp = instance.type;
    if (isFunction(Comp)) {
      return;
    }
    const extracted = {};
    for (const key in instance.ctx) {
      if (isKeyOfType(Comp, key, type)) {
        extracted[key] = instance.ctx[key];
      }
    }
    return extracted;
  }
  function isKeyOfType(Comp, key, type) {
    const opts = Comp[type];
    if (isArray(opts) && opts.includes(key) || isObject(opts) && key in opts) {
      return true;
    }
    if (Comp.extends && isKeyOfType(Comp.extends, key, type)) {
      return true;
    }
    if (Comp.mixins && Comp.mixins.some((m) => isKeyOfType(m, key, type))) {
      return true;
    }
  }
  function genRefFlag(v) {
    if (isShallow(v)) {
      return `ShallowRef`;
    }
    if (v.effect) {
      return `ComputedRef`;
    }
    return `Ref`;
  }
  if (window.devtoolsFormatters) {
    window.devtoolsFormatters.push(formatter);
  } else {
    window.devtoolsFormatters = [
      formatter
    ];
  }
}
var version = "3.5.13";
var warn2 = true ? warn$1 : NOOP;

// node_modules/.deno/@vue+runtime-dom@3.5.13/node_modules/@vue/runtime-dom/dist/runtime-dom.esm-bundler.js
var policy = void 0;
var tt = typeof window !== "undefined" && window.trustedTypes;
if (tt) {
  try {
    policy = /* @__PURE__ */ tt.createPolicy("vue", {
      createHTML: (val) => val
    });
  } catch (e) {
    warn2(`Error creating trusted types policy: ${e}`);
  }
}
var unsafeToTrustedHTML = policy ? (val) => policy.createHTML(val) : (val) => val;
var svgNS = "http://www.w3.org/2000/svg";
var mathmlNS = "http://www.w3.org/1998/Math/MathML";
var doc = typeof document !== "undefined" ? document : null;
var templateContainer = doc && /* @__PURE__ */ doc.createElement("template");
var nodeOps = {
  insert: (child, parent, anchor) => {
    parent.insertBefore(child, anchor || null);
  },
  remove: (child) => {
    const parent = child.parentNode;
    if (parent) {
      parent.removeChild(child);
    }
  },
  createElement: (tag, namespace, is, props) => {
    const el = namespace === "svg" ? doc.createElementNS(svgNS, tag) : namespace === "mathml" ? doc.createElementNS(mathmlNS, tag) : is ? doc.createElement(tag, {
      is
    }) : doc.createElement(tag);
    if (tag === "select" && props && props.multiple != null) {
      el.setAttribute("multiple", props.multiple);
    }
    return el;
  },
  createText: (text) => doc.createTextNode(text),
  createComment: (text) => doc.createComment(text),
  setText: (node, text) => {
    node.nodeValue = text;
  },
  setElementText: (el, text) => {
    el.textContent = text;
  },
  parentNode: (node) => node.parentNode,
  nextSibling: (node) => node.nextSibling,
  querySelector: (selector) => doc.querySelector(selector),
  setScopeId(el, id) {
    el.setAttribute(id, "");
  },
  // __UNSAFE__
  // Reason: innerHTML.
  // Static content here can only come from compiled templates.
  // As long as the user only uses trusted templates, this is safe.
  insertStaticContent(content, parent, anchor, namespace, start, end) {
    const before = anchor ? anchor.previousSibling : parent.lastChild;
    if (start && (start === end || start.nextSibling)) {
      while (true) {
        parent.insertBefore(start.cloneNode(true), anchor);
        if (start === end || !(start = start.nextSibling)) break;
      }
    } else {
      templateContainer.innerHTML = unsafeToTrustedHTML(namespace === "svg" ? `<svg>${content}</svg>` : namespace === "mathml" ? `<math>${content}</math>` : content);
      const template = templateContainer.content;
      if (namespace === "svg" || namespace === "mathml") {
        const wrapper = template.firstChild;
        while (wrapper.firstChild) {
          template.appendChild(wrapper.firstChild);
        }
        template.removeChild(wrapper);
      }
      parent.insertBefore(template, anchor);
    }
    return [
      // first
      before ? before.nextSibling : parent.firstChild,
      // last
      anchor ? anchor.previousSibling : parent.lastChild
    ];
  }
};
var vtcKey = Symbol("_vtc");
function patchClass(el, value, isSVG) {
  const transitionClasses = el[vtcKey];
  if (transitionClasses) {
    value = (value ? [
      value,
      ...transitionClasses
    ] : [
      ...transitionClasses
    ]).join(" ");
  }
  if (value == null) {
    el.removeAttribute("class");
  } else if (isSVG) {
    el.setAttribute("class", value);
  } else {
    el.className = value;
  }
}
var vShowOriginalDisplay = Symbol("_vod");
var vShowHidden = Symbol("_vsh");
var vShow = {
  beforeMount(el, { value }, { transition }) {
    el[vShowOriginalDisplay] = el.style.display === "none" ? "" : el.style.display;
    if (transition && value) {
      transition.beforeEnter(el);
    } else {
      setDisplay(el, value);
    }
  },
  mounted(el, { value }, { transition }) {
    if (transition && value) {
      transition.enter(el);
    }
  },
  updated(el, { value, oldValue }, { transition }) {
    if (!value === !oldValue) return;
    if (transition) {
      if (value) {
        transition.beforeEnter(el);
        setDisplay(el, true);
        transition.enter(el);
      } else {
        transition.leave(el, () => {
          setDisplay(el, false);
        });
      }
    } else {
      setDisplay(el, value);
    }
  },
  beforeUnmount(el, { value }) {
    setDisplay(el, value);
  }
};
if (true) {
  vShow.name = "show";
}
function setDisplay(el, value) {
  el.style.display = value ? el[vShowOriginalDisplay] : "none";
  el[vShowHidden] = !value;
}
var CSS_VAR_TEXT = Symbol(true ? "CSS_VAR_TEXT" : "");
var displayRE = /(^|;)\s*display\s*:/;
function patchStyle(el, prev, next) {
  const style = el.style;
  const isCssString = isString(next);
  let hasControlledDisplay = false;
  if (next && !isCssString) {
    if (prev) {
      if (!isString(prev)) {
        for (const key in prev) {
          if (next[key] == null) {
            setStyle(style, key, "");
          }
        }
      } else {
        for (const prevStyle of prev.split(";")) {
          const key = prevStyle.slice(0, prevStyle.indexOf(":")).trim();
          if (next[key] == null) {
            setStyle(style, key, "");
          }
        }
      }
    }
    for (const key in next) {
      if (key === "display") {
        hasControlledDisplay = true;
      }
      setStyle(style, key, next[key]);
    }
  } else {
    if (isCssString) {
      if (prev !== next) {
        const cssVarText = style[CSS_VAR_TEXT];
        if (cssVarText) {
          next += ";" + cssVarText;
        }
        style.cssText = next;
        hasControlledDisplay = displayRE.test(next);
      }
    } else if (prev) {
      el.removeAttribute("style");
    }
  }
  if (vShowOriginalDisplay in el) {
    el[vShowOriginalDisplay] = hasControlledDisplay ? style.display : "";
    if (el[vShowHidden]) {
      style.display = "none";
    }
  }
}
var semicolonRE = /[^\\];\s*$/;
var importantRE = /\s*!important$/;
function setStyle(style, name, val) {
  if (isArray(val)) {
    val.forEach((v) => setStyle(style, name, v));
  } else {
    if (val == null) val = "";
    if (true) {
      if (semicolonRE.test(val)) {
        warn2(`Unexpected semicolon at the end of '${name}' style value: '${val}'`);
      }
    }
    if (name.startsWith("--")) {
      style.setProperty(name, val);
    } else {
      const prefixed = autoPrefix(style, name);
      if (importantRE.test(val)) {
        style.setProperty(hyphenate(prefixed), val.replace(importantRE, ""), "important");
      } else {
        style[prefixed] = val;
      }
    }
  }
}
var prefixes = [
  "Webkit",
  "Moz",
  "ms"
];
var prefixCache = {};
function autoPrefix(style, rawName) {
  const cached = prefixCache[rawName];
  if (cached) {
    return cached;
  }
  let name = camelize(rawName);
  if (name !== "filter" && name in style) {
    return prefixCache[rawName] = name;
  }
  name = capitalize(name);
  for (let i = 0; i < prefixes.length; i++) {
    const prefixed = prefixes[i] + name;
    if (prefixed in style) {
      return prefixCache[rawName] = prefixed;
    }
  }
  return rawName;
}
var xlinkNS = "http://www.w3.org/1999/xlink";
function patchAttr(el, key, value, isSVG, instance, isBoolean3 = isSpecialBooleanAttr(key)) {
  if (isSVG && key.startsWith("xlink:")) {
    if (value == null) {
      el.removeAttributeNS(xlinkNS, key.slice(6, key.length));
    } else {
      el.setAttributeNS(xlinkNS, key, value);
    }
  } else {
    if (value == null || isBoolean3 && !includeBooleanAttr(value)) {
      el.removeAttribute(key);
    } else {
      el.setAttribute(key, isBoolean3 ? "" : isSymbol(value) ? String(value) : value);
    }
  }
}
function patchDOMProp(el, key, value, parentComponent, attrName) {
  if (key === "innerHTML" || key === "textContent") {
    if (value != null) {
      el[key] = key === "innerHTML" ? unsafeToTrustedHTML(value) : value;
    }
    return;
  }
  const tag = el.tagName;
  if (key === "value" && tag !== "PROGRESS" && // custom elements may use _value internally
  !tag.includes("-")) {
    const oldValue = tag === "OPTION" ? el.getAttribute("value") || "" : el.value;
    const newValue = value == null ? (
      // #11647: value should be set as empty string for null and undefined,
      // but <input type="checkbox"> should be set as 'on'.
      el.type === "checkbox" ? "on" : ""
    ) : String(value);
    if (oldValue !== newValue || !("_value" in el)) {
      el.value = newValue;
    }
    if (value == null) {
      el.removeAttribute(key);
    }
    el._value = value;
    return;
  }
  let needRemove = false;
  if (value === "" || value == null) {
    const type = typeof el[key];
    if (type === "boolean") {
      value = includeBooleanAttr(value);
    } else if (value == null && type === "string") {
      value = "";
      needRemove = true;
    } else if (type === "number") {
      value = 0;
      needRemove = true;
    }
  }
  try {
    el[key] = value;
  } catch (e) {
    if (!needRemove) {
      warn2(`Failed setting prop "${key}" on <${tag.toLowerCase()}>: value ${value} is invalid.`, e);
    }
  }
  needRemove && el.removeAttribute(attrName || key);
}
function addEventListener(el, event, handler, options) {
  el.addEventListener(event, handler, options);
}
function removeEventListener(el, event, handler, options) {
  el.removeEventListener(event, handler, options);
}
var veiKey = Symbol("_vei");
function patchEvent(el, rawName, prevValue, nextValue, instance = null) {
  const invokers = el[veiKey] || (el[veiKey] = {});
  const existingInvoker = invokers[rawName];
  if (nextValue && existingInvoker) {
    existingInvoker.value = true ? sanitizeEventValue(nextValue, rawName) : nextValue;
  } else {
    const [name, options] = parseName(rawName);
    if (nextValue) {
      const invoker = invokers[rawName] = createInvoker(true ? sanitizeEventValue(nextValue, rawName) : nextValue, instance);
      addEventListener(el, name, invoker, options);
    } else if (existingInvoker) {
      removeEventListener(el, name, existingInvoker, options);
      invokers[rawName] = void 0;
    }
  }
}
var optionsModifierRE = /(?:Once|Passive|Capture)$/;
function parseName(name) {
  let options;
  if (optionsModifierRE.test(name)) {
    options = {};
    let m;
    while (m = name.match(optionsModifierRE)) {
      name = name.slice(0, name.length - m[0].length);
      options[m[0].toLowerCase()] = true;
    }
  }
  const event = name[2] === ":" ? name.slice(3) : hyphenate(name.slice(2));
  return [
    event,
    options
  ];
}
var cachedNow = 0;
var p = /* @__PURE__ */ Promise.resolve();
var getNow = () => cachedNow || (p.then(() => cachedNow = 0), cachedNow = Date.now());
function createInvoker(initialValue, instance) {
  const invoker = (e) => {
    if (!e._vts) {
      e._vts = Date.now();
    } else if (e._vts <= invoker.attached) {
      return;
    }
    callWithAsyncErrorHandling(patchStopImmediatePropagation(e, invoker.value), instance, 5, [
      e
    ]);
  };
  invoker.value = initialValue;
  invoker.attached = getNow();
  return invoker;
}
function sanitizeEventValue(value, propName) {
  if (isFunction(value) || isArray(value)) {
    return value;
  }
  warn2(`Wrong type passed as event handler to ${propName} - did you forget @ or : in front of your prop?
Expected function or array of functions, received type ${typeof value}.`);
  return NOOP;
}
function patchStopImmediatePropagation(e, value) {
  if (isArray(value)) {
    const originalStop = e.stopImmediatePropagation;
    e.stopImmediatePropagation = () => {
      originalStop.call(e);
      e._stopped = true;
    };
    return value.map((fn) => (e2) => !e2._stopped && fn && fn(e2));
  } else {
    return value;
  }
}
var isNativeOn = (key) => key.charCodeAt(0) === 111 && key.charCodeAt(1) === 110 && // lowercase letter
key.charCodeAt(2) > 96 && key.charCodeAt(2) < 123;
var patchProp = (el, key, prevValue, nextValue, namespace, parentComponent) => {
  const isSVG = namespace === "svg";
  if (key === "class") {
    patchClass(el, nextValue, isSVG);
  } else if (key === "style") {
    patchStyle(el, prevValue, nextValue);
  } else if (isOn(key)) {
    if (!isModelListener(key)) {
      patchEvent(el, key, prevValue, nextValue, parentComponent);
    }
  } else if (key[0] === "." ? (key = key.slice(1), true) : key[0] === "^" ? (key = key.slice(1), false) : shouldSetAsProp(el, key, nextValue, isSVG)) {
    patchDOMProp(el, key, nextValue);
    if (!el.tagName.includes("-") && (key === "value" || key === "checked" || key === "selected")) {
      patchAttr(el, key, nextValue, isSVG, parentComponent, key !== "value");
    }
  } else if (
    // #11081 force set props for possible async custom element
    el._isVueCE && (/[A-Z]/.test(key) || !isString(nextValue))
  ) {
    patchDOMProp(el, camelize(key), nextValue, parentComponent, key);
  } else {
    if (key === "true-value") {
      el._trueValue = nextValue;
    } else if (key === "false-value") {
      el._falseValue = nextValue;
    }
    patchAttr(el, key, nextValue, isSVG);
  }
};
function shouldSetAsProp(el, key, value, isSVG) {
  if (isSVG) {
    if (key === "innerHTML" || key === "textContent") {
      return true;
    }
    if (key in el && isNativeOn(key) && isFunction(value)) {
      return true;
    }
    return false;
  }
  if (key === "spellcheck" || key === "draggable" || key === "translate") {
    return false;
  }
  if (key === "form") {
    return false;
  }
  if (key === "list" && el.tagName === "INPUT") {
    return false;
  }
  if (key === "type" && el.tagName === "TEXTAREA") {
    return false;
  }
  if (key === "width" || key === "height") {
    const tag = el.tagName;
    if (tag === "IMG" || tag === "VIDEO" || tag === "CANVAS" || tag === "SOURCE") {
      return false;
    }
  }
  if (isNativeOn(key) && isString(value)) {
    return false;
  }
  return key in el;
}
var moveCbKey = Symbol("_moveCb");
var enterCbKey2 = Symbol("_enterCb");
var assignKey = Symbol("_assign");
var rendererOptions = /* @__PURE__ */ extend({
  patchProp
}, nodeOps);
var renderer;
function ensureRenderer() {
  return renderer || (renderer = createRenderer(rendererOptions));
}
var createApp = (...args) => {
  const app = ensureRenderer().createApp(...args);
  if (true) {
    injectNativeTagCheck(app);
    injectCompilerOptionsCheck(app);
  }
  const { mount } = app;
  app.mount = (containerOrSelector) => {
    const container = normalizeContainer(containerOrSelector);
    if (!container) return;
    const component8 = app._component;
    if (!isFunction(component8) && !component8.render && !component8.template) {
      component8.template = container.innerHTML;
    }
    if (container.nodeType === 1) {
      container.textContent = "";
    }
    const proxy = mount(container, false, resolveRootNamespace(container));
    if (container instanceof Element) {
      container.removeAttribute("v-cloak");
      container.setAttribute("data-v-app", "");
    }
    return proxy;
  };
  return app;
};
function resolveRootNamespace(container) {
  if (container instanceof SVGElement) {
    return "svg";
  }
  if (typeof MathMLElement === "function" && container instanceof MathMLElement) {
    return "mathml";
  }
}
function injectNativeTagCheck(app) {
  Object.defineProperty(app.config, "isNativeTag", {
    value: (tag) => isHTMLTag(tag) || isSVGTag(tag) || isMathMLTag(tag),
    writable: false
  });
}
function injectCompilerOptionsCheck(app) {
  if (isRuntimeOnly()) {
    const isCustomElement = app.config.isCustomElement;
    Object.defineProperty(app.config, "isCustomElement", {
      get() {
        return isCustomElement;
      },
      set() {
        warn2(`The \`isCustomElement\` config option is deprecated. Use \`compilerOptions.isCustomElement\` instead.`);
      }
    });
    const compilerOptions = app.config.compilerOptions;
    const msg = `The \`compilerOptions\` config option is only respected when using a build of Vue.js that includes the runtime compiler (aka "full build"). Since you are using the runtime-only build, \`compilerOptions\` must be passed to \`@vue/compiler-dom\` in the build setup instead.
- For vue-loader: pass it via vue-loader's \`compilerOptions\` loader option.
- For vue-cli: see https://cli.vuejs.org/guide/webpack.html#modifying-options-of-a-loader
- For vite: pass it via @vitejs/plugin-vue options. See https://github.com/vitejs/vite-plugin-vue/tree/main/packages/plugin-vue#example-for-passing-options-to-vuecompiler-sfc`;
    Object.defineProperty(app.config, "compilerOptions", {
      get() {
        warn2(msg);
        return compilerOptions;
      },
      set() {
        warn2(msg);
      }
    });
  }
}
function normalizeContainer(container) {
  if (isString(container)) {
    const res = document.querySelector(container);
    if (!res) {
      warn2(`Failed to mount app: mount target selector "${container}" returned null.`);
    }
    return res;
  }
  if (window.ShadowRoot && container instanceof window.ShadowRoot && container.mode === "closed") {
    warn2(`mounting on a ShadowRoot with \`{mode: "closed"}\` may lead to unpredictable bugs`);
  }
  return container;
}

// node_modules/.deno/vue@3.5.13/node_modules/vue/dist/vue.runtime.esm-bundler.js
function initDev() {
  {
    initCustomFormatter();
  }
}
if (true) {
  initDev();
}

// node_modules/.deno/@vue+devtools-shared@7.7.9/node_modules/@vue/devtools-shared/dist/index.js
var __create = Object.create;
var __defProp = Object.defineProperty;
var __getOwnPropDesc = Object.getOwnPropertyDescriptor;
var __getOwnPropNames = Object.getOwnPropertyNames;
var __getProtoOf = Object.getPrototypeOf;
var __hasOwnProp = Object.prototype.hasOwnProperty;
var __esm = (fn, res) => function __init() {
  return fn && (res = (0, fn[__getOwnPropNames(fn)[0]])(fn = 0)), res;
};
var __commonJS = (cb, mod) => function __require() {
  return mod || (0, cb[__getOwnPropNames(cb)[0]])((mod = {
    exports: {}
  }).exports, mod), mod.exports;
};
var __copyProps = (to, from, except, desc) => {
  if (from && typeof from === "object" || typeof from === "function") {
    for (let key of __getOwnPropNames(from)) if (!__hasOwnProp.call(to, key) && key !== except) __defProp(to, key, {
      get: () => from[key],
      enumerable: !(desc = __getOwnPropDesc(from, key)) || desc.enumerable
    });
  }
  return to;
};
var __toESM = (mod, isNodeMode, target2) => (target2 = mod != null ? __create(__getProtoOf(mod)) : {}, __copyProps(
  // If the importer is in node compatibility mode or this is not an ESM
  // file that has been converted to a CommonJS file using a Babel-
  // compatible transform (i.e. "__esModule" has not been set), then set
  // "default" to the CommonJS "module.exports" for node compatibility.
  1 ? __defProp(target2, "default", {
    value: mod,
    enumerable: true
  }) : target2,
  mod
));
var init_esm_shims = __esm({
  "../../node_modules/.pnpm/tsup@8.4.0_@microsoft+api-extractor@7.51.1_@types+node@22.13.14__jiti@2.4.2_postcss@8.5_96eb05a9d65343021e53791dd83f3773/node_modules/tsup/assets/esm_shims.js"() {
    "use strict";
  }
});
var require_rfdc = __commonJS({
  "../../node_modules/.pnpm/rfdc@1.4.1/node_modules/rfdc/index.js"(exports, module) {
    "use strict";
    init_esm_shims();
    module.exports = rfdc2;
    function copyBuffer(cur) {
      if (cur instanceof Buffer) {
        return Buffer.from(cur);
      }
      return new cur.constructor(cur.buffer.slice(), cur.byteOffset, cur.length);
    }
    function rfdc2(opts) {
      opts = opts || {};
      if (opts.circles) return rfdcCircles(opts);
      const constructorHandlers = /* @__PURE__ */ new Map();
      constructorHandlers.set(Date, (o) => new Date(o));
      constructorHandlers.set(Map, (o, fn) => new Map(cloneArray(Array.from(o), fn)));
      constructorHandlers.set(Set, (o, fn) => new Set(cloneArray(Array.from(o), fn)));
      if (opts.constructorHandlers) {
        for (const handler2 of opts.constructorHandlers) {
          constructorHandlers.set(handler2[0], handler2[1]);
        }
      }
      let handler = null;
      return opts.proto ? cloneProto : clone;
      function cloneArray(a, fn) {
        const keys = Object.keys(a);
        const a2 = new Array(keys.length);
        for (let i = 0; i < keys.length; i++) {
          const k = keys[i];
          const cur = a[k];
          if (typeof cur !== "object" || cur === null) {
            a2[k] = cur;
          } else if (cur.constructor !== Object && (handler = constructorHandlers.get(cur.constructor))) {
            a2[k] = handler(cur, fn);
          } else if (ArrayBuffer.isView(cur)) {
            a2[k] = copyBuffer(cur);
          } else {
            a2[k] = fn(cur);
          }
        }
        return a2;
      }
      function clone(o) {
        if (typeof o !== "object" || o === null) return o;
        if (Array.isArray(o)) return cloneArray(o, clone);
        if (o.constructor !== Object && (handler = constructorHandlers.get(o.constructor))) {
          return handler(o, clone);
        }
        const o2 = {};
        for (const k in o) {
          if (Object.hasOwnProperty.call(o, k) === false) continue;
          const cur = o[k];
          if (typeof cur !== "object" || cur === null) {
            o2[k] = cur;
          } else if (cur.constructor !== Object && (handler = constructorHandlers.get(cur.constructor))) {
            o2[k] = handler(cur, clone);
          } else if (ArrayBuffer.isView(cur)) {
            o2[k] = copyBuffer(cur);
          } else {
            o2[k] = clone(cur);
          }
        }
        return o2;
      }
      function cloneProto(o) {
        if (typeof o !== "object" || o === null) return o;
        if (Array.isArray(o)) return cloneArray(o, cloneProto);
        if (o.constructor !== Object && (handler = constructorHandlers.get(o.constructor))) {
          return handler(o, cloneProto);
        }
        const o2 = {};
        for (const k in o) {
          const cur = o[k];
          if (typeof cur !== "object" || cur === null) {
            o2[k] = cur;
          } else if (cur.constructor !== Object && (handler = constructorHandlers.get(cur.constructor))) {
            o2[k] = handler(cur, cloneProto);
          } else if (ArrayBuffer.isView(cur)) {
            o2[k] = copyBuffer(cur);
          } else {
            o2[k] = cloneProto(cur);
          }
        }
        return o2;
      }
    }
    function rfdcCircles(opts) {
      const refs = [];
      const refsNew = [];
      const constructorHandlers = /* @__PURE__ */ new Map();
      constructorHandlers.set(Date, (o) => new Date(o));
      constructorHandlers.set(Map, (o, fn) => new Map(cloneArray(Array.from(o), fn)));
      constructorHandlers.set(Set, (o, fn) => new Set(cloneArray(Array.from(o), fn)));
      if (opts.constructorHandlers) {
        for (const handler2 of opts.constructorHandlers) {
          constructorHandlers.set(handler2[0], handler2[1]);
        }
      }
      let handler = null;
      return opts.proto ? cloneProto : clone;
      function cloneArray(a, fn) {
        const keys = Object.keys(a);
        const a2 = new Array(keys.length);
        for (let i = 0; i < keys.length; i++) {
          const k = keys[i];
          const cur = a[k];
          if (typeof cur !== "object" || cur === null) {
            a2[k] = cur;
          } else if (cur.constructor !== Object && (handler = constructorHandlers.get(cur.constructor))) {
            a2[k] = handler(cur, fn);
          } else if (ArrayBuffer.isView(cur)) {
            a2[k] = copyBuffer(cur);
          } else {
            const index = refs.indexOf(cur);
            if (index !== -1) {
              a2[k] = refsNew[index];
            } else {
              a2[k] = fn(cur);
            }
          }
        }
        return a2;
      }
      function clone(o) {
        if (typeof o !== "object" || o === null) return o;
        if (Array.isArray(o)) return cloneArray(o, clone);
        if (o.constructor !== Object && (handler = constructorHandlers.get(o.constructor))) {
          return handler(o, clone);
        }
        const o2 = {};
        refs.push(o);
        refsNew.push(o2);
        for (const k in o) {
          if (Object.hasOwnProperty.call(o, k) === false) continue;
          const cur = o[k];
          if (typeof cur !== "object" || cur === null) {
            o2[k] = cur;
          } else if (cur.constructor !== Object && (handler = constructorHandlers.get(cur.constructor))) {
            o2[k] = handler(cur, clone);
          } else if (ArrayBuffer.isView(cur)) {
            o2[k] = copyBuffer(cur);
          } else {
            const i = refs.indexOf(cur);
            if (i !== -1) {
              o2[k] = refsNew[i];
            } else {
              o2[k] = clone(cur);
            }
          }
        }
        refs.pop();
        refsNew.pop();
        return o2;
      }
      function cloneProto(o) {
        if (typeof o !== "object" || o === null) return o;
        if (Array.isArray(o)) return cloneArray(o, cloneProto);
        if (o.constructor !== Object && (handler = constructorHandlers.get(o.constructor))) {
          return handler(o, cloneProto);
        }
        const o2 = {};
        refs.push(o);
        refsNew.push(o2);
        for (const k in o) {
          const cur = o[k];
          if (typeof cur !== "object" || cur === null) {
            o2[k] = cur;
          } else if (cur.constructor !== Object && (handler = constructorHandlers.get(cur.constructor))) {
            o2[k] = handler(cur, cloneProto);
          } else if (ArrayBuffer.isView(cur)) {
            o2[k] = copyBuffer(cur);
          } else {
            const i = refs.indexOf(cur);
            if (i !== -1) {
              o2[k] = refsNew[i];
            } else {
              o2[k] = cloneProto(cur);
            }
          }
        }
        refs.pop();
        refsNew.pop();
        return o2;
      }
    }
  }
});
init_esm_shims();
init_esm_shims();
init_esm_shims();
var isBrowser = typeof navigator !== "undefined";
var target = typeof window !== "undefined" ? window : typeof globalThis !== "undefined" ? globalThis : typeof global !== "undefined" ? global : {};
var isInChromePanel = typeof target.chrome !== "undefined" && !!target.chrome.devtools;
var isInIframe = isBrowser && target.self !== target.top;
var _a;
var isInElectron = typeof navigator !== "undefined" && ((_a = navigator.userAgent) == null ? void 0 : _a.toLowerCase().includes("electron"));
var isNuxtApp = typeof window !== "undefined" && !!window.__NUXT__;
init_esm_shims();
var import_rfdc = __toESM(require_rfdc(), 1);
var classifyRE2 = /(?:^|[-_/])(\w)/g;
function toUpper(_, c) {
  return c ? c.toUpperCase() : "";
}
function classify2(str) {
  return str && `${str}`.replace(classifyRE2, toUpper);
}
function basename(filename, ext) {
  let normalizedFilename = filename.replace(/^[a-z]:/i, "").replace(/\\/g, "/");
  if (normalizedFilename.endsWith(`index${ext}`)) {
    normalizedFilename = normalizedFilename.replace(`/index${ext}`, ext);
  }
  const lastSlashIndex = normalizedFilename.lastIndexOf("/");
  const baseNameWithExt = normalizedFilename.substring(lastSlashIndex + 1);
  if (ext) {
    const extIndex = baseNameWithExt.lastIndexOf(ext);
    return baseNameWithExt.substring(0, extIndex);
  }
  return "";
}
var deepClone = (0, import_rfdc.default)({
  circles: true
});

// node_modules/.deno/perfect-debounce@1.0.0/node_modules/perfect-debounce/dist/index.mjs
var DEBOUNCE_DEFAULTS = {
  trailing: true
};
function debounce(fn, wait = 25, options = {}) {
  options = {
    ...DEBOUNCE_DEFAULTS,
    ...options
  };
  if (!Number.isFinite(wait)) {
    throw new TypeError("Expected `wait` to be a finite number");
  }
  let leadingValue;
  let timeout;
  let resolveList = [];
  let currentPromise;
  let trailingArgs;
  const applyFn = (_this, args) => {
    currentPromise = _applyPromised(fn, _this, args);
    currentPromise.finally(() => {
      currentPromise = null;
      if (options.trailing && trailingArgs && !timeout) {
        const promise = applyFn(_this, trailingArgs);
        trailingArgs = null;
        return promise;
      }
    });
    return currentPromise;
  };
  return function(...args) {
    if (currentPromise) {
      if (options.trailing) {
        trailingArgs = args;
      }
      return currentPromise;
    }
    return new Promise((resolve) => {
      const shouldCallNow = !timeout && options.leading;
      clearTimeout(timeout);
      timeout = setTimeout(() => {
        timeout = null;
        const promise = options.leading ? leadingValue : applyFn(this, args);
        for (const _resolve of resolveList) {
          _resolve(promise);
        }
        resolveList = [];
      }, wait);
      if (shouldCallNow) {
        leadingValue = applyFn(this, args);
        resolve(leadingValue);
      } else {
        resolveList.push(resolve);
      }
    });
  };
}
async function _applyPromised(fn, _this, args) {
  return await fn.apply(_this, args);
}

// node_modules/.deno/hookable@5.5.3/node_modules/hookable/dist/index.mjs
function flatHooks(configHooks, hooks2 = {}, parentName) {
  for (const key in configHooks) {
    const subHook = configHooks[key];
    const name = parentName ? `${parentName}:${key}` : key;
    if (typeof subHook === "object" && subHook !== null) {
      flatHooks(subHook, hooks2, name);
    } else if (typeof subHook === "function") {
      hooks2[name] = subHook;
    }
  }
  return hooks2;
}
var defaultTask = {
  run: (function_) => function_()
};
var _createTask = () => defaultTask;
var createTask = typeof console.createTask !== "undefined" ? console.createTask : _createTask;
function serialTaskCaller(hooks2, args) {
  const name = args.shift();
  const task = createTask(name);
  return hooks2.reduce((promise, hookFunction) => promise.then(() => task.run(() => hookFunction(...args))), Promise.resolve());
}
function parallelTaskCaller(hooks2, args) {
  const name = args.shift();
  const task = createTask(name);
  return Promise.all(hooks2.map((hook2) => task.run(() => hook2(...args))));
}
function callEachWith(callbacks, arg0) {
  for (const callback of [
    ...callbacks
  ]) {
    callback(arg0);
  }
}
var Hookable = class {
  constructor() {
    this._hooks = {};
    this._before = void 0;
    this._after = void 0;
    this._deprecatedMessages = void 0;
    this._deprecatedHooks = {};
    this.hook = this.hook.bind(this);
    this.callHook = this.callHook.bind(this);
    this.callHookWith = this.callHookWith.bind(this);
  }
  hook(name, function_, options = {}) {
    if (!name || typeof function_ !== "function") {
      return () => {
      };
    }
    const originalName = name;
    let dep;
    while (this._deprecatedHooks[name]) {
      dep = this._deprecatedHooks[name];
      name = dep.to;
    }
    if (dep && !options.allowDeprecated) {
      let message = dep.message;
      if (!message) {
        message = `${originalName} hook has been deprecated` + (dep.to ? `, please use ${dep.to}` : "");
      }
      if (!this._deprecatedMessages) {
        this._deprecatedMessages = /* @__PURE__ */ new Set();
      }
      if (!this._deprecatedMessages.has(message)) {
        console.warn(message);
        this._deprecatedMessages.add(message);
      }
    }
    if (!function_.name) {
      try {
        Object.defineProperty(function_, "name", {
          get: () => "_" + name.replace(/\W+/g, "_") + "_hook_cb",
          configurable: true
        });
      } catch {
      }
    }
    this._hooks[name] = this._hooks[name] || [];
    this._hooks[name].push(function_);
    return () => {
      if (function_) {
        this.removeHook(name, function_);
        function_ = void 0;
      }
    };
  }
  hookOnce(name, function_) {
    let _unreg;
    let _function = (...arguments_) => {
      if (typeof _unreg === "function") {
        _unreg();
      }
      _unreg = void 0;
      _function = void 0;
      return function_(...arguments_);
    };
    _unreg = this.hook(name, _function);
    return _unreg;
  }
  removeHook(name, function_) {
    if (this._hooks[name]) {
      const index = this._hooks[name].indexOf(function_);
      if (index !== -1) {
        this._hooks[name].splice(index, 1);
      }
      if (this._hooks[name].length === 0) {
        delete this._hooks[name];
      }
    }
  }
  deprecateHook(name, deprecated) {
    this._deprecatedHooks[name] = typeof deprecated === "string" ? {
      to: deprecated
    } : deprecated;
    const _hooks = this._hooks[name] || [];
    delete this._hooks[name];
    for (const hook2 of _hooks) {
      this.hook(name, hook2);
    }
  }
  deprecateHooks(deprecatedHooks) {
    Object.assign(this._deprecatedHooks, deprecatedHooks);
    for (const name in deprecatedHooks) {
      this.deprecateHook(name, deprecatedHooks[name]);
    }
  }
  addHooks(configHooks) {
    const hooks2 = flatHooks(configHooks);
    const removeFns = Object.keys(hooks2).map((key) => this.hook(key, hooks2[key]));
    return () => {
      for (const unreg of removeFns.splice(0, removeFns.length)) {
        unreg();
      }
    };
  }
  removeHooks(configHooks) {
    const hooks2 = flatHooks(configHooks);
    for (const key in hooks2) {
      this.removeHook(key, hooks2[key]);
    }
  }
  removeAllHooks() {
    for (const key in this._hooks) {
      delete this._hooks[key];
    }
  }
  callHook(name, ...arguments_) {
    arguments_.unshift(name);
    return this.callHookWith(serialTaskCaller, name, ...arguments_);
  }
  callHookParallel(name, ...arguments_) {
    arguments_.unshift(name);
    return this.callHookWith(parallelTaskCaller, name, ...arguments_);
  }
  callHookWith(caller, name, ...arguments_) {
    const event = this._before || this._after ? {
      name,
      args: arguments_,
      context: {}
    } : void 0;
    if (this._before) {
      callEachWith(this._before, event);
    }
    const result = caller(name in this._hooks ? [
      ...this._hooks[name]
    ] : [], arguments_);
    if (result instanceof Promise) {
      return result.finally(() => {
        if (this._after && event) {
          callEachWith(this._after, event);
        }
      });
    }
    if (this._after && event) {
      callEachWith(this._after, event);
    }
    return result;
  }
  beforeEach(function_) {
    this._before = this._before || [];
    this._before.push(function_);
    return () => {
      if (this._before !== void 0) {
        const index = this._before.indexOf(function_);
        if (index !== -1) {
          this._before.splice(index, 1);
        }
      }
    };
  }
  afterEach(function_) {
    this._after = this._after || [];
    this._after.push(function_);
    return () => {
      if (this._after !== void 0) {
        const index = this._after.indexOf(function_);
        if (index !== -1) {
          this._after.splice(index, 1);
        }
      }
    };
  }
};
function createHooks() {
  return new Hookable();
}

// node_modules/.deno/@vue+devtools-kit@7.7.9/node_modules/@vue/devtools-kit/dist/index.js
var __create2 = Object.create;
var __defProp2 = Object.defineProperty;
var __getOwnPropDesc2 = Object.getOwnPropertyDescriptor;
var __getOwnPropNames2 = Object.getOwnPropertyNames;
var __getProtoOf2 = Object.getPrototypeOf;
var __hasOwnProp2 = Object.prototype.hasOwnProperty;
var __esm2 = (fn, res) => function __init() {
  return fn && (res = (0, fn[__getOwnPropNames2(fn)[0]])(fn = 0)), res;
};
var __commonJS2 = (cb, mod) => function __require() {
  return mod || (0, cb[__getOwnPropNames2(cb)[0]])((mod = {
    exports: {}
  }).exports, mod), mod.exports;
};
var __copyProps2 = (to, from, except, desc) => {
  if (from && typeof from === "object" || typeof from === "function") {
    for (let key of __getOwnPropNames2(from)) if (!__hasOwnProp2.call(to, key) && key !== except) __defProp2(to, key, {
      get: () => from[key],
      enumerable: !(desc = __getOwnPropDesc2(from, key)) || desc.enumerable
    });
  }
  return to;
};
var __toESM2 = (mod, isNodeMode, target22) => (target22 = mod != null ? __create2(__getProtoOf2(mod)) : {}, __copyProps2(
  // If the importer is in node compatibility mode or this is not an ESM
  // file that has been converted to a CommonJS file using a Babel-
  // compatible transform (i.e. "__esModule" has not been set), then set
  // "default" to the CommonJS "module.exports" for node compatibility.
  isNodeMode || !mod || !mod.__esModule ? __defProp2(target22, "default", {
    value: mod,
    enumerable: true
  }) : target22,
  mod
));
var init_esm_shims2 = __esm2({
  "../../node_modules/.pnpm/tsup@8.4.0_@microsoft+api-extractor@7.51.1_@types+node@22.13.14__jiti@2.4.2_postcss@8.5_96eb05a9d65343021e53791dd83f3773/node_modules/tsup/assets/esm_shims.js"() {
    "use strict";
  }
});
var require_speakingurl = __commonJS2({
  "../../node_modules/.pnpm/speakingurl@14.0.1/node_modules/speakingurl/lib/speakingurl.js"(exports, module) {
    "use strict";
    init_esm_shims2();
    (function(root) {
      "use strict";
      var charMap = {
        // latin
        "\xC0": "A",
        "\xC1": "A",
        "\xC2": "A",
        "\xC3": "A",
        "\xC4": "Ae",
        "\xC5": "A",
        "\xC6": "AE",
        "\xC7": "C",
        "\xC8": "E",
        "\xC9": "E",
        "\xCA": "E",
        "\xCB": "E",
        "\xCC": "I",
        "\xCD": "I",
        "\xCE": "I",
        "\xCF": "I",
        "\xD0": "D",
        "\xD1": "N",
        "\xD2": "O",
        "\xD3": "O",
        "\xD4": "O",
        "\xD5": "O",
        "\xD6": "Oe",
        "\u0150": "O",
        "\xD8": "O",
        "\xD9": "U",
        "\xDA": "U",
        "\xDB": "U",
        "\xDC": "Ue",
        "\u0170": "U",
        "\xDD": "Y",
        "\xDE": "TH",
        "\xDF": "ss",
        "\xE0": "a",
        "\xE1": "a",
        "\xE2": "a",
        "\xE3": "a",
        "\xE4": "ae",
        "\xE5": "a",
        "\xE6": "ae",
        "\xE7": "c",
        "\xE8": "e",
        "\xE9": "e",
        "\xEA": "e",
        "\xEB": "e",
        "\xEC": "i",
        "\xED": "i",
        "\xEE": "i",
        "\xEF": "i",
        "\xF0": "d",
        "\xF1": "n",
        "\xF2": "o",
        "\xF3": "o",
        "\xF4": "o",
        "\xF5": "o",
        "\xF6": "oe",
        "\u0151": "o",
        "\xF8": "o",
        "\xF9": "u",
        "\xFA": "u",
        "\xFB": "u",
        "\xFC": "ue",
        "\u0171": "u",
        "\xFD": "y",
        "\xFE": "th",
        "\xFF": "y",
        "\u1E9E": "SS",
        // language specific
        // Arabic
        "\u0627": "a",
        "\u0623": "a",
        "\u0625": "i",
        "\u0622": "aa",
        "\u0624": "u",
        "\u0626": "e",
        "\u0621": "a",
        "\u0628": "b",
        "\u062A": "t",
        "\u062B": "th",
        "\u062C": "j",
        "\u062D": "h",
        "\u062E": "kh",
        "\u062F": "d",
        "\u0630": "th",
        "\u0631": "r",
        "\u0632": "z",
        "\u0633": "s",
        "\u0634": "sh",
        "\u0635": "s",
        "\u0636": "dh",
        "\u0637": "t",
        "\u0638": "z",
        "\u0639": "a",
        "\u063A": "gh",
        "\u0641": "f",
        "\u0642": "q",
        "\u0643": "k",
        "\u0644": "l",
        "\u0645": "m",
        "\u0646": "n",
        "\u0647": "h",
        "\u0648": "w",
        "\u064A": "y",
        "\u0649": "a",
        "\u0629": "h",
        "\uFEFB": "la",
        "\uFEF7": "laa",
        "\uFEF9": "lai",
        "\uFEF5": "laa",
        // Persian additional characters than Arabic
        "\u06AF": "g",
        "\u0686": "ch",
        "\u067E": "p",
        "\u0698": "zh",
        "\u06A9": "k",
        "\u06CC": "y",
        // Arabic diactrics
        "\u064E": "a",
        "\u064B": "an",
        "\u0650": "e",
        "\u064D": "en",
        "\u064F": "u",
        "\u064C": "on",
        "\u0652": "",
        // Arabic numbers
        "\u0660": "0",
        "\u0661": "1",
        "\u0662": "2",
        "\u0663": "3",
        "\u0664": "4",
        "\u0665": "5",
        "\u0666": "6",
        "\u0667": "7",
        "\u0668": "8",
        "\u0669": "9",
        // Persian numbers
        "\u06F0": "0",
        "\u06F1": "1",
        "\u06F2": "2",
        "\u06F3": "3",
        "\u06F4": "4",
        "\u06F5": "5",
        "\u06F6": "6",
        "\u06F7": "7",
        "\u06F8": "8",
        "\u06F9": "9",
        // Burmese consonants
        "\u1000": "k",
        "\u1001": "kh",
        "\u1002": "g",
        "\u1003": "ga",
        "\u1004": "ng",
        "\u1005": "s",
        "\u1006": "sa",
        "\u1007": "z",
        "\u1005\u103B": "za",
        "\u100A": "ny",
        "\u100B": "t",
        "\u100C": "ta",
        "\u100D": "d",
        "\u100E": "da",
        "\u100F": "na",
        "\u1010": "t",
        "\u1011": "ta",
        "\u1012": "d",
        "\u1013": "da",
        "\u1014": "n",
        "\u1015": "p",
        "\u1016": "pa",
        "\u1017": "b",
        "\u1018": "ba",
        "\u1019": "m",
        "\u101A": "y",
        "\u101B": "ya",
        "\u101C": "l",
        "\u101D": "w",
        "\u101E": "th",
        "\u101F": "h",
        "\u1020": "la",
        "\u1021": "a",
        // consonant character combos
        "\u103C": "y",
        "\u103B": "ya",
        "\u103D": "w",
        "\u103C\u103D": "yw",
        "\u103B\u103D": "ywa",
        "\u103E": "h",
        // independent vowels
        "\u1027": "e",
        "\u104F": "-e",
        "\u1023": "i",
        "\u1024": "-i",
        "\u1009": "u",
        "\u1026": "-u",
        "\u1029": "aw",
        "\u101E\u103C\u1031\u102C": "aw",
        "\u102A": "aw",
        // numbers
        "\u1040": "0",
        "\u1041": "1",
        "\u1042": "2",
        "\u1043": "3",
        "\u1044": "4",
        "\u1045": "5",
        "\u1046": "6",
        "\u1047": "7",
        "\u1048": "8",
        "\u1049": "9",
        // virama and tone marks which are silent in transliteration
        "\u1039": "",
        "\u1037": "",
        "\u1038": "",
        // Czech
        "\u010D": "c",
        "\u010F": "d",
        "\u011B": "e",
        "\u0148": "n",
        "\u0159": "r",
        "\u0161": "s",
        "\u0165": "t",
        "\u016F": "u",
        "\u017E": "z",
        "\u010C": "C",
        "\u010E": "D",
        "\u011A": "E",
        "\u0147": "N",
        "\u0158": "R",
        "\u0160": "S",
        "\u0164": "T",
        "\u016E": "U",
        "\u017D": "Z",
        // Dhivehi
        "\u0780": "h",
        "\u0781": "sh",
        "\u0782": "n",
        "\u0783": "r",
        "\u0784": "b",
        "\u0785": "lh",
        "\u0786": "k",
        "\u0787": "a",
        "\u0788": "v",
        "\u0789": "m",
        "\u078A": "f",
        "\u078B": "dh",
        "\u078C": "th",
        "\u078D": "l",
        "\u078E": "g",
        "\u078F": "gn",
        "\u0790": "s",
        "\u0791": "d",
        "\u0792": "z",
        "\u0793": "t",
        "\u0794": "y",
        "\u0795": "p",
        "\u0796": "j",
        "\u0797": "ch",
        "\u0798": "tt",
        "\u0799": "hh",
        "\u079A": "kh",
        "\u079B": "th",
        "\u079C": "z",
        "\u079D": "sh",
        "\u079E": "s",
        "\u079F": "d",
        "\u07A0": "t",
        "\u07A1": "z",
        "\u07A2": "a",
        "\u07A3": "gh",
        "\u07A4": "q",
        "\u07A5": "w",
        "\u07A6": "a",
        "\u07A7": "aa",
        "\u07A8": "i",
        "\u07A9": "ee",
        "\u07AA": "u",
        "\u07AB": "oo",
        "\u07AC": "e",
        "\u07AD": "ey",
        "\u07AE": "o",
        "\u07AF": "oa",
        "\u07B0": "",
        // Georgian https://en.wikipedia.org/wiki/Romanization_of_Georgian
        // National system (2002)
        "\u10D0": "a",
        "\u10D1": "b",
        "\u10D2": "g",
        "\u10D3": "d",
        "\u10D4": "e",
        "\u10D5": "v",
        "\u10D6": "z",
        "\u10D7": "t",
        "\u10D8": "i",
        "\u10D9": "k",
        "\u10DA": "l",
        "\u10DB": "m",
        "\u10DC": "n",
        "\u10DD": "o",
        "\u10DE": "p",
        "\u10DF": "zh",
        "\u10E0": "r",
        "\u10E1": "s",
        "\u10E2": "t",
        "\u10E3": "u",
        "\u10E4": "p",
        "\u10E5": "k",
        "\u10E6": "gh",
        "\u10E7": "q",
        "\u10E8": "sh",
        "\u10E9": "ch",
        "\u10EA": "ts",
        "\u10EB": "dz",
        "\u10EC": "ts",
        "\u10ED": "ch",
        "\u10EE": "kh",
        "\u10EF": "j",
        "\u10F0": "h",
        // Greek
        "\u03B1": "a",
        "\u03B2": "v",
        "\u03B3": "g",
        "\u03B4": "d",
        "\u03B5": "e",
        "\u03B6": "z",
        "\u03B7": "i",
        "\u03B8": "th",
        "\u03B9": "i",
        "\u03BA": "k",
        "\u03BB": "l",
        "\u03BC": "m",
        "\u03BD": "n",
        "\u03BE": "ks",
        "\u03BF": "o",
        "\u03C0": "p",
        "\u03C1": "r",
        "\u03C3": "s",
        "\u03C4": "t",
        "\u03C5": "y",
        "\u03C6": "f",
        "\u03C7": "x",
        "\u03C8": "ps",
        "\u03C9": "o",
        "\u03AC": "a",
        "\u03AD": "e",
        "\u03AF": "i",
        "\u03CC": "o",
        "\u03CD": "y",
        "\u03AE": "i",
        "\u03CE": "o",
        "\u03C2": "s",
        "\u03CA": "i",
        "\u03B0": "y",
        "\u03CB": "y",
        "\u0390": "i",
        "\u0391": "A",
        "\u0392": "B",
        "\u0393": "G",
        "\u0394": "D",
        "\u0395": "E",
        "\u0396": "Z",
        "\u0397": "I",
        "\u0398": "TH",
        "\u0399": "I",
        "\u039A": "K",
        "\u039B": "L",
        "\u039C": "M",
        "\u039D": "N",
        "\u039E": "KS",
        "\u039F": "O",
        "\u03A0": "P",
        "\u03A1": "R",
        "\u03A3": "S",
        "\u03A4": "T",
        "\u03A5": "Y",
        "\u03A6": "F",
        "\u03A7": "X",
        "\u03A8": "PS",
        "\u03A9": "O",
        "\u0386": "A",
        "\u0388": "E",
        "\u038A": "I",
        "\u038C": "O",
        "\u038E": "Y",
        "\u0389": "I",
        "\u038F": "O",
        "\u03AA": "I",
        "\u03AB": "Y",
        // Latvian
        "\u0101": "a",
        // 'č': 'c', // duplicate
        "\u0113": "e",
        "\u0123": "g",
        "\u012B": "i",
        "\u0137": "k",
        "\u013C": "l",
        "\u0146": "n",
        // 'š': 's', // duplicate
        "\u016B": "u",
        // 'ž': 'z', // duplicate
        "\u0100": "A",
        // 'Č': 'C', // duplicate
        "\u0112": "E",
        "\u0122": "G",
        "\u012A": "I",
        "\u0136": "k",
        "\u013B": "L",
        "\u0145": "N",
        // 'Š': 'S', // duplicate
        "\u016A": "U",
        // 'Ž': 'Z', // duplicate
        // Macedonian
        "\u040C": "Kj",
        "\u045C": "kj",
        "\u0409": "Lj",
        "\u0459": "lj",
        "\u040A": "Nj",
        "\u045A": "nj",
        "\u0422\u0441": "Ts",
        "\u0442\u0441": "ts",
        // Polish
        "\u0105": "a",
        "\u0107": "c",
        "\u0119": "e",
        "\u0142": "l",
        "\u0144": "n",
        // 'ó': 'o', // duplicate
        "\u015B": "s",
        "\u017A": "z",
        "\u017C": "z",
        "\u0104": "A",
        "\u0106": "C",
        "\u0118": "E",
        "\u0141": "L",
        "\u0143": "N",
        "\u015A": "S",
        "\u0179": "Z",
        "\u017B": "Z",
        // Ukranian
        "\u0404": "Ye",
        "\u0406": "I",
        "\u0407": "Yi",
        "\u0490": "G",
        "\u0454": "ye",
        "\u0456": "i",
        "\u0457": "yi",
        "\u0491": "g",
        // Romanian
        "\u0103": "a",
        "\u0102": "A",
        "\u0219": "s",
        "\u0218": "S",
        // 'ş': 's', // duplicate
        // 'Ş': 'S', // duplicate
        "\u021B": "t",
        "\u021A": "T",
        "\u0163": "t",
        "\u0162": "T",
        // Russian https://en.wikipedia.org/wiki/Romanization_of_Russian
        // ICAO
        "\u0430": "a",
        "\u0431": "b",
        "\u0432": "v",
        "\u0433": "g",
        "\u0434": "d",
        "\u0435": "e",
        "\u0451": "yo",
        "\u0436": "zh",
        "\u0437": "z",
        "\u0438": "i",
        "\u0439": "i",
        "\u043A": "k",
        "\u043B": "l",
        "\u043C": "m",
        "\u043D": "n",
        "\u043E": "o",
        "\u043F": "p",
        "\u0440": "r",
        "\u0441": "s",
        "\u0442": "t",
        "\u0443": "u",
        "\u0444": "f",
        "\u0445": "kh",
        "\u0446": "c",
        "\u0447": "ch",
        "\u0448": "sh",
        "\u0449": "sh",
        "\u044A": "",
        "\u044B": "y",
        "\u044C": "",
        "\u044D": "e",
        "\u044E": "yu",
        "\u044F": "ya",
        "\u0410": "A",
        "\u0411": "B",
        "\u0412": "V",
        "\u0413": "G",
        "\u0414": "D",
        "\u0415": "E",
        "\u0401": "Yo",
        "\u0416": "Zh",
        "\u0417": "Z",
        "\u0418": "I",
        "\u0419": "I",
        "\u041A": "K",
        "\u041B": "L",
        "\u041C": "M",
        "\u041D": "N",
        "\u041E": "O",
        "\u041F": "P",
        "\u0420": "R",
        "\u0421": "S",
        "\u0422": "T",
        "\u0423": "U",
        "\u0424": "F",
        "\u0425": "Kh",
        "\u0426": "C",
        "\u0427": "Ch",
        "\u0428": "Sh",
        "\u0429": "Sh",
        "\u042A": "",
        "\u042B": "Y",
        "\u042C": "",
        "\u042D": "E",
        "\u042E": "Yu",
        "\u042F": "Ya",
        // Serbian
        "\u0452": "dj",
        "\u0458": "j",
        // 'љ': 'lj',  // duplicate
        // 'њ': 'nj', // duplicate
        "\u045B": "c",
        "\u045F": "dz",
        "\u0402": "Dj",
        "\u0408": "j",
        // 'Љ': 'Lj', // duplicate
        // 'Њ': 'Nj', // duplicate
        "\u040B": "C",
        "\u040F": "Dz",
        // Slovak
        "\u013E": "l",
        "\u013A": "l",
        "\u0155": "r",
        "\u013D": "L",
        "\u0139": "L",
        "\u0154": "R",
        // Turkish
        "\u015F": "s",
        "\u015E": "S",
        "\u0131": "i",
        "\u0130": "I",
        // 'ç': 'c', // duplicate
        // 'Ç': 'C', // duplicate
        // 'ü': 'u', // duplicate, see langCharMap
        // 'Ü': 'U', // duplicate, see langCharMap
        // 'ö': 'o', // duplicate, see langCharMap
        // 'Ö': 'O', // duplicate, see langCharMap
        "\u011F": "g",
        "\u011E": "G",
        // Vietnamese
        "\u1EA3": "a",
        "\u1EA2": "A",
        "\u1EB3": "a",
        "\u1EB2": "A",
        "\u1EA9": "a",
        "\u1EA8": "A",
        "\u0111": "d",
        "\u0110": "D",
        "\u1EB9": "e",
        "\u1EB8": "E",
        "\u1EBD": "e",
        "\u1EBC": "E",
        "\u1EBB": "e",
        "\u1EBA": "E",
        "\u1EBF": "e",
        "\u1EBE": "E",
        "\u1EC1": "e",
        "\u1EC0": "E",
        "\u1EC7": "e",
        "\u1EC6": "E",
        "\u1EC5": "e",
        "\u1EC4": "E",
        "\u1EC3": "e",
        "\u1EC2": "E",
        "\u1ECF": "o",
        "\u1ECD": "o",
        "\u1ECC": "o",
        "\u1ED1": "o",
        "\u1ED0": "O",
        "\u1ED3": "o",
        "\u1ED2": "O",
        "\u1ED5": "o",
        "\u1ED4": "O",
        "\u1ED9": "o",
        "\u1ED8": "O",
        "\u1ED7": "o",
        "\u1ED6": "O",
        "\u01A1": "o",
        "\u01A0": "O",
        "\u1EDB": "o",
        "\u1EDA": "O",
        "\u1EDD": "o",
        "\u1EDC": "O",
        "\u1EE3": "o",
        "\u1EE2": "O",
        "\u1EE1": "o",
        "\u1EE0": "O",
        "\u1EDE": "o",
        "\u1EDF": "o",
        "\u1ECB": "i",
        "\u1ECA": "I",
        "\u0129": "i",
        "\u0128": "I",
        "\u1EC9": "i",
        "\u1EC8": "i",
        "\u1EE7": "u",
        "\u1EE6": "U",
        "\u1EE5": "u",
        "\u1EE4": "U",
        "\u0169": "u",
        "\u0168": "U",
        "\u01B0": "u",
        "\u01AF": "U",
        "\u1EE9": "u",
        "\u1EE8": "U",
        "\u1EEB": "u",
        "\u1EEA": "U",
        "\u1EF1": "u",
        "\u1EF0": "U",
        "\u1EEF": "u",
        "\u1EEE": "U",
        "\u1EED": "u",
        "\u1EEC": "\u01B0",
        "\u1EF7": "y",
        "\u1EF6": "y",
        "\u1EF3": "y",
        "\u1EF2": "Y",
        "\u1EF5": "y",
        "\u1EF4": "Y",
        "\u1EF9": "y",
        "\u1EF8": "Y",
        "\u1EA1": "a",
        "\u1EA0": "A",
        "\u1EA5": "a",
        "\u1EA4": "A",
        "\u1EA7": "a",
        "\u1EA6": "A",
        "\u1EAD": "a",
        "\u1EAC": "A",
        "\u1EAB": "a",
        "\u1EAA": "A",
        // 'ă': 'a', // duplicate
        // 'Ă': 'A', // duplicate
        "\u1EAF": "a",
        "\u1EAE": "A",
        "\u1EB1": "a",
        "\u1EB0": "A",
        "\u1EB7": "a",
        "\u1EB6": "A",
        "\u1EB5": "a",
        "\u1EB4": "A",
        "\u24EA": "0",
        "\u2460": "1",
        "\u2461": "2",
        "\u2462": "3",
        "\u2463": "4",
        "\u2464": "5",
        "\u2465": "6",
        "\u2466": "7",
        "\u2467": "8",
        "\u2468": "9",
        "\u2469": "10",
        "\u246A": "11",
        "\u246B": "12",
        "\u246C": "13",
        "\u246D": "14",
        "\u246E": "15",
        "\u246F": "16",
        "\u2470": "17",
        "\u2471": "18",
        "\u2472": "18",
        "\u2473": "18",
        "\u24F5": "1",
        "\u24F6": "2",
        "\u24F7": "3",
        "\u24F8": "4",
        "\u24F9": "5",
        "\u24FA": "6",
        "\u24FB": "7",
        "\u24FC": "8",
        "\u24FD": "9",
        "\u24FE": "10",
        "\u24FF": "0",
        "\u24EB": "11",
        "\u24EC": "12",
        "\u24ED": "13",
        "\u24EE": "14",
        "\u24EF": "15",
        "\u24F0": "16",
        "\u24F1": "17",
        "\u24F2": "18",
        "\u24F3": "19",
        "\u24F4": "20",
        "\u24B6": "A",
        "\u24B7": "B",
        "\u24B8": "C",
        "\u24B9": "D",
        "\u24BA": "E",
        "\u24BB": "F",
        "\u24BC": "G",
        "\u24BD": "H",
        "\u24BE": "I",
        "\u24BF": "J",
        "\u24C0": "K",
        "\u24C1": "L",
        "\u24C2": "M",
        "\u24C3": "N",
        "\u24C4": "O",
        "\u24C5": "P",
        "\u24C6": "Q",
        "\u24C7": "R",
        "\u24C8": "S",
        "\u24C9": "T",
        "\u24CA": "U",
        "\u24CB": "V",
        "\u24CC": "W",
        "\u24CD": "X",
        "\u24CE": "Y",
        "\u24CF": "Z",
        "\u24D0": "a",
        "\u24D1": "b",
        "\u24D2": "c",
        "\u24D3": "d",
        "\u24D4": "e",
        "\u24D5": "f",
        "\u24D6": "g",
        "\u24D7": "h",
        "\u24D8": "i",
        "\u24D9": "j",
        "\u24DA": "k",
        "\u24DB": "l",
        "\u24DC": "m",
        "\u24DD": "n",
        "\u24DE": "o",
        "\u24DF": "p",
        "\u24E0": "q",
        "\u24E1": "r",
        "\u24E2": "s",
        "\u24E3": "t",
        "\u24E4": "u",
        "\u24E6": "v",
        "\u24E5": "w",
        "\u24E7": "x",
        "\u24E8": "y",
        "\u24E9": "z",
        // symbols
        "\u201C": '"',
        "\u201D": '"',
        "\u2018": "'",
        "\u2019": "'",
        "\u2202": "d",
        "\u0192": "f",
        "\u2122": "(TM)",
        "\xA9": "(C)",
        "\u0153": "oe",
        "\u0152": "OE",
        "\xAE": "(R)",
        "\u2020": "+",
        "\u2120": "(SM)",
        "\u2026": "...",
        "\u02DA": "o",
        "\xBA": "o",
        "\xAA": "a",
        "\u2022": "*",
        "\u104A": ",",
        "\u104B": ".",
        // currency
        "$": "USD",
        "\u20AC": "EUR",
        "\u20A2": "BRN",
        "\u20A3": "FRF",
        "\xA3": "GBP",
        "\u20A4": "ITL",
        "\u20A6": "NGN",
        "\u20A7": "ESP",
        "\u20A9": "KRW",
        "\u20AA": "ILS",
        "\u20AB": "VND",
        "\u20AD": "LAK",
        "\u20AE": "MNT",
        "\u20AF": "GRD",
        "\u20B1": "ARS",
        "\u20B2": "PYG",
        "\u20B3": "ARA",
        "\u20B4": "UAH",
        "\u20B5": "GHS",
        "\xA2": "cent",
        "\xA5": "CNY",
        "\u5143": "CNY",
        "\u5186": "YEN",
        "\uFDFC": "IRR",
        "\u20A0": "EWE",
        "\u0E3F": "THB",
        "\u20A8": "INR",
        "\u20B9": "INR",
        "\u20B0": "PF",
        "\u20BA": "TRY",
        "\u060B": "AFN",
        "\u20BC": "AZN",
        "\u043B\u0432": "BGN",
        "\u17DB": "KHR",
        "\u20A1": "CRC",
        "\u20B8": "KZT",
        "\u0434\u0435\u043D": "MKD",
        "z\u0142": "PLN",
        "\u20BD": "RUB",
        "\u20BE": "GEL"
      };
      var lookAheadCharArray = [
        // burmese
        "\u103A",
        // Dhivehi
        "\u07B0"
      ];
      var diatricMap = {
        // Burmese
        // dependent vowels
        "\u102C": "a",
        "\u102B": "a",
        "\u1031": "e",
        "\u1032": "e",
        "\u102D": "i",
        "\u102E": "i",
        "\u102D\u102F": "o",
        "\u102F": "u",
        "\u1030": "u",
        "\u1031\u102B\u1004\u103A": "aung",
        "\u1031\u102C": "aw",
        "\u1031\u102C\u103A": "aw",
        "\u1031\u102B": "aw",
        "\u1031\u102B\u103A": "aw",
        "\u103A": "\u103A",
        // this is special case but the character will be converted to latin in the code
        "\u1000\u103A": "et",
        "\u102D\u102F\u1000\u103A": "aik",
        "\u1031\u102C\u1000\u103A": "auk",
        "\u1004\u103A": "in",
        "\u102D\u102F\u1004\u103A": "aing",
        "\u1031\u102C\u1004\u103A": "aung",
        "\u1005\u103A": "it",
        "\u100A\u103A": "i",
        "\u1010\u103A": "at",
        "\u102D\u1010\u103A": "eik",
        "\u102F\u1010\u103A": "ok",
        "\u103D\u1010\u103A": "ut",
        "\u1031\u1010\u103A": "it",
        "\u1012\u103A": "d",
        "\u102D\u102F\u1012\u103A": "ok",
        "\u102F\u1012\u103A": "ait",
        "\u1014\u103A": "an",
        "\u102C\u1014\u103A": "an",
        "\u102D\u1014\u103A": "ein",
        "\u102F\u1014\u103A": "on",
        "\u103D\u1014\u103A": "un",
        "\u1015\u103A": "at",
        "\u102D\u1015\u103A": "eik",
        "\u102F\u1015\u103A": "ok",
        "\u103D\u1015\u103A": "ut",
        "\u1014\u103A\u102F\u1015\u103A": "nub",
        "\u1019\u103A": "an",
        "\u102D\u1019\u103A": "ein",
        "\u102F\u1019\u103A": "on",
        "\u103D\u1019\u103A": "un",
        "\u101A\u103A": "e",
        "\u102D\u102F\u101C\u103A": "ol",
        "\u1009\u103A": "in",
        "\u1036": "an",
        "\u102D\u1036": "ein",
        "\u102F\u1036": "on",
        // Dhivehi
        "\u07A6\u0787\u07B0": "ah",
        "\u07A6\u0781\u07B0": "ah"
      };
      var langCharMap = {
        "en": {},
        // default language
        "az": {
          // Azerbaijani
          "\xE7": "c",
          "\u0259": "e",
          "\u011F": "g",
          "\u0131": "i",
          "\xF6": "o",
          "\u015F": "s",
          "\xFC": "u",
          "\xC7": "C",
          "\u018F": "E",
          "\u011E": "G",
          "\u0130": "I",
          "\xD6": "O",
          "\u015E": "S",
          "\xDC": "U"
        },
        "cs": {
          // Czech
          "\u010D": "c",
          "\u010F": "d",
          "\u011B": "e",
          "\u0148": "n",
          "\u0159": "r",
          "\u0161": "s",
          "\u0165": "t",
          "\u016F": "u",
          "\u017E": "z",
          "\u010C": "C",
          "\u010E": "D",
          "\u011A": "E",
          "\u0147": "N",
          "\u0158": "R",
          "\u0160": "S",
          "\u0164": "T",
          "\u016E": "U",
          "\u017D": "Z"
        },
        "fi": {
          // Finnish
          // 'å': 'a', duplicate see charMap/latin
          // 'Å': 'A', duplicate see charMap/latin
          "\xE4": "a",
          // ok
          "\xC4": "A",
          // ok
          "\xF6": "o",
          // ok
          "\xD6": "O"
        },
        "hu": {
          // Hungarian
          "\xE4": "a",
          // ok
          "\xC4": "A",
          // ok
          // 'á': 'a', duplicate see charMap/latin
          // 'Á': 'A', duplicate see charMap/latin
          "\xF6": "o",
          // ok
          "\xD6": "O",
          // ok
          // 'ő': 'o', duplicate see charMap/latin
          // 'Ő': 'O', duplicate see charMap/latin
          "\xFC": "u",
          "\xDC": "U",
          "\u0171": "u",
          "\u0170": "U"
        },
        "lt": {
          // Lithuanian
          "\u0105": "a",
          "\u010D": "c",
          "\u0119": "e",
          "\u0117": "e",
          "\u012F": "i",
          "\u0161": "s",
          "\u0173": "u",
          "\u016B": "u",
          "\u017E": "z",
          "\u0104": "A",
          "\u010C": "C",
          "\u0118": "E",
          "\u0116": "E",
          "\u012E": "I",
          "\u0160": "S",
          "\u0172": "U",
          "\u016A": "U"
        },
        "lv": {
          // Latvian
          "\u0101": "a",
          "\u010D": "c",
          "\u0113": "e",
          "\u0123": "g",
          "\u012B": "i",
          "\u0137": "k",
          "\u013C": "l",
          "\u0146": "n",
          "\u0161": "s",
          "\u016B": "u",
          "\u017E": "z",
          "\u0100": "A",
          "\u010C": "C",
          "\u0112": "E",
          "\u0122": "G",
          "\u012A": "i",
          "\u0136": "k",
          "\u013B": "L",
          "\u0145": "N",
          "\u0160": "S",
          "\u016A": "u",
          "\u017D": "Z"
        },
        "pl": {
          // Polish
          "\u0105": "a",
          "\u0107": "c",
          "\u0119": "e",
          "\u0142": "l",
          "\u0144": "n",
          "\xF3": "o",
          "\u015B": "s",
          "\u017A": "z",
          "\u017C": "z",
          "\u0104": "A",
          "\u0106": "C",
          "\u0118": "e",
          "\u0141": "L",
          "\u0143": "N",
          "\xD3": "O",
          "\u015A": "S",
          "\u0179": "Z",
          "\u017B": "Z"
        },
        "sv": {
          // Swedish
          // 'å': 'a', duplicate see charMap/latin
          // 'Å': 'A', duplicate see charMap/latin
          "\xE4": "a",
          // ok
          "\xC4": "A",
          // ok
          "\xF6": "o",
          // ok
          "\xD6": "O"
        },
        "sk": {
          // Slovak
          "\xE4": "a",
          "\xC4": "A"
        },
        "sr": {
          // Serbian
          "\u0459": "lj",
          "\u045A": "nj",
          "\u0409": "Lj",
          "\u040A": "Nj",
          "\u0111": "dj",
          "\u0110": "Dj"
        },
        "tr": {
          // Turkish
          "\xDC": "U",
          "\xD6": "O",
          "\xFC": "u",
          "\xF6": "o"
        }
      };
      var symbolMap = {
        "ar": {
          "\u2206": "delta",
          "\u221E": "la-nihaya",
          "\u2665": "hob",
          "&": "wa",
          "|": "aw",
          "<": "aqal-men",
          ">": "akbar-men",
          "\u2211": "majmou",
          "\xA4": "omla"
        },
        "az": {},
        "ca": {
          "\u2206": "delta",
          "\u221E": "infinit",
          "\u2665": "amor",
          "&": "i",
          "|": "o",
          "<": "menys que",
          ">": "mes que",
          "\u2211": "suma dels",
          "\xA4": "moneda"
        },
        "cs": {
          "\u2206": "delta",
          "\u221E": "nekonecno",
          "\u2665": "laska",
          "&": "a",
          "|": "nebo",
          "<": "mensi nez",
          ">": "vetsi nez",
          "\u2211": "soucet",
          "\xA4": "mena"
        },
        "de": {
          "\u2206": "delta",
          "\u221E": "unendlich",
          "\u2665": "Liebe",
          "&": "und",
          "|": "oder",
          "<": "kleiner als",
          ">": "groesser als",
          "\u2211": "Summe von",
          "\xA4": "Waehrung"
        },
        "dv": {
          "\u2206": "delta",
          "\u221E": "kolunulaa",
          "\u2665": "loabi",
          "&": "aai",
          "|": "noonee",
          "<": "ah vure kuda",
          ">": "ah vure bodu",
          "\u2211": "jumula",
          "\xA4": "faisaa"
        },
        "en": {
          "\u2206": "delta",
          "\u221E": "infinity",
          "\u2665": "love",
          "&": "and",
          "|": "or",
          "<": "less than",
          ">": "greater than",
          "\u2211": "sum",
          "\xA4": "currency"
        },
        "es": {
          "\u2206": "delta",
          "\u221E": "infinito",
          "\u2665": "amor",
          "&": "y",
          "|": "u",
          "<": "menos que",
          ">": "mas que",
          "\u2211": "suma de los",
          "\xA4": "moneda"
        },
        "fa": {
          "\u2206": "delta",
          "\u221E": "bi-nahayat",
          "\u2665": "eshgh",
          "&": "va",
          "|": "ya",
          "<": "kamtar-az",
          ">": "bishtar-az",
          "\u2211": "majmooe",
          "\xA4": "vahed"
        },
        "fi": {
          "\u2206": "delta",
          "\u221E": "aarettomyys",
          "\u2665": "rakkaus",
          "&": "ja",
          "|": "tai",
          "<": "pienempi kuin",
          ">": "suurempi kuin",
          "\u2211": "summa",
          "\xA4": "valuutta"
        },
        "fr": {
          "\u2206": "delta",
          "\u221E": "infiniment",
          "\u2665": "Amour",
          "&": "et",
          "|": "ou",
          "<": "moins que",
          ">": "superieure a",
          "\u2211": "somme des",
          "\xA4": "monnaie"
        },
        "ge": {
          "\u2206": "delta",
          "\u221E": "usasruloba",
          "\u2665": "siqvaruli",
          "&": "da",
          "|": "an",
          "<": "naklebi",
          ">": "meti",
          "\u2211": "jami",
          "\xA4": "valuta"
        },
        "gr": {},
        "hu": {
          "\u2206": "delta",
          "\u221E": "vegtelen",
          "\u2665": "szerelem",
          "&": "es",
          "|": "vagy",
          "<": "kisebb mint",
          ">": "nagyobb mint",
          "\u2211": "szumma",
          "\xA4": "penznem"
        },
        "it": {
          "\u2206": "delta",
          "\u221E": "infinito",
          "\u2665": "amore",
          "&": "e",
          "|": "o",
          "<": "minore di",
          ">": "maggiore di",
          "\u2211": "somma",
          "\xA4": "moneta"
        },
        "lt": {
          "\u2206": "delta",
          "\u221E": "begalybe",
          "\u2665": "meile",
          "&": "ir",
          "|": "ar",
          "<": "maziau nei",
          ">": "daugiau nei",
          "\u2211": "suma",
          "\xA4": "valiuta"
        },
        "lv": {
          "\u2206": "delta",
          "\u221E": "bezgaliba",
          "\u2665": "milestiba",
          "&": "un",
          "|": "vai",
          "<": "mazak neka",
          ">": "lielaks neka",
          "\u2211": "summa",
          "\xA4": "valuta"
        },
        "my": {
          "\u2206": "kwahkhyaet",
          "\u221E": "asaonasme",
          "\u2665": "akhyait",
          "&": "nhin",
          "|": "tho",
          "<": "ngethaw",
          ">": "kyithaw",
          "\u2211": "paungld",
          "\xA4": "ngwekye"
        },
        "mk": {},
        "nl": {
          "\u2206": "delta",
          "\u221E": "oneindig",
          "\u2665": "liefde",
          "&": "en",
          "|": "of",
          "<": "kleiner dan",
          ">": "groter dan",
          "\u2211": "som",
          "\xA4": "valuta"
        },
        "pl": {
          "\u2206": "delta",
          "\u221E": "nieskonczonosc",
          "\u2665": "milosc",
          "&": "i",
          "|": "lub",
          "<": "mniejsze niz",
          ">": "wieksze niz",
          "\u2211": "suma",
          "\xA4": "waluta"
        },
        "pt": {
          "\u2206": "delta",
          "\u221E": "infinito",
          "\u2665": "amor",
          "&": "e",
          "|": "ou",
          "<": "menor que",
          ">": "maior que",
          "\u2211": "soma",
          "\xA4": "moeda"
        },
        "ro": {
          "\u2206": "delta",
          "\u221E": "infinit",
          "\u2665": "dragoste",
          "&": "si",
          "|": "sau",
          "<": "mai mic ca",
          ">": "mai mare ca",
          "\u2211": "suma",
          "\xA4": "valuta"
        },
        "ru": {
          "\u2206": "delta",
          "\u221E": "beskonechno",
          "\u2665": "lubov",
          "&": "i",
          "|": "ili",
          "<": "menshe",
          ">": "bolshe",
          "\u2211": "summa",
          "\xA4": "valjuta"
        },
        "sk": {
          "\u2206": "delta",
          "\u221E": "nekonecno",
          "\u2665": "laska",
          "&": "a",
          "|": "alebo",
          "<": "menej ako",
          ">": "viac ako",
          "\u2211": "sucet",
          "\xA4": "mena"
        },
        "sr": {},
        "tr": {
          "\u2206": "delta",
          "\u221E": "sonsuzluk",
          "\u2665": "ask",
          "&": "ve",
          "|": "veya",
          "<": "kucuktur",
          ">": "buyuktur",
          "\u2211": "toplam",
          "\xA4": "para birimi"
        },
        "uk": {
          "\u2206": "delta",
          "\u221E": "bezkinechnist",
          "\u2665": "lubov",
          "&": "i",
          "|": "abo",
          "<": "menshe",
          ">": "bilshe",
          "\u2211": "suma",
          "\xA4": "valjuta"
        },
        "vn": {
          "\u2206": "delta",
          "\u221E": "vo cuc",
          "\u2665": "yeu",
          "&": "va",
          "|": "hoac",
          "<": "nho hon",
          ">": "lon hon",
          "\u2211": "tong",
          "\xA4": "tien te"
        }
      };
      var uricChars = [
        ";",
        "?",
        ":",
        "@",
        "&",
        "=",
        "+",
        "$",
        ",",
        "/"
      ].join("");
      var uricNoSlashChars = [
        ";",
        "?",
        ":",
        "@",
        "&",
        "=",
        "+",
        "$",
        ","
      ].join("");
      var markChars = [
        ".",
        "!",
        "~",
        "*",
        "'",
        "(",
        ")"
      ].join("");
      var getSlug = function getSlug2(input, opts) {
        var separator = "-";
        var result = "";
        var diatricString = "";
        var convertSymbols = true;
        var customReplacements = {};
        var maintainCase;
        var titleCase;
        var truncate;
        var uricFlag;
        var uricNoSlashFlag;
        var markFlag;
        var symbol;
        var langChar;
        var lucky;
        var i;
        var ch;
        var l;
        var lastCharWasSymbol;
        var lastCharWasDiatric;
        var allowedChars = "";
        if (typeof input !== "string") {
          return "";
        }
        if (typeof opts === "string") {
          separator = opts;
        }
        symbol = symbolMap.en;
        langChar = langCharMap.en;
        if (typeof opts === "object") {
          maintainCase = opts.maintainCase || false;
          customReplacements = opts.custom && typeof opts.custom === "object" ? opts.custom : customReplacements;
          truncate = +opts.truncate > 1 && opts.truncate || false;
          uricFlag = opts.uric || false;
          uricNoSlashFlag = opts.uricNoSlash || false;
          markFlag = opts.mark || false;
          convertSymbols = opts.symbols === false || opts.lang === false ? false : true;
          separator = opts.separator || separator;
          if (uricFlag) {
            allowedChars += uricChars;
          }
          if (uricNoSlashFlag) {
            allowedChars += uricNoSlashChars;
          }
          if (markFlag) {
            allowedChars += markChars;
          }
          symbol = opts.lang && symbolMap[opts.lang] && convertSymbols ? symbolMap[opts.lang] : convertSymbols ? symbolMap.en : {};
          langChar = opts.lang && langCharMap[opts.lang] ? langCharMap[opts.lang] : opts.lang === false || opts.lang === true ? {} : langCharMap.en;
          if (opts.titleCase && typeof opts.titleCase.length === "number" && Array.prototype.toString.call(opts.titleCase)) {
            opts.titleCase.forEach(function(v) {
              customReplacements[v + ""] = v + "";
            });
            titleCase = true;
          } else {
            titleCase = !!opts.titleCase;
          }
          if (opts.custom && typeof opts.custom.length === "number" && Array.prototype.toString.call(opts.custom)) {
            opts.custom.forEach(function(v) {
              customReplacements[v + ""] = v + "";
            });
          }
          Object.keys(customReplacements).forEach(function(v) {
            var r;
            if (v.length > 1) {
              r = new RegExp("\\b" + escapeChars(v) + "\\b", "gi");
            } else {
              r = new RegExp(escapeChars(v), "gi");
            }
            input = input.replace(r, customReplacements[v]);
          });
          for (ch in customReplacements) {
            allowedChars += ch;
          }
        }
        allowedChars += separator;
        allowedChars = escapeChars(allowedChars);
        input = input.replace(/(^\s+|\s+$)/g, "");
        lastCharWasSymbol = false;
        lastCharWasDiatric = false;
        for (i = 0, l = input.length; i < l; i++) {
          ch = input[i];
          if (isReplacedCustomChar(ch, customReplacements)) {
            lastCharWasSymbol = false;
          } else if (langChar[ch]) {
            ch = lastCharWasSymbol && langChar[ch].match(/[A-Za-z0-9]/) ? " " + langChar[ch] : langChar[ch];
            lastCharWasSymbol = false;
          } else if (ch in charMap) {
            if (i + 1 < l && lookAheadCharArray.indexOf(input[i + 1]) >= 0) {
              diatricString += ch;
              ch = "";
            } else if (lastCharWasDiatric === true) {
              ch = diatricMap[diatricString] + charMap[ch];
              diatricString = "";
            } else {
              ch = lastCharWasSymbol && charMap[ch].match(/[A-Za-z0-9]/) ? " " + charMap[ch] : charMap[ch];
            }
            lastCharWasSymbol = false;
            lastCharWasDiatric = false;
          } else if (ch in diatricMap) {
            diatricString += ch;
            ch = "";
            if (i === l - 1) {
              ch = diatricMap[diatricString];
            }
            lastCharWasDiatric = true;
          } else if (
            // process symbol chars
            symbol[ch] && !(uricFlag && uricChars.indexOf(ch) !== -1) && !(uricNoSlashFlag && uricNoSlashChars.indexOf(ch) !== -1)
          ) {
            ch = lastCharWasSymbol || result.substr(-1).match(/[A-Za-z0-9]/) ? separator + symbol[ch] : symbol[ch];
            ch += input[i + 1] !== void 0 && input[i + 1].match(/[A-Za-z0-9]/) ? separator : "";
            lastCharWasSymbol = true;
          } else {
            if (lastCharWasDiatric === true) {
              ch = diatricMap[diatricString] + ch;
              diatricString = "";
              lastCharWasDiatric = false;
            } else if (lastCharWasSymbol && (/[A-Za-z0-9]/.test(ch) || result.substr(-1).match(/A-Za-z0-9]/))) {
              ch = " " + ch;
            }
            lastCharWasSymbol = false;
          }
          result += ch.replace(new RegExp("[^\\w\\s" + allowedChars + "_-]", "g"), separator);
        }
        if (titleCase) {
          result = result.replace(/(\w)(\S*)/g, function(_, i2, r) {
            var j = i2.toUpperCase() + (r !== null ? r : "");
            return Object.keys(customReplacements).indexOf(j.toLowerCase()) < 0 ? j : j.toLowerCase();
          });
        }
        result = result.replace(/\s+/g, separator).replace(new RegExp("\\" + separator + "+", "g"), separator).replace(new RegExp("(^\\" + separator + "+|\\" + separator + "+$)", "g"), "");
        if (truncate && result.length > truncate) {
          lucky = result.charAt(truncate) === separator;
          result = result.slice(0, truncate);
          if (!lucky) {
            result = result.slice(0, result.lastIndexOf(separator));
          }
        }
        if (!maintainCase && !titleCase) {
          result = result.toLowerCase();
        }
        return result;
      };
      var createSlug = function createSlug2(opts) {
        return function getSlugWithConfig(input) {
          return getSlug(input, opts);
        };
      };
      var escapeChars = function escapeChars2(input) {
        return input.replace(/[-\\^$*+?.()|[\]{}\/]/g, "\\$&");
      };
      var isReplacedCustomChar = function(ch, customReplacements) {
        for (var c in customReplacements) {
          if (customReplacements[c] === ch) {
            return true;
          }
        }
      };
      if (typeof module !== "undefined" && module.exports) {
        module.exports = getSlug;
        module.exports.createSlug = createSlug;
      } else if (typeof define !== "undefined" && define.amd) {
        define([], function() {
          return getSlug;
        });
      } else {
        try {
          if (root.getSlug || root.createSlug) {
            throw "speakingurl: globals exists /(getSlug|createSlug)/";
          } else {
            root.getSlug = getSlug;
            root.createSlug = createSlug;
          }
        } catch (e) {
        }
      }
    })(exports);
  }
});
var require_speakingurl2 = __commonJS2({
  "../../node_modules/.pnpm/speakingurl@14.0.1/node_modules/speakingurl/index.js"(exports, module) {
    "use strict";
    init_esm_shims2();
    module.exports = require_speakingurl();
  }
});
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
function getComponentTypeName(options) {
  var _a25;
  const name = options.name || options._componentTag || options.__VUE_DEVTOOLS_COMPONENT_GUSSED_NAME__ || options.__name;
  if (name === "index" && ((_a25 = options.__file) == null ? void 0 : _a25.endsWith("index.vue"))) {
    return "";
  }
  return name;
}
function getComponentFileName(options) {
  const file = options.__file;
  if (file) return classify2(basename(file, ".vue"));
}
function saveComponentGussedName(instance, name) {
  instance.type.__VUE_DEVTOOLS_COMPONENT_GUSSED_NAME__ = name;
  return name;
}
function getAppRecord(instance) {
  if (instance.__VUE_DEVTOOLS_NEXT_APP_RECORD__) return instance.__VUE_DEVTOOLS_NEXT_APP_RECORD__;
  else if (instance.root) return instance.appContext.app.__VUE_DEVTOOLS_NEXT_APP_RECORD__;
}
function isFragment(instance) {
  var _a25, _b25;
  const subTreeType = (_a25 = instance.subTree) == null ? void 0 : _a25.type;
  const appRecord = getAppRecord(instance);
  if (appRecord) {
    return ((_b25 = appRecord == null ? void 0 : appRecord.types) == null ? void 0 : _b25.Fragment) === subTreeType;
  }
  return false;
}
function getInstanceName(instance) {
  var _a25, _b25, _c;
  const name = getComponentTypeName((instance == null ? void 0 : instance.type) || {});
  if (name) return name;
  if ((instance == null ? void 0 : instance.root) === instance) return "Root";
  for (const key in (_b25 = (_a25 = instance.parent) == null ? void 0 : _a25.type) == null ? void 0 : _b25.components) {
    if (instance.parent.type.components[key] === (instance == null ? void 0 : instance.type)) return saveComponentGussedName(instance, key);
  }
  for (const key in (_c = instance.appContext) == null ? void 0 : _c.components) {
    if (instance.appContext.components[key] === (instance == null ? void 0 : instance.type)) return saveComponentGussedName(instance, key);
  }
  const fileName = getComponentFileName((instance == null ? void 0 : instance.type) || {});
  if (fileName) return fileName;
  return "Anonymous Component";
}
function getUniqueComponentId(instance) {
  var _a25, _b25, _c;
  const appId = (_c = (_b25 = (_a25 = instance == null ? void 0 : instance.appContext) == null ? void 0 : _a25.app) == null ? void 0 : _b25.__VUE_DEVTOOLS_NEXT_APP_RECORD_ID__) != null ? _c : 0;
  const instanceId = instance === (instance == null ? void 0 : instance.root) ? "root" : instance.uid;
  return `${appId}:${instanceId}`;
}
function getComponentInstance(appRecord, instanceId) {
  instanceId = instanceId || `${appRecord.id}:root`;
  const instance = appRecord.instanceMap.get(instanceId);
  return instance || appRecord.instanceMap.get(":root");
}
function createRect() {
  const rect = {
    top: 0,
    bottom: 0,
    left: 0,
    right: 0,
    get width() {
      return rect.right - rect.left;
    },
    get height() {
      return rect.bottom - rect.top;
    }
  };
  return rect;
}
var range;
function getTextRect(node) {
  if (!range) range = document.createRange();
  range.selectNode(node);
  return range.getBoundingClientRect();
}
function getFragmentRect(vnode) {
  const rect = createRect();
  if (!vnode.children) return rect;
  for (let i = 0, l = vnode.children.length; i < l; i++) {
    const childVnode = vnode.children[i];
    let childRect;
    if (childVnode.component) {
      childRect = getComponentBoundingRect(childVnode.component);
    } else if (childVnode.el) {
      const el = childVnode.el;
      if (el.nodeType === 1 || el.getBoundingClientRect) childRect = el.getBoundingClientRect();
      else if (el.nodeType === 3 && el.data.trim()) childRect = getTextRect(el);
    }
    if (childRect) mergeRects(rect, childRect);
  }
  return rect;
}
function mergeRects(a, b) {
  if (!a.top || b.top < a.top) a.top = b.top;
  if (!a.bottom || b.bottom > a.bottom) a.bottom = b.bottom;
  if (!a.left || b.left < a.left) a.left = b.left;
  if (!a.right || b.right > a.right) a.right = b.right;
  return a;
}
var DEFAULT_RECT = {
  top: 0,
  left: 0,
  right: 0,
  bottom: 0,
  width: 0,
  height: 0
};
function getComponentBoundingRect(instance) {
  const el = instance.subTree.el;
  if (typeof window === "undefined") {
    return DEFAULT_RECT;
  }
  if (isFragment(instance)) return getFragmentRect(instance.subTree);
  else if ((el == null ? void 0 : el.nodeType) === 1) return el == null ? void 0 : el.getBoundingClientRect();
  else if (instance.subTree.component) return getComponentBoundingRect(instance.subTree.component);
  else return DEFAULT_RECT;
}
init_esm_shims2();
function getRootElementsFromComponentInstance(instance) {
  if (isFragment(instance)) return getFragmentRootElements(instance.subTree);
  if (!instance.subTree) return [];
  return [
    instance.subTree.el
  ];
}
function getFragmentRootElements(vnode) {
  if (!vnode.children) return [];
  const list = [];
  vnode.children.forEach((childVnode) => {
    if (childVnode.component) list.push(...getRootElementsFromComponentInstance(childVnode.component));
    else if (childVnode == null ? void 0 : childVnode.el) list.push(childVnode.el);
  });
  return list;
}
var CONTAINER_ELEMENT_ID = "__vue-devtools-component-inspector__";
var CARD_ELEMENT_ID = "__vue-devtools-component-inspector__card__";
var COMPONENT_NAME_ELEMENT_ID = "__vue-devtools-component-inspector__name__";
var INDICATOR_ELEMENT_ID = "__vue-devtools-component-inspector__indicator__";
var containerStyles = {
  display: "block",
  zIndex: 2147483640,
  position: "fixed",
  backgroundColor: "#42b88325",
  border: "1px solid #42b88350",
  borderRadius: "5px",
  transition: "all 0.1s ease-in",
  pointerEvents: "none"
};
var cardStyles = {
  fontFamily: "Arial, Helvetica, sans-serif",
  padding: "5px 8px",
  borderRadius: "4px",
  textAlign: "left",
  position: "absolute",
  left: 0,
  color: "#e9e9e9",
  fontSize: "14px",
  fontWeight: 600,
  lineHeight: "24px",
  backgroundColor: "#42b883",
  boxShadow: "0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px -1px rgba(0, 0, 0, 0.1)"
};
var indicatorStyles = {
  display: "inline-block",
  fontWeight: 400,
  fontStyle: "normal",
  fontSize: "12px",
  opacity: 0.7
};
function getContainerElement() {
  return document.getElementById(CONTAINER_ELEMENT_ID);
}
function getCardElement() {
  return document.getElementById(CARD_ELEMENT_ID);
}
function getIndicatorElement() {
  return document.getElementById(INDICATOR_ELEMENT_ID);
}
function getNameElement() {
  return document.getElementById(COMPONENT_NAME_ELEMENT_ID);
}
function getStyles(bounds) {
  return {
    left: `${Math.round(bounds.left * 100) / 100}px`,
    top: `${Math.round(bounds.top * 100) / 100}px`,
    width: `${Math.round(bounds.width * 100) / 100}px`,
    height: `${Math.round(bounds.height * 100) / 100}px`
  };
}
function create(options) {
  var _a25;
  const containerEl = document.createElement("div");
  containerEl.id = (_a25 = options.elementId) != null ? _a25 : CONTAINER_ELEMENT_ID;
  Object.assign(containerEl.style, {
    ...containerStyles,
    ...getStyles(options.bounds),
    ...options.style
  });
  const cardEl = document.createElement("span");
  cardEl.id = CARD_ELEMENT_ID;
  Object.assign(cardEl.style, {
    ...cardStyles,
    top: options.bounds.top < 35 ? 0 : "-35px"
  });
  const nameEl = document.createElement("span");
  nameEl.id = COMPONENT_NAME_ELEMENT_ID;
  nameEl.innerHTML = `&lt;${options.name}&gt;&nbsp;&nbsp;`;
  const indicatorEl = document.createElement("i");
  indicatorEl.id = INDICATOR_ELEMENT_ID;
  indicatorEl.innerHTML = `${Math.round(options.bounds.width * 100) / 100} x ${Math.round(options.bounds.height * 100) / 100}`;
  Object.assign(indicatorEl.style, indicatorStyles);
  cardEl.appendChild(nameEl);
  cardEl.appendChild(indicatorEl);
  containerEl.appendChild(cardEl);
  document.body.appendChild(containerEl);
  return containerEl;
}
function update(options) {
  const containerEl = getContainerElement();
  const cardEl = getCardElement();
  const nameEl = getNameElement();
  const indicatorEl = getIndicatorElement();
  if (containerEl) {
    Object.assign(containerEl.style, {
      ...containerStyles,
      ...getStyles(options.bounds)
    });
    Object.assign(cardEl.style, {
      top: options.bounds.top < 35 ? 0 : "-35px"
    });
    nameEl.innerHTML = `&lt;${options.name}&gt;&nbsp;&nbsp;`;
    indicatorEl.innerHTML = `${Math.round(options.bounds.width * 100) / 100} x ${Math.round(options.bounds.height * 100) / 100}`;
  }
}
function highlight(instance) {
  const bounds = getComponentBoundingRect(instance);
  if (!bounds.width && !bounds.height) return;
  const name = getInstanceName(instance);
  const container = getContainerElement();
  container ? update({
    bounds,
    name
  }) : create({
    bounds,
    name
  });
}
function unhighlight() {
  const el = getContainerElement();
  if (el) el.style.display = "none";
}
var inspectInstance = null;
function inspectFn(e) {
  const target22 = e.target;
  if (target22) {
    const instance = target22.__vueParentComponent;
    if (instance) {
      inspectInstance = instance;
      const el = instance.vnode.el;
      if (el) {
        const bounds = getComponentBoundingRect(instance);
        const name = getInstanceName(instance);
        const container = getContainerElement();
        container ? update({
          bounds,
          name
        }) : create({
          bounds,
          name
        });
      }
    }
  }
}
function selectComponentFn(e, cb) {
  e.preventDefault();
  e.stopPropagation();
  if (inspectInstance) {
    const uniqueComponentId = getUniqueComponentId(inspectInstance);
    cb(uniqueComponentId);
  }
}
var inspectComponentHighLighterSelectFn = null;
function cancelInspectComponentHighLighter() {
  unhighlight();
  window.removeEventListener("mouseover", inspectFn);
  window.removeEventListener("click", inspectComponentHighLighterSelectFn, true);
  inspectComponentHighLighterSelectFn = null;
}
function inspectComponentHighLighter() {
  window.addEventListener("mouseover", inspectFn);
  return new Promise((resolve) => {
    function onSelect(e) {
      e.preventDefault();
      e.stopPropagation();
      selectComponentFn(e, (id) => {
        window.removeEventListener("click", onSelect, true);
        inspectComponentHighLighterSelectFn = null;
        window.removeEventListener("mouseover", inspectFn);
        const el = getContainerElement();
        if (el) el.style.display = "none";
        resolve(JSON.stringify({
          id
        }));
      });
    }
    inspectComponentHighLighterSelectFn = onSelect;
    window.addEventListener("click", onSelect, true);
  });
}
function scrollToComponent(options) {
  const instance = getComponentInstance(activeAppRecord.value, options.id);
  if (instance) {
    const [el] = getRootElementsFromComponentInstance(instance);
    if (typeof el.scrollIntoView === "function") {
      el.scrollIntoView({
        behavior: "smooth"
      });
    } else {
      const bounds = getComponentBoundingRect(instance);
      const scrollTarget = document.createElement("div");
      const styles = {
        ...getStyles(bounds),
        position: "absolute"
      };
      Object.assign(scrollTarget.style, styles);
      document.body.appendChild(scrollTarget);
      scrollTarget.scrollIntoView({
        behavior: "smooth"
      });
      setTimeout(() => {
        document.body.removeChild(scrollTarget);
      }, 2e3);
    }
    setTimeout(() => {
      const bounds = getComponentBoundingRect(instance);
      if (bounds.width || bounds.height) {
        const name = getInstanceName(instance);
        const el2 = getContainerElement();
        el2 ? update({
          ...options,
          name,
          bounds
        }) : create({
          ...options,
          name,
          bounds
        });
        setTimeout(() => {
          if (el2) el2.style.display = "none";
        }, 1500);
      }
    }, 1200);
  }
}
init_esm_shims2();
var _a2;
var _b;
(_b = (_a2 = target).__VUE_DEVTOOLS_COMPONENT_INSPECTOR_ENABLED__) != null ? _b : _a2.__VUE_DEVTOOLS_COMPONENT_INSPECTOR_ENABLED__ = true;
function waitForInspectorInit(cb) {
  let total = 0;
  const timer = setInterval(() => {
    if (target.__VUE_INSPECTOR__) {
      clearInterval(timer);
      total += 30;
      cb();
    }
    if (total >= /* 5s */
    5e3) clearInterval(timer);
  }, 30);
}
function setupInspector() {
  const inspector = target.__VUE_INSPECTOR__;
  const _openInEditor = inspector.openInEditor;
  inspector.openInEditor = async (...params) => {
    inspector.disable();
    _openInEditor(...params);
  };
}
function getComponentInspector() {
  return new Promise((resolve) => {
    function setup8() {
      setupInspector();
      resolve(target.__VUE_INSPECTOR__);
    }
    if (!target.__VUE_INSPECTOR__) {
      waitForInspectorInit(() => {
        setup8();
      });
    } else {
      setup8();
    }
  });
}
init_esm_shims2();
init_esm_shims2();
function isReadonly2(value) {
  return !!(value && value[
    "__v_isReadonly"
    /* IS_READONLY */
  ]);
}
function isReactive2(value) {
  if (isReadonly2(value)) {
    return isReactive2(value[
      "__v_raw"
      /* RAW */
    ]);
  }
  return !!(value && value[
    "__v_isReactive"
    /* IS_REACTIVE */
  ]);
}
function isRef2(r) {
  return !!(r && r.__v_isRef === true);
}
function toRaw2(observed) {
  const raw = observed && observed[
    "__v_raw"
    /* RAW */
  ];
  return raw ? toRaw2(raw) : observed;
}
var Fragment2 = Symbol.for("v-fgt");
var StateEditor = class {
  constructor() {
    this.refEditor = new RefStateEditor();
  }
  set(object, path, value, cb) {
    const sections = Array.isArray(path) ? path : path.split(".");
    const markRef = false;
    while (sections.length > 1) {
      const section = sections.shift();
      if (object instanceof Map) object = object.get(section);
      else if (object instanceof Set) object = Array.from(object.values())[section];
      else object = object[section];
      if (this.refEditor.isRef(object)) object = this.refEditor.get(object);
    }
    const field = sections[0];
    const item = this.refEditor.get(object)[field];
    if (cb) {
      cb(object, field, value);
    } else {
      if (this.refEditor.isRef(item)) this.refEditor.set(item, value);
      else if (markRef) object[field] = value;
      else object[field] = value;
    }
  }
  get(object, path) {
    const sections = Array.isArray(path) ? path : path.split(".");
    for (let i = 0; i < sections.length; i++) {
      if (object instanceof Map) object = object.get(sections[i]);
      else object = object[sections[i]];
      if (this.refEditor.isRef(object)) object = this.refEditor.get(object);
      if (!object) return void 0;
    }
    return object;
  }
  has(object, path, parent = false) {
    if (typeof object === "undefined") return false;
    const sections = Array.isArray(path) ? path.slice() : path.split(".");
    const size = !parent ? 1 : 2;
    while (object && sections.length > size) {
      const section = sections.shift();
      object = object[section];
      if (this.refEditor.isRef(object)) object = this.refEditor.get(object);
    }
    return object != null && Object.prototype.hasOwnProperty.call(object, sections[0]);
  }
  createDefaultSetCallback(state) {
    return (object, field, value) => {
      if (state.remove || state.newKey) {
        if (Array.isArray(object)) object.splice(field, 1);
        else if (toRaw2(object) instanceof Map) object.delete(field);
        else if (toRaw2(object) instanceof Set) object.delete(Array.from(object.values())[field]);
        else Reflect.deleteProperty(object, field);
      }
      if (!state.remove) {
        const target22 = object[state.newKey || field];
        if (this.refEditor.isRef(target22)) this.refEditor.set(target22, value);
        else if (toRaw2(object) instanceof Map) object.set(state.newKey || field, value);
        else if (toRaw2(object) instanceof Set) object.add(value);
        else object[state.newKey || field] = value;
      }
    };
  }
};
var RefStateEditor = class {
  set(ref2, value) {
    if (isRef2(ref2)) {
      ref2.value = value;
    } else {
      if (ref2 instanceof Set && Array.isArray(value)) {
        ref2.clear();
        value.forEach((v) => ref2.add(v));
        return;
      }
      const currentKeys = Object.keys(value);
      if (ref2 instanceof Map) {
        const previousKeysSet2 = new Set(ref2.keys());
        currentKeys.forEach((key) => {
          ref2.set(key, Reflect.get(value, key));
          previousKeysSet2.delete(key);
        });
        previousKeysSet2.forEach((key) => ref2.delete(key));
        return;
      }
      const previousKeysSet = new Set(Object.keys(ref2));
      currentKeys.forEach((key) => {
        Reflect.set(ref2, key, Reflect.get(value, key));
        previousKeysSet.delete(key);
      });
      previousKeysSet.forEach((key) => Reflect.deleteProperty(ref2, key));
    }
  }
  get(ref2) {
    return isRef2(ref2) ? ref2.value : ref2;
  }
  isRef(ref2) {
    return isRef2(ref2) || isReactive2(ref2);
  }
};
var stateEditor = new StateEditor();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
var TIMELINE_LAYERS_STATE_STORAGE_ID = "__VUE_DEVTOOLS_KIT_TIMELINE_LAYERS_STATE__";
function getTimelineLayersStateFromStorage() {
  if (typeof window === "undefined" || !isBrowser || typeof localStorage === "undefined" || localStorage === null) {
    return {
      recordingState: false,
      mouseEventEnabled: false,
      keyboardEventEnabled: false,
      componentEventEnabled: false,
      performanceEventEnabled: false,
      selected: ""
    };
  }
  const state = typeof localStorage.getItem !== "undefined" ? localStorage.getItem(TIMELINE_LAYERS_STATE_STORAGE_ID) : null;
  return state ? JSON.parse(state) : {
    recordingState: false,
    mouseEventEnabled: false,
    keyboardEventEnabled: false,
    componentEventEnabled: false,
    performanceEventEnabled: false,
    selected: ""
  };
}
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
var _a22;
var _b2;
(_b2 = (_a22 = target).__VUE_DEVTOOLS_KIT_TIMELINE_LAYERS) != null ? _b2 : _a22.__VUE_DEVTOOLS_KIT_TIMELINE_LAYERS = [];
var devtoolsTimelineLayers = new Proxy(target.__VUE_DEVTOOLS_KIT_TIMELINE_LAYERS, {
  get(target22, prop, receiver) {
    return Reflect.get(target22, prop, receiver);
  }
});
function addTimelineLayer(options, descriptor) {
  devtoolsState.timelineLayersState[descriptor.id] = false;
  devtoolsTimelineLayers.push({
    ...options,
    descriptorId: descriptor.id,
    appRecord: getAppRecord(descriptor.app)
  });
}
var _a3;
var _b3;
(_b3 = (_a3 = target).__VUE_DEVTOOLS_KIT_INSPECTOR__) != null ? _b3 : _a3.__VUE_DEVTOOLS_KIT_INSPECTOR__ = [];
var devtoolsInspector = new Proxy(target.__VUE_DEVTOOLS_KIT_INSPECTOR__, {
  get(target22, prop, receiver) {
    return Reflect.get(target22, prop, receiver);
  }
});
var callInspectorUpdatedHook = debounce(() => {
  devtoolsContext.hooks.callHook("sendInspectorToClient", getActiveInspectors());
});
function addInspector(inspector, descriptor) {
  var _a25, _b25;
  devtoolsInspector.push({
    options: inspector,
    descriptor,
    treeFilterPlaceholder: (_a25 = inspector.treeFilterPlaceholder) != null ? _a25 : "Search tree...",
    stateFilterPlaceholder: (_b25 = inspector.stateFilterPlaceholder) != null ? _b25 : "Search state...",
    treeFilter: "",
    selectedNodeId: "",
    appRecord: getAppRecord(descriptor.app)
  });
  callInspectorUpdatedHook();
}
function getActiveInspectors() {
  return devtoolsInspector.filter((inspector) => inspector.descriptor.app === activeAppRecord.value.app).filter((inspector) => inspector.descriptor.id !== "components").map((inspector) => {
    var _a25;
    const descriptor = inspector.descriptor;
    const options = inspector.options;
    return {
      id: options.id,
      label: options.label,
      logo: descriptor.logo,
      icon: `custom-ic-baseline-${(_a25 = options == null ? void 0 : options.icon) == null ? void 0 : _a25.replace(/_/g, "-")}`,
      packageName: descriptor.packageName,
      homepage: descriptor.homepage,
      pluginId: descriptor.id
    };
  });
}
function getInspector(id, app) {
  return devtoolsInspector.find((inspector) => inspector.options.id === id && (app ? inspector.descriptor.app === app : true));
}
function createDevToolsCtxHooks() {
  const hooks2 = createHooks();
  hooks2.hook("addInspector", ({ inspector, plugin }) => {
    addInspector(inspector, plugin.descriptor);
  });
  const debounceSendInspectorTree = debounce(async ({ inspectorId, plugin }) => {
    var _a25;
    if (!inspectorId || !((_a25 = plugin == null ? void 0 : plugin.descriptor) == null ? void 0 : _a25.app) || devtoolsState.highPerfModeEnabled) return;
    const inspector = getInspector(inspectorId, plugin.descriptor.app);
    const _payload = {
      app: plugin.descriptor.app,
      inspectorId,
      filter: (inspector == null ? void 0 : inspector.treeFilter) || "",
      rootNodes: []
    };
    await new Promise((resolve) => {
      hooks2.callHookWith(
        async (callbacks) => {
          await Promise.all(callbacks.map((cb) => cb(_payload)));
          resolve();
        },
        "getInspectorTree"
        /* GET_INSPECTOR_TREE */
      );
    });
    hooks2.callHookWith(
      async (callbacks) => {
        await Promise.all(callbacks.map((cb) => cb({
          inspectorId,
          rootNodes: _payload.rootNodes
        })));
      },
      "sendInspectorTreeToClient"
      /* SEND_INSPECTOR_TREE_TO_CLIENT */
    );
  }, 120);
  hooks2.hook("sendInspectorTree", debounceSendInspectorTree);
  const debounceSendInspectorState = debounce(async ({ inspectorId, plugin }) => {
    var _a25;
    if (!inspectorId || !((_a25 = plugin == null ? void 0 : plugin.descriptor) == null ? void 0 : _a25.app) || devtoolsState.highPerfModeEnabled) return;
    const inspector = getInspector(inspectorId, plugin.descriptor.app);
    const _payload = {
      app: plugin.descriptor.app,
      inspectorId,
      nodeId: (inspector == null ? void 0 : inspector.selectedNodeId) || "",
      state: null
    };
    const ctx = {
      currentTab: `custom-inspector:${inspectorId}`
    };
    if (_payload.nodeId) {
      await new Promise((resolve) => {
        hooks2.callHookWith(
          async (callbacks) => {
            await Promise.all(callbacks.map((cb) => cb(_payload, ctx)));
            resolve();
          },
          "getInspectorState"
          /* GET_INSPECTOR_STATE */
        );
      });
    }
    hooks2.callHookWith(
      async (callbacks) => {
        await Promise.all(callbacks.map((cb) => cb({
          inspectorId,
          nodeId: _payload.nodeId,
          state: _payload.state
        })));
      },
      "sendInspectorStateToClient"
      /* SEND_INSPECTOR_STATE_TO_CLIENT */
    );
  }, 120);
  hooks2.hook("sendInspectorState", debounceSendInspectorState);
  hooks2.hook("customInspectorSelectNode", ({ inspectorId, nodeId, plugin }) => {
    const inspector = getInspector(inspectorId, plugin.descriptor.app);
    if (!inspector) return;
    inspector.selectedNodeId = nodeId;
  });
  hooks2.hook("timelineLayerAdded", ({ options, plugin }) => {
    addTimelineLayer(options, plugin.descriptor);
  });
  hooks2.hook("timelineEventAdded", ({ options, plugin }) => {
    var _a25;
    const internalLayerIds = [
      "performance",
      "component-event",
      "keyboard",
      "mouse"
    ];
    if (devtoolsState.highPerfModeEnabled || !((_a25 = devtoolsState.timelineLayersState) == null ? void 0 : _a25[plugin.descriptor.id]) && !internalLayerIds.includes(options.layerId)) return;
    hooks2.callHookWith(
      async (callbacks) => {
        await Promise.all(callbacks.map((cb) => cb(options)));
      },
      "sendTimelineEventToClient"
      /* SEND_TIMELINE_EVENT_TO_CLIENT */
    );
  });
  hooks2.hook("getComponentInstances", async ({ app }) => {
    const appRecord = app.__VUE_DEVTOOLS_NEXT_APP_RECORD__;
    if (!appRecord) return null;
    const appId = appRecord.id.toString();
    const instances = [
      ...appRecord.instanceMap
    ].filter(([key]) => key.split(":")[0] === appId).map(([, instance]) => instance);
    return instances;
  });
  hooks2.hook("getComponentBounds", async ({ instance }) => {
    const bounds = getComponentBoundingRect(instance);
    return bounds;
  });
  hooks2.hook("getComponentName", ({ instance }) => {
    const name = getInstanceName(instance);
    return name;
  });
  hooks2.hook("componentHighlight", ({ uid: uid2 }) => {
    const instance = activeAppRecord.value.instanceMap.get(uid2);
    if (instance) {
      highlight(instance);
    }
  });
  hooks2.hook("componentUnhighlight", () => {
    unhighlight();
  });
  return hooks2;
}
var _a4;
var _b4;
(_b4 = (_a4 = target).__VUE_DEVTOOLS_KIT_APP_RECORDS__) != null ? _b4 : _a4.__VUE_DEVTOOLS_KIT_APP_RECORDS__ = [];
var _a5;
var _b5;
(_b5 = (_a5 = target).__VUE_DEVTOOLS_KIT_ACTIVE_APP_RECORD__) != null ? _b5 : _a5.__VUE_DEVTOOLS_KIT_ACTIVE_APP_RECORD__ = {};
var _a6;
var _b6;
(_b6 = (_a6 = target).__VUE_DEVTOOLS_KIT_ACTIVE_APP_RECORD_ID__) != null ? _b6 : _a6.__VUE_DEVTOOLS_KIT_ACTIVE_APP_RECORD_ID__ = "";
var _a7;
var _b7;
(_b7 = (_a7 = target).__VUE_DEVTOOLS_KIT_CUSTOM_TABS__) != null ? _b7 : _a7.__VUE_DEVTOOLS_KIT_CUSTOM_TABS__ = [];
var _a8;
var _b8;
(_b8 = (_a8 = target).__VUE_DEVTOOLS_KIT_CUSTOM_COMMANDS__) != null ? _b8 : _a8.__VUE_DEVTOOLS_KIT_CUSTOM_COMMANDS__ = [];
var STATE_KEY = "__VUE_DEVTOOLS_KIT_GLOBAL_STATE__";
function initStateFactory() {
  return {
    connected: false,
    clientConnected: false,
    vitePluginDetected: true,
    appRecords: [],
    activeAppRecordId: "",
    tabs: [],
    commands: [],
    highPerfModeEnabled: true,
    devtoolsClientDetected: {},
    perfUniqueGroupId: 0,
    timelineLayersState: getTimelineLayersStateFromStorage()
  };
}
var _a9;
var _b9;
(_b9 = (_a9 = target)[STATE_KEY]) != null ? _b9 : _a9[STATE_KEY] = initStateFactory();
var callStateUpdatedHook = debounce((state) => {
  devtoolsContext.hooks.callHook("devtoolsStateUpdated", {
    state
  });
});
var callConnectedUpdatedHook = debounce((state, oldState) => {
  devtoolsContext.hooks.callHook("devtoolsConnectedUpdated", {
    state,
    oldState
  });
});
var devtoolsAppRecords = new Proxy(target.__VUE_DEVTOOLS_KIT_APP_RECORDS__, {
  get(_target, prop, receiver) {
    if (prop === "value") return target.__VUE_DEVTOOLS_KIT_APP_RECORDS__;
    return target.__VUE_DEVTOOLS_KIT_APP_RECORDS__[prop];
  }
});
var activeAppRecord = new Proxy(target.__VUE_DEVTOOLS_KIT_ACTIVE_APP_RECORD__, {
  get(_target, prop, receiver) {
    if (prop === "value") return target.__VUE_DEVTOOLS_KIT_ACTIVE_APP_RECORD__;
    else if (prop === "id") return target.__VUE_DEVTOOLS_KIT_ACTIVE_APP_RECORD_ID__;
    return target.__VUE_DEVTOOLS_KIT_ACTIVE_APP_RECORD__[prop];
  }
});
function updateAllStates() {
  callStateUpdatedHook({
    ...target[STATE_KEY],
    appRecords: devtoolsAppRecords.value,
    activeAppRecordId: activeAppRecord.id,
    tabs: target.__VUE_DEVTOOLS_KIT_CUSTOM_TABS__,
    commands: target.__VUE_DEVTOOLS_KIT_CUSTOM_COMMANDS__
  });
}
function setActiveAppRecord(app) {
  target.__VUE_DEVTOOLS_KIT_ACTIVE_APP_RECORD__ = app;
  updateAllStates();
}
function setActiveAppRecordId(id) {
  target.__VUE_DEVTOOLS_KIT_ACTIVE_APP_RECORD_ID__ = id;
  updateAllStates();
}
var devtoolsState = new Proxy(target[STATE_KEY], {
  get(target22, property) {
    if (property === "appRecords") {
      return devtoolsAppRecords;
    } else if (property === "activeAppRecordId") {
      return activeAppRecord.id;
    } else if (property === "tabs") {
      return target.__VUE_DEVTOOLS_KIT_CUSTOM_TABS__;
    } else if (property === "commands") {
      return target.__VUE_DEVTOOLS_KIT_CUSTOM_COMMANDS__;
    }
    return target[STATE_KEY][property];
  },
  deleteProperty(target22, property) {
    delete target22[property];
    return true;
  },
  set(target22, property, value) {
    const oldState = {
      ...target[STATE_KEY]
    };
    target22[property] = value;
    target[STATE_KEY][property] = value;
    return true;
  }
});
function openInEditor(options = {}) {
  var _a25, _b25, _c;
  const { file, host, baseUrl = window.location.origin, line = 0, column = 0 } = options;
  if (file) {
    if (host === "chrome-extension") {
      const fileName = file.replace(/\\/g, "\\\\");
      const _baseUrl = (_b25 = (_a25 = window.VUE_DEVTOOLS_CONFIG) == null ? void 0 : _a25.openInEditorHost) != null ? _b25 : "/";
      fetch(`${_baseUrl}__open-in-editor?file=${encodeURI(file)}`).then((response) => {
        if (!response.ok) {
          const msg = `Opening component ${fileName} failed`;
          console.log(`%c${msg}`, "color:red");
        }
      });
    } else if (devtoolsState.vitePluginDetected) {
      const _baseUrl = (_c = target.__VUE_DEVTOOLS_OPEN_IN_EDITOR_BASE_URL__) != null ? _c : baseUrl;
      target.__VUE_INSPECTOR__.openInEditor(_baseUrl, file, line, column);
    }
  }
}
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
var _a10;
var _b10;
(_b10 = (_a10 = target).__VUE_DEVTOOLS_KIT_PLUGIN_BUFFER__) != null ? _b10 : _a10.__VUE_DEVTOOLS_KIT_PLUGIN_BUFFER__ = [];
var devtoolsPluginBuffer = new Proxy(target.__VUE_DEVTOOLS_KIT_PLUGIN_BUFFER__, {
  get(target22, prop, receiver) {
    return Reflect.get(target22, prop, receiver);
  }
});
function _getSettings(settings) {
  const _settings = {};
  Object.keys(settings).forEach((key) => {
    _settings[key] = settings[key].defaultValue;
  });
  return _settings;
}
function getPluginLocalKey(pluginId) {
  return `__VUE_DEVTOOLS_NEXT_PLUGIN_SETTINGS__${pluginId}__`;
}
function getPluginSettingsOptions(pluginId) {
  var _a25, _b25, _c;
  const item = (_b25 = (_a25 = devtoolsPluginBuffer.find((item2) => {
    var _a26;
    return item2[0].id === pluginId && !!((_a26 = item2[0]) == null ? void 0 : _a26.settings);
  })) == null ? void 0 : _a25[0]) != null ? _b25 : null;
  return (_c = item == null ? void 0 : item.settings) != null ? _c : null;
}
function getPluginSettings(pluginId, fallbackValue) {
  var _a25, _b25, _c;
  const localKey = getPluginLocalKey(pluginId);
  if (localKey) {
    const localSettings = localStorage.getItem(localKey);
    if (localSettings) {
      return JSON.parse(localSettings);
    }
  }
  if (pluginId) {
    const item = (_b25 = (_a25 = devtoolsPluginBuffer.find((item2) => item2[0].id === pluginId)) == null ? void 0 : _a25[0]) != null ? _b25 : null;
    return _getSettings((_c = item == null ? void 0 : item.settings) != null ? _c : {});
  }
  return _getSettings(fallbackValue);
}
function initPluginSettings(pluginId, settings) {
  const localKey = getPluginLocalKey(pluginId);
  const localSettings = localStorage.getItem(localKey);
  if (!localSettings) {
    localStorage.setItem(localKey, JSON.stringify(_getSettings(settings)));
  }
}
function setPluginSettings(pluginId, key, value) {
  const localKey = getPluginLocalKey(pluginId);
  const localSettings = localStorage.getItem(localKey);
  const parsedLocalSettings = JSON.parse(localSettings || "{}");
  const updated = {
    ...parsedLocalSettings,
    [key]: value
  };
  localStorage.setItem(localKey, JSON.stringify(updated));
  devtoolsContext.hooks.callHookWith(
    (callbacks) => {
      callbacks.forEach((cb) => cb({
        pluginId,
        key,
        oldValue: parsedLocalSettings[key],
        newValue: value,
        settings: updated
      }));
    },
    "setPluginSettings"
    /* SET_PLUGIN_SETTINGS */
  );
}
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
var _a11;
var _b11;
var devtoolsHooks = (_b11 = (_a11 = target).__VUE_DEVTOOLS_HOOK) != null ? _b11 : _a11.__VUE_DEVTOOLS_HOOK = createHooks();
var on = {
  vueAppInit(fn) {
    devtoolsHooks.hook("app:init", fn);
  },
  vueAppUnmount(fn) {
    devtoolsHooks.hook("app:unmount", fn);
  },
  vueAppConnected(fn) {
    devtoolsHooks.hook("app:connected", fn);
  },
  componentAdded(fn) {
    return devtoolsHooks.hook("component:added", fn);
  },
  componentEmit(fn) {
    return devtoolsHooks.hook("component:emit", fn);
  },
  componentUpdated(fn) {
    return devtoolsHooks.hook("component:updated", fn);
  },
  componentRemoved(fn) {
    return devtoolsHooks.hook("component:removed", fn);
  },
  setupDevtoolsPlugin(fn) {
    devtoolsHooks.hook("devtools-plugin:setup", fn);
  },
  perfStart(fn) {
    return devtoolsHooks.hook("perf:start", fn);
  },
  perfEnd(fn) {
    return devtoolsHooks.hook("perf:end", fn);
  }
};
var hook = {
  on,
  setupDevToolsPlugin(pluginDescriptor, setupFn) {
    return devtoolsHooks.callHook("devtools-plugin:setup", pluginDescriptor, setupFn);
  }
};
var DevToolsV6PluginAPI = class {
  constructor({ plugin, ctx }) {
    this.hooks = ctx.hooks;
    this.plugin = plugin;
  }
  get on() {
    return {
      // component inspector
      visitComponentTree: (handler) => {
        this.hooks.hook("visitComponentTree", handler);
      },
      inspectComponent: (handler) => {
        this.hooks.hook("inspectComponent", handler);
      },
      editComponentState: (handler) => {
        this.hooks.hook("editComponentState", handler);
      },
      // custom inspector
      getInspectorTree: (handler) => {
        this.hooks.hook("getInspectorTree", handler);
      },
      getInspectorState: (handler) => {
        this.hooks.hook("getInspectorState", handler);
      },
      editInspectorState: (handler) => {
        this.hooks.hook("editInspectorState", handler);
      },
      // timeline
      inspectTimelineEvent: (handler) => {
        this.hooks.hook("inspectTimelineEvent", handler);
      },
      timelineCleared: (handler) => {
        this.hooks.hook("timelineCleared", handler);
      },
      // settings
      setPluginSettings: (handler) => {
        this.hooks.hook("setPluginSettings", handler);
      }
    };
  }
  // component inspector
  notifyComponentUpdate(instance) {
    var _a25;
    if (devtoolsState.highPerfModeEnabled) {
      return;
    }
    const inspector = getActiveInspectors().find((i) => i.packageName === this.plugin.descriptor.packageName);
    if (inspector == null ? void 0 : inspector.id) {
      if (instance) {
        const args = [
          instance.appContext.app,
          instance.uid,
          (_a25 = instance.parent) == null ? void 0 : _a25.uid,
          instance
        ];
        devtoolsHooks.callHook("component:updated", ...args);
      } else {
        devtoolsHooks.callHook(
          "component:updated"
          /* COMPONENT_UPDATED */
        );
      }
      this.hooks.callHook("sendInspectorState", {
        inspectorId: inspector.id,
        plugin: this.plugin
      });
    }
  }
  // custom inspector
  addInspector(options) {
    this.hooks.callHook("addInspector", {
      inspector: options,
      plugin: this.plugin
    });
    if (this.plugin.descriptor.settings) {
      initPluginSettings(options.id, this.plugin.descriptor.settings);
    }
  }
  sendInspectorTree(inspectorId) {
    if (devtoolsState.highPerfModeEnabled) {
      return;
    }
    this.hooks.callHook("sendInspectorTree", {
      inspectorId,
      plugin: this.plugin
    });
  }
  sendInspectorState(inspectorId) {
    if (devtoolsState.highPerfModeEnabled) {
      return;
    }
    this.hooks.callHook("sendInspectorState", {
      inspectorId,
      plugin: this.plugin
    });
  }
  selectInspectorNode(inspectorId, nodeId) {
    this.hooks.callHook("customInspectorSelectNode", {
      inspectorId,
      nodeId,
      plugin: this.plugin
    });
  }
  visitComponentTree(payload) {
    return this.hooks.callHook("visitComponentTree", payload);
  }
  // timeline
  now() {
    if (devtoolsState.highPerfModeEnabled) {
      return 0;
    }
    return Date.now();
  }
  addTimelineLayer(options) {
    this.hooks.callHook("timelineLayerAdded", {
      options,
      plugin: this.plugin
    });
  }
  addTimelineEvent(options) {
    if (devtoolsState.highPerfModeEnabled) {
      return;
    }
    this.hooks.callHook("timelineEventAdded", {
      options,
      plugin: this.plugin
    });
  }
  // settings
  getSettings(pluginId) {
    return getPluginSettings(pluginId != null ? pluginId : this.plugin.descriptor.id, this.plugin.descriptor.settings);
  }
  // utilities
  getComponentInstances(app) {
    return this.hooks.callHook("getComponentInstances", {
      app
    });
  }
  getComponentBounds(instance) {
    return this.hooks.callHook("getComponentBounds", {
      instance
    });
  }
  getComponentName(instance) {
    return this.hooks.callHook("getComponentName", {
      instance
    });
  }
  highlightElement(instance) {
    const uid2 = instance.__VUE_DEVTOOLS_NEXT_UID__;
    return this.hooks.callHook("componentHighlight", {
      uid: uid2
    });
  }
  unhighlightElement() {
    return this.hooks.callHook(
      "componentUnhighlight"
      /* COMPONENT_UNHIGHLIGHT */
    );
  }
};
var DevToolsPluginAPI = DevToolsV6PluginAPI;
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
var UNDEFINED = "__vue_devtool_undefined__";
var INFINITY = "__vue_devtool_infinity__";
var NEGATIVE_INFINITY = "__vue_devtool_negative_infinity__";
var NAN = "__vue_devtool_nan__";
init_esm_shims2();
init_esm_shims2();
var tokenMap = {
  [UNDEFINED]: "undefined",
  [NAN]: "NaN",
  [INFINITY]: "Infinity",
  [NEGATIVE_INFINITY]: "-Infinity"
};
var reversedTokenMap = Object.entries(tokenMap).reduce((acc, [key, value]) => {
  acc[value] = key;
  return acc;
}, {});
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
var _a12;
var _b12;
(_b12 = (_a12 = target).__VUE_DEVTOOLS_KIT__REGISTERED_PLUGIN_APPS__) != null ? _b12 : _a12.__VUE_DEVTOOLS_KIT__REGISTERED_PLUGIN_APPS__ = /* @__PURE__ */ new Set();
function setupDevToolsPlugin(pluginDescriptor, setupFn) {
  return hook.setupDevToolsPlugin(pluginDescriptor, setupFn);
}
function callDevToolsPluginSetupFn(plugin, app) {
  const [pluginDescriptor, setupFn] = plugin;
  if (pluginDescriptor.app !== app) return;
  const api = new DevToolsPluginAPI({
    plugin: {
      setupFn,
      descriptor: pluginDescriptor
    },
    ctx: devtoolsContext
  });
  if (pluginDescriptor.packageName === "vuex") {
    api.on.editInspectorState((payload) => {
      api.sendInspectorState(payload.inspectorId);
    });
  }
  setupFn(api);
}
function registerDevToolsPlugin(app, options) {
  if (target.__VUE_DEVTOOLS_KIT__REGISTERED_PLUGIN_APPS__.has(app)) {
    return;
  }
  if (devtoolsState.highPerfModeEnabled && !(options == null ? void 0 : options.inspectingComponent)) {
    return;
  }
  target.__VUE_DEVTOOLS_KIT__REGISTERED_PLUGIN_APPS__.add(app);
  devtoolsPluginBuffer.forEach((plugin) => {
    callDevToolsPluginSetupFn(plugin, app);
  });
}
init_esm_shims2();
init_esm_shims2();
var ROUTER_KEY = "__VUE_DEVTOOLS_ROUTER__";
var ROUTER_INFO_KEY = "__VUE_DEVTOOLS_ROUTER_INFO__";
var _a13;
var _b13;
(_b13 = (_a13 = target)[ROUTER_INFO_KEY]) != null ? _b13 : _a13[ROUTER_INFO_KEY] = {
  currentRoute: null,
  routes: []
};
var _a14;
var _b14;
(_b14 = (_a14 = target)[ROUTER_KEY]) != null ? _b14 : _a14[ROUTER_KEY] = {};
var devtoolsRouterInfo = new Proxy(target[ROUTER_INFO_KEY], {
  get(target22, property) {
    return target[ROUTER_INFO_KEY][property];
  }
});
var devtoolsRouter = new Proxy(target[ROUTER_KEY], {
  get(target22, property) {
    if (property === "value") {
      return target[ROUTER_KEY];
    }
  }
});
function getRoutes(router) {
  const routesMap = /* @__PURE__ */ new Map();
  return ((router == null ? void 0 : router.getRoutes()) || []).filter((i) => !routesMap.has(i.path) && routesMap.set(i.path, 1));
}
function filterRoutes(routes) {
  return routes.map((item) => {
    let { path, name, children, meta } = item;
    if (children == null ? void 0 : children.length) children = filterRoutes(children);
    return {
      path,
      name,
      children,
      meta
    };
  });
}
function filterCurrentRoute(route) {
  if (route) {
    const { fullPath, hash, href, path, name, matched, params, query } = route;
    return {
      fullPath,
      hash,
      href,
      path,
      name,
      params,
      query,
      matched: filterRoutes(matched)
    };
  }
  return route;
}
function normalizeRouterInfo(appRecord, activeAppRecord2) {
  function init() {
    var _a25;
    const router = (_a25 = appRecord.app) == null ? void 0 : _a25.config.globalProperties.$router;
    const currentRoute = filterCurrentRoute(router == null ? void 0 : router.currentRoute.value);
    const routes = filterRoutes(getRoutes(router));
    const c = console.warn;
    console.warn = () => {
    };
    target[ROUTER_INFO_KEY] = {
      currentRoute: currentRoute ? deepClone(currentRoute) : {},
      routes: deepClone(routes)
    };
    target[ROUTER_KEY] = router;
    console.warn = c;
  }
  init();
  hook.on.componentUpdated(debounce(() => {
    var _a25;
    if (((_a25 = activeAppRecord2.value) == null ? void 0 : _a25.app) !== appRecord.app) return;
    init();
    if (devtoolsState.highPerfModeEnabled) return;
    devtoolsContext.hooks.callHook("routerInfoUpdated", {
      state: target[ROUTER_INFO_KEY]
    });
  }, 200));
}
function createDevToolsApi(hooks2) {
  return {
    // get inspector tree
    async getInspectorTree(payload) {
      const _payload = {
        ...payload,
        app: activeAppRecord.value.app,
        rootNodes: []
      };
      await new Promise((resolve) => {
        hooks2.callHookWith(
          async (callbacks) => {
            await Promise.all(callbacks.map((cb) => cb(_payload)));
            resolve();
          },
          "getInspectorTree"
          /* GET_INSPECTOR_TREE */
        );
      });
      return _payload.rootNodes;
    },
    // get inspector state
    async getInspectorState(payload) {
      const _payload = {
        ...payload,
        app: activeAppRecord.value.app,
        state: null
      };
      const ctx = {
        currentTab: `custom-inspector:${payload.inspectorId}`
      };
      await new Promise((resolve) => {
        hooks2.callHookWith(
          async (callbacks) => {
            await Promise.all(callbacks.map((cb) => cb(_payload, ctx)));
            resolve();
          },
          "getInspectorState"
          /* GET_INSPECTOR_STATE */
        );
      });
      return _payload.state;
    },
    // edit inspector state
    editInspectorState(payload) {
      const stateEditor2 = new StateEditor();
      const _payload = {
        ...payload,
        app: activeAppRecord.value.app,
        set: (obj, path = payload.path, value = payload.state.value, cb) => {
          stateEditor2.set(obj, path, value, cb || stateEditor2.createDefaultSetCallback(payload.state));
        }
      };
      hooks2.callHookWith(
        (callbacks) => {
          callbacks.forEach((cb) => cb(_payload));
        },
        "editInspectorState"
        /* EDIT_INSPECTOR_STATE */
      );
    },
    // send inspector state
    sendInspectorState(inspectorId) {
      const inspector = getInspector(inspectorId);
      hooks2.callHook("sendInspectorState", {
        inspectorId,
        plugin: {
          descriptor: inspector.descriptor,
          setupFn: () => ({})
        }
      });
    },
    // inspect component inspector
    inspectComponentInspector() {
      return inspectComponentHighLighter();
    },
    // cancel inspect component inspector
    cancelInspectComponentInspector() {
      return cancelInspectComponentHighLighter();
    },
    // get component render code
    getComponentRenderCode(id) {
      const instance = getComponentInstance(activeAppRecord.value, id);
      if (instance) return !(typeof (instance == null ? void 0 : instance.type) === "function") ? instance.render.toString() : instance.type.toString();
    },
    // scroll to component
    scrollToComponent(id) {
      return scrollToComponent({
        id
      });
    },
    // open in editor
    openInEditor,
    // get vue inspector
    getVueInspector: getComponentInspector,
    // toggle app
    toggleApp(id, options) {
      const appRecord = devtoolsAppRecords.value.find((record) => record.id === id);
      if (appRecord) {
        setActiveAppRecordId(id);
        setActiveAppRecord(appRecord);
        normalizeRouterInfo(appRecord, activeAppRecord);
        callInspectorUpdatedHook();
        registerDevToolsPlugin(appRecord.app, options);
      }
    },
    // inspect dom
    inspectDOM(instanceId) {
      const instance = getComponentInstance(activeAppRecord.value, instanceId);
      if (instance) {
        const [el] = getRootElementsFromComponentInstance(instance);
        if (el) {
          target.__VUE_DEVTOOLS_INSPECT_DOM_TARGET__ = el;
        }
      }
    },
    updatePluginSettings(pluginId, key, value) {
      setPluginSettings(pluginId, key, value);
    },
    getPluginSettings(pluginId) {
      return {
        options: getPluginSettingsOptions(pluginId),
        values: getPluginSettings(pluginId)
      };
    }
  };
}
init_esm_shims2();
var _a15;
var _b15;
(_b15 = (_a15 = target).__VUE_DEVTOOLS_ENV__) != null ? _b15 : _a15.__VUE_DEVTOOLS_ENV__ = {
  vitePluginDetected: false
};
var hooks = createDevToolsCtxHooks();
var _a16;
var _b16;
(_b16 = (_a16 = target).__VUE_DEVTOOLS_KIT_CONTEXT__) != null ? _b16 : _a16.__VUE_DEVTOOLS_KIT_CONTEXT__ = {
  hooks,
  get state() {
    return {
      ...devtoolsState,
      activeAppRecordId: activeAppRecord.id,
      activeAppRecord: activeAppRecord.value,
      appRecords: devtoolsAppRecords.value
    };
  },
  api: createDevToolsApi(hooks)
};
var devtoolsContext = target.__VUE_DEVTOOLS_KIT_CONTEXT__;
init_esm_shims2();
var import_speakingurl = __toESM2(require_speakingurl2(), 1);
var _a17;
var _b17;
var appRecordInfo = (_b17 = (_a17 = target).__VUE_DEVTOOLS_NEXT_APP_RECORD_INFO__) != null ? _b17 : _a17.__VUE_DEVTOOLS_NEXT_APP_RECORD_INFO__ = {
  id: 0,
  appIds: /* @__PURE__ */ new Set()
};
init_esm_shims2();
init_esm_shims2();
function toggleHighPerfMode(state) {
  devtoolsState.highPerfModeEnabled = state != null ? state : !devtoolsState.highPerfModeEnabled;
  if (!state && activeAppRecord.value) {
    registerDevToolsPlugin(activeAppRecord.value.app);
  }
}
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
function updateDevToolsClientDetected(params) {
  devtoolsState.devtoolsClientDetected = {
    ...devtoolsState.devtoolsClientDetected,
    ...params
  };
  const devtoolsClientVisible = Object.values(devtoolsState.devtoolsClientDetected).some(Boolean);
  toggleHighPerfMode(!devtoolsClientVisible);
}
var _a18;
var _b18;
(_b18 = (_a18 = target).__VUE_DEVTOOLS_UPDATE_CLIENT_DETECTED__) != null ? _b18 : _a18.__VUE_DEVTOOLS_UPDATE_CLIENT_DETECTED__ = updateDevToolsClientDetected;
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
var DoubleIndexedKV = class {
  constructor() {
    this.keyToValue = /* @__PURE__ */ new Map();
    this.valueToKey = /* @__PURE__ */ new Map();
  }
  set(key, value) {
    this.keyToValue.set(key, value);
    this.valueToKey.set(value, key);
  }
  getByKey(key) {
    return this.keyToValue.get(key);
  }
  getByValue(value) {
    return this.valueToKey.get(value);
  }
  clear() {
    this.keyToValue.clear();
    this.valueToKey.clear();
  }
};
var Registry = class {
  constructor(generateIdentifier) {
    this.generateIdentifier = generateIdentifier;
    this.kv = new DoubleIndexedKV();
  }
  register(value, identifier) {
    if (this.kv.getByValue(value)) {
      return;
    }
    if (!identifier) {
      identifier = this.generateIdentifier(value);
    }
    this.kv.set(identifier, value);
  }
  clear() {
    this.kv.clear();
  }
  getIdentifier(value) {
    return this.kv.getByValue(value);
  }
  getValue(identifier) {
    return this.kv.getByKey(identifier);
  }
};
var ClassRegistry = class extends Registry {
  constructor() {
    super((c) => c.name);
    this.classToAllowedProps = /* @__PURE__ */ new Map();
  }
  register(value, options) {
    if (typeof options === "object") {
      if (options.allowProps) {
        this.classToAllowedProps.set(value, options.allowProps);
      }
      super.register(value, options.identifier);
    } else {
      super.register(value, options);
    }
  }
  getAllowedProps(value) {
    return this.classToAllowedProps.get(value);
  }
};
init_esm_shims2();
init_esm_shims2();
function valuesOfObj(record) {
  if ("values" in Object) {
    return Object.values(record);
  }
  const values = [];
  for (const key in record) {
    if (record.hasOwnProperty(key)) {
      values.push(record[key]);
    }
  }
  return values;
}
function find(record, predicate) {
  const values = valuesOfObj(record);
  if ("find" in values) {
    return values.find(predicate);
  }
  const valuesNotNever = values;
  for (let i = 0; i < valuesNotNever.length; i++) {
    const value = valuesNotNever[i];
    if (predicate(value)) {
      return value;
    }
  }
  return void 0;
}
function forEach(record, run) {
  Object.entries(record).forEach(([key, value]) => run(value, key));
}
function includes(arr, value) {
  return arr.indexOf(value) !== -1;
}
function findArr(record, predicate) {
  for (let i = 0; i < record.length; i++) {
    const value = record[i];
    if (predicate(value)) {
      return value;
    }
  }
  return void 0;
}
var CustomTransformerRegistry = class {
  constructor() {
    this.transfomers = {};
  }
  register(transformer) {
    this.transfomers[transformer.name] = transformer;
  }
  findApplicable(v) {
    return find(this.transfomers, (transformer) => transformer.isApplicable(v));
  }
  findByName(name) {
    return this.transfomers[name];
  }
};
init_esm_shims2();
init_esm_shims2();
var getType2 = (payload) => Object.prototype.toString.call(payload).slice(8, -1);
var isUndefined = (payload) => typeof payload === "undefined";
var isNull = (payload) => payload === null;
var isPlainObject2 = (payload) => {
  if (typeof payload !== "object" || payload === null) return false;
  if (payload === Object.prototype) return false;
  if (Object.getPrototypeOf(payload) === null) return true;
  return Object.getPrototypeOf(payload) === Object.prototype;
};
var isEmptyObject = (payload) => isPlainObject2(payload) && Object.keys(payload).length === 0;
var isArray2 = (payload) => Array.isArray(payload);
var isString2 = (payload) => typeof payload === "string";
var isNumber = (payload) => typeof payload === "number" && !isNaN(payload);
var isBoolean2 = (payload) => typeof payload === "boolean";
var isRegExp2 = (payload) => payload instanceof RegExp;
var isMap2 = (payload) => payload instanceof Map;
var isSet2 = (payload) => payload instanceof Set;
var isSymbol2 = (payload) => getType2(payload) === "Symbol";
var isDate = (payload) => payload instanceof Date && !isNaN(payload.valueOf());
var isError = (payload) => payload instanceof Error;
var isNaNValue = (payload) => typeof payload === "number" && isNaN(payload);
var isPrimitive2 = (payload) => isBoolean2(payload) || isNull(payload) || isUndefined(payload) || isNumber(payload) || isString2(payload) || isSymbol2(payload);
var isBigint = (payload) => typeof payload === "bigint";
var isInfinite = (payload) => payload === Infinity || payload === -Infinity;
var isTypedArray = (payload) => ArrayBuffer.isView(payload) && !(payload instanceof DataView);
var isURL = (payload) => payload instanceof URL;
init_esm_shims2();
var escapeKey = (key) => key.replace(/\./g, "\\.");
var stringifyPath = (path) => path.map(String).map(escapeKey).join(".");
var parsePath = (string) => {
  const result = [];
  let segment = "";
  for (let i = 0; i < string.length; i++) {
    let char = string.charAt(i);
    const isEscapedDot = char === "\\" && string.charAt(i + 1) === ".";
    if (isEscapedDot) {
      segment += ".";
      i++;
      continue;
    }
    const isEndOfSegment = char === ".";
    if (isEndOfSegment) {
      result.push(segment);
      segment = "";
      continue;
    }
    segment += char;
  }
  const lastSegment = segment;
  result.push(lastSegment);
  return result;
};
init_esm_shims2();
function simpleTransformation(isApplicable, annotation, transform, untransform) {
  return {
    isApplicable,
    annotation,
    transform,
    untransform
  };
}
var simpleRules = [
  simpleTransformation(isUndefined, "undefined", () => null, () => void 0),
  simpleTransformation(isBigint, "bigint", (v) => v.toString(), (v) => {
    if (typeof BigInt !== "undefined") {
      return BigInt(v);
    }
    console.error("Please add a BigInt polyfill.");
    return v;
  }),
  simpleTransformation(isDate, "Date", (v) => v.toISOString(), (v) => new Date(v)),
  simpleTransformation(isError, "Error", (v, superJson) => {
    const baseError = {
      name: v.name,
      message: v.message
    };
    superJson.allowedErrorProps.forEach((prop) => {
      baseError[prop] = v[prop];
    });
    return baseError;
  }, (v, superJson) => {
    const e = new Error(v.message);
    e.name = v.name;
    e.stack = v.stack;
    superJson.allowedErrorProps.forEach((prop) => {
      e[prop] = v[prop];
    });
    return e;
  }),
  simpleTransformation(isRegExp2, "regexp", (v) => "" + v, (regex) => {
    const body = regex.slice(1, regex.lastIndexOf("/"));
    const flags = regex.slice(regex.lastIndexOf("/") + 1);
    return new RegExp(body, flags);
  }),
  simpleTransformation(
    isSet2,
    "set",
    // (sets only exist in es6+)
    // eslint-disable-next-line es5/no-es6-methods
    (v) => [
      ...v.values()
    ],
    (v) => new Set(v)
  ),
  simpleTransformation(isMap2, "map", (v) => [
    ...v.entries()
  ], (v) => new Map(v)),
  simpleTransformation((v) => isNaNValue(v) || isInfinite(v), "number", (v) => {
    if (isNaNValue(v)) {
      return "NaN";
    }
    if (v > 0) {
      return "Infinity";
    } else {
      return "-Infinity";
    }
  }, Number),
  simpleTransformation((v) => v === 0 && 1 / v === -Infinity, "number", () => {
    return "-0";
  }, Number),
  simpleTransformation(isURL, "URL", (v) => v.toString(), (v) => new URL(v))
];
function compositeTransformation(isApplicable, annotation, transform, untransform) {
  return {
    isApplicable,
    annotation,
    transform,
    untransform
  };
}
var symbolRule = compositeTransformation((s, superJson) => {
  if (isSymbol2(s)) {
    const isRegistered = !!superJson.symbolRegistry.getIdentifier(s);
    return isRegistered;
  }
  return false;
}, (s, superJson) => {
  const identifier = superJson.symbolRegistry.getIdentifier(s);
  return [
    "symbol",
    identifier
  ];
}, (v) => v.description, (_, a, superJson) => {
  const value = superJson.symbolRegistry.getValue(a[1]);
  if (!value) {
    throw new Error("Trying to deserialize unknown symbol");
  }
  return value;
});
var constructorToName = [
  Int8Array,
  Uint8Array,
  Int16Array,
  Uint16Array,
  Int32Array,
  Uint32Array,
  Float32Array,
  Float64Array,
  Uint8ClampedArray
].reduce((obj, ctor) => {
  obj[ctor.name] = ctor;
  return obj;
}, {});
var typedArrayRule = compositeTransformation(isTypedArray, (v) => [
  "typed-array",
  v.constructor.name
], (v) => [
  ...v
], (v, a) => {
  const ctor = constructorToName[a[1]];
  if (!ctor) {
    throw new Error("Trying to deserialize unknown typed array");
  }
  return new ctor(v);
});
function isInstanceOfRegisteredClass(potentialClass, superJson) {
  if (potentialClass == null ? void 0 : potentialClass.constructor) {
    const isRegistered = !!superJson.classRegistry.getIdentifier(potentialClass.constructor);
    return isRegistered;
  }
  return false;
}
var classRule = compositeTransformation(isInstanceOfRegisteredClass, (clazz, superJson) => {
  const identifier = superJson.classRegistry.getIdentifier(clazz.constructor);
  return [
    "class",
    identifier
  ];
}, (clazz, superJson) => {
  const allowedProps = superJson.classRegistry.getAllowedProps(clazz.constructor);
  if (!allowedProps) {
    return {
      ...clazz
    };
  }
  const result = {};
  allowedProps.forEach((prop) => {
    result[prop] = clazz[prop];
  });
  return result;
}, (v, a, superJson) => {
  const clazz = superJson.classRegistry.getValue(a[1]);
  if (!clazz) {
    throw new Error(`Trying to deserialize unknown class '${a[1]}' - check https://github.com/blitz-js/superjson/issues/116#issuecomment-773996564`);
  }
  return Object.assign(Object.create(clazz.prototype), v);
});
var customRule = compositeTransformation((value, superJson) => {
  return !!superJson.customTransformerRegistry.findApplicable(value);
}, (value, superJson) => {
  const transformer = superJson.customTransformerRegistry.findApplicable(value);
  return [
    "custom",
    transformer.name
  ];
}, (value, superJson) => {
  const transformer = superJson.customTransformerRegistry.findApplicable(value);
  return transformer.serialize(value);
}, (v, a, superJson) => {
  const transformer = superJson.customTransformerRegistry.findByName(a[1]);
  if (!transformer) {
    throw new Error("Trying to deserialize unknown custom value");
  }
  return transformer.deserialize(v);
});
var compositeRules = [
  classRule,
  symbolRule,
  customRule,
  typedArrayRule
];
var transformValue = (value, superJson) => {
  const applicableCompositeRule = findArr(compositeRules, (rule) => rule.isApplicable(value, superJson));
  if (applicableCompositeRule) {
    return {
      value: applicableCompositeRule.transform(value, superJson),
      type: applicableCompositeRule.annotation(value, superJson)
    };
  }
  const applicableSimpleRule = findArr(simpleRules, (rule) => rule.isApplicable(value, superJson));
  if (applicableSimpleRule) {
    return {
      value: applicableSimpleRule.transform(value, superJson),
      type: applicableSimpleRule.annotation
    };
  }
  return void 0;
};
var simpleRulesByAnnotation = {};
simpleRules.forEach((rule) => {
  simpleRulesByAnnotation[rule.annotation] = rule;
});
var untransformValue = (json, type, superJson) => {
  if (isArray2(type)) {
    switch (type[0]) {
      case "symbol":
        return symbolRule.untransform(json, type, superJson);
      case "class":
        return classRule.untransform(json, type, superJson);
      case "custom":
        return customRule.untransform(json, type, superJson);
      case "typed-array":
        return typedArrayRule.untransform(json, type, superJson);
      default:
        throw new Error("Unknown transformation: " + type);
    }
  } else {
    const transformation = simpleRulesByAnnotation[type];
    if (!transformation) {
      throw new Error("Unknown transformation: " + type);
    }
    return transformation.untransform(json, superJson);
  }
};
init_esm_shims2();
var getNthKey = (value, n) => {
  if (n > value.size) throw new Error("index out of bounds");
  const keys = value.keys();
  while (n > 0) {
    keys.next();
    n--;
  }
  return keys.next().value;
};
function validatePath(path) {
  if (includes(path, "__proto__")) {
    throw new Error("__proto__ is not allowed as a property");
  }
  if (includes(path, "prototype")) {
    throw new Error("prototype is not allowed as a property");
  }
  if (includes(path, "constructor")) {
    throw new Error("constructor is not allowed as a property");
  }
}
var getDeep = (object, path) => {
  validatePath(path);
  for (let i = 0; i < path.length; i++) {
    const key = path[i];
    if (isSet2(object)) {
      object = getNthKey(object, +key);
    } else if (isMap2(object)) {
      const row = +key;
      const type = +path[++i] === 0 ? "key" : "value";
      const keyOfRow = getNthKey(object, row);
      switch (type) {
        case "key":
          object = keyOfRow;
          break;
        case "value":
          object = object.get(keyOfRow);
          break;
      }
    } else {
      object = object[key];
    }
  }
  return object;
};
var setDeep = (object, path, mapper) => {
  validatePath(path);
  if (path.length === 0) {
    return mapper(object);
  }
  let parent = object;
  for (let i = 0; i < path.length - 1; i++) {
    const key = path[i];
    if (isArray2(parent)) {
      const index = +key;
      parent = parent[index];
    } else if (isPlainObject2(parent)) {
      parent = parent[key];
    } else if (isSet2(parent)) {
      const row = +key;
      parent = getNthKey(parent, row);
    } else if (isMap2(parent)) {
      const isEnd = i === path.length - 2;
      if (isEnd) {
        break;
      }
      const row = +key;
      const type = +path[++i] === 0 ? "key" : "value";
      const keyOfRow = getNthKey(parent, row);
      switch (type) {
        case "key":
          parent = keyOfRow;
          break;
        case "value":
          parent = parent.get(keyOfRow);
          break;
      }
    }
  }
  const lastKey = path[path.length - 1];
  if (isArray2(parent)) {
    parent[+lastKey] = mapper(parent[+lastKey]);
  } else if (isPlainObject2(parent)) {
    parent[lastKey] = mapper(parent[lastKey]);
  }
  if (isSet2(parent)) {
    const oldValue = getNthKey(parent, +lastKey);
    const newValue = mapper(oldValue);
    if (oldValue !== newValue) {
      parent.delete(oldValue);
      parent.add(newValue);
    }
  }
  if (isMap2(parent)) {
    const row = +path[path.length - 2];
    const keyToRow = getNthKey(parent, row);
    const type = +lastKey === 0 ? "key" : "value";
    switch (type) {
      case "key": {
        const newKey = mapper(keyToRow);
        parent.set(newKey, parent.get(keyToRow));
        if (newKey !== keyToRow) {
          parent.delete(keyToRow);
        }
        break;
      }
      case "value": {
        parent.set(keyToRow, mapper(parent.get(keyToRow)));
        break;
      }
    }
  }
  return object;
};
function traverse2(tree, walker2, origin = []) {
  if (!tree) {
    return;
  }
  if (!isArray2(tree)) {
    forEach(tree, (subtree, key) => traverse2(subtree, walker2, [
      ...origin,
      ...parsePath(key)
    ]));
    return;
  }
  const [nodeValue, children] = tree;
  if (children) {
    forEach(children, (child, key) => {
      traverse2(child, walker2, [
        ...origin,
        ...parsePath(key)
      ]);
    });
  }
  walker2(nodeValue, origin);
}
function applyValueAnnotations(plain, annotations, superJson) {
  traverse2(annotations, (type, path) => {
    plain = setDeep(plain, path, (v) => untransformValue(v, type, superJson));
  });
  return plain;
}
function applyReferentialEqualityAnnotations(plain, annotations) {
  function apply2(identicalPaths, path) {
    const object = getDeep(plain, parsePath(path));
    identicalPaths.map(parsePath).forEach((identicalObjectPath) => {
      plain = setDeep(plain, identicalObjectPath, () => object);
    });
  }
  if (isArray2(annotations)) {
    const [root, other] = annotations;
    root.forEach((identicalPath) => {
      plain = setDeep(plain, parsePath(identicalPath), () => plain);
    });
    if (other) {
      forEach(other, apply2);
    }
  } else {
    forEach(annotations, apply2);
  }
  return plain;
}
var isDeep = (object, superJson) => isPlainObject2(object) || isArray2(object) || isMap2(object) || isSet2(object) || isInstanceOfRegisteredClass(object, superJson);
function addIdentity(object, path, identities) {
  const existingSet = identities.get(object);
  if (existingSet) {
    existingSet.push(path);
  } else {
    identities.set(object, [
      path
    ]);
  }
}
function generateReferentialEqualityAnnotations(identitites, dedupe) {
  const result = {};
  let rootEqualityPaths = void 0;
  identitites.forEach((paths) => {
    if (paths.length <= 1) {
      return;
    }
    if (!dedupe) {
      paths = paths.map((path) => path.map(String)).sort((a, b) => a.length - b.length);
    }
    const [representativePath, ...identicalPaths] = paths;
    if (representativePath.length === 0) {
      rootEqualityPaths = identicalPaths.map(stringifyPath);
    } else {
      result[stringifyPath(representativePath)] = identicalPaths.map(stringifyPath);
    }
  });
  if (rootEqualityPaths) {
    if (isEmptyObject(result)) {
      return [
        rootEqualityPaths
      ];
    } else {
      return [
        rootEqualityPaths,
        result
      ];
    }
  } else {
    return isEmptyObject(result) ? void 0 : result;
  }
}
var walker = (object, identities, superJson, dedupe, path = [], objectsInThisPath = [], seenObjects = /* @__PURE__ */ new Map()) => {
  var _a25;
  const primitive = isPrimitive2(object);
  if (!primitive) {
    addIdentity(object, path, identities);
    const seen = seenObjects.get(object);
    if (seen) {
      return dedupe ? {
        transformedValue: null
      } : seen;
    }
  }
  if (!isDeep(object, superJson)) {
    const transformed2 = transformValue(object, superJson);
    const result2 = transformed2 ? {
      transformedValue: transformed2.value,
      annotations: [
        transformed2.type
      ]
    } : {
      transformedValue: object
    };
    if (!primitive) {
      seenObjects.set(object, result2);
    }
    return result2;
  }
  if (includes(objectsInThisPath, object)) {
    return {
      transformedValue: null
    };
  }
  const transformationResult = transformValue(object, superJson);
  const transformed = (_a25 = transformationResult == null ? void 0 : transformationResult.value) != null ? _a25 : object;
  const transformedValue = isArray2(transformed) ? [] : {};
  const innerAnnotations = {};
  forEach(transformed, (value, index) => {
    if (index === "__proto__" || index === "constructor" || index === "prototype") {
      throw new Error(`Detected property ${index}. This is a prototype pollution risk, please remove it from your object.`);
    }
    const recursiveResult = walker(value, identities, superJson, dedupe, [
      ...path,
      index
    ], [
      ...objectsInThisPath,
      object
    ], seenObjects);
    transformedValue[index] = recursiveResult.transformedValue;
    if (isArray2(recursiveResult.annotations)) {
      innerAnnotations[index] = recursiveResult.annotations;
    } else if (isPlainObject2(recursiveResult.annotations)) {
      forEach(recursiveResult.annotations, (tree, key) => {
        innerAnnotations[escapeKey(index) + "." + key] = tree;
      });
    }
  });
  const result = isEmptyObject(innerAnnotations) ? {
    transformedValue,
    annotations: !!transformationResult ? [
      transformationResult.type
    ] : void 0
  } : {
    transformedValue,
    annotations: !!transformationResult ? [
      transformationResult.type,
      innerAnnotations
    ] : innerAnnotations
  };
  if (!primitive) {
    seenObjects.set(object, result);
  }
  return result;
};
init_esm_shims2();
init_esm_shims2();
function getType22(payload) {
  return Object.prototype.toString.call(payload).slice(8, -1);
}
function isArray22(payload) {
  return getType22(payload) === "Array";
}
function isPlainObject3(payload) {
  if (getType22(payload) !== "Object") return false;
  const prototype = Object.getPrototypeOf(payload);
  return !!prototype && prototype.constructor === Object && prototype === Object.prototype;
}
function isNull2(payload) {
  return getType22(payload) === "Null";
}
function isOneOf(a, b, c, d, e) {
  return (value) => a(value) || b(value) || !!c && c(value) || !!d && d(value) || !!e && e(value);
}
function isUndefined2(payload) {
  return getType22(payload) === "Undefined";
}
var isNullOrUndefined = isOneOf(isNull2, isUndefined2);
function assignProp(carry, key, newVal, originalObject, includeNonenumerable) {
  const propType = {}.propertyIsEnumerable.call(originalObject, key) ? "enumerable" : "nonenumerable";
  if (propType === "enumerable") carry[key] = newVal;
  if (includeNonenumerable && propType === "nonenumerable") {
    Object.defineProperty(carry, key, {
      value: newVal,
      enumerable: false,
      writable: true,
      configurable: true
    });
  }
}
function copy(target22, options = {}) {
  if (isArray22(target22)) {
    return target22.map((item) => copy(item, options));
  }
  if (!isPlainObject3(target22)) {
    return target22;
  }
  const props = Object.getOwnPropertyNames(target22);
  const symbols = Object.getOwnPropertySymbols(target22);
  return [
    ...props,
    ...symbols
  ].reduce((carry, key) => {
    if (isArray22(options.props) && !options.props.includes(key)) {
      return carry;
    }
    const val = target22[key];
    const newVal = copy(val, options);
    assignProp(carry, key, newVal, target22, options.nonenumerable);
    return carry;
  }, {});
}
var SuperJSON = class {
  /**
   * @param dedupeReferentialEqualities  If true, SuperJSON will make sure only one instance of referentially equal objects are serialized and the rest are replaced with `null`.
   */
  constructor({ dedupe = false } = {}) {
    this.classRegistry = new ClassRegistry();
    this.symbolRegistry = new Registry((s) => {
      var _a25;
      return (_a25 = s.description) != null ? _a25 : "";
    });
    this.customTransformerRegistry = new CustomTransformerRegistry();
    this.allowedErrorProps = [];
    this.dedupe = dedupe;
  }
  serialize(object) {
    const identities = /* @__PURE__ */ new Map();
    const output = walker(object, identities, this, this.dedupe);
    const res = {
      json: output.transformedValue
    };
    if (output.annotations) {
      res.meta = {
        ...res.meta,
        values: output.annotations
      };
    }
    const equalityAnnotations = generateReferentialEqualityAnnotations(identities, this.dedupe);
    if (equalityAnnotations) {
      res.meta = {
        ...res.meta,
        referentialEqualities: equalityAnnotations
      };
    }
    return res;
  }
  deserialize(payload) {
    const { json, meta } = payload;
    let result = copy(json);
    if (meta == null ? void 0 : meta.values) {
      result = applyValueAnnotations(result, meta.values, this);
    }
    if (meta == null ? void 0 : meta.referentialEqualities) {
      result = applyReferentialEqualityAnnotations(result, meta.referentialEqualities);
    }
    return result;
  }
  stringify(object) {
    return JSON.stringify(this.serialize(object));
  }
  parse(string) {
    return this.deserialize(JSON.parse(string));
  }
  registerClass(v, options) {
    this.classRegistry.register(v, options);
  }
  registerSymbol(v, identifier) {
    this.symbolRegistry.register(v, identifier);
  }
  registerCustom(transformer, name) {
    this.customTransformerRegistry.register({
      name,
      ...transformer
    });
  }
  allowErrorProps(...props) {
    this.allowedErrorProps.push(...props);
  }
};
SuperJSON.defaultInstance = new SuperJSON();
SuperJSON.serialize = SuperJSON.defaultInstance.serialize.bind(SuperJSON.defaultInstance);
SuperJSON.deserialize = SuperJSON.defaultInstance.deserialize.bind(SuperJSON.defaultInstance);
SuperJSON.stringify = SuperJSON.defaultInstance.stringify.bind(SuperJSON.defaultInstance);
SuperJSON.parse = SuperJSON.defaultInstance.parse.bind(SuperJSON.defaultInstance);
SuperJSON.registerClass = SuperJSON.defaultInstance.registerClass.bind(SuperJSON.defaultInstance);
SuperJSON.registerSymbol = SuperJSON.defaultInstance.registerSymbol.bind(SuperJSON.defaultInstance);
SuperJSON.registerCustom = SuperJSON.defaultInstance.registerCustom.bind(SuperJSON.defaultInstance);
SuperJSON.allowErrorProps = SuperJSON.defaultInstance.allowErrorProps.bind(SuperJSON.defaultInstance);
var serialize = SuperJSON.serialize;
var deserialize = SuperJSON.deserialize;
var stringify = SuperJSON.stringify;
var parse = SuperJSON.parse;
var registerClass = SuperJSON.registerClass;
var registerCustom = SuperJSON.registerCustom;
var registerSymbol = SuperJSON.registerSymbol;
var allowErrorProps = SuperJSON.allowErrorProps;
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
var _a19;
var _b19;
(_b19 = (_a19 = target).__VUE_DEVTOOLS_KIT_MESSAGE_CHANNELS__) != null ? _b19 : _a19.__VUE_DEVTOOLS_KIT_MESSAGE_CHANNELS__ = [];
var _a20;
var _b20;
(_b20 = (_a20 = target).__VUE_DEVTOOLS_KIT_RPC_CLIENT__) != null ? _b20 : _a20.__VUE_DEVTOOLS_KIT_RPC_CLIENT__ = null;
var _a21;
var _b21;
(_b21 = (_a21 = target).__VUE_DEVTOOLS_KIT_RPC_SERVER__) != null ? _b21 : _a21.__VUE_DEVTOOLS_KIT_RPC_SERVER__ = null;
var _a222;
var _b22;
(_b22 = (_a222 = target).__VUE_DEVTOOLS_KIT_VITE_RPC_CLIENT__) != null ? _b22 : _a222.__VUE_DEVTOOLS_KIT_VITE_RPC_CLIENT__ = null;
var _a23;
var _b23;
(_b23 = (_a23 = target).__VUE_DEVTOOLS_KIT_VITE_RPC_SERVER__) != null ? _b23 : _a23.__VUE_DEVTOOLS_KIT_VITE_RPC_SERVER__ = null;
var _a24;
var _b24;
(_b24 = (_a24 = target).__VUE_DEVTOOLS_KIT_BROADCAST_RPC_SERVER__) != null ? _b24 : _a24.__VUE_DEVTOOLS_KIT_BROADCAST_RPC_SERVER__ = null;
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
init_esm_shims2();
var MAX_SERIALIZED_SIZE = 2 * 1024 * 1024;

// node_modules/.deno/pinia@3.0.3/node_modules/pinia/dist/pinia.mjs
var activePinia;
var setActivePinia = (pinia) => activePinia = pinia;
var piniaSymbol = true ? Symbol("pinia") : (
  /* istanbul ignore next */
  Symbol()
);
function isPlainObject4(o) {
  return o && typeof o === "object" && Object.prototype.toString.call(o) === "[object Object]" && typeof o.toJSON !== "function";
}
var MutationType;
(function(MutationType2) {
  MutationType2["direct"] = "direct";
  MutationType2["patchObject"] = "patch object";
  MutationType2["patchFunction"] = "patch function";
})(MutationType || (MutationType = {}));
var IS_CLIENT = typeof window !== "undefined";
var _global = /* @__PURE__ */ (() => typeof window === "object" && window.window === window ? window : typeof self === "object" && self.self === self ? self : typeof global === "object" && global.global === global ? global : typeof globalThis === "object" ? globalThis : {
  HTMLElement: null
})();
function bom(blob, { autoBom = false } = {}) {
  if (autoBom && /^\s*(?:text\/\S*|application\/xml|\S*\/\S*\+xml)\s*;.*charset\s*=\s*utf-8/i.test(blob.type)) {
    return new Blob([
      String.fromCharCode(65279),
      blob
    ], {
      type: blob.type
    });
  }
  return blob;
}
function download(url, name, opts) {
  const xhr = new XMLHttpRequest();
  xhr.open("GET", url);
  xhr.responseType = "blob";
  xhr.onload = function() {
    saveAs(xhr.response, name, opts);
  };
  xhr.onerror = function() {
    console.error("could not download file");
  };
  xhr.send();
}
function corsEnabled(url) {
  const xhr = new XMLHttpRequest();
  xhr.open("HEAD", url, false);
  try {
    xhr.send();
  } catch (e) {
  }
  return xhr.status >= 200 && xhr.status <= 299;
}
function click(node) {
  try {
    node.dispatchEvent(new MouseEvent("click"));
  } catch (e) {
    const evt = new MouseEvent("click", {
      bubbles: true,
      cancelable: true,
      view: window,
      detail: 0,
      screenX: 80,
      screenY: 20,
      clientX: 80,
      clientY: 20,
      ctrlKey: false,
      altKey: false,
      shiftKey: false,
      metaKey: false,
      button: 0,
      relatedTarget: null
    });
    node.dispatchEvent(evt);
  }
}
var _navigator = typeof navigator === "object" ? navigator : {
  userAgent: ""
};
var isMacOSWebView = /* @__PURE__ */ (() => /Macintosh/.test(_navigator.userAgent) && /AppleWebKit/.test(_navigator.userAgent) && !/Safari/.test(_navigator.userAgent))();
var saveAs = !IS_CLIENT ? () => {
} : typeof HTMLAnchorElement !== "undefined" && "download" in HTMLAnchorElement.prototype && !isMacOSWebView ? downloadSaveAs : "msSaveOrOpenBlob" in _navigator ? msSaveAs : fileSaverSaveAs;
function downloadSaveAs(blob, name = "download", opts) {
  const a = document.createElement("a");
  a.download = name;
  a.rel = "noopener";
  if (typeof blob === "string") {
    a.href = blob;
    if (a.origin !== location.origin) {
      if (corsEnabled(a.href)) {
        download(blob, name, opts);
      } else {
        a.target = "_blank";
        click(a);
      }
    } else {
      click(a);
    }
  } else {
    a.href = URL.createObjectURL(blob);
    setTimeout(function() {
      URL.revokeObjectURL(a.href);
    }, 4e4);
    setTimeout(function() {
      click(a);
    }, 0);
  }
}
function msSaveAs(blob, name = "download", opts) {
  if (typeof blob === "string") {
    if (corsEnabled(blob)) {
      download(blob, name, opts);
    } else {
      const a = document.createElement("a");
      a.href = blob;
      a.target = "_blank";
      setTimeout(function() {
        click(a);
      });
    }
  } else {
    navigator.msSaveOrOpenBlob(bom(blob, opts), name);
  }
}
function fileSaverSaveAs(blob, name, opts, popup) {
  popup = popup || open("", "_blank");
  if (popup) {
    popup.document.title = popup.document.body.innerText = "downloading...";
  }
  if (typeof blob === "string") return download(blob, name, opts);
  const force = blob.type === "application/octet-stream";
  const isSafari = /constructor/i.test(String(_global.HTMLElement)) || "safari" in _global;
  const isChromeIOS = /CriOS\/[\d]+/.test(navigator.userAgent);
  if ((isChromeIOS || force && isSafari || isMacOSWebView) && typeof FileReader !== "undefined") {
    const reader = new FileReader();
    reader.onloadend = function() {
      let url = reader.result;
      if (typeof url !== "string") {
        popup = null;
        throw new Error("Wrong reader.result type");
      }
      url = isChromeIOS ? url : url.replace(/^data:[^;]*;/, "data:attachment/file;");
      if (popup) {
        popup.location.href = url;
      } else {
        location.assign(url);
      }
      popup = null;
    };
    reader.readAsDataURL(blob);
  } else {
    const url = URL.createObjectURL(blob);
    if (popup) popup.location.assign(url);
    else location.href = url;
    popup = null;
    setTimeout(function() {
      URL.revokeObjectURL(url);
    }, 4e4);
  }
}
function toastMessage(message, type) {
  const piniaMessage = "\u{1F34D} " + message;
  if (typeof __VUE_DEVTOOLS_TOAST__ === "function") {
    __VUE_DEVTOOLS_TOAST__(piniaMessage, type);
  } else if (type === "error") {
    console.error(piniaMessage);
  } else if (type === "warn") {
    console.warn(piniaMessage);
  } else {
    console.log(piniaMessage);
  }
}
function isPinia(o) {
  return "_a" in o && "install" in o;
}
function checkClipboardAccess() {
  if (!("clipboard" in navigator)) {
    toastMessage(`Your browser doesn't support the Clipboard API`, "error");
    return true;
  }
}
function checkNotFocusedError(error) {
  if (error instanceof Error && error.message.toLowerCase().includes("document is not focused")) {
    toastMessage('You need to activate the "Emulate a focused page" setting in the "Rendering" panel of devtools.', "warn");
    return true;
  }
  return false;
}
async function actionGlobalCopyState(pinia) {
  if (checkClipboardAccess()) return;
  try {
    await navigator.clipboard.writeText(JSON.stringify(pinia.state.value));
    toastMessage("Global state copied to clipboard.");
  } catch (error) {
    if (checkNotFocusedError(error)) return;
    toastMessage(`Failed to serialize the state. Check the console for more details.`, "error");
    console.error(error);
  }
}
async function actionGlobalPasteState(pinia) {
  if (checkClipboardAccess()) return;
  try {
    loadStoresState(pinia, JSON.parse(await navigator.clipboard.readText()));
    toastMessage("Global state pasted from clipboard.");
  } catch (error) {
    if (checkNotFocusedError(error)) return;
    toastMessage(`Failed to deserialize the state from clipboard. Check the console for more details.`, "error");
    console.error(error);
  }
}
async function actionGlobalSaveState(pinia) {
  try {
    saveAs(new Blob([
      JSON.stringify(pinia.state.value)
    ], {
      type: "text/plain;charset=utf-8"
    }), "pinia-state.json");
  } catch (error) {
    toastMessage(`Failed to export the state as JSON. Check the console for more details.`, "error");
    console.error(error);
  }
}
var fileInput;
function getFileOpener() {
  if (!fileInput) {
    fileInput = document.createElement("input");
    fileInput.type = "file";
    fileInput.accept = ".json";
  }
  function openFile() {
    return new Promise((resolve, reject) => {
      fileInput.onchange = async () => {
        const files = fileInput.files;
        if (!files) return resolve(null);
        const file = files.item(0);
        if (!file) return resolve(null);
        return resolve({
          text: await file.text(),
          file
        });
      };
      fileInput.oncancel = () => resolve(null);
      fileInput.onerror = reject;
      fileInput.click();
    });
  }
  return openFile;
}
async function actionGlobalOpenStateFile(pinia) {
  try {
    const open2 = getFileOpener();
    const result = await open2();
    if (!result) return;
    const { text, file } = result;
    loadStoresState(pinia, JSON.parse(text));
    toastMessage(`Global state imported from "${file.name}".`);
  } catch (error) {
    toastMessage(`Failed to import the state from JSON. Check the console for more details.`, "error");
    console.error(error);
  }
}
function loadStoresState(pinia, state) {
  for (const key in state) {
    const storeState = pinia.state.value[key];
    if (storeState) {
      Object.assign(storeState, state[key]);
    } else {
      pinia.state.value[key] = state[key];
    }
  }
}
function formatDisplay(display) {
  return {
    _custom: {
      display
    }
  };
}
var PINIA_ROOT_LABEL = "\u{1F34D} Pinia (root)";
var PINIA_ROOT_ID = "_root";
function formatStoreForInspectorTree(store) {
  return isPinia(store) ? {
    id: PINIA_ROOT_ID,
    label: PINIA_ROOT_LABEL
  } : {
    id: store.$id,
    label: store.$id
  };
}
function formatStoreForInspectorState(store) {
  if (isPinia(store)) {
    const storeNames = Array.from(store._s.keys());
    const storeMap = store._s;
    const state2 = {
      state: storeNames.map((storeId) => ({
        editable: true,
        key: storeId,
        value: store.state.value[storeId]
      })),
      getters: storeNames.filter((id) => storeMap.get(id)._getters).map((id) => {
        const store2 = storeMap.get(id);
        return {
          editable: false,
          key: id,
          value: store2._getters.reduce((getters, key) => {
            getters[key] = store2[key];
            return getters;
          }, {})
        };
      })
    };
    return state2;
  }
  const state = {
    state: Object.keys(store.$state).map((key) => ({
      editable: true,
      key,
      value: store.$state[key]
    }))
  };
  if (store._getters && store._getters.length) {
    state.getters = store._getters.map((getterName) => ({
      editable: false,
      key: getterName,
      value: store[getterName]
    }));
  }
  if (store._customProperties.size) {
    state.customProperties = Array.from(store._customProperties).map((key) => ({
      editable: true,
      key,
      value: store[key]
    }));
  }
  return state;
}
function formatEventData(events) {
  if (!events) return {};
  if (Array.isArray(events)) {
    return events.reduce((data, event) => {
      data.keys.push(event.key);
      data.operations.push(event.type);
      data.oldValue[event.key] = event.oldValue;
      data.newValue[event.key] = event.newValue;
      return data;
    }, {
      oldValue: {},
      keys: [],
      operations: [],
      newValue: {}
    });
  } else {
    return {
      operation: formatDisplay(events.type),
      key: formatDisplay(events.key),
      oldValue: events.oldValue,
      newValue: events.newValue
    };
  }
}
function formatMutationType(type) {
  switch (type) {
    case MutationType.direct:
      return "mutation";
    case MutationType.patchFunction:
      return "$patch";
    case MutationType.patchObject:
      return "$patch";
    default:
      return "unknown";
  }
}
var isTimelineActive = true;
var componentStateTypes = [];
var MUTATIONS_LAYER_ID = "pinia:mutations";
var INSPECTOR_ID = "pinia";
var { assign: assign$1 } = Object;
var getStoreType = (id) => "\u{1F34D} " + id;
function registerPiniaDevtools(app, pinia) {
  setupDevToolsPlugin({
    id: "dev.esm.pinia",
    label: "Pinia \u{1F34D}",
    logo: "https://pinia.vuejs.org/logo.svg",
    packageName: "pinia",
    homepage: "https://pinia.vuejs.org",
    componentStateTypes,
    app
  }, (api) => {
    if (typeof api.now !== "function") {
      toastMessage("You seem to be using an outdated version of Vue Devtools. Are you still using the Beta release instead of the stable one? You can find the links at https://devtools.vuejs.org/guide/installation.html.");
    }
    api.addTimelineLayer({
      id: MUTATIONS_LAYER_ID,
      label: `Pinia \u{1F34D}`,
      color: 15064968
    });
    api.addInspector({
      id: INSPECTOR_ID,
      label: "Pinia \u{1F34D}",
      icon: "storage",
      treeFilterPlaceholder: "Search stores",
      actions: [
        {
          icon: "content_copy",
          action: () => {
            actionGlobalCopyState(pinia);
          },
          tooltip: "Serialize and copy the state"
        },
        {
          icon: "content_paste",
          action: async () => {
            await actionGlobalPasteState(pinia);
            api.sendInspectorTree(INSPECTOR_ID);
            api.sendInspectorState(INSPECTOR_ID);
          },
          tooltip: "Replace the state with the content of your clipboard"
        },
        {
          icon: "save",
          action: () => {
            actionGlobalSaveState(pinia);
          },
          tooltip: "Save the state as a JSON file"
        },
        {
          icon: "folder_open",
          action: async () => {
            await actionGlobalOpenStateFile(pinia);
            api.sendInspectorTree(INSPECTOR_ID);
            api.sendInspectorState(INSPECTOR_ID);
          },
          tooltip: "Import the state from a JSON file"
        }
      ],
      nodeActions: [
        {
          icon: "restore",
          tooltip: 'Reset the state (with "$reset")',
          action: (nodeId) => {
            const store = pinia._s.get(nodeId);
            if (!store) {
              toastMessage(`Cannot reset "${nodeId}" store because it wasn't found.`, "warn");
            } else if (typeof store.$reset !== "function") {
              toastMessage(`Cannot reset "${nodeId}" store because it doesn't have a "$reset" method implemented.`, "warn");
            } else {
              store.$reset();
              toastMessage(`Store "${nodeId}" reset.`);
            }
          }
        }
      ]
    });
    api.on.inspectComponent((payload) => {
      const proxy = payload.componentInstance && payload.componentInstance.proxy;
      if (proxy && proxy._pStores) {
        const piniaStores = payload.componentInstance.proxy._pStores;
        Object.values(piniaStores).forEach((store) => {
          payload.instanceData.state.push({
            type: getStoreType(store.$id),
            key: "state",
            editable: true,
            value: store._isOptionsAPI ? {
              _custom: {
                value: toRaw(store.$state),
                actions: [
                  {
                    icon: "restore",
                    tooltip: "Reset the state of this store",
                    action: () => store.$reset()
                  }
                ]
              }
            } : Object.keys(store.$state).reduce((state, key) => {
              state[key] = store.$state[key];
              return state;
            }, {})
          });
          if (store._getters && store._getters.length) {
            payload.instanceData.state.push({
              type: getStoreType(store.$id),
              key: "getters",
              editable: false,
              value: store._getters.reduce((getters, key) => {
                try {
                  getters[key] = store[key];
                } catch (error) {
                  getters[key] = error;
                }
                return getters;
              }, {})
            });
          }
        });
      }
    });
    api.on.getInspectorTree((payload) => {
      if (payload.app === app && payload.inspectorId === INSPECTOR_ID) {
        let stores = [
          pinia
        ];
        stores = stores.concat(Array.from(pinia._s.values()));
        payload.rootNodes = (payload.filter ? stores.filter((store) => "$id" in store ? store.$id.toLowerCase().includes(payload.filter.toLowerCase()) : PINIA_ROOT_LABEL.toLowerCase().includes(payload.filter.toLowerCase())) : stores).map(formatStoreForInspectorTree);
      }
    });
    globalThis.$pinia = pinia;
    api.on.getInspectorState((payload) => {
      if (payload.app === app && payload.inspectorId === INSPECTOR_ID) {
        const inspectedStore = payload.nodeId === PINIA_ROOT_ID ? pinia : pinia._s.get(payload.nodeId);
        if (!inspectedStore) {
          return;
        }
        if (inspectedStore) {
          if (payload.nodeId !== PINIA_ROOT_ID) globalThis.$store = toRaw(inspectedStore);
          payload.state = formatStoreForInspectorState(inspectedStore);
        }
      }
    });
    api.on.editInspectorState((payload) => {
      if (payload.app === app && payload.inspectorId === INSPECTOR_ID) {
        const inspectedStore = payload.nodeId === PINIA_ROOT_ID ? pinia : pinia._s.get(payload.nodeId);
        if (!inspectedStore) {
          return toastMessage(`store "${payload.nodeId}" not found`, "error");
        }
        const { path } = payload;
        if (!isPinia(inspectedStore)) {
          if (path.length !== 1 || !inspectedStore._customProperties.has(path[0]) || path[0] in inspectedStore.$state) {
            path.unshift("$state");
          }
        } else {
          path.unshift("state");
        }
        isTimelineActive = false;
        payload.set(inspectedStore, path, payload.state.value);
        isTimelineActive = true;
      }
    });
    api.on.editComponentState((payload) => {
      if (payload.type.startsWith("\u{1F34D}")) {
        const storeId = payload.type.replace(/^🍍\s*/, "");
        const store = pinia._s.get(storeId);
        if (!store) {
          return toastMessage(`store "${storeId}" not found`, "error");
        }
        const { path } = payload;
        if (path[0] !== "state") {
          return toastMessage(`Invalid path for store "${storeId}":
${path}
Only state can be modified.`);
        }
        path[0] = "$state";
        isTimelineActive = false;
        payload.set(store, path, payload.state.value);
        isTimelineActive = true;
      }
    });
  });
}
function addStoreToDevtools(app, store) {
  if (!componentStateTypes.includes(getStoreType(store.$id))) {
    componentStateTypes.push(getStoreType(store.$id));
  }
  setupDevToolsPlugin({
    id: "dev.esm.pinia",
    label: "Pinia \u{1F34D}",
    logo: "https://pinia.vuejs.org/logo.svg",
    packageName: "pinia",
    homepage: "https://pinia.vuejs.org",
    componentStateTypes,
    app,
    settings: {
      logStoreChanges: {
        label: "Notify about new/deleted stores",
        type: "boolean",
        defaultValue: true
      }
    }
  }, (api) => {
    const now = typeof api.now === "function" ? api.now.bind(api) : Date.now;
    store.$onAction(({ after, onError, name, args }) => {
      const groupId = runningActionId++;
      api.addTimelineEvent({
        layerId: MUTATIONS_LAYER_ID,
        event: {
          time: now(),
          title: "\u{1F6EB} " + name,
          subtitle: "start",
          data: {
            store: formatDisplay(store.$id),
            action: formatDisplay(name),
            args
          },
          groupId
        }
      });
      after((result) => {
        activeAction = void 0;
        api.addTimelineEvent({
          layerId: MUTATIONS_LAYER_ID,
          event: {
            time: now(),
            title: "\u{1F6EC} " + name,
            subtitle: "end",
            data: {
              store: formatDisplay(store.$id),
              action: formatDisplay(name),
              args,
              result
            },
            groupId
          }
        });
      });
      onError((error) => {
        activeAction = void 0;
        api.addTimelineEvent({
          layerId: MUTATIONS_LAYER_ID,
          event: {
            time: now(),
            logType: "error",
            title: "\u{1F4A5} " + name,
            subtitle: "end",
            data: {
              store: formatDisplay(store.$id),
              action: formatDisplay(name),
              args,
              error
            },
            groupId
          }
        });
      });
    }, true);
    store._customProperties.forEach((name) => {
      watch2(() => unref(store[name]), (newValue, oldValue) => {
        api.notifyComponentUpdate();
        api.sendInspectorState(INSPECTOR_ID);
        if (isTimelineActive) {
          api.addTimelineEvent({
            layerId: MUTATIONS_LAYER_ID,
            event: {
              time: now(),
              title: "Change",
              subtitle: name,
              data: {
                newValue,
                oldValue
              },
              groupId: activeAction
            }
          });
        }
      }, {
        deep: true
      });
    });
    store.$subscribe(({ events, type }, state) => {
      api.notifyComponentUpdate();
      api.sendInspectorState(INSPECTOR_ID);
      if (!isTimelineActive) return;
      const eventData = {
        time: now(),
        title: formatMutationType(type),
        data: assign$1({
          store: formatDisplay(store.$id)
        }, formatEventData(events)),
        groupId: activeAction
      };
      if (type === MutationType.patchFunction) {
        eventData.subtitle = "\u2935\uFE0F";
      } else if (type === MutationType.patchObject) {
        eventData.subtitle = "\u{1F9E9}";
      } else if (events && !Array.isArray(events)) {
        eventData.subtitle = events.type;
      }
      if (events) {
        eventData.data["rawEvent(s)"] = {
          _custom: {
            display: "DebuggerEvent",
            type: "object",
            tooltip: "raw DebuggerEvent[]",
            value: events
          }
        };
      }
      api.addTimelineEvent({
        layerId: MUTATIONS_LAYER_ID,
        event: eventData
      });
    }, {
      detached: true,
      flush: "sync"
    });
    const hotUpdate = store._hotUpdate;
    store._hotUpdate = markRaw((newStore) => {
      hotUpdate(newStore);
      api.addTimelineEvent({
        layerId: MUTATIONS_LAYER_ID,
        event: {
          time: now(),
          title: "\u{1F525} " + store.$id,
          subtitle: "HMR update",
          data: {
            store: formatDisplay(store.$id),
            info: formatDisplay(`HMR update`)
          }
        }
      });
      api.notifyComponentUpdate();
      api.sendInspectorTree(INSPECTOR_ID);
      api.sendInspectorState(INSPECTOR_ID);
    });
    const { $dispose } = store;
    store.$dispose = () => {
      $dispose();
      api.notifyComponentUpdate();
      api.sendInspectorTree(INSPECTOR_ID);
      api.sendInspectorState(INSPECTOR_ID);
      api.getSettings().logStoreChanges && toastMessage(`Disposed "${store.$id}" store \u{1F5D1}`);
    };
    api.notifyComponentUpdate();
    api.sendInspectorTree(INSPECTOR_ID);
    api.sendInspectorState(INSPECTOR_ID);
    api.getSettings().logStoreChanges && toastMessage(`"${store.$id}" store installed \u{1F195}`);
  });
}
var runningActionId = 0;
var activeAction;
function patchActionForGrouping(store, actionNames, wrapWithProxy) {
  const actions = actionNames.reduce((storeActions, actionName) => {
    storeActions[actionName] = toRaw(store)[actionName];
    return storeActions;
  }, {});
  for (const actionName in actions) {
    store[actionName] = function() {
      const _actionId = runningActionId;
      const trackedStore = wrapWithProxy ? new Proxy(store, {
        get(...args) {
          activeAction = _actionId;
          return Reflect.get(...args);
        },
        set(...args) {
          activeAction = _actionId;
          return Reflect.set(...args);
        }
      }) : store;
      activeAction = _actionId;
      const retValue = actions[actionName].apply(trackedStore, arguments);
      activeAction = void 0;
      return retValue;
    };
  }
}
function devtoolsPlugin({ app, store, options }) {
  if (store.$id.startsWith("__hot:")) {
    return;
  }
  store._isOptionsAPI = !!options.state;
  if (!store._p._testing) {
    patchActionForGrouping(store, Object.keys(options.actions), store._isOptionsAPI);
    const originalHotUpdate = store._hotUpdate;
    toRaw(store)._hotUpdate = function(newStore) {
      originalHotUpdate.apply(this, arguments);
      patchActionForGrouping(store, Object.keys(newStore._hmrPayload.actions), !!store._isOptionsAPI);
    };
  }
  addStoreToDevtools(
    app,
    // FIXME: is there a way to allow the assignment from Store<Id, S, G, A> to StoreGeneric?
    store
  );
}
function createPinia() {
  const scope = effectScope(true);
  const state = scope.run(() => ref({}));
  let _p = [];
  let toBeInstalled = [];
  const pinia = markRaw({
    install(app) {
      setActivePinia(pinia);
      pinia._a = app;
      app.provide(piniaSymbol, pinia);
      app.config.globalProperties.$pinia = pinia;
      if (IS_CLIENT) {
        registerPiniaDevtools(app, pinia);
      }
      toBeInstalled.forEach((plugin) => _p.push(plugin));
      toBeInstalled = [];
    },
    use(plugin) {
      if (!this._a) {
        toBeInstalled.push(plugin);
      } else {
        _p.push(plugin);
      }
      return this;
    },
    _p,
    // it's actually undefined here
    // @ts-expect-error
    _a: null,
    _e: scope,
    _s: /* @__PURE__ */ new Map(),
    state
  });
  if (IS_CLIENT && typeof Proxy !== "undefined") {
    pinia.use(devtoolsPlugin);
  }
  return pinia;
}
function disposePinia(pinia) {
  pinia._e.stop();
  pinia._s.clear();
  pinia._p.splice(0);
  pinia.state.value = {};
  pinia._a = null;
}
function patchObject(newState, oldState) {
  for (const key in oldState) {
    const subPatch = oldState[key];
    if (!(key in newState)) {
      continue;
    }
    const targetValue = newState[key];
    if (isPlainObject4(targetValue) && isPlainObject4(subPatch) && !isRef(subPatch) && !isReactive(subPatch)) {
      newState[key] = patchObject(targetValue, subPatch);
    } else {
      newState[key] = subPatch;
    }
  }
  return newState;
}
var noop = () => {
};
function addSubscription(subscriptions, callback, detached, onCleanup = noop) {
  subscriptions.push(callback);
  const removeSubscription = () => {
    const idx = subscriptions.indexOf(callback);
    if (idx > -1) {
      subscriptions.splice(idx, 1);
      onCleanup();
    }
  };
  if (!detached && getCurrentScope()) {
    onScopeDispose(removeSubscription);
  }
  return removeSubscription;
}
function triggerSubscriptions(subscriptions, ...args) {
  subscriptions.slice().forEach((callback) => {
    callback(...args);
  });
}
var fallbackRunWithContext = (fn) => fn();
var ACTION_MARKER = Symbol();
var ACTION_NAME = Symbol();
function mergeReactiveObjects(target2, patchToApply) {
  if (target2 instanceof Map && patchToApply instanceof Map) {
    patchToApply.forEach((value, key) => target2.set(key, value));
  } else if (target2 instanceof Set && patchToApply instanceof Set) {
    patchToApply.forEach(target2.add, target2);
  }
  for (const key in patchToApply) {
    if (!patchToApply.hasOwnProperty(key)) continue;
    const subPatch = patchToApply[key];
    const targetValue = target2[key];
    if (isPlainObject4(targetValue) && isPlainObject4(subPatch) && target2.hasOwnProperty(key) && !isRef(subPatch) && !isReactive(subPatch)) {
      target2[key] = mergeReactiveObjects(targetValue, subPatch);
    } else {
      target2[key] = subPatch;
    }
  }
  return target2;
}
var skipHydrateSymbol = true ? Symbol("pinia:skipHydration") : (
  /* istanbul ignore next */
  Symbol()
);
function skipHydrate(obj) {
  return Object.defineProperty(obj, skipHydrateSymbol, {});
}
function shouldHydrate(obj) {
  return !isPlainObject4(obj) || !Object.prototype.hasOwnProperty.call(obj, skipHydrateSymbol);
}
var { assign } = Object;
function isComputed(o) {
  return !!(isRef(o) && o.effect);
}
function createOptionsStore(id, options, pinia, hot) {
  const { state, actions, getters } = options;
  const initialState = pinia.state.value[id];
  let store;
  function setup8() {
    if (!initialState && !hot) {
      pinia.state.value[id] = state ? state() : {};
    }
    const localState = hot ? toRefs(ref(state ? state() : {}).value) : toRefs(pinia.state.value[id]);
    return assign(localState, actions, Object.keys(getters || {}).reduce((computedGetters, name) => {
      if (name in localState) {
        console.warn(`[\u{1F34D}]: A getter cannot have the same name as another state property. Rename one of them. Found with "${name}" in store "${id}".`);
      }
      computedGetters[name] = markRaw(computed2(() => {
        setActivePinia(pinia);
        const store2 = pinia._s.get(id);
        return getters[name].call(store2, store2);
      }));
      return computedGetters;
    }, {}));
  }
  store = createSetupStore(id, setup8, options, pinia, hot, true);
  return store;
}
function createSetupStore($id, setup8, options = {}, pinia, hot, isOptionsStore) {
  let scope;
  const optionsForPlugin = assign({
    actions: {}
  }, options);
  if (!pinia._e.active) {
    throw new Error("Pinia destroyed");
  }
  const $subscribeOptions = {
    deep: true
  };
  if (true) {
    $subscribeOptions.onTrigger = (event) => {
      if (isListening) {
        debuggerEvents = event;
      } else if (isListening == false && !store._hotUpdating) {
        if (Array.isArray(debuggerEvents)) {
          debuggerEvents.push(event);
        } else {
          console.error("\u{1F34D} debuggerEvents should be an array. This is most likely an internal Pinia bug.");
        }
      }
    };
  }
  let isListening;
  let isSyncListening;
  let subscriptions = [];
  let actionSubscriptions = [];
  let debuggerEvents;
  const initialState = pinia.state.value[$id];
  if (!isOptionsStore && !initialState && !hot) {
    pinia.state.value[$id] = {};
  }
  const hotState = ref({});
  let activeListener;
  function $patch(partialStateOrMutator) {
    let subscriptionMutation;
    isListening = isSyncListening = false;
    if (true) {
      debuggerEvents = [];
    }
    if (typeof partialStateOrMutator === "function") {
      partialStateOrMutator(pinia.state.value[$id]);
      subscriptionMutation = {
        type: MutationType.patchFunction,
        storeId: $id,
        events: debuggerEvents
      };
    } else {
      mergeReactiveObjects(pinia.state.value[$id], partialStateOrMutator);
      subscriptionMutation = {
        type: MutationType.patchObject,
        payload: partialStateOrMutator,
        storeId: $id,
        events: debuggerEvents
      };
    }
    const myListenerId = activeListener = Symbol();
    nextTick().then(() => {
      if (activeListener === myListenerId) {
        isListening = true;
      }
    });
    isSyncListening = true;
    triggerSubscriptions(subscriptions, subscriptionMutation, pinia.state.value[$id]);
  }
  const $reset = isOptionsStore ? function $reset2() {
    const { state } = options;
    const newState = state ? state() : {};
    this.$patch(($state) => {
      assign($state, newState);
    });
  } : (
    /* istanbul ignore next */
    true ? () => {
      throw new Error(`\u{1F34D}: Store "${$id}" is built using the setup syntax and does not implement $reset().`);
    } : noop
  );
  function $dispose() {
    scope.stop();
    subscriptions = [];
    actionSubscriptions = [];
    pinia._s.delete($id);
  }
  const action = (fn, name = "") => {
    if (ACTION_MARKER in fn) {
      fn[ACTION_NAME] = name;
      return fn;
    }
    const wrappedAction = function() {
      setActivePinia(pinia);
      const args = Array.from(arguments);
      const afterCallbackList = [];
      const onErrorCallbackList = [];
      function after(callback) {
        afterCallbackList.push(callback);
      }
      function onError(callback) {
        onErrorCallbackList.push(callback);
      }
      triggerSubscriptions(actionSubscriptions, {
        args,
        name: wrappedAction[ACTION_NAME],
        store,
        after,
        onError
      });
      let ret;
      try {
        ret = fn.apply(this && this.$id === $id ? this : store, args);
      } catch (error) {
        triggerSubscriptions(onErrorCallbackList, error);
        throw error;
      }
      if (ret instanceof Promise) {
        return ret.then((value) => {
          triggerSubscriptions(afterCallbackList, value);
          return value;
        }).catch((error) => {
          triggerSubscriptions(onErrorCallbackList, error);
          return Promise.reject(error);
        });
      }
      triggerSubscriptions(afterCallbackList, ret);
      return ret;
    };
    wrappedAction[ACTION_MARKER] = true;
    wrappedAction[ACTION_NAME] = name;
    return wrappedAction;
  };
  const _hmrPayload = /* @__PURE__ */ markRaw({
    actions: {},
    getters: {},
    state: [],
    hotState
  });
  const partialStore = {
    _p: pinia,
    // _s: scope,
    $id,
    $onAction: addSubscription.bind(null, actionSubscriptions),
    $patch,
    $reset,
    $subscribe(callback, options2 = {}) {
      const removeSubscription = addSubscription(subscriptions, callback, options2.detached, () => stopWatcher());
      const stopWatcher = scope.run(() => watch2(() => pinia.state.value[$id], (state) => {
        if (options2.flush === "sync" ? isSyncListening : isListening) {
          callback({
            storeId: $id,
            type: MutationType.direct,
            events: debuggerEvents
          }, state);
        }
      }, assign({}, $subscribeOptions, options2)));
      return removeSubscription;
    },
    $dispose
  };
  const store = reactive(true ? assign({
    _hmrPayload,
    _customProperties: markRaw(/* @__PURE__ */ new Set())
  }, partialStore) : partialStore);
  pinia._s.set($id, store);
  const runWithContext = pinia._a && pinia._a.runWithContext || fallbackRunWithContext;
  const setupStore = runWithContext(() => pinia._e.run(() => (scope = effectScope()).run(() => setup8({
    action
  }))));
  for (const key in setupStore) {
    const prop = setupStore[key];
    if (isRef(prop) && !isComputed(prop) || isReactive(prop)) {
      if (hot) {
        hotState.value[key] = toRef(setupStore, key);
      } else if (!isOptionsStore) {
        if (initialState && shouldHydrate(prop)) {
          if (isRef(prop)) {
            prop.value = initialState[key];
          } else {
            mergeReactiveObjects(prop, initialState[key]);
          }
        }
        pinia.state.value[$id][key] = prop;
      }
      if (true) {
        _hmrPayload.state.push(key);
      }
    } else if (typeof prop === "function") {
      const actionValue = hot ? prop : action(prop, key);
      setupStore[key] = actionValue;
      if (true) {
        _hmrPayload.actions[key] = prop;
      }
      optionsForPlugin.actions[key] = prop;
    } else if (true) {
      if (isComputed(prop)) {
        _hmrPayload.getters[key] = isOptionsStore ? options.getters[key] : prop;
        if (IS_CLIENT) {
          const getters = setupStore._getters || // @ts-expect-error: same
          (setupStore._getters = markRaw([]));
          getters.push(key);
        }
      }
    }
  }
  assign(store, setupStore);
  assign(toRaw(store), setupStore);
  Object.defineProperty(store, "$state", {
    get: () => hot ? hotState.value : pinia.state.value[$id],
    set: (state) => {
      if (hot) {
        throw new Error("cannot set hotState");
      }
      $patch(($state) => {
        assign($state, state);
      });
    }
  });
  if (true) {
    store._hotUpdate = markRaw((newStore) => {
      store._hotUpdating = true;
      newStore._hmrPayload.state.forEach((stateKey) => {
        if (stateKey in store.$state) {
          const newStateTarget = newStore.$state[stateKey];
          const oldStateSource = store.$state[stateKey];
          if (typeof newStateTarget === "object" && isPlainObject4(newStateTarget) && isPlainObject4(oldStateSource)) {
            patchObject(newStateTarget, oldStateSource);
          } else {
            newStore.$state[stateKey] = oldStateSource;
          }
        }
        store[stateKey] = toRef(newStore.$state, stateKey);
      });
      Object.keys(store.$state).forEach((stateKey) => {
        if (!(stateKey in newStore.$state)) {
          delete store[stateKey];
        }
      });
      isListening = false;
      isSyncListening = false;
      pinia.state.value[$id] = toRef(newStore._hmrPayload, "hotState");
      isSyncListening = true;
      nextTick().then(() => {
        isListening = true;
      });
      for (const actionName in newStore._hmrPayload.actions) {
        const actionFn = newStore[actionName];
        store[actionName] = //
        action(actionFn, actionName);
      }
      for (const getterName in newStore._hmrPayload.getters) {
        const getter = newStore._hmrPayload.getters[getterName];
        const getterValue = isOptionsStore ? computed2(() => {
          setActivePinia(pinia);
          return getter.call(store, store);
        }) : getter;
        store[getterName] = //
        getterValue;
      }
      Object.keys(store._hmrPayload.getters).forEach((key) => {
        if (!(key in newStore._hmrPayload.getters)) {
          delete store[key];
        }
      });
      Object.keys(store._hmrPayload.actions).forEach((key) => {
        if (!(key in newStore._hmrPayload.actions)) {
          delete store[key];
        }
      });
      store._hmrPayload = newStore._hmrPayload;
      store._getters = newStore._getters;
      store._hotUpdating = false;
    });
  }
  if (IS_CLIENT) {
    const nonEnumerable = {
      writable: true,
      configurable: true,
      // avoid warning on devtools trying to display this property
      enumerable: false
    };
    [
      "_p",
      "_hmrPayload",
      "_getters",
      "_customProperties"
    ].forEach((p2) => {
      Object.defineProperty(store, p2, assign({
        value: store[p2]
      }, nonEnumerable));
    });
  }
  pinia._p.forEach((extender) => {
    if (IS_CLIENT) {
      const extensions = scope.run(() => extender({
        store,
        app: pinia._a,
        pinia,
        options: optionsForPlugin
      }));
      Object.keys(extensions || {}).forEach((key) => store._customProperties.add(key));
      assign(store, extensions);
    } else {
      assign(store, scope.run(() => extender({
        store,
        app: pinia._a,
        pinia,
        options: optionsForPlugin
      })));
    }
  });
  if (store.$state && typeof store.$state === "object" && typeof store.$state.constructor === "function" && !store.$state.constructor.toString().includes("[native code]")) {
    console.warn(`[\u{1F34D}]: The "state" must be a plain object. It cannot be
	state: () => new MyClass()
Found in store "${store.$id}".`);
  }
  if (initialState && isOptionsStore && options.hydrate) {
    options.hydrate(store.$state, initialState);
  }
  isListening = true;
  isSyncListening = true;
  return store;
}
// @__NO_SIDE_EFFECTS__
function defineStore(id, setup8, setupOptions) {
  let options;
  const isSetupStore = typeof setup8 === "function";
  options = isSetupStore ? setupOptions : setup8;
  function useStore(pinia, hot) {
    const hasContext = hasInjectionContext();
    pinia = // in test mode, ignore the argument provided as we can always retrieve a
    // pinia instance with getActivePinia()
    (false ? null : pinia) || (hasContext ? inject(piniaSymbol, null) : null);
    if (pinia) setActivePinia(pinia);
    if (!activePinia) {
      throw new Error(`[\u{1F34D}]: "getActivePinia()" was called but there was no active Pinia. Are you trying to use a store before calling "app.use(pinia)"?
See https://pinia.vuejs.org/core-concepts/outside-component-usage.html for help.
This will fail in production.`);
    }
    pinia = activePinia;
    if (!pinia._s.has(id)) {
      if (isSetupStore) {
        createSetupStore(id, setup8, options, pinia);
      } else {
        createOptionsStore(id, options, pinia);
      }
      if (true) {
        useStore._pinia = pinia;
      }
    }
    const store = pinia._s.get(id);
    if (hot) {
      const hotId = "__hot:" + id;
      const newStore = isSetupStore ? createSetupStore(hotId, setup8, options, pinia, true) : createOptionsStore(hotId, assign({}, options), pinia, true);
      hot._hotUpdate(newStore);
      delete pinia.state.value[hotId];
      pinia._s.delete(hotId);
    }
    if (IS_CLIENT) {
      const currentInstance2 = getCurrentInstance();
      if (currentInstance2 && currentInstance2.proxy && // avoid adding stores that are just built for hot module replacement
      !hot) {
        const vm = currentInstance2.proxy;
        const cache = "_pStores" in vm ? vm._pStores : vm._pStores = {};
        cache[id] = store;
      }
    }
    return store;
  }
  useStore.$id = id;
  return useStore;
}
var mapStoreSuffix = "Store";
function setMapStoreSuffix(suffix) {
  mapStoreSuffix = suffix;
}
function mapStores(...stores) {
  if (Array.isArray(stores[0])) {
    console.warn(`[\u{1F34D}]: Directly pass all stores to "mapStores()" without putting them in an array:
Replace
	mapStores([useAuthStore, useCartStore])
with
	mapStores(useAuthStore, useCartStore)
This will fail in production if not fixed.`);
    stores = stores[0];
  }
  return stores.reduce((reduced, useStore) => {
    reduced[useStore.$id + mapStoreSuffix] = function() {
      return useStore(this.$pinia);
    };
    return reduced;
  }, {});
}
function mapState(useStore, keysOrMapper) {
  return Array.isArray(keysOrMapper) ? keysOrMapper.reduce((reduced, key) => {
    reduced[key] = function() {
      return useStore(this.$pinia)[key];
    };
    return reduced;
  }, {}) : Object.keys(keysOrMapper).reduce((reduced, key) => {
    reduced[key] = function() {
      const store = useStore(this.$pinia);
      const storeKey = keysOrMapper[key];
      return typeof storeKey === "function" ? storeKey.call(this, store) : store[storeKey];
    };
    return reduced;
  }, {});
}
function mapActions(useStore, keysOrMapper) {
  return Array.isArray(keysOrMapper) ? keysOrMapper.reduce((reduced, key) => {
    reduced[key] = function(...args) {
      return useStore(this.$pinia)[key](...args);
    };
    return reduced;
  }, {}) : Object.keys(keysOrMapper).reduce((reduced, key) => {
    reduced[key] = function(...args) {
      return useStore(this.$pinia)[keysOrMapper[key]](...args);
    };
    return reduced;
  }, {});
}
function storeToRefs(store) {
  const rawStore = toRaw(store);
  const refs = {};
  for (const key in rawStore) {
    const value = rawStore[key];
    if (value.effect) {
      refs[key] = // ...
      computed2({
        get: () => store[key],
        set(value2) {
          store[key] = value2;
        }
      });
    } else if (isRef(value) || isReactive(value)) {
      refs[key] = // ---
      toRef(store, key);
    }
  }
  return refs;
}

// ../../../.tmp/sample-smoke/ECMAScript.Pinia.Counter/Release/jazor/stores/counter-store.mjs
var useCounterStore = defineStore("counter", {
  state: createState,
  getters: {
    doubleCount: /* @__PURE__ */ ((__cb) => function() {
      return __cb(this, ...arguments);
    })(readDoubleCount)
  },
  actions: {
    increment: /* @__PURE__ */ ((__cb) => function() {
      return __cb(this, ...arguments);
    })(increment),
    decrement: /* @__PURE__ */ ((__cb) => function() {
      return __cb(this, ...arguments);
    })(decrement)
  }
});
var useProjectedCounterStore = useCounterStore;
function useCounterStoreRefs(store) {
  return storeToRefs(store);
}
function useProjectedCounterStoreRefs(store) {
  return storeToRefs(store);
}
function installAuditPlugin(context) {
  if (context.store.$id !== "counter") {
    return null;
  }
  let projectedStore = context.store;
  let customState = projectedStore.$state;
  customState.persistedAt = "plugin:" + context.store.$id;
  return {
    auditTag: context.store.$id + ":audited"
  };
}
function createState() {
  return {
    count: 2,
    status: "Store seeded through defineStore()."
  };
}
function readDoubleCount(self2) {
  return self2.count * 2;
}
function increment(self2) {
  self2.count += 1;
  self2.status = "increment() updated the store.";
}
function decrement(self2) {
  if (self2.count > 0) {
    self2.count -= 1;
    self2.status = "decrement() updated the store.";
    return;
  }
  self2.status = "decrement() is clamped at zero.";
}

// ../../../.tmp/sample-smoke/ECMAScript.Pinia.Counter/Release/jazor/components/counter-app.mjs
var component = defineComponent({
  name: "PiniaCounterApp",
  setup
});
function setup() {
  let store = useCounterStore();
  let refs = useCounterStoreRefs(store);
  let patchPlusFive = () => {
    store.$patch({
      count: store.count + 5,
      status: "Applied $patch({ ... }) from the component."
    });
    return;
  };
  let resetStore = store.$reset.bind(store);
  return () => {
    return h("section", {
      class: "counter-shell"
    }, [
      h("p", {
        class: "counter-kicker"
      }, "ECMAScript.Pinia sample"),
      h("h1", {
        class: "counter-title"
      }, "Typed Pinia store authored in C#"),
      h("p", {
        class: "counter-copy"
      }, "The store comes from defineStore(), is resolved through StoreDefinition.Use(), and is read via storeToRefs()."),
      h("div", {
        class: "counter-grid"
      }, [
        createMetricCard("count", refs.count.value, "metric-card metric-card--primary"),
        createMetricCard("doubleCount", refs.doubleCount.value, "metric-card metric-card--secondary")
      ]),
      h("p", {
        class: "counter-status"
      }, refs.status.value),
      h("div", {
        class: "counter-actions"
      }, [
        createActionButton("Increment", "action-button action-button--accent", store.increment.bind(store)),
        createActionButton("Decrement", "action-button", store.decrement.bind(store)),
        createActionButton("Patch +5", "action-button", patchPlusFive),
        createActionButton("Reset", "action-button action-button--ghost", resetStore)
      ]),
      h("ul", {
        class: "counter-notes"
      }, [
        h("li", "createPinia() stays a normal external runtime import."),
        h("li", "StoreDefinition<TStore>.Use() keeps the callable store factory explicit in C#."),
        h("li", "storeToRefs() returns typed refs for both state and getters.")
      ])
    ]);
  };
}
function createMetricCard(label, value, className) {
  return h("article", {
    class: className
  }, [
    h("span", {
      class: "metric-label"
    }, label),
    h("strong", {
      class: "metric-value"
    }, value)
  ]);
}
function createActionButton(label, className, handler) {
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}

// ../../../.tmp/sample-smoke/ECMAScript.Pinia.Counter/Release/jazor/components/counter-cookbook.mjs
var component2 = defineComponent({
  name: "PiniaCounterCookbook",
  computed: createComputed(),
  methods: createMethods(),
  setup: setup2
});
function createComputed() {
  return mapState(useProjectedCounterStore, {
    count: "count",
    status: "status",
    doubleCount: "doubleCount",
    tripleCount: readTripleCount,
    auditTag: "auditTag"
  });
}
function createMethods() {
  return mapActions(useProjectedCounterStore, [
    "increment",
    "decrement"
  ]);
}
function setup2() {
  let projectedStore = useProjectedCounterStore();
  let refs = useProjectedCounterStoreRefs(projectedStore);
  let baseStore = projectedStore;
  let customState = projectedStore.$state;
  customState.persistedAt = "component:" + baseStore.$id;
  return () => {
    return h("section", {
      class: "counter-cookbook-shell"
    }, [
      h("h2", "Projected plugin cookbook"),
      h("p", "Projected store definitions flow through storeToRefs(), Options API helpers, and direct custom-property/custom-state projections without inventing a separate runtime object."),
      h("ul", [
        h("li", "auditTag: " + projectedStore.auditTag),
        h("li", "persistedAt: " + projectedStore.$state.persistedAt),
        h("li", "countRef: " + refs["count"].value),
        h("li", "statusRef: " + refs["status"].value),
        h("li", "doubleCount: " + projectedStore.doubleCount),
        h("li", "tripleCount: " + readTripleCount(projectedStore))
      ]),
      h("div", {
        class: "counter-actions"
      }, [
        createActionButton2("Projected increment", "action-button action-button--accent", baseStore.increment.bind(baseStore)),
        createActionButton2("Projected decrement", "action-button", baseStore.decrement.bind(baseStore))
      ]),
      h("p", {
        class: "counter-status"
      }, "Options API helpers are configured through CreateComputed()/CreateMethods(); the live card shows the projected store + projected refs path.")
    ]);
  };
}
function createActionButton2(label, className, handler) {
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}
function readTripleCount(store) {
  return store.count * 3;
}

// ../../../.tmp/sample-smoke/ECMAScript.Pinia.Counter/Release/jazor/components/counter-hmr.mjs
var component3 = defineComponent({
  name: "PiniaCounterHmrCookbook",
  setup: setup3
});
function setup3() {
  let store = useCounterStore();
  let projectedStore = useProjectedCounterStore();
  let customState = projectedStore.$state;
  let installHotSnapshot = () => {
    customState.persistedAt = "hmr:" + projectedStore.$id;
    return;
  };
  return () => {
    return h("section", {
      class: "counter-hmr-shell"
    }, [
      h("h2", "HMR cookbook"),
      h("p", "acceptHMRUpdate(useStore, hot) and storeDefinition.Use(pinia, hot) stay explicit in C# so Vite/Jolt hot-module wiring can remain a host concern instead of hidden compiler magic."),
      h("ul", {
        class: "counter-notes"
      }, [
        h("li", "store id: " + store.$id),
        h("li", "auditTag: " + projectedStore.auditTag),
        h("li", "persistedAt: " + customState.persistedAt),
        h("li", "consumer bridge calls import.meta.hot.accept(createCounterHotHandler(import.meta.hot))")
      ]),
      h("div", {
        class: "counter-actions"
      }, [
        createActionButton3("Prime HMR snapshot", "action-button action-button--accent", installHotSnapshot)
      ])
    ]);
  };
}
function createActionButton3(label, className, handler) {
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}

// ../../../.tmp/sample-smoke/ECMAScript.Pinia.Counter/Release/jazor/components/counter-hydration.mjs
var useHydrationStore = defineStore("counterHydration", setupHydrationStore);
var useHydrationOptionStore = defineStore("counterHydrationOptions", {
  state: createHydrationState,
  hydrate: hydrateOptionStore
});
var component4 = defineComponent({
  name: "PiniaCounterHydrationCookbook",
  setup: setup4
});
function seedInitialOptionStoreState(pinia) {
  pinia.state.value["counterHydrationOptions"] = {
    count: 12,
    status: "serialized SSR payload"
  };
}
function setupHydrationStore(helpers) {
  let clientOnlyNote = skipHydrate(ref("client-only note seeded in setup store"));
  return {
    count: 4,
    status: "setup-store hydration boundary is ready",
    clientOnlyNote,
    canHydrateClientOnlyNote: helpers.action(() => {
      return shouldHydrate(clientOnlyNote);
    }, "canHydrateClientOnlyNote"),
    refreshClientOnlyNote: helpers.action(() => {
      clientOnlyNote.value = "client note refreshed at " + useHydrationOptionStore().status;
      return;
    }, "refreshClientOnlyNote")
  };
}
function createHydrationState() {
  return {
    count: 8,
    status: "option-store hydration hook waiting"
  };
}
function hydrateOptionStore(storeState, initialState) {
  storeState.count = initialState.count;
  storeState.status = initialState.status + " -> hydrate(storeState, initialState)";
}
function setup4() {
  let setupStore = useHydrationStore();
  let optionStore = useHydrationOptionStore();
  let clientHydrates = computed2(setupStore.canHydrateClientOnlyNote);
  let reapplyClientNote = setupStore.refreshClientOnlyNote;
  let hydrateSnapshot = () => {
    optionStore.$patch({
      status: "hydration snapshot captured from client action"
    });
    return;
  };
  return () => {
    return h("section", {
      class: "counter-hydration-shell"
    }, [
      h("h2", "Hydration cookbook"),
      h("p", "skipHydrate()/shouldHydrate() remain explicit setup-store authoring tools, while option-store hydrate(storeState, initialState) stays available for SSR/client boundary repair without hiding runtime semantics."),
      h("ul", {
        class: "counter-notes"
      }, [
        h("li", "setup store id: counterHydration"),
        h("li", "option store id: " + optionStore.$id),
        h("li", "client-only note: " + setupStore.clientOnlyNote),
        h("li", "should hydrate client-only note: " + clientHydrates.value),
        h("li", "option-store status: " + optionStore.status)
      ]),
      h("div", {
        class: "counter-actions"
      }, [
        createActionButton4("Refresh client note", "action-button action-button--accent", reapplyClientNote),
        createActionButton4("Hydration snapshot", "action-button", hydrateSnapshot)
      ])
    ]);
  };
}
function createActionButton4(label, className, handler) {
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}

// ../../../.tmp/sample-smoke/ECMAScript.Pinia.Counter/Release/jazor/components/counter-isolation.mjs
var component5 = defineComponent({
  name: "PiniaCounterIsolationCookbook",
  setup: setup5
});
function setup5() {
  let leftPinia = createInstalledConfiguredPinia();
  let rightPinia = createInstalledConfiguredPinia();
  let leftStore = useCounterStore(leftPinia);
  let leftProjected = useProjectedCounterStore(leftPinia);
  let rightStore = useCounterStore(rightPinia);
  let rightProjected = useProjectedCounterStore(rightPinia);
  let snapshot = ref(describeSnapshot(leftStore, rightStore, leftProjected, rightProjected));
  let incrementLeftOnly = () => {
    leftStore.increment();
    leftProjected.$state.persistedAt = "isolated:left:" + leftStore.count;
    snapshot.value = describeSnapshot(leftStore, rightStore, leftProjected, rightProjected);
    return;
  };
  let incrementRightOnly = () => {
    rightStore.increment();
    rightProjected.$state.persistedAt = "isolated:right:" + rightStore.count;
    snapshot.value = describeSnapshot(leftStore, rightStore, leftProjected, rightProjected);
    return;
  };
  onUnmounted(() => {
    disposePinia(leftPinia);
    disposePinia(rightPinia);
    return;
  });
  return () => {
    return h("section", {
      class: "counter-isolation-shell"
    }, [
      h("h2", "Root isolation cookbook"),
      h("p", "Explicit StoreDefinition.Use(pinia) resolution keeps multiple Pinia roots isolated even when they reuse the same generated store definition and plugin projection contract."),
      h("ul", {
        class: "counter-notes"
      }, [
        h("li", "snapshot: " + snapshot.value),
        h("li", "left persistedAt: " + leftProjected.$state.persistedAt),
        h("li", "right persistedAt: " + rightProjected.$state.persistedAt)
      ]),
      h("div", {
        class: "counter-actions"
      }, [
        createActionButton5("Increment left root", "action-button action-button--accent", incrementLeftOnly),
        createActionButton5("Increment right root", "action-button", incrementRightOnly)
      ])
    ]);
  };
}
function createInstalledConfiguredPinia() {
  let pinia = createConfiguredPinia();
  createPiniaInstallationApp(pinia);
  return pinia;
}
function describeSnapshot(leftStore, rightStore, leftProjected, rightProjected) {
  return "left=" + leftStore.count + ", right=" + rightStore.count + ", leftAudit=" + leftProjected.auditTag + ", rightAudit=" + rightProjected.auditTag;
}
function createActionButton5(label, className, handler) {
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}

// ../../../.tmp/sample-smoke/ECMAScript.Pinia.Counter/Release/jazor/stores/activity-store.mjs
var useActivityStore = defineStore("activity", {
  state: createState2,
  getters: {
    summary: /* @__PURE__ */ ((__cb) => function() {
      return __cb(this, ...arguments);
    })(readSummary)
  },
  actions: {
    capture: /* @__PURE__ */ ((__cb) => function() {
      return __cb(this, ...arguments);
    })(capture),
    queueReview: /* @__PURE__ */ ((__cb) => function() {
      return __cb(this, ...arguments);
    })(queueReview)
  }
});
function createState2() {
  return {
    completedActions: 1,
    pendingReviews: 2,
    highlight: "Waiting for the next workflow capture."
  };
}
function readSummary(self2) {
  return self2.highlight + " (done: " + self2.completedActions + ", pending: " + self2.pendingReviews + ")";
}
function capture(self2, source) {
  self2.completedActions += 1;
  if (self2.pendingReviews > 0) {
    self2.pendingReviews -= 1;
  }
  self2.highlight = "capture(" + source + ") updated the activity store.";
}
function queueReview(self2) {
  self2.pendingReviews += 1;
  self2.highlight = "queueReview() recorded another follow-up item.";
}

// ../../../.tmp/sample-smoke/ECMAScript.Pinia.Counter/Release/jazor/components/counter-multi-store.mjs
var optionsApiSnapshot = ref("mapStores() snapshot will be captured after mount.");
var optionsApiStoreIds = ref("store ids pending");
var component6 = defineComponent({
  name: "PiniaCounterMultiStore",
  computed: createComputed2(),
  mounted: /* @__PURE__ */ ((__cb) => function() {
    return __cb(this, ...arguments);
  })(captureMappedStores),
  setup: setup6
});
function createComputed2() {
  setMapStoreSuffix("");
  return mapStores(useCounterStore, useActivityStore);
}
function setup6() {
  let counter = useCounterStore();
  let activity = useActivityStore();
  let incrementAndCapture = () => {
    counter.increment();
    activity.capture("increment");
    return;
  };
  let queueReview2 = activity.queueReview.bind(activity);
  let liveSummary = computed2(() => {
    return counter.status + " | " + activity.summary;
  });
  let combinedScore = computed2(() => {
    return counter.count + activity.completedActions + activity.pendingReviews;
  });
  return () => {
    return h("section", {
      class: "counter-multi-store-shell"
    }, [
      h("h2", "Multi-store cookbook"),
      h("p", 'SetMapStoreSuffix("") keeps the component-instance fields aligned with store ids while mapStores() projects both stores through one Options API entry point.'),
      h("div", {
        class: "counter-summary-grid"
      }, [
        createMetricCard2("counter.count", counter.count, "metric-card metric-card--primary"),
        createMetricCard2("activity.done", activity.completedActions, "metric-card metric-card--secondary"),
        createMetricCard2("activity.pending", activity.pendingReviews, "metric-card metric-card--neutral"),
        createMetricCard2("combined", combinedScore.value, "metric-card metric-card--accent")
      ]),
      h("p", {
        class: "counter-status"
      }, liveSummary.value),
      h("div", {
        class: "counter-actions"
      }, [
        createActionButton6("Increment + capture", "action-button action-button--accent", incrementAndCapture),
        createActionButton6("Queue review", "action-button", queueReview2)
      ]),
      h("ul", {
        class: "counter-notes"
      }, [
        h("li", "mounted snapshot via mapStores(): " + optionsApiSnapshot.value),
        h("li", "mapped component store ids: " + optionsApiStoreIds.value),
        h("li", "direct setup render keeps the live surface readable while Options API captures the heterogeneous store projection.")
      ])
    ]);
  };
}
function captureMappedStores(self2) {
  optionsApiSnapshot.value = self2.counter.status + " | " + self2.activity.summary;
  optionsApiStoreIds.value = self2.counter.$id + " + " + self2.activity.$id;
}
function createMetricCard2(label, value, className) {
  return h("article", {
    class: className
  }, [
    h("span", {
      class: "metric-label"
    }, label),
    h("strong", {
      class: "metric-value"
    }, value)
  ]);
}
function createActionButton6(label, className, handler) {
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}

// ../../../.tmp/sample-smoke/ECMAScript.Pinia.Counter/Release/jazor/components/counter-subscription.mjs
var component7 = defineComponent({
  name: "PiniaCounterSubscriptionCookbook",
  setup: setup7
});
function setup7() {
  let store = useCounterStore();
  let mutationKind = ref("No mutations observed yet.");
  let storeId = ref(store.$id);
  let statusSnapshot = ref(store.status);
  let countSnapshot = ref(store.count);
  let payloadSnapshot = ref("payload appears only for $patch({ ... }) mutations.");
  let eventShape = ref("Debugger events are dev-only and may be unavailable.");
  let notificationCount = ref(0);
  let detach = null;
  let handleMutation = (mutation, state) => {
    notificationCount.value += 1;
    mutationKind.value = describeMutationType(mutation.type);
    storeId.value = mutation.storeId;
    countSnapshot.value = state.count;
    statusSnapshot.value = state.status;
    payloadSnapshot.value = readMutationSummary(mutation);
    eventShape.value = describeEvents(mutation.events);
    return;
  };
  let applyDirectMutation = () => {
    store.count += 1;
    store.status = "Direct assignment updated the counter store.";
    return;
  };
  let applyObjectPatch = () => {
    store.$patch({
      count: store.count + 3,
      status: "Object patch updated the counter store."
    });
    return;
  };
  let applyFunctionPatch = () => {
    store.$patch((state) => {
      state.count += 2;
      state.status = "Function patch updated the counter store.";
      return;
    });
    return;
  };
  onMounted(() => {
    detach = store.$subscribe(handleMutation, {
      detached: true,
      flush: "sync"
    });
    return;
  });
  onUnmounted(() => {
    if (detach !== null) {
      detach();
    }
    return;
  });
  return () => {
    return h("section", {
      class: "counter-subscription-shell"
    }, [
      h("h2", "Subscription cookbook"),
      h("p", "$subscribe() is registered with detached + sync options so the sample can inspect direct mutations, object patches, and function patches from one stable callback. Direct assignments may report multiple sync notifications when several fields change back-to-back."),
      h("div", {
        class: "counter-actions"
      }, [
        createActionButton7("Direct +1", "action-button action-button--accent", applyDirectMutation),
        createActionButton7("Object patch", "action-button", applyObjectPatch),
        createActionButton7("Function patch", "action-button", applyFunctionPatch)
      ]),
      h("ul", {
        class: "counter-notes"
      }, [
        h("li", "mutation kind: " + mutationKind.value),
        h("li", "store id: " + storeId.value),
        h("li", "count snapshot: " + countSnapshot.value),
        h("li", "status snapshot: " + statusSnapshot.value),
        h("li", "payload summary: " + payloadSnapshot.value),
        h("li", "events shape: " + eventShape.value),
        h("li", "notifications seen: " + notificationCount.value)
      ])
    ]);
  };
}
function describeMutationType(type) {
  return (() => {
    const __swexpr$59f6aa2a1ae075e733124215 = type;
    if (__swexpr$59f6aa2a1ae075e733124215 === "direct") return "direct assignment";
    if (__swexpr$59f6aa2a1ae075e733124215 === "patch object") return "$patch({ ... }) object merge";
    if (__swexpr$59f6aa2a1ae075e733124215 === "patch function") return "$patch((state) => ...) callback";
    return "unknown mutation";
  })();
}
function readMutationSummary(mutation) {
  if (mutation.type === "patch object") {
    let patchMutation = mutation;
    let payload = patchMutation.payload;
    if (payload.status !== null) {
      return "payload.status = " + payload.status;
    }
    if (payload.count !== null) {
      return "payload.count = " + payload.count;
    }
    return "object patch payload captured without known fields";
  }
  if (mutation.type === "patch function") {
    return "function patch metadata captured without a payload object";
  }
  return "direct assignments do not expose a payload object";
}
function describeEvents(events) {
  return events === null ? "not provided" : "reported";
}
function createActionButton7(label, className, handler) {
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}

// ../../../.tmp/sample-smoke/ECMAScript.Pinia.Counter/Release/jazor/host/app.mjs
function createConfiguredApp() {
  let app = createApp(defineComponent({
    name: "PiniaCounterRoot",
    render: renderRoot
  }));
  let pinia = createConfiguredPinia();
  app.use(pinia);
  app.onUnmount(() => {
    disposePinia(pinia);
    return;
  });
  return app;
}
function createConfiguredPinia() {
  let pinia = createPinia().use(installAuditPlugin);
  seedInitialOptionStoreState(pinia);
  return pinia;
}
function createPiniaInstallationApp(pinia) {
  let app = createApp(defineComponent({
    name: "PiniaConfiguredRootShell",
    render: renderPiniaInstallationShell
  }));
  app.use(pinia);
  return app;
}
function boot(selector) {
  let app = createConfiguredApp();
  app.mount(selector);
}
function renderPiniaInstallationShell() {
  return h("div");
}
function renderRoot() {
  return h("main", {
    class: "counter-root"
  }, [
    h("section", {
      class: "counter-hero"
    }, [
      h("p", {
        class: "counter-kicker"
      }, "ECMAScript.Pinia production sample"),
      h("h1", {
        class: "counter-title"
      }, "Typed Pinia stores, projected plugins, multi-store helpers, subscriptions, and testing"),
      h("p", {
        class: "counter-copy"
      }, "The sample keeps Pinia as a normal external runtime while exercising authoring paths that matter in production code: defineStore(), storeToRefs(), plugin projections, mapStores(), $subscribe(), acceptHMRUpdate(), and createTestingPinia().")
    ]),
    h("div", {
      class: "counter-stack"
    }, [
      h(component),
      h(component2),
      h(component6),
      h(component7),
      h(component4),
      h(component5),
      h(component3)
    ])
  ]);
}

// .deno-build/client-entry.mjs
boot("#app");
/*! Bundled license information:

@vue/shared/dist/shared.esm-bundler.js:
  (**
  * @vue/shared v3.5.13
  * (c) 2018-present Yuxi (Evan) You and Vue contributors
  * @license MIT
  **)
  (*! #__NO_SIDE_EFFECTS__ *)

@vue/reactivity/dist/reactivity.esm-bundler.js:
  (**
  * @vue/reactivity v3.5.13
  * (c) 2018-present Yuxi (Evan) You and Vue contributors
  * @license MIT
  **)

@vue/runtime-core/dist/runtime-core.esm-bundler.js:
  (**
  * @vue/runtime-core v3.5.13
  * (c) 2018-present Yuxi (Evan) You and Vue contributors
  * @license MIT
  **)

@vue/runtime-core/dist/runtime-core.esm-bundler.js:
@vue/runtime-core/dist/runtime-core.esm-bundler.js:
@vue/runtime-core/dist/runtime-core.esm-bundler.js:
@vue/runtime-dom/dist/runtime-dom.esm-bundler.js:
@vue/runtime-dom/dist/runtime-dom.esm-bundler.js:
pinia/dist/pinia.mjs:
  (*! #__NO_SIDE_EFFECTS__ *)

@vue/runtime-dom/dist/runtime-dom.esm-bundler.js:
  (**
  * @vue/runtime-dom v3.5.13
  * (c) 2018-present Yuxi (Evan) You and Vue contributors
  * @license MIT
  **)

vue/dist/vue.runtime.esm-bundler.js:
  (**
  * vue v3.5.13
  * (c) 2018-present Yuxi (Evan) You and Vue contributors
  * @license MIT
  **)

pinia/dist/pinia.mjs:
  (*!
   * pinia v3.0.3
   * (c) 2025 Eduardo San Martin Morote
   * @license MIT
   *)
*/
//# sourceMappingURL=client-entry.js.map
