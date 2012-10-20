(function($, Compilify) {
    'use strict';
    
    if (!Compilify) {
        throw new Error('Compilify has not yet been declared. Check the order in which the scripts were loaded.');
    }

    var $container,
        $tabTemplate;
    
    function _getTabForDocument(document) {
        if (!document) {
            return null;
        }
        
        return $('.tab[data-documentId="' + document.getId() + '"]');
    }
    
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

    function _onDocumentAdded(doc) {
        var $tab = $tabTemplate({ id: doc.getId(), name: doc.getName() });

        $tab.on('click', function() {
            Compilify.Workspace.setActiveDocument(doc);
        });

        $container.find('.nav-tabs').prepend($tab);
    }
    
    function _onProjectLoaded() {
        var documents = Compilify.Workspace.getDocuments();
        documents.forEach(_onDocumentAdded);
    }
    
    function _onActiveDocumentChanged(active, previous) {
        $container.find('.nav li.active').removeClass('active');
        
        var $tab = _getTabForDocument(active);
        $tab.parent('li').addClass('active');
    }

    $(Compilify)
        .on('projectLoaded', function($event) { _onProjectLoaded(); })
        .on('documentAdded', function($event, document) { _onDocumentAdded(document); })
        .on('activeDocumentChanged', function($event, current, previous) { _onActiveDocumentChanged(current, previous); })
        .on('documentRenamed', function($event, document) { throw new Error('Not implemented'); })
        .on('documentRemoved', function($event) { throw new Error('Not implemented'); });
    
    $(function() {
        var template = doT.template($('#tab-template').html());

        $tabTemplate = function(obj) {
            return $(template(obj));
        };

        $container = $('#top-bar');

        $container
            .on('click', '#new-document', function() { Compilify.Workspace.addDocument(); })
            .on('click', '.js-delete-tab', _deleteTab)
            .on('click', '.js-rename-tab', _renameTab);
    });

}).call(window, jQuery, Compilify);