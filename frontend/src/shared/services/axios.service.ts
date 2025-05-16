import axios, {AxiosHeaders} from "axios";
import {authHeader} from "@shared/services/auth.service.ts";
import {InputTheme, Theme} from "../../entities/Theme.ts";
import {setRefreshToken, setJWTToken} from "@shared/services/localStorage.service.ts";
import {refreshToken} from "@shared/services/auth.service.ts";
import {InputPractice, Practice } from "@/entities/Practice.ts";

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
        const loginUrl = "/login"
        try {
            if (error.response.status === 401) {
                if (error.config.url === "/refresh") {
                    setJWTToken("");
                    setRefreshToken("");
                    window.location.assign(loginUrl);
                    return Promise.reject(error);
                }

                await refreshToken()
                return axiosService(error.config);
            }
            return Promise.reject(error);
        } catch {
            window.location.assign(loginUrl);
            return;
        }
    });

export const login = (email: string, password: string) => axiosService.post(`auth-api/login`, {
    email: email,
    password: password
})

export const register = (data: RegisterData) => axiosService.post(`auth-api/register`, {
    email: data.email,
    password: data.password,
    firstName: data.firstName,
    lastName: data.lastName,
    middleName: data.middleName, 
    roles: data.roles
})

export const getThemes = () => axiosService.get("core-api/themes")

export const getTheme = (id: number) => axiosService.get(`core-api/themes?id=${id}`)

export const postTheme = (inputTheme: InputTheme) => axiosService.post("core-api/themes", inputTheme)

export const putTheme = (theme: Theme) => axiosService.put("core-api/themes", theme)

export const getLecturers = () => axiosService.get("core-api/lecturers")
export const getConsultants = () => axiosService.get("core-api/consultants")

export const getPractices = () => axiosService.get("core-api/practices")

export const getUserPractices = (userId: string) => axiosService.get(`core-api/practices/query?userId=${userId}`)

export const postPractice = (inputPractice: Practice) => axiosService.post("core-api/practices", inputPractice)

export const putPractice = (practice: InputPractice) => axiosService.put("core-api/practices", practice)

export const getStudentByUserId = (userId: string) => axiosService.get(`core-api/students/byUserId?userId=${userId}`)

export const getAllUsers = () => axiosService.get(`auth-api/users`)
export const getMe = () => axiosService.get("core-api/me")