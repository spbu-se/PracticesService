import { createBrowserRouter } from "react-router-dom";
import { LoginPage } from "@pages/LoginPage.tsx";
import { BasePage } from "@pages/BasePage.tsx";
import { ThemePage } from "@pages/ThemePage.tsx"; // Import ThemePage

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
    }
]);
