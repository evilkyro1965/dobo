/// <reference path="jquery-1.4.4-vsdoc.js" />
/// <reference path="jquery-ui.js" />
/// <reference path="extension/js.extension.js" />


/// <reference path="kooboo.js" />

/*
*********comment style*****************


///<summary>
///
///
///</summary>
///	<param name="selector" type="String">
///
///	</param>
///	<param name="context" type="jQuery">
///		
///	</param>
///	<returns type="jQuery" />


*/


kooboo.namespace("kooboo.cms.ui");



///kooboo.cms.ui.dynamicList
(function () {

    kooboo.cms.ui.extend({
        dynamicList: (function (option) {
            var api = {};
            var _instanceMap = {};
            var _configMap = {};

            api.instance = function (option) {
                //Singleton Pattern return the same instance
                if (typeof (option) == 'string') {
                    return _instanceMap[option];
                }
                if (!option.containerId) {
                    throw 'container Id is null';
                }

                if (_instanceMap[option.containerId]) {
                    return _instanceMap[option.containerId];
                }

                var instance = new _dynamicList(option);

                _instanceMap[option.containerId] = instance;

                return instance;
            }

            function _dynamicList(option) {
                if (typeof (option) == 'string') {
                    option = _configMap[option];
                }
                if (option && !option.containerId) {
                    throw "containerId is null!";
                }
                if (option && !option.propertyName) {
                    throw "propertyName is null!";
                }
                var config = {
                    containerId: '', //not null
                    propertyName: '',
                    templateClass: 'template', //could be null
                    newItemClass: "datafield",
                    templateId: '',
                    data: [], //could be null,
                    onInit: function () { }
                };
                kooboo.extend(config, option);
                var container = $('#' + config.containerId);
                var template = $('#' + config.templateId);
                var tagName = template.attr('tagName');
                template.hide();
                template.find('input,select').attr('disabled', true);

                function init() {

                    if ($.isArray(config.data)) {
                        $(config.data).each(function (index, value) {
                            var clone = template.clone();
                            clone.addClass(config.newItemClass);
                            clone.find('input:not([indexField]),select:not(indexField)').attr('disabled', false);
                            clone.show();
                            clone.find('input:not([indexField]),select:not(indexField)').each(function () {
                                var input = $(this);
                                var name = input.attr('name');

                                var fieldType = input.attr('fieldtype');
                                if (!fieldType) {
                                    if (name.indexOf('.') > 0) {//property is object
                                        var val = kooboo.object.getVal(value, name);
                                        if (kooboo.date.reg.utc.test(val)) {
                                            val = kooboo.date.parser.utc(val).format('dd/mm/yyyy');
                                        }
                                        input.val(val);
                                    } else {//property is value
                                        if (value[name]) {
                                            var val = kooboo.object(value).getVal(name);
                                            if (kooboo.date.reg.utc.test(val)) {
                                                val = kooboo.date.parser.utc(val).format('dd/mm/yyyy');
                                            }
                                            input.val(val);
                                        } else if (!$.isPlainObject(value)) {
                                            input.val(value);
                                        }
                                    }

                                    ///handle for checkbox \ radio
                                } else {
                                    _fieldtype[fieldType](input, value, index);
                                }

                            });
                            container.append(clone);
                        }); //end each loop
                    }

                }

                init(); //do init 
                this.getItems = function () {
                    return container.children('.datafield');
                }
                this.add = function () {
                    ///<summary>
                    ///return new item
                    ///</summary>
                    var random = Math.random().toString().replace('.', '-');
                    var clone = template.clone().attr('id', 'machine-list-' + random);


                    clone.addClass(config.newItemClass);
                    clone.show('bounce', {}, 500);
                    clone.find('input,select').attr('disabled', false).removeAttr('id');
                    container.append(clone);
                    setTimeout(function () {
                        clone.find('input,select').first().focus();
                    }, 200);

                    return clone;
                }
                this.remove = function (obj) {
                    ///<summary>
                    ///obj is the item you will delete
                    ///</summary>
                    obj.remove();
                    this.sort();
                }
                this.sort = function () {
                    ///<summary>
                    /// Sort index for names
                    ///</summary>
                    container.find(tagName + '.datafield').each(function (index) {
                        var current = $(this);
                        current.find('input,select').each(function (i) {

                            var name = $(template.find('input,select')[i]).attr('name');

                            name = (name && name.trim().length > 0) ? (config.propertyName + '[' + index + '].' + name) : (config.propertyName + '[' + index + ']');

                            $(this).attr('name', name);
                        });
                    });

                }
                this.reset = function () {
                    container.find(tagName + '.datafield').remove();
                    init();
                }
            }

            return api;
        })() //end dynamicList
    });                         //end kooboo.cms.ui.extend()


    var _fieldtype = {
        checkbox: function (input, value, index) {
            //debugger;
            var name = input.attr('name');
            var val = kooboo.object(value).getVal(name);
            ///checkbox for display
            if (input.attr('type') == 'checkbox') {

                if (val && val.toString().toLower() == 'true') {
                    input.attr('checked', true);
                } else {
                    input.attr('checked', false);
                }
                ///hidden for checkbox
            } else {
                ///do nothing
            }
        },
        radio: function (input, value, index) {
            var name = input.attr('name');
            var val = kooboo.object(value).getVal(name);
            if (val && val.toLower() == input.val().toLower()) {
                input.attr('checked', true);
            }
        }
    };
    kooboo.cms.ui.dynamicList.fieldType = _fieldtype;

    kooboo.cms.ui.extend({
        dynamicListInstance: function (config) {
            ///<summary>
            /// you can config option like this
            ///option = {
            ///    containerId:'',
            ///    templateId:'',
            ///    delClass:'del',/*default(del)*/
            ///    onInit:function(){},/*default(null)*/
            ///    beforeAdd:function(){},/*default(null)*/
            ///    onAdd:function(){},/*default(null)*/
            ///    beforeRemove:function(){},/*default(null)*/
            ///    onRemove:function(){},/*default(null)*/
            ///    addButtonId:'',
            ///    propertyName:''
            ///}
            ///</summary>

            var option = {
                containerId: '', //not null
                templateId: '', //not null
                addButtonId: '', //not null
                delClass: 'remove',
                onInit: function () { },
                beforeAdd: function () { },
                onAdd: function () { },
                beforeRemove: function () { },
                onRemove: function () { },
                parentSelector: null,
                hideRemoveBtn: false
            };

            kooboo.extend(option, config);

            var instance = kooboo.cms.ui.dynamicList.instance(option);

            initItems();

            function initItems() {

                var items = instance.getItems();
                ///init remove method
                var removeBtn = items.find('.' + option.delClass).click(function () {
                    if (typeof (option.beforeRemove) == 'function') {
                        var result = option.beforeRemove();
                        if (result == false) {
                            return false;
                        }
                    }
                    var parent = $(this).parent();
                    if (option.parentSelector != null) {
                        parent = $(this).parents(option.parentSelector);
                    }
                    instance.remove(parent);
                    instance.sort();
                    option.onRemove();
                    return false;
                });
                if (option.hideRemoveBtn) {
                    items.hover(
					function () {
					    $(this).find('.' + option.delClass).show();
					},
					function () {
					    $(this).find('.' + option.delClass).hide();
					});
                    removeBtn.hide();
                }

                instance.sort();

                if (typeof (option.onInit) == 'function') {
                    option.onInit(instance); //events after inits
                }
            }

            ///init add method

            var addBtn = $('#' + option.addButtonId);



            if (!addBtn.data('dynamiclist-bind')) {
                addBtn.data('dynamiclist-bind', true);
                addBtn.click(function () {
                    if (typeof (option.beforeAdd) == 'function') {
                        var result = option.beforeAdd();
                        if (result == false) {
                            return false;
                        }
                    }
                    //debugger;
                    var item = instance.add();
                    initItems();
                    if (typeof (option.onAdd) == 'function') {
                        option.onAdd();
                    }
                    return false;
                });
            }


        }
    });

})();

