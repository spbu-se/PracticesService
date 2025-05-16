import {Group} from "./Group.ts";

export interface Student {
    id: number;
    firstName: string;
    lastName: string;
    middleName: string;
    groupId: number;
    group: Group
}