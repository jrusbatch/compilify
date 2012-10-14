/// <reference path="app.ts" />
/// <reference path="ProjectManager.ts" />
/// <reference path="Document.ts" />

module Compilify.DocumentManager {
    var _currentDocument: IDocument = null;
    var _openDocuments: IDocument[] = [];

    function _addDocument(document: IDocument, suppressTrigger?: bool): void {

        _openDocuments.push(document);
        
        if (!suppressTrigger) {
            $(DocumentManager).triggerHandler('documentAdded', document);
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

        $(DocumentManager).triggerHandler('documentAddedList', [documents]);
        setCurrentDocument(documents[0]);

        console.log('Done loading documents!', documents);
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

        $(DocumentManager).triggerHandler('currentDocumentChange', document, previousDocument);
    }

    $(ProjectManager).on('projectOpen', function(event: JQueryEventObject, project: IProjectState) {
        _addDocumentList(project.Documents);
    });
}