
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
    
    function save() {
        /// <summary>
        /// Save content.</summary>
        
        // $('form').submit();
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

                            var editor = Compilify.GetEditorByName(file);
                            
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

    (function () {
        var validateEnvironment = _.throttle(function () {
            var documents = [],
                editors = Compilify.Editors || [];
            
            for (var i = 0, len = editors.length; i < len; i++) {
                var editor = editors[i];
                documents.push({ Name: editor.name, Text: editor.getValue() });
            }

            console.log(documents);

            validate(documents);
        }, 250);

        var defaults = {
            indentUnit: 4,
            lineNumbers: true,
            theme: 'neat',
            mode: 'text/x-csharp',
            onChange: function (cm, changes) {
                validateEnvironment();
            }
        };

        function Editor(name, textarea) {
            this.name = name;
            this._codeMirror = CodeMirror.fromTextArea(textarea, defaults);
            this.markedErrors = [];

            this.getValue = this._codeMirror.getValue;
        }

        Compilify.Editor = Editor;
        Compilify.GetEditorByName = function(name) {
            for (var i = 0, len = Compilify.Editors.length; i < len; i++) {
                var editor = Compilify.Editors[i];
                if (editor.name == name) {
                    return editor;
                }
            }

            return null;
        };
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

        connection.start({ transport: 'longPolling' });
        
        //
        // Set up CodeMirror editor
        //

        Compilify.Editors = [];
        $('#editors textarea').each(function () {
            var $this = $(this);
            var editor = new Compilify.Editor($this.data('name'), this);
            Compilify.Editors.push(editor);
        });
        
        $('.js-run').on('click', function() {
            
            var documents = [],
                editors = Compilify.Editors || [];

            for (var i = 0, len = editors.length; i < len; i++) {
                var editor = editors[i];
                documents.push({ Name: editor.name, Text: editor.getValue() });
            }

            execute({ Documents: documents });

            $('.results pre').html('');
            
            return false;
        });
        
        //
        // Set up key binds
        //

        shortcut.add("Ctrl+B",function() {
            $(".js-run").click();
        });
        
        shortcut.add("Ctrl+S", save);

    });
}).call(window, window.jQuery, window._, window.Compilify || {});
