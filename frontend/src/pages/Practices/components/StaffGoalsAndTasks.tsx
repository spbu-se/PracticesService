import { useState, useEffect } from "react";
import { Grid, Typography, Paper, Box, CircularProgress } from "@mui/material";
import MDEditor from "@uiw/react-md-editor";
import { getGoalsTasksByPracticeId } from "@/shared/services/axios.service";
import { GoalsTasks } from "@/entities/GoalsTasks";

export function StaffGoalsAndTasks({ practiceId }: { practiceId?: number }) {
    const [goals, setGoals] = useState<string>("");
    const [tasks, setTasks] = useState<string>("");
    const [loading, setLoading] = useState<boolean>(false);

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
                console.warn("Ошибка при получении целей/задач:", error);
            })
            .finally(() => setLoading(false));
    }, [practiceId]);

    return (
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
                            <MDEditor.Markdown
                                source={goals || "Цели работы пока не заданы."}
                                style={{ backgroundColor: "transparent" }}
                            />
                        </Paper>
                    </Grid>

                    <Grid item>
                        <Paper elevation={3} sx={{ p: 3, borderRadius: 2 }}>
                            <Typography variant="h6" gutterBottom>Задачи</Typography>
                            <MDEditor.Markdown
                                source={tasks || "Задачи пока не заданы."}
                                style={{ backgroundColor: "transparent" }}
                            />
                        </Paper>
                    </Grid>
                </>
            )}
        </Grid>
    );
}
