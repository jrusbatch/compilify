(function($) {
    $(function() {

        var $columns = $('.columns'),
            $left = $('.columns .column.left'),
            $right = $('.columns .column.right');

        function onResize() {
            var remainingSpace = $columns.width() - $left.outerWidth();
            var divTwoWidth = remainingSpace - ($right.outerWidth() - $right.width());
            $right.css('width', divTwoWidth + 'px');
        }

        $(window).resize(onResize);

        $('#ide .column.left').resizable({
            containment: $columns,
            handles: 'e',
            minWidth: '550',
            resize: onResize
        });

        $('#ide .column.right .execute').resizable({
            containment: $right,
            handles: 's'
        });

        onResize();
    });
}).call(window, jQuery);