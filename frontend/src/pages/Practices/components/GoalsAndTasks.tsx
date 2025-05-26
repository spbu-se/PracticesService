import { useState } from "react";
import { Grid, Typography, Box, Paper } from "@mui/material";
import MDEditor from "@uiw/react-md-editor";

export function GoalsAndTasks({ practiceId }: { practiceId?: number }) {
    const [goals, setGoals] = useState("");
    const [tasks, setTasks] = useState("");

    return (
        <Grid container direction="column" spacing={4}>
            <Grid item>
                <Paper elevation={3} sx={{ p: 3, borderRadius: 2 }}>
                    <Typography variant="h6" gutterBottom>Цели работы</Typography>
                    <MDEditor
                        value={goals}
                        onChange={(val) => setGoals(val)}
                        height={200}
                        preview="edit"
                        visibleDragbar={false}
                        textareaProps={{
                            placeholder: "Введите цели работы..."
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
                        onChange={(val) => setTasks(val)}
                        height={200}
                        preview="edit"
                        visibleDragbar={false}
                        textareaProps={{
                            placeholder: "Введите задачи..."
                        }}
                        style={{ borderRadius: 8 }}
                    />
                </Paper>
            </Grid>
        </Grid>
    );
}
