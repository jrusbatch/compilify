/// <reference path="vendor/jquery.d.ts" />
/// <reference path="Editor.ts" />
/// <reference path="ProjectManager.ts" />

declare var CodeMirror: any;

interface IProjectState {
    Id: string;
    Documents: IDocumentState[];
    References: IReferenceState[];
}

interface IDocumentState {
    Name: string;
    Content: string;
    LastEdited: Date;
}

interface IReferenceState {
    Name: string;
    Version: string;
    IsRemovable: bool;
}

interface IWorkspaceState {
    Project: IProjectState;
}

module Compilify {
    var global = (new Function('return this'))(),
        $ = <JQueryStatic>global.jQuery,
        
        _isInitialized = false;

    export function initializeWorkspace(state: IWorkspaceState) {
        if (_isInitialized) {
            throw new Error('Workspace has already been initialized.');
        }

        _isInitialized = true;

        $(function() {
            ProjectManager.openProject(state.Project);
        });
    }
}
