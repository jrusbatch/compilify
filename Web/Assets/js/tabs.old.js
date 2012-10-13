(function($) {

    var count = 0;

    var dropdownHtml =
        '<span class="dropdown">' +
            '<a class="dropdown-toggle" data-toggle="dropdown">' +
                '<i class="icon-chevron-down"></i>' +
            '</a>' +
            '<ul class="dropdown-menu">' +
                '<li>' +
                    '<a class="js-rename-tab" href="#">' +
                        '<i class="icon-pencil"></i>' +
                        'Rename' +
                    '</a>' +    
                '</li>' +
                '<li class="divider"></li>' +
                '<li>' +
                    '<a class="js-delete-tab" href="#">' +
                        '<i class="icon-trash"></i>' +
                        'Delete' +  
                    '</a>' +
                '</li>' +
            '</ul>' +
        '</span>';
    
    function _deleteTab() {
        var $tab = $(this).parents('.tab'),
            $tabItem = $tab.parent('li'),
            targetSelector = $tab.data('target'),
            $nextTabItem = $tabItem.next('li');

        if ($nextTabItem.length === 0) {
            $nextTabItem = $tabItem.prev('li');
            // There is no next tab, so look for a previous tab that isn't the 'new-tab' button
            if ($nextTabItem.length === 0 || $nextTabItem.has('.new-tab').length > 0) {
                return;
            }
        }

        if ($tabItem.hasClass('active')) {
            var $nextTab = $nextTabItem.children('.tab'),
                nextTargetSelector = $nextTab.data('target');

            $nextTabItem.add(nextTargetSelector).addClass('active');
        }

        $tabItem.add(targetSelector).remove();
    }
    
    function _renameTab() {
        var $tab = $(this).parents('.tab'),
            $tabName = $tab.children('.tab-name');

        var documentName = prompt('Enter a unique name for this tab', $tabName.text());

        $tabName.text(documentName);
    }
    
    function _createTab() {
        var $nav = $(this).parents('.nav').detach();

        $nav.children('li').removeClass('active');

        count += 1;
        var id = 'Untitled-' + count;

        var $tab = $('<li class="active">' +
                            '<span class="tab" data-toggle="tab" data-target="#' + id + '">' +
                                '<span class="tab-name">' + id + '</span>' +
                                dropdownHtml + 
                            '</span>' +
                        '</li>');

        $tab.insertAfter($(this).parent('li'));

        var $tabPane = $('<div id="' + id + '" class="tab-pane active"><textarea></textarea></div>'),
            $tabContainer = $('.tab-content').parent(),
            $tabContent = $('.tab-content').detach();
            
        $tabContent.find('.tab-pane').removeClass('active');
        $tabContent.prepend($tabPane);

        $tabContent.prependTo($tabContainer);

        var editor = CodeMirror.fromTextArea($tabPane.children('textarea')[0], editorOptions);

        $('#ide').prepend($nav);

        $tab.tab();

        $tab.on('show', function() {
            editor.focus();
        });
    }

    $(function() {
        $('.new-tab').on('click', _createTab);

        $('.nav')
            .on('click', '.js-delete-tab', _deleteTab)
            .on('click', '.js-rename-tab', _renameTab);
    });

}).call(window, jQuery);