import { useState, useEffect } from "react";
import {
    Grid, Typography, Paper, Box, List, ListItem, ListItemText, Divider,
    Snackbar, Alert, CircularProgress, TextField, Button
} from "@mui/material";
import { getReportsByPracticeId, postCommentToReport, getMe } from "@/shared/services/axios.service";
import { Report } from "@/entities/Report";
import { Comment } from "@/entities/Comment";
import { User } from "@/entities/User";

export function StaffReporting({ practiceId }: { practiceId?: number }) {
    const [user, setUser] = useState<User>();
    const [reports, setReports] = useState<Report[]>([]);
    const [loading, setLoading] = useState<boolean>(false);
    const [snackbarOpen, setSnackbarOpen] = useState<boolean>(false);
    const [snackbarMessage, setSnackbarMessage] = useState<string>("");
    const [snackbarSeverity, setSnackbarSeverity] = useState<"success" | "error">("success");
    const [newComments, setNewComments] = useState<{ [reportId: string]: string }>({});
    const [commentSubmitting, setCommentSubmitting] = useState<{ [reportId: string]: boolean }>({});

    useEffect(() => {
        if (!practiceId) return;

        setLoading(true);
        getMe().then(response => setUser(response.data));
        getReportsByPracticeId(practiceId)
            .then(response => setReports(response.data))
            .catch(err => {
                console.error("Error fetching reports:", err);
                setSnackbarMessage("Ошибка загрузки отчетов.");
                setSnackbarSeverity("error");
                setSnackbarOpen(true);
            })
            .finally(() => setLoading(false));
    }, [practiceId]);

    const handleCommentChange = (reportId: string, text: string) => {
        setNewComments(prev => ({ ...prev, [reportId]: text }));
    };

    const handleSubmitComment = async (reportId: string) => {
        const commentText = newComments[reportId]?.trim();
        if (!commentText) return;

        setCommentSubmitting(prev => ({ ...prev, [reportId]: true }));
        const newComment: Comment = {
            author: `${user?.lastName} ${user?.firstName} ${user?.middleName}`,
            text: commentText,
            createdAt: new Date(),
        };

        try {
            await postCommentToReport(reportId, newComment);
            setSnackbarMessage("Комментарий добавлен!");
            setSnackbarSeverity("success");
            setSnackbarOpen(true);
            setNewComments(prev => ({ ...prev, [reportId]: "" }));
            const updatedReports = await getReportsByPracticeId(practiceId);
            setReports(updatedReports.data);
        } catch (error) {
            console.error("Error adding comment:", error);
            setSnackbarMessage("Ошибка при добавлении комментария.");
            setSnackbarSeverity("error");
            setSnackbarOpen(true);
        } finally {
            setCommentSubmitting(prev => ({ ...prev, [reportId]: false }));
        }
    };

    const handleSnackbarClose = () => setSnackbarOpen(false);

    return (
        <>
            <Grid container direction="column" spacing={4}>
                <Grid item>
                    <Typography variant="h5" gutterBottom>Отчеты по практике</Typography>
                </Grid>

                <Grid item>
                    <Paper elevation={3} sx={{ p: 3, borderRadius: 2 }}>
                        {loading ? (
                            <Box display="flex" justifyContent="center">
                                <CircularProgress />
                            </Box>
                        ) : reports.length > 0 ? (
                            <List>
                                {reports.map(report => (
                                    <Box key={report.id} sx={{ mb: 2 }}>
                                        <ListItem alignItems="flex-start">
                                            <ListItemText
                                                primary={`Отчет от ${new Date(report.createdAt!).toLocaleString()}`}
                                                secondary={
                                                    <>
                                                        <Typography variant="body2"><b>Сделано:</b></Typography>
                                                        <Typography variant="body2" sx={{ ml: 2 }}>{report.done}</Typography>
                                                        <Typography variant="body2" sx={{ mt: 1 }}><b>Планируется:</b></Typography>
                                                        <Typography variant="body2" sx={{ ml: 2 }}>{report.planned}</Typography>

                                                        <Box sx={{ mt: 1 }}>
                                                            <Typography variant="subtitle2">Комментарии:</Typography>
                                                            {report?.comments?.length > 0 ? (
                                                                report?.comments.map((comment, idx) => (
                                                                    <Box key={idx} sx={{ ml: 2, mt: 0.5 }}>
                                                                        <Typography variant="body2">
                                                                            <b>{comment.author}</b> ({new Date(comment.createdAt ?? '').toLocaleString()}): {comment.text}
                                                                        </Typography>
                                                                    </Box>
                                                                ))
                                                            ) : (
                                                                <Typography variant="body2" sx={{ ml: 2 }}>Комментариев нет.</Typography>
                                                            )}
                                                            <Box sx={{ mt: 1, display: "flex", gap: 1 }}>
                                                                <TextField
                                                                    size="small"
                                                                    label="Новый комментарий"
                                                                    variant="outlined"
                                                                    value={newComments[report.id!] || ""}
                                                                    onChange={(e) => handleCommentChange(report.id!, e.target.value)}
                                                                    fullWidth
                                                                />
                                                                <Button
                                                                    variant="contained"
                                                                    size="small"
                                                                    disabled={commentSubmitting[report.id!]}
                                                                    onClick={() => handleSubmitComment(report.id!)}
                                                                >
                                                                    {commentSubmitting[report.id!] ? "Отправка..." : "Отправить"}
                                                                </Button>
                                                            </Box>
                                                        </Box>
                                                    </>
                                                }
                                            />
                                        </ListItem>
                                        <Divider />
                                    </Box>
                                ))}
                            </List>
                        ) : (
                            <Typography variant="body2">Отчетов пока нет.</Typography>
                        )}
                    </Paper>
                </Grid>
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
        </>
    );
}
