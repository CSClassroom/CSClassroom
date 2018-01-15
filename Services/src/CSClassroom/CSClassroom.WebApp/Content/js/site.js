/**
 * Creates a code editor control.
 * @param editorId The ID of the DOM element wrapping the editor. This must be unique.
 * @param textArea The text area to keep in sync with the editor, if any.
 * @param minLines The minimum number of lines to show in the editor.
 * @param maxLines The maximum number of lines to show in the editor (beyond which scrolling will occur).
 */
function createCodeEditor(editorId, textArea, minLines, maxLines) {
    var editor = ace.edit(editorId);
    editor.setTheme("ace/theme/eclipse");
    editor.setShowPrintMargin(false);
    editor.setHighlightSelectedWord(true);
    editor.session.setMode("ace/mode/java");
    editor.session.setFoldStyle("manual");
    editor.session.setNewLineMode("windows");
    editor.setOptions({
        minLines: minLines,
        maxLines: maxLines
    });

    if (textArea) {
        var syncTextArea = function() {
            textArea.val(editor.getSession().getValue());
        };

        syncTextArea();

        editor.getSession().on("change", function() {
            syncTextArea();
        });
    }
}

/**
 * Rewrites form field names in grids to conform with MVC model binding of collections.
 */
function rewriteGridCollectionNames()
{
    $("input[id$='rowOrder']").each(function (index) {
        var tableName = this.id.substring(0, this.id.indexOf("_")); 
        var rowOrder = this;
        var form = this.form;
            
        $(form).validate({
            onsubmit: false
        });

        $(form).submit(function (e) {
            if (!$(form).valid())
            {
                return false;
            }

            $(form).find(":input").each(function (index) {
                var tableNameIndex = this.id.indexOf(tableName);
                var startIndex = this.name.indexOf("[");
                var endIndex = this.name.indexOf("]");
                if (tableNameIndex >= 0 && startIndex >= 0 && endIndex >= 0) {
                    var uniqueIndex = this.name.substring(startIndex + 1, endIndex);
                    var order = $(rowOrder).val().split(',').indexOf(uniqueIndex);
                    this.name = this.name.substring(0, startIndex + 1)
                        + order
                        + this.name.substring(endIndex);
                }
            });

            return true;
        });
    });
}

/**
 * Keeps the authenticated session alive, by making repeated authenticated requests.
 * @param ensureAuthenticatedUrl The URL to call.
 */
function keepAuthenticatedSessionAlive(ensureAuthenticatedUrl)
{
    var renewalDelayMs = 120 * 60 * 1000; // 120 minutes
    var maxRenewals = 12; // 24 hours
    var renewalCount = 0;
    var sessionExpiredError = "Session expired. Please copy your submission and refresh the page before making another submission.";
    var renewToken;

    renewToken = function() {
        $.ajax({ type: "GET", url: "/EnsureAuthenticated" })
            .done(function() {
                renewalCount++;
                if (renewalCount < maxRenewals) {
                    setTimeout(renewToken, renewalDelayMs);
                } else {
                    alert(sessionExpiredError);
                }
            }).fail(function() {
                alert(sessionExpiredError);
            });
        };

    setTimeout(renewToken, renewalDelayMs);
}

function delayPromise(delayMs) {
    return new Promise((resolve, reject) => {
        setTimeout(() => resolve(), delayMs)
    });
}

function prolongPromise(promise, delayMs = 1000) {
    return Promise.all([promise, delayPromise(delayMs)])
        .then((results) => {
            return results[0];
        })
}

function alertDialogInternal(title, width, height) {
    return $("<div></div>").dialog( {
        buttons: { "Ok": function () { $(this).dialog("close"); } },
        close: function (event, ui) { $(this).remove(); },
        resizable: false,
        title: title,
        width: width,
        height: height, 
        modal: true
    });
}

function alertDialogHtml(title, htmlMessage, width = 600, height = undefined) {
    alertDialogInternal(title, width, height).html(htmlMessage);
}

function alertDialogText(title, textMessage, width = 600, height = undefined) {
    alertDialogInternal(title, width, height).text(textMessage);
}

function confirmDialog(title, message, callback, width = 600, height = undefined) {
    $("<div></div>").dialog( {
        buttons: { 
            Ok: function () { $(this).dialog("close"); callback(true); }, 
            Cancel: function () { $(this).dialog("close"); callback(false); }
        },
        close: function (event, ui) { $(this).remove(); },
        resizable: false,
        title: title,
        width: width,
        height: height,
        modal: true
    }).text(message);
}