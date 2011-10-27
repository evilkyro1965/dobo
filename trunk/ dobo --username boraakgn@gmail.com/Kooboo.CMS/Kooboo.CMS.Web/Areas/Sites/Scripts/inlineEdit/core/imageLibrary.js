/*
*
* img library
* author: ronglin
* create date: 2010.10.28
*
*/

(function ($) {

    var imgLib = function (config) {
        $.extend(this, config);
        this.initialize();
    };

    imgLib.prototype = {

        url: null, width: 700, height: 400,

        title: 'Image Library',

        outerDialog: false,

        dialogTarget: null,

        initialize: function () { },

        // callback envents
        OnSelect: function (url, w, h, alt) { },

        breakLeak: function () {
            if (!this.dialogTarget) { return; }
            var iframe = this.dialogTarget.children('iframe').get(0);
            if (!iframe) { return; }
            try {
                iframe.outerApi = undefined;
                delete iframe['outerApi'];
            } catch (ex) { }
            iframe.src = 'javascript:false;';
            iframe.parentNode.removeChild(iframe);
        },

        show: function () {
            var zIndex = yardi.zIndexMax + 100;
            if (this.outerDialog) {
                window.designer && window.designer.showDialog({
                    url: this.url,
                    title: this.title,
                    zIndex: zIndex
                }, {
                    OnSelect: this.OnSelect
                });
            } else {
                this.close();
                var self = this;
                this.dialogTarget = $('<div></div>').appendTo('body').dialog({
                    position: 'center',
                    modal: true,
                    width: this.width,
                    height: this.height,
                    title: this.title,
                    zIndex: zIndex
                });
                this.dialogTarget.bind('dialogclose', function () { self.close(); });
                this.dialogTarget.append('<iframe id="__imagelibrary" frameBorder="0" style="width:100%;height:100%;" src="' + this.url + '"></iframe>');
                this.dialogTarget.children('iframe').get(0).outerApi = this;
            }
        },

        close: function () {
            if (this.dialogTarget) {
                this.breakLeak();
                this.dialogTarget.remove();
                this.dialogTarget = null;
            }
        }
    };

    // register
    yardi.imgLib = imgLib;

})(jQuery);
