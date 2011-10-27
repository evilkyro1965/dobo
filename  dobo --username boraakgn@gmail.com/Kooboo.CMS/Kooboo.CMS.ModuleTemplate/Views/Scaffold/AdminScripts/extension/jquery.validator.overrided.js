/// <reference path="../jquery-1.4.4-vsdoc.js" />
/// <reference path="../jquery.validate-vsdoc.js" />
/// <reference path="../jquery.poshytip.min.js" />

//$.validator.setDefaults({
//	showErrors: function (errorMap, errorList) {
//		if (errorList) {
//			errorList.each(function (err, index) {
//				var el = err.element, message = err.message;
//				if (!$(el).data('error-tip-bind')) {
//					$(el).data('error-tip-bind', true)
//					$(el).poshytip({
//						className: 'tip-twitter',
//						content: message,
//						showOn: 'none',
//						alignTo: 'target',
//						alignY: 'center',
//						//						alignX: 'inner-left',
//						//						offsetX: index * 70,
//						//						offsetY: 5,
//						opacity: 0.7
//					});
//				}
//				$(el).poshytip('show').data('poshytip').update(message);
//				//kooboo.dump(this);
//				//alert(this.toHide());
//				//debugger;
//			});
//		}
//		//top.kooboo.dump(errorList);
//	},
//	_errorPromts: [],
//	'errorContainer':'tip-twitter',
//	success: function (label) {
//		alert(label);
//	}
//});