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
    Avatar,
    useTheme,
    useMediaQuery,
    Divider,
    Badge
} from "@mui/material";
import {
    AccountCircle,
    AdminPanelSettings,
    MenuBook,
    WorkOutline,
    KeyboardArrowDown
} from "@mui/icons-material";
import { useNavigate } from "react-router-dom";
import { logout } from "@shared/services/auth.service.ts";
import { getMe } from "@/shared/services/axios.service.ts";
import { User } from "@/entities/User";
import {UserRole} from "../../../entities/UserRoles";

export default function Header() {
    const navigate = useNavigate();
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down('md'));
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const [me, setMe] = useState<User>();
    const isMenuOpen = Boolean(anchorEl);

    const handleMenuOpen = (event: React.MouseEvent<HTMLButtonElement>) => {
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
        if (window.location.pathname === "/login") return;

        getMe().then(res => {
            const user: User = res.data;
            setMe(user);
        });
    }, [navigate]);

    return (
        <AppBar
            position="static"
            elevation={1}
            sx={{
                bgcolor: 'primary.main',
                color: 'primary.contrastText',
                py: 1,
                borderBottom: `1px solid ${theme.palette.primary.light}`
            }}
        >
            <Container maxWidth="xl">
                <Toolbar disableGutters sx={{ justifyContent: 'space-between' }}>
                    <Box
                        onClick={() => navigate("/")}
                        sx={{
                            display: 'flex',
                            alignItems: 'center',
                            gap: 1,
                            cursor: 'pointer',
                            '&:hover': { opacity: 0.9 }
                        }}
                    >
                        <Typography
                            variant="h6"
                            sx={{
                                fontWeight: 700,
                                letterSpacing: 0.5,
                                fontSize: { xs: '1.1rem', md: '1.25rem' }
                            }}
                        >
                            PracticesService
                        </Typography>
                    </Box>
                    
                    {window.location.pathname !== "/login" && (
                        <Box sx={{
                            display: 'flex',
                            alignItems: 'center',
                            gap: { xs: 1, md: 2 }
                        }}>
                            {me?.roles.includes(UserRole.ADMIN) && (
                                <Button
                                    startIcon={<AdminPanelSettings />}
                                    onClick={() => navigate("/admin")}
                                    sx={{
                                        color: 'inherit',
                                        '&:hover': {
                                            bgcolor: 'primary.dark'
                                        },
                                        minWidth: 'unset',
                                        px: 2
                                    }}
                                >
                                    {!isMobile && 'Админ панель'}
                                </Button>
                            )}

                            <Button
                                startIcon={<MenuBook />}
                                onClick={() => navigate("/themes")}
                                sx={{
                                    color: 'inherit',
                                    '&:hover': {
                                        bgcolor: 'primary.dark'
                                    },
                                    minWidth: 'unset',
                                    px: 2
                                }}
                            >
                                {!isMobile && 'Темы'}
                            </Button>

                            <Button
                                startIcon={<WorkOutline />}
                                onClick={() => navigate("/practices")}
                                sx={{
                                    color: 'inherit',
                                    '&:hover': {
                                        bgcolor: 'primary.dark'
                                    },
                                    minWidth: 'unset',
                                    px: 2
                                }}
                            >
                                {!isMobile && 'Практики'}
                            </Button>
                            
                            <Box sx={{ ml: 1 }}>
                                <IconButton
                                    onClick={handleMenuOpen}
                                    sx={{
                                        color: 'inherit',
                                        p: 1,
                                        '&:hover': {
                                            bgcolor: 'primary.dark'
                                        }
                                    }}
                                >
                                    <Badge
                                        overlap="circular"
                                        badgeContent={
                                            <KeyboardArrowDown sx={{
                                                fontSize: '1rem',
                                                color: 'inherit'
                                            }} />
                                        }
                                    >
                                        <AccountCircle sx={{ fontSize: 32 }} />
                                    </Badge>
                                </IconButton>
                            </Box>
                        </Box>
                    )}
                </Toolbar>
            </Container>
            
            <Menu
                anchorEl={anchorEl}
                open={isMenuOpen}
                onClose={handleMenuClose}
                PaperProps={{
                    elevation: 4,
                    sx: {
                        minWidth: 200,
                        borderRadius: 2,
                        mt: 1,
                        bgcolor: 'primary.dark',
                        color: 'primary.contrastText',
                        '& .MuiMenuItem-root': {
                            py: 1.5,
                            '&:hover': {
                                bgcolor: 'primary.light'
                            }
                        }
                    }
                }}
            >
                <MenuItem onClick={() => handleNavigate("/profile")}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                        <AccountCircle fontSize="small" />
                        Профиль
                    </Box>
                </MenuItem>

                <Divider sx={{ my: 0.5, bgcolor: 'primary.light' }} />

                <MenuItem
                    onClick={() => { logout(); handleMenuClose(); }}
                    sx={{ color: 'error.light', display: 'flex', alignItems: 'center' }}
                >
                    Выйти
                </MenuItem>
            </Menu>
        </AppBar>
    );
}