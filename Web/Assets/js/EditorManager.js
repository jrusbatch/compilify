/// <reference path="app.ts" />
/// <reference path="Editor.ts" />
/// <reference path="DocumentManager.ts" />
var Compilify;
(function (Compilify) {
    (function (EditorManager) {
        var _currentEditor = null;
        var _currentEditorsDocument = null;
        var _container = null;
        function _init() {
            // TODO: Initialize the status bar here?
                    }
        function _doFocusedEditorChanged(current, previous) {
            // Skip if the new editor is already the focused editor.
            // This may happen if the window loses then regains focus.
            if(previous === current) {
                return;
            }
            // if switching to no-editor, hide the last full editor
            if(_currentEditor && !current) {
                _currentEditor.setVisible(false);
            }
            _currentEditor = current;
            // Window may have been resized since last time editor was visible, so kick it now
            if(_currentEditor) {
                _currentEditor.setVisible(true);
                resizeEditor();
            }
            Compilify.Events.trigger('focusedEditorChange', [
                current, 
                previous
            ]);
        }
        function _doShow(document) {
            // Show new editor
            _currentEditorsDocument = document;
            _doFocusedEditorChanged(document._masterEditor, _currentEditor);
        }
        function _createEditorForDocument(document, makeMasterEditor, container) {
            return new Compilify.Editor(document, makeMasterEditor, container);
        }
        function _createFullEditorForDocument(document) {
            console.log('creating editor for', document);
            // Create editor; make it initially invisible
            var container = _container.get(0);
            var editor = _createEditorForDocument(document, true, container);
            editor.setVisible(false);
        }
        function _showEditor(document) {
            // Hide whatever was visible before
            if(!_currentEditor) {
                $("#not-editor").css("display", "none");
            } else {
                _currentEditor.setVisible(false);
            }
            // Ensure a main editor exists for this document to show in the UI
            if(!document.getMasterEditor()) {
                // Editor doesn't exist: populate a new Editor with the text
                _createFullEditorForDocument(document);
            }
            _doShow(document);
        }
        function _onCurrentDocumentChange() {
            var document = Compilify.DocumentManager.getCurrentDocument();
            var container = $('.editor.tab-content').get(0);

            _showEditor(document);
        }
        function _onDocumentAdded(document) {
            _createFullEditorForDocument(document);
        }
        function _onDocumentAddedList() {
            var documents = [];
            for (var _i = 0; _i < (arguments.length - 0); _i++) {
                documents[_i] = arguments[_i + 0];
            }
            console.log('_onDocumentAddedList');
            for(var i = 0, len = documents.length; i < len; i++) {
                _createFullEditorForDocument(documents[i]);
            }
        }
        function _onDocumentRemove() {
            throw new Error('Not Implemented');
        }
        function _onDocumentRemoveList() {
            throw new Error('Not Implemented');
        }
        function resizeEditor() {
            if(_currentEditor) {
                $(_currentEditor.getScrollerElement()).height(_container.height());
                _currentEditor.refresh();
            }
        }
        Compilify.Events.on('documentAdded', _onDocumentAdded);
        Compilify.Events.on('documentAddedList', _onDocumentAddedList);
        Compilify.Events.on('currentDocumentChange', _onCurrentDocumentChange);
        Compilify.Events.on('documentRemove', _onDocumentRemove);
        Compilify.Events.on('documentRemoveList', _onDocumentRemoveList);
    })(Compilify.EditorManager || (Compilify.EditorManager = {}));
    var EditorManager = Compilify.EditorManager;

})(Compilify || (Compilify = {}));

