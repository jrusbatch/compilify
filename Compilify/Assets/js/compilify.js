
(function($, CodeMirror, undefined) {
    var root = this;

    var textarea = document.getElementById('editor');

    root.Editor = CodeMirror.fromTextArea(textarea, {
        indentUnit: 4,
        lineNumbers: true
    });
}).call(window, jQuery, CodeMirror);
