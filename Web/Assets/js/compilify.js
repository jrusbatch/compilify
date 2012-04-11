
if (typeof String.prototype.trim !== 'function') {
    String.prototype.trim = function() {
        return this.replace(/^\s+|\s+$/g, '');
    };
}

(function(Compilify) {
    var root = this,
        $ = root.jQuery,
        _ = root._,
        CodeMirror = root.CodeMirror,
        isConnected = false,
        original, connection;
    
    function saveContent(value, callback) {
        $.ajax(window.location.pathname, {
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ post: { Content: value } }),
            success: function(msg) {
                root.history.pushState({ }, '', msg.data.url);
                original = value.toUpperCase();
                
                if (_.isFunction(callback)) {
                    callback(msg.data);
                }
            }
        });
    }
    
    function execute(value) {
        if (!isConnected) {
            connection.start();
        }

        connection.send(value);
    }
    
    function htmlEscape(str) {
        return String(str)
                .replace(/&/g, '&amp;')
                .replace(/"/g, '&quot;')
                .replace(/'/g, '&#39;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;');
    }
    
    var validate = _.debounce(function(sender) {
        $.ajax('/validate', {
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ code: sender.getValue() }),
            success: function (msg) {
                var $list = $('#define .messages ul').detach().empty(),
                    hasErrors = msg.data.length > 0;

                if (!hasErrors) {
                    $list.append('<li class="message success">Build completed successfully.</li>');
                }
                else {
                    for (var i in msg.data) {
                        var error = msg.data[i];
                        $list.append('<li class="message error">' + htmlEscape(error) + '</li>');
                    }
                }
                
                $('#define .messages').append($list);
            }
        });
    }, 500);

    $(function() {
        var editor = $('#define .editor textarea')[0];

        original = editor.innerHTML.trim().toUpperCase();

        // Set up editor
        Compilify.Editor = CodeMirror.fromTextArea(editor, {
            indentUnit: 4,
            lineNumbers: true,
            theme: 'neat',
            mode: 'text/x-csharp',
            onChange: validate
        });
        
        $('#define .js-save').on('click', function() {
            var currentValue = Compilify.Editor.getValue().trim();
            
            if (currentValue.toUpperCase() !== original) {
                saveContent(currentValue);
            }

            return false;
        });

        $('#define .js-execute').on('click', function() {
            var currentValue = Compilify.Editor.getValue().trim();

            execute(currentValue);
            
            return false;
        });
        
        connection = $.connection('/execute');
        connection.logging = true;
        
        connection.received(function(message) {
            if (message.status === "ok") {
                if (message.data && !_.isUndefined(message.data.result)) {
                    var result = htmlEscape(message.data.result.toString());
                    $('#results p').html(result);
                }
            }
        });
        
        connection.error(function(e) {
            console.error(e);
        });

        connection.start({ transport: 'auto' });
    });
}).call(window, window.Compilify || {});

