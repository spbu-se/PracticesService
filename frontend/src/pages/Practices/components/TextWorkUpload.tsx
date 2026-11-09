import {useEffect, useState } from "react";
import { Typography, Button, TextField, Link, Grid, Paper } from "@mui/material";
import UploadFileIcon from '@mui/icons-material/UploadFile';
import {TextWork} from "../../../entities/TextWork";
import {getLatestTextWorkVersion, postTextWork } from "@/shared/services/axios.service";
import { useNotifier } from "@/shared/ui/notifications/notificationContext";

export function TextWorkUpload({ practiceId }: { practiceId: number }) {
    const notifier = useNotifier();
    const [textFile, setTextFile] = useState<File | null>(null);
    const [textLink, setTextLink] = useState("");

    const [textVersion, setTextVersion] = useState(1);
    const [latestTextWork, setLatestTextWorks] = useState<TextWork>();
    
    const handleSubmit = async () => {
        if (!practiceId || (!textLink.trim() && !textFile)) {
            notifier.notify("Пожалуйста, укажите ссылку или выберите файл текста работы.", "error");
            return;
        }
        try {
            await postTextWork({
                practiceId: practiceId,
                link: textLink.trim(),
                version: textVersion,
                file: textFile,
            });

            notifier.notify("Текст работы успешно загружен!", "success");
            setTextFile(null);
            setTextVersion((prev) => prev + 1);
        } catch (error) {
            console.error("Ошибка загрузки текста:", error);
            notifier.notify("Не удалось загрузить текст работы.", "error");
        }
    }

    useEffect(() => {
        getLatestTextWorkVersion(practiceId)
            .then(text => {
                if (!text) return;
                setLatestTextWorks(text);
                setTextVersion(text.version + 1);
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
        <Grid size="auto">
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


