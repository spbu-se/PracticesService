import {Layout} from "@shared/ui/layout/Layout.tsx";
import {Button, Container, Typography, Paper, CircularProgress, Box} from "@mui/material";

export function BasePage() {
    return (
        <Layout>
            <Container maxWidth="md" sx={{mt: 4}}>
                <Paper elevation={3} sx={{mt: 3, p: 4, borderRadius: 2}}>
                    <Typography variant="h4" align="center" gutterBottom>
                        Добро пожаловать в сервис для работы с учебными практиками
                    </Typography>
                </Paper>
            </Container>
        </Layout>
    );
}
