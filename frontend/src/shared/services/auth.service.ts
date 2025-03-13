import {
    getJWTToken,
    setJWTToken
} from "@shared/services/localStorage.service.ts";

/// Header with access token for axios requests
export const authHeader = () => {
    const token = getJWTToken();

    if (token) {
        return {Authorization: 'Bearer ' + token, "Content-Type": "application/json"};
    } else {
        return {};
    }
}

export const logout = () => {
    setJWTToken("")
    window.location.assign("/login");
}