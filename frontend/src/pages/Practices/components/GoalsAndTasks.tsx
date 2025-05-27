import { useState, useEffect } from "react";
import { Grid, Typography, Paper, Button, Box, CircularProgress, Snackbar, Alert } from "@mui/material";
import MDEditor from "@uiw/react-md-editor";
import { getGoalsTasksByPracticeId, postGoalsTasks } from "@/shared/services/axios.service"; // Adjust path if needed
import { GoalsTasks } from "@/entities/GoalsTasks"; // Adjust path if needed

export function GoalsAndTasks({ practiceId }: { practiceId?: number }) {
    const [goals, setGoals] = useState<string>("");
    const [tasks, setTasks] = useState<string>("");
    const [loading, setLoading] = useState<boolean>(false);
    const [saving, setSaving] = useState<boolean>(false);
    const [snackbarOpen, setSnackbarOpen] = useState<boolean>(false);
    const [snackbarMessage, setSnackbarMessage] = useState<string>("");
    const [snackbarSeverity, setSnackbarSeverity] = useState<"success" | "error">("success");

    useEffect(() => {
        if (!practiceId) return;

        setLoading(true);
        getGoalsTasksByPracticeId(practiceId)
            .then((response) => {
                const data: GoalsTasks = response.data;
                setGoals(data.goals ?? "");
                setTasks(data.tasks ?? "");
            })
            .catch((error) => {
                console.warn("No existing goals/tasks found or error:", error);
            })
            .finally(() => setLoading(false));
    }, [practiceId]);

    const handleSave = async () => {
        if (!practiceId) return;
        setSaving(true);

        const input: GoalsTasks = {
            practiceId: practiceId,
            goals: goals,
            tasks: tasks,
        };

        try {
            await postGoalsTasks(input);
            setSnackbarMessage("Цели и задачи успешно сохранены!");
            setSnackbarSeverity("success");
            setSnackbarOpen(true);
        } catch (error) {
            console.error("Failed to save goals/tasks:", error);
            setSnackbarMessage("Ошибка при сохранении целей и задач.");
            setSnackbarSeverity("error");
            setSnackbarOpen(true);
        } finally {
            setSaving(false);
        }
    };

    const handleSnackbarClose = () => {
        setSnackbarOpen(false);
    };

    return (
        <>
            <Grid container direction="column" spacing={4}>
                {loading ? (
                    <Box display="flex" justifyContent="center">
                        <CircularProgress />
                    </Box>
                ) : (
                    <>
                        <Grid item>
                            <Paper elevation={3} sx={{ p: 3, borderRadius: 2 }}>
                                <Typography variant="h6" gutterBottom>Цели работы</Typography>
                                <MDEditor
                                    value={goals}
                                    onChange={(val) => setGoals(val ?? "")}
                                    height={200}
                                    preview="edit"
                                    visibleDragbar={false}
                                    textareaProps={{
                                        placeholder: "Введите цели работы...",
                                    }}
                                    style={{ borderRadius: 8 }}
                                />
                            </Paper>
                        </Grid>

                        <Grid item>
                            <Paper elevation={3} sx={{ p: 3, borderRadius: 2 }}>
                                <Typography variant="h6" gutterBottom>Задачи</Typography>
                                <MDEditor
                                    value={tasks}
                                    onChange={(val) => setTasks(val ?? "")}
                                    height={200}
                                    preview="edit"
                                    visibleDragbar={false}
                                    textareaProps={{
                                        placeholder: "Введите задачи...",
                                    }}
                                    style={{ borderRadius: 8 }}
                                />
                            </Paper>
                        </Grid>

                        <Grid item>
                            <Button
                                variant="contained"
                                color="primary"
                                onClick={handleSave}
                                disabled={saving}
                            >
                                {saving ? "Сохраняем..." : "Сохранить"}
                            </Button>
                        </Grid>
                    </>
                )}
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
