import { Student } from "./Student.ts";
import { Theme } from "./Theme.ts";

export interface Practice {
    id: number;
    studentid: number;
    themeid: 0;
    type: string;
    finalgrade: string;
    status: string;
    createddate: Date;
    updateddate: Date;
    student: Student;
    theme: Theme;
}

export interface InputPractice {
    studentid: number;
    themeid: number;
    type: string;
    finalgrade: string;
    status: string;
}