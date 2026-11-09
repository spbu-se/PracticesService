import axios from "axios";

/// Text of an error answer of the server (the "message" field), or the fallback when there is none.
export function getErrorMessage(error: unknown, fallback: string): string {
    if (axios.isAxiosError(error)) {
        const data = error.response?.data;
        if (typeof data === "string" && data.length > 0 && data.length < 300) {
            return data;
        }
        if (data && typeof data === "object" && "message" in data && typeof data.message === "string") {
            return data.message;
        }
    }
    return fallback;
}
