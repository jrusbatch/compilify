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
    
    function _onDocumentAdded(doc) {
        var $tab = $tabTemplate({ id: doc.getId(), name: doc.getName(), isPrimary: doc.isPrimary() });

        // TODO: Clicking on the dropdown toggle should not set make the tab active.
        $tab.on('click', '.js-rename-document', function() { Compilify.Workspace.renameDocument(doc); })
            .on('click', '.js-delete-document', function() { Compilify.Workspace.removeDocument(doc); })
            .on('dblclick', function() { Compilify.Workspace.renameDocument(doc); })
            .on('click', function() { Compilify.Workspace.setActiveDocument(doc); });

        $container.find('.nav-tabs').prepend($tab);
    }
    
    function _onProjectLoaded() {
        var documents = Compilify.Workspace.getDocuments();
        documents.forEach(_onDocumentAdded);
    }
    
    function _onActiveDocumentChanged(active, previous) {
        if (previous) {
            $container.find('.nav li.active').removeClass('active');
        }
        
        var $tab = _getTabForDocument(active);
        $tab.parent('li').addClass('active');
    }
    
    function _onDocumentRenamed(document) {
        _getTabForDocument(document).find('.tab-name').text(document.getName());
    }
    
    function _onDocumentRemoved(document) {
        _getTabForDocument(document).remove();
    }

    $(Compilify)
        .on('projectLoaded', _onProjectLoaded)
        .on('documentAdded', function($event, document) { _onDocumentAdded(document); })
        .on('activeDocumentChanged', function($event, active, previous) { _onActiveDocumentChanged(active, previous); })
        .on('documentRenamed', function($event, document) { _onDocumentRenamed(document); })
        .on('documentRemoved', function($event, document) { _onDocumentRemoved(document); });
    
    $(function() {
        var template = doT.template($('#tab-template').html());

        $tabTemplate = function(obj) {
            return $(template(obj));
        };

        $container = $('#top-bar');

        $container
            .on('click', '#new-document', function() { Compilify.Workspace.addDocument(); });
    });

}).call(window, jQuery, Compilify);