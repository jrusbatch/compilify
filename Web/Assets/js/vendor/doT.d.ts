interface IDotSettings {
    evaluate: RegExp;
    interpolate: RegExp;
    encode: RegExp;
    use: RegExp;
    define: RegExp;
    conditional: RegExp;
    iterate: RegExp;
    varname: string;
    strip: bool;
    append: bool;
    selfcontained: bool;
}

interface DotStatic {
    version: string;
    templateSettings: IDotSettings;
    template(templateText: string, settings?: IDotSettings): (data: any) => string;
}

declare var doT: DotStatic;