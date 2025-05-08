import { Outlet, useLocation, useNavigate } from "react-router-dom";
import {
    AppBar,
    Box,
    Button,
    Container,
    CssBaseline,
    Tab,
    Tabs,
    Toolbar,
    Typography
} from "@mui/material";
import { useEffect, useState } from "react";
import { getMe } from "@/shared/services/axios.service";
import { User } from "@/entities/User";

const tabRoutes = [
    { label: "Панель", path: "/admin" },
    { label: "Пользователи", path: "/admin/users" },
    { label: "Консультанты", path: "/admin/" },
    { label: "Преподаватели", path: "/admin/teachers" },
    { label: "Студенты", path: "/admin/students" },
    { label: "Темы", path: "/admin/themes" },
    { label: "Практики", path: "/admin/practices" }
];

export function AdminBasePage() {
    const navigate = useNavigate();
    const location = useLocation();
    const [selectedTab, setSelectedTab] = useState(0);

    useEffect(() => {
        getMe().then(res => {
            const user: User = res.data;
            if (!user.roles.includes("Администратор")) {
                navigate("/");
            }
        });
    }, [navigate]);

    useEffect(() => {
        const currentIndex = tabRoutes.findIndex(tab => location.pathname.startsWith(tab.path));
        if (currentIndex !== -1) setSelectedTab(currentIndex);
    }, [location.pathname]);

    const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
        setSelectedTab(newValue);
        navigate(tabRoutes[newValue].path);
    };

    return (
        <Box sx={{ display: "flex", flexDirection: "column", height: "100vh" }}>
            <CssBaseline />
            
            <AppBar position="static">
                <Toolbar>
                    <Typography variant="h6" sx={{ flexGrow: 1 }}>
                        Панель администратора
                    </Typography>
                    <Button color="inherit" onClick={() => navigate("/")}>
                        На главную
                    </Button>
                </Toolbar>
            </AppBar>
            
            <Tabs value={selectedTab} onChange={handleTabChange} variant="scrollable" scrollButtons="auto">
                {tabRoutes.map((tab, index) => (
                    <Tab key={index} label={tab.label} />
                ))}
            </Tabs>
            
            <Box component="main" sx={{ flexGrow: 1, p: 3, overflow: "auto" }}>
                <Container maxWidth="lg">
                    <Outlet />
                </Container>
            </Box>
        </Box>
    );
}
