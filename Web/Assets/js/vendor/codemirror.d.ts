
declare class CodeMirror {
    static fromTextArea(textarea: HTMLTextAreaElement, options?: any): CodeMirror;

    static commands: any;

    constructor(container: JQuery, options: any);

    setOption(option: string, value: any): void;

    getWrapperElement(): HTMLDivElement;

    refresh(): void;

    getScrollerElement(): HTMLElement;
}
