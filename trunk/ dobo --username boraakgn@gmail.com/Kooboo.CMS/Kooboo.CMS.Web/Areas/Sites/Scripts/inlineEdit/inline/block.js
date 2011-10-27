/*
*
* block
* author: ronglin
* create date: 2011.01.28
*
*/

(function ($) {

    // text resource
    var options = {
        confirmDel: 'Are you sure you want to delete this item?',
        errorMessage: 'Network error, the action has been cancelled',

        copying: 'copying...',
        deleting: 'deleting...',
        publishing: 'publishing...',

        copySuccess: 'copy successfully.',
        deleteSuccess: 'delete successfully.',
        publishSuccess: 'publish successfully.',

        copyFailure: 'The attempt to copy has failed',
        deleteFailure: 'The attempt to delete has failed',
        publishFailure: 'The attempt to publish has failed'
    };

    // override text resource
    $.extend(options, __inlineEditVars.block_js);

    /*
    * block class
    */
    var blockClass = function (config) {
        $.extend(this, config);
        this.initialize();
    };

    blockClass.prototype = {

        el: null,

        params: null, menu: null,

        copyAction: null, deleteAction: null, updateAction: null,

        getParams: function () {
            return {
                uuid: this.el.attr('uuid'),
                schema: this.el.attr('schema'),
                published: this.el.attr('published'),
                editUrl: this.el.attr('editUrl'),
                summary: this.el.attr('summary')
            };
        },

        initialize: function () {
            this.params = this.getParams();
            // bind menu bar
            var self = this;
            this.menu = new yardi.blockMenuAnchorBar({
                title: this.params.schema,
                alignTo: this.el,
                renderTo: yardi.cacheCon,
                onEdit: function () { self.doEdit(); },
                onCopy: function () { self.doCopy(); },
                onDelete: function () { self.doDelete(); },
                onPublish: function () { self.doPublish(); }
            });
            this.setPublish(this.params.published !== '');
        },

        setPublish: function (published) {
            if (published) {
                this.el.removeClass('kb-block-unpublish');
            } else {
                this.el.addClass('kb-block-unpublish');
            }
            this.menu.setPublished(published);
        },

        remove: function () {
            this.menu.remove();
        },

        disable: function (disabled, all) {
            this.menu.disableBtns(disabled);
            yardi.anchorBar.hideAll((all === true) ? undefined : this.menu);
            yardi.anchorBar.fixAll(disabled);
        },

        doPost: function (url, data, callback) {
            var self = this;
            $.ajax({
                url: url, data: data,
                type: 'post', dataType: 'json', timeout: 30000,
                error: function () { self.menu.message(options.errorMessage); self.disable(false); },
                beforeSend: function (request) { self.disable(true); },
                complete: function (request, state) { },
                success: function (d, state) { self.menu.message(); callback(d); }
            });
        },

        doEdit: function () {
            var self = this;
            self.disable(true, true);
            $.pop({
                url: self.params.editUrl,
                title: self.params.summary,
                width: 800,
                height: 580,
                frameHeight: '100%',
                popupOnTop: true,
                onclose: function () {
                    self.disable(false);
                    if (top.kooboo.data('parent-page-reload')) {
                        //kooboo.cms.ui.getOpener().reload();
                        window.location.reload();
                    }
                }
            });
        },

        doCopy: function () {
            var self = this;
            var message = function (msg) { self.menu.message(msg, self.menu.components.btnCopy.el); };
            //message(options.copying);
            this.doPost(this.copyAction, this.params, function (data) {
                if (!data.Success) {
                    message(options.copyFailure);
                    self.disable(false);
                    return;
                }
                // successful
                message(); //message(options.copySuccess);
                //setTimeout(function () {
                // copy client content
                var clone = self.el.clone().hide().insertAfter(self.el);
                $.each(data.Model, function (key, val) {
                    clone.attr(key, val);
                });
                // update parameters
                clone.find('[uuid=' + self.params.uuid + ']').attr('uuid', data.Model.uuid);
                // bind component
                yardi.initInlineEdit(clone.wrap('<div style="display:none;"></div>').parent());
                clone.unwrap();
                // show
                clone.slideDown('normal');
                self.disable(false);
                //}, 800);
            });
        },

        doDelete: function () {
            var self = this;
            yardi.anchorBar.fixAll(true);
            if (!confirm(options.confirmDel)) {
                yardi.anchorBar.fixAll(false);
                return;
            }
            var message = function (msg) { self.menu.message(msg, self.menu.components.btnRemove.el); };
            //message(options.deleting);
            this.doPost(this.deleteAction, this.params, function (data) {
                if (!data.Success) {
                    message(options.deleteFailure);
                    self.disable(false);
                    return;
                }
                // successful
                message(); //message(options.deleteSuccess);
                //setTimeout(function () {
                // remove component
                self.remove();
                // remove element
                self.el.slideUp('normal', function () {
                    $(this).remove();
                });
                self.disable(false);
                //}, 800);
            });
        },

        doPublish: function () {
            var self = this;
            var message = function (msg) { self.menu.message(msg, self.menu.components.btnPublish.el); };
            //message(options.publishing);
            var postData = $.extend({}, this.params, { fieldName: 'Published', value: 'True' });
            this.doPost(this.updateAction, postData, function (data) {
                if (!data.Success) {
                    message(options.publishFailure);
                    self.disable(false);
                    return;
                }
                // successful
                message(); //message(options.publishSuccess);
                //setTimeout(function () {
                self.setPublish(true);
                self.disable(false);
                //}, 800);
            });
        }
    };

    // register
    yardi.blockClass = blockClass;

})(jQuery);
