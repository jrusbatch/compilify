
if (typeof String.prototype.trim !== 'function') {
    String.prototype.trim = function() {
        return this.replace(/^\s+|\s+$/g, '');
    };
}

(function($, _, Compilify) {
    var root = this,
        original, connection;
    
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
    
    function save(code) {
        /// <summary>
        /// Save content.</summary>
        
        trackEvent('Code', 'Save', window.location.pathname);

        return $.ajax(window.location.pathname, {
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ post: { Content: code } })
        });
    }

    var validate = function(code) {
        /// <summary>
        /// Sends code to the server for validation and displays the resulting
        /// errors, if any.</summary>

        trackEvent('Code', 'Validate', window.location.pathname);

        return $.ajax('/validate', {
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ 'code': code }),
            success: function(msg) {
                var data = msg.data;

                if (_.isArray(data)) {
                    var $list = $('#define .messages ul').detach().empty();

                    if (data.length == 0) {
                        $list.append('<li class="message success">Build ' +
                            'completed successfully.</li>');
                    } else {
                        for (var i in msg.data) {
                            var error = msg.data[i];
                            $list.append('<li class="message error">' +
                                htmlEscape(error) + '</li>');
                        }
                    }

                    $('#define .messages').append($list);
                }
            }
        });
    };

    function execute(code) {
        /// <summary>
        /// Queues code for execution on the server.</summary>
        
        if (_.isString(code) && code.length > 0) {
            trackEvent('Code', 'Execute', window.location.pathname);
            connection.send(code);
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
        connection = $.connection('/execute');
        
        connection.received(function(msg) {
            if (msg && msg.status === "ok") {
                var data = msg.data;
                if (data && data.result) {
                    var result = htmlEscape(data.result.toString());
                    setResult(result);
                }
            }
        });

        connection.error(function(e) {
            console.error(e);
        });

        connection.start();
        
        //
        // Set up CodeMirror editor
        //
        
        // Get the editor and save the current content so we can tell when it 
        // changes
        var editor = $('#define .editor textarea')[0];
        original = editor.innerHTML.trim().toUpperCase();
        
        Compilify.Editor = root.CodeMirror.fromTextArea(editor, {
            indentUnit: 4,
            lineNumbers: true,
            theme: 'neat',
            mode: 'text/x-csharp',
            onChange: _.debounce(function (sender) {
                var code = sender.getValue().trim();
                
                // throttled to once every 500ms max
                validate(code);
            }, 500)
        });

        Compilify.Editor.save = _.bind(function() {
            var code = this.getValue().trim();

            // Only save the content if it changed since we last loaded it.
            save(code)
                .done(function(msg) {
                    // Might be able to use the object stored by pushState 
                    // hold the original value
                    root.history.pushState({ }, '', msg.data.url);
                });

            return false;
        }, Compilify.Editor);
        
        root.CodeMirror.commands["save"] = Compilify.Editor.save;
        
        $('#define .js-save').on('click', Compilify.Editor.save);

        $('#define .js-execute').on('click', function() {
            var code = Compilify.Editor.getValue().trim();
            execute(code);
            return false;
        });
    });
}).call(window, window.jQuery, window._, window.Compilify || {});
