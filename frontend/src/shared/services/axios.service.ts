import axios, {AxiosHeaders} from "axios";
import {authHeader} from "@shared/services/auth.service.ts";
import {InputTheme, Theme} from "../../entities/Theme.ts";

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
    }, async function (error) {
        if (error.response && error.response.status === 401) {
            const loginUrl = "/login";
            window.location.assign(loginUrl);
        }
        return Promise.reject(error);
    });

export const login = (email: string, password: string) => axiosService.post(`auth-api/login`, {
    email: email,
    password: password
})

export const getThemes = () => axiosService.get("core-api/themes")

export const postTheme = (inputTheme: InputTheme) => axiosService.post("core-api/themes", inputTheme)

export const putTheme = (theme: Theme) => axiosService.put("core-api/themes", theme)

export const getLecturers = () => axiosService.get("core-api/lecturers")
export const getConsultants = () => axiosService.get("core-api/consultants")

export const getMe = () => axiosService.get("core-api/me")