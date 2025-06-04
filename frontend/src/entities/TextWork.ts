import {Comment} from "./Comment.ts";

export interface TextWork {
    id: string;
    practiceId: number;
    fileName: string;
    comments: Comment[];
    link: string;
    version: number;
    uploadedAt: string;
}

export interface TextWorkUploadInput {
    practiceId: number;
    link: string;
    version: number;
    file?: File;
}