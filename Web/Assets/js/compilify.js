
(function($, _, Compilify) {
    'use strict';

    var root = this,
        connection,
        markedErrors = [];
    
    function _htmlEncode(value) {
        return $('<div/>').text(value).html();
    }

    //
    // Set up the SignalR connection
    //
    connection = $.connection('/execute');

    connection.received(function(msg) {
        if (msg && msg.status === "ok") {
            $('#footer .results pre').html(msg.data);
        }

        // $('#footer').removeClass('loading');
    });

    connection.start();
    
    function _clearMarkedErrors() {
        for (var i = markedErrors.length - 1; i >= 0; i--) {
            markedErrors[i].clear();
        }

        markedErrors.length = 0;
    }
    
    function _onValidationCompleted(msg) {
        var data = msg.data;
        
        if (!_.isArray(data)) {
            return;
        }

        _clearMarkedErrors();

        // var $list = $('#footer .status ul.messages').detach().empty();

        if (data.length === 0) {
            // $('#footer .status').removeClass('status-error').addClass('status-success');
            // $list.html("<li>No errors!</li>");
            return;
        }

        // $('#footer .status').removeClass('status-success').addClass('status-error');
        for (var index in msg.data) {
            var error = msg.data[index];
            var loc = error.Location;

            var file = loc.DocumentName;

            var start = loc.StartLinePosition;
            var end = loc.EndLinePosition;

            var editor = Compilify.Editor.getEditorByName(file);

            if (editor) {
                var markStart = { line: start.Line, ch: start.Character };
                var markEnd = { line: end.Line, ch: end.Character };

                var mark = editor._codeMirror.markText(markStart, markEnd, 'compilation-error');

                markedErrors.push(mark);
            }

            // var message = 'Line: ' + (start.Line + 1) +
            //              ' Column: ' + start.Character + ' - ' + error.Message;

            // $list.append('<li data-errorId="' + index + '">' + _.escape(message) + '</li>');
        }


        // $('#footer .status').append($list);
    }

    var Document = (function() {

        var counter = 0;
        
        function Document(name, text) {
            var id = ++counter;
            this._id = id;
            this._name = name || 'Untitled-' + id;
            this._text = text || '';
        }

        Document.load = function(documentState) {
            if (!documentState) {
                throw new Error('documentState cannot be null');
            }

            return new Document(documentState.Name, documentState.Content);
        };

        Document.prototype._id = 0;
        Document.prototype._name = 'Untitled-0';
        Document.prototype._text = '';

        Document.prototype.getId = function() {
            return this._id;
        };

        Document.prototype.getName = function() {
            return this._name;
        };

        Document.prototype.setName = function(name) {
            if (!name || name.length === 0) {
                throw new Error('Document name cannot be undefined, null, or an empty string');
            }

            this._name = name;
        };
        
        Document.prototype.getText = function() {
            var editor;
            if (editor  = this._editor) {
                return editor.getText();
            }
            
            return this._text;
        };

        Document.prototype.getState = function() {
            return {
                Name: this._name,
                Content: this._content
            };
        };

        return Document;
    }());

    var Editor = (function() {
        var instances = [],
            $container;

        var defaults = {
            indentUnit: 4,
            lineNumbers: true,
            theme: 'neat',
            mode: 'text/x-csharp',
            // autofocus: true
        };
        
        function Editor(doc) {
            if (!doc) {
                throw new Error('An editor cannot be created without an underlying document');
            }

            var options = _.defaults(defaults, { value: doc.getText() });

            var $tabPane = $('<div data-documentId="' + doc.getId() + '" class="tab-pane"><textarea>' + doc.getText() + '</textarea></div>').hide();
            // $tabContainer = $('.tab-content').parent(),
            // $tabContent = $('.tab-content').detach();

            //$tabContent.find('.tab-pane').removeClass('active');
            $container.append($tabPane);

            //$tabContent.prependTo($tabContainer);

            this._codeMirror = CodeMirror.fromTextArea($tabPane.children('textarea')[0], options);
            this._markedErrors = [];

            this._$element = $tabPane;

            doc._editor = this;
        }

        Editor.prototype._$element = null;
        Editor.prototype._codeMirror = null;
        Editor.prototype._markedErrors = null;
        
        Editor.getEditorByName = function(name) {
            name = (name || '').toUpperCase();
            for (var i = 0, len = instances.length; i < len; i++) {
                var editor = instances[i];
                if (editor.getName().toUpperCase() === name) {
                    return editor;
                }
            }

            return null;
        };

        Editor.getAllEditors = function() {
            return instances.slice();
        };
        
        Editor.prototype.getName = function() {
            return this._name;
        };

        Editor.prototype.getText = function() {
            return this._codeMirror.getValue();
        };

        Editor.prototype.setVisible = function(visible) {
            this._$element.toggle(visible);
            if (visible) {
                this.refresh();
                this.focus();
            }
        };

        Editor.prototype.focus = function() {
            this._codeMirror.focus();
        };

        Editor.prototype.refresh = function() {
            this._codeMirror.refresh();
        };

        Editor.prototype.dispose = function() {
            $(this._codeMirror.getWrapperElement()).remove();
            $(this._element).remove();
            instances.splice(instances.indexOf(this), 1);
        };

        function _onProjectLoaded() {
            var documents = Compilify.Workspace.getDocuments();
            documents.forEach(_onDocumentAdded);
        }

        function _onDocumentAdded(document) {
            var editor = new Editor(document);
            instances.push(editor);
        }

        function _onActiveDocumentChanged(active, previous) {
            if (previous) {
                previous._editor.setVisible(false);
            }
            
            if (active) {
                active._editor.setVisible(true);
            }
        }
        
        function _onDocumentRemoved(document) {
            document._editor.dispose();
        }

        $(Compilify)
            .on('projectLoaded', _onProjectLoaded)
            .on('documentAdded', function($e, document) { _onDocumentAdded(document); })
            .on('documentRemoved', function($e, document) { _onDocumentRemoved(document); })
            .on('activeDocumentChanged', function($e, active, previous) { _onActiveDocumentChanged(active, previous); });

        $(function() {
            $container = $('.tab-content');
        });

        return Editor;
    }());

    var Sidebar = (function() {

        var $container,
            $referencesContainer,
            referenceTemplate;

        function _initSidebar() {
            $container = $('#sidebar');
            $referencesContainer = $container.find('.references');

            referenceTemplate = doT.template($('#reference-template').html());
        }
        
        function _onReferenceAdded(reference) {
            var listItem = referenceTemplate(reference);
            $referencesContainer.append(listItem);
        }
        
        function _onProjectLoaded() {
            var references = Compilify.Workspace.getReferences();
            references.forEach(_onReferenceAdded);
        }

        $(Compilify).on('projectLoaded', _onProjectLoaded);

        $(_initSidebar);
    }());

    var Workspace = (function() {

        function Workspace(container, state) {
            this._$container = $(container);
            this._references = [];
            this._documents = [];
            this._activeDocument = null;
        }

        Workspace.prototype._$container = null;
        Workspace.prototype._references = null;
        Workspace.prototype._documents = null;
        Workspace.prototype._activeDocument = null;

        Workspace.prototype._getDocumentStates = function() {
            return this._documents.map(function(document) {
                return document.getState();
            });
        };

        Workspace.prototype.openProject = function(project) {
            var documents = project.Documents || [],
                references = project.References || [];
            
            this._documents = documents.map(function(document) {
                return Document.load(document);
            });

            this._references = references;

            $(Compilify).triggerHandler('projectLoaded');
            
            if (this._documents.length > 0) {
                this.setActiveDocument(this._documents[0]);
            }
        };

        Workspace.prototype.saveProject = function() {

            var documents = this._getDocumentStates();

            var project = {
                Documents: documents,
                References: this.getReferences()
            };

            var request = $.ajax({
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(project)
            });

            request.done(function() {
                console.log('project saved.', arguments);
            });
        };

        Workspace.prototype.validateProject = function() {
            return $.ajax('/validate', {
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify({ Documents: this._getDocumentStates() }),
                dataType: 'json',
                success: function(msg) {
                    _onValidationCompleted(msg);
                    $(Compilify).triggerHandler('validationComplete', msg.data);
                }
            });
        };

        Workspace.prototype.executeProject = function() {
            var documents = this._getDocumentStates();

            connection.send(JSON.stringify({ Documents: documents }));

            $('#footer:not(.loading)').addClass('loading');

            $('.results pre').empty();
        };


        Workspace.prototype.addDocument = function() {
            var document = new Document();
            
            this._documents.unshift(document);

            $(Compilify).triggerHandler('documentAdded', document);

            this.setActiveDocument(document);
        };

        Workspace.prototype.setActiveDocument = function(document) {
            if (!document) {
                throw new Error('There must be a document active at all times');
            }
            
            if (document === this._activeDocument) {
                return;
            }
            
            if (this._documents.indexOf(document) === -1) {
                return; // The document has recently been deleted
            }

            var previous = this._activeDocument;

            this._activeDocument = document;
            
            $(Compilify).triggerHandler('activeDocumentChanged', [document, previous]);
        };

        Workspace.prototype.getDocuments = function() {
            return this._documents.slice(0);
        };

        Workspace.prototype.getDocumentByName = function(name) {
            if (name && name.length > 0) {
                name = name.toUpperCase();

                var document;
                for (var i = 0, len = this._documents.length; i < len; i++) {
                    document = this._documents[i];
                    if (name === document.getName().toUpperCase()) {
                        return document;
                    }
                }
            }

            return null;
        };

        Workspace.prototype.renameDocument = function(document) {
            if (!document || document.getName() === 'Main') {
                return; // The main tab cannot be renamed
            }

            var oldName = document.getName();
            
            // TODO: Prompt user for a new name for this document
            var newName = _htmlEncode(prompt('Please enter a new, unique name for this document', oldName));
            
            if (newName && newName.length > 0 && newName !== oldName && !this.getDocumentByName(newName)) {
                document.setName(newName);
                $(Compilify).triggerHandler('documentRenamed', document);
            }
        };

        Workspace.prototype.removeDocument = function(document) {
            if (document.getName() === 'Main') {
                return; // The Main document cannot be removed
            }

            var index = this._documents.indexOf(document);

            if (index !== -1) {

                this._documents.splice(index, 1);

                $(Compilify).triggerHandler('documentRemoved', document);
                
                if (document === this._activeDocument) {
                    this.setActiveDocument(this._documents[0]);
                }
            }
        };


        Workspace.prototype.addReference = function() { throw new Error('Not implemented'); };

        Workspace.prototype.getReferences = function() {
            return this._references.slice(0);
        };

        Workspace.prototype.removeReference = function() { throw new Error('Not implemented'); };

        return Workspace;
    }());
    
    Compilify.init = function(container, state) {
        delete Compilify.init;
        Compilify.Workspace = new Workspace($(container), state);
        Compilify.Workspace.openProject(state.Project);
    };
    
}).call(window, window.jQuery, window._, window.Compilify || (window.Compilify = { }));
