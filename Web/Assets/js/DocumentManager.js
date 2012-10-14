/// <reference path="app.ts" />
/// <reference path="ProjectManager.ts" />
/// <reference path="Document.ts" />
var Compilify;
(function (Compilify) {
    (function (DocumentManager) {
        var _currentDocument = null;
        var _openDocuments = [];
        function _addDocument(document, suppressTrigger) {
            _openDocuments.push(document);
            if(!suppressTrigger) {
                $(DocumentManager).triggerHandler('documentAdded', document);
                setCurrentDocument(document);
            }
        }
        function _addDocumentList(documentStates) {
            var documents = [];
            var document;
            for(var i = 0, len = documentStates.length; i < len; i++) {
                document = new Compilify.Document(documentStates[i]);
                _addDocument(document, true);
                documents.push(document);
            }
            $(DocumentManager).triggerHandler('documentAddedList', [
                documents
            ]);
            setCurrentDocument(documents[0]);
            console.log('Done loading documents!', documents);
        }
        function getCurrentDocument() {
            return _currentDocument;
        }
        DocumentManager.getCurrentDocument = getCurrentDocument;
        function setCurrentDocument(document) {
            if(_currentDocument === document) {
                return;
            }
            var previousDocument = _currentDocument;
            _currentDocument = document;
            $(DocumentManager).triggerHandler('currentDocumentChange', document, previousDocument);
        }
        DocumentManager.setCurrentDocument = setCurrentDocument;
        $(Compilify.ProjectManager).on('projectOpen', function (event, project) {
            _addDocumentList(project.Documents);
        });
    })(Compilify.DocumentManager || (Compilify.DocumentManager = {}));
    var DocumentManager = Compilify.DocumentManager;

})(Compilify || (Compilify = {}));

