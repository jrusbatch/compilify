var Compilify;
(function (Compilify) {
    (function (DocumentManager) {
        var _currentDocument;
        function _addDocument(document, suppressTrigger) {
            if (typeof suppressTrigger === "undefined") { suppressTrigger = false; }
            if(!suppressTrigger) {
                $(DocumentManager).triggerHandler('documentAdded', document);
                setCurrentDocument(document);
            }
        }
        function _addDocumentList(documents) {
            for(var i = 0, len = documents.length; i < len; i++) {
                _addDocument(documents[i], true);
            }
            $(DocumentManager).triggerHandler('documentListAdded', [
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
            $(DocumentManager).triggerHandler('currentDocumentChange', document, previousDocument);
        }
        DocumentManager.setCurrentDocument = setCurrentDocument;
        $(Compilify.ProjectManager).on('projectOpen', function (event, project) {
            _addDocumentList(project.Documents);
        });
    })(Compilify.DocumentManager || (Compilify.DocumentManager = {}));
    var DocumentManager = Compilify.DocumentManager;

})(Compilify || (Compilify = {}));

