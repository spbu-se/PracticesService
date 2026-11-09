import { Student } from "./Student.ts";
import { Theme } from "./Theme.ts";
import { Consultant } from "./Consultant.ts";
import { Lecturer } from "./Lecturer.ts";

export interface Practice {
    id: number;
    studentid: number;
    themeid: number;
    supervisorid: number;
    consultantid: number | null;
    type: string;
    finalgrade: string | null;
    status: string;
    createddate: Date;
    updateddate: Date;
    student: Student;
    theme: Theme;
    supervisor: Lecturer;
    consultant: Consultant | null;
}

export interface InputPractice {
    id?: number;
    studentid: number;
    themeid: number;
    supervisorid: number;
    consultantid: number | null;
    type: string;
    finalgrade: string | null;
    status: string;
}