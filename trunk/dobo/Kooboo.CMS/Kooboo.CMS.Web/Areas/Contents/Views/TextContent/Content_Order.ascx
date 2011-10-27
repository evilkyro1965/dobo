<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Kooboo.CMS.Web.Areas.Contents.Models.TextContentGrid>" %>
<%var folder = ViewData["Folder"] as Kooboo.CMS.Content.Models.TextFolder;
  var contentPagedList = ViewData["ContentPagedList"] as PagedList<Kooboo.CMS.Content.Models.TextContent>;
  var isFirstPage = contentPagedList.CurrentPageIndex == 1;
  var isLastPage = contentPagedList.CurrentPageIndex == contentPagedList.TotalPageCount;
%>
<%--  Order Begin --%>
<%if (folder != null &&
          folder.OrderBySequence &&
          ViewData["WhereClause"] == null &&
          string.IsNullOrEmpty(Request["Search"]))
  {%>
<script language="javascript" type="text/javascript">
    $(function () {

        var asc = '<%:folder.OrderSetting.Direction == Kooboo.CMS.Content.Models.OrderDirection.Ascending %>' == 'True',
             table = $("div.table-container table"),
            sortListUrl = '<%=Url.Action("Sort",Request.RequestContext.AllRouteValues()) %>',
            crossPageUrl = '<%=Url.Action("CrossPageSort",Request.RequestContext.AllRouteValues()) %>',
            request = $.request(crossPageUrl),
            hasPrev = false,
            hasNext = false,
            isFirstPage = '<%:isFirstPage %>' == 'True',
            isLastPage = '<%:isLastPage %>' == 'True',
            isUp = false;
        table.tableSorter({
            //dragable: true,
            cancel: 'tr.folderTr',
            beforeUp: function (handle) {
                hasPrev = handle.prev().length > 0;
            },
            up: function (handle) {
                isUp = true;
                if (hasPrev) {
                    sortChange();
                } else {
                    request.queryString["up"] = 'true';
                    sortChange({
                        UUID: handle.find('input:checkbox').val(),
                        Sequence: handle.find('input:hidden[name=Sequence]').val()
                    }, request.getUrl());
                }
            },
            beforeDown: function (handle) {
                hasNext = handle.next().length > 0;
            },
            down: function (handle) {
                isUp = false;
                if (hasNext) {
                    sortChange();
                } else {
                    request.queryString["up"] = 'false';
                    sortChange({
                        UUID: handle.find('input:checkbox').val(),
                        Sequence: handle.find('input:hidden[name=Sequence]').val()
                    }, request.getUrl());
                }

            },
            showUp: function (handle) {
                if (handle.prev().length || isFirstPage) {
                    return false;
                }
            },
            showDown: function (handle) {

                if (handle.next().length || isLastPage) {
                    return false;
                }
            }
        });

        var tbody = table.find('tbody:eq(1)').sortable({
            revert: true,
            handle: 'td:eq(0)',
            cancel: 'tr.folderTr',
            start: function (event, ui) {
                ui.placeholder.html('<td colspan="100"></td>');
            },
            change: function (event, ui) {
            },
            stop: function () {
                sortChange();
            },
            placeholder: "ui-state-highlight holder",
            cursor: 'move',
            helper: 'clone'
        });

        var sortList = getSortlist();

        function sortChange(data, url, dataname) {
            if (data) {
                post(data, url || crossPageUrl, dataname || 'sourceContent', function (response) {
                    if (response.Success && (!hasNext && !isUp || !hasPrev && isUp)) {
                        setTimeout(function () {
                            document.location.reload(true);
                        }, 400);
                    }
                });
            } else {
                var oldList = getSortlist();
                initSort(sortList);
                var newList = getSortlist();
                var changedList = getChangedList(oldList, newList);
                post(changedList, url || sortListUrl, 'list');
            }
            $('tr').removeClass('even').filter(':odd').addClass('even');
        }

        function getSortlist() {
            var list = [];
            tbody.find('tr').each(function () {
                var handle = $(this),
                    sequenceInput = handle.find('input[name=Sequence]'),
                    uuidInput = handle.find('input:checkbox.select.docs'),
                     sequence = sequenceInput.val() || 0,
                     uuid = uuidInput.val();
                list.push({
                    Sequence: sequence,
                    UUID: uuid
                });
            });
            return list;
        }

        function initSort(sortList) {
            sortList = sortList || getSortlist();
            if (asc) {
                sortList = sortList.sort(function (a, b) { return a.Sequence - b.Sequence; });
            } else {
                sortList = sortList.sort(function (a, b) { return b.Sequence - a.Sequence; });
            }
            tbody.find('tr').each(function (index) {
                var tr = $(this);
                tr.find('input[name=Sequence]').val(sortList[index].Sequence);
            });
        }

        function getChangedList(oldList, newList) {
            if (!(oldList && newList)) {
                return [];
            }
            var list = newList.where(function (o) {
                return oldList.where(function (x) {
                    return x.UUID == o.UUID;
                }).first().Sequence != o.Sequence;
            });
            return list;
        }

        function post(changedList, url, dataName, callBack) {
            var tempForm = kooboo.cms.ui.formHelper.tempForm(changedList, url, dataName);
            tempForm.ajaxSubmit({
                success: function (response) {
                    kooboo.cms.ui.messageBox().showResponse(response);
                    if (typeof callBack == 'function') {
                        callBack(response);
                    }
                },
                error: function () {

                }
            });

        }
    });
</script>
<style type="text/css">
    .holder
    {
        width: 100%;
        height: 30px;
    }
</style>
<% }
  else
  { %>
<script language="javascript" type="text/javascript">
    $(function () {
        $('td.draggable,td.undraggable,th.draggable').removeClass('draggable undraggable');
    });
</script>
<%} %>
<%-- Order End --%>