/// kooboo.cms.content.selectList
kooboo.cms.ui.extend((function () {
    var configMap = {};
    var apiMap = {};

    function selectList(option) {
        if (!option) {
            return;
        }
        var config = {
            id: "",
            templateId: '',
            sourceId: '',
            selectedId: '',
            sourceData: [],
            selecteData: []
        };

        if (typeof (option) == 'string') {
            option = configMap[option];
        }
        if (apiMap[config.id]) {
            return apiMap[config.id];
        }
        if (!option.id) {
            alert("option.id is null");
            throw "option.id is null";
        }
        if (!option.templateId) {
            alert("option.templateId is null");
            throw "option.templateId is null";
        }
        if (!option.sourceId) {
            alert("option.sourceId is null");
            throw "option.sourceId is null";
        }
        if (!option.selectedId) {
            alert("option.selectedId is null");
            throw "option.selectedId is null";
        }

        kooboo.extend(config, option);
        configMap[config.id] = config;

        var template = $('#' + config.templateId);
        template.hide();
        var sourceContainer = $('#' + config.sourceId);
        var selectContainer = $('#' + config.selectedId);
        var selectedClass = "selected";


        function _initList(container, data, dbclick) {
            container.html('');
            for (var i = 0; i < data.length; i++) {
                var clone = template.clone();
                clone.show();
                container.append(clone);
                clone.find('td:not(:hidden)').each(function () {
                    var name = $(this).html().trim();
                    var val = kooboo.object.getVal(data[i], name);
                    $(this).html(val);
                });
                clone.find('input').each(function () {
                    var name = $(this).attr('name').trim();
                    var val = kooboo.object.getVal(data[i], name);
                    $(this).val(val);
                });
                if (typeof (dbclick) == 'function') {
                    dbclick(clone);
                }
            }
        }

        function initSelected() {
            _initList(selectContainer, config.selecteData, function (obj) {
                doadd(obj);
            });
        };

        function initSource() {
            _initList(sourceContainer, config.sourceData, function (obj) {
                doremove(obj);
            });
        };

        function doadd(obj) {
            var clone = obj.clone();
            clone.css('cursor', 'pointer');
            clone.removeClass(selectedClass);
            clone.toggle(function () {
                $(this).addClass(selectedClass);
            }, function () {
                $(this).removeClass(selectedClass);
            });
            clone.dblclick(function () {
                doremove(clone);
            });
            obj.remove();
            $(selectContainer).append(clone);
        }
        function doremove(obj) {
            var clone = obj.clone();
            clone.css('cursor', 'pointer');
            clone.removeClass(selectedClass);
            clone.toggle(function () {
                $(this).addClass(selectedClass);
            }, function () {
                $(this).removeClass(selectedClass);
            });
            clone.dblclick(function () {
                doadd(clone);
            });
            obj.remove();
            $(sourceContainer).append(clone);
        }

        var api = {};

        function getSelecte() {
            var query = sourceContainer.children('.' + selectedClass);
            return query;
        }
        function getRemove() {
            var query = selectContainer.children('.' + selectedClass);
            return query;
        }
        function remove() {
            var toRemove = getRemove();
            toRemove.each(function () {
                doremove($(this));
            });
        }
        function add() {
            var toAdd = getSelecte();
            toAdd.each(function () {
                doadd($(this));
            });
        }
        function selectAll(container) {
            var all = container.children();
            all.addClass(selectedClass);
            return all;
        }

        function init() {
            initSelected();
            initSource();
        }

        init();
        api.getSelecte = getSelecte;
        api.getRemove = getRemove;
        api.remove = remove;
        api.add = add;
        api.reset = init;
        api.selectAllSource = function () {
            return selectAll(sourceContainer);
        }
        api.selectAllSelect = function () {
            return selectAll(selectContainer);
        }
        api.addAll = function () {
            this.selectAllSource();
            this.add();
        }
        api.removeAll = function () {
            this.selectAllSelect();
            this.remove();
        }
        apiMap[config.id] = api;
        return api;
    }

    var ui = {};
    ui.selectList = selectList;
    return ui;
})()); //end select list
//end kooboo.cms.ui.extend


