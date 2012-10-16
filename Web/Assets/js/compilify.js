
(function() {
    if (typeof String.prototype.trim !== 'function') {
        String.prototype.trim = function() {
            return this.replace(/^\s+|\s+$/g, '');
        };
    }
})(window);

(function($, _, Compilify) {
    var root = this,
        connection;

    // window.Compilify = Compilify;
    
    function getAllDocuments() {
        var documents = [],
            editors = Compilify.Editors || [];

        for (var i = 0, len = editors.length; i < len; i++) {
            var editor = editors[i];
            documents.push({ Name: editor.name, Text: editor.getValue() });
        }

        return documents;
    }

    function save() {
        /// <summary>
        /// Save content.</summary>
        
        // $('form').submit();

        var documents = getAllDocuments();

        var post = {
            Documents: documents
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

    var markedErrors = [];
    

    function validate(documents) {
        /// <summary>
        /// Sends code to the server for validation and displays the resulting
        /// errors, if any.</summary>
        
        return $.ajax('/validate', {
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({ Documents: documents }),
            dataType: 'json',
            success: function(msg) {
                var data = msg.data;
                
                for (var i = markedErrors.length - 1; i >= 0; i--) {
                    markedErrors[i].clear();
                }

                markedErrors.length = 0;

                if (_.isArray(data)) {
                    var $list = $('#footer .status ul.messages').detach().empty();

                    if (data.length == 0) {
                        $('#footer .status').removeClass('status-error').addClass('status-success');
                        $list.html("<li>No errors!</li>");
                        
                    } else {
                        $('#footer .status').removeClass('status-success').addClass('status-error');
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
                            
                            var message = 'Line: ' + (start.Line + 1) +
                                          ' Column: ' + start.Character + ' - ' + error.Message;

                            $list.append('<li data-errorId="' + index + '">' + _.escape(message) + '</li>');
                        }
                    }

                    $('#footer .status').append($list);
                }
            }
        });
    }

    function execute(documents) {
        /// <summary>
        /// Queues code for execution on the server.</summary>
        
        //if (_.isString(command) && command.length > 0) {
            // connection.send(JSON.stringify({ 'Content': command, 'Classes': classes }));
            connection.send(JSON.stringify(documents));
            $('#footer:not(.loading)').addClass('loading');
        //}
    }
    
    function setResult(data) {
        /// <summary>
        /// Sets the content displayed in the results section.</summary>

        $('#footer .results pre').html(data);
    }

    Compilify.Editor = (function() {
        var instances = [];

        var validateEnvironment = _.throttle(function () {
            var documents = [];
            
            for (var i = 0, len = instances.length; i < len; i++) {
                var editor = instances[i];
                documents.push({ Name: editor.name, Text: editor.getValue() });
            }

            validate(documents);
        }, 250);

        var defaults = {
            indentUnit: 4,
            lineNumbers: true,
            theme: 'neat',
            mode: 'text/x-csharp',
            autofocus: true,
            onChange: function() {
                validateEnvironment();
            },
            keyMap: {
                'Shift-Tab': 'indentLess',
                'Ctrl-B': function() {
                    $('.js-run').click();
                },
                'Ctrl-S': save
            }
        };
        
        function Editor(name, textarea) {
            if (!name) {
                throw new Error('An editor cannot be created without a name');
            }

            this._name = name;
            this._codeMirror = CodeMirror.fromTextArea(textarea, defaults);
            this._markedErrors = [];

            instances.push(this);
        }
        
        Editor.getEditorByName = function(name) {
            name = (name || '').toUpperCase();
            for (var i = 0, len = instances.length; i < len; i++) {
                var editor = instances[i];
                if (editor.getName().toUpperCase() === name) {
                    return editor;
                }
            }

            return null;
        };

        Editor.refreshAll = function() {
            for (var i = 0, len = instances.length; i < len; i++) {
                instances[i]._codeMirror.refresh();
            }
        };

        Editor.prototype.getName = function() {
            return this._name;
        };

        Editor.prototype.getText = function() {
            return this._codeMirror.getText();
        };

        Editor.prototype.focus = function() {
            this._codeMirror.focus();
        };

        Editor.prototype.refresh = function() {
            this._codeMirror.refresh();
        };

        return Editor;
    }());
    

    $(function() {
        //
        // Set up the SignalR connection
        //
        connection = $.connection('/execute');

        connection.received(function (msg) {
            if (msg && msg.status === "ok") {
                setResult(msg.data);
            }
            
            $('#footer').removeClass('loading');
        });

        connection.start();
        
        //
        // Set up CodeMirror editor
        //
        
        $('.tab-content textarea').each(function () {
            var $this = $(this);
            var $parent = $this.parent('.tab-pane');
            var name = $parent.attr('id');
            new Compilify.Editor(name, this);
        });
        
        $('.js-run').on('click', function() {
            
            var documents = [],
                editors = Compilify.Editors || [];

            for (var i = 0, len = editors.length; i < len; i++) {
                var editor = editors[i];
                documents.push({ Name: editor.getName(), Text: editor.getText() });
            }

            execute({ Documents: documents });

            $('.results pre').html('');
            
            return false;
        });

        $('.js-save').on('click', save);
    });
}).call(window, window.jQuery, window._, window.Compilify || (window.Compilify = { }));
