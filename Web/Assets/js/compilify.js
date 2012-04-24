
if (typeof String.prototype.trim !== 'function') {
    String.prototype.trim = function() {
        return this.replace(/^\s+|\s+$/g, '');
    };
}

(function($, _, Compilify) {
    var root = this,
        connection,
    
        EndpointConnection = function(url, options) {
            /// <summary>
            /// Wraps a SignalR PersistentConnection object to close the connection after a set period of time.</summary>
            /// <param name="url" type="String">
            /// The URL of the endpoint to connect to.</param>
            /// <param name="options" type="Object">
            /// The URL of the endpoint to connect to.</param>
            /// <returns type="EndpointConnection" />
            
            var timer, 
                isConnected = false,
                conn = $.connection(url), 
                opts = _.defaults(options, {
                    timeout: 30000,
                    onReceived: $.noop
                });
            
            function onTimeout() {
                conn.stop();
                isConnected = false;
                timer = null;
            }

            conn.sending(function() {
                /// 
                if (timer != null) {
                    root.clearTimeout(timer);
                }
                timer = root.setTimeout(onTimeout, opts.timeout);
            });

            conn.received(opts.onReceived);
            
            return {
                send: function (data) {
                    if (isConnected === true) {
                        conn.send(data);
                    }
                    else {
                        conn.start({ transport: [ 'serverSentEvents', 'foreverFrame', 'longPolling' ] }, function() {
                            isConnected = true;
                            conn.send(data);
                        });
                    }
                }
            };
        };
    
    function htmlEscape(str) {
        /// <summary>
        /// Sanitizes a string for display as HTML content.</summary>
        
        return String(str)
                .replace(/&/g, '&amp;')
                .replace(/"/g, '&quot;')
                .replace(/'/g, '&#39;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;');
    }
    
    function trackEvent(category, action, label) {
        /// <summary>
        /// Tracks an event in Google Analytics if it is initialized.</summary>
        
        var gaq = root._gaq;
        if (gaq && _.isFunction(gaq.push)) {
            gaq.push(['_trackEvent', category, action, label, , false]);
        }
    }
    
    function save() {
        /// <summary>
        /// Save content.</summary>
        
        $('form').submit();
        return false;
    }

    var markedErrors = [];

    function validate(command, classes) {
        /// <summary>
        /// Sends code to the server for validation and displays the resulting
        /// errors, if any.</summary>
        var pathname = window.location.pathname;
        
        trackEvent('Code', 'Validate', pathname);

        return $.ajax('/validate', {
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ 'Command': command, 'Classes': classes }),
            success: function(msg) {
                var data = msg.data;
                
                for (var i = markedErrors.length - 1; i >= 0; i--) {
                    markedErrors[i].clear();
                }

                markedErrors.length = 0;

                if (_.isArray(data)) {
                    var $list = $('.messages ul').detach().empty();

                    if (data.length == 0) {
                        $list.append('<li class="message success">Build ' +
                            'completed successfully.</li>');
                    } else {
                        for (var index in msg.data) {
                            var error = msg.data[index];

                            var start = error.Location.StartLinePosition;
                            var end = error.Location.EndLinePosition;

                            var markStart = { line: start.Line, ch: start.Character };
                            var markEnd = { line: end.Line, ch: end.Character };
                            
                            var mark = Compilify[error.Location.FileName].markText(markStart, markEnd, 'compilation-error');

                            markedErrors.push(mark);

                            var message = 'Line: ' + (start.Line + 1) + ' Column: ' + start.Character + ' - ' + error.Message;

                            $list.append('<li class="message error" data-errorId="' + index + '">' +
                                htmlEscape(message) + '</li>');
                        }
                    }

                    $('.messages').append($list);
                }
            }
        });
    }

    function execute(command, classes) {
        /// <summary>
        /// Queues code for execution on the server.</summary>
        
        if (_.isString(command) && command.length > 0) {
            trackEvent('Code', 'Execute', window.location.pathname);
            connection.send(JSON.stringify({ 'Content': command, 'Classes': classes }));
        }
    }
    
    function setResult(result) {
        /// <summary>
        /// Sets the content displayed in the results section.</summary>
        $('#results pre').html(result);
    }

    $(function() {
        //
        // Set up the SignalR connection
        //
        connection = new EndpointConnection('/execute', {
            onReceived: function(msg) {
                if (msg && msg.status === "ok") {
                    var data = msg.data;
                    if (data && data.result) {
                        var result = htmlEscape(data.result.toString());
                        setResult(result);
                    }
                }
            }
        });
        
        //
        // Set up CodeMirror editor
        //
        
        // Get the editor and save the current content so we can tell when it 
        // changes
        var editor = $('#define .editor textarea')[0],
            prompt = $('#execute .editor textarea')[0];

        var validateEnvironment = _.debounce(function() {
            var classes = Compilify.Editor.getValue();
            var command = Compilify.Prompt.getValue();

            validate(command, classes);
        }, 500);
        
        root.CodeMirror.commands.save = save;

        var editorOptions = {
            indentUnit: 4,
            lineNumbers: true,
            theme: 'neat',
            mode: 'text/x-csharp',
            onChange: function() {
                validateEnvironment();
            }
        };
        
        if (!editor || !prompt) {
            return;
        }
        
        Compilify.Editor = root.CodeMirror.fromTextArea(editor, editorOptions);
        Compilify.Editor.save = save;

        Compilify.Prompt = root.CodeMirror.fromTextArea(prompt, editorOptions);
        Compilify.Prompt.save = save;
        
        $('#content .js-save').on('click', save);

        $('#content .js-run').on('click', function() {
            var command = Compilify.Prompt.getValue().trim();
            var classes = Compilify.Editor.getValue().trim();
            
            execute(command, classes);
            
            return false;
        });
        
        //
        // Set up key binds
        //

        shortcut.add("Ctrl+B",function() {
            $("#define .js-run").click();
        });
        
        shortcut.add("Ctrl+S", save);

    });
}).call(window, window.jQuery, window._, window.Compilify || {});
