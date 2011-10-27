/*
*
* Yardi javascript.
*
*/

// global
yardi = function () {
    // copy from extjs
    var docMode = document.documentMode,
    ua = navigator.userAgent.toLowerCase(),
    check = function (r) { return r.test(ua); },
    // Opera
    isOpera = check(/opera/),
    isOpera10_5 = isOpera && check(/version\/10\.5/),
    // Chrome
    isChrome = check(/\bchrome\b/),
    // Safari
    isWebKit = check(/webkit/),
    isSafari = !isChrome && check(/safari/),
    isSafari2 = isSafari && check(/applewebkit\/4/),
    isSafari3 = isSafari && check(/version\/3/),
    isSafari4 = isSafari && check(/version\/4/),
    // IE
    isIE = !isOpera && check(/msie/),
    isIE7 = isIE && (check(/msie 7/) || docMode == 7),
    isIE8 = isIE && (check(/msie 8/) && docMode != 7 && docMode != 9 || docMode == 8),
    isIE9 = isIE && (check(/msie 9/) && docMode != 7 && docMode != 8 || docMode == 9),
    isIE6 = isIE && check(/msie 6/),
    //Netscape, Firefox
    isGecko = !isWebKit && check(/gecko/),
    isGecko2 = isGecko && check(/rv:1\.8/),
    isGecko3 = isGecko && check(/rv:1\.9/),
    isGecko4 = isGecko && check(/rv:2\.0/),
    // others
    isWindows = check(/windows|win32/),
    isMac = check(/macintosh|mac os x/),
    isLinux = check(/linux/);

    var OP = Object.prototype;

    var ADD = ["toString", "valueOf"];

    var _hasOwnProperty = (OP.hasOwnProperty) ? function (o, prop) {
        return o && o.hasOwnProperty(prop);
    } : function (o, prop) {
        return !yardi.isUndefined(o[prop]) && o.constructor.prototype[prop] !== o[prop];
    };

    var _IEEnumFix = (isIE) ? function (r, s) {
        var i, fname, f;
        for (i = 0; i < ADD.length; i = i + 1) {
            fname = ADD[i];
            f = s[fname];
            if (yardi.isFunction(f) && f != OP[fname]) {
                r[fname] = f;
            }
        }
    } : function () { };

    // apis
    return {

        isOpera: isOpera,
        isOpera10_5: isOpera10_5,
        isChrome: isChrome,
        isWebKit: isWebKit,
        isSafari: isSafari,
        isSafari2: isSafari2,
        isSafari3: isSafari3,
        isSafari4: isSafari4,
        isIE: isIE,
        isIE7: isIE7,
        isIE8: isIE8,
        isIE9: isIE9,
        isIE6: isIE6,
        isGecko: isGecko,
        isGecko2: isGecko2,
        isGecko3: isGecko3,
        isGecko4: isGecko4,
        isWindows: isWindows,
        isLinux: isLinux,
        isMac: isMac,

        ieEnumFix: _IEEnumFix,

        hasOwnProperty: _hasOwnProperty,

        isUndefined: function (o) {
            return typeof o === 'undefined';
        },

        isArray: function (v) {
            return OP.toString.apply(v) === '[object Array]';
        },

        isFunction: function (o) {
            return OP.toString.apply(o) === '[object Function]';
        },

        isString: function (v) {
            return typeof v === 'string';
        },

        isBoolean: function (v) {
            return typeof v === 'boolean';
        },

        isNumber: function (v) {
            return typeof v === 'number' && isFinite(v);
        },

        setTimeout: function (func, args, time, scope) {
            return window.setTimeout(function () {
                func.apply(scope || window, args || []);
            }, time || 0);
        },

        setInterval: function (func, args, time, scope) {
            return window.setInterval(function () {
                func.apply(scope || window, args || []);
            }, time || 0);
        }
    };

} ();

