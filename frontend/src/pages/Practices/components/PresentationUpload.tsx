import {useEffect, useState } from "react";
import { Typography, Button, TextField, Link, Grid, Paper } from "@mui/material";
import UploadFileIcon from '@mui/icons-material/UploadFile';
import {getLatestPresentationVersion, postPresentation} from "@/shared/services/axios.service";
import {Presentation} from "@/entities/Presentation";
import { useNotifier } from "@/shared/ui/notifications/notificationContext";

export function PresentationUpload({ practiceId }: { practiceId: number }) {
    const notifier = useNotifier();
    const [presentationFile, setPresentationFile] = useState<File | null>(null);
    const [presentationLink, setPresentationLink] = useState("");

    const [presentationVersion, setPresentationVersion] = useState(1);
    const [latestPresentation, setLatestPresentation] = useState<Presentation>();

    const handleSubmit = async () => {
        if (!practiceId || (!presentationLink.trim() && !presentationFile)) {
            notifier.notify("Пожалуйста, укажите ссылку или выберите файл презентации.", "error");
            return;
        }

        try {
            await postPresentation({
                practiceId,
                link: presentationLink.trim(),
                version: presentationVersion,
                file: presentationFile,
            });

            notifier.notify("Презентация успешно загружена!", "success");
            setPresentationFile(null);
            setPresentationVersion(prev => prev + 1);
        } catch (error) {
            console.error("Ошибка загрузки презентации:", error);
            notifier.notify("Не удалось загрузить презентацию.", "error");
        }
    }

    useEffect(() => {
        getLatestPresentationVersion(practiceId)
            .then(presentation => {
                if (!presentation) return;
                setLatestPresentation(presentation);
                setPresentationVersion(presentation.version + 1);
            })
            .catch(() => setPresentationVersion(1));
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
                <Typography variant="h6">Презентация</Typography>
                <Button variant="outlined" component="label" startIcon={<UploadFileIcon />} sx={{ mt: 2 }}>
                    Загрузить презентацию (PDF)
                    <input type="file" hidden accept=".pdf" onChange={handleFileChange(setPresentationFile)} />
                </Button>
                {presentationFile && (
                    <Typography variant="body2" sx={{ mt: 1 }}>
                        Выбран файл для презентации: {presentationFile.name}
                    </Typography>
                )}
                <TextField
                    fullWidth
                    label="Ссылка на презентацию (например, Google Docs)"
                    value={presentationLink}
                    onChange={(e) => setPresentationLink(e.target.value)}
                    sx={{ mt: 2 }}
                />
                <Button variant="contained" sx={{ mt: 2 }} onClick={() => handleSubmit()}>
                    Сохранить презентацию
                </Button>

                {latestPresentation && (
                    <Typography sx={{ mt: 2 }}>
                        Последняя загруженная презентация:{" "}
                        <Link href={latestPresentation.link} target="_blank" rel="noopener">
                            {latestPresentation.fileName}
                        </Link>
                    </Typography>
                )}
            </Paper>
        </Grid>
    );
}


