/// <reference path="app.ts" />
/// <reference path="ProjectManager.ts" />

module Compilify.DocumentManager {
    var _currentDocument: IDocumentState;

    function _addDocument(document: IDocumentState, suppressTrigger?: bool = false) {
        // Do some spectacular here
        
        if (!suppressTrigger) {
            $(DocumentManager).triggerHandler('documentAdded', document);
            setCurrentDocument(document);
        }
    }

    function _addDocumentList(documents: IDocumentState[]): void {
        for (var i = 0, len = documents.length; i < len; i++) {
            _addDocument(documents[i], true);
        }

        $(DocumentManager).triggerHandler('documentListAdded', [documents]);
        setCurrentDocument(documents[0]);
    }

    export function getCurrentDocument(): IDocumentState {
        return _currentDocument;
    }

    export function setCurrentDocument(document: IDocumentState): void {
        if (_currentDocument === document) {
            return;
        }
        var previousDocument = _currentDocument;

        _currentDocument = document;

        $(DocumentManager).triggerHandler('currentDocumentChange', document, previousDocument);
    }

    $(ProjectManager).on('projectOpen', function(event: JQueryEventObject, project: IProjectState) {
        _addDocumentList(project.Documents);
    });
}