/// <reference path="../../../Scripts/jquery-1.4.1-vsdoc.js" />
/// <reference path="../../../Scripts/jQueryUI/jquery-ui-1.8.4.all.min.js" />


/// <reference path="../../../Scripts/kooboo.js" />
/// <reference path="../../../Scripts/kooboo.cms.ui.js" />

kooboo.cms.extend({
	messageBox: function () {
		var messageBox = $("#message-box");
		var api = {
			show: function (tip, tipClass, timeout) {
				tip = tip || "Success";
				timeout = timeout || 5000;
				tipClass = tipClass || "info";

				messageBox.find("p.waiting").addClass("hide");


				messageBox.removeClass("hide").removeClass("info warning error").addClass(tipClass);

				messageBox.find("p.msg").html(tip).removeClass("hide");

				setTimeout(function () {
					messageBox.addClass("hide").removeClass("info warning error");
				}, timeout);
			},
			hide: function () {
				messageBox.addClass("hide").removeClass("info warning error");
				messageBox.find("p.waiting").add("hide");
			},
			waiting: function () {
				messageBox.removeClass("hide");
				messageBox.find("p.msg").addClass("hide");
				messageBox.find("p.waiting").removeClass("hide");
			}
		};
		return api;
	}
});
kooboo.namespace("kooboo.cms.content.home");
kooboo.cms.content.home.extend({
	init: function (model) {
		var current = this;
		$(function () { current._init(model); });
	},
	_init: function (model) {
		var statusDic = ["disabled", "enabled", "done"];
		var popConfigDic = { "Repository": { width: 360, height: 160, isIframe: true, iframeHeight: "90%"} };


		for (var p in model) {
			var step = $("#" + p).addClass(statusDic[model[p].Status]);
			if (model[p].ActionUrl) {
				var a = step.find("a").attr("href", model[p].ActionUrl);
				if (a.hasClass("pop")) {
					a.pop(popConfigDic[p]);
				}
			}

		}

	}
});


kooboo.namespace("kooboo.cms.content");
kooboo.cms.content.extend({
	ready: function (current) {
		this.initImportDialog();
	},
	initImportDialog: function () {
		$('a[name="import"]').click(function () {
			var dialog = $("#dialog");
			dialog.dialog({
				title: $("#dialog .title").attr("title"),
				width: 420,
				height: 200,
				resizable: false,
				modal: true
			});
			return false;
		})
	}, //end initImportDialog
	dynamicList: function (config, flexListConfig) {
		///<summary>
		/// you can config option like this
		///option = {
		///    containerId:'',
		///    templateId:'',
		///    delClass:'',
		///    onInit:function(){},
		///    beforeAdd:function(){},
		///    onAdd:function(){},
		///    beforeRemove:function(){},
		///    onRemove:function(){},
		///    addButtonId:
		///}
		///</summary>

		var option = {
			containerId: '', //not null
			templateId: '', //not null
			addButtonId: '', //not null
			delClass: 'del',
			onInit: function () { },
			beforeAdd: function () { },
			onAdd: function () { },
			beforeRemove: function () { },
			onRemove: function () { }
		};

		kooboo.extend(option, config);

		var instance = kooboo.cms.ui.dynamicList.instance(option);

		initItems();

		function initItems() {
			var items = instance.getItems();

			///init remove method
			items.find('.' + option.delClass).click(function () {
				if (typeof (option.beforeRemove) == 'function') {
					var result = option.beforeRemove();
					if (result == false) {
						return false;
					}
				}
				instance.remove($(this).parents('#' + option.templateId));
				instance.sort();
				option.onRemove();
			});
			instance.sort();

			if (typeof (option.onInit) == 'function') {
				option.onInit(); //events after inits
			}
		}

		///init add method
		$('#' + option.addButtonId).unbind('click').click(function () {
			if (typeof (option.beforeAdd) == 'function') {
				var result = option.beforeAdd();
				if (result == false) {
					return false;
				}
			}
			var item = instance.add();

			initItems();

			item.siblings().find("input,select").unbind("blur");

			var inputSelects = item.find("input,select");

			inputSelects.first().focus();


			if (typeof (option.onAdd) == 'function') {
				option.onAdd();
			}
		});


		kooboo.cms.ui.flexList();
	}
});

