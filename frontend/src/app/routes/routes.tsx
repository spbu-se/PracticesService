import {createBrowserRouter} from "react-router-dom";
import {LoginPage} from "@pages/LoginPage.tsx";
import {BasePage} from "@pages/BasePage.tsx";
import {ThemePage} from "@pages/ThemePage.tsx";
import {CreateThemePage} from "@pages/CreateThemePage.tsx";
import {EditThemePage} from "@pages/EditThemePage.tsx";
import {ProfilePage} from "@pages/ProfilePage.tsx";

export const routes = createBrowserRouter([
    {
        path: "/",
        element: <BasePage/>,
    },
    {
        path: "/login",
        element: <LoginPage/>,
    },
    {
        path: "/theme/:id",
        element: <ThemePage/>,
    },
    {
        path: "/createTheme",
        element: <CreateThemePage/>,
    },
    {
        path: "/editTheme/:id",
        element: <EditThemePage/>,
    },
    {
        path: "/profile",
        element: <ProfilePage/>,
    }
]);
