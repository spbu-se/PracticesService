import React, { useState } from "react";
import { Navbar, Nav, Container, Dropdown } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import { logout } from "@shared/services/auth.service.ts";

export default function Header() {
    const navigate = useNavigate();
    const [showDropdown, setShowDropdown] = useState(false);

    const isProjectOpen = window.location.pathname !== "/" && window.location.pathname !== "/login";

    const handleNavigate = (path: string) => {
        navigate(path);
    };

    return (
        <Navbar expand="lg" bg="primary" variant="dark">
            <Container>
                {/* Brand Logo */}
                <Navbar.Brand href="/" className="fw-bold">PracticesService</Navbar.Brand>

                {/* Navbar Toggle for Mobile */}
                {isProjectOpen && <Navbar.Toggle aria-controls="navbar-nav" />}

                {/* Navbar Content */}
                <Navbar.Collapse id="navbar-nav">
                    <Nav className="mx-auto">
                        {isProjectOpen && (
                            <>
                                <Nav.Link href="/dashboard">Dashboard</Nav.Link>
                                <Nav.Link href="/settings">Settings</Nav.Link>
                            </>
                        )}
                    </Nav>

                    {/* User Dropdown */}
                    {window.location.pathname !== "/login" && (
                        <Dropdown show={showDropdown} onToggle={() => setShowDropdown(!showDropdown)}>
                            <Dropdown.Toggle variant="light" className="border-0">
                                <i className="bi bi-person-circle"></i>
                            </Dropdown.Toggle>
                            <Dropdown.Menu align="end">
                            <Dropdown.Item onClick={() => handleNavigate("/profile")}>Профиль</Dropdown.Item>
                                <Dropdown.Item onClick={() => logout()}>Выйти</Dropdown.Item>
                            </Dropdown.Menu>
                        </Dropdown>
                    )}
                </Navbar.Collapse>
            </Container>
        </Navbar>
    );
}
