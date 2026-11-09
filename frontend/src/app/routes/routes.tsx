import {ComponentType} from "react";
import {createBrowserRouter} from "react-router-dom";

type PageModule = Record<string, unknown>;

const loaders: Array<() => Promise<PageModule>> = [];

/// A page that is loaded when it is opened. Every page is also remembered, so that preloadPages can fetch them in advance.
const lazyPage = (loader: () => Promise<PageModule>, name: string) => {
    loaders.push(loader);
    return () => loader().then((module) => ({ Component: module[name] as ComponentType }));
};

/// Fetches the code of all pages while the browser is idle: a click on a menu item then shows the page at once.
export const preloadPages = () => {
    let index = 0;
    const next = () => {
        if (index >= loaders.length) return;
        loaders[index++]().catch(() => undefined);
        setTimeout(next, 150);
    };
    next();
};

export const BASENAME = "/practices-service/";

export const routes = createBrowserRouter([
    {
        path: "/",
        lazy: lazyPage(() => import("@pages/BasePage.tsx"), "BasePage"),
    },
    {
        path: "/themes",
        lazy: lazyPage(() => import("@pages/Themes/ThemesIndexPage.tsx"), "ThemesIndexPage"),
    },
    {
        path: "/practices",
        lazy: lazyPage(() => import("@pages/Practices/PracticesIndexPage.tsx"), "PracticesIndexPage"),
    },
    {
        path: "/document-check",
        lazy: lazyPage(() => import("@/pages/DocumentCheck/DocumentCheckPage"), "DocumentCheckPage"),
    },
    {
        path: "/practices-staff",
        lazy: lazyPage(() => import("@/pages/Practices/SupervisorPracticesPage"), "SupervisorPracticesPage"),
    },
    {
        path: "/practices-manager",
        lazy: lazyPage(() => import("@/pages/Practices/ManagerPracticesPage"), "ManagerPracticesPage"),
    },
    {
        path: "/practice/:id",
        lazy: lazyPage(() => import("@/pages/Practices/PracticePage"), "PracticePage"),
    },
    {
        path: "/practice-staff/:id",
        lazy: lazyPage(() => import("@/pages/Practices/PracticeStaffPage"), "PracticeStaffPage"),
    },
    {
        path: "/create/practice",
        lazy: lazyPage(() => import("@pages/Practices/CreatePracticePage.tsx"), "CreatePracticePage"),
    },
    {
        path: "/login",
        lazy: lazyPage(() => import("@pages/LoginPage.tsx"), "LoginPage"),
    },
    {
        path: "/register",
        lazy: lazyPage(() => import("@/pages/RegisterPage"), "RegisterPage"),
    },
    {
        path: "/forgot-password",
        lazy: lazyPage(() => import("@/pages/ForgotPasswordPage"), "ForgotPasswordPage"),
    },
    {
        path: "/reset-password",
        lazy: lazyPage(() => import("@/pages/ResetPasswordPage"), "ResetPasswordPage"),
    },
    {
        path: "/confirm-email",
        lazy: lazyPage(() => import("@/pages/ConfirmEmailPage"), "ConfirmEmailPage"),
    },
    {
        path: "/resend-confirmation",
        lazy: lazyPage(() => import("@/pages/ResendConfirmationPage"), "ResendConfirmationPage"),
    },
    {
        path: "/theme/:id",
        lazy: lazyPage(() => import("@pages/Themes/ThemePage.tsx"), "ThemePage"),
    },
    {
        path: "/create/theme",
        lazy: lazyPage(() => import("@pages/Themes/CreateThemePage.tsx"), "CreateThemePage"),
    },
    {
        path: "/edit/theme/:id",
        lazy: lazyPage(() => import("@pages/Themes/EditThemePage.tsx"), "EditThemePage"),
    },
    {
        path: "/profile",
        lazy: lazyPage(() => import("@pages/ProfilePage.tsx"), "ProfilePage"),
    },
    {
        path: "/admin",
        lazy: lazyPage(() => import("@pages/Admin/AdminBasePage"), "AdminBasePage"),
        children: [
            {
                path: "users",
                lazy: lazyPage(() => import("@pages/Admin/AdminUsersPage"), "AdminUsersPage"),
            },
            {
                path: "consultants",
                lazy: lazyPage(() => import("@pages/Admin/AdminConsultantsPage"), "AdminConsultantsPage"),
            },
            {
                path: "lecturers",
                lazy: lazyPage(() => import("@pages/Admin/AdminLecturersPage"), "AdminLecturersPage"),
            },
            {
                path: "students",
                lazy: lazyPage(() => import("@pages/Admin/AdminStudentsPage"), "AdminStudentsPage"),
            },
            {
                path: "groups",
                lazy: lazyPage(() => import("@pages/Admin/AdminGroupsPage"), "AdminGroupsPage"),
            },
        ],
    },
], {basename: BASENAME});
