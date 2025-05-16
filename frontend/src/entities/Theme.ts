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
    consultantid: number;
    supervisorid: number;
    createddate: Date;
    updateddate: Date;
    consultant: Consultant;
    supervisor: Lecturer;
}

export interface InputTheme {
    title: string;
    description: string;
    tags: string;
    level: string;
    department: string;
    isarchived: boolean;
    suggestedby: string;
    consultantid: number;
    supervisorid: number;
}