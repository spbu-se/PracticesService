export interface ResetPassword {
    token: string;
    newPassword: string;
    confirmPassword: string;
    email: string;
}