import { useState } from "react";
import { Grid, Typography, Paper, Button, TextField, Box, InputLabel } from "@mui/material";
import UploadFileIcon from '@mui/icons-material/UploadFile';

export function Attachments({ practiceId }: { practiceId?: number }) {
    const [textFile, setTextFile] = useState<File | null>(null);
    const [textLink, setTextLink] = useState("");
    const [supervisorReview, setSupervisorReview] = useState<File | null>(null);
    const [consultantReview, setConsultantReview] = useState<File | null>(null);
    const [presentationFile, setPresentationFile] = useState<File | null>(null);
    const [presentationLink, setPresentationLink] = useState("");
    const [codeLink, setCodeLink] = useState("");
    const [accountName, setAccountName] = useState("");

    const handleFileChange = (setter: (file: File | null) => void) => (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files && e.target.files.length > 0) {
            setter(e.target.files[0]);
        } else {
            setter(null);
        }
    };

    const handleSubmit = (section: string) => {
        console.log(`Submitting ${section}`);
    };

    return (
        <Grid container spacing={4} direction="column">
            <Grid item>
                <Typography variant="h5">Загрузка файлов и ссылок</Typography>
            </Grid>
            
            <Grid item>
                <Paper elevation={3} sx={{ p: 3, borderRadius: 2 }}>
                    <Typography variant="h6">Текст работы</Typography>
                    <Button variant="outlined" component="label" startIcon={<UploadFileIcon />} sx={{ mt: 2 }}>
                        Загрузить текст работы (PDF)
                        <input type="file" hidden accept=".pdf" onChange={handleFileChange(setTextFile)} />
                    </Button>
                    <TextField
                        fullWidth
                        label="Ссылка на текст работы (например, Google Docs)"
                        value={textLink}
                        onChange={(e) => setTextLink(e.target.value)}
                        sx={{ mt: 2 }}
                    />
                    <Button variant="contained" sx={{ mt: 2 }} onClick={() => handleSubmit("text")}>
                        Сохранить текст
                    </Button>
                </Paper>
            </Grid>
            
            <Grid item>
                <Paper elevation={3} sx={{ p: 3, borderRadius: 2 }}>
                    <Typography variant="h6">Отзывы</Typography>
                    <Button variant="outlined" component="label" startIcon={<UploadFileIcon />} sx={{ mt: 2 }}>
                        Загрузить отзыв научного руководителя (PDF)
                        <input type="file" hidden accept=".pdf" onChange={handleFileChange(setSupervisorReview)} />
                    </Button>
                    <Button variant="outlined" component="label" startIcon={<UploadFileIcon />} sx={{ mt: 2 }}>
                        Загрузить отзыв консультанта (PDF)
                        <input type="file" hidden accept=".pdf" onChange={handleFileChange(setConsultantReview)} />
                    </Button>
                    <Button variant="contained" sx={{ mt: 2 }} onClick={() => handleSubmit("reviews")}>
                        Сохранить отзывы
                    </Button>
                </Paper>
            </Grid>
            
            <Grid item>
                <Paper elevation={3} sx={{ p: 3, borderRadius: 2 }}>
                    <Typography variant="h6">Презентация</Typography>
                    <Button variant="outlined" component="label" startIcon={<UploadFileIcon />} sx={{ mt: 2 }}>
                        Загрузить презентацию (PDF)
                        <input type="file" hidden accept=".pdf" onChange={handleFileChange(setPresentationFile)} />
                    </Button>
                    <TextField
                        fullWidth
                        label="Ссылка на презентацию (например, Google Docs)"
                        value={presentationLink}
                        onChange={(e) => setPresentationLink(e.target.value)}
                        sx={{ mt: 2 }}
                    />
                    <Button variant="contained" sx={{ mt: 2 }} onClick={() => handleSubmit("presentation")}>
                        Сохранить презентацию
                    </Button>
                </Paper>
            </Grid>
            
            <Grid item>
                <Paper elevation={3} sx={{ p: 3, borderRadius: 2 }}>
                    <Typography variant="h6">Реализация</Typography>
                    <TextField
                        fullWidth
                        label="Ссылка на репозиторий или укажите, что код закрыт"
                        value={codeLink}
                        onChange={(e) => setCodeLink(e.target.value)}
                        sx={{ mt: 2 }}
                    />
                    <TextField
                        fullWidth
                        label="Имя аккаунта (если проект групповой)"
                        value={accountName}
                        onChange={(e) => setAccountName(e.target.value)}
                        sx={{ mt: 2 }}
                    />
                    <Button variant="contained" sx={{ mt: 2 }} onClick={() => handleSubmit("implementation")}>
                        Сохранить реализацию
                    </Button>
                </Paper>
            </Grid>
        </Grid>
    );
}
