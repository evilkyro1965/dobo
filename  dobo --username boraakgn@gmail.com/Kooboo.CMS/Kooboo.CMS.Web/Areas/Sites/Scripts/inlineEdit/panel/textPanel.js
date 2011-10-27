/*
*
* text panel
* author: ronglin
* create date: 2010.06.22
*
*/

/*
* config parameters:
* width, title, renderTo, onOk, onCancel, onPreview
*/

(function ($) {

    var textPanel = function (config) {
        config.width = 450;
        textPanel.superclass.constructor.call(this, config);
    };

    yardi.extend(textPanel, yardi.arrowPanel, {

        // public config
        title: 'text panel',
        textValue: '',

        // public event
        onOk: function (newCssText) { },
        onCancel: function (oldCssText) { },
        onPreview: function (cssText) { },
        onClose: function (ev) { $('input[_cel=1]', this.el).click(); },

        bodyBuilder: function () {
            var html = [];
            html.push('<var class="kb-textpanel">');
            html.push('<var class="kb-row">');
            html.push('<textarea _area="1" class="kb-area"></textarea>');
            html.push('</var>');
            html.push('<var class="kb-row">');
            html.push('<input _prv="1" class="kb-btn" type="button" value="preview" />');
            html.push('<input _rst="1" class="kb-btn" type="button" value="reset" />');
            html.push('<input _ok="1" class="kb-btn" type="button" value="ok" />');
            html.push('<input _cel="1" class="kb-btn" type="button" value="cancel" />');
            html.push('</var>');
            html.push('</var>');
            return html.join('');
        },

        bindEvents: function () {
            textPanel.superclass.bindEvents.call(this);
            var self = this;
            var oText = $('textarea[_area=1]', this.el);
            oText.val(this.textValue);
            $('input[_ok=1]', this.el).click(function () {
                self.onOk(oText.val());
                self.remove();
            });
            $('input[_cel=1]', this.el).click(function () {
                self.onCancel(self.textValue);
                self.remove();
            });
            $('input[_prv=1]', this.el).click(function () {
                self.onPreview(oText.val());
            });
            $('input[_rst=1]', this.el).click(function () {
                oText.val(self.textValue);
                self.onPreview(self.textValue);
            });
        }
    });

    // register
    yardi.textPanel = textPanel;

})(jQuery);
