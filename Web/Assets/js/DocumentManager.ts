/// <reference path="app.ts" />
/// <reference path="ProjectManager.ts" />
/// <reference path="Document.ts" />

module Compilify.DocumentManager {
    var _currentDocument: IDocument = null;
    var _openDocuments: IDocument[] = [];

    function _addDocument(document: IDocument, suppressTrigger?: bool): void {

        _openDocuments.push(document);
        
        if (!suppressTrigger) {
            Events.trigger('documentAdded', document);
            setCurrentDocument(document);
        }
    }

    function _addDocumentList(documentStates: IDocumentState[]): void {
        var documents: IDocument[] = [];

        var document;
        for (var i = 0, len = documentStates.length; i < len; i++) {
            document = new Document(documentStates[i]);
            _addDocument(document, true);
            documents.push(document);
        }

        Events.trigger('documentAddedList', [documents]);

        setCurrentDocument(documents[0]);
    }

    export function getCurrentDocument(): IDocument {
        return _currentDocument;
    }

    export function setCurrentDocument(document: IDocument): void {
        if (_currentDocument === document) {
            return;
        }

        var previousDocument = _currentDocument;

        _currentDocument = document;

        Events.trigger('currentDocumentChange', document, previousDocument);
    }

    Events.on('projectOpen', function(project: IProjectState) {
        _addDocumentList(project.Documents);
    });
}