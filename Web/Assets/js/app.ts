/// <reference path="vendor/jquery.d.ts" />
/// <reference path="Editor.ts" />
/// <reference path="ProjectManager.ts" />
/// <reference path="Events.ts" />

module Compilify {
    var global = (new Function('return this')()),
        $ = <JQueryStatic>global.jQuery,
        
        _isInitialized = false;

    export interface IDisposable {
        dispose(): void;
    }

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

    export function init(state: IWorkspaceState) {
        if (_isInitialized) {
            throw new Error('Workspace has already been initialized.');
        }

        _isInitialized = true;

        $(function() {
            ProjectManager.openProject(state.Project);
        });
    }
}
