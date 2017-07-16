function getTableHtml(tableInfo, hasParent, hasChildren) {
    var columns = tableInfo.columns;

    var tableHtml = "";

    tableHtml += '<table class="display" align="left" cellpadding="5" cellspacing="0" border="2" width="auto">';

    if (tableInfo.showHeader)
        tableHtml += '<thead>';
    else
        tableHtml += '<thead style="display: none">';

    tableHtml += '<tr>';

    if (hasChildren) {
        tableHtml += '<th style="min-width: 20px, max-width: 20px"></th>';
    }

    for (var columnIndex = 0; columnIndex < columns.length; columnIndex++) {
        tableHtml += '<th>' + columns[columnIndex].text + '</th>';
    }

    tableHtml += "</tr>"
    tableHtml += '		</thead>';

    tableHtml += '</table>';

    return tableHtml;
}

function createFlatTable(parentRow, tableInfo, hasChildren) {
    if (parentRow instanceof jQuery) {
        var newFlatTable = $(getTableHtml(tableInfo, false /*hasParent*/, hasChildren));
        parentRow.append(newFlatTable);
        return newFlatTable;
    } else {
        parentRow.child(getTableHtml(tableInfo, true /*hasParent*/, hasChildren)).show();
        return $(parentRow.node()).next().find('table');
    }
}

function getChildTableInfo(childTableInfos, type) {
    if (childTableInfos.length === 1) {
        return childTableInfos[0];
    } else {
        for (var index = 0; index < childTableInfos.length; index++) {
            var actualType = type.substr(0, type.indexOf(","));
            if (childTableInfos[index].type === actualType) {
                return childTableInfos[index];
            }
        }

        throw new Error("Invalid child table type.");
    }
}

function createNestedTable(parentRow, tableInfo, jsonArray, emptyTableLanguage, order) {
    if (!emptyTableLanguage)
        emptyTableLanguage = "No data available in table";

    if (!order)
        order = [];

    var columns = tableInfo.columns;

    var hasChildren = false;
    if (!!tableInfo.childTableInfos) {
        for (var index = 0; index < jsonArray.length; index++) {
            var childTableData = jsonArray[index].childTableData;
            if (childTableData && childTableData.length > 0) {
                hasChildren = true;
            }
        }
    }

    var columnInput = [];

    if (hasChildren) {
        columnInput.push({
            "className": 'details-control',
            "orderable": false,
            "data": null,
            "defaultContent": ''
        });
    }

    for (var columnIndex = 0; columnIndex < columns.length; columnIndex++) {
        columnInput.push({ data: columns[columnIndex].name });
    }

    var flatTable = createFlatTable(parentRow, tableInfo, hasChildren);

    var dataTable = flatTable.DataTable({
        "data": jsonArray,
        "columns": columnInput,
        "order": [[1, 'asc']],
        "bFilter": false,
        "bInfo": false,
        "bPaginate": false,
        "bAutoWidth": true,
        "order": order,
        "language": {
            "emptyTable": emptyTableLanguage
        }
    });

    if (!!tableInfo.childTableInfos) {
        var flatTableBody = flatTable.find('tbody');
        flatTableBody.children('tr').children('td.details-control').each(function (iteration) {
            var tb = flatTableBody;
            var tr = $(this).closest('tr');
            var row = dataTable.row(tr);

            if (row.data().childTableData && row.data().childTableData.length > 0) {
                $(this).on('click', function () {
                    if (row.child.isShown()) {
                        // This row is already open - close it
                        tr.next().children().first().remove();
                        row.child.hide();
                        tr.removeClass('shown');
                    }
                    else {
                        // Open this row
                        var childTableInfo = getChildTableInfo(tableInfo.childTableInfos, row.data().childTableData[0]["$type"]);
                        createNestedTable(row, childTableInfo, row.data().childTableData);
                        tr.next().prepend($('<td></td>'));
                        tr.next().children().last().attr("colspan", 3);
                        tr.addClass('shown');
                    }
                });
            }
            else {
                tr.children().first().attr("class", "");
            }
        });
    }
}