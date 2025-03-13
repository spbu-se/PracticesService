export interface Theme {
    id: number;
    title: string;
    description: string;
    tags: string;
    level: string;
    department: string;
    isarchived: boolean;
    suggestedby: string;
    consultantid: number;
    supervisorid: number;
    createddate: Date;
    updateddate: Date;
}