///Class Folder

kooboo.namespace("kooboo.cms.content.folder");

kooboo.cms.content.folder.extend({

	initAllowExtension: function (option) {

		kooboo.cms.ui.dynamicListInstance(option);

	},

	initFolderUrlSettings: function (option) {

		kooboo.cms.ui.dynamicListInstance(option);

	},

	initGrid: function (confirmMsg, alertMsg) {

		$("a.delete").click(function () {

			var handle = $(this);

			var selectedStr = '';


			if (selected.length == 0) {

				alert(alertMsg);

				return false;

			}


			var selected = $("input:checkbox[name=Selected][checked1]").each(function () {

				selectedStr += $(this).val() + ',';

			});



			if (confirm(confirmMsg)) {

				var request = $.request(handle.attr("href"));

				request.queryString['folderStr'] = selectedStr;

				document.location.href = request.getUrl();

			}


			return false;

		});


	}

});



kooboo.cms.content.extend({

	selectListView: function (option) {

		$(function () {

			var selectList = kooboo.cms.ui.selectList(option);

			var buttons = {};

			$('.buttonField').find('input:button,input:submit').each(function () {

				var current = $(this);

				var html = current.val();

				buttons[html] = function () { current.click(); };

				current.hide();

			});


			var dialog = $('#selectList');

			//dialog.find('table').flexigrid();

			dialog.find('input:button,input:submit').button();

			var dialog = dialog.dialog({ modal: true, buttons: buttons, autoOpen: false, width: dialog.width() });

			$("#cancelSelect").click(function () {

				dialog.dialog("close");

			});

			$("#saveSelect").click(function () {

				alert('save');

			});


			$('#selectCategory,#selectDisplay').click(function () {

				dialog.dialog('open');

			});


			$("#select_addbtn").click(function () {

				selectList.add();

			});

			$("#select_removebtn").click(function () {

				selectList.remove();

			});

			$("#select_addallbtn").click(function () {

				selectList.addAll();

			});

			$("#select_removeallbtn").click(function () {

				selectList.removeAll();

			});

		});


	}

});



kooboo.cms.content.extend({

	checkList: function (option) {

		$(document).ready(function () {

			var checkboxList = kooboo.cms.ui.iframeCheckboxList(option);

			var container = $("#checkListData");

			container.find('button').button();


			container.find("button.save").click(function () {

				kooboo.console.write("cheched count");

				kooboo.console.write(checkboxList.checkedList.length);

			});

			container.find("button.cancel").click(function () {

				container.dialog("close");

			});


			var title = container.children(".title").html();

			container.dialog({ autoOpen: false, width: 600, title: title });


		});

	}

});


