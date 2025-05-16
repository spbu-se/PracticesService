import {UserRole} from "./UserRoles";

export interface User {
    userId: string;
    email: string;
    userName: string;
    firstName: string;
    lastName: string;
    middleName: string;
    roles: UserRole[];
}