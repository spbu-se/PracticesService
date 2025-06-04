export interface Feedback {
    id?: string;
    practiceId: number;
    fileName: string;
    link?: string;
    uploadedAt: string; 
    feedbackType: string;
}

export interface FeedbackUploadInput {
    practiceId: number;
    feedbackType: string;
    file?: File;
}

