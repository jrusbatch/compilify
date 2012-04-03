(function(Compilify, undefined) {
    var root = this,
        $ = root.jQuery;

    // Log all AJAX requests to Google Analytics
    $(function() {
        $('#content').ajaxSend(function(event, jqXhr, options) {
            var gaq = root._gaq;
            if (gaq) {
                gaq.push(['_trackPageview', options.url]);
            }
        });
    });
    
    root.Compilify = Compilify;
}).call(window, window.Compilify || {});
