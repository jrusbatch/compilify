/// <reference path="app.ts" />
/// <reference path="DocumentManager.ts" />
/// <reference path="TemplateManager.ts" />

module Compilify.DocumentTabsView {
    function _onDocumentAdded(document: IDocumentState) {
        var template = TemplateManager.getTemplateById('#tab-template');
        var $newTab = $(template(document));
        $('#document-tabs').append($newTab);
    }

    function _onDocumentListAdded(documents: IDocumentState[]) {
        for (var i = 0, len = documents.length; i < len; i++) {
            _onDocumentAdded(documents[i]);
        }
    }

    $(DocumentManager).on('documentAdded', function(event: JQueryEventObject, document: IDocumentState) {
        _onDocumentAdded(document);
    });

    $(DocumentManager).on('documentListAdded', function(event: JQueryEventObject, documents: IDocumentState[]) {
        console.log('documentListAdded!', documents);
        _onDocumentListAdded(documents);
    });
}