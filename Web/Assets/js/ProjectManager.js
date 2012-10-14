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
                $(ProjectManager).triggerHandler('beforeProjectClose');
            }
            _currentProject = project;
            $(ProjectManager).triggerHandler('projectOpen', project);
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

