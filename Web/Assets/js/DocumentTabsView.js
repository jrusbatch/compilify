/// <reference path="app.ts" />
/// <reference path="DocumentManager.ts" />
/// <reference path="TemplateManager.ts" />
var Compilify;
(function (Compilify) {
    (function (DocumentTabsView) {
        function _findTabByDocument(document) {
            return $('#document-tabs').find('[data-target="' + document.getName() + '"]').parent('li');
        }
        function _onDocumentAdded(document) {
            var template = TemplateManager.getTemplateById('#tab-template');
            var $newTab = $(template(document));
            $newTab.on('click', function () {
                Compilify.DocumentManager.setCurrentDocument(document);
            });
            $('#document-tabs').append($newTab);
        }
        function _onDocumentListAdded(documents) {
            for(var i = 0, len = documents.length; i < len; i++) {
                _onDocumentAdded(documents[i]);
            }
        }
        function _onCurrentDocumentChange() {
            var currentDocument = Compilify.DocumentManager.getCurrentDocument();
            $('#document-tabs').find('li').removeClass('active');
            _findTabByDocument(currentDocument).addClass('active');
        }
        Compilify.Events.on('documentAdded', function (document) {
            _onDocumentAdded(document);
        });
        Compilify.Events.on('documentAddedList', function (documents) {
            if (typeof documents === "undefined") { documents = []; }
            _onDocumentListAdded(documents);
        });
        Compilify.Events.on('currentDocumentChange', _onCurrentDocumentChange);
    })(Compilify.DocumentTabsView || (Compilify.DocumentTabsView = {}));
    var DocumentTabsView = Compilify.DocumentTabsView;

})(Compilify || (Compilify = {}));

