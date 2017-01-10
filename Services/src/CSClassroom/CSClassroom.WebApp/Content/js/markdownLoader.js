// Source: https://github.com/ppham27/phillypham/blob/master/public/javascripts/markdownLoader.js
// Commit: e2c230f1ac87229dccd14dde6a347d24d47bd526

var markdown = require('./markdown');
var MarkdownAceEditor = require('./markdownAceEditor');
var hljs = require('highlight.js');

function createMarkdownEditor(editorName) {
    var div = document.getElementById('wmd-editor-' + editorName);
    var editor = new MarkdownAceEditor(markdown.Converter, "-" + editorName, { } /*options*/);

    var highlightCodeBlocks = function (editor) {
        var codeBlocks = Array.prototype.slice.call(editor.panels.preview.querySelectorAll('pre code'));
        codeBlocks.forEach(function (code) {
            hljs.highlightBlock(code);
        });
        return editor;
    }

    editor.hooks.chain('onPreviewRefresh', highlightCodeBlocks);

    $(function () {
        highlightCodeBlocks(editor);
    });

    editor.run();
}

function createMarkdownViewer(viewerName, initialContents) {
    var div = document.getElementById('wmd-viewer-' + viewerName);
    div.innerHTML = markdown.Converter.makeHtml(initialContents);

    var codeBlocks = Array.prototype.slice.call(div.querySelectorAll('pre code'));
    codeBlocks.forEach(function (code) {
        hljs.highlightBlock(code);
    });
}

window.createMarkdownEditor = createMarkdownEditor;
window.createMarkdownViewer = createMarkdownViewer;