import {useEffect, useState } from "react";
import { Typography, Button, TextField, Link, Grid, Paper } from "@mui/material";
import UploadFileIcon from '@mui/icons-material/UploadFile';
import {TextWork} from "../../../entities/TextWork";
import {getLatestTextWorkVersion, postTextWork } from "@/shared/services/axios.service";

export function TextWorkUpload({ practiceId, setSnackbarMessage, setSnackbarSeverity, setSnackbarOpen }: any) {
    const [textFile, setTextFile] = useState<File | null>(null);
    const [textLink, setTextLink] = useState("");
    
    const [latestTextWork, setLatestTextWorks] = useState<TextWork>();
    
    const handleSubmit = async () => {
        if (!practiceId || (!textLink.trim() && !textFile)) {
            setSnackbarMessage("Пожалуйста, укажите ссылку или выберите файл текста работы.");
            setSnackbarSeverity("error");
            setSnackbarOpen(true);
            return;
        }
        try {
            await postTextWork({
                practiceId: practiceId,
                link: textLink.trim(),
                version: latestTextWork?.version ?? 0 + 1,
                file: textFile,
            });

            setSnackbarMessage("Текст работы успешно загружен!");
            setSnackbarSeverity("success");
            setSnackbarOpen(true);
            setTextFile(null);
            latestTextWork.version = latestTextWork?.version ?? 0 + 1;
            setLatestTextWorks(latestTextWork);
        } catch (error) {
            console.error("Ошибка загрузки текста:", error);
            setSnackbarMessage("Не удалось загрузить текст работы.");
            setSnackbarSeverity("error");
            setSnackbarOpen(true);
        }
    }

    useEffect(() => {
        getLatestTextWorkVersion(practiceId)
            .then(res => {
                const text: TextWork = res.data;
                setLatestTextWorks(text);
            });
    }, [practiceId]);

    const handleFileChange = (setter: (file: File | null) => void) => (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files && e.target.files.length > 0) {
            setter(e.target.files[0]);
        } else {
            setter(null);
        }
    };

    return (
        <Grid item>
            <Paper elevation={3} sx={{ p: 3, borderRadius: 2 }}>
                <Typography variant="h6">Текст работы</Typography>
                <Button variant="outlined" component="label" startIcon={<UploadFileIcon />} sx={{ mt: 2 }}>
                    Загрузить текст работы (PDF)
                    <input type="file" hidden accept=".pdf" onChange={handleFileChange(setTextFile)} />
                </Button>
                {textFile && (
                    <Typography variant="body2" sx={{ mt: 1 }}>
                        Выбран файл: {textFile.name}
                    </Typography>
                )}
                <TextField
                    fullWidth
                    label="Ссылка на текст работы (например, Google Docs)"
                    value={textLink}
                    onChange={(e) => setTextLink(e.target.value)}
                    sx={{ mt: 2 }}
                />
                <Button variant="contained" sx={{ mt: 2 }} onClick={() => handleSubmit()}>
                    Сохранить текст
                </Button>

                {latestTextWork && (
                    <Typography sx={{ mt: 2 }}>
                        Последний загруженный текст:{" "}
                        <Link href={latestTextWork.link} target="_blank" rel="noopener">
                            {latestTextWork.fileName}
                        </Link>
                    </Typography>
                )}
            </Paper>
        </Grid>
    );
}


