var Compilify;
(function (Compilify) {
    var global = (new Function('return this'))();
    var $ = global.jQuery;
    var _ = global._;
    var _isInitialized = false;

    function _openProject(project) {
    }
    function initializeWorkspace(state) {
        if(_isInitialized) {
            throw new Error('Workspace has already been initialized.');
        }
        _isInitialized = true;
        $(function () {
            _openProject(state.Project);
        });
    }
    Compilify.initializeWorkspace = initializeWorkspace;
})(Compilify || (Compilify = {}));

