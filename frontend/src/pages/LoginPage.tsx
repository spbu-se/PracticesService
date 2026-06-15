import { Layout } from "@shared/ui/layout/Layout.tsx";
import { ChangeEvent, FormEvent, useState } from "react";
import { login } from "@shared/services/axios.service.ts";
import { LoginResponse } from "@entities/LoginResponse.ts";
import { setJWTToken, setRefreshToken } from "@shared/services/localStorage.service.ts";
import {
    Container,
    TextField,
    Button,
    Typography,
    Paper,
    Box,
    Link
} from "@mui/material";
import { useNavigate } from "react-router-dom";

export function LoginPage() {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const navigate = useNavigate();

    const onChangeLogin = (event: ChangeEvent<HTMLInputElement>) => {
        setEmail(event.target.value);
    };

    const onChangePassword = (event: ChangeEvent<HTMLInputElement>) => {
        setPassword(event.target.value);
    };

    const onSubmit = (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        login(email, password)
            .then(response => {
                const loginResponse: LoginResponse = response.data;
                setJWTToken(loginResponse.token);
                setRefreshToken(loginResponse.refreshToken);
                navigate("/");
            })
            .catch(e => {
                console.log(e);
                alert("Не удалось войти");
            });
    };

    const handleForgotPassword = (event: React.MouseEvent) => {
        event.preventDefault();
        navigate('/forgot-password');
    };

    return (
        <Layout>
            <Container maxWidth="xs" sx={{ minHeight: "70vh", display: "flex", alignItems: "center", justifyContent: "center" }}>
                <Paper elevation={3} sx={{ padding: 4, width: "100%", borderRadius: 2 }}>
                    <Typography variant="h5" align="center" gutterBottom>
                        Вход
                    </Typography>

                    <Box component="form" onSubmit={onSubmit}>
                        <TextField
                            fullWidth
                            label="Логин"
                            variant="outlined"
                            margin="normal"
                            type="text"
                            value={email}
                            onChange={onChangeLogin}
                            required
                        />

                        <TextField
                            fullWidth
                            label="Пароль"
                            variant="outlined"
                            margin="normal"
                            type="password"
                            value={password}
                            onChange={onChangePassword}
                            required
                        />

                        <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 1 }}>
                            <Link
                                component="button"
                                variant="body2"
                                onClick={handleForgotPassword}
                                sx={{ color: 'primary.main' }}
                                type="button" 
                            >
                                Забыли пароль?
                            </Link>
                        </Box>

                        <Button
                            type="submit"
                            fullWidth
                            variant="contained"
                            color="primary"
                            sx={{ mt: 2 }}
                        >
                            Войти
                        </Button>
                        <Button
                            fullWidth
                            variant="contained"
                            color="secondary"
                            onClick={() => navigate('/register')}
                            sx={{ mt: 2 }}
                            type="button"
                        >
                            Регистрация
                        </Button>
                    </Box>
                </Paper>
            </Container>
        </Layout>
    );
}