///kooboo.cms.ui.iframeCheckboxList
kooboo.cms.ui.extend({
    iframeCheckboxList: (function () {
        var checkedList = [];
        var apiMap = {};

        function iframeCheckboxList(option) {
            var config = {
                iframeId: '',
                checkboxListClass: "checkboxList",
                checkedList: [],
                initAfterReady: true,
                cache: true,
                onItemChecked: function () { },
                onItemInit: function () { },
                beforeAdd: function () { },
                onAdd: function () { },
                beforeRemove: function () { },
                onReomve: function () { }
            };

            kooboo.extend(config, option);

            if (apiMap[config.iframeId] && config.cache) {
                return apiMap[config.iframeId];
            }
            var iframe = $("#" + config.iframeId);

            function readyEvent() {
                iframe.contents()
                    .find("input:checkbox." + config.checkboxListClass)
                    .each(function () {
                        var current = $(this);
                        current.unbind("click").click(function (event) {
                            event.stopPropagation();
                            if (!current.attr('checked')) {//cancel checked
                                config.checkedList = config.checkedList.removeElement(current.val());
                            } else {//checked
                                config.checkedList.push(current.val());
                            }
                            config.onItemChecked(current.attr('checked'), current);
                        });
                        var checked = config.checkedList.contain(current.val());
                        current.attr('checked', checked);
                        config.onItemInit(checked, current);
                    });
            }

            $(config.checkedList).each(function (index) {
                config.checkedList[index] = config.checkedList[index].toString();
            });


            readyEvent();
            var api = {};
            api.getCheckedList = function () {
                return config.checkedList;
            }
            api.reset = (function () {
                var checkedList = config.checkedList;
                return function () {
                    config.checkedList = checkedList;
                    readyEvent();
                }
            })();

            api.add = function (val) {
                config.checkedList.push(val);
                readyEvent();
            }
            api.remove = function (val) {
                config.checkedList = config.checkedList.removeElement(val);
                readyEvent();
            }
            api.getConfig = function () {
                return config;
            }
            if (config.cache) {
                apiMap[config.iframeId] = api;
            }
            return api;

        } //end function checkboxList()
        return iframeCheckboxList;
    })()//end kooboo.cms.ui.iframeCheckboxList
});            //end kooboo.cms.ui.extend


