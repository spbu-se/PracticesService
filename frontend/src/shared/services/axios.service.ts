import axios, {AxiosHeaders} from "axios";
import {authHeader} from "@shared/services/auth.service.ts";
import {InputTheme, Theme} from "../../entities/Theme.ts";
import {setRefreshToken, setJWTToken} from "@shared/services/localStorage.service.ts";
import {refreshToken} from "@shared/services/auth.service.ts";
import {InputPractice, Practice } from "@/entities/Practice.ts";
import {InputUser, User } from "@/entities/User.ts";

// Axios service for API requesting
export const axiosService = axios.create({
    baseURL: "http://localhost:5000/",
    headers: undefined,
    data: undefined
});

export const axiosPublic = axios.create({
    baseURL: "http://localhost:5000/",
    data: undefined,
    headers: undefined
});

axiosPublic.interceptors.request
    .use(function (config) {
        if (config.url?.includes("api/")) {
            config.headers = {...authHeader()} as AxiosHeaders
        }
        return config;
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

export const getThemesPublic = () => axiosPublic.get("core-api/themes")
export const getTheme = (id: number) => axiosService.get(`core-api/themes?id=${id}`)

export const postTheme = (inputTheme: InputTheme) => axiosService.post("core-api/themes", inputTheme)

export const putTheme = (theme: Theme) => axiosService.put("core-api/themes", theme)

export const getLecturers = () => axiosService.get("core-api/lecturers")
export const getConsultants = () => axiosService.get("core-api/consultants")

export const getPractices = () => axiosService.get("core-api/practices")

export const getPractice = (id: number) => axiosService.get(`core-api/practices?id=${id}`)

export const getUserPractices = (userId: string) => axiosService.get(`core-api/practices/query?userId=${userId}`)

export const postPractice = (inputPractice: Practice) => axiosService.post("core-api/practices", inputPractice)

export const putPractice = (practice: InputPractice) => axiosService.put("core-api/practices", practice)

export const getStudentByUserId = (userId: string) => axiosService.get(`core-api/students/byUserId?userId=${userId}`)

export const getConsultantByUserId = (userId: string) => axiosService.get(`core-api/consultants/byUserId?userId=${userId}`)

export const getLecturerByUserId = (userId: string) => axiosService.get(`core-api/lecturers/byUserId?userId=${userId}`)

export const getAllUsers = () => axiosService.get(`auth-api/users`)
export const getMe = () => axiosService.get("core-api/me")

export const getMePublic = () => axiosPublic.get("core-api/me")

export const createUser = (data: User) => axiosService.post('auth-api/register',
    {
        email: data.email,
        password: data.password,
        firstName: data.firstName,
        lastName: data.lastName,
        middleName: data.middleName,
        roles: data.roles
    });
export const updateUser = (data: InputUser) => axiosService.put(`auth-api/users/${data.userId}`,
    {
        email: data.email,
        firstName: data.firstName,
        lastName: data.lastName,
        middleName: data.middleName,
        roles: data.roles
    });
export const deleteUser = (userId: string) => axiosService.delete(`auth-api/users/${userId}`);

export const getAllConsultants = () => axiosService.get<Consultant[]>('core-api/consultants');
export const createConsultant = (consultant: Omit<Consultant, 'id'>) => axiosService.post<Consultant>('core-api/consultants', consultant);
export const updateConsultant = (consultant: Consultant) => axiosService.put<Consultant>(`core-api/consultants`, consultant);
export const deleteConsultant = (id: number) => axiosService.delete(`core-api/consultants/${id}`);

export const getAllLecturers = () => axiosService.get("core-api/lecturers");
export const createLecturer = (lecturer: Omit<Lecturer, 'id'>) => axiosService.post<Lecturer>('core-api/lecturers', lecturer);
export const updateLecturer = (lecturer: Lecturer) => axiosService.put<Lecturer>(`core-api/lecturers/${lecturer.id}`, lecturer);
export const deleteLecturer = (id: number) => axiosService.delete(`core-api/lecturers/${id}`);

export const getAllStudents = () => axiosService.get<Student[]>('core-api/students');
export const createStudent = (student: Omit<Student, 'id'>) =>
    axiosService.post<Student>('core-api/students', student);
export const updateStudent = (student: Student) =>
    axiosService.put<Student>(`core-api/students`, student);
export const deleteStudent = (id: number) =>
    axiosService.delete(`core-api/students/${id}`);
export const getAllGroups = () =>
    axiosService.get<Group[]>('core-api/groups');
export const createGroup = (group: Omit<Group, 'id'>) =>
    axiosService.post<Group>('core-api/groups', group);
export const updateGroup = (group: Group) =>
    axiosService.put<Group>(`core-api/groups/${group.id}`, group);
export const deleteGroup = (id: number) =>
    axiosService.delete(`core-api/groups/${id}`);