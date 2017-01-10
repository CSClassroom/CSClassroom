function getTableHtml(tableInfo, hasParent) {
    var hasChildren = !!tableInfo.childTableInfo;
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

function createFlatTable(parentRow, tableInfo) {
    if (parentRow instanceof jQuery) {
        var newFlatTable = $(getTableHtml(tableInfo, false /*hasParent*/));
        parentRow.append(newFlatTable);
        return newFlatTable;
    } else {
        parentRow.child(getTableHtml(tableInfo, true /*hasParent*/)).show();
        return $(parentRow.node()).next().find('table');
    }
}

function createNestedTable(parentRow, tableInfo, jsonArray, emptyTableLanguage, order) {
    if (!emptyTableLanguage)
        emptyTableLanguage = "No data available in table";

    if (!order)
        order = [];

    var columns = tableInfo.columns;
    var flatTable = createFlatTable(parentRow, tableInfo);

    var columnInput = [];

    if (!!tableInfo.childTableInfo) {
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

    if (!!tableInfo.childTableInfo) {
        var flatTableBody = flatTable.find('tbody');
        flatTableBody.children('tr').children('td.details-control').each(function (iteration) {
            var tb = flatTableBody;
            var tr = $(this).closest('tr');
            var row = dataTable.row(tr);

            if (row.data().childTableData) {
                $(this).on('click', function () {
                    if (row.child.isShown()) {
                        // This row is already open - close it
                        tr.next().children().first().remove();
                        row.child.hide();
                        tr.removeClass('shown');
                    }
                    else {
                        // Open this row
                        createNestedTable(row, tableInfo.childTableInfo, row.data().childTableData);
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