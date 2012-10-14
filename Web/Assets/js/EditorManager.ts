/// <reference path="app.ts" />
/// <reference path="Editor.ts" />
/// <reference path="DocumentManager.ts" />

module Compilify.EditorManager {
    var _currentEditor: Editor = null;
    var _currentEditorsDocument: IDocument = null;
    var _container: JQuery = null;
    
    function _init() {
        _container = $('.editor.tab-content');

        // TODO: Initialize the status bar here?
    }

    function _doFocusedEditorChanged(current: Editor, previous: Editor) {
        // Skip if the new editor is already the focused editor.
        // This may happen if the window loses then regains focus.
        if (previous === current) {
            return;
        }

        // if switching to no-editor, hide the last full editor
        if (_currentEditor && !current) {
            _currentEditor.setVisible(false);
        }

        _currentEditor = current;

        // Window may have been resized since last time editor was visible, so kick it now
        if (_currentEditor) {
            _currentEditor.setVisible(true);
            resizeEditor();
        }

        Events.trigger('focusedEditorChange', [current, previous]);
    }

    function _doShow(document) {
        // Show new editor
        _currentEditorsDocument = document;
        _doFocusedEditorChanged(document._masterEditor, _currentEditor);
    }

    function _createEditorForDocument(document: IDocument, makeMasterEditor: bool, container: JQuery): Editor {
        return new Editor(document, makeMasterEditor, container);
    }

    function _createFullEditorForDocument(document: IDocument) {
        // Create editor; make it initially invisible
        var container = _container.get(0);
        var editor = _createEditorForDocument(document, true, container);
        editor.setVisible(false);
    }
    
    function _showEditor(document: IDocument): void {
        // Hide whatever was visible before
        if (!_currentEditor) {
            $("#not-editor").css("display", "none");
        } else {
            _currentEditor.setVisible(false);
        }
        
        // Ensure a main editor exists for this document to show in the UI
        if (!document.getMasterEditor()) {
            // Editor doesn't exist: populate a new Editor with the text
            _createFullEditorForDocument(document);
        }
        
        _doShow(document);
    }

    function _onCurrentDocumentChange(): void {
        var document = DocumentManager.getCurrentDocument(),
            container = $('.editor.tab-content').get(0);

        _showEditor(document);
    }

    function _onDocumentAdded(document: IDocument): void {
        _createFullEditorForDocument(document);
    }

    function _onDocumentAddedList(...documents: IDocument[]): void { 
        for (var i = 0, len = documents.length; i < len; i++) {
            _createFullEditorForDocument(documents[i]);
        }
    }

    function _onDocumentRemove(): void {
        throw new Error('Not Implemented');
    }

    function _onDocumentRemoveList(): void {
        throw new Error('Not Implemented');
    }
    
    function resizeEditor(): void {
        if (_currentEditor) {
            $(_currentEditor.getScrollerElement()).height(_container.height());
            _currentEditor.refresh();
        }
    }

    $(_init);

    Events.on('documentAdded', _onDocumentAdded);
    Events.on('documentAddedList', _onDocumentAddedList);
    Events.on('currentDocumentChange', _onCurrentDocumentChange);
    Events.on('documentRemove', _onDocumentRemove);
    Events.on('documentRemoveList', _onDocumentRemoveList);
}
