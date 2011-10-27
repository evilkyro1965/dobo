/*
*
* arrow panel
* author: ronglin
* create date: 2010.06.17
*
*/


/*
* config parameters:
* width, title, renderTo, bodyBuilder, onClose
*/

(function ($) {

    /*
    * twinkle
    * function1, functon2, callback, times
    */
    var twinkle = function (func1, func2, cb, times) {
        times = times || 2;
        var count = 0, tw;
        (tw = function () {
            if (count < times) {
                count++;
                func1();
                setTimeout(function () {
                    func2();
                    setTimeout(function () {
                        tw();
                    }, 150);
                }, 150);
            } else {
                cb && cb();
            }
        })();
    };

    var arrowPanel = function (config) {
        $.extend(this, config);
    };

    arrowPanel.prototype = {

        el: null, renderTo: 'body',

        inited: false, showed: false,

        title: '', width: 160, showArrow: false,

        onClose: function (ev) { }, bodyBuilder: null,

        headTarget: null, closeTarget: null, titleTarget: null, arrowTarget: null,

        // build html
        buildHtml: function () {
            var html = [];
            html.push('<var class="kb-arrowpanel">');
            if (this.showArrow) { html.push('<var class="kb-arrow"></var>'); }
            html.push('<var class="kb-head">');
            html.push('<var class="kb-close" title="close">X</var>');
            html.push('<var class="kb-title"></var>');
            html.push('</var>');
            html.push('<var class="kb-body">');
            html.push(this.bodyBuilder ? this.bodyBuilder() : '');
            html.push('</var>');
            html.push('</var>');
            return html.join('');
        },

        // private, init
        init: function () {
            // build
            var html = this.buildHtml();
            this.el = $(html).appendTo(this.renderTo);
            if (this.width != null) this.el.width(this.width);
            // targets
            this.headTarget = $('.kb-head', this.el);
            this.closeTarget = $('.kb-close', this.el);
            this.titleTarget = $('.kb-title', this.el);
            if (this.title) { this.titleTarget.html(this.title); }
            // arrow left
            if (this.showArrow) {
                this.arrowTarget = $('.kb-arrow', this.el);
                this.arrowLeft(null);
            }
            // events
            this.bindEvents();
            // set status
            this.inited = true;
        },

        // provate
        _docKeydown: null,
        _docMouseup: null,
        _docMousemove: null,

        // private, bind events
        bindEvents: function () {
            // drag move
            var self = this, drag = false, event, left = 0, top = 0, selfSize, viewport;
            this.titleTarget.mousedown(function () {
                drag = true;
                top = parseInt(self.el.css('top').replace(/\D/g, ''), 10);
                left = parseInt(self.el.css('left').replace(/\D/g, ''), 10);
                selfSize = { width: self.el.outerWidth(), height: self.el.outerHeight() };
                viewport = yardi.getViewportSize(window);
            });
            this._docKeydown = function (ev) {
                (ev.keyCode == 27) && self.closeTarget.click();
            };
            this._docMouseup = function () {
                drag = false;
            };
            this._docMousemove = function (e) {
                if (drag && event) {
                    left -= event.pageX - e.pageX;
                    top -= event.pageY - e.pageY;
                    self.el.css({
                        left: Math.min(Math.max(left, 0), viewport.width - selfSize.width),
                        top: Math.min(Math.max(top, 0), viewport.height - selfSize.height)
                    });
                }
                event = e;
                return !drag;
            };
            $(document).keydown(this._docKeydown).mouseup(this._docMouseup).mousemove(this._docMousemove);
            // close
            this.closeTarget.click(function (ev) {
                self.onClose(ev);
                self.remove();
            });
        },

        // public 
        remove: function () {
            $(document).unbind('keydown', this._docKeydown).unbind('mouseup', this._docMouseup).unbind('mousemove', this._docMousemove);
            this.hide();
            this.el.remove();
        },

        // public
        getPos: function (refEl) {
            return yardi.flatPos(this.el, $(refEl));
        },

        // arrow left
        arrowLeft: function (left) {
            if (this.showArrow) {
                var max = this.el.width() - this.arrowTarget.width();
                if (yardi.isNumber(left)) {
                    left = Math.max(left, 0);
                    left = Math.min(left, max);
                } else {
                    left = (max) / 2;
                }
                this.arrowTarget.css({ left: left });
            }
        },

        // twinkle
        twinkle: function () {
            var self = this;
            twinkle(function () {
                self.headTarget.addClass('kb-twinkle');
            }, function () {
                self.headTarget.removeClass('kb-twinkle');
            });
        },

        // public
        show: function (refEl) {
            if (this.inited == false) { this.init(); }
            var pos = this.getPos(refEl);
            this.showPos(pos);
        },

        // public
        showPos: function (pos) {
            if (this.inited == false) { this.init(); }
            if (this.showed == true) { return; }
            if (this.showArrow && yardi.isNumber(pos.top)) { pos.top += this.arrowTarget.height(); }
            yardi.zTop(this.el);
            this.el.css({
                top: pos.top,
                left: pos.left,
                display: 'none'
            });
            this.el.slideDown('fast');
            this.showed = true;
            yardi.modeling = true;
            yardi.dialoging = true;
        },

        // public
        hide: function () {
            if (this.inited == false || this.showed == false) { return; }
            // hide
            this.el.css('display', 'none');
            yardi.zOld(this.el);
            this.showed = false;
            yardi.modeling = false;
            yardi.dialoging = false;
        }
    };

    // register
    yardi.arrowPanel = arrowPanel;

})(jQuery);
