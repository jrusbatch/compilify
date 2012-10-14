/// <reference path="vendor/jquery.d.ts" />
/// <reference path="app.ts" />

module Compilify.ReferenceManager {
    var _references: IReferenceState[] = [];

    function _areEqual(first: IReferenceState, second: IReferenceState) {
        return first.Name === second.Name && first.Version === second.Version;
    }

    // TODO: Move this into the Reference class
    function _containsReference(reference: IReferenceState) {
        var current;
        for (var i = 0, len = _references.length; i < len; i++) {
            current = _references[i];
            if (_areEqual(reference, current)) {
                return true;
            }
        }
    }

    function _addReference(reference: IReferenceState) {
        _references.push(reference);
    }

    export function addReference(reference: IReferenceState) {
        if (!_containsReference(reference)) {
            _addReference(reference);
            $(ReferenceManager).triggerHandler('referenceAdded', [ reference ]);
        }
    }

    export function addReferenceList(references: IReferenceState[]) {
        var added: IReferenceState[] = [];
        var reference;
        for (var i = 0, len = references.length; i < len; i++) {
            reference = references[i];
            if (!_containsReference(reference)) {
                _addReference(reference);
                added.push(reference);
            }
        }

        if (added.length > 0) {
            $(ReferenceManager).triggerHandler('referencesAdded', added);
        }
    }
}