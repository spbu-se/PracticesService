import {useParams, useNavigate} from "react-router-dom";
import {useEffect, useState} from "react";
import {getTheme} from "@/shared/services/axios.service.ts";
import {Theme} from "@/entities/Theme.ts";
import {Layout} from "@shared/ui/layout/Layout.tsx";
import {Button, Container, Typography, Paper, CircularProgress, Box} from "@mui/material";
import {UserRole} from "../../entities/UserRoles";
import MDEditor from "@uiw/react-md-editor";

export function ThemePage() {
    const {id} = useParams();
    const navigate = useNavigate();
    const [theme, setTheme] = useState<Theme | null>(null);

    useEffect(() => {
        if (!id) return;
            
        getTheme(parseInt(id)).then(response => {
            const selectedTheme = response.data.find((t: Theme) => t.id.toString() === id);
            setTheme(selectedTheme);
        });
    }, [id]);

    if (!theme) {
        return (
            <Container sx={{display: "flex", justifyContent: "center", alignItems: "center", height: "100vh"}}>
                <CircularProgress/>
            </Container>
        );
    }

    return (
        <Layout>
            <Container maxWidth="md" sx={{mt: 4}}>
                <Button variant="contained" color="secondary" onClick={() => navigate(-1)}>
                    ← Назад
                </Button>

                <Paper elevation={3} sx={{mt: 3, p: 4, borderRadius: 2}}>
                    <Typography variant="h4" align="center" gutterBottom>
                        {theme.title}
                    </Typography>

                    <Box sx={{mt: 2}}>
                        <Typography variant="subtitle1"><strong>Уровень:</strong> {theme.level}</Typography>
                        <Typography variant="subtitle1"><strong>Кафедра:</strong> {theme.department}</Typography>
                        <Typography variant="subtitle1"><strong>Источник:</strong> {theme.source}</Typography>
                        <Typography variant="subtitle1"><strong>{UserRole.SUPERVISOR}:</strong> {theme.supervisor ? `${theme.supervisor.lastName} ${theme.supervisor.firstName} ${theme.supervisor.middleName}` : "Не назначен"}
                        </Typography>
                        <Typography
                            variant="subtitle1"><strong>Консультант:</strong> {theme.consultant ? `${theme.consultant.lastName} ${theme.consultant.firstName} ${theme.consultant.middleName}` : "Не назначен"}
                        </Typography>
                        <Typography variant="subtitle1"><strong>Контакты
                            консультанта:</strong> {theme.consultant?.contact ?? ""}</Typography>
                        <Typography variant="body1" sx={{ mt: 2 }}>
                            <strong>Описание:</strong>
                        </Typography>
                        <div style={{ marginTop: 8 }}>
                            <MDEditor.Markdown
                                source={theme.description || 'Нет описания'}
                                style={{
                                    backgroundColor: 'transparent',
                                    padding: 0,
                                    color: 'inherit', // Inherits text color from parent
                                    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif' // Match MUI typography
                                }}
                            />
                        </div>
                    </Box>
                </Paper>
            </Container>
        </Layout>
    );
}
