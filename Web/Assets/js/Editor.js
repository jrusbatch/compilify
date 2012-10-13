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
        function Editor(name, textarea) {
            var self = this;
            _instances.push(this);
            var codeMirror = CodeMirror.fromTextArea(textarea, editorDefaults);
            codeMirror.setOption('onChange', function (instance, changeList) {
                $(self).triggerHandler('change', [
                    self, 
                    changeList
                ]);
            });
            this._codeMirror = codeMirror;
        }
        Editor.prototype.destroy = function () {
            $(this._codeMirror.getRootElement()).remove();
            _instances.splice(_instances.indexOf(this), 1);
        };
        return Editor;
    })();
    Compilify.Editor = Editor;    
    function _handleTabKey(instance) {
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

