/// <reference path="vendor/jquery.d.ts" />
/// <reference path="Editor.ts" />
/// <reference path="ProjectManager.ts" />

module Compilify {
    var global = (new Function('return this'))(),
        $ = <JQueryStatic>global.jQuery,
        
        _isInitialized = false;

    export interface IProjectState {
        Id: string;
        Documents: IDocumentState[];
        References: IReferenceState[];
    }

    export interface IDocumentState {
        Name: string;
        Content: string;
        LastEdited: Date;
    }

    export interface IReferenceState {
        Name: string;
        Version: string;
        IsRemovable: bool;
    }

    export interface IWorkspaceState {
        Project: IProjectState;
    }

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
