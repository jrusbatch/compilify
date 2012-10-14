/// <reference path="vendor/jquery.d.ts" />
/// <reference path="Editor.ts" />
/// <reference path="ProjectManager.ts" />
/// <reference path="Events.ts" />
var Compilify;
(function (Compilify) {
    var global = (new Function('return this')());
    var $ = global.jQuery;
    var _isInitialized = false;

    function init(state) {
        if(_isInitialized) {
            throw new Error('Workspace has already been initialized.');
        }
        _isInitialized = true;
        $(function () {
            Compilify.ProjectManager.openProject(state.Project);
        });
    }
    Compilify.init = init;
})(Compilify || (Compilify = {}));

