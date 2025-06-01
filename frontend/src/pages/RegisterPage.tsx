import { Layout } from "@shared/ui/layout/Layout.tsx";
import { ChangeEvent, FormEvent, useState } from "react";
import { register } from "@shared/services/axios.service.ts"; // You'll need to create this function
import { setJWTToken, setRefreshToken } from "@shared/services/localStorage.service.ts";
import {
    Container,
    TextField,
    Button,
    Typography,
    Paper,
    Box,
    Grid
} from "@mui/material";
import { useNavigate } from "react-router-dom";
import {UserRole} from "../entities/UserRoles";

export function RegisterPage() {
    const [formData, setFormData] = useState<RegisterData>({
        email: "",
        password: "",
        firstName: "",
        lastName: "",
        middleName: ""
    });

    const navigate = useNavigate();

    const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: value
        }));
    };

    const onSubmit = (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        
        if (!formData.email || !formData.password || !formData.firstName || !formData.lastName) {
            alert("Пожалуйста, заполните все обязательные поля");
            return;
        }

        formData.roles = [UserRole.STUDENT];
        register(formData)
            .then(response => {
                setJWTToken(response.data.token);
                setRefreshToken(response.data.refreshToken);
                navigate("/");
            })
            .catch(e => {
                console.error("Registration error:", e);
                alert("Не удалось зарегистрироваться: " + (e.response?.data?.message || e.message));
            });
    };

    return (
        <Layout>
            <Container maxWidth="sm" sx={{ minHeight: "70vh", display: "flex", alignItems: "center", justifyContent: "center" }}>
                <Paper elevation={3} sx={{ padding: 4, width: "100%", borderRadius: 2 }}>
                    <Typography variant="h5" align="center" gutterBottom>
                        Регистрация
                    </Typography>

                    <Box component="form" onSubmit={onSubmit}>
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={6}>
                                <TextField
                                    fullWidth
                                    label="Имя"
                                    name="firstName"
                                    variant="outlined"
                                    margin="normal"
                                    value={formData.firstName}
                                    onChange={handleChange}
                                    required
                                    inputProps={{ maxLength: 100 }}
                                />
                            </Grid>
                            <Grid item xs={12} sm={6}>
                                <TextField
                                    fullWidth
                                    label="Фамилия"
                                    name="lastName"
                                    variant="outlined"
                                    margin="normal"
                                    value={formData.lastName}
                                    onChange={handleChange}
                                    required
                                    inputProps={{ maxLength: 100 }}
                                />
                            </Grid>
                            <Grid item xs={12}>
                                <TextField
                                    fullWidth
                                    label="Отчество (необязательно)"
                                    name="middleName"
                                    variant="outlined"
                                    margin="normal"
                                    value={formData.middleName}
                                    onChange={handleChange}
                                    inputProps={{ maxLength: 100 }}
                                />
                            </Grid>
                            <Grid item xs={12}>
                                <TextField
                                    fullWidth
                                    label="Email"
                                    name="email"
                                    type="email"
                                    variant="outlined"
                                    margin="normal"
                                    value={formData.email}
                                    onChange={handleChange}
                                    required
                                />
                            </Grid>
                            <Grid item xs={12}>
                                <Typography variant="body2" color="textSecondary">
                                    Пароль должен содержать:
                                </Typography>
                                <ul style={{ marginTop: 0, fontSize: '0.875rem' }}>
                                    <li>Минимум 8 символов</li>
                                    <li>Хотя бы одну цифру (0-9)</li>
                                    <li>Хотя бы одну заглавную (A-Z) и строчную (a-z) букву</li>
                                    <li>Хотя бы один спецсимвол (!@#$%^&*)</li>
                                </ul>
                            </Grid>
                            <Grid item xs={12}>
                                <TextField
                                    fullWidth
                                    label="Пароль"
                                    name="password"
                                    type="password"
                                    variant="outlined"
                                    margin="normal"
                                    value={formData.password}
                                    onChange={handleChange}
                                    required
                                />
                            </Grid>
                        </Grid>

                        <Button
                            type="submit"
                            fullWidth
                            variant="contained"
                            color="primary"
                            size="large"
                            sx={{ mt: 3 }}
                        >
                            Зарегистрироваться
                        </Button>

                        <Button
                            fullWidth
                            variant="outlined"
                            color="secondary"
                            onClick={() => navigate('/login')}
                            sx={{ mt: 2 }}
                        >
                            Уже есть аккаунт? Войти
                        </Button>
                    </Box>
                </Paper>
            </Container>
        </Layout>
    );
}