kooboo.cms.content.extend({

	initSelectCategory: function () {

		$(document).ready(function () {
			function changePopHref(currentHandle) {

				var fullName = $.request(currentHandle.attr('href')).queryString["categoryFolder"];

				var displayId = "[name=cat_" + fullName + "_display]";

				var valueId = "[name=cat_" + fullName + "_value]";

				var href = currentHandle.attr('href');

				$.request(href).queryString['selected'] = $(valueId).val();

				var url = $.request(href).getUrl();

				currentHandle.attr('href', url);

				return url;

			}
			$('.categoryButton').each(function () {

				$(this).pop({

					isIframe: true,

					frameHeight: "98%",

					width: 700,

					height: 506,

					beforeLoad: changePopHref,

					popupOnTop: true,

					onload: function (currentHandle, pop, config) {
						var fullName = $.request(currentHandle.attr('href')).queryString["categoryFolder"];

						var displayId = "[name=cat_" + fullName + "_display]";

						var valueId = "[name=cat_" + fullName + "_value]";

						var iframe = pop.children('iframe');

						var contents = pop.children('iframe').contents();

						var selectedTable = iframe.contents().find('table.selected tbody');

						var datasourceTable = iframe.contents().find('table.datasource tbody');

						var checkedList = $.request(iframe[0].src).queryString['selected'] ? $.request(iframe[0].src).queryString['selected'].split(',') : [];

						var checkboxList = top.kooboo.cms.ui.iframeCheckboxList({

							iframeId: iframe.attr('id'),

							checkedList: checkedList,

							onItemChecked: itemChecked,

							onItemInit: itemInit,

							cache: false

						});

						function itemInit(result, checkItem) {

							var parent = checkItem.parent().parent();

							var val = checkItem.val();

							var query = selectedTable.find('[value="' + val + '"],[title="' + val + '"]');

							if (result) {

								parent.addClass('selected');

								if (query.length == 0) {

									var clone = parent.clone();

									clone.removeClass('selected hover');

									clone.find("input:checkbox").remove();

									clone.children('td:last').html('<a href="javascript:;" class="remove-selected" title="' + checkItem.val() + '"><img src="/Areas/Contents/Images/Icons/delete.png" alt="Remove"></a>');


									clone.appendTo(selectedTable);

									initRemoveSelected();

								}

							} else {

								parent.removeClass('selected');

								if (query.length > 0) {

									query.parent().parent().remove();

								}

							}

						}

						function itemChecked(result, checkItem) {

							itemInit(result, checkItem);

							initHref();

						}

						function initHref() {

							var checked = checkboxList.getCheckedList();

							var selectedStr = '';

							for (var i = 0; i < checked.length; i++) {

								selectedStr += checked[i] + ',';

							}

							if (selectedStr.length > 0) {

								selectedStr = selectedStr.substring(0, selectedStr.length - 1);

							}

							contents.find("div.pager a").each(function () {

								if (this.href) {

									var href = this.href;

									$.request(href).queryString['selected'] = selectedStr;

									this.href = $.request(href).getUrl();


									$(this).click(function () {

										iframe[0].src = this.href;

									});

								}

							});

						}

						function initRemoveSelected() {

							selectedTable.find('a.remove-selected').unbind("click").click(function () {

								checkboxList.remove(this.title);

								$(this).parent().parent().remove();

								initHref();

								return false;

							});

						};

						initRemoveSelected();

						(function initSourceTable() {

							iframe.contents().find('table.datasource tr').css('cursor', 'pointer').hover(
                        function () { $(this).addClass('hover'); },
                        function () {
                        	$(this).removeClass('hover');
                        }).click(function () {
                        	var chectItem = $(this).find("input:checkbox");
                        	if (chectItem.attr("checked") == true) {
                        		chectItem.attr("checked", false);
                        		checkboxList.remove(chectItem.val());
                        	} else {
                        		chectItem.attr("checked", true)
                        		checkboxList.add(chectItem.val());
                        	}
                        	initHref();
                        });

						})();

						(function initButton() {

							var button = contents.find('button.form-button'); //.button().height(30);

							contents.find('button.save').click(function () {
								//debugger;
								var checked = checkboxList.getCheckedList();
								var valueStr = '';
								for (var i = 0; i < checked.length; i++) {

									valueStr += ',' + checked[i];

								}

								valueStr = valueStr.length > 0 ? valueStr.substring(1) : valueStr;

								$(valueId).val(valueStr);


								var displayStr = '';

								datasourceTable.find('tr.selected').each(function () {

									displayStr += ',' + $(this).find('td:eq(1)').text().trim();

								});

								displayStr = displayStr.length > 0 ? displayStr.substring(1).trim() : displayStr;

								$(displayId).val(displayStr).attr('title', displayStr);

								var src = changePopHref(currentHandle);

								pop.close();

								return false;

							});

							contents.find('button.cancel').click(function () {

								pop.close();

								return false;

							});

						})(); // end  initbutton

						//itemChecked();

					}, //end onload

					onclose: function (handle, pop, congfig) {
						var form = pop.find('iframe').contents().find('form');
						//form.get(0).reset();
					}
				}); //end $().pop

			});
		}); //end document.ready

	}

});


