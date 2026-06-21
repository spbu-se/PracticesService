import {createBrowserRouter} from "react-router-dom";
import {LoginPage} from "@pages/LoginPage.tsx";
import {ThemesIndexPage} from "@pages/Themes/ThemesIndexPage.tsx";
import {ThemePage} from "@pages/Themes/ThemePage.tsx";
import {CreateThemePage} from "@pages/Themes/CreateThemePage.tsx";
import {EditThemePage} from "@pages/Themes/EditThemePage.tsx";
import {ProfilePage} from "@pages/ProfilePage.tsx";
import {BasePage} from "@pages/BasePage.tsx";
import {PracticesIndexPage} from "@pages/Practices/PracticesIndexPage.tsx";
import {CreatePracticePage} from "@pages/Practices/CreatePracticePage.tsx";
import {AdminBasePage} from "@pages/Admin/AdminBasePage";
import {AdminUsersPage} from "@pages/Admin/AdminUsersPage";
import {AdminConsultantsPage} from "@pages/Admin/AdminConsultantsPage";
import {AdminLecturersPage} from "@pages/Admin/AdminLecturersPage";
import {AdminStudentsPage} from "@pages/Admin/AdminStudentsPage";
import {AdminGroupsPage} from "@pages/Admin/AdminGroupsPage";
import { RegisterPage } from "@/pages/RegisterPage";
import { PracticePage } from "@/pages/Practices/PracticePage";
import { SupervisorPracticesPage } from "@/pages/Practices/SupervisorPracticesPage";
import { ManagerPracticesPage } from "@/pages/Practices/ManagerPracticesPage";
import { PracticeStaffPage } from "@/pages/Practices/PracticeStaffPage";
import { ForgotPasswordPage } from "@/pages/ForgotPasswordPage";
import { ResetPasswordPage } from "@/pages/ResetPasswordPage";

export const BASENAME = "/practices-service/";

export const routes = createBrowserRouter([
    {
        path: "/",
        element: <BasePage/>,
    },
    {
        path: "/themes",
        element: <ThemesIndexPage/>,
    },
    {
        path: "/practices",
        element: <PracticesIndexPage/>,
    },
    {
        path: "/practices-staff",
        element: <SupervisorPracticesPage/>,
    },
    {
        path: "/practices-manager",
        element: <ManagerPracticesPage/>,
    },
    {
        path: "/practice/:id",
        element: <PracticePage/>,
    },
    {
        path: "/practice-staff/:id",
        element: <PracticeStaffPage/>,
    },
    {
        path: "/create/practice",
        element: <CreatePracticePage/>,
    },
    {
        path: "/login",
        element: <LoginPage/>,
    },
    {
        path: "/register",
        element: <RegisterPage/>,
    },
    {
        path: "/forgot-password",
        element: <ForgotPasswordPage/>,
    },
    {
        path: "/reset-password",
        element: <ResetPasswordPage/>,
    },
    {
        path: "/theme/:id",
        element: <ThemePage/>,
    },
    {
        path: "/create/theme",
        element: <CreateThemePage/>,
    },
    {
        path: "/edit/theme/:id",
        element: <EditThemePage/>,
    },
    {
        path: "/profile",
        element: <ProfilePage/>,
    },
    {
        path: "/admin",
        element: <AdminBasePage/>,
        children: [
            {
                path: "users",
                element: <AdminUsersPage />,
            },
            {
                path: "consultants",
                element: <AdminConsultantsPage />,
            },
            {
                path: "lecturers",
                element: <AdminLecturersPage />,
            },
            {
                path: "students",
                element: <AdminStudentsPage />,
            },
            {
                path: "groups",
                element: <AdminGroupsPage />,
            },
        ],
    },
], {basename: BASENAME});
