import React, { useEffect, useState } from "react";
import {
    AppBar,
    Toolbar,
    Typography,
    Button,
    IconButton,
    Menu,
    MenuItem,
    Container,
    Box,
    useTheme,
    useMediaQuery,
    Divider
} from "@mui/material";
import { useNavigate } from "react-router-dom";
import { logout } from "@shared/services/auth.service.ts";
import { getMePublic } from "@/shared/services/axios.service.ts";
import { User } from "@/entities/User";
import { UserRole } from "../../../entities/UserRoles";
import MenuIcon from '@mui/icons-material/Menu';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';

export default function Header() {
    const navigate = useNavigate();
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const [me, setMe] = useState<User>();
    const isMenuOpen = Boolean(anchorEl);

    const handleMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
        setAnchorEl(event.currentTarget);
    };

    const handleMenuClose = () => {
        setAnchorEl(null);
    };

    const handleNavigate = (path: string) => {
        navigate(path);
        handleMenuClose();
    };

    useEffect(() => {
        if (window.location.pathname === "/login" || window.location.pathname === "/register") return;

        getMePublic()
            .then((res) => setMe(res.data))
            .catch(() => {});
    }, [navigate]);

    return (
        <AppBar
            position="static"
            elevation={1}
            sx={{
                bgcolor: "primary.main",
                color: "primary.contrastText",
                py: 1,
                borderBottom: `1px solid ${theme.palette.primary.light}`
            }}
        >
            <Container maxWidth="xl">
                <Toolbar disableGutters sx={{ justifyContent: "space-between" }}>
                    <Box
                        onClick={() => navigate("/")}
                        sx={{
                            display: "flex",
                            alignItems: "center",
                            gap: 1,
                            cursor: "pointer",
                            "&:hover": { opacity: 0.9 }
                        }}
                    >
                        {/*<img*/}
                        {/*    src="/public/icon.svg"*/}
                        {/*    alt="Logo"*/}
                        {/*    style={{ width: '32px', height: '32px' }}*/}
                        {/*/>*/}
                        <Typography
                            variant="h6"
                            sx={{
                                fontWeight: 700,
                                letterSpacing: 0.5,
                                fontSize: { xs: "1.1rem", md: "1.25rem" }
                            }}
                        >
                            PracticesService
                        </Typography>
                    </Box>

                    {isMobile ? (
                        <Box>
                            <Button
                                startIcon={<MenuIcon/>}
                                color="inherit"
                                onClick={handleMenuOpen}
                                sx={{
                                    textTransform: "none",
                                    px: 2
                                }}
                            >
                                Меню
                            </Button>
                            <Menu
                                anchorEl={anchorEl}
                                open={isMenuOpen}
                                onClose={handleMenuClose}
                                PaperProps={{
                                    sx: {
                                        borderRadius: 2,
                                        mt: 1,
                                        bgcolor: "primary.dark",
                                        color: "primary.contrastText"
                                    }
                                }}
                            >
                                {me?.roles?.includes(UserRole.ADMIN) && (
                                    <MenuItem onClick={() => handleNavigate("/admin")}>Админ панель</MenuItem>
                                )}
                                <MenuItem onClick={() => handleNavigate("/themes")}>Темы</MenuItem>
                                <MenuItem onClick={() => handleNavigate("/practices")}>Практики</MenuItem>
                                <Divider sx={{ my: 1, bgcolor: "primary.light" }} />
                                <MenuItem onClick={() => handleNavigate("/profile")}>Профиль</MenuItem>
                                {me && <MenuItem
                                    onClick={() => {
                                        logout();
                                        handleMenuClose();
                                    }}
                                    sx={{ color: "error.light" }}
                                >
                                    Выйти
                                </MenuItem>}
                            </Menu>
                        </Box>
                    ) : (
                        <Box sx={{ display: "flex", gap: 2, alignItems: "center" }}>
                            {me?.roles?.includes(UserRole.ADMIN) && (
                                <Button color="inherit" onClick={() => navigate("/admin")}>
                                    Админ панель
                                </Button>
                            )}
                            <Button color="inherit" onClick={() => navigate("/themes")}>
                                Темы
                            </Button>
                            <Button color="inherit" onClick={() => navigate("/practices")}>
                                Практики
                            </Button>
                            <Button color="inherit" onClick={handleMenuOpen}>
                                <AccountCircleIcon />
                            </Button>
                            <Menu
                                anchorEl={anchorEl}
                                open={isMenuOpen}
                                onClose={handleMenuClose}
                                PaperProps={{
                                    sx: {
                                        minWidth: 180,
                                        borderRadius: 2,
                                        mt: 1,
                                        bgcolor: "primary.dark",
                                        color: "primary.contrastText"
                                    }
                                }}
                            >
                                <MenuItem onClick={() => handleNavigate("/profile")}>Профиль</MenuItem>
                                <Divider sx={{ my: 1, bgcolor: "primary.light" }} />
                                {me && <MenuItem
                                    onClick={() => {
                                        logout();
                                        handleMenuClose();
                                    }}
                                    sx={{ color: "error.light" }}
                                >
                                    Выйти
                                </MenuItem>}
                            </Menu>
                        </Box>
                    )}
                </Toolbar>
            </Container>
        </AppBar>
    );
}