kooboo.namespace("kooboo.cms.content.mediaFolder");

kooboo.cms.content.mediaFolder.extend({
	initUploadForm: function () {


	}

});



kooboo.namespace("kooboo.cms.content.sendingSetting");

kooboo.cms.content.sendingSetting.extend({

	initAutoComplete: function (repository) {

		$(function () {

			$("#FolderName").autocomplete({

				source: "/Contents/SendingSetting/FolderDataSource/" + repository + "/"

			});

			$("#SchemaName").autocomplete({

				source: "/Contents/SendingSetting/SchemaDataSource/" + repository + "/"

			});

		});


	}

});


kooboo.cms.content.extend({

	initContentMessageCenter: function () {


		var koobooAlert = $("#AlterMessage").val();

		var koobooTip = $("#koobooTip").val();


		var koobooConfirm = $("#koobooComfirm").val();


		if (koobooAlert) {

			jAlert(koobooAlert);

		}


		if (koobooConfirm) {

			jConfirm(koobooConfirm);

		}



	}

});



kooboo.namespace("kooboo.cms.content.textcontent");

kooboo.cms.content.textcontent.extend({
	initGrid: function () {

		$(function () { kooboo.cms.content.textcontent._initGrid(); });
	},
	_initGrid: function () {
		$(document).click(function () {
			$('div.dropdown-button').children('div').addClass('hide');
		});
		var dropdown = $('div.dropdown-button').click(function (e) {
			e.stopPropagation();
			var menu = $(this).children('div');
			if (menu.hasClass('hide')) {
				menu.removeClass('hide');
			} else {
				menu.addClass('hide');
			}
		}).children().click(function () {
			$('div.dropdown-button').children('div').addClass('hide');
		});

		$('a.delete.button').click(function () {
			var handle = $(this);
			var docArr = [];
			var folderArr = [];
			var removeTrs = [];
			$('input:checkbox:checked.folders').each(function () {
				folderArr.push($(this).val());
			});
			$('input:checkbox:checked.docs').each(function () {
				docArr.push($(this).val());
				removeTrs.push($(this).parents('tr:eq(0)'));
			});

			var form = $('<form></form>')
			.attr('action', handle.attr('href'))
			.attr('method', 'post');

			kooboo.cms.ui.formHelper.createHidden(folderArr, 'folderArr', form);
			kooboo.cms.ui.formHelper.createHidden(docArr, 'docArr', form);

			form.ajaxSubmit({
				beforeSend: function () {
					kooboo.cms.messageBox().waiting();
				},
				success: function (response) {
					if (response.Success) {
						if (folderArr.length > 0) {
							document.location.reload();
						} else {
							removeTrs.each(function (tr) { tr.fadeOut(function () { tr.remove(); }); });
						}
					} else {
						kooboo.cms.ui.messageBox().showResponse(response);
					}
				}
			});

			return false;
		});
	}, //end initGrid

	initVersion: function (tip, title) {

		var current = this;

		$(function () { current._initVersion(tip); });

	}, //end initVersion

	_initVersion: function (tip, title) {

		tip = tip || "please select two versions!";

		title = title || "Diff";

		$("input:checkbox:eq(0)").hide();


		var diffButton = $("a.diff.button").click(function () {

			var current = $(this);

			var selected = $("input:checkbox[checked]");


			if (selected.length < 2) {

				alert(tip);

				return false;

			} else {

				var href = current.attr("href");


				var request = $.request(href);


				request.queryString["v1"] = selected.first().val();

				request.queryString["v2"] = selected.last().val();


				$({}).pop({

					url: request.getUrl(),

					autoOpen: true,

					width: 900,

					height: 600,

					frameHeight: "98%",

					popupOnTop: true,

					openOnclick: false,

					onclose: function (handle, pop, config) {

						pop.destory();

					},

					title: title

				});

			}



			return false;

		});






		$("input:checkbox").change(function () {

			var selectedCount = $("input:checkbox[checked]").length;

			if (selectedCount > 2) {

				$(this).attr("checked", false);

				return false;

			}

		});

	}

});
