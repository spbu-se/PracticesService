export interface Lecturer {
    id: number;
    userid: string | null;
    firstName: string;
    lastName: string;
    middleName: string;
    department: string;
    cansupervisevkr: boolean;
}