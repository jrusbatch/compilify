/// <reference path="vendor/jquery.d.ts" />
/// <reference path="vendor/doT.d.ts" />
var TemplateManager;
(function (TemplateManager) {
    var global = (new Function('return this')());
    var doT = global.doT;
    var _templates = {
    };

    function getTemplateById(id) {
        var tmpl;
        if(!(tmpl = _templates[id])) {
            var text = $(id).html();
            tmpl = _templates[id] = doT.template(text);
        }
        return tmpl;
    }
    TemplateManager.getTemplateById = getTemplateById;
})(TemplateManager || (TemplateManager = {}));

