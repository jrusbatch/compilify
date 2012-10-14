/// <reference path="app.ts" />
/// <reference path="ReferenceManager.ts" />

module Compilify.ProjectManager {
    var _currentProject: IProjectState = null;

    export function openProject(project: IProjectState) {
        if (_currentProject) {
            $(ProjectManager).triggerHandler('beforeProjectClose');
        }
        console.log(project);
        ReferenceManager.addReferenceList(project.References);

        _currentProject = project;


    }

    export function renameDocument() { return null; }

    export function createDocument() { }
}

