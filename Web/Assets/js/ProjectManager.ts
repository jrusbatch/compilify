/// <reference path="app.ts" />
/// <reference path="ReferenceManager.ts" />

module Compilify.ProjectManager {
    var _currentProject: IProjectState = null;

    export function openProject(project: IProjectState) {
        if (_currentProject) {
            $(ProjectManager).triggerHandler('beforeProjectClose');
        }

        _currentProject = project;

        $(ProjectManager).triggerHandler('projectOpen', project);
    }

    export function renameDocument() {
        throw new Error('Not implemented');
    }

    export function createDocument() {
        throw new Error('Not implemented');
    }
}
