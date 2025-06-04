import {useEffect, useState } from "react";
import { Typography, Button, Link, Grid, Paper } from "@mui/material";
import UploadFileIcon from '@mui/icons-material/UploadFile';
import {getFeedbacksByPracticeId, postFeedback} from "@/shared/services/axios.service";
import { Feedback } from "@/entities/Feedback";

export function FeedbackUpload({ practiceId, setSnackbarMessage, setSnackbarSeverity, setSnackbarOpen }: any) {
    const [feedbacks, setFeedbacks] = useState<Feedback[]>([]);
    const [supervisorReview, setSupervisorReview] = useState<File | null>(null);
    const [consultantReview, setConsultantReview] = useState<File | null>(null);

    const supervisorFeedback = feedbacks.find(f => f.feedbackType == "Supervisor");
    const consultantFeedback = feedbacks.find(f => f.feedbackType == "Consultant");

    const handleSubmit = async () => {
        try {
            if (supervisorReview) {
                await postFeedback({
                    practiceId: practiceId!,
                    feedbackType: "Supervisor",
                    file: supervisorReview,
                });
            }

            if (consultantReview) {
                await postFeedback({
                    practiceId: practiceId!,
                    feedbackType: "Consultant",
                    file: consultantReview,
                });
            }

            const res = await getFeedbacksByPracticeId(practiceId);
            setFeedbacks(res.data);

            setSnackbarMessage("Отзывы успешно загружены!");
            setSnackbarSeverity("success");
            setSnackbarOpen(true);
        } catch (error) {
            console.error("Ошибка загрузки отзывов:", error);
            setSnackbarMessage("Не удалось загрузить отзывы.");
            setSnackbarSeverity("error");
            setSnackbarOpen(true);
        } finally {
            setSupervisorReview(null);
            setConsultantReview(null);
        }
    }

    useEffect(() => {
        getFeedbacksByPracticeId(practiceId).then(res => {
            setFeedbacks(res.data);
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

                {supervisorReview && (
                    <Typography variant="body2" sx={{ mt: 1 }}>
                        Выбран файл для научного руководителя: {supervisorReview.name}
                    </Typography>
                )}

                {consultantReview && (
                    <Typography variant="body2" sx={{ mt: 1 }}>
                        Выбран файл для консультанта: {consultantReview.name}
                    </Typography>
                )}

                {supervisorFeedback && (
                    <Typography sx={{ mt: 2 }}>
                        Отзыв научного руководителя:{" "}
                        <Link href={supervisorFeedback.link} target="_blank" rel="noopener">
                            {supervisorFeedback.fileName}
                        </Link>
                    </Typography>
                )}

                {consultantFeedback && (
                    <Typography sx={{ mt: 2 }}>
                        Отзыв консультанта:{" "}
                        <Link href={consultantFeedback.link} target="_blank" rel="noopener">
                            {consultantFeedback.fileName}
                        </Link>
                    </Typography>
                )}
            </Paper>
        </Grid>
    );
}


