/*
*
* image panel
* author: ronglin
* create date: 2010.06.17
*
*/


/*
* config parameters:
* width, title, renderTo, onOk, onCancel
*/

(function ($) {

    var imagePanel = function (config) {
        config.width = 300;
        imagePanel.superclass.constructor.call(this, config);
    };

    yardi.extend(imagePanel, yardi.arrowPanel, {

        // public config
        title: 'image options',

        // public event
        onOk: function (val) { },
        onCancel: function () { },
        onDelete: function () { },

        urlTarget: null, descTarget: null, wTarget: null, hTarget: null,

        onLibrary: function (sender, ev) {
            var self = this;
            var lib = new yardi.imgLib({
                title: 'Media library',
                width: 900, height: 500,
                url: __inlineEditVars.MediaLibrary + '&fileType=image',
                OnSelect: function (url, w, h, alt) {
                    self.urlTarget.val(url);
                    if (!self.wTarget.val()) { self.wTarget.val(w); }
                    if (!self.hTarget.val()) { self.hTarget.val(h); }
                    self.descTarget.val(alt);
                }
            });
            lib.show();
        },

        bodyBuilder: function () {
            var html = [];
            html.push('<var class="kb-imagepanel">');
            html.push('<var class="kb-row">');
            html.push('URL:<input _url="1" class="kb-url" type="text" />');
            html.push('<input _lib="1" class="kb-btn kb-lib" type="button" value="library" />');
            html.push('</var>');
            html.push('<var class="kb-row">');
            html.push('ALT:<input _desc="1" class="kb-alt" type="text" />');
            html.push('</var>');
            html.push('<var class="kb-row">');
            html.push('Width:<input type="text" _w="1" class="kb-size" />&nbsp;Height:<input _h="1" type="text" class="kb-size" />');
            html.push('</var>');
            html.push('<var class="kb-bottom">');
            html.push('<input _ok="1" class="kb-btn" type="button" value="ok" style="float:left;" />');
            html.push('<input _cnl="1" class="kb-btn" type="button" value="cancel" style="float:left;" />');
            html.push('<input _del="1" class="kb-btn" type="button" value="remove" />');
            html.push('</var>');
            html.push('</var>');
            return html.join('');
        },

        buildImageHtml: function () {
            var html = [];
            html.push('<img alt="' + this.descTarget.val() + '"');
            html.push(' src="' + this.urlTarget.val() + '"');
            html.push(' width="' + this.wTarget.val() + '"');
            html.push(' height="' + this.hTarget.val() + '"');
            html.push(' />');
            return html.join('');
        },

        bindEvents: function () {
            imagePanel.superclass.bindEvents.call(this);
            var self = this;

            this.urlTarget = $('input[_url=1]', this.el);
            this.descTarget = $('input[_desc=1]', this.el);
            this.wTarget = $('input[_w=1]', this.el);
            this.hTarget = $('input[_h=1]', this.el);

            // btn ok
            $('input[_ok=1]', this.el).click(function () {
                self.onOk(self.urlTarget.val(),
                      self.descTarget.val(),
                      self.wTarget.val(),
                      self.hTarget.val(),
                      self.buildImageHtml());
            });
            // btn cancel
            $('input[_cnl=1]', this.el).click(function () {
                self.onCancel();
            });
            // btn library
            $('input[_lib=1]', this.el).click(function (ev) {
                self.onLibrary(self, ev);
            });
            // btn delete
            $('input[_del=1]', this.el).click(function () {
                self.onDelete();
            });
        }
    });

    // register
    yardi.imagePanel = imagePanel;

})(jQuery);