yardi = function (yardi, $) {

    // custom html tag
    var defaultCustomTag = 'yardi';
    yardi.get = function (html, customTag) {
        customTag = customTag || defaultCustomTag;
        if (!yardi.isString(html) || html.indexOf('<' + customTag + ' ') === -1) {
            return $(html);
        } else {
            var doc = window.document;
            doc.createElement(customTag);
            var div = doc.createElement('div');
            doc.body.appendChild(div);
            div.innerHTML = html;
            var ret = $(div.childNodes);
            div.parentNode.removeChild(div);
            return ret;
        }
    };

    // amazing implement
    var ie = (function () {
        var undef, v = 3,
        div = document.createElement('div'),
        all = div.getElementsByTagName('i');
        while (div.innerHTML = '<!--[if gt IE ' + (++v) + ']><i></i><![endif]-->', all[0]);
        return v > 4 ? v : undef;
    } ());

    /*
    * extend object inherit function
    */
    yardi.extend = function (subc, superc, overrides) {
        var F = function () { }, i;
        F.prototype = superc.prototype;
        subc.prototype = new F();
        subc.prototype.constructor = subc;
        subc.superclass = superc.prototype;
        if (superc.prototype.constructor == Object.prototype.constructor) {
            superc.prototype.constructor = superc;
        }
        if (overrides) {
            for (i in overrides) {
                if (yardi.hasOwnProperty(overrides, i)) {
                    subc.prototype[i] = overrides[i];
                }
            }
            yardi.ieEnumFix(subc.prototype, overrides);
        }
    };

    /*
    * javascript template small engine.
    */
    var Template = function (template, pattern) {
        this.template = String(template);
        this.pattern = pattern || Template.Pattern;
    };
    Template.Pattern = /#\{([^}]*)\}/mg;
    Template.trim = String.trim || function (str) {
        return str.replace(/^\s+|\s+$/g, '');
    };
    Template.prototype = {
        constructor: Template,
        compile: function (object) {
            return this.template.replace(this.pattern, function (displace, variable) {
                variable = Template.trim(variable);
                return displace = object[variable];
            });
        }
    };
    yardi.template = Template;

    /*
    * condition monitor.
    */
    var Monitor = function (config) {
        $.extend(this, config);
        this.initialize();
    };
    Monitor.prototype = {
        scope: null,
        tester: null,
        handler: null,
        interval: null,
        initialize: function () {
            if (!this.interval) { this.interval = 50; }
            if (!this.scope) { this.scope = window; }
        },
        test: function () {
            (this.tester.call(this.scope) === true) && this.handler.call(this.scope);
        },
        start: function () {
            this.stop();
            var self = this;
            this._id = setInterval(function () {
                self.test();
            }, this.interval);
        },
        stop: function () {
            clearInterval(this._id);
            this._id = null;
        }
    };
    yardi.Monitor = Monitor;

    /*
    * custom event dispatcher
    */
    var Dispatcher = function (scope) {
        this.scope = scope || this;
        this.listeners = [];
    };
    Dispatcher.prototype = {
        scope: null,
        listeners: null,
        add: function (fn, scope) {
            var item = { fn: fn, scope: scope || this.scope };
            this.listeners.push(item);
            return item;
        },
        addToTop: function (fn, scope) {
            var item = { fn: fn, scope: scope || this.scope };
            this.listeners.unshift(item);
            return item;
        },
        remove: function (fn) {
            var cache;
            for (var i = 0; i < this.listeners.length; i++) {
                var item = this.listeners[i];
                if (fn == item.fn) {
                    cache = item;
                    this.listeners.splice(i, 1);
                }
            }
            return cache;
        },
        dispatch: function () {
            // needs to be a real loop since the listener count might change while looping, and this is also more efficient
            var result;
            for (var i = 0; i < this.listeners.length; i++) {
                var item = this.listeners[i];
                result = item.fn.apply(item.scope, arguments);
                if (result === false)
                    break;
            }
            return result;
        }
    };
    yardi.dispatcher = Dispatcher;

    /*
    * get viewport size
    */
    yardi.getViewportSize = function (win) {
        var w = win || window, doc = w.document, docEl = doc.documentElement;
        var mode = doc.compatMode, width = w.self.innerWidth, height = w.self.innerHeight; // Safari, Opera
        // get
        if ((mode || yardi.isIE) && !yardi.isOpera) { // IE, Gecko
            height = (mode == 'CSS1Compat') ?
                        docEl.clientHeight : // Standards
                        doc.body.clientHeight; // Quirks
            width = (mode == 'CSS1Compat') ?
                        docEl.clientWidth : // Standards
                        doc.body.clientWidth; // Quirks
        }
        // ret
        return { width: width, height: height };
    };

    /*
    * get element's position
    */
    yardi.getElementPosition = function (element) {
        var result = { x: 0, y: 0, width: 0, height: 0 };
        if (element.offsetParent) {
            result.x = element.offsetLeft;
            result.y = element.offsetTop;
            var parent = element.offsetParent;
            while (parent) {
                result.x += parent.offsetLeft;
                result.y += parent.offsetTop;
                var parentTagName = parent.tagName.toLowerCase();
                if (parentTagName != "table" &&
                parentTagName != "body" &&
                parentTagName != "html" &&
                parentTagName != "div" &&
                parent.clientTop &&
                parent.clientLeft) {
                    result.x += parent.clientLeft;
                    result.y += parent.clientTop;
                }
                parent = parent.offsetParent;
            }
        }
        else if (element.left && element.top) {
            result.x = element.left;
            result.y = element.top;
        }
        else {
            if (element.x) {
                result.x = element.x;
            }
            if (element.y) {
                result.y = element.y;
            }
        }
        if (element.offsetWidth && element.offsetHeight) {
            result.width = element.offsetWidth;
            result.height = element.offsetHeight;
        }
        else if (element.style && element.style.pixelWidth && element.style.pixelHeight) {
            result.width = element.style.pixelWidth;
            result.height = element.style.pixelHeight;
        }
        return result;
    };

    /*
    * get mouse position.
    */
    yardi.getMousePosition = function (e) {
        var result = { x: 0, y: 0 };
        if (!e) e = window.event;
        if (e.pageX || e.pageY) {
            result.x = e.pageX;
            result.y = e.pageY;
        } else if (e.clientX || e.clientY) {
            result.x = e.clientX + (document.documentElement.scrollLeft ? document.documentElement.scrollLeft : document.body.scrollLeft);
            result.y = e.clientY + (document.documentElement.scrollTop ? document.documentElement.scrollTop : document.body.scrollTop);
        }
        return result;
    };

    /*
    * parse color value to hex format.
    */
    yardi.colorHex = function (value) {
        if (yardi.isNumber(value)) {
            var c = value.toString(16).toUpperCase();
            switch (c.length) {
                case 1:
                    c = '0' + c + '0000';
                    break;
                case 2:
                    c = c + '0000';
                    break;
                case 3:
                    c = c.substring(1, 3) + '0' + c.substring(0, 1) + '00';
                    break;
                case 4:
                    c = c.substring(2, 4) + c.substring(0, 2) + '00';
                    break;
                case 5:
                    c = c.substring(3, 5) + c.substring(1, 3) + '0' + c.substring(0, 1);
                    break;
                case 6:
                    c = c.substring(4, 6) + c.substring(2, 4) + c.substring(0, 2);
                    break;
                default:
                    c = '';
            }
            return '#' + c;
        }
        if (yardi.isString(value)) {
            var toHex = function (num) {
                if (num == null)
                    return '00';
                num = parseInt(num);
                if (num == 0 || isNaN(num))
                    return '00';
                num = Math.max(0, num);
                num = Math.min(num, 255);
                num = Math.round(num);
                return '0123456789ABCDEF'.charAt((num - num % 16) / 16) + '0123456789ABCDEF'.charAt(num % 16);
            };
            var hex = '#';
            value = (value || '').toUpperCase();
            if (value == 'TRANSPARENT') {
                hex = 'transparent';
            } else if (value.indexOf('RGB') == 0) {
                value.replace(/\d{1,3}(,|\))/g, function (match) {
                    match = match.substr(0, match.length - 1);
                    hex += toHex(parseInt(match, 10));
                });
            } else if (value.indexOf('#') == 0) {
                hex = value;
            } else {
                hex += '000000'; //default
            }
            return hex;
        }
    };

    /*
    * size unit parser, support units: px,pt,em.
    */
    var sizeUnitParser = function (value, type) {
        type = (type || '').toLowerCase();
        this.oldValue = (value.toString() || '').toLowerCase();
        this.isPx = (this.oldValue.indexOf('px') != -1) || (type == 'px');
        this.isPt = (this.oldValue.indexOf('pt') != -1) || (type == 'pt');
        this.isEm = (this.oldValue.indexOf('em') != -1) || (type == 'em');
        this.value = parseFloat(this.oldValue.replace(/\D/, ''));
        this.hasType = (this.isPx || this.isPt || this.isEm);
    };
    sizeUnitParser.prototype = {
        value: 0,
        oldValue: null,
        hasType: false,
        isPx: false, isPt: false, isEm: false,
        _num: function (args) { return (args.length > 0 && args[0] === true); },
        toPx: function () {
            if (this.hasType == false) throw new Error('parse size error!');
            var ret = 0;
            if (this.isPx) {
                ret = this.value;
            } else if (this.isPt) {
                ret = this.value * 4 / 3;
            } else if (this.isEm) {
                ret = this.value * 16;
            }
            return this._num(arguments) ? ret : ret + 'px';
        },
        toPt: function () {
            var px = this.toPx(true);
            var ret = px * 3 / 4;
            return this._num(arguments) ? ret : ret + 'pt';
        },
        toEm: function () {
            var px = this.toPx(true);
            var ret = px / 16;
            return this._num(arguments) ? ret : ret + 'em';
        }
    };
    yardi.sizeUnitParser = sizeUnitParser;

    /*
    * get flat position
    */
    yardi.flatPos = function (el, refEl) {
        var refOffset = refEl.offset();
        var refHeight = refEl.outerHeight(), refWidth = refEl.outerWidth();
        var selHeight = el.outerHeight(), selWidth = el.outerWidth();
        var winHeight = $(window).height(), winWidth = $(window).width();
        var scrollTop = $(window).scrollTop(), scrollLeft = $(window).scrollLeft();
        var left = 0, top = 0;
        if (refOffset.top + refHeight - scrollTop + selHeight > winHeight) {
            top = refOffset.top - selHeight - 1;
        } else {
            top = refOffset.top + refHeight + 1;
        }
        if (refOffset.left + refWidth - scrollLeft + selWidth > winWidth) {
            left = refOffset.left + refWidth - selWidth;
        } else {
            left = refOffset.left;
        }
        return { left: left, top: top };
    };

    /*
    * check if c element is a children of p element. (copy from extjs)
    */
    yardi.isAncestor = function (p, c) {
        var ret = false;
        if (p && c) {
            if (p.contains) {
                return p.contains(c);
            } else if (p.compareDocumentPosition) {
                return !!(p.compareDocumentPosition(c) & 16);
            } else {
                while (c = c.parentNode) {
                    ret = c == p || ret;
                }
            }
        }
        return ret;
    };

    /*
    * get the runtime style.
    */
    yardi.currentStyle = function (el, p) {
        var s = el.currentStyle || el.ownerDocument.defaultView.getComputedStyle(el, null);
        if (p === undefined) {
            return s;
        } else {
            p = p.replace(/(-[a-z])/gi, function (m, a) {
                return a.charAt(1).toUpperCase();
            });
            return s[p];
        }
    };

    /*
    * String extensions
    */
    String.prototype.trim = function () {
        var str = this, whitespace = ' \n\r\t\f\x0b\xa0\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200a\u200b\u2028\u2029\u3000';
        for (var i = 0, len = str.length; i < len; i++) {
            if (whitespace.indexOf(str.charAt(i)) === -1) {
                str = str.substring(i);
                break;
            }
        }
        for (i = str.length - 1; i >= 0; i--) {
            if (whitespace.indexOf(str.charAt(i)) === -1) {
                str = str.substring(0, i + 1);
                break;
            }
        }
        return whitespace.indexOf(str.charAt(0)) === -1 ? str : '';
    };

    /*
    * innerHTML, outerHTML
    */
    if (!String.prototype.quote) {
        var metaObject = {
            '\b': '\\b',
            '\t': '\\t',
            '\n': '\\n',
            '\f': '\\f',
            '\r': '\\r',
            '\\': '\\\\'
        };
        String.prototype.quote = function () {
            str = this.replace(/[\x00-\x1f\\]/g, function (chr) {
                var special = metaObject[chr];
                return special ? special : '\\u' + ('0000' + chr.charCodeAt(0).toString(16)).slice(-4)
            });
            return '"' + str.replace(/"/g, '\\"') + '"';
        };
    }
    var rselfClosing = /^(?:area|br|col|embed|hr|img|input|link|meta|param)$/i;
    var outerHTML = function (el, recursion) {
        switch (el.nodeType + "") {
            case "1":
                var array = [];
                var nodeName = el.nodeName;
                if (recursion && yardi.currentStyle(el, "display") == "block") {
                    array.push("\n")
                }
                array.push("<" + nodeName);
                for (var i = 0, t; t = el.attributes[i++]; ) {
                    array.push(" " + t.name + "=" + (t.value || t.specified + "").quote())
                }
                if (rselfClosing.test(el.nodeName)) {
                    array.push("\/>")
                } else {
                    array.push(">");
                    for (var i = 0, c; c = el.childNodes[i++]; ) {
                        array.push(outerHTML(c, true))
                    }
                    array.push("<\/" + el.nodeName + ">")
                }
                return array.join("");
            case "3":
                return el.nodeValue.trim();
            case "8":
                return "innerHTML" in el ? el.innerHTML : "<!--" + el.nodeValue + "-->"
        }
    };
    var innerHTML = function (el) {
        var array = [];
        for (var i = 0, c; c = el.childNodes[i++]; ) {
            array.push(outerHTML(c, true))
        }
        return array.join("");
    };
    yardi.outerHTML = outerHTML;
    yardi.innerHTML = innerHTML;

    /*
    * max zIndex
    */
    var zIndexMax;
    yardi.zTop = function (el) {
        if (zIndexMax === undefined) {
            zIndexMax = function () {
                var zmax;
                $('*').each(function () {
                    var cur = parseInt($(this).css('z-index'));
                    zmax = cur > (zmax || 0) ? cur : zmax;
                });
                return zmax;
            } () || 0;
            yardi.zIndexMax = zIndexMax;
        }
        yardi.zOld(el);
        var old = parseInt(el.css('z-index')) || 0;
        return el.attr('zold', old).css('z-index', old + zIndexMax);
    };
    yardi.zOld = function (el) {
        var zold = el.attr('zold');
        if (!zold) { return el; }
        return el.css('z-index', (parseInt(zold) || 0)).removeAttr('zold');
    };

    /*
    * resize event
    * fix  resize bug.
    * when scroll bar appeared, the window does not trigger the resize event
    */
    var resizeUtil = function () {
        var resizeCache = {};
        var dataKey = 'resize_datakey';
        return {
            hashCode: function (s) {
                var h = 31; // empty default
                var i = 0;
                var l = s.length;
                while (i < l)
                    h ^= (h << 5) + (h >> 2) + s.charCodeAt(i++);
                return h;
            },
            genKey: function (el, fn) {
                var elkey = el.data(dataKey);
                if (!elkey) {
                    elkey = Math.random();
                    el.data(dataKey, elkey);
                }
                var fnkey = fn ? this.hashCode(fn.toString()) : '';
                return elkey + '_' + fnkey;
            },
            register: function (el, fn) {
                if (!fn) { return; }
                var key = this.genKey(el, fn);
                if (resizeCache[key]) { return; }
                var cache = resizeCache[key] = {};
                // do wrap
                cache.wrap = function (o, c) {
                    return function () {
                        if (c.monitor) {
                            c.monitor.stop();
                            setTimeout(function () {
                                c.oldSize = { w: o.width(), h: o.height() };
                                c.monitor.start();
                            }, 128);
                        }
                        fn.apply(this, arguments);
                    }
                } (el, cache);
                // monitor
                (function (o, c) {
                    c.oldSize = { w: o.width(), h: o.height() };
                    c.monitor = new Monitor({
                        scope: o,
                        interval: 32,
                        handler: c.wrap,
                        tester: function () {
                            var size = { w: this.width(), h: this.height() };
                            if (c.oldSize.w != size.w || c.oldSize.h != size.h) {
                                c.oldSize.w = size.w; c.oldSize.h = size.h;
                                return true;
                            }
                        }
                    });
                    c.monitor.start();
                })(el, cache);
            },
            unregister: function (el, fn) {
                var key = this.genKey(el, fn);
                for (var k in resizeCache) {
                    if (k.indexOf(key) !== 0)
                        continue;
                    var cache = resizeCache[k];
                    if (cache) {
                        cache.monitor.stop();
                        delete resizeCache[k];
                    }
                }
            }
        }
    } ();
    $.fn.monitorResize = function (fn) { this.each(function () { resizeUtil.register($(this), fn); }); };
    $.fn.unmonitorResize = function (fn) { this.each(function () { resizeUtil.unregister($(this), fn); }); };

    /*
    * test scroll
    */
    yardi.isScroll = function (el) {
        // test targets
        var elems = el ? [el] : [document.documentElement, document.body];
        var scrollX = false, scrollY = false;
        for (var i = 0; i < elems.length; i++) {
            var o = elems[i];
            // test horizontal
            var sl = o.scrollLeft;
            o.scrollLeft += (sl > 0) ? -1 : 1;
            o.scrollLeft !== sl && (scrollX = scrollX || true);
            o.scrollLeft = sl;
            // test vertical
            var st = o.scrollTop;
            o.scrollTop += (st > 0) ? -1 : 1;
            o.scrollTop !== st && (scrollY = scrollY || true);
            o.scrollTop = st;
        }
        // ret
        return {
            scrollX: scrollX,
            scrollY: scrollY
        };
    };
    var _scrollBarWidth;
    yardi.scrollBarWidth = function () {
        if (_scrollBarWidth) { return _scrollBarWidth; }
        var helper = document.createElement('div');
        helper.style.cssText = 'overflow:scroll;width:100px;height:100px;';
        document.body.appendChild(helper);
        _scrollBarWidth = {
            vertical: helper.offsetWidth - helper.clientWidth,
            horizontal: helper.offsetHeight - helper.clientHeight
        };
        document.body.removeChild(helper);
        return _scrollBarWidth;
    };

    // ret
    return yardi;

} (yardi, jQuery);
