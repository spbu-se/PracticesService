import {Group} from "./Group.ts";

export interface Student {
    id: number;
    userid: string;
    firstName: string;
    lastName: string;
    middleName: string;
    groupId: number;
    group: Group
}