kooboo.cms.ui.extend({
    flexList: function (option) {
        var config = {
            containerSelector: ".flex-list",
            btnSelector: ".arrow",
            activeClass: "active",
            itemSelector: ".datafield",
            autoExpand: true
        };

        $.extend(config, option);

        var flexList = $(config.containerSelector);
        flexList.each(function () {
            var current = $(this);

            var btn = current.find(config.btnSelector);

            if (config.autoExpand) {
                show(btn, current);
                btn.toggle(function () {
                    hide(btn, current);
                }, function () {
                    show(btn, current);
                });
            } else {
                hide(btn, current);
                btn.toggle(function () {
                    show(btn, current);
                }, function () {
                    hide(btn, current);
                });
            } //end if config.autoExpand

            function hide(btn, current) {
                btn.removeClass("active");
                current.find(config.itemSelector).hide();
            }
            function show(btn, current) {
                btn.addClass("active");
                current.find(config.itemSelector).show();
            }

        });
    }
});

//kooboo.cms.ui.formHelper
(function () {
    kooboo.namespace("kooboo.cms.ui.formHelper");
    kooboo.cms.ui.formHelper.extend({

        createHidden: function (data, name, $form) {
            var current = this;
            form = $($form);

            if ($.isPlainObject(data)) {
                for (var p in data) {
                    current.createHidden(data[p], name + '.' + p, form);
                }
            } else if ($.isArray(data)) {
                data.each(function (val, index) {
                    current.createHidden(val, name + '[' + index + ']', form);
                });
            } else {
                form.addHidden(name, data);
            }
        },

        setForm: function (data, $form, pre) {
            form = $($form);
            var current = this;
            for (var prop in data) {
                var fullName = pre ? (pre + "." + prop) : prop;
                if ($.isPlainObject(data[prop])) {
                    current.setForm(data[prop], form, fullName);
                } else if (!$.isArray(data[prop])) {
                    form.setFormField(fullName, data[prop]);
                }
            }
        },

        tempForm: function (dataObj, url, pre, formAttr, $form) {
            ///<summary>
            /// make data into hidden field
            /// data can be object or array
            /// 
            ///</summary>
            /// 
            ///	<param name="dataObj" type="object">
            ///	the data you want to put into form
            ///	</param>
            /// 
            ///	<param name="url" type="String">
            ///	the form's url  (can be null)
            ///	</param>
            /// 
            ///	<param name="pre" type="String">
            ///	the data's prefix (can be null)
            ///	</param>
            ///	<param name="formAttr" type="object">
            ///	the form's html attributes
            ///	</param>
            ///	<param name="$form" type="jQuery">
            ///	can be null or jquery object
            ///	</param>
            ///	<returns type="formHelpAPI" />
            var formAttrConfig = {
                action: url,
                method: 'post'
            };
            var current = this;
            $.extend(formAttrConfig, formAttr);

            var form = $form || $("<form></form>").attr(formAttrConfig);

            form.appendTo($("body"));

            function init(data) {
                current.createHidden(data, pre, form);
            }

            init(dataObj);

            var api = {
                clear: function () {
                    form.html('');
                },
                reset: function (data) {
                    form.html('');
                    data = data || dataObj;
                    init(data);
                },
                submit: function () {
                    form.submit();
                },
                addData: function (data, dataPre) {
                    dataPre = dataPre ? dataPre : pre;
                    current.createHidden(data, dataPre, this.form);
                    return this;
                },
                ajaxSubmit: function (option) {
                    option = option ? option : {};
                    option.url = option.url ? option.url : formAttrConfig.action;
                    option.type = option.type ? option.type : formAttrConfig.method;
                    var ajaxConfig = {
                        data: form.serialize()
                    };
                    $.extend(option, ajaxConfig);
                    $.ajax(option);
                },
                form: form
            };

            return api;
        },

        copyForm: function ($source, $dest, nameProvider) {
            ///<summary>
            /// copy input or select or hidden value to destination form
            /// UNDONE
            ///</summary>
            ///	<param name="$source" type="String,jQuery">
            /// 
            ///	</param>
            ///	<param name="$form" type="jQuery">
            ///		
            ///	</param>
            ///	<returns type="jQuery" />
            $source = $($source);
            $dest = $($dest);

            $source.find('input,select,textarea').each(function () {
                var input = $(this);

                var inputType = input.attr('type');

                var hidden = $('<input type="hidden" />');

                if (formHelper.inputTypeHandle[inputType]) {

                } else {

                }


                var name = input.attr('name');
                if ($.isFunction(nameProvider)) {
                    name = nameProvider(input);
                }
                hidden.attr('name', name);
            });

            return $dest;
        }

    });
    var _hiddenStr = '<input type="hidden" />';
    function generateHidde() {
        return $(_hiddenStr);
    }
    var formHelper = {
        inputTypeHandle: {
            'radio': function (input, $source, $dest) {
                var hidden = generateHidde();

                if (input.attr('checked')) {
                    hidden.val(input.val());
                    return hidden;
                } else {
                    return false;
                }
            },
            'checkbox': function (input, $source, $dest) {
                var hidden = generateHidde();
                if (hidden.attr('checked')) {
                    hidden.val(true);
                }
            },
            'select-one': function (input, $source, $dest) {
                var hidden = generateHidde();

            },
            'select-many': function (input, $source, $dest) {
                var hidden = generateHidde();
            },
            'hidden': function (input, $source, $dest) {
                var hidden = generateHidde();
            },
            defaultInput: function (input, $source, $dest) {
                var hidden = generateHidde();

            }
        }
    };
})();


