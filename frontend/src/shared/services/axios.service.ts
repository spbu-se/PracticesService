import axios, {AxiosHeaders} from "axios";
import {authHeader} from "@shared/services/auth.service.ts";

// Axios service for API requesting
export const axiosService = axios.create({
    baseURL: "http://localhost:5000/",
    headers: undefined,
    data: undefined
});

/// Config for axios requests
axiosService.interceptors.request
    .use(function (config) {
        if (config.url?.includes("api/")) {
            config.headers = {...authHeader()} as AxiosHeaders
        }
        return config;
    });

/// Expired token and unauthorized handler
axiosService.interceptors.response
    .use(function (response) {
        return response;
    }, async function () {
        const loginUrl = "/login"
        window.location.assign(loginUrl);
        return;
    });

export const login = (email: string, password: string) => axiosService.post(`auth-api/login`, {email: email, password: password})

export const getThemes = () => axiosService.get("core-api/themes")