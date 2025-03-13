import {createBrowserRouter} from "react-router-dom";
import {LoginPage} from "@pages/LoginPage.tsx";
import {BasePage} from "@pages/BasePage.tsx";

// Create routes
export const routes = createBrowserRouter([
    {
        path: "/",
        element: <BasePage/>
    },
    {
        path: "/login",
        element: <LoginPage/>
    }
])