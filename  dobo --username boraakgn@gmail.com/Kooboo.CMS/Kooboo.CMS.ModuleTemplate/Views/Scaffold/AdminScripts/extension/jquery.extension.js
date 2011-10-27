/// <reference path="../jquery-1.4.4-vsdoc.js" />

/// <reference path="../jQueryUI/jquery-ui-1.8.4.all.min.js" />

/// <reference path="../kooboo.js" />

///jQuery extension
(function () {
    jQuery.fn.extend({
        isString: function () {
            return typeof (this) == 'string';
        },
        isFunction: function () {
            return typeof (this) == 'function';
        },
        isObject: function () {
            return typeof (this) == 'object';
        }
    }); //end jQuery.fn.extend


    ///-------------jQuery.fn.pop------------
    var __apiMap = {};
    var __configMap = {};
    var __divStr = "<div></div>";
    var __iframe = '<iframe  frameborder="0" allowTransparency="true"/>';

    jQuery.fn.pop = function (option) {
        this.each(function () {
            $(this).__pop(option);
        });
        return this;
    }

    jQuery.fn.__pop = function (option) {
        var config = {
            id: '',
            url: '', // url will being getted from href prior
            title: '', // you dont need to config this if you link element has title attribute
            isIframe: true,
            useUrl: true, // 
            useContent: false,
            contentId: '', // may be a function like function(handle,pop,config){  return contentID ;};
            useClone: false, //this will clone pop content every time

            events: 'click',

            cloneId: 'pop-clone-' + Math.random().toString().replace('.', '-'),

            reload: true, // reload content every time when open

            popupOnTop: false, // show pop on parent page

            openOnclick: true,

            allowTransparency: true,
            frameborder: '0',
            border: '0',

            frameWidth: '100%',
            frameHeight: 'auto',
            unbindEvents: 'click',

            width: 780, //jquery dialog option
            height: 560, //jquery dialog option
            show: 'fade', //jquery dialog option
            autoOpen: false, //jquery dialog option
            modal: true, //jquery dialog option

            onopen: function (currentHandle, pop, config) { },
            beforeLoad: function (currentHandle, pop, config) { }, // before pop content loaded
            onload: function (currentHandle, pop, config) { }, // on pop content loaded 
            onclose: function (currentHandle, pop, config) { } // on pop closed
        };

        if ($(option).isString()) {
            config.id = option;
        } else if ($(option).isObject()) {
            $.extend(config, option);
        }

        config.id = (config.id != '' ? config.id : Math.random().toString().replace('.', '_'));
        if (__apiMap[config.id]) {
            return __apiMap[config.id];
        }

        var current = this;
        current.data("POPHANDLE", true);
        var pop = $(__divStr);
        config.close = function () {

            config.onclose(current, pop, config);
            pop.find('iframe').attr('src', 'about:blank');
            $.popContext.popList.pop();
        }
        if (config.popupOnTop) {
            pop = top.jQuery(__divStr).appendTo(top.document.getElementsByTagName("body").item(0));
        } else {
            pop.appendTo("body");
        }

        pop.attr('id', config.id);
        pop.hide();

        if (this.attr('title') && this.attr('title').length > 0) {
            config.title = this.attr('title');
        }
        pop.dialog(config);

        if (config.openOnclick) {
            function showPop() {
                show();
                $.popContext.popList.push(pop);
                pop.dialog('open');
            }
            this.unbind(config.unbindEvents).keydown(function () {
                showPop();
                return false;
            }).mousedown(function (e) {
                if (e.which != 3) {
                    showPop()
                }
                return false;
            })
            .bind("contextmenu", function () { return false; }).click(function () {
                showPop();
                return false;
            });
        } else {
            show();
            $.popContext.popList.push(pop);
        }
        function loadContent() {
            config.beforeLoad(current, pop, config);
            if (current.attr('href')) {
                config.url = current.attr('href');
            }

            current.data('loaded', true);
            if (config.url == '') {
                if (current.attr('url') && current.attr('url').length > 0) {
                    config.url = current.attr('url');
                } else if (current.attr('href') && current.attr('href').length > 0) {
                    config.url = current.attr('href');
                } else if (current.attr('src') && current.attr('src').length > 0) {
                    config.url = current.attr('src');
                }
            }

            if (config.url != '' && config.useUrl && !config.useContent && !config.useText) {
                if (!config.isIframe) {
                    $.ajax({
                        url: config.url,
                        success: function (data) {
                            pop.html(data);
                            config.onload(current, pop, config);
                            config.onopen(current, pop, config);
                        },
                        error: function () {
                            pop.html('some errors occur during request!');
                        }
                    });
                } else {
                    var iframe = $();

                    if (pop.find('iframe').length == 0) {
                        iframe = $(__iframe).appendTo(pop);
                        iframe.css('width', config.frameWidth);
                        iframe.attr('frameborder', config.frameborder);
                        iframe.attr('allowTransparency', config.allowTransparency);

                        iframe.attr('border', config.border);
                        iframe.attr('src', config.url);
                        iframe.attr('id', "pop_iframe" + config.id);
                        if (config.height <= 200) {

                            iframe.css("height", "90%");
                        }

                        if (config.frameHeight == 'auto') {
                            var loadCount = 0;

                            iframe.load(function () {

                                var height = iframe.contents().height();
                                iframe.height(height);
                                loadCount++;
                            });
                        } else {
                            iframe.height(config.frameHeight);
                        }
                        iframe.load(function () {
                            var $fromPop = iframe.contents().find("input:hidden[name=FromPop]");
                            if ($fromPop.length == 0) {
                                $fromPop = $('<input type="hidden" name="FromPop" value="true"/>').appendTo(iframe.contents().find("form"));
                            }
                            pop.iframe = iframe;
                            config.onload(current, pop, config);
                            config.onopen(current, pop, config);
                        });
                    } else {
                        iframe = pop.children('iframe');
                        iframe.attr('src', config.url);
                        config.onload(current, pop, config);
                        config.onopen(current, pop, config);
                    }
                }
            } else if (config.useContent) {
                if (typeof (config.contentId) == 'function') {
                    config.contentId = config.contentId(current, pop, config);
                }
                var source = $('#' + config.contentId);
                if (config.useClone) {
                    source = source.clone();

                    if (typeof (config.cloneId) == 'function') {
                        source.attr('id', config.cloneId(source));
                        source.find('[id]').each(function () {
                            $(this).attr('id', config.cloneId($(this)));
                        });

                    } else {
                        var sid = source.attr('id');
                        source.attr('id', sid + config.cloneId);
                        source.find('[id]').each(function () {
                            var id = $(this).attr('id');
                            $(this).attr('id', id + '-' + config.cloneId);
                        });
                    }
                    source.appendTo(pop).show();
                } else {
                    if (!current.data('pop-content-loaded')) {
                        source.appendTo(pop).show();
                        current.data('pop-content-loaded', true);
                    }
                }
                config.onload(current, pop, config);
            }
        }
        pop.close = close;
        function close() {
            pop.dialog('close');
        }
        function destory() {
            try {
                CollectGarbage(); //
            } catch (E) {
            }
        }
        pop.destory = function () {
            pop.dialog("destory");
        }
        pop.open = function () {
            pop.dialog('open');
        }
        function show() {

            if (config.reload || !current.data('loaded')) {
                if (current.attr('href')) {
                    config.url = current.attr('href') || config.url;
                }
                loadContent();
            }
        }


        var api = {};
        api.show = show;
        api.reload = loadContent;
        api.close = close;
        api.destory = destory;
        api.getIframe = function () {
            ///return juery object
            return pop.children('iframe');
        }
        api.$ = pop.dialog();
        __apiMap[config.id] = api;
        return api;
    }

    

    if (top.jQuery && top.jQuery.popContext) {
        jQuery.popContext = top.jQuery.popContext;
    } else {
        jQuery.popContext = {
            popList: [],
            getCurrent: function () {
                if (this.popList.length > 0) {
                    return this.popList[this.popList.length - 1];
                }
                return null;
            }
        };
    }

    $.pop = function (option) {
        option = option || {};
        option.autoOpen = true;
        var handle = $('<a></a>');
        if (option.url) {
            handle.attr('href', option.url);
        }

        handle.pop(option);

        setTimeout(function () {
            handle.click();
        }, 10);
    };
    ///end --- jQuery.fn.pop----------------


    $.request = (function () {
        var apiMap = {};

        function request(queryStr) {
            var api = {};

            if (apiMap[queryStr]) {
                return apiMap[queryStr];
            }

            api.queryString = (function () {
                var urlParams = {};
                var e,
                d = function (s) { return decodeURIComponent(s.replace(/\+/g, " ")); },
                q = queryStr.substring(queryStr.indexOf('?') + 1),
                r = /([^&=]+)=?([^&]*)/g;

                while (e = r.exec(q))
                    urlParams[d(e[1])] = d(e[2]);


                return urlParams;
            })();

            api.getUrl = function () {
                var url = queryStr.indexOf('?') > 0 ? queryStr.substring(0, queryStr.indexOf('?') + 1) : queryStr;

                for (var p in api.queryString) {
                    url += p + '=' + api.queryString[p] + "&";
                }

                if (url.lastIndexOf('&') == url.length - 1) {
                    return url.substring(0, url.lastIndexOf('&'));
                }
                return url;
            }

            apiMap[queryStr] = api;
            return api;
        }

        $.extend(request, request(window.location.href));

        return request;

    })();

    $.fn.koobooTab = function (option) {
        var config = {
            containerClass: "tab-content",
            currentClass: "current",
            tabClass: "tab-content",
            event: "click",
            tabContentTag: "<div></div>",
            showTabIndex: 0
        };

        var tabContents = this.find('.' + config.tabClass).hide();
        var tabMap = [];
        $.extend(config, option);
        var ul = this.find("ul");

        var index = 0;
        ul.find("li a").each(function (val) {
            var current = $(this);
            var tabApi = initTab(current, index);
            index++;
        });

        function initTab(current, index) {
            var href = current.attr("href");
            if (href.indexOf("#") == 0) {
                current.bind(config.event, function () {
                    api.show();
                });
            }

            var api = {
                show: function () {
                    var expire = /#\S+$/;
                    var id = expire.exec(href);
                    if (id && id[0]) {
                        tabContents.hide();
                        current.parent().siblings().removeClass(config.currentClass);
                        current.parent().addClass(config.currentClass);
                        $(id[0]).show();
                    }

                }
            };
            tabMap.push({ id: href, handle: current, tab: api, index: index });
            return api;
        }



        var api = {
            showTab: function (tab) {
                tab = tab == undefined ? 0 : tab;
                var tabObj;
                if (typeof (tab) == 'string') {
                    tabObj = tabMap.where(function (o) { return o.id == tab; }).first();
                } else if (!isNaN(tab)) {
                    tabObj = tabMap.where(function (val, index) { return index == tab; }).first();
                }
                tabObj = tabObj ? tabObj : tabMap.first();
                tabObj.tab.show();
            }
        };

        this.data('koobooTab', api);

        $(function () {
            var url = $.request.getUrl();
            var expire = /#\w+$/;
            var tab = config.showTabIndex;

            if (expire.test(url)) {
                var tabId = expire.exec(url);
                tab = tabId[0];
            }

            api.showTab(tab);

        });

        return this;
    }
})();                                                                                                                                                                                         //end jquery.extend
$.fn.addHidden = function (name, val, fieldSet) {
    if (this.find('[name="' + name + '"]').length > 0) {
        this.find('[name="' + name + '"]').val(val);
        return this;
    }
    var hiddenInput = $('<input type="hidden"/>').val(val).attr("name", name);

    if (fieldSet) {
        var fieldSet = this.find("fieldset[name=" + fieldSet + "]");
        if (fieldSet.length > 0) {
            fieldSet.append(hiddenInput);
        } else {
            fieldSet = $("<fieldset></fieldset>").attr("name", fieldSet).appendTo(this);
        }
    }
    this.append(hiddenInput);
}
$.fn.setFormField = function (name, val) {
    this.find("[name=" + name + "]").val(val);
}


