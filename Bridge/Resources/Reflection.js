    Bridge.Reflection = {
        setMetadata: function (type, metadata) {
            type.$getMetadata = Bridge.Reflection.getMetadata;
            type.$metadata = metadata;
        },

        initMetaData: function (type, metadata) {
            if (metadata.m) {
                for (var i = 0; i < metadata.m.length; i++) {
                    var m = metadata.m[i];

                    m.td = type;

                    if (m.ad) {
                        m.ad.td = type;
                    }

                    if (m.r) {
                        m.r.td = type;
                    }

                    if (m.g) {
                        m.g.td = type;
                    }

                    if (m.s) {
                        m.s.td = type;
                    }
                }
            }

            type.$metadata = metadata;
            type.$initMetaData = true;
        },

        getMetadata: function () {
            if (!this.$metadata && this.$genericTypeDefinition) {
                this.$metadata = this.$genericTypeDefinition.$factoryMetadata || this.$genericTypeDefinition.$metadata;
            }

            var metadata = this.$metadata;

            if (typeof (metadata) === "function") {
                if (this.$isGenericTypeDefinition) {
                    var i,
                        size = this.$typeArgumentCount,
                        arr = new Array(size);

                    for (i = 0; i < size; i++) {
                        arr[i] = Object;
                    }

                    this.$factoryMetadata = this.$metadata;
                    metadata = this.$metadata.apply(null, arr);
                } else if (this.$typeArguments) {
                    metadata = this.$metadata.apply(null, this.$typeArguments);
                } else {
                    metadata = this.$metadata();
                }
            }

            if (!this.$initMetaData && metadata) {
                Bridge.Reflection.initMetaData(this, metadata);
            }

            return metadata;
        },

        load: function (name) {
            return System.Reflection.Assembly.assemblies[name] || require(name);
        },

        getGenericTypeDefinition: function (type) {
            if (type.$isGenericTypeDefinition) {
                return type;
            }

            if (!type.$genericTypeDefinition) {
                throw new System.InvalidOperationException("This operation is only valid on generic types.");
            }

            return type.$genericTypeDefinition;
        },

        getGenericParameterCount: function (type) {
            return type.$typeArgumentCount || 0;
        },

        getGenericArguments: function (type) {
            return type.$typeArguments || null;
        },

        isGenericTypeDefinition: function (type) {
            return type.$isGenericTypeDefinition || false;
        },

        getBaseType: function (type) {
            if (type === Object || type.$kind === "interface" || type.prototype == null) {
                return null;
            } else if (Object.getPrototypeOf) {
                return Object.getPrototypeOf(type.prototype).constructor;
            } else {
                var p = type.prototype;

                if (Object.prototype.hasOwnProperty.call(p, "constructor")) {
                    var ownValue;

                    try {
                        ownValue = p.constructor;
                        delete p.constructor;
                        return p.constructor;
                    }
                    finally {
                        p.constructor = ownValue;
                    }
                }

                return p.constructor;
            }
        },

        getTypeFullName: function (obj) {
            var str;

            if (obj.$$fullname) {
                return obj.$$fullname;
            }

            if (obj.$$name) {
                return obj.$$name;
            }

            if ((obj).constructor === Function) {
                str = (obj).toString();
            } else {
                str = (obj).constructor.toString();
            }

            var results = (/function (.{1,})\(/).exec(str);

            return (results && results.length > 1) ? results[1] : "Object";
        },

        _makeQName: function (name, asm) {
            return name + (asm ? ', ' + asm.name : '');
        },

        getTypeQName: function (type) {
            return Bridge.Reflection._makeQName(Bridge.Reflection.getTypeFullName(type), type.$assembly);
        },

        getTypeName: function (type) {
            var fullName = Bridge.Reflection.getTypeFullName(type),
                bIndex = fullName.indexOf('['),
                nsIndex = fullName.lastIndexOf('.', bIndex >= 0 ? bIndex : fullName.length);

            return nsIndex > 0 ? fullName.substr(nsIndex + 1) : fullName;
        },

        getTypeNamespace: function (type) {
            var fullName = Bridge.Reflection.getTypeFullName(type),
                bIndex = fullName.indexOf('['),
                nsIndex = fullName.lastIndexOf('.', bIndex >= 0 ? bIndex : fullName.length),
                ns = nsIndex > 0 ? fullName.substr(0, nsIndex) : '';

            if (type.$assembly) {
                var parentType = Bridge.Reflection._getAssemblyType(type.$assembly, ns);

                if (parentType) {
                    ns = Bridge.Reflection.getTypeNamespace(parentType);
                }
            }

            return ns;
        },

        getTypeAssembly: function (type) {
            if (System.Array.contains([Date, Number, Boolean, String, Function, Array], type)) {
                return Bridge.SystemAssembly;
            } else {
                return type.$assembly || Bridge.SystemAssembly;
            }
        },

        _getAssemblyType: function (asm, name) {
            var noAsm = false;

            if (!asm) {
                asm = Bridge.SystemAssembly;
                noAsm = true;
            }

            if (asm.$types) {
                var t = asm.$types[name] || null;

                if (t) {
                    return t;
                }

                if (asm.name === "mscorlib") {
                    asm = Bridge.global;
                } else {
                    return null;
                }
            }

            var a = name.split('.'),
                scope = asm;

            for (var i = 0; i < a.length; i++) {
                scope = scope[a[i]];

                if (!scope) {
                    return null;
                }
            }

            if (typeof scope !== 'function' || !noAsm && scope.$assembly && asm.name !== scope.$assembly.name) {
                return null;
            }

            return scope;
        },

        getAssemblyTypes: function (asm) {
            var result = [];

            if (asm.$types) {
                for (var t in asm.$types) {
                    if (asm.$types.hasOwnProperty(t)) {
                        result.push(asm.$types[t]);
                    }
                }
            } else {
                var traverse = function (s, n) {
                    for (var c in s) {
                        if (s.hasOwnProperty(c)) {
                            traverse(s[c], c);
                        }
                    }

                    if (typeof (s) === 'function' && Bridge.isUpper(n.charCodeAt(0))) {
                        result.push(s);
                    }
                };

                traverse(asm, '');
            }

            return result;
        },

        createAssemblyInstance: function (asm, typeName) {
            var t = Bridge.Reflection.getType(typeName, asm);

            return t ? Bridge.createInstance(t) : null;
        },

        getInterfaces: function (type) {
            if (type.$allInterfaces) {
                return type.$allInterfaces;
            } else if (type === Date) {
                return [System.IComparable$1(Date), System.IEquatable$1(Date), System.IComparable, System.IFormattable];
            } else if (type === Number) {
                return [System.IComparable$1(Bridge.Int), System.IEquatable$1(Bridge.Int), System.IComparable, System.IFormattable];
            } else if (type === Boolean) {
                return [System.IComparable$1(Boolean), System.IEquatable$1(Boolean), System.IComparable];
            } else if (type === String) {
                return [System.IComparable$1(String), System.IEquatable$1(String), System.IComparable, System.ICloneable, System.Collections.IEnumerable, System.Collections.Generic.IEnumerable$1(System.Char)];
            } else if (type === Array || System.Array._typedArrays[Bridge.getTypeName(type)]) {
                return [System.Collections.IEnumerable, System.Collections.ICollection, System.ICloneable, System.Collections.Generic.IEnumerable$1(Object), System.Collections.Generic.ICollection$1(Object), System.Collections.Generic.IList$1(Object)];
            } else {
                return [];
            }
        },

        isInstanceOfType: function (instance, type) {
            return Bridge.is(instance, type);
        },

        isAssignableFrom: function (baseType, type) {
            if (baseType == null) {
                throw new System.NullReferenceException();
            }

            if (type == null) {
                return false;
            }

            if (baseType === type || baseType === Object) {
                return true;
            }

            if (Bridge.isFunction(baseType.isAssignableFrom)) {
                return baseType.isAssignableFrom(type);
            }

            if (type === Array) {
                return System.Array.is([], baseType);
            }

            if (Bridge.Reflection.isInterface(baseType) && System.Array.contains(Bridge.Reflection.getInterfaces(type), baseType)) {
                return true;
            }

            var inheritors = type.$$inherits,
                i,
                r;

            if (inheritors) {
                for (i = 0; i < inheritors.length; i++) {
                    r = Bridge.Reflection.isAssignableFrom(baseType, inheritors[i]);

                    if (r) {
                        return true;
                    }
                }
            }

            return false;
        },

        isClass: function (type) {
            return (type.$kind === "class" || type === Array || type === Function || type === RegExp || type === String || type === Error || type === Object);
        },

        isEnum: function (type) {
            return type.$kind === "enum";
        },

        isFlags: function (type) {
            return !!(type.prototype && type.prototype.$flags);
        },

        isInterface: function (type) {
            return type.$kind === "interface";
        },

        _getType: function (typeName, asm, re) {
            var outer = !re;

            re = re || /[[,\]]/g;

            var last = re.lastIndex,
                m = re.exec(typeName),
                tname,
                targs = [],
                t;

            if (m) {
                tname = typeName.substring(last, m.index);

                switch (m[0]) {
                    case '[':
                        if (typeName[m.index + 1] !== '[') {
                            return null;
                        }

                        for (; ;) {
                            re.exec(typeName);
                            t = Bridge.Reflection._getType(typeName, Bridge.SystemAssembly, re);

                            if (!t) {
                                return null;
                            }

                            targs.push(t);
                            m = re.exec(typeName);

                            if (m[0] === ']') {
                                break;
                            } else if (m[0] !== ',') {
                                return null;
                            }
                        }

                        m = re.exec(typeName);

                        if (m && m[0] === ',') {
                            re.exec(typeName);

                            if (!(asm = System.Reflection.Assembly.assemblies[(re.lastIndex > 0 ? typeName.substring(m.index + 1, re.lastIndex - 1) : typeName.substring(m.index + 1)).trim()])) {
                                return null;
                            }
                        }
                        break;

                    case ']':
                        break;

                    case ',':
                        re.exec(typeName);

                        if (!(asm = System.Reflection.Assembly.assemblies[(re.lastIndex > 0 ? typeName.substring(m.index + 1, re.lastIndex - 1) : typeName.substring(m.index + 1)).trim()])) {
                            return null;
                        }

                        break;
                }
            } else {
                tname = typeName.substring(last);
            }

            if (outer && re.lastIndex) {
                return null;
            }

            t = Bridge.Reflection._getAssemblyType(asm, tname.trim());

            t = targs.length ? t.apply(null, targs) : t;
            if (t && t.$staticInit) {
                t.$staticInit();
            }
            return t;
        },

        getType: function (typeName, asm) {
            return typeName ? Bridge.Reflection._getType(typeName, asm) : null;
        },

        canAcceptNull: function (type) {
            if (type.$kind === "struct" ||
                type === System.Decimal ||
                type === System.Int64 ||
                type === System.UInt64 ||
                type === System.Double ||
                type === System.Single ||
                type === System.Byte ||
                type === System.SByte ||
                type === System.Int16 ||
                type === System.UInt16 ||
                type === System.Int32 ||
                type === System.UInt32 ||
                type === Bridge.Int ||
                type === Boolean ||
                type === Date ||
                type === Number) {
                return false;
            }

            return true;
        },

        applyConstructor: function (constructor, args) {
            if (!args || args.length === 0) {
                return new constructor();
            }

            if (constructor.$$initCtor && constructor.$kind !== "anonymous") {
                var md = Bridge.getMetadata(constructor),
                    count = 0;

                if (md) {
                    var ctors = Bridge.Reflection.getMembers(constructor, 1, 28),
                        found;

                    for (var j = 0; j < ctors.length; j++) {
                        var ctor = ctors[j];

                        if (ctor.p && ctor.p.length === args.length) {
                            found = true;

                            for (var k = 0; k < ctor.p.length; k++) {
                                var p = ctor.p[k];

                                if (!Bridge.is(args[k], p) || args[k] == null && !Bridge.Reflection.canAcceptNull(p)) {
                                    found = false;
                                    break;
                                }
                            }

                            if (found) {
                                constructor = constructor[ctor.sn];
                                count++;
                            }
                        }
                    }
                } else {
                    if (Bridge.isFunction(constructor.ctor) && constructor.ctor.length === args.length) {
                        constructor = constructor.ctor;
                    } else {
                        var name = "$ctor",
                            i = 1;

                        while (Bridge.isFunction(constructor[name + i])) {
                            if (constructor[name + i].length === args.length) {
                                constructor = constructor[name + i];
                                count++;
                            }

                            i++;
                        }
                    }
                }

                if (count > 1) {
                    throw new System.Exception("The ambiguous constructor call");
                }
            }

            var f = function () {
                constructor.apply(this, args);
            };

            f.prototype = constructor.prototype;

            return new f();
        },

        getAttributes: function (type, attrType, inherit) {
            var result = [],
                i,
                t,
                a,
                md,
                type_md;

            if (inherit) {
                var b = Bridge.Reflection.getBaseType(type);

                if (b) {
                    a = Bridge.Reflection.getAttributes(b, attrType, true);

                    for (i = 0; i < a.length; i++) {
                        t = Bridge.getType(a[i]);
                        md = Bridge.getMetadata(t);

                        if (!md || !md.ni) {
                            result.push(a[i]);
                        }
                    }
                }
            }

            type_md = Bridge.getMetadata(type);

            if (type_md && type_md.at) {
                for (i = 0; i < type_md.at.length; i++) {
                    a = type_md.at[i];

                    if (attrType == null || Bridge.Reflection.isInstanceOfType(a, attrType)) {
                        t = Bridge.getType(a);
                        md = Bridge.getMetadata(t);

                        if (!md || !md.am) {
                            for (var j = result.length - 1; j >= 0; j--) {
                                if (Bridge.Reflection.isInstanceOfType(result[j], t)) {
                                    result.splice(j, 1);
                                }
                            }
                        }

                        result.push(a);
                    }
                }
            }

            return result;
        },

        getMembers: function (type, memberTypes, bindingAttr, name, params) {
            var result = [];

            if ((bindingAttr & 72) === 72 || (bindingAttr & 6) === 4) {
                var b = Bridge.Reflection.getBaseType(type);

                if (b) {
                    result = Bridge.Reflection.getMembers(b, memberTypes & ~1, bindingAttr & (bindingAttr & 64 ? 255 : 247) & (bindingAttr & 2 ? 251 : 255), name, params);
                }
            }

            var f = function (m) {
                if ((memberTypes & m.t) && (((bindingAttr & 4) && !m.is) || ((bindingAttr & 8) && m.is)) && (!name || m.n === name)) {
                    if (params) {
                        if ((m.p || []).length !== params.length) {
                            return;
                        }

                        for (var i = 0; i < params.length; i++) {
                            if (params[i] !== m.p[i]) {
                                return;
                            }
                        }
                    }

                    result.push(m);
                }
            };

            var type_md = Bridge.getMetadata(type);

            if (type_md && type_md.m) {
                var mNames = ['g', 's', 'ad', 'r'];

                for (var i = 0; i < type_md.m.length; i++) {
                    var m = type_md.m[i];

                    f(m);

                    for (var j = 0; j < 4; j++) {
                        var a = mNames[j];

                        if (m[a]) {
                            f(m[a]);
                        }
                    }
                }
            }

            if (bindingAttr & 256) {
                while (type) {
                    var r = [];

                    for (var i = 0; i < result.length; i++) {
                        if (result[i].td === type) {
                            r.push(result[i]);
                        }
                    }

                    if (r.length > 1) {
                        throw new System.Reflection.AmbiguousMatchException('Ambiguous match');
                    } else if (r.length === 1) {
                        return r[0];
                    }

                    type = Bridge.Reflection.getBaseType(type);
                }

                return null;
            }

            return result;
        },

        midel: function (mi, target, typeArguments) {
            if (mi.is && !!target) {
                throw new System.ArgumentException('Cannot specify target for static method');
            } else if (!mi.is && !target)
                throw new System.ArgumentException('Must specify target for instance method');

            var method;

            if (mi.fg) {
                method = function () { return (mi.is ? mi.td : this)[mi.fg]; };
            } else if (mi.fs) {
                method = function (v) { (mi.is ? mi.td : this)[mi.fs] = v; };
            } else {
                method = mi.def || (mi.is || mi.sm ? mi.td[mi.sn] : target[mi.sn]);

                if (mi.tpc) {
                    if (!typeArguments || typeArguments.length !== mi.tpc) {
                        throw new System.ArgumentException('Wrong number of type arguments');
                    }

                    var gMethod = method;

                    method = function () {
                        return gMethod.apply(this, typeArguments.concat(Array.prototype.slice.call(arguments)));
                    }
                } else {
                    if (typeArguments && typeArguments.length) {
                        throw new System.ArgumentException('Cannot specify type arguments for non-generic method');
                    }
                }

                if (mi.exp) {
                    var _m1 = method;

                    method = function () { return _m1.apply(this, Array.prototype.slice.call(arguments, 0, arguments.length - 1).concat(arguments[arguments.length - 1])); };
                }

                if (mi.sm) {
                    var _m2 = method;

                    method = function () { return _m2.apply(null, [this].concat(Array.prototype.slice.call(arguments))); };
                }
            }

            return Bridge.fn.bind(target, method);
        },

        invokeCI: function (ci, args) {
            if (ci.exp) {
                args = args.slice(0, args.length - 1).concat(args[args.length - 1]);
            }

            if (ci.def) {
                return ci.def.apply(null, args);
            } else if (ci.sm) {
                return ci.td[ci.sn].apply(null, args);
            } else {
                return Bridge.Reflection.applyConstructor(ci.sn ? ci.td[ci.sn] : ci.td, args);
            }
        },

        fieldAccess: function (fi, obj) {
            if (fi.is && !!obj) {
                throw new System.ArgumentException('Cannot specify target for static field');
            } else if (!fi.is && !obj) {
                throw new System.ArgumentException('Must specify target for instance field');
            }

            obj = fi.is ? fi.td : obj;

            if (arguments.length === 3) {
                obj[fi.sn] = arguments[2];
            } else {
                return obj[fi.sn];
            }
        }
    };

    Bridge.setMetadata = Bridge.Reflection.setMetadata;

    System.Reflection.ConstructorInfo = {
        $is: function (obj) {
            return obj != null && obj.t === 1;
        }
    };

    System.Reflection.EventInfo = {
        $is: function (obj) {
            return obj != null && obj.t === 2;
        }
    };

    System.Reflection.FieldInfo = {
        $is: function (obj) {
            return obj != null && obj.t === 4;
        }
    };

    System.Reflection.MethodBase = {
        $is: function (obj) {
            return obj != null && (obj.t === 1 || obj.t === 8);
        }
    };

    System.Reflection.MethodInfo = {
        $is: function (obj) {
            return obj != null && obj.t === 8;
        }
    };

    System.Reflection.PropertyInfo = {
        $is: function (obj) {
            return obj != null && obj.t === 16;
        }
    };

    System.AppDomain = {
        getAssemblies: function () {
            return Object.keys(System.Reflection.Assembly.assemblies).map(function (n) { return System.Reflection.Assembly.assemblies[n]; });
        }
    };
