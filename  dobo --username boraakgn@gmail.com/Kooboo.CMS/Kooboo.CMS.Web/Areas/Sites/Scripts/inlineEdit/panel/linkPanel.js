/*
*
* link panel
* author: ronglin
* create date: 2010.06.17
*
*/


/*
* config parameters:
* width, title, renderTo, onOk, onUnlink, onCancel
*/

(function ($) {

    var linkPanel = function (config) {
        config.width = 300;
        linkPanel.superclass.constructor.call(this, config);
    };

    yardi.extend(linkPanel, yardi.arrowPanel, {

        // public config
        title: 'link panel',
        linkText: '',

        // public event
        onOk: function (txt, url, title, target, html) { },
        onUnlink: function () { },
        onCancel: function () { },

        textTarget: null, urlTarget: null, titleTarget: null, targetTarget: null, //linkTypes: null,

        bodyBuilder: function () {
            var html = [];
            html.push('<var class="kb-linkpanel">');
            html.push('<var class="kb-row">');
            html.push('Text:<input _txt="1" class="kb-text" type="text" />');
            html.push('</var>');
            html.push('<var class="kb-row">');
            html.push('Url:<input _url="1" class="kb-url" type="text" />');
            html.push('<input _viw="1" class="kb-btn kb-view" type="button" value="view" />');
            html.push('</var>');
            html.push('<var class="kb-row">');
            html.push('Title:<input _til="1" class="kb-tle" type="text" />');
            html.push('</var>');
            //html.push('<var class="kb-row">');
            //html.push('Link Type:<input id="rdoNormal" name="linktype" type="radio" value="" class="kb-normal" /><label for="rdoNormal" class="kb-normal-text">normal</label><input id="rdoConfirm" name="linktype" type="radio" value="confirm" class="kb-confirm" /><label for="rdoConfirm" class="kb-confirm-text">confirm subscription</label><br/>');
            //html.push('<input id="rdoUnsubscribe" name="linktype" type="radio" value="unsubscribe" class="kb-unsubscribe" /><label for="rdoUnsubscribe" class="kb-unsubscribe-text">unsubscribe</label><input id="rdoOnlineversion" name="linktype" type="radio" value="onlineversion" class="kb-onlineversion" /><label for="rdoOnlineversion" class="kb-onlineversion-text">online version</label>');
            //html.push('</var>');
            html.push('<var class="kb-row kb-newwin">');
            html.push('<input _chk="1" class="kb-check" id="__check" type="checkbox" /><label for="__check"> Open in a new window</label>');
            html.push('</var>');
            html.push('<var class="kb-bottom">');
            html.push('<input _ok="1" class="kb-btn" type="button" value="ok" style="float:left;" />');
            html.push('<input _cnl="1" class="kb-btn" type="button" value="cancel" style="float:left;" />');
            html.push('<input _ulk="1" class="kb-btn" type="button" value="unlink" />');
            html.push('</var>');
            html.push('</var>');
            return html.join('');
        },

        buildLinkHtml: function () {
            var html = [];
            html.push('<a href="');
            html.push(this.urlTarget.val());
            html.push('" title="');
            html.push(this.titleTarget.val());
            html.push('" target="');
            html.push(this.targetTarget.attr('checked') ? '_blank' : '_self');
            html.push('">');
            html.push(this.textTarget.val());
            html.push('</a>');
            return html.join('');
        },

        bindEvents: function () {
            linkPanel.superclass.bindEvents.call(this);

            this.textTarget = $('input[_txt=1]', this.el);
            this.urlTarget = $('input[_url=1]', this.el);
            this.titleTarget = $('input[_til=1]', this.el);
            this.targetTarget = $('input[_chk=1]', this.el);
            //this.linkTypes = $('input[name=linktype]', this.el);

            var self = this;
            this.textTarget.val(this.linkText);

            // btn ok
            $('input[_ok=1]', this.el).click(function () {
                self.onOk(self.textTarget.val(),
                self.urlTarget.val(),
                self.titleTarget.val(),
                self.targetTarget.attr('checked') ? '_blank' : '_self',
                self.buildLinkHtml());
            });
            // btn unlink
            $('input[_ulk=1]', this.el).click(function () {
                self.onUnlink();
            });
            // btn cancel
            $('input[_cnl=1]', this.el).click(function () {
                self.onCancel();
            });
            // btn view
            $('input[_viw=1]', this.el).click(function () {
                var val = self.urlTarget.val();
                if (val.length > 0) {
                    (val.indexOf('#') != 0) && window.open(val);
                } else {
                    alert('please input the url.');
                }
            });
        }
    });

    // register
    yardi.linkPanel = linkPanel;

})(jQuery);
