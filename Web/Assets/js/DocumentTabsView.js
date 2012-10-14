var Compilify;
(function (Compilify) {
    (function (DocumentTabsView) {
        function _onDocumentAdded(document) {
            var template = TemplateManager.getTemplateById('#tab-template');
            var $newTab = $(template(document));
            $('#document-tabs').append($newTab);
        }
        function _onDocumentListAdded(documents) {
            for(var i = 0, len = documents.length; i < len; i++) {
                _onDocumentAdded(documents[i]);
            }
        }
        $(Compilify.DocumentManager).on('documentAdded', function (event, document) {
            _onDocumentAdded(document);
        });
        $(Compilify.DocumentManager).on('documentListAdded', function (event, documents) {
            console.log('documentListAdded!', documents);
            _onDocumentListAdded(documents);
        });
    })(Compilify.DocumentTabsView || (Compilify.DocumentTabsView = {}));
    var DocumentTabsView = Compilify.DocumentTabsView;

})(Compilify || (Compilify = {}));

