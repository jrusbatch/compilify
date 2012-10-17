
(function() {
    if (typeof String.prototype.trim !== 'function') {
        String.prototype.trim = function() {
            return this.replace(/^\s+|\s+$/g, '');
        };
    }
})(window);

(function($, _, Compilify) {
    'use strict';

    var root = this,
        connection,
        markedErrors = [];
    

    //
    // Set up the SignalR connection
    //
    connection = $.connection('/execute');

    connection.received(function(msg) {
        if (msg && msg.status === "ok") {
            $('#footer .results pre').html(msg.data);
        }

        // $('#footer').removeClass('loading');
    });

    connection.start();
    
    function _getAllDocuments() {
        var editors = Compilify.Editor.getAllEditors();
        
        var documents = [];
        for (var i = 0, len = editors.length; i < len; i++) {
            var editor = editors[i];
            documents.push({ Name: editor.getName(), Text: editor.getText() });
        }

        return documents;
    }
    
    function _save() {
        
        var post = {
            Documents: _getAllDocuments()
        };

        $.ajax({
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(post),
            success: function(msg) {
                console.log(msg);
            }
        });

        return false;
    }

    function _run() {
        connection.send(JSON.stringify({ Documents: _getAllDocuments() }));
        
        $('#footer:not(.loading)').addClass('loading');

        $('.results pre').empty();

        return false;
    }

    function _validate() {
        return $.ajax('/validate', {
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({ Documents: _getAllDocuments() }),
            dataType: 'json',
            success: function(msg) {
                _onValidationCompleted(msg);
                $(Compilify).triggerHandler('validationComplete', msg.data);
            }
        });
    }
    
    function _clearMarkedErrors() {
        for (var i = markedErrors.length - 1; i >= 0; i--) {
            markedErrors[i].clear();
        }

        markedErrors.length = 0;
    }
    
    function _onValidationCompleted(msg) {
        var data = msg.data;
        
        if (!_.isArray(data)) {
            return;
        }

        _clearMarkedErrors();

        // var $list = $('#footer .status ul.messages').detach().empty();

        if (data.length === 0) {
            // $('#footer .status').removeClass('status-error').addClass('status-success');
            // $list.html("<li>No errors!</li>");
            return;
        }

        // $('#footer .status').removeClass('status-success').addClass('status-error');
        for (var index in msg.data) {
            var error = msg.data[index];
            var loc = error.Location;

            var file = loc.DocumentName;

            var start = loc.StartLinePosition;
            var end = loc.EndLinePosition;

            var editor = Compilify.Editor.getEditorByName(file);

            if (editor) {
                var markStart = { line: start.Line, ch: start.Character };
                var markEnd = { line: end.Line, ch: end.Character };

                var mark = editor._codeMirror.markText(markStart, markEnd, 'compilation-error');

                markedErrors.push(mark);
            }

            // var message = 'Line: ' + (start.Line + 1) +
            //              ' Column: ' + start.Character + ' - ' + error.Message;

            // $list.append('<li data-errorId="' + index + '">' + _.escape(message) + '</li>');
        }


        // $('#footer .status').append($list);
    }
    
    Compilify.Editor = (function() {
        var _instances = [];

        var defaults = {
            indentUnit: 4,
            lineNumbers: true,
            theme: 'neat',
            mode: 'text/x-csharp',
            autofocus: true,
            onChange: _validate
        };
        
        function Editor(name, textarea) {
            if (!name) {
                throw new Error('An editor cannot be created without a name');
            }

            this._name = name;
            this._codeMirror = CodeMirror.fromTextArea(textarea, defaults);
            this._markedErrors = [];

            _instances.push(this);
        }
        
        Editor.getEditorByName = function(name) {
            name = (name || '').toUpperCase();
            for (var i = 0, len = _instances.length; i < len; i++) {
                var editor = _instances[i];
                if (editor.getName().toUpperCase() === name) {
                    return editor;
                }
            }

            return null;
        };

        Editor.getAllEditors = function() {
            return _instances.slice();
        };

        Editor.prototype.getName = function() {
            return this._name;
        };

        Editor.prototype.getText = function() {
            return this._codeMirror.getValue();
        };

        Editor.prototype.focus = function() {
            this._codeMirror.focus();
        };

        Editor.prototype.refresh = function() {
            this._codeMirror.refresh();
        };

        Editor.prototype.dispose = function() {
            $(this._codeMirror.getWrapperElement()).remove();
            _instances.splice(_instances.indexOf(this), 1);
        };

        return Editor;
    }());

    $(function() {
        
        //
        // Set up CodeMirror editor
        //
        
        $('.tab-content textarea').each(function () {
            var $this = $(this);
            var $parent = $this.parent('.tab-pane');
            var name = $parent.attr('id');
            new Compilify.Editor(name, this);
        });
        
        $('.js-run').on('click', _run);

        $('.js-save').on('click', _save);
    });
}).call(window, window.jQuery, window._, window.Compilify || (window.Compilify = { }));
