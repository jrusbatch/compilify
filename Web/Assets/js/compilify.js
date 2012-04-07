
(function(Compilify) {
    var root = this,
        $ = root.jQuery,
        _ = root._,
        CodeMirror = root.CodeMirror;
    
    var validate = _.debounce(function(sender, key) {
        $.ajax('/validate', {
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ code: sender.getValue() }),
            success: function (msg) {
                var $list = $('#errors ul').detach().empty(),
                    hasErrors = msg.data.length > 0;

                if (!hasErrors) {
                    $list.append('<li>Build completed successfully.</li>');
                }
                else {
                    for (var i in msg.data) {
                        var error = msg.data[i];
                                
                        $list.append('<li>' + error.Message + '</li>');
                    }
                }
                
                $('#errors').append($list);
            }
        });
    }, 500);

    $(function() {
        var editor = $('#define .js-editor')[0];

        // Set up editor
        Compilify.Editor = CodeMirror.fromTextArea(editor, {
            indentUnit: 4,
            lineNumbers: true,
            theme: 'neat',
            mode: 'text/x-csharp',
            onChange: validate
        });

        originalContent = Compilify.Editor.getValue().trim().toUpperCase();
            
        $('#define .js-save').on('click', function() {
            $.ajax(window.location.pathname, {
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ content: { Code: Compilify.Editor.getValue() } }),
                success: function(msg) {
                    window.history.pushState({ }, '', msg.data.url);
                }
            });
            return false;
        });
    });
}).call(window, window.Compilify || {});

