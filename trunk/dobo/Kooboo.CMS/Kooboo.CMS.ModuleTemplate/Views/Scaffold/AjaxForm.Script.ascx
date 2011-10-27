<script type="text/javascript" language="javascript">
    $(function () {
        function getOpener() {
            var opener = window.parent || window.top;

            var hasPop = false;
            if ($.popContext.popList.length >= 1) {
                opener = $.popContext.getCurrent();
                hasPop = true;
            }

            var api = {
                reload: function (callback) {
                    if (hasPop) {
                        opener.iframe.get(0).src = opener.iframe.get(0).src;
                    }
                    else {
                        opener.location.reload(true);
                    }
                    if (callback) { callback(); }
                },
                redirectTo: function (href, callback) {
                    if (hasPop) {
                        opener.iframe.get(0).src = href;
                    }
                    else {
                        opener.location.href = href;
                    }
                    if (callback) { callback(); }
                }
            }

            return api;
        }

        $(document).ajaxStop(function () {
            kooboo.cms.ui.loading().hide();
        });

        $(document).ajaxError(function () {
            kooboo.cms.ui.loading().hide();
        });

        $('a.boolean-ajax-link').click(function () {
            var handle = $(this), confirmMsg;

            if (handle.hasClass('cross')) {
                confirmMsg = handle.attr('confirmMsg');
            } else {
                confirmMsg = handle.attr('unConfirmMsg');
            }

            kooboo.confirm(confirmMsg, function (r) {
                if (r) {
                    var dataField = handle.attr('data');

                    $.ajax({
                        url: handle.attr('href'),
                        type: 'post',
                        beforeSend: function () {
                            kooboo.cms.ui.loading().show();
                        },
                        success: function (response) {
                            if (response.Success) {
                                if (handle.hasClass('cross')) {
                                    handle.removeClass('cross');
                                    handle.addClass('tick');
                                } else {
                                    handle.removeClass('tick');
                                    handle.addClass('cross');
                                }
                            } else {
                                kooboo.cms.ui.messageBox().showResponse(response);
                            }
                            kooboo.cms.ui.loading().hide();
                        },
                        error: function () {
                            kooboo.cms.ui.messageBox().show('There is an error occurs', 'error');
                        }
                    });
                }
            });


            return false;
        });

        if (!kooboo.data('kooboo-formHandle-executed')) {
            kooboo.data('kooboo-formHandle-executed', true);
            if (window.formHandle) {
                window.formHandle();
            } else {
                $('form:not(.no-ajax)').bind(function () {
                    var form = $(this);
                    kooboo.cms.ui.event.ajaxSubmit(form);
                });

                $('form:not(.no-ajax)').each(function () {
                    var form = $(this).submit(function () {
                        if (!form.valid()) {
                            if (form.find('.tabs').length) {
                                var selector = 'input.input-validation-error,select.input-validation-error';
                                $(selector).parents('div.tab-content')
								.each(function () {
								    var tab = $(this);
								    var li = $('a[href="#' + tab.attr('id') + '"]')
									.hide().show('pulsate', {}, 100)
									.show('highlight', {}, 200)
									.show('pulsate', {}, 300)
									.show('highlight', {}, 400);
								});
                            }
                        }
                    });
                    form.ajaxForm({
                        dataType: 'json',
                        beforeSerialize: function () {
                            var globResult = kooboo.cms.ui.event.ajaxSubmit();
                            //var selfResult = kooboo.cms.ui.event.ajaxSubmit(form);
                            return globResult; //!= false && selfResult != false;
                        },
                        beforeSend: function () {
                            kooboo.cms.ui.loading().show();
                            form.find("[type=submit]").addClass("disabled").attr("disabled", true);
                        },
                        beforeSubmit: function (arr, $form, options) {
                            var stayHere = arr.where(function (val, index) { return val.name == 'stayHere' }).first();
                            if (stayHere != null)
                                form.data("stayHere", stayHere.value.toLower());
                        },
                        success: function (response, statusText, xhr, $form) {
                            kooboo.cms.ui.messageBox().hide();
                            var responseData = response;
                            if (typeof (responseData) == 'string') {
                                responseData = $.parseJSON($(responseData).text());
                            }

                            kooboo.cms.ui.messageBox().showResponse(responseData);
                            form.find("[type=submit]").removeClass("disabled").attr("disabled", false);
                            if (!responseData.Success) {
                                var validator = form.validate();
                                //                            var errors = [];
                                for (var i = 0; i < responseData.FieldErrors.length; i++) {
                                    var obj = {};
                                    obj[responseData.FieldErrors[i].FieldName] = responseData.FieldErrors[i].ErrorMessage;
                                    validator.showErrors(obj);
                                }
                            }
                            else {
                                if (window.onSuccess && (typeof (onSuccess) == 'function')) {
                                    onSuccess(responseData);
                                } else {
                                    if (top.jQuery.popContext.getCurrent()) {
                                        top.jQuery.popContext.getCurrent().close();
                                        top.kooboo.cms.ui.loading().show();
                                    }
                                    var stayHere = form.data("stayHere");
                                    if (stayHere != "true") {
                                        if (responseData.RedirectUrl) {
                                            getOpener().redirectTo(responseData.RedirectUrl, top.kooboo.cms.ui.loading().hide);
                                        } else {
                                            getOpener().reload(top.kooboo.cms.ui.loading().hide);
                                        }
                                    }
                                }
                            }
                        },
                        error: function () {
                            kooboo.cms.ui.loading().hide();
                            form.find("[type=submit]").removeClass('disabled').attr('disabled', false);
                        }
                    });
                })
            }
        }
    });
</script>
