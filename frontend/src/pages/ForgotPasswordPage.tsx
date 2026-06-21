import { Layout } from "@shared/ui/layout/Layout.tsx";
import { ChangeEvent, FormEvent, useState } from "react";
import { postForgotPassword } from "@shared/services/axios.service.ts";
import { ForgotPassword } from "@entities/ForgotPassword.ts";
import {
    Container,
    TextField,
    Button,
    Typography,
    Paper,
    Box,
    Alert,
    Link
} from "@mui/material";
import { useNavigate } from "react-router-dom";

export function ForgotPasswordPage() {
    const [email, setEmail] = useState("");
    const [isLoading, setIsLoading] = useState(false);
    const [success, setSuccess] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    const onChangeEmail = (event: ChangeEvent<HTMLInputElement>) => {
        setEmail(event.target.value);
        setError(null);
    };

    const onSubmit = (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        setIsLoading(true);
        setError(null);

        const forgotPasswordData: ForgotPassword = { email };

        postForgotPassword(forgotPasswordData)
            .then(() => {
                setSuccess(true);
                setIsLoading(false);
            })
            .catch((e) => {
                console.error("Ошибка восстановления пароля:", e);
                setError(e.response?.data?.message || "Не удалось отправить запрос на восстановление. Проверьте правильность email.");
                setIsLoading(false);
            });
    };

    return (
        <Layout>
            <Container maxWidth="xs" sx={{ minHeight: "70vh", display: "flex", alignItems: "center", justifyContent: "center" }}>
                <Paper elevation={3} sx={{ padding: 4, width: "100%", borderRadius: 2 }}>
                    <Typography variant="h5" align="center" gutterBottom>
                        Восстановление пароля
                    </Typography>

                    <Typography variant="body2" align="center" color="text.secondary" sx={{ mb: 3 }}>
                        Введите email, указанный при регистрации. Мы отправим ссылку для сброса пароля.
                    </Typography>

                    {success ? (
                        <Box>
                            <Alert severity="success" sx={{ mb: 3 }}>
                                Ссылка для восстановления пароля отправлена на ваш email.
                                Проверьте почту и следуйте инструкциям.
                            </Alert>
                            <Button
                                fullWidth
                                variant="contained"
                                color="primary"
                                onClick={() => navigate('/login')}
                                sx={{ mt: 2 }}
                            >
                                Вернуться ко входу
                            </Button>
                        </Box>
                    ) : (
                        <Box component="form" onSubmit={onSubmit}>
                            <TextField
                                fullWidth
                                label="Email"
                                variant="outlined"
                                margin="normal"
                                type="email"
                                value={email}
                                onChange={onChangeEmail}
                                required
                                disabled={isLoading}
                            />

                            {error && (
                                <Alert severity="error" sx={{ mt: 2 }}>
                                    {error}
                                </Alert>
                            )}

                            <Button
                                type="submit"
                                fullWidth
                                variant="contained"
                                color="primary"
                                sx={{ mt: 3 }}
                                disabled={isLoading || !email}
                            >
                                {isLoading ? "Отправка..." : "Отправить ссылку"}
                            </Button>

                            <Box sx={{ mt: 2, textAlign: 'center' }}>
                                <Link
                                    component="button"
                                    variant="body2"
                                    onClick={() => navigate('/login')}
                                    sx={{ color: 'primary.main' }}
                                >
                                    Вернуться ко входу
                                </Link>
                            </Box>
                        </Box>
                    )}
                </Paper>
            </Container>
        </Layout>
    );
}