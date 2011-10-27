/// <reference path="../../tiny_mce_src.js" />

(function () {
    //tinymce.PluginManager.requireLangPack('KoobooMediaLibrary');
    tinymce.create('tinymce.plugins.KoobooMediaLibrary', {
        init: function (ed, url) {
            ed.addCommand('KoobooMediaLibrary', function () {
                var $textarea = ed.getParam('$textarea');
                /// on selected callback
                function onSelected(src, text, option) {                    
                    option = option || {};
                    var imgExp = /.jpg$|.png$|.gif$|.bmp$|.jpeg$/i, $dom;

                    if (!imgExp.test(src)) {
                        $dom = $(' <a>' + text + '</a> ').attr('href', src); ;
                    } else {
                        $dom = $('<img alt="IMG" />').attr('src', src);
                    }

                    $dom.attr(option);

                    ed.focus();
                    ed.selection.setNode($dom.get(0));
                }

                top.jQuery.pop({
                    url: ed.getParam('media_library_url'),
                    title: ed.getParam('media_library_title'),
                    width: 900,
                    height: 500,
                    frameHeight: '100%',
                    beforeLoad: function () {
                        kooboo.cms.ui.loading().show();
                    },
                    onload: function (handle, pop, config) {
                        kooboo.cms.ui.messageBox().hide();
                        top.kooboo.data('onFileSelected', onSelected);
                        top.kooboo.data('fileSelectPop', pop);
                    },
                    onclose: function (handle, pop, config) {
                        kooboo.data('onFileSelected', undefined);
                        kooboo.data('fileSelectPop', undefined);
                        setTimeout(function () {
                            pop.destory();
                            pop.remove();
                        }, 10);
                    }
                });

            });
            ed.addButton('KoobooMediaLibrary', {
                cmd: 'KoobooMediaLibrary',
                title: 'Media Content'
            });
        },
        getInfo: function () {
            return {
                longname: 'İçerik Yönetim Sistemi',
                author: 'İçerik Yönetim Sistemi',
                authorurl: '',
                infourl: '',
                version: tinymce.majorVersion + "." + tinymce.minorVersion
            };
        }
    });
    tinymce.PluginManager.add('KoobooMediaLibrary', tinymce.plugins.KoobooMediaLibrary);
})();

