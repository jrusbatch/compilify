/// <reference path="vendor/jquery.d.ts" />
/// <reference path="app.ts" />
/// <reference path="TemplateManager.ts" />
var Compilify;
(function (Compilify) {
    (function (ReferenceManager) {
        var _references = [];
        function _areEqual(first, second) {
            return first.Name === second.Name && first.Version === second.Version;
        }
        // TODO: Move this into the Reference class
        function _containsReference(reference) {
            var current;
            for(var i = 0, len = _references.length; i < len; i++) {
                current = _references[i];
                if(_areEqual(reference, current)) {
                    return true;
                }
            }
        }
        function _addReference(reference) {
            _references.push(reference);
        }
        function addReference(reference) {
            if(!_containsReference(reference)) {
                _addReference(reference);
                Compilify.Events.trigger('referenceAdded', [
                    reference
                ]);
            }
        }
        ReferenceManager.addReference = addReference;
        function addReferenceList(references) {
            var added = [];
            var reference;
            for(var i = 0, len = references.length; i < len; i++) {
                reference = references[i];
                if(!_containsReference(reference)) {
                    _addReference(reference);
                    added.push(reference);
                }
            }
            if(added.length > 0) {
                Compilify.Events.trigger('referencesAdded', added);
            }
        }
        ReferenceManager.addReferenceList = addReferenceList;
        Compilify.Events.on('projectOpen', function (project) {
            addReferenceList(project.References);
        });
    })(Compilify.ReferenceManager || (Compilify.ReferenceManager = {}));
    var ReferenceManager = Compilify.ReferenceManager;

})(Compilify || (Compilify = {}));

