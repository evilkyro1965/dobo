<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<script type="text/javascript">

    $(document).ready(function () {

        $('input:visible').first().focus();

        $(window).bind('unload', function () {
            kooboo.cms.ui.loading().show();
        });

        /* 
        fixed page unload event in ie 
        under ie javascript link can trigger page unload event.
        sometimes it will show an confirm dialog .   
        */
        var javascriptLinkReg = /javascript:;/g;

        $('a').each(function () {
            var tabIndex = this.tabindex;
            tabIndex = tabIndex != undefined ? tabIndex : 1000;
            this.setAttribute('tabindex', tabIndex);

            if (javascriptLinkReg.test(this.href)) {
                $(this).click(function () {
                    return false;
                });
            }
        });

        /* end fixed page unload event in ie   */

        $('input[value-type=float]').numeric();
        $('input[value-type=int32]').numeric(false);

        //init import dialog
        $('a[name=import]').unbind('click').pop({
            useContent: true,
            contentId: 'dialog',
            width: 600,
            height: 200,
            title: 'Import'
        });

        //init dialog links 
        $('a.dialog-link').pop({
            width: 800,
            height: 580,
            frameHeight: "100%",
            popupOnTop: true
        });

        //init copy dialog 
        var copyDialog = $('#copy-form').dialog({
            width: 600,
            height: 200,
            title: 'Copy',
            autoOpen: false,
            modal: true,
            close: function () {
                copyDialog.find('form').get(0).reset();
            }
        });

        $('a.common-copy').mousedown(function (e) {
            if (e.which != 3) {
                copyDialog.dialog('open');
                copyDialog.find('form').attr('action', this.href);
            }
            return false;
        }).bind("contextmenu", function () { return false; })
        .click(function () {
            return false;
        });


        (function initTinymce() {

            $('textarea.tinymce').each(function () {
                var textarea = $(this);

                tinyMCE.init({
                    '$textarea': textarea,
                    'media_library_url': textarea.attr('media_library_url'),
                    'media_library_title': textarea.attr('media_library_title'),
                    elements: textarea.attr('id'),
                    mode: "exact",
                    plugins: "fullscreen,autoresize,searchreplace,KoobooMediaLibrary",
                    theme_advanced_toolbar_location: "top",
                    theme_advanced_toolbar_align: "left",
                    theme: "advanced",
                    theme_advanced_buttons1: "search,replace,undo,redo,|,KoobooMediaLibrary,image,link,unlink,charmap,|,bold,italic,fontselect,forecolor,backcolor,|,numlist,bullist,formatselect,|,code,fullscreen",
                    theme_advanced_buttons2: "",
                    theme_advanced_buttons3: "",
                    relative_urls: false

                });
                // copy content to texteare onsubmit
                kooboo.cms.ui.event.ajaxSubmit(function () {
                    var html = textarea.next().find('iframe').contents().find('body').html();
                    if (html) {
                        textarea.val(html);
                    }
                });
            });


        })();



        $('.icon-switch').click(function () {
            $(this).siblings('.list').slideToggle("normal");
        }).blur(function () {
            $(this).siblings('.list').delay(500).slideUp("normal");
        });
        $('.menu li.has-sub a').click(function () {
            $(this).siblings('span.arrow').click();
        });
        $('.menu span.arrow').click(function () {
            $(this).parent().toggleClass("active");
        });

        //init tooltip 
        $('.tooltip-link').yardiTip({ offsetX: -20 });

    });
</script>
