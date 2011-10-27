
(function ($, tinymce) {

    if (!$ || !tinymce) { return; }

    // tinymce comment out the javascript and style content by default
    // it need to be restored to original format, and available for cms.
    var uncommentHtml = function (val) {
        var rscript = /<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi;
        val = val.replace(rscript, function (matched) {
            return matched.replace('>// <![CDATA[', '>').replace('// ]]></', '</');
        });
        var rstyle = /<style\b[^<]*(?:(?!<\/style>)<[^<]*)*<\/style>/gi;
        val = val.replace(rstyle, function (matched) {
            return matched.replace('><!--', '>').replace('--></', '</');
        });
        return val;
    };

    // to fix the problem when custom html all css style are reseted
    // we decide to add a class name(unreset) to all elements at top layer.
    // attention: can not use jQuery to analyze, jQuery will ignore script tag.
    var unresetHtml = function (val) {
        var wrap = document.createElement('div');
        wrap.style.position = 'absolute';
        wrap.style.left = '-10000px';
        wrap.style.top = '-10000px';
        document.body.appendChild(wrap);
        // fix bug: set html to element innerHTML, while style or script tag at the first position of html.
        // ie6 ie7 ie8 will remove the first style or script tag, so add a temporary span at front of the html.
        if ($.browser.msie && $.browser.version < 9) {
            wrap.innerHTML = '<span>temp</span>' + val;
            wrap.removeChild(wrap.firstChild);
        } else {
            wrap.innerHTML = val;
        }
        var ignoreTags = '|STYLE|SCRIPT|NOSCRIPT|'.toUpperCase();
        for (var i = 0; i < wrap.childNodes.length; i++) {
            var child = wrap.childNodes[i];
            if (child.nodeType === 1) { // element type
                if (ignoreTags.indexOf('|' + child.tagName + '|') === -1) {
                    $(child).addClass('unreset');
                }
            }
        }
        val = wrap.innerHTML;
        document.body.removeChild(wrap);
        return val;
    };

    // hack: getContent
    var getContent = tinymce.Editor.prototype.getContent;
    tinymce.Editor.prototype.getContent = function () {
        var content = getContent.apply(this, arguments);
        // for html source (tinymce plugin)
        if (arguments[0] && arguments[0].source_view === true) {
            return uncommentHtml(content);
        } else {
            return content;
        }
    };

    // hack: triggerSave
    var triggerSave = tinymce.triggerSave;
    tinymce.triggerSave = function () {
        triggerSave.apply(this, arguments);
        // core
        var len = tinymce.editors.length;
        for (var i = 0; i < len; i++) {
            var id = tinymce.editors[i].id;
            var textarea = $('#' + id);
            if (!textarea.length) { textarea = $('[name=' + id + ']'); }
            textarea.each(function () {
                var val = $(this).val();
                //val = unresetHtml(val);
                val = uncommentHtml(val);
                $(this).val(val);
            });
        }
    };

} (jQuery, window.tinymce || window.tinyMCE));
