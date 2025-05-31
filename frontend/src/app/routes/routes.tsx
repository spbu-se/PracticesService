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
        path: "/practice/:id",
        element: <PracticePage/>,
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
]);