$.fn.tableSorter = function (optoin) {
    var config = {
        items: "tr",
        cancel: "",
        divInsertAt: 1,
        beforeUp: function () { },
        beforeDown: function () { },
        up: function (handle) { },
        down: function (handle) { },
        move: function (handle) { },
        showUp: function (handle) { },
        showDown: function (handle) { }
    };
    $.extend(config, optoin);
    var sorter = $();
    function createCursorNearDiv(handle, left, cfg) {
        handle = $(handle);
        cfg = config;

        sorter = $('<span><a href="javascript:" class="o-icon move-up inline-action">Move up</a> <a href="javascript:" class="o-icon move-down inline-action">Move up</a></span>');

        sorter.appendTo(handle.find("td:eq(" + cfg.divInsertAt + ")"));

        var up = sorter.find("a.move-up");
        var down = sorter.find("a.move-down");

        if (cfg.showUp(handle) == false) {
            up.hide();
        }
        if (cfg.showDown(handle) == false) {
            down.hide();
        }

        up.click(function () {
            if (cfg.beforeUp(handle) == false) {
                return false;
            }
            var prev = handle.prev();
            if (prev.length > 0) {
                prev.before(handle);
            } else {
                return false;
            }
            cfg.up(handle);
            cfg.move();
            sorter.remove();
        });


        down.click(function () {
            if (cfg.beforeDown(handle) == false) {
                return false;
            }
            var next = handle.next();
            if (next.length > 0) {
                next.after(handle);
            } else {
                return false;
            }
            cfg.down(handle);
            cfg.move();
            sorter.remove();
        });
    }

    var table = $(this);
    var items = table.find("tr:not(" + config.cancel + ")");
    items.hover(function (e) {
        var handle = $(this);
        createCursorNearDiv(handle, e.clientX + 30, config);
    }, function () {
        sorter.remove();
    });

    function initIndex(items) {
        items.each(function (val, index) {
            $(this).data("tableSortIndex", index);
        });
    }
};