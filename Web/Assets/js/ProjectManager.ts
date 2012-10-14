/// <reference path="app.ts" />
/// <reference path="ReferenceManager.ts" />

module Compilify.ProjectManager {
    var _currentProject: IProjectState = null;

    export function getCurrentProject() {
        return _currentProject;
    }

    export function openProject(project: IProjectState) {
        if (_currentProject) {
            Events.trigger('beforeProjectClose');
        }

        _currentProject = project;

        Events.trigger('projectOpen', project);
    }

    export function renameDocument() {
        throw new Error('Not implemented');
    }

    export function createDocument() {
        throw new Error('Not implemented');
    }
}
