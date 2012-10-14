var Compilify;
(function (Compilify) {
    'use strict';
    var _openDocuments = {
    };
    var Document = (function () {
        function Document(documentState) {
            if(!(this instanceof Document)) {
                throw new Error("Document constructor must be called with 'new'");
            }
            var name = documentState.Name;
            if(_openDocuments[name]) {
                throw new Error("Creating a document when one already exists, for: " + name);
            }
            _openDocuments[name] = this;
            this._name = name;
        }
        Document.prototype.getName = function () {
            return this._name;
        };
        Document.prototype.setName = function (name) {
            this._name = name;
        };
        Document.prototype.getText = function () {
            return this._text;
        };
        Document.prototype.setText = function (text) {
            this._text = text;
        };
        Document.prototype.getMasterEditor = function () {
            return this._masterEditor;
        };
        return Document;
    })();
    Compilify.Document = Document;    
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
    })(Compilify || (Compilify = {}));

