﻿    var base = {
        _initialize: function () {
            if (this.$initialized) {
                return;
            }

            this.$initialized = Bridge.emptyFn;

            if (this.$staticInit) {
                this.$staticInit();
            }

            if (this.$initMembers) {
                this.$initMembers();
            }
        },

        initConfig: function (extend, base, config, statics, scope, prototype) {
            var initFn,
                name;

            if (config.fields) {
                for (name in config.fields) {
                    scope[name] = config.fields[name];
                }
            }

            if (config.properties) {
                for (name in config.properties) {
                    Bridge.property(scope, name, config.properties[name], statics);
                }
            }

            if (config.events) {
                for (name in config.events) {
                    Bridge.event(scope, name, config.events[name], statics);
                }
            }

            if (config.alias) {
                for (var i = 0; i < config.alias.length; i++) {
                    var m = scope[config.alias[i]];

                    if (m === undefined && prototype) {
                        m = prototype[config.alias[i]];
                    }

                    scope[config.alias[i + 1]] = m;
                    i++;
                }
            }

            if (config.init) {
                initFn = config.init;
            }

            if (initFn || (extend && !statics && base.$initMembers)) {
                scope.$initMembers = function () {
                    if (extend && !statics && base.$initMembers) {
                        base.$initMembers.call(this);
                    }

                    if (initFn) {
                        initFn.call(this);
                    }
                };
            }
        },

        definei: function (className, gscope, prop) {
            if ((prop === true || !prop) && gscope) {
                gscope.$kind = "interface";
            } else if (prop) {
                prop.$kind = "interface";
            } else {
                gscope = { $kind: "interface" };
            }

            var c = Bridge.define(className, gscope, prop);

            c.$kind = "interface";
            c.$isInterface = true;

            return c;
        },

        // Create a new Class that inherits from this class
        define: function (className, gscope, prop, gCfg) {
            var isGenericInstance = false;

            if (prop === true) {
                isGenericInstance = true;
                prop = gscope;
                gscope = Bridge.global;
            } else if (!prop) {
                prop = gscope;
                gscope = Bridge.global;
            }

            var fn;

            if (Bridge.isFunction(prop)) {
                fn = function () {
                    var args,
                        key,
                        obj,
                        c;

                    key = Bridge.Class.getCachedType(fn, arguments);

                    if (key) {
                        return key.type;
                    }

                    args = Array.prototype.slice.call(arguments);
                    obj = prop.apply(null, args);
                    c = Bridge.define(Bridge.Class.genericName(className, args), obj, true, { fn: fn, args: args });

                    if (!Bridge.Class.staticInitAllow) {
                        Bridge.Class.$queue.push(c);
                    }

                    return Bridge.get(c);
                };

                fn.$cache = [];

                return Bridge.Class.generic(className, gscope, fn, prop.length);
            }

            if (!isGenericInstance) {
                Bridge.Class.staticInitAllow = false;
            }

            prop = prop || {};

            var extend = prop.$inherits || prop.inherits,
                statics = prop.$statics || prop.statics,
                isEntryPoint = prop.$entryPoint,
                base,
                prototype,
                scope = prop.$scope || gscope || Bridge.global,
                i,
                v,
                isCtor,
                ctorName,
                name;

            prop.$kind = prop.$kind || "class";

            if (prop.$inherits) {
                delete prop.$inherits;
            } else {
                delete prop.inherits;
            }

            if (isEntryPoint) {
                delete prop.$entryPoint;
            }

            if (Bridge.isFunction(statics)) {
                statics = null;
            } else if (prop.$statics) {
                delete prop.$statics;
            } else {
                delete prop.statics;
            }

            var Class,
                cls = prop.hasOwnProperty("ctor") && prop.ctor;

            if (!cls) {
                if (prop.$literal) {
                    Class = function () {
                        return {};
                    };
                } else {
                    Class = function () {
                        this.$initialize();
                        if (Class.$base) {
                            if (Class.$$inherits && Class.$$inherits.length > 0 && Class.$$inherits[0].$staticInit) {
                                Class.$$inherits[0].$staticInit();
                            }
                            Class.$base.ctor.call(this);
                        }
                    };
                }

                prop.ctor = Class;
            } else {
                Class = cls;
            }

            if (!isGenericInstance) {
                scope = Bridge.Class.set(scope, className, Class);
            }

            if (gCfg) {
                gCfg.fn.$cache.push({ type: Class, args: gCfg.args });
            }

            Class.$$name = className;
            Class.$kind = prop.$kind;

            if (gCfg && isGenericInstance) {
                Class.$genericTypeDefinition = gCfg.fn;
                Class.$typeArguments = gCfg.args;
                Class.$assembly = gCfg.fn.$assembly || Bridge.$currentAssembly;

                var result = Bridge.Reflection.getTypeFullName(gCfg.fn);

                for (i = 0; i < gCfg.args.length; i++) {
                    result += (i === 0 ? '[' : ',') + '[' + Bridge.Reflection.getTypeQName(gCfg.args[i]) + ']';
                }

                result += ']';

                Class.$$fullname = result;
            } else {
                Class.$$fullname = Class.$$name;
            }

            if (extend && Bridge.isFunction(extend)) {
                extend = extend();
            }

            var interfaces = [],
                baseInterfaces = [];

            if (extend) {
                for (var j = 0; j < extend.length; j++) {
                    var baseType = extend[j],
                        baseI = (baseType.$interfaces || []).concat(baseType.$baseInterfaces || []);

                    if (baseI.length > 0) {
                        for (var k = 0; k < baseI.length; k++) {
                            if (baseInterfaces.indexOf(baseI[k]) < 0) {
                                baseInterfaces.push(baseI[k]);
                            }
                        }
                    }

                    if (baseType.$kind === "interface") {
                        interfaces.push(baseType);
                    }
                }
            }

            Class.$baseInterfaces = baseInterfaces;
            Class.$interfaces = interfaces;
            Class.$allInterfaces = interfaces.concat(baseInterfaces);

            var noBase = extend ? extend[0].$kind === "interface" : true;

            if (noBase) {
                extend = null;
            }

            base = extend ? extend[0].prototype : this.prototype;
            Class.$base = base;
            prototype = extend ? (extend[0].$$initCtor ? new extend[0].$$initCtor() : new extend[0]()) : new Object();

            Class.$$initCtor = function () { };
            Class.$$initCtor.prototype = prototype;
            Class.$$initCtor.prototype.constructor = Class;
            Class.$$initCtor.prototype.$$fullname = gCfg && isGenericInstance ? Class.$$fullname : Class.$$name;

            if (statics) {
                var staticsConfig = statics.$config || statics.config;

                if (staticsConfig && !Bridge.isFunction(staticsConfig)) {
                    Bridge.Class.initConfig(extend, base, staticsConfig, true, Class);

                    if (statics.$config) {
                        delete statics.$config;
                    } else {
                        delete statics.config;
                    }
                }
            }

            var instanceConfig = prop.$config || prop.config;

            if (instanceConfig && !Bridge.isFunction(instanceConfig)) {
                Bridge.Class.initConfig(extend, base, instanceConfig, false, prop, prototype);

                if (prop.$config) {
                    delete prop.$config;
                } else {
                    delete prop.config;
                }
            } else if (extend && base.$initMembers) {
                prop.$initMembers = function () {
                    base.$initMembers.call(this);
                };
            }

            prop.$initialize = Bridge.Class._initialize;

            var keys = [];

            for (name in prop) {
                keys.push(name);
            }

            for (i = 0; i < keys.length; i++) {
                name = keys[i];

                v = prop[name];
                isCtor = name === "ctor";
                ctorName = name;

                if (Bridge.isFunction(v) && (isCtor || name.match("^\\$ctor") !== null)) {
                    isCtor = true;
                }

                if (isCtor) {
                    Class[ctorName] = prop[name];
                    Class[ctorName].prototype = prototype;
                    Class[ctorName].prototype.constructor = Class;
                    prototype[ctorName] = prop[name];
                } else {
                    prototype[ctorName] = prop[name];
                }
            }

            prototype.$$name = className;

            if (statics) {
                for (name in statics) {
                    if (name === "ctor") {
                        Class["$ctor"] = statics[name];
                    } else {
                        Class[name] = statics[name];
                    }
                }
            }

            if (!extend) {
                extend = [Object].concat(interfaces);
            }

            Class.$$inherits = extend;

            for (i = 0; i < extend.length; i++) {
                scope = extend[i];

                if (!scope.$$inheritors) {
                    scope.$$inheritors = [];
                }

                scope.$$inheritors.push(Class);
            }

            fn = function () {
                if (Bridge.Class.staticInitAllow) {
                    Class.$staticInit = null;

                    if (Class.$initMembers) {
                        Class.$initMembers();
                    }

                    if (Class.$ctor) {
                        Class.$ctor();
                    }
                }
            };

            if (isEntryPoint || Bridge.isFunction(prototype.$main)) {
                Bridge.Class.$queueEntry.push(Class);
            }

            Class.$staticInit = fn;

            if (!isGenericInstance) {
                Bridge.Class.registerType(className, Class);
            }

            if (Bridge.Reflection) {
                Class.$getMetadata = Bridge.Reflection.getMetadata;
            }

            if (Class.$kind === "enum") {
                Class.$is = function (instance) {
                    var utype = Class.prototype.$utype;

                    if (utype === System.String) {
                        return typeof (instance) == "string";
                    }

                    if (utype && utype.$is) {
                        return utype.$is(instance);
                    }

                    return typeof (instance) == "number";
                };

                Class.getDefaultValue = function () {
                    var utype = Class.prototype.$utype;

                    if (utype === System.String) {
                        return null;
                    }

                    return 0;
                };
            }

            if (Class.$kind === "interface") {
                if (Class.prototype.$variance) {
                    Class.isAssignableFrom = Bridge.Class.varianceAssignable;
                }

                Class.$isInterface = true;
            }

            return Class;
        },

        varianceAssignable: function (source) {
            var check = function (target, type) {
                if (type.$genericTypeDefinition === target.$genericTypeDefinition && type.$typeArguments.length === target.$typeArguments.length) {
                    for (var i = 0; i < target.$typeArguments.length; i++) {
                        var v = target.prototype.$variance[i], t = target.$typeArguments[i], s = type.$typeArguments[i];

                        switch (v) {
                            case 1: if (!Bridge.Reflection.isAssignableFrom(t, s))
                                return false;
                                break;
                            case 2: if (!Bridge.Reflection.isAssignableFrom(s, t))
                                return false;
                                break;
                            default: if (s !== t)
                                return false;
                        }
                    }

                    return true;
                }

                return false;
            };

            if (source.$kind === "interface" && check(this, source)) {
                return true;
            }

            var ifs = Bridge.Reflection.getInterfaces(source);

            for (var i = 0; i < ifs.length; i++) {
                if (ifs[i] === this || check(this, ifs[i])) {
                    return true;
                }
            }
            return false;
        },

        registerType: function (className, cls) {
            if (Bridge.$currentAssembly) {
                Bridge.$currentAssembly.$types[className] = cls;
                cls.$assembly = Bridge.$currentAssembly;
            }
        },

        addExtend: function (cls, extend) {
            var i,
                scope;

            Array.prototype.push.apply(cls.$$inherits, extend);
            cls.$interfaces = cls.$interfaces || [];
            cls.$baseInterfaces = cls.$baseInterfaces || [];

            for (i = 0; i < extend.length; i++) {
                scope = extend[i];

                if (!scope.$$inheritors) {
                    scope.$$inheritors = [];
                }

                scope.$$inheritors.push(cls);

                var baseI = (scope.$interfaces || []).concat(scope.$baseInterfaces || []);

                if (baseI.length > 0) {
                    for (var k = 0; k < baseI.length; k++) {
                        if (cls.$baseInterfaces.indexOf(baseI[k]) < 0) {
                            cls.$baseInterfaces.push(baseI[k]);
                        }
                    }
                }

                if (scope.$kind === "interface") {
                    cls.$interfaces.push(scope);
                }
            }

            cls.$allInterfaces = cls.$interfaces.concat(cls.$baseInterfaces);
        },

        set: function (scope, className, cls, noDefineProp) {
            var nameParts = className.split("."),
                name,
                key,
                exists,
                i;

            for (i = 0; i < (nameParts.length - 1) ; i++) {
                if (typeof scope[nameParts[i]] == "undefined") {
                    scope[nameParts[i]] = {};
                }

                scope = scope[nameParts[i]];
            }

            name = nameParts[nameParts.length - 1];
            exists = scope[name];

            if (exists) {
                if (exists.$$name === className) {
                    throw "Class '" + className + "' is already defined";
                }

                for (key in exists) {
                    var o = exists[key];

                    if (typeof o === "function" && o.$$name) {
                        (function (cls, key, o) {
                            Object.defineProperty(cls, key, {
                                get: function () {
                                    if (Bridge.Class.staticInitAllow) {
                                        if (o.$staticInit) {
                                            o.$staticInit();
                                        }

                                        Bridge.Class.defineProperty(cls, key, o);
                                    }

                                    return o;
                                },

                                set: function (newValue) {
                                    o = newValue;
                                },

                                enumerable: true,

                                configurable: true
                            });
                        })(cls, key, o);
                    }
                }
            }

            if (noDefineProp !== true) {
                (function (scope, name, cls) {
                    Object.defineProperty(scope, name, {
                        get: function () {
                            if (Bridge.Class.staticInitAllow) {
                                if (cls.$staticInit) {
                                    cls.$staticInit();
                                }

                                Bridge.Class.defineProperty(scope, name, cls);
                            }

                            return cls;
                        },

                        set: function (newValue) {
                            cls = newValue;
                        },

                        enumerable: true,

                        configurable: true
                    });
                })(scope, name, cls);
            } else {
                scope[name] = cls;
            }

            return scope;
        },

        defineProperty: function (scope, name, cls) {
            Object.defineProperty(scope, name, {
                value: cls,
                enumerable: true,
                configurable: true
            });
        },

        genericName: function (name, typeArguments) {
            var gName = name;

            for (var i = 0; i < typeArguments.length; i++) {
                var ta = typeArguments[i];

                gName += "$" + (ta.$$name || Bridge.getTypeName(ta));
            }

            return gName;
        },

        getCachedType: function (fn, args) {
            var arr = fn.$cache,
                len = arr.length,
                key,
                found,
                i, g;

            for (i = 0; i < len; i++) {
                key = arr[i];

                if (key.args.length === args.length) {
                    found = true;

                    for (g = 0; g < key.args.length; g++) {
                        if (key.args[g] !== args[g]) {
                            found = false;

                            break;
                        }
                    }

                    if (found) {
                        return key;
                    }
                }
            }

            return null;
        },

        generic: function (className, scope, fn, length) {
            fn.$$name = className;
            fn.$kind = "class";

            Bridge.Class.set(scope, className, fn, true);
            Bridge.Class.registerType(className, fn);

            fn.$typeArgumentCount = length;
            fn.$isGenericTypeDefinition = true;
            fn.$getMetadata = Bridge.Reflection.getMetadata;

            return fn;
        },

        init: function (fn) {
            Bridge.Class.staticInitAllow = true;

            var queue = Bridge.Class.$queue.concat(Bridge.Class.$queueEntry);

            for (var i = 0; i < queue.length; i++) {
                var t = queue[i];

                if (t.$staticInit) {
                    t.$staticInit();
                }

                if (t.prototype.$main) {
                    Bridge.ready(t.prototype.$main);
                }
            }

            Bridge.Class.$queue.length = 0;
            Bridge.Class.$queueEntry.length = 0;

            if (fn) {
                fn();
            }
        }
    };

    Bridge.Class = base;
    Bridge.Class.$queue = [];
    Bridge.Class.$queueEntry = [];
    Bridge.define = Bridge.Class.define;
    Bridge.definei = Bridge.Class.definei;
    Bridge.init = Bridge.Class.init;
