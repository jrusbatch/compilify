/// <reference path="app.ts" />
/// <reference path="ProjectManager.ts" />

module Compilify.DocumentManager {

    function _addDocument(document: IDocumentState, suppressTrigger?: bool = false) {
        // Do some spectacular here

        if (!suppressTrigger) {
            $(DocumentManager).triggerHandler('documentAdded', document);
        }
    }

    function _addDocumentList(documents: IDocumentState[]): void {
        console.log('Adding documents', documents);

        for (var i = 0, len = documents.length; i < len; i++) {
            _addDocument(documents[i], true);
        }

        $(DocumentManager).triggerHandler('documentListAdded', [documents]);
    }

    export function getCurrentDocument(): IDocumentState {
        return null;
    }

    $(ProjectManager).on('projectOpen', function(event: JQueryEventObject, project: IProjectState) {
        console.log('projectOpen event received', project);
        _addDocumentList(project.Documents);
    });
}