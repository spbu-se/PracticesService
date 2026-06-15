import { Layout } from "@shared/ui/layout/Layout.tsx";
import { ChangeEvent, FormEvent, useState, useEffect } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { resetPassword } from "@shared/services/axios.service.ts";
import { ResetPassword } from "@entities/ResetPassword.ts";
import {
    Container,
    TextField,
    Button,
    Typography,
    Paper,
    Box,
    Alert,
    IconButton,
    InputAdornment,
    CircularProgress,
    List,
    ListItem,
    ListItemIcon,
    ListItemText,
    Chip
} from "@mui/material";
import {
    Visibility,
    VisibilityOff,
    CheckCircle,
    Cancel,
    CheckCircleOutline,
    HighlightOff
} from "@mui/icons-material";

export function ResetPasswordPage() {
    const [searchParams] = useSearchParams();
    const token = searchParams.get('token');
    const email = searchParams.get('email');

    const [password, setPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [showPassword, setShowPassword] = useState(false);
    const [showConfirmPassword, setShowConfirmPassword] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const [success, setSuccess] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();
    
    const [isLengthValid, setIsLengthValid] = useState(false);
    const [isDigitValid, setIsDigitValid] = useState(false);
    const [isUpperCaseValid, setIsUpperCaseValid] = useState(false);
    const [isLowerCaseValid, setIsLowerCaseValid] = useState(false);
    const [isSpecialCharValid, setIsSpecialCharValid] = useState(false);
    const [isPasswordFocused, setIsPasswordFocused] = useState(false);
    
    useEffect(() => {
        if (!token || !email) {
            setError("Недействительная ссылка для сброса пароля");
        }
    }, [token, email]);

    const validatePassword = (pwd: string) => {
        setIsLengthValid(pwd.length >= 8);
        setIsDigitValid(/\d/.test(pwd));
        setIsUpperCaseValid(/[A-Z]/.test(pwd));
        setIsLowerCaseValid(/[a-z]/.test(pwd));
        setIsSpecialCharValid(/[!@#$%^&*()]/.test(pwd));
    };

    const onChangePassword = (event: ChangeEvent<HTMLInputElement>) => {
        const value = event.target.value;
        setPassword(value);
        setError(null);
        validatePassword(value);
    };

    const onChangeConfirmPassword = (event: ChangeEvent<HTMLInputElement>) => {
        setConfirmPassword(event.target.value);
        setError(null);
    };

    const handleTogglePasswordVisibility = () => {
        setShowPassword(!showPassword);
    };

    const handleToggleConfirmPasswordVisibility = () => {
        setShowConfirmPassword(!showConfirmPassword);
    };

    const getPasswordStrength = () => {
        const validations = [isLengthValid, isDigitValid, isUpperCaseValid, isLowerCaseValid, isSpecialCharValid];
        const count = validations.filter(v => v).length;

        if (count <= 2) return { label: "Слабый", color: "error" as const };
        if (count <= 3) return { label: "Средний", color: "warning" as const };
        if (count <= 4) return { label: "Хороший", color: "info" as const };
        return { label: "Сильный", color: "success" as const };
    };

    const isPasswordValid = () => {
        return isLengthValid && isDigitValid && isUpperCaseValid &&
            isLowerCaseValid && isSpecialCharValid;
    };

    const validateAll = (pwd: string): string | null => {
        if (pwd.length < 8) {
            return "Пароль должен содержать минимум 8 символов";
        }
        if (!/\d/.test(pwd)) {
            return "Пароль должен содержать хотя бы одну цифру (0-9)";
        }
        if (!/[A-Z]/.test(pwd)) {
            return "Пароль должен содержать хотя бы одну заглавную букву (A-Z)";
        }
        if (!/[a-z]/.test(pwd)) {
            return "Пароль должен содержать хотя бы одну строчную букву (a-z)";
        }
        if (!/[!@#$%^&*()]/.test(pwd)) {
            return "Пароль должен содержать хотя бы один спецсимвол (!@#$%^&*)()";
        }
        return null;
    };

    const onSubmit = (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        
        if (password !== confirmPassword) {
            setError("Пароли не совпадают");
            return;
        }
        
        const pwdError = validateAll(password);
        if (pwdError) {
            setError(pwdError);
            return;
        }

        if (!token || !email) {
            setError("Отсутствует токен или email для сброса пароля");
            return;
        }

        setIsLoading(true);
        setError(null);

        const resetData: ResetPassword = {
            token: token,
            newPassword: password,
            email: email
        };

        resetPassword(resetData)
            .then(() => {
                setSuccess(true);
                setIsLoading(false);
                setTimeout(() => {
                    navigate('/login');
                }, 3000);
            })
            .catch((e) => {
                console.error("Ошибка сброса пароля:", e);
                const errorMessage = e.response?.data?.message ||
                    e.response?.data?.title ||
                    "Не удалось сбросить пароль. Возможно, ссылка устарела или недействительна.";
                setError(errorMessage);
                setIsLoading(false);
            });
    };
    
    if (!token || !email) {
        return (
            <Layout>
                <Container maxWidth="xs" sx={{ minHeight: "70vh", display: "flex", alignItems: "center", justifyContent: "center" }}>
                    <Paper elevation={3} sx={{ padding: 4, width: "100%", borderRadius: 2 }}>
                        <Alert severity="error" sx={{ mb: 3 }}>
                            {error || "Недействительная ссылка для сброса пароля"}
                        </Alert>
                        <Button
                            fullWidth
                            variant="contained"
                            color="primary"
                            onClick={() => navigate('/login')}
                        >
                            Вернуться ко входу
                        </Button>
                    </Paper>
                </Container>
            </Layout>
        );
    }

    const strength = getPasswordStrength();

    return (
        <Layout>
            <Container maxWidth="sm" sx={{ minHeight: "70vh", display: "flex", alignItems: "center", justifyContent: "center" }}>
                <Paper elevation={3} sx={{ padding: 4, width: "100%", borderRadius: 2 }}>
                    <Typography variant="h5" align="center" gutterBottom>
                        Сброс пароля
                    </Typography>

                    {success ? (
                        <Box>
                            <Alert severity="success" sx={{ mb: 3 }}>
                                Пароль успешно изменен!
                            </Alert>
                            <Typography variant="body2" align="center" sx={{ mb: 2 }}>
                                Перенаправление на страницу входа...
                            </Typography>
                            <CircularProgress size={24} sx={{ display: 'block', margin: '0 auto', mb: 2 }} />
                            <Button
                                fullWidth
                                variant="contained"
                                color="primary"
                                onClick={() => navigate('/login')}
                            >
                                Перейти ко входу
                            </Button>
                        </Box>
                    ) : (
                        <Box component="form" onSubmit={onSubmit}>
                            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                                Введите новый пароль для вашей учетной записи.
                            </Typography>

                            <Alert severity="info" sx={{ mb: 2 }}>
                                <Typography variant="body2" fontWeight="bold">Пароль должен содержать:</Typography>
                                <List dense sx={{ pl: 2 }}>
                                    <ListItem sx={{ py: 0 }}>
                                        <ListItemIcon sx={{ minWidth: 30 }}>
                                            {isLengthValid ? <CheckCircle color="success" fontSize="small" /> : <HighlightOff color="error" fontSize="small" />}
                                        </ListItemIcon>
                                        <ListItemText primary="Минимум 8 символов" />
                                    </ListItem>
                                    <ListItem sx={{ py: 0 }}>
                                        <ListItemIcon sx={{ minWidth: 30 }}>
                                            {isDigitValid ? <CheckCircle color="success" fontSize="small" /> : <HighlightOff color="error" fontSize="small" />}
                                        </ListItemIcon>
                                        <ListItemText primary="Хотя бы одну цифру (0-9)" />
                                    </ListItem>
                                    <ListItem sx={{ py: 0 }}>
                                        <ListItemIcon sx={{ minWidth: 30 }}>
                                            {isUpperCaseValid ? <CheckCircle color="success" fontSize="small" /> : <HighlightOff color="error" fontSize="small" />}
                                        </ListItemIcon>
                                        <ListItemText primary="Хотя бы одну заглавную букву (A-Z)" />
                                    </ListItem>
                                    <ListItem sx={{ py: 0 }}>
                                        <ListItemIcon sx={{ minWidth: 30 }}>
                                            {isLowerCaseValid ? <CheckCircle color="success" fontSize="small" /> : <HighlightOff color="error" fontSize="small" />}
                                        </ListItemIcon>
                                        <ListItemText primary="Хотя бы одну строчную букву (a-z)" />
                                    </ListItem>
                                    <ListItem sx={{ py: 0 }}>
                                        <ListItemIcon sx={{ minWidth: 30 }}>
                                            {isSpecialCharValid ? <CheckCircle color="success" fontSize="small" /> : <HighlightOff color="error" fontSize="small" />}
                                        </ListItemIcon>
                                        <ListItemText primary="Хотя бы один спецсимвол (!@#$%^&*)()" />
                                    </ListItem>
                                </List>
                            </Alert>

                            <TextField
                                fullWidth
                                label="Новый пароль"
                                variant="outlined"
                                margin="normal"
                                type={showPassword ? "text" : "password"}
                                value={password}
                                onChange={onChangePassword}
                                onFocus={() => setIsPasswordFocused(true)}
                                onBlur={() => setIsPasswordFocused(false)}
                                required
                                disabled={isLoading}
                                error={!!error && error !== "Пароли не совпадают"}
                                helperText={password.length > 0 && !isPasswordValid() ? "Пароль не соответствует требованиям" : ""}
                                InputProps={{
                                    endAdornment: (
                                        <InputAdornment position="end">
                                            <IconButton
                                                onClick={handleTogglePasswordVisibility}
                                                edge="end"
                                            >
                                                {showPassword ? <VisibilityOff /> : <Visibility />}
                                            </IconButton>
                                        </InputAdornment>
                                    )
                                }}
                            />

                            {password.length > 0 && (
                                <Box sx={{ mt: 1, display: 'flex', alignItems: 'center', gap: 1 }}>
                                    <Typography variant="body2" color="text.secondary">
                                        Сложность пароля:
                                    </Typography>
                                    <Chip
                                        label={strength.label}
                                        color={strength.color}
                                        size="small"
                                        sx={{ fontWeight: 'bold' }}
                                    />
                                </Box>
                            )}

                            <TextField
                                fullWidth
                                label="Подтвердите пароль"
                                variant="outlined"
                                margin="normal"
                                type={showConfirmPassword ? "text" : "password"}
                                value={confirmPassword}
                                onChange={onChangeConfirmPassword}
                                required
                                disabled={isLoading}
                                error={!!error && error === "Пароли не совпадают"}
                                helperText={error === "Пароли не совпадают" ? error : ""}
                                InputProps={{
                                    endAdornment: (
                                        <InputAdornment position="end">
                                            <IconButton
                                                onClick={handleToggleConfirmPasswordVisibility}
                                                edge="end"
                                            >
                                                {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                                            </IconButton>
                                        </InputAdornment>
                                    )
                                }}
                            />

                            {error && error !== "Пароли не совпадают" && (
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
                                disabled={isLoading || !password || !confirmPassword || !isPasswordValid()}
                            >
                                {isLoading ? <CircularProgress size={24} /> : "Сбросить пароль"}
                            </Button>

                            <Box sx={{ mt: 2, textAlign: 'center' }}>
                                <Button
                                    variant="text"
                                    onClick={() => navigate('/login')}
                                    sx={{ color: 'primary.main' }}
                                >
                                    Вернуться ко входу
                                </Button>
                            </Box>
                        </Box>
                    )}
                </Paper>
            </Container>
        </Layout>
    );
}