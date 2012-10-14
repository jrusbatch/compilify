/// <reference path="vendor/jquery.d.ts" />
/// <reference path="vendor/doT.d.ts" />

module TemplateManager {
    var global = (new Function('return this'))(),
        doT = global.doT,
        
        _templates = {};

    export function getTemplateById(id: string) {
        var tmpl: (t: any) => string;

        if (!(tmpl = _templates[id])) {
            var text = $(id).html();
            tmpl = _templates[id] = doT.template(text);
        }

        return tmpl;
    }
}