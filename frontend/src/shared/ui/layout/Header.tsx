import React, {useEffect, useState} from "react";
import { AppBar, Toolbar, Typography, Button, IconButton, Menu, MenuItem, Container, useMediaQuery } from "@mui/material";
import { AccountCircle } from "@mui/icons-material";
import { useNavigate } from "react-router-dom";
import { logout } from "@shared/services/auth.service.ts";
import {getMe} from "@/shared/services/axios.service.ts";
import { User } from "@/entities/User";

export default function Header() {
    const navigate = useNavigate();
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const [me, setMe] = useState<User>()
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
        if (window.location.pathname == "/login") return;
        
        getMe().then(res => {
            const user: User = res.data;
            setMe(user);
        });
    }, [navigate]);

    return (
        <AppBar position="static" color="primary">
            <Container>
                <Toolbar>
                    <Typography variant="h6" sx={{ flexGrow: 1, fontWeight: "bold", cursor: "pointer" }} onClick={() => navigate("/")}>
                        PracticesService
                    </Typography>

                    {window.location.pathname !== "/login" && me?.roles.includes("Администратор") && (
                        <>
                            <Typography variant="h6" sx={{ flexGrow: 1, cursor: "pointer" }} onClick={() => navigate("/admin")}>
                                Админ панель
                            </Typography>
                        </>
                    )}


                    {window.location.pathname !== "/login" && (
                        <>
                            <Typography variant="h6" sx={{ flexGrow: 1, cursor: "pointer" }} onClick={() => navigate("/themes")}>
                                Темы
                            </Typography>
                            <Typography variant="h6" sx={{ flexGrow: 1, cursor: "pointer" }} onClick={() => navigate("/practices")}>
                                Практики
                            </Typography>
                            
                            <IconButton color="inherit" onClick={handleMenuOpen}>
                                <AccountCircle />
                            </IconButton>
                            <Menu anchorEl={anchorEl} open={isMenuOpen} onClose={handleMenuClose}>
                                <MenuItem onClick={() => handleNavigate("/profile")}>Профиль</MenuItem>
                                <MenuItem onClick={() => { logout(); handleMenuClose(); }}>Выйти</MenuItem>
                            </Menu>
                        </>
                    )}
                </Toolbar>
            </Container>
        </AppBar>
    );
}
