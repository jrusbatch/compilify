var Compilify;
(function (Compilify) {
    (function (DocumentManager) {
        function _addDocument(document, suppressTrigger) {
            if (typeof suppressTrigger === "undefined") { suppressTrigger = false; }
            if(!suppressTrigger) {
                $(DocumentManager).triggerHandler('documentAdded', document);
            }
        }
        function _addDocumentList(documents) {
            console.log('Adding documents', documents);
            for(var i = 0, len = documents.length; i < len; i++) {
                _addDocument(documents[i], true);
            }
            $(DocumentManager).triggerHandler('documentListAdded', [
                documents
            ]);
        }
        function getCurrentDocument() {
            return null;
        }
        DocumentManager.getCurrentDocument = getCurrentDocument;
        $(Compilify.ProjectManager).on('projectOpen', function (event, project) {
            console.log('projectOpen event received', project);
            _addDocumentList(project.Documents);
        });
    })(Compilify.DocumentManager || (Compilify.DocumentManager = {}));
    var DocumentManager = Compilify.DocumentManager;

})(Compilify || (Compilify = {}));

