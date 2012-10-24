(function($) {
    'use strict';

    var $columns, $left, $right;
    
    function _onResize() {
        var remainingSpace = $columns.width() - $left.outerWidth();
        var divTwoWidth = remainingSpace - ($right.outerWidth() - $right.width());
        $right.css('width', divTwoWidth + 'px');
    }

    $(function() {
        $columns = $('.columns');
        $left = $columns.find('.left');
        $right = $columns.find('.right');

        $left.resizable({
            containment: $columns,
            handles: 'e',
            minWidth: '550',
            resize: _onResize
        });

        $(window).resize(_onResize);

        _onResize();
    });
}).call(window, jQuery);