kooboo.cms.ui.extend({
    timemout: (function () {
        var s = 5, ui;
        setInterval(function () {
            s--;
            if (s <= 0) {
                if (ui) {
                    ui.messageBox().hide();
                }
            }
        }, 1000);
        return function (UI, t) {
            ui = UI;
            s = t ? t / 1000 : 5;
        }
    })(),
    messageBox: function () {
        var ui = this;
        var messageBox = $("#message-box");
        var api = {
            showResponse: function (response) {
                var tipClass = "error";
                if (response.Success)
                    tipClass = "info";
                var messages = response.Messages;
                if (messages && messages.length > 0) {
                    var str = messages.join("<br />");                   
                    this.show(str, tipClass);
                }
            },
            show: function (tip, tipClass, timeout) {
                tip = tip || "";
                timeout = timeout || 5000;
                tipClass = tipClass || "info";

                this.removeWaiting();

                messageBox.removeClass("hide").removeClass("info warning error").addClass(tipClass);

                messageBox.find("p.msg").html(tip).removeClass("hide");

                ui.timemout(ui, timeout);
            },
            hide: function () {
                messageBox.addClass("hide").removeClass("info warning error");
                messageBox.find("p.waiting").add("hide");

                this.removeWaiting();
            },
            removeWaiting: function () {
                ui.loading().hide();
            },
            waiting: function () {
                ui.loading().show();
            }
        };
        return api;
    },
    loading: function () {
        var timmer = 0;
        var loading = $('div.kooboo-cms-waiting');
        var submit = $(':submit,.submit,.button');
        return {
            show: function () {
                timmer = 0;
                var host = this;
                loading.show();
                submit.attr('diabled', true);
                setInterval(function () {
                    timmer++;
                    if (timmer > 20) {
                        //host.hide();
                        timmer = 0;
                    }
                }, 1000);
            },
            hide: function () {
                timmer = 0;
                loading.hide();
                submit.attr('diabled', false);
            }
        }
    }
});

kooboo.cms.ui.extend({
    status: (function () {
        var status = {};

        return function (name) {
            name = name || "kooboo-page-status";
            if (status[name]) {
                return status[name];
            } else {
                var editStatus = (function () {
                    var canleave = true;
                    var api = {
                        canLeave: function () {
                            return canleave;
                        },
                        stop: function () {
                            canleave = false;
                        },
                        pass: function () {
                            canleave = true;
                        }
                    };

                    status[name] = api;
                })();

                return status[name];
            }

        }

    })()
});

kooboo.cms.ui.extend({
    event: {
        eventList: {},
        add: function (func, name, data) {
            name = name || "default";
            this.eventList[name] = this.eventList[name] || [];
            this.eventList[name].push({
                func: func,
                data: data
            });
        },
        execute: function (name) {

            name = name || 'default';

            this.eventList[name] = this.eventList[name] || [];

            var result = true;
            this.eventList[name].each(function (fnObj) {
                result = result && (!(fnObj.func(fnObj.data) == false));
            });
            return result;
        },
        ajaxSubmit: function (func) {
            if (func) {
                this.add(func, 'ajax-submit');
            } else {
                return this.execute('ajax-submit');
            }
        }
    }
});