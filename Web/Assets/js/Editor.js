/// <reference path="app.ts" />
/// <reference path="Document.ts" />
/// <reference path="vendor/codemirror.d.ts" />
var Compilify;
(function (Compilify) {
    'use strict';
    var editorDefaults = {
        indentUnit: 4,
        lineNumbers: true,
        theme: 'neat',
        mode: 'text/x-csharp',
        matchBrackets: true,
        dragDrop: false,
        extraKeys: {
            'Tab': _handleTabKey,
            'Shift-Tab': 'indentLess'
        }
    };
    var _instances = [];

    var Editor = (function () {
        function Editor(document, makeMaster, container) {
            var self = this;
            _instances.push(this);
            var codeMirror = new CodeMirror(container, editorDefaults);
            codeMirror.setOption('onChange', function (instance, changeList) {
                Compilify.Events.trigger('change', [
                    self, 
                    changeList
                ]);
            });
            this._codeMirror = codeMirror;
        }
        Editor.prototype.getRootElement = function () {
            return this._codeMirror.getWrapperElement();
        };
        Editor.prototype.setVisible = function (show) {
            $(this._codeMirror.getWrapperElement()).toggle(show);
            this._codeMirror.refresh();
        };
        Editor.prototype.getScrollerElement = function () {
            return this._codeMirror.getScrollerElement();
        };
        Editor.prototype.refresh = function () {
            this._codeMirror.refresh();
        };
        Editor.prototype.dispose = function () {
            // remove the CodeMirror element
            $(this._codeMirror.getWrapperElement()).remove();
            _instances.splice(_instances.indexOf(this), 1);
        };
        return Editor;
    })();
    Compilify.Editor = Editor;    
    function _handleTabKey(instance) {
        // Tab key handling is done as follows:
        // 1. If the selection is before any text and the indentation is to the left of
        //    the proper indentation then indent it to the proper place. Otherwise,
        //    add another tab. In either case, move the insertion point to the
        //    beginning of the text.
        // 2. If the selection is after the first non-space character, and is not an
        //    insertion point, indent the entire line(s).
        // 3. If the selection is after the first non-space character, and is an
        //    insertion point, insert a tab character or the appropriate number
        //    of spaces to pad to the nearest tab boundary.
                var from = instance.getCursor(true);
        var to = instance.getCursor(false);
        var line = instance.getLine(from.line);
        var indentAuto = false;
        var insertTab = false;

        if(from.line === to.line) {
            if(line.search(/\S/) > to.ch || to.ch === 0) {
                indentAuto = true;
            }
        }
        if(indentAuto) {
            var currentLength = line.length;
            CodeMirror.commands.indentAuto(instance);
            // If the amount of whitespace didn't change, insert another tab
            if(instance.getLine(from.line).length === currentLength) {
                insertTab = true;
                to.ch = 0;
            }
        } else {
            if(instance.somethingSelected()) {
                CodeMirror.commands.indentMore(instance);
            } else {
                insertTab = true;
            }
        }
        if(insertTab) {
            if(instance.getOption('indentWithTabs')) {
                CodeMirror.commands.insertTab(instance);
            } else {
                var i;
                var ins = '';
                var numSpaces = 4;

                numSpaces -= to.ch % numSpaces;
                for(i = 0; i < numSpaces; i++) {
                    ins += ' ';
                }
                instance.replaceSelection(ins, 'end');
            }
        }
    }
})(Compilify || (Compilify = {}));

