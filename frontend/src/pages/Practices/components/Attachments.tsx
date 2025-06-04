import {useEffect, useState } from "react";
import { Grid, Typography, Paper, Button, TextField } from "@mui/material";
import { postRepository, getRepositoryByPracticeId } from "@/shared/services/axios.service"; 
import { Repository } from "@/entities/Repository"; 
import { Snackbar, Alert } from "@mui/material";
import { TextWorkUpload } from "./TextWorkUpload";
import { FeedbackUpload } from "./FeedbackUpload";
import { PresentationUpload } from "./PresentationUpload";



export function Attachments({ practiceId }: { practiceId?: number }) {
    const [codeLink, setCodeLink] = useState("");
    const [accountName, setAccountName] = useState("");
    const [snackbarOpen, setSnackbarOpen] = useState(false);
    const [snackbarMessage, setSnackbarMessage] = useState("");
    const [snackbarSeverity, setSnackbarSeverity] = useState<"success" | "error">("success");

    const handleSnackbarClose = () => setSnackbarOpen(false);

    const handleSubmit = async () => {
        if (!practiceId || !codeLink.trim()) {
            setSnackbarMessage("Пожалуйста, укажите ссылку на репозиторий или пометьте, что код закрыт.");
            setSnackbarSeverity("error");
            setSnackbarOpen(true);
            return;
        }
        const newRepository: Omit<Repository, "id" | "uploadedAt"> = {
            practiceId,
            repositoryLink: codeLink.trim(),
            accountName: accountName.trim(),
        };
        try {
            await postRepository(newRepository);
            setSnackbarMessage("Реализация успешно сохранена!");
            setSnackbarSeverity("success");
            setSnackbarOpen(true);
        } catch (error) {
            console.error("Error saving repository:", error);
            setSnackbarMessage("Ошибка сохранения реализации.");
            setSnackbarSeverity("error");
            setSnackbarOpen(true);
        }
    };

    useEffect(() => {
        getRepositoryByPracticeId(practiceId).then(res => {
            const repo: Repository = res.data;
            
            setAccountName(repo.accountName);
            setCodeLink(repo.repositoryLink);
        })
    }, []);

    return (
        <Grid container spacing={4} direction="column">
            <Grid item>
                <Typography variant="h5">Загрузка файлов и ссылок</Typography>
            </Grid>
            
            <TextWorkUpload practiceId={practiceId} setSnackbarMessage={setSnackbarMessage} setSnackbarSeverity={setSnackbarSeverity} setSnackbarOpen={setSnackbarOpen}/>

            <FeedbackUpload practiceId={practiceId} setSnackbarMessage={setSnackbarMessage} setSnackbarSeverity={setSnackbarSeverity} setSnackbarOpen={setSnackbarOpen}/>

            <PresentationUpload practiceId={practiceId} setSnackbarMessage={setSnackbarMessage} setSnackbarSeverity={setSnackbarSeverity} setSnackbarOpen={setSnackbarOpen}/>
            
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
                    <Button variant="contained" sx={{ mt: 2 }} onClick={() => handleSubmit()}>
                        Сохранить реализацию
                    </Button>
                </Paper>
            </Grid>
            
            <Snackbar
                open={snackbarOpen}
                autoHideDuration={4000}
                onClose={handleSnackbarClose}
                anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
            >
                <Alert onClose={handleSnackbarClose} severity={snackbarSeverity} sx={{ width: "100%" }}>
                    {snackbarMessage}
                </Alert>
            </Snackbar>

        </Grid>
    );
}
