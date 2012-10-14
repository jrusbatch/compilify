/// <reference path="vendor/jquery.d.ts" />
/// <reference path="Editor.ts" />
/// <reference path="ProjectManager.ts" />
var Compilify;
(function (Compilify) {
    var global = (new Function('return this'))();
    var $ = global.jQuery;
    var _isInitialized = false;

    function initializeWorkspace(state) {
        if(_isInitialized) {
            throw new Error('Workspace has already been initialized.');
        }
        _isInitialized = true;
        $(function () {
            Compilify.ProjectManager.openProject(state.Project);
        });
    }
    Compilify.initializeWorkspace = initializeWorkspace;
})(Compilify || (Compilify = {}));

