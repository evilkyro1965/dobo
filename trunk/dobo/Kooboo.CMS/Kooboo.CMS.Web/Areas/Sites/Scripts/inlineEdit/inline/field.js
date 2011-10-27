/*
*
* field
* author: ronglin
* create date: 2011.01.28
*
*/

(function ($) {

    // text resource
    var options = {
        errorMessage: 'Network error, the action has been cancelled',
        saving: 'saving...',
        saveSuccess: 'save successfully.',
        saveFailure: 'The attempt to save has failed'
    };

    // override text resource
    $.extend(options, __inlineEditVars.field_js);

    /*
    * field class
    */
    var fieldClass = function (config) {
        $.extend(this, config);
        this.initialize();
    };

    fieldClass.prototype = {

        el: null, wrapper: null,

        params: null, menu: null, editor: null,

        updateAction: null,

        initialize: function () {
            this.params = this.getParams();
            this.ensureEditSpace();
            this.createMenu();
        },

        createMenu: function () {
            var self = this;
            this.menu = new yardi.fieldMenuAnchorBar({
                title: this.params.fieldName,
                alignTo: this.el,
                renderTo: yardi.cacheCon,
                onEdit: function () { self.edit(); }
            });
        },

        getParams: function () {
            return {
                uuid: this.el.attr('uuid'),
                schema: this.el.attr('schema'),
                fieldName: this.el.attr('fieldName')
            };
        },

        ensureEditSpace: function () {
            this.removeEditSpace();
            var empty = ((this.el.text() || '').trim() === '');
            if (empty && (this.el.height() < 20 || this.el.width() < 40)) {
                this.el.addClass('kb-field-empty');
            }
        },

        removeEditSpace: function () {
            this.el.removeClass('kb-field-empty');
        },

        message: function (msg) {
            if (!this.editor) { return; }
            this.editor.message(msg);
        },

        doPost: function (url, data, callback) {
            var self = this;
            $.ajax({
                url: url, data: data,
                type: 'post', dataType: 'json', timeout: 30000,
                error: function () { self.message(options.errorMessage); self.editor.disabled(false); },
                beforeSend: function (request) { self.editor.disabled(true); },
                complete: function (request, state) { },
                success: function (d, state) { self.message(); callback(d); }
            });
        },

        wrap: function () {
            var dom = this.el.get(0), w;
            if ($.browser.mozilla) {
                // ensure firefox use DIV as editor element, 
                // in firefox there ara some problems in other element types.
                // eg: SPAN within A will cause the content can not be removed.
                if (dom.tagName == 'DIV') { return this.el; }
                w = $('<div></div>').appendTo(this.el);
            } else {
                if (dom.tagName == 'DIV' || dom.tagName == 'SPAN') { return this.el; }
                w = $('<span></span>').appendTo(this.el);
            }
            while (dom.childNodes.length > 1) { w.append(dom.childNodes[0]); }
            var float = this.el.css('float');
            if (float && float != 'none') {
                // fix widget float bug.
                // when widget set to float left or right, it break away the layout
                w.css({
                    float: float,
                    minWidth: el.css('width')
                });
            }
            return w;
        },

        unwrap: function () {
            if (!this.wrapper) { return; }
            if (this.wrapper == this.el) { return; }
            var node = this.wrapper.get(0);
            while (node.childNodes.length > 0)
                node.parentNode.insertBefore(node.childNodes[0], node);
            this.wrapper.remove();
        },

        edit: function () {
            if (this.editor) { return; }
            var self = this;
            this.removeEditSpace();
            this.wrapper = this.wrap();
            this.editor = new yardi.inlineEditor({
                el: this.wrapper,
                onSave: function () { self.save(); },
                onCancel: function () { self.cancel(); }
            });
            this.editor.edit();
        },

        cancel: function (revert) {
            if (!this.editor) { return; }
            this.editor.cancel(revert);
            this.unwrap();
            this.wrapper = null;
            this.ensureEditSpace();
            var self = this;
            setTimeout(function () { self.editor = null; }, 100);
        },

        save: function () {
            if (!this.editor) { return; }
            //this.message(options.saving);
            var html = this.editor.value(), self = this;
            var postData = $.extend({}, this.params, { value: html });
            this.doPost(this.updateAction, postData, function (data) {
                if (!data.Success) {
                    self.editor.disabled(false);
                    self.message(options.saveFailure);
                    return;
                }
                // successful
                self.message(); //self.message(options.saveSuccess);
                //setTimeout(function () {
                self.editor.disabled(false);
                self.cancel(false);
                //}, 800);
            });
        }
    };

    // register
    yardi.fieldClass = fieldClass;

})(jQuery);
