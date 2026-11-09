import axios, {AxiosHeaders, AxiosResponse} from "axios";
import {authHeader} from "@shared/services/auth.service.ts";
import {InputTheme, Theme} from "../../entities/Theme.ts";
import {setRefreshToken, setJWTToken} from "@shared/services/localStorage.service.ts";
import {refreshToken} from "@shared/services/auth.service.ts";
import {InputPractice} from "@/entities/Practice.ts";
import {InputUser, User } from "@/entities/User.ts";
import {GoalsTasks} from "@/entities/GoalsTasks";
import { Repository } from "@/entities/Repository.ts";
import {Report} from "../../entities/Report";
import {Comment} from "../../entities/Comment";
import { BASENAME } from "@/app/routes/routes.tsx";
import {Message} from "../../entities/Message";
import {Feedback, FeedbackUploadInput } from "@/entities/Feedback.ts";
import { TextWork, TextWorkUploadInput } from "@/entities/TextWork.ts";
import {Presentation, PresentationUploadInput } from "@/entities/Presentation.ts";
import {ForgotPassword} from "@/entities/ForgotPassword";
import {ResetPassword} from "@/entities/ResetPassword";
import {ConfirmEmail} from "@/entities/ConfirmEmail";
import {ResendConfirmation} from "../../entities/ResendConfirmation";
import {Agent, AnalyzeDocumentInput} from "@/entities/DocumentCheck";
import {Consultant} from "@/entities/Consultant";
import {Group} from "@/entities/Group";
import {Lecturer} from "@/entities/Lecturer";
import {RegisterData} from "@/entities/RegisterData";
import {Student} from "@/entities/Student";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export const axiosService = axios.create({
    baseURL: API_BASE_URL,
    headers: undefined,
    data: undefined
});

export const axiosPublic = axios.create({
    baseURL: API_BASE_URL,
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
            config.headers = {
                ...(config.headers || {}),
                ...authHeader(),
            } as AxiosHeaders;

        }
        return config;
    });

/// Expired token and unauthorized handler
axiosService.interceptors.response.use(
    (response) => response,
    async (error) => {
        const loginUrl = `${BASENAME}login`;
        try {
            if (error.response.status === 401) {
                if (error.config.url === "/refresh") {
                    setJWTToken("");
                    setRefreshToken("");
                    window.location.href = loginUrl;
                    return Promise.reject(error);
                }

                await refreshToken();
                return axiosService(error.config);
            }
            return Promise.reject(error);
        } catch {
            window.location.href = loginUrl;
            return Promise.reject(error);
        }
    }
);

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
export const getTheme = (id: number) => axiosService.get(`core-api/themes/${id}`)

export const postTheme = (inputTheme: InputTheme) => axiosService.post("core-api/themes", inputTheme)

export const putTheme = (theme: Partial<Theme> & { id: number }) => axiosService.put("core-api/themes", theme)

export const getLecturers = () => axiosService.get("core-api/lecturers")
export const getConsultants = () => axiosService.get("core-api/consultants")

export const getPractices = () => axiosService.get("core-api/practices")

export const getPractice = (id: number) => axiosService.get(`core-api/practices/${id}`)

export const deletePractice = (id: number) => axiosService.delete(`core-api/practices/${id}`)


export const getUserPractices = (userId: string) => axiosService.get(`core-api/practices/student?userId=${userId}`)

export const getSupervisorPractices = (userId: string) => axiosService.get(`core-api/practices/supervisor?userId=${userId}`)

export const postPractice = (inputPractice: InputPractice) => axiosService.post("core-api/practices", inputPractice)

export const putPractice = (practice: InputPractice) => axiosService.put("core-api/practices", practice)

export const getStudentByUserId = (userId: string) => axiosService.get(`core-api/students/byUserId?userId=${userId}`)

export const getConsultantByUserId = (userId: string) => axiosService.get(`core-api/consultants/byUserId?userId=${userId}`)

export const getLecturerByUserId = (userId: string) => axiosService.get(`core-api/lecturers/byUserId?userId=${userId}`)

export const getAllUsers = () => axiosService.get(`auth-api/users`)
// The profile is needed by the header and by almost every page: one request is shared for a short time.
let meRequest: Promise<AxiosResponse<User>> | null = null;
let meRequestedAt = 0;
const ME_CACHE_MS = 30_000;

const sharedMe = (request: () => Promise<AxiosResponse<User>>) => {
    if (!meRequest || Date.now() - meRequestedAt > ME_CACHE_MS) {
        meRequestedAt = Date.now();
        const current = request();
        // A failed request must not be remembered, the next call asks again.
        current.catch(() => {
            if (meRequest === current) meRequest = null;
        });
        meRequest = current;
    }
    return meRequest;
};

/// Forget the remembered profile: after sign-in, registration and after the profile was changed.
export const clearMeCache = () => {
    meRequest = null;
};

export const getMe = () => sharedMe(() => axiosService.get<User>("core-api/me"))

export const getMePublic = () => sharedMe(() => axiosPublic.get<User>("core-api/me"))

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
export const updateLecturer = (lecturer: Lecturer) => axiosService.put<Lecturer>(`core-api/lecturers`, lecturer);
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
    axiosService.put<Group>(`core-api/groups`, group);
