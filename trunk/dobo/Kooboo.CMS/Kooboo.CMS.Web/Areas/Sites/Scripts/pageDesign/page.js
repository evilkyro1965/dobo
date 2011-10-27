/*
*
* page
* author: ronglin
* create date: 2010.11.16
*
*/

(function ($) {

    // text resource
    var options = {
        errorMessage: 'network error',
        selectView: 'Please select a view',
        selectFolder: 'Please select a data folder',
        selectModule: 'Please select a module',
        selectHtmlBlock: 'Please select a HTML block'
    };

    // override text resource
    $.extend(options, __pageDesign);

    var __extend = function (subc, superc, overrides) {
        var F = function () { };
        F.prototype = superc.prototype;
        subc.prototype = new F();
        subc.prototype.constructor = subc;
        subc.superclass = superc.prototype;
        if (superc.prototype.constructor == Object.prototype.constructor) {
            superc.prototype.constructor = superc;
        }
        if (overrides) {
            for (var i in overrides) {
                subc.prototype[i] = overrides[i];
            }
        }
    };

    var IPageDesign = function (config) {
        $.extend(this, config);
    };

    IPageDesign.types = {};
    IPageDesign.register = function (key, type) {
        IPageDesign.types[key] = type;
    };

    IPageDesign.prototype = {

        isEdit: null,

        outerApi: null,

        outerValue: null,

        initialize: function () {
            var self = this;
            this.outerValue = {};
            this.isEdit = ($('#IsEdit').val() === 'True');
            this.isEdit && (this.outerValue = this.outerApi.onGetValue());

            // cancel btn close
            $('#btnCancel').click(function () {
                self.outerApi.onInnerClose();
            });

            // ok btn submit
            $('form').submit(function (ev) {
                // trigger
                self.triggerSave();
                // post data
                var postData = $(this).serializeArray();
                // valid
                var success = self.validateForm({
                    postData: postData,
                    isEmpty: function (name) {
                        var empty = true;
                        $.each(postData, function () {
                            if (this.value && this.name == name) {
                                empty = false;
                                return false;
                            }
                        });
                        return empty;
                    }
                });
                (success !== false) && (success = self.outerApi.onValid(postData));
                if (success !== false) {
                    self.doPost($(this).attr('action'), postData, function (data) {
                        if (self.outerApi.onInnerOk(data.html) !== false) {
                            // after the success handler executed, there are some global events will be fire in jQuery.ajax,
                            // and when i close the dislog page at success handler, and jQuery.ajax throw errors, 
                            // so i setTimeout to close the dialog page. but only ie9 throw this error, strangely!
                            setTimeout(function () {
                                self.outerApi.onInnerClose();
                            }, 32);
                        }
                    });
                }
                // ret
                return false;
            });
        },

        disableBtns: function (disabled) {
            $('#btnCancel, input[type=submit]').attr('disabled', disabled);
        },

        doPost: function (url, data, callback) {
            // This is working, but I don't know what is the jquery 1.5.x version changed.
            var dataType = ($.fn.jquery.indexOf('1.5') === 0) ? 'text json' : 'json';
            var self = this;
            $.ajax({
                url: url,
                data: data,
                type: 'post', dataType: dataType, timeout: 30000,
                error: function () { alert(options.errorMessage); },
                beforeSend: function (request) { self.disableBtns(true); },
                complete: function (request, state) { self.disableBtns(false); },
                success: function (d, state) { callback(d); }
            });
        },

        restoreValue: function () {
            if (this.isEdit) {
                $('#Order').val(this.outerValue.Order);
                $('#PagePositionId').val(this.outerValue.PagePositionId);
            } else {
                // set a large value to place this field at last
                // this order value will be reordered after added
                $('#Order').val('999999');
            }
        },

        triggerSave: function () { },

        validateForm: function (context) { }
    };


    /*
    * view
    */
    var ProcessView = function (config) {
        ProcessView.superclass.constructor.call(this, config);
    };
    __extend(ProcessView, IPageDesign, {
        initialize: function () {
            ProcessView.superclass.initialize.call(this);
            var self = this;
            // view tree
            var tree = $(".view-tree").treeview({
                unique: true
            });
            var checks = tree.find('input[name=ViewName]');
            checks.change(function () {
                if ($(this).attr('checked')) {
                    checks.attr('checked', false);
                    $(this).attr('checked', true);
                }
            });
            checks.change(function () {
                var parameter = $('#parameter').empty();
                var current = $('.current-view');
                // no check
                if (!$(this).attr('checked')) {
                    current.text('');
                    return;
                }
                // show view parameters
                parameter.append($(this).siblings('.parameters').html());
                // rename parameters
                var index = 0;
                $('td', parameter).each(function () {
                    var prefix = 'Parameters[' + (index++) + '].';
                    $(this).children().each(function () {
                        var name = $(this).attr('kb_name');
                        if (name) {
                            name = prefix + name;
                            $(this).attr('name', name);
                        }
                    });
                });
                // bind parameters editor
                $('input[kb_name=DataType]', parameter).each(function () {
                    var valueInput = $(this).siblings('input[kb_name=Value]');
                    var dataType = $(this).val();
                    if (dataType == 'String') {
                        // nothing
                    } else if (dataType == 'Bool') {
                        // nothing
                    } else if (dataType == 'Int') {
                        valueInput.numeric(false);
                    } else if (dataType == 'Decimal') {
                        valueInput.numeric('.');
                    } else if (dataType == 'DateTime') {
                        valueInput.datepicker();
                    }
                });
                // set current select
                var selected = $(this).val();
                current.text(selected);
                // resotre orginal parameters
                if (selected == self.outerValue.ViewName) {
                    self.restorePrameters();
                }
            });
            // cache duration
            $('#enablecache').change(function () {
                var con = $('#cache_con');
                if ($(this).attr('checked')) {
                    var input = con.show().find('input').focus();
                    if (!input.val()) { input.val('120'); } // default to 120 second
                } else {
                    con.hide();
                }
            });
            // mask input
            $('#cache_con').find('input').numeric(false);
        },

        restorePrameters: function () {
            var params = $.parseJSON(this.outerValue.Parameters);
            if (!params) { return; }
            $.each(params, function () {
                var nameInput = $('input[kb_name=Name][value=' + this.Name + ']', $('#parameter'));
                var valueInput = nameInput.siblings('input[kb_name=Value]');
                if (valueInput.length == 1) {
                    if (valueInput.attr('type') == 'checkbox') {
                        valueInput.attr('checked', (this.Value.toLowerCase() == 'true'));
                    } else {
                        var strval = this.Value ? this.Value.toString() : '';
                        if (this.DataType == 'DateTime') { strval = strval.replace(/\+/g, ' '); }
                        valueInput.val(strval);
                    }
                }
            });
        },

        duration: function (val) {
            var input = $('#cache_con').find('input');
            if (val === undefined) {
                return input.val();
            } else {
                input.val(val);
                $('#enablecache').attr('checked', val != '').trigger('change');
            }
        },

        expirationPolicy: function (val) {
            var ep = $('#ExpirationPolicy');
            if (val) {
                ep.val(val);
            } else {
                ep.attr('selectIndex', 0);
            }
        },

        restoreValue: function () {
            ProcessView.superclass.restoreValue.call(this);
            // fire select view event
            var viewName = this.outerValue.ViewName;
            if (viewName) { $('input[name=ViewName][value=' + viewName + ']').attr('checked', true).trigger('change'); }
            // OutputCache
            var dur, policy, outputCache = $.parseJSON(this.outerValue.OutputCache);
            if (outputCache) {
                dur = outputCache.Duration;
                policy = outputCache.ExpirationPolicy;
            }
            this.duration(dur || '');
            this.expirationPolicy(policy);
        },

        triggerSave: function () {
            ProcessView.superclass.triggerSave.call(this);
            var enabled = $('#enablecache').attr('checked');
            if (!enabled) { this.duration(''); }
        },

        validateForm: function (context) {
            var valid = ProcessView.superclass.validateForm.call(this, context);
            if (valid === false) { return false; }
            if (context.isEmpty('ViewName')) {
                alert(options.selectView);
                return false;
            }
        }
    });
    IPageDesign.register('view', ProcessView);


    /*
    * html
    */
    var ProcessHtml = function (config) {
        ProcessHtml.superclass.constructor.call(this, config);
    };
    __extend(ProcessHtml, IPageDesign, {
        textarea: null,
        initialize: function () {
            ProcessHtml.superclass.initialize.call(this);
            var self = this;
            // init tinymce
            this.textarea = $('#Textarea1');
            tinyMCE.init({
                theme: "advanced",
                mode: "exact",
                elements: this.textarea.attr('id'),
                media_library_url: this.textarea.attr('media_library_url'),
                media_library_title: this.textarea.attr('media_library_title'),
                editor_selector: "richeditor",
                plugins: "KoobooMediaLibrary", //"fullscreen,autoresize,searchreplace",
                theme_advanced_toolbar_location: "top",
                theme_advanced_toolbar_align: "left",
                theme_advanced_buttons1: "search,replace,undo,redo,|,KoobooMediaLibrary,image,link,unlink,charmap,|,bold,italic,fontselect,forecolor,backcolor,|,numlist,bullist,formatselect,|,code,fullscreen",
                theme_advanced_buttons2: "",
                theme_advanced_buttons3: "",
                convert_urls: false,
                extended_valid_elements: "style[type],script[type|src],iframe[src|style|width|height|scrolling|marginwidth|marginheight|frameborder]",
                verify_html: false,
                valid_children: "+body[style]", //http://tinymce.moxiecode.com/wiki.php/Configuration:valid_children
                // load page styles to simulate a real runtime environment
                oninit: function () {
                    // seeto: http://msdn.microsoft.com/en-us/library/aa770023%28VS.85%29.aspx
                    if ($.browser.msie) {
                        try {
                            var doc = tinyMCE.get(self.textarea.attr('id')).dom.doc;
                            doc.execCommand('RespectVisibilityInDesign', true, true);
                        } catch (ex) { }
                    }
                    // inject style
                    self.injectStyle();
                }
            });
        },
        injectStyle: function () {
            var $P = this.outerApi.onGetParent();
            var sheets = $P('link[rel=stylesheet]');
            if (sheets.length > 0) {
                var urls = [];
                sheets.each(function () { urls.push(this.href); });
                tinyMCE.get(this.textarea.attr('id')).dom.loadCSS(urls.join(','));
            }
            var styles = $P('style');
            if (styles.length > 0) {
                var cssText = [];
                styles.each(function () { cssText.push(this.textContent || this.innerHTML); });
                cssText = cssText.join('\n');
                var doc = tinyMCE.get(this.textarea.attr('id')).dom.doc;
                var head = doc.getElementsByTagName('head')[0];
                var node = doc.createElement('style');
                node.setAttribute('type', 'text/css');
                if ($.browser.msie) {
                    head.appendChild(node);
                    node.styleSheet.cssText = cssText;
                } else {
                    try {
                        node.appendChild(doc.createTextNode(cssText));
                    } catch (ex) {
                        node.cssText = cssText;
                    }
                    head.appendChild(node);
                }
            }
        },
        triggerSave: function () {
            ProcessHtml.superclass.triggerSave.call(this);
            window.tinyMCE && (window.tinyMCE.triggerSave());
        },
        restoreValue: function () {
            ProcessHtml.superclass.restoreValue.call(this);
            this.textarea.val(this.outerValue.Html);
        }
    });
    IPageDesign.register('html', ProcessHtml);


    /*
    * folder
    */
    var ProcessFolder = function (config) {
        ProcessFolder.superclass.constructor.call(this, config);
    };
    __extend(ProcessFolder, IPageDesign, {
        initialize: function () {
            ProcessFolder.superclass.initialize.call(this);
            // Type
            var self = this;
            $('input[name=Type]').change(function () {
                var t = self.resultType();
                var tp = $('#top_con'), uk = $('#userkey_con');
                if (t.val() == 'List') {
                    tp.show().find('input').focus();
                    uk.hide();
                } else if (top) {
                    uk.show().find('input').focus();
                    tp.hide();
                }
            });
            // UserKey
            var userKey = $('#UserKey');
            var getSource = function () {
                var folder = $('.folder-checkbox:checked').val();
                if (!folder) { return false; }
                var base = userKey.attr('source');
                return userKey.attr('source') + '&folderName=' + encodeURIComponent(folder) + '&term=' + encodeURIComponent(userKey.val());
            };
            userKey.autocomplete({
                search: function (event, ui) {
                    var s = getSource();
                    if (!s) { return false; }
                    $(this).autocomplete('option', 'source', s);
                }
            });
            // Top
            $('#Top').numeric(false);
        },

        resultType: function (type) {
            var inputs = $('input[name=Type]');
            if (type === undefined) {
                var ret;
                inputs.each(function () {
                    if ($(this).attr('checked')) {
                        ret = $(this);
                        return false;
                    }
                });
                return ret;
            } else {
                return inputs.each(function () {
                    if ($(this).val() == type) {
                        $(this).attr('checked', true);
                        $(this).trigger('change');
                        return false;
                    }
                });
            }
        },

        triggerSave: function () {
            ProcessFolder.superclass.triggerSave.call(this);
            if (this.resultType().val() == 'List') {
                $('#UserKey').val('');
            } else {
                $('#Top').val('');
            }
        },

        restoreValue: function () {
            ProcessFolder.superclass.restoreValue.call(this);
            // type, default to select list
            var type = this.outerValue.ContentPositionType;
            this.resultType(type || 'List');
            // data rule
            var dataRule = $.parseJSON(this.outerValue.DataRule);
            if (!dataRule) { return; }
            if (dataRule.Top) {
                $('#Top').val(dataRule.Top);
            }
            if (dataRule.WhereClauses && dataRule.WhereClauses.length > 0) {
                $('#UserKey').val(dataRule.WhereClauses[0].Value1);
            }
            $('.folder-checkbox[value=' + dataRule.FolderName + ']').attr('checked', true);
        },

        validateForm: function (context) {
            var valid = ProcessFolder.superclass.validateForm.call(this, context);
            if (valid === false) { return false; }
            if (context.isEmpty('DataRule.FolderName')) {
                alert(options.selectFolder);
                return false;
            }
        }
    });
    IPageDesign.register('folder', ProcessFolder);


    /*
    * module
    */
    var ProcessModule = function (config) {
        ProcessModule.superclass.constructor.call(this, config);
    };
    __extend(ProcessModule, IPageDesign, {
        EntryActionInput: null,
        EntryControllerInput: null,
        EntryOptionsSelect: null,
        initialize: function () {
            ProcessModule.superclass.initialize.call(this);
            this.EntryActionInput = $('input[id=EntryAction]');
            this.EntryControllerInput = $('input[id=EntryController]');
            this.EntryOptionsSelect = $('select[id=EntryOptions]');
            var checks = $('input[name=ModuleName]'), self = this;
            checks.change(function () {
                if ($(this).attr('checked')) {
                    checks.attr('checked', false);
                    $(this).attr('checked', true);
                }
            });
            checks.change(function () {
                // no check
                if (!$(this).attr('checked')) {
                    return;
                }
                // set module default settings
                var module = $(this).val();
                self.EntryActionInput.val($('input[id=' + module + 'EntryAction]').val());
                self.EntryControllerInput.val($('input[id=' + module + 'EntryController]').val());
                var optionsHtml = [], options = $.parseJSON($('input[id=' + module + 'EntryOptions]').val());
                if (options && options.length) {
                    optionsHtml.push('<option></option>');
                    $.each(options, function (index, op) {
                        optionsHtml.push('<option action="' + op.EntryAction + '" controller="' + op.EntryController + '">' + op.Name + '</option>');
                    });
                }
                self.EntryOptionsSelect.html(optionsHtml.join(''));
            });
            this.EntryOptionsSelect.change(function () {
                var op = $(this).children().eq(this.selectedIndex);
                var action = op.attr('action'), controller = op.attr('controller');
                self.EntryActionInput.val(action); self.EntryControllerInput.val(controller);
            });
        },
        restoreValue: function () {
            ProcessModule.superclass.restoreValue.call(this);
            // 
            var moduleName = this.outerValue.ModuleName;
            if (moduleName) { $('input[name=ModuleName][value=' + moduleName + ']').attr('checked', true).trigger('change'); }
            //
            var exclusive = this.outerValue.Exclusive;
            if (exclusive == 'true') { $('input[name=Exclusive]').attr('checked', true); }
            //
            var action = this.outerValue.EntryAction;
            if (action) { this.EntryActionInput.val(action); }
            //
            var controller = this.outerValue.EntryController;
            if (controller) { this.EntryControllerInput.val(controller); }
        },

        validateForm: function (context) {
            var valid = ProcessModule.superclass.validateForm.call(this, context);
            if (valid === false) { return false; }
            if (context.isEmpty('ModuleName')) {
                alert(options.selectModule);
                return false;
            }
        }
    });
    IPageDesign.register('module', ProcessModule);

    /*
    * htmlBlock
    */
    var ProcessHtmlBlock = function (config) {
        ProcessHtmlBlock.superclass.constructor.call(this, config);
    };
    __extend(ProcessHtmlBlock, IPageDesign, {
        initialize: function () {
            ProcessHtmlBlock.superclass.initialize.call(this);
            var checks = $('input[name=BlockName]');
            checks.change(function () {
                if ($(this).attr('checked')) {
                    checks.attr('checked', false);
                    $(this).attr('checked', true);
                }
            });
        },
        restoreValue: function () {
            ProcessHtmlBlock.superclass.restoreValue.call(this);
            // 
            var blockName = this.outerValue.BlockName;
            if (blockName) { $('input[name=BlockName][value=' + blockName + ']').attr('checked', true).trigger('change'); }
        },
        validateForm: function (context) {
            var valid = ProcessHtmlBlock.superclass.validateForm.call(this, context);
            if (valid === false) { return false; }
            if (context.isEmpty('BlockName')) {
                alert(options.selectHtmlBlock);
                return false;
            }
        }
    });
    IPageDesign.register('htmlBlock', ProcessHtmlBlock);


    /*
    * page load
    */
    if (!window.getDesignerType) {
        // empty implement, this functon must be override in specific page.
        window.getDesignerType = function (types) { };
    }
    $(function () {

        // get content iframe
        var iframe = function () {
            window.__rid = Math.random().toString();
            var frm, frms = window.parent.document.getElementsByTagName('iframe');
            for (var i = 0; i < frms.length; i++) {
                if (frms[i].contentWindow.__rid == window.__rid) {
                    frm = frms[i];
                    break;
                }
            }
            return frm;
        } ();
        if (!iframe) { return; }

        // get outer api
        var outerApi = iframe.outerApi;
        if (!outerApi) { return; }

        // init
        var type = getDesignerType(IPageDesign.types);
        if (type) {
            var designer = new type({ outerApi: outerApi });
            designer.initialize();
            designer.restoreValue();
        }
    });

    var IRichEditor = function (config) {
        $.extend(this, config);
        this.initialize();
    };
    IRichEditor.prototype = {
        host: null,
        initialize: function () { },
        triggerSave: function () { },
        getValue: function () { },
        setValue: function (val) { },
        destroy: function () { }
    };

})(jQuery);