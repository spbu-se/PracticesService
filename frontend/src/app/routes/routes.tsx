import { createBrowserRouter } from "react-router-dom";
import { LoginPage } from "@pages/LoginPage.tsx";
import { BasePage } from "@pages/BasePage.tsx";
import { ThemePage } from "@pages/ThemePage.tsx";
import {CreateThemePage} from "@pages/CreateThemePage.tsx";

export const routes = createBrowserRouter([
    {
        path: "/",
        element: <BasePage />,
    },
    {
        path: "/login",
        element: <LoginPage />,
    },
    {
        path: "/theme/:id",
        element: <ThemePage />,
    },
    {
        path: "/createTheme",
        element: <CreateThemePage />,
    }
]);
