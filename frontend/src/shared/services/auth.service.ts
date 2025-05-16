import {
    getJWTToken,
    getRefreshToken,
    setJWTToken,
    setRefreshToken
} from "@shared/services/localStorage.service.ts";
import {axiosService} from "@shared/services/axios.service.ts";

/// Header with access token for axios requests
export const authHeader = () => {
    const token = getJWTToken();

    if (token) {
        return {Authorization: 'Bearer ' + token, "Content-Type": "application/json"};
    } else {
        return {};
    }
}

/// Refresh expired access token
export const refreshToken = async () => {
    const refresh = getRefreshToken()
    const access = getJWTToken()
    const response = await axiosService
        .post("auth-api/refresh/", {refreshToken: refresh, token: access});
    if (response.data.token && response.data.refreshToken) {
        setJWTToken(response.data.token)
        setRefreshToken(response.data.refreshToken)
    }
}

export const logout = () => {
    setJWTToken("")
    setRefreshToken("")
    window.location.assign("/login");
}