import { Layout } from "@shared/ui/layout/Layout.tsx";
import {
    Container,
    Typography,
    Box,
    useTheme,
    Fade
} from "@mui/material";

export function BasePage() {
    const theme = useTheme();

    return (
        <Layout>
            <Container maxWidth="md">
                <Box
                    sx={{
                        height: "70vh",
                        display: "flex",
                        flexDirection: "column",
                        alignItems: "center",
                        justifyContent: "center",
                        textAlign: "center",
                        gap: 2
                    }}
                >
                    <Fade in timeout={1000}>
                        <Typography
                            variant="h2"
                            component="h1"
                            sx={{
                                fontWeight: 700,
                                color: theme.palette.mode === 'dark'
                                    ? theme.palette.primary.light
                                    : theme.palette.primary.dark,
                                letterSpacing: 1.5,
                                mb: 2
                            }}
                        >
                            Добро пожаловать
                        </Typography>
                    </Fade>

                    <Fade in timeout={1500}>
                        <Typography
                            variant="h4"
                            component="p"
                            sx={{
                                color: theme.palette.text.secondary,
                                maxWidth: "600px",
                                lineHeight: 1.6
                            }}
                        >
                            Сервис для работы с учебными практиками
                        </Typography>
                    </Fade>
                </Box>
            </Container>
        </Layout>
    );
}