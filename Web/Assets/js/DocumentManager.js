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
                Compilify.Events.trigger('documentAdded', document);
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
            Compilify.Events.trigger('documentAddedList', [
                documents
            ]);
            setCurrentDocument(documents[0]);
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
            Compilify.Events.trigger('currentDocumentChange', document, previousDocument);
        }
        DocumentManager.setCurrentDocument = setCurrentDocument;
        Compilify.Events.on('projectOpen', function (project) {
            _addDocumentList(project.Documents);
        });
    })(Compilify.DocumentManager || (Compilify.DocumentManager = {}));
    var DocumentManager = Compilify.DocumentManager;

})(Compilify || (Compilify = {}));

