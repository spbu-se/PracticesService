import {Comment} from "./Comment.ts";

export interface Report {
    id?: string;
    practiceId: number;
    done: string;
    planned: string;
    comments: Comment[];
    createdAt?: Date;
}
