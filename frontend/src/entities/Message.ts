export interface Message {
    id?: string;    
    practiceId: number;
    author: string;  
    text: string;      
    createdAt?: Date;    
}
