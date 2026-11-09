export interface Agent {
    name: string;
    title: string;
    description: string;
}

export type DocumentType = "report" | "presentation" | "feedback";

export type OutputMode = "annotate" | "report";

export interface AnalyzeDocumentInput {
    file: File;
    documentType: DocumentType;
    agents: string[];
    outputMode: OutputMode;
}
