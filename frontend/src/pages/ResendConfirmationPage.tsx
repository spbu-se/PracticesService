import { Layout } from "@shared/ui/layout/Layout.tsx";
import { ChangeEvent, FormEvent, useState } from "react";
import { resendConfirmation } from "@shared/services/axios.service.ts";
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

export function ResendConfirmationPage() {
    const [email, setEmail] = useState("");
    const [loading, setLoading] = useState(false);
    const [success, setSuccess] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    const onSubmit = async (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        setLoading(true);
        setError(null);

        try {
            const response = await resendConfirmation({ email });

            // Check if email is already confirmed
            if (response.data?.message?.includes("уже подтвержден")) {
                setError("Email уже подтвержден. Вы можете войти в систему.");
                setSuccess(false);
            } else {
                setSuccess(true);
            }
        } catch (err: any) {
            const message = err.response?.data?.message || "Не удалось отправить письмо подтверждения";
            
            if (message?.includes("уже подтвержден")) {
                setError("Email уже подтвержден. Вы можете войти в систему.");
                setSuccess(false);
            } else {
                setError(message);
                setSuccess(false);
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <Layout>
            <Container maxWidth="xs" sx={{ minHeight: "70vh", display: "flex", alignItems: "center", justifyContent: "center" }}>
                <Paper elevation={3} sx={{ padding: 4, width: "100%", borderRadius: 2 }}>
                    <Typography variant="h5" align="center" gutterBottom>
                        Отправить письмо повторно
                    </Typography>

                    {success ? (
                        <Box>
                            <Alert severity="success" sx={{ mb: 3 }}>
                                Письмо подтверждения отправлено. Проверьте ваш почтовый ящик.
                            </Alert>
                            <Button
                                fullWidth
                                variant="contained"
                                color="primary"
                                onClick={() => navigate('/login')}
                            >
                                Войти
                            </Button>
                        </Box>
                    ) : (
                        <Box component="form" onSubmit={onSubmit}>
                            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                                Введите ваш email, и мы отправим новую ссылку для подтверждения.
                            </Typography>

                            <TextField
                                fullWidth
                                label="Email"
                                variant="outlined"
                                margin="normal"
                                type="email"
                                value={email}
                                onChange={(e: ChangeEvent<HTMLInputElement>) => setEmail(e.target.value)}
                                required
                                disabled={loading}
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
                                disabled={loading || !email}
                            >
                                {loading ? "Отправка..." : "Отправить письмо"}
                            </Button>

                            <Box sx={{ mt: 2, textAlign: 'center' }}>
                                <Link
                                    component="button"
                                    variant="body2"
                                    onClick={() => navigate('/login')}
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