/// <reference path="app.ts" />
/// <reference path="ReferenceManager.ts" />
var Compilify;
(function (Compilify) {
    (function (ProjectManager) {
        var _currentProject = null;
        function getCurrentProject() {
            return _currentProject;
        }
        ProjectManager.getCurrentProject = getCurrentProject;
        function openProject(project) {
            if(_currentProject) {
                Compilify.Events.trigger('beforeProjectClose');
            }
            _currentProject = project;
            Compilify.Events.trigger('projectOpen', project);
        }
        ProjectManager.openProject = openProject;
        function renameDocument() {
            throw new Error('Not implemented');
        }
        ProjectManager.renameDocument = renameDocument;
        function createDocument() {
            throw new Error('Not implemented');
        }
        ProjectManager.createDocument = createDocument;
    })(Compilify.ProjectManager || (Compilify.ProjectManager = {}));
    var ProjectManager = Compilify.ProjectManager;

})(Compilify || (Compilify = {}));

