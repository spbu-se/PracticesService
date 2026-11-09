import { useEffect, useState } from "react";
import {
    AppBar,
    Avatar,
    Box,
    Button,
    Container,
    Divider,
    IconButton,
    ListItemText,
    Menu,
    MenuItem,
    Skeleton,
    Toolbar,
    Tooltip,
    Typography,
    useMediaQuery,
    useTheme,
} from "@mui/material";
import { useLocation, useNavigate } from "react-router-dom";
import MenuIcon from "@mui/icons-material/Menu";
import { logout } from "@shared/services/auth.service.ts";
import { getJWTToken } from "@/shared/services/localStorage.service.ts";
import { getMePublic } from "@/shared/services/axios.service.ts";
import { User } from "@/entities/User";
import { UserRole } from "../../../entities/UserRoles";

interface NavItem {
    label: string;
    path: string;
}

/// Sections the user may open. The same list feeds the bar on a wide screen and the menu on a narrow one.
function getNavItems(me: User | undefined): NavItem[] {
    const has = (role: UserRole) => me?.roles?.includes(role) ?? false;
    const items: NavItem[] = [];

    if (has(UserRole.ADMIN)) items.push({ label: "Админ-панель", path: "/admin" });
    items.push({ label: "Темы", path: "/themes" });
    if (me) items.push({ label: "Проверка документов", path: "/document-check" });
    if (has(UserRole.SUPERVISOR)) items.push({ label: "Практики студентов", path: "/practices-staff" });
    if (has(UserRole.PRACTICE_SUPERVISOR)) items.push({ label: "Все практики", path: "/practices-manager" });
    if (has(UserRole.STUDENT) || !me) items.push({ label: "Мои практики", path: "/practices" });

    return items;
}

function initials(me: User | undefined) {
    const letters = `${me?.firstName?.[0] ?? ""}${me?.lastName?.[0] ?? ""}`.toUpperCase();
    return letters || (me?.email?.[0]?.toUpperCase() ?? "?");
}

export default function Header() {
    const navigate = useNavigate();
    const location = useLocation();
    const theme = useTheme();
    // Eight or so sections do not fit in a bar below about 1200px.
    const collapsed = useMediaQuery(theme.breakpoints.down("lg"));
    const [navAnchor, setNavAnchor] = useState<null | HTMLElement>(null);
    const [accountAnchor, setAccountAnchor] = useState<null | HTMLElement>(null);
    const [me, setMe] = useState<User>();
    // While the profile is loading the bar must not flash the sign-in button at a signed-in user.
    const [loading, setLoading] = useState(Boolean(getJWTToken()));

    useEffect(() => {
        // Without a token the server would answer 401 and there is nobody to show.
        if (!getJWTToken()) {
            setMe(undefined);
            setLoading(false);
            return;
        }

        getMePublic()
            .then((res) => setMe(res.data))
            .catch(() => setMe(undefined))
            .finally(() => setLoading(false));
    }, [location.pathname]);

    const items = getNavItems(me);
    const isActive = (path: string) => location.pathname === path || location.pathname.startsWith(`${path}/`);

    const go = (path: string) => {
        setNavAnchor(null);
        setAccountAnchor(null);
        navigate(path);
    };

    return (
        <AppBar position="sticky" elevation={1} sx={{ bgcolor: "primary.main", color: "primary.contrastText" }}>
            <Container maxWidth="xl">
                <Toolbar disableGutters sx={{ gap: 1, minHeight: { xs: 56, md: 64 } }}>
                    {collapsed && (
                        <>
                            <IconButton color="inherit" aria-label="Меню" onClick={(e) => setNavAnchor(e.currentTarget)}>
                                <MenuIcon />
                            </IconButton>
                            <Menu anchorEl={navAnchor} open={Boolean(navAnchor)} onClose={() => setNavAnchor(null)}>
                                {items.map((item) => (
                                    <MenuItem key={item.path} selected={isActive(item.path)} onClick={() => go(item.path)}>
                                        {item.label}
                                    </MenuItem>
                                ))}
                            </Menu>
                        </>
                    )}

                    <Typography
                        variant="h6"
                        onClick={() => navigate("/")}
                        sx={{ fontWeight: 700, letterSpacing: 0.5, cursor: "pointer", fontSize: { xs: "1.05rem", md: "1.25rem" }, mr: 2, whiteSpace: "nowrap" }}
                    >
                        PracticesService
                    </Typography>

                    {!collapsed && (
                        <Box component="nav" sx={{ display: "flex", gap: 0.5, flexGrow: 1 }}>
                            {items.map((item) => (
                                <Button
                                    key={item.path}
                                    color="inherit"
                                    onClick={() => navigate(item.path)}
                                    sx={{
                                        textTransform: "none",
                                        fontWeight: isActive(item.path) ? 700 : 500,
                                        whiteSpace: "nowrap",
                                        borderRadius: 0,
                                        px: 1.5,
                                        borderBottom: "3px solid",
                                        borderColor: isActive(item.path) ? "primary.contrastText" : "transparent",
                                        "&:hover": { bgcolor: "rgba(255,255,255,0.1)" },
                                    }}
                                >
                                    {item.label}
                                </Button>
                            ))}
                        </Box>
                    )}

                    <Box sx={{ flexGrow: collapsed ? 1 : 0 }} />

                    {me ? (
                        <>
                            <Tooltip title={`${me.lastName} ${me.firstName}`}>
                                <IconButton onClick={(e) => setAccountAnchor(e.currentTarget)} sx={{ p: 0.5 }} aria-label="Аккаунт">
                                    <Avatar sx={{ width: 34, height: 34, bgcolor: "primary.dark", fontSize: 14 }}>{initials(me)}</Avatar>
                                </IconButton>
                            </Tooltip>
                            <Menu
                                anchorEl={accountAnchor}
                                open={Boolean(accountAnchor)}
                                onClose={() => setAccountAnchor(null)}
                                slotProps={{ paper: { sx: { minWidth: 220, mt: 1 } } }}
                            >
                                <Box sx={{ px: 2, py: 1 }}>
                                    <Typography fontWeight={600} noWrap>{me.lastName} {me.firstName}</Typography>
                                    <Typography variant="body2" color="text.secondary" noWrap>{me.email}</Typography>
                                </Box>
                                <Divider />
                                <MenuItem onClick={() => go("/profile")}>
                                    <ListItemText>Профиль</ListItemText>
                                </MenuItem>
                                <MenuItem onClick={() => { setAccountAnchor(null); logout(); }} sx={{ color: "error.main" }}>
                                    <ListItemText>Выйти</ListItemText>
                                </MenuItem>
                            </Menu>
                        </>
                    ) : loading ? (
                        <Skeleton variant="circular" width={34} height={34} sx={{ bgcolor: "rgba(255,255,255,0.25)" }} />
                    ) : (
                        <Button color="inherit" variant="outlined" onClick={() => navigate("/login")} sx={{ textTransform: "none", borderColor: "rgba(255,255,255,0.6)" }}>
                            Войти
                        </Button>
                    )}
                </Toolbar>
            </Container>
        </AppBar>
    );
}
