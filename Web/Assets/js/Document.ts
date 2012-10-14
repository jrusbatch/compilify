module Compilify {
    'use strict';

    export interface IDocument {
        getName(): string;
        setName(name: string): void;

        getText(): string;
        setText(text: string): void;

        getMasterEditor(): Editor;
    }

    var _openDocuments: any = {};

    export class Document implements IDocument {
        private _name: string;
        private _text: string;
        private _masterEditor: Editor;

        constructor(documentState: IDocumentState) {
            if (!(this instanceof Document)) {
                throw new Error("Document constructor must be called with 'new'");
            }

            var name = documentState.Name;

            if (_openDocuments[name]) {
                throw new Error("Creating a document when one already exists, for: " + name);
            }

            _openDocuments[name] = this;
        
            this._name = name;
        }

        public getName(): string {
            return this._name;
        }

        public setName(name: string): void {
            this._name = name;
        }

        getText(): string {
            return this._text;
        }
        
        setText(text: string): void {
            this._text = text;
        }

        getMasterEditor(): Editor {
            return this._masterEditor;
        }

    }

    /**
    Document.prototype._refCount = 0;
    
    Document.prototype.file = null;
    
    Document.prototype.isDirty = false;
    
    Document.prototype.diskTimestamp = null;
    
    Document.prototype._text = null;
    
    Document.prototype._masterEditor = null;
    
    Document.prototype._lineEndings = null;

    Document.prototype._ensureMasterEditor = function () {
        if (!this._masterEditor) {
            EditorManager._createFullEditorForDocument(this);
        }
    };
    
    Document.prototype.getText = function () {
        if (this._masterEditor) {
            return this._masterEditor._codeMirror.getValue();
        } else {
            return this._text;
        }
    };
    
    Document.prototype.setText = function (text) {
        this._ensureMasterEditor();
        this._masterEditor._codeMirror.setValue(text);
        // _handleEditorChange() triggers "change" event
    };
    
    Document.prototype.replaceRange = function (text, start, end) {
        this._ensureMasterEditor();
        this._masterEditor._codeMirror.replaceRange(text, start, end);
        // _handleEditorChange() triggers "change" event
    };
    
    Document.prototype.getRange = function (start, end) {
        this._ensureMasterEditor();
        return this._masterEditor._codeMirror.getRange(start, end);
    };
    
    Document.prototype.getLine = function (lineNum) {
        this._ensureMasterEditor();
        return this._masterEditor._codeMirror.getLine(lineNum);
    };
    
    
**/
}