export const deleteGroup = (id: number) =>
    axiosService.delete(`core-api/groups/${id}`);


/// A request for a record that may not exist yet: the server answers 204 when there is none, the result is then null.
const getOptional = async <T>(url: string): Promise<T | null> => {
    const response = await axiosService.get<T | "">(url);
    return response.status === 204 || response.data === "" ? null : (response.data as T);
};

export const postRepository = (inputRepository: Repository) =>
    axiosService.post("practice-entities-api/repositories", inputRepository);

export const getRepositoryByPracticeId = (practiceId: number) =>
    getOptional<Repository>(`practice-entities-api/repositories/${practiceId}`);

export const postReport = (inputReport: Report) =>
    axiosService.post("practice-entities-api/reports", inputReport);

export const getReportsByPracticeId = (practiceId: number) =>
    axiosService.get(`practice-entities-api/reports/${practiceId}`);

export const postCommentToReport = (id: string, inputComment: Comment) =>
    axiosService.post(`practice-entities-api/reports/${id}/comments`, inputComment);

export const postGoalsTasks = (inputGoalsTasks: GoalsTasks) =>
    axiosService.post("practice-entities-api/goals-tasks", inputGoalsTasks);

export const getGoalsTasksByPracticeId = (practiceId: number) =>
    getOptional<GoalsTasks>(`practice-entities-api/goals-tasks/${practiceId}`);

export const postMessage = (input: Message) =>
    axiosService.post("practice-entities-api/messages", input);

export const getMessagesByPracticeId = (practiceId: number) =>
    axiosService.get(`practice-entities-api/messages/${practiceId}`);

export const postFeedback = (input: FeedbackUploadInput) => {
    const formData = new FormData();
    formData.append("PracticeId", input.practiceId.toString());
    formData.append("FeedbackType", input.feedbackType);
    if (input.file) {
        formData.append("File", input.file);
    }
    
    return axiosService.post("practice-entities-api/feedbacks", formData);
};

export const getFeedbacksByPracticeId = (practiceId: number) =>
    axiosService.get<Feedback[]>(`practice-entities-api/feedbacks/${practiceId}`);

export const postTextWork = (input: TextWorkUploadInput) => {
    const formData = new FormData();
    formData.append("PracticeId", input.practiceId.toString());
    formData.append("Link", input.link);
    formData.append("Version", input.version.toString());
    if (input.file) {
        formData.append("File", input.file);
    }

    return axiosService.post("practice-entities-api/text-works", formData);
};

export const getTextWorksByPracticeId = (practiceId: number) =>
    axiosService.get<TextWork[]>(`practice-entities-api/text-works/${practiceId}`);

export const postTextWorkComment = (textWorkId: string, input: Comment) =>
    axiosService.post(`/practice-entities-api/text-works/${textWorkId}/comments`, input);

export const getLatestTextWorkVersion = (practiceId: number) =>
    getOptional<TextWork>(`practice-entities-api/text-works/latest/${practiceId}`);

export const postPresentation = (input: PresentationUploadInput) => {
    const formData = new FormData();
    formData.append("PracticeId", input.practiceId.toString());
    formData.append("Link", input.link);
    formData.append("Version", input.version.toString());
    if (input.file) {
        formData.append("File", input.file);
    }

    return axiosService.post("practice-entities-api/presentations", formData);
};

export const getPresentationsByPracticeId = (practiceId: number) =>
    axiosService.get<Presentation[]>(`practice-entities-api/presentations/${practiceId}`);

export const getLatestPresentationVersion = (practiceId: number) =>
    getOptional<Presentation>(`practice-entities-api/presentations/latest/${practiceId}`);

export const postPresentationComment = (presentationId: string, input: Comment) =>
    axiosService.post(`/practice-entities-api/presentations/${presentationId}/comments`, input);

export const postForgotPassword = (input: ForgotPassword) =>
    axiosService.post(`/auth-api/forgot-password`, input);
export const resetPassword = (input: ResetPassword) =>
    axiosService.post(`/auth-api/reset-password`, input);
export const confirmEmail = (input: ConfirmEmail) =>
    axiosService.post(`/auth-api/confirm-email`, input);
export const resendConfirmation = (input: ResendConfirmation) =>
    axiosService.post(`/auth-api/resend-confirmation`, input);

export const getLlmAgents = () =>
    axiosService.get<Agent[]>("llm-api/api/analysis/agents");

/// Returns the PDF: the original with highlights ("annotate") or a summary report ("report").
export const analyzeDocument = (input: AnalyzeDocumentInput) => {
    const formData = new FormData();
    formData.append("File", input.file);
    formData.append("DocumentType", input.documentType);
    formData.append("Agents", input.agents.join(","));
    formData.append("Annotate", String(input.outputMode === "annotate"));
    formData.append("GeneratePdf", String(input.outputMode === "report"));

    return axiosService.post<Blob>("llm-api/api/analysis/analyze", formData, {
        responseType: "blob",
    });
};