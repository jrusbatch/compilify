/// <reference path="app.ts" />
/// <reference path="DocumentManager.ts" />
/// <reference path="TemplateManager.ts" />

module Compilify.DocumentTabsView {
    function _findTabByDocument(document: IDocumentState): JQuery {
        return $('#document-tabs').find('[data-target="' + document.Name + '"]').parent('li');
    }

    function _onDocumentAdded(document: IDocumentState) {
        var template = TemplateManager.getTemplateById('#tab-template');
        var $newTab = $(template(document));

        $newTab.on('click', function() {
            DocumentManager.setCurrentDocument(document);
        });

        $('#document-tabs').append($newTab);
    }

    function _onDocumentListAdded(documents: IDocumentState[]) {
        for (var i = 0, len = documents.length; i < len; i++) {
            _onDocumentAdded(documents[i]);
        }
    }

    function _onCurrentDocumentChange() {
        var currentDocument = DocumentManager.getCurrentDocument();
        $('#document-tabs').find('li').removeClass('active');
        _findTabByDocument(currentDocument).addClass('active');
    }

    $(DocumentManager).on('documentAdded', function(event: JQueryEventObject, document: IDocumentState) {
        _onDocumentAdded(document);
    });

    $(DocumentManager).on('documentListAdded', function(event: JQueryEventObject, documents: IDocumentState[] = []) {
        _onDocumentListAdded(documents);
    });

    $(DocumentManager).on('currentDocumentChange', _onCurrentDocumentChange);
}