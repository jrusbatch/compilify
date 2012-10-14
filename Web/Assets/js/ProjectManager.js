var Compilify;
(function (Compilify) {
    (function (ProjectManager) {
        var _currentProject = null;
        function openProject(project) {
            if(_currentProject) {
                $(ProjectManager).triggerHandler('beforeProjectClose');
            }
            console.log(project);
            Compilify.ReferenceManager.addReferenceList(project.References);
            _currentProject = project;
        }
        ProjectManager.openProject = openProject;
        function renameDocument() {
            return null;
        }
        ProjectManager.renameDocument = renameDocument;
        function createDocument() {
        }
        ProjectManager.createDocument = createDocument;
    })(Compilify.ProjectManager || (Compilify.ProjectManager = {}));
    var ProjectManager = Compilify.ProjectManager;

})(Compilify || (Compilify = {}));

