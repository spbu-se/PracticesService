import {Lecturer} from "./Lecturer.ts";
import {Consultant} from "./Consultant.ts";

export interface Theme {
    id: number;
    title: string;
    description: string;
    tags: string;
    level: string;
    department: string;
    isarchived: boolean;
    suggestedby: string;
    source: string;
    consultantid: number | null;
    supervisorid: number | null;
    createddate: Date;
    updateddate: Date;
    consultant: Consultant | null;
    supervisor: Lecturer | null;
}

export interface InputTheme {
    title: string;
    description: string;
    tags?: string;
    level: string;
    department: string;
    isarchived?: boolean;
    source: string;
    suggestedby: string;
    consultantid: number | null;
    supervisorid: number | null;
}