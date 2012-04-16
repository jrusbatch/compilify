
if (typeof String.prototype.trim !== 'function') {
    String.prototype.trim = function() {
        return this.replace(/^\s+|\s+$/g, '');
    };
}

(function($, _, Compilify) {
    var root = this,
        connection;
    
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
        var pathname = window.location.pathname;

        var command = Compilify.Prompt.getValue().trim();
        var classes = Compilify.Editor.getValue().trim();

        trackEvent('Code', 'Save', pathname);

        return $.ajax(pathname, {
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({ post: { 'Content': command, 'Classes': classes } })
                })
                .done(function(msg) {
                    root.history.pushState({ }, '', msg.data.url);
                });;
    }

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

                if (_.isArray(data)) {
                    var $list = $('.messages ul').detach().empty();

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
        var editor = $('#define .editor textarea')[0],
            prompt = $('#execute .editor textarea')[0];

        var validateEnvironment = _.debounce(function() {
            var classes = Compilify.Editor.getValue();
            var command = Compilify.Prompt.getValue();

            validate(command, classes);
        }, 500);
        
        root.CodeMirror.commands.save = save;

        var opts = {
            indentUnit: 4,
            lineNumbers: true,
            theme: 'neat',
            mode: 'text/x-csharp',
            onChange: function() {
                validateEnvironment();
            }
        };
        
        Compilify.Editor = root.CodeMirror.fromTextArea(editor, opts);
        Compilify.Editor.save = save;

        Compilify.Prompt = root.CodeMirror.fromTextArea(prompt, opts);
        Compilify.Prompt.save = save;
        
        $('#define .js-save').on('click', save);

        $('#define .js-execute').on('click', function() {
            var command = Compilify.Prompt.getValue().trim();
            var classes = Compilify.Editor.getValue().trim();
            execute(command, classes);
            return false;
        });
    });
}).call(window, window.jQuery, window._, window.Compilify || {});
