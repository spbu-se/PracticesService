import React, { useState } from "react";
import { AppBar, Toolbar, Typography, Button, IconButton, Menu, MenuItem, Container, useMediaQuery } from "@mui/material";
import { AccountCircle } from "@mui/icons-material";
import { useNavigate } from "react-router-dom";
import { logout } from "@shared/services/auth.service.ts";

export default function Header() {
    const navigate = useNavigate();
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
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

    return (
        <AppBar position="static" color="primary">
            <Container>
                <Toolbar>
                    <Typography variant="h6" sx={{ flexGrow: 1, fontWeight: "bold", cursor: "pointer" }} onClick={() => navigate("/")}>
                        PracticesService
                    </Typography>

                    {window.location.pathname !== "/login" && (
                        <>
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
