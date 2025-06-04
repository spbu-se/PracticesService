import {Comment} from "./Comment";

export interface Presentation {
    id?: string;
    practiceId: number;
    fileName: string;
    link: string;
    version: number;
    uploadedAt: string;
    comments?: Comment[];
}

export interface PresentationUploadInput {
    practiceId: number;
    link: string;
    version: number;
    file?: File;
}
