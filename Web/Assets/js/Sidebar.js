/// <reference path="vendor/jquery.d.ts" />
/// <reference path="app.ts" />
/// <reference path="DocumentManager.ts" />
/// <reference path="TemplateManager.ts" />
var Compilify;
(function (Compilify) {
    (function (Sidebar) {
        var $sidebar = null;
        var $referencesContainer = null;

        $(function () {
            $sidebar = $('#sidebar');
            $referencesContainer = $('#references');
            Compilify.ReferenceListView.create($referencesContainer);
        });
    })(Compilify.Sidebar || (Compilify.Sidebar = {}));
    var Sidebar = Compilify.Sidebar;

})(Compilify || (Compilify = {}));

var Compilify;
(function (Compilify) {
    (function (ReferenceListView) {
        var _container = null;
        var _list = null;
        var _template;

        function _handleReferencesAdded(references) {
            var html = '';
            for(var i = 0, len = references.length; i < len; i++) {
                var reference = references[i];
                html += _template(reference);
            }
            _list.html(_list.html() + html);
        }
        function create(element) {
            _container = element;
            _list = element.find('ul');
            _template = TemplateManager.getTemplateById('#reference-template');
            Compilify.Events.on('referencesAdded', function () {
                var references = [];
                for (var _i = 0; _i < (arguments.length - 0); _i++) {
                    references[_i] = arguments[_i + 0];
                }
                console.log(event, references);
                _handleReferencesAdded(references);
            });
        }
        ReferenceListView.create = create;
    })(Compilify.ReferenceListView || (Compilify.ReferenceListView = {}));
    var ReferenceListView = Compilify.ReferenceListView;

})(Compilify || (Compilify = {}));

