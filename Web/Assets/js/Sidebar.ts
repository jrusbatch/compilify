/// <reference path="vendor/jquery.d.ts" />
/// <reference path="app.ts" />
/// <reference path="DocumentManager.ts" />
/// <reference path="TemplateManager.ts" />

module Compilify.Sidebar {
    var $sidebar: JQuery = null,
        $referencesContainer: JQuery = null;

    $(function () {
        $sidebar = $('#sidebar');
        $referencesContainer = $('#references');
        ReferenceListView.create($referencesContainer);
    });
}

module Compilify.ReferenceListView {
    var _container: JQuery = null,
        _list: JQuery = null,
        _template: (obj: any) => string;

    function _handleReferencesAdded(references: IReferenceState[]) {
        var html = '';
        for (var i = 0, len = references.length; i < len; i++) {
            var reference = references[i];
            html += _template(reference);
        }

        _list.html(_list.html() + html);
    }

    export function create(element: JQuery) {
        _container = element;
        _list = element.find('ul');
        _template = TemplateManager.getTemplateById('#reference-template');

        $(ReferenceManager).on('referencesAdded', function(event: string, ...references: IReferenceState[]) {
            console.log(event, references);
            _handleReferencesAdded(references);
        });
    }
}
