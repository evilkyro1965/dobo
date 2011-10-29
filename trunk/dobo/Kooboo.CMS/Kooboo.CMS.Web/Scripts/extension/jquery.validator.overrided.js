/// <reference path="../jquery.js" />
/// <reference path="../jquery.validate.js" />
(function () {
    var spliterArr = '-,.,_,/,\\,~'.split(','),
    spliterLength = spliterArr.length,
    spliterChecker = /(-)|(\.)|(_)|(\/)|(\\)|(\~)/,
    yearChecker = /yy/,
    monthChecker = /(mm|m)/,
    dayChecker = /(dd|d)/,
    formatChecker = /(yyyy|yy)|(mm|m)|(dd|d)/,
     dateParser = function (valStr, formatStr) {
         formatStr = formatStr.replace(/,/g, '_');
         var grouping = spliterChecker.exec(formatStr), spliter;
         for (var i = 0; i < spliterLength; i++) {
             if (grouping[i]) { spliter = spliterArr[i]; }
         }

         var valArr = valStr.split(spliter),
         formatArr = formatArr.split(spliter),
         year, month, day;

         for (var i = 0; i < formatArr.length; i++) {
             if (yearChecker.test(formatArr[i])) {
                 year = valArr[i];
             } else if (monthChecker.test(formatArr[i])) {
                 month = valArr[i];
             } else if (dayChecker.test(formatArr[i])) {
                 day = valArr[i];
             }
         }
     }
})();