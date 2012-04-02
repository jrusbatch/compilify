(function(Compilify) {
    var $ = this.jQuery;

    var isArray = Array.isArray || function(obj) {
        return toString.call(obj) == '[object Array]';
    };

    // Log all AJAX requests to Google Analytics
    $.ajaxSend(function(event, jqXhr, options) {
        var queue = _gaq;
        if (isArray(queue) && queue !== null) {
            queue.push(['_trackPageview', options.url]);
        }
    });

    this.Compilify = Compilify;
}).call(window, window.Compilify || {});
