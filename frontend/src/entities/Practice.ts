import { Student } from "./Student.ts";
import { Theme } from "./Theme.ts";
import { Consultant } from "./Consultant.ts";
import { Lecturer } from "./Lecturer.ts";

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
    supervisor: Lecturer;
    consultant: Consultant;
}

export interface InputPractice {
    studentid: number;
    themeid: number;
    type: string;
    finalgrade: string;
    status: string;
}