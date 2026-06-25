import { Layout } from "@shared/ui/layout/Layout.tsx";
import { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { confirmEmail } from "@shared/services/axios.service.ts";
import {
    Container,
    Button,
    Typography,
    Paper,
    Box,
    Alert,
    CircularProgress
} from "@mui/material";
import { CheckCircle, Error as ErrorIcon } from "@mui/icons-material";

export function ConfirmEmailPage() {
    const [searchParams] = useSearchParams();
    const token = searchParams.get('token');
    const email = searchParams.get('email');
    const [loading, setLoading] = useState(true);
    const [success, setSuccess] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    useEffect(() => {
        if (!token || !email) {
            setError("Недействительная ссылка подтверждения");
            setLoading(false);
            return;
        }

        const confirm = async () => {
            try {
                await confirmEmail({ token, email });
                setSuccess(true);
            } catch (err: any) {
                const message = err.response?.data?.message || "Не удалось подтвердить email. Возможно, ссылка устарела или недействительна.";
                setError(message);
            } finally {
                setLoading(false);
            }
        };

        confirm();
    }, [token, email]);

    if (loading) {
        return (
            <Layout>
                <Container maxWidth="xs" sx={{ minHeight: "70vh", display: "flex", alignItems: "center", justifyContent: "center" }}>
                    <Paper elevation={3} sx={{ padding: 4, width: "100%", borderRadius: 2, textAlign: "center" }}>
                        <CircularProgress size={48} sx={{ mb: 2 }} />
                        <Typography variant="body1">Подтверждение email...</Typography>
                    </Paper>
                </Container>
            </Layout>
        );
    }

    return (
        <Layout>
            <Container maxWidth="xs" sx={{ minHeight: "70vh", display: "flex", alignItems: "center", justifyContent: "center" }}>
                <Paper elevation={3} sx={{ padding: 4, width: "100%", borderRadius: 2 }}>
                    {success ? (
                        <Box textAlign="center">
                            <CheckCircle sx={{ fontSize: 64, color: "success.main", mb: 2 }} />
                            <Typography variant="h5" gutterBottom>
                                Email подтвержден!
                            </Typography>
                            <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
                                Ваш email успешно подтвержден. Теперь вы можете войти в свой аккаунт.
                            </Typography>
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
                        <Box textAlign="center">
                            <ErrorIcon sx={{ fontSize: 64, color: "error.main", mb: 2 }} />
                            <Typography variant="h5" gutterBottom>
                                Ошибка подтверждения
                            </Typography>
                            <Alert severity="error" sx={{ mb: 3 }}>
                                {error || "Не удалось подтвердить email. Попробуйте еще раз или запросите новую ссылку."}
                            </Alert>
                            <Button
                                fullWidth
                                variant="contained"
                                color="primary"
                                onClick={() => navigate('/login')}
                                sx={{ mb: 2 }}
                            >
                                Войти
                            </Button>
                        </Box>
                    )}
                </Paper>
            </Container>
        </Layout>
    );
}