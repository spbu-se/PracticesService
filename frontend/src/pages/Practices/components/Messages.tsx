import { useState, useEffect, useRef } from "react";
import {
    Grid, Typography, Paper, Button, Box, List, ListItem, ListItemText, Divider,
    Snackbar, Alert, CircularProgress, TextField
} from "@mui/material";
import { getMessagesByPracticeId, postMessage, getMe } from "@/shared/services/axios.service";
import { Message } from "@/entities/Message";
import { User } from "@/entities/User";

export function Messages({ practiceId }: { practiceId?: number }) {
    const [user, setUser] = useState<User>();
    const [messages, setMessages] = useState<Message[]>([]);
    const [newMessage, setNewMessage] = useState<string>("");
    const [loading, setLoading] = useState<boolean>(false);
    const [submitting, setSubmitting] = useState<boolean>(false);
    const [snackbarOpen, setSnackbarOpen] = useState<boolean>(false);
    const [snackbarMessage, setSnackbarMessage] = useState<string>("");
    const [snackbarSeverity, setSnackbarSeverity] = useState<"success" | "error">("success");
    const messagesEndRef = useRef<HTMLDivElement | null>(null);

    useEffect(() => {
        if (!practiceId) return;

        setLoading(true);
        getMe().then(response => setUser(response.data));
        loadMessages();
    }, [practiceId]);

    const loadMessages = () => {
        getMessagesByPracticeId(practiceId!)
            .then(response => setMessages(response.data.reverse()))  // reverse для новых сверху
            .catch(err => {
                console.error("Error fetching messages:", err);
                setSnackbarMessage("Ошибка загрузки сообщений.");
                setSnackbarSeverity("error");
                setSnackbarOpen(true);
            })
            .finally(() => setLoading(false));
    };

    const handleSubmitMessage = async () => {
        if (!newMessage.trim() || !practiceId) return;

        setSubmitting(true);
        const message: Message = {
            practiceId,
            author: `${user?.lastName} ${user?.firstName} ${user?.middleName}`,
            text: newMessage.trim(),
            createdAt: new Date(),
        };

        try {
            await postMessage(message);
            setSnackbarMessage("Сообщение отправлено!");
            setSnackbarSeverity("success");
            setSnackbarOpen(true);
            setNewMessage("");
            loadMessages();
        } catch (error) {
            console.error("Error posting message:", error);
            setSnackbarMessage("Ошибка отправки сообщения.");
            setSnackbarSeverity("error");
            setSnackbarOpen(true);
        } finally {
            setSubmitting(false);
        }
    };

    const handleSnackbarClose = () => setSnackbarOpen(false);

    useEffect(() => {
        if (messagesEndRef.current) {
            messagesEndRef.current.scrollIntoView({ behavior: "smooth" });
        }
    }, [messages]);

    return (
        <Grid container direction="column" spacing={2}>
            <Grid item>
                <Typography variant="h5" gutterBottom>Сообщения</Typography>
            </Grid>

            <Grid item>
                <Paper elevation={3} sx={{ p: 2, borderRadius: 2 }}>
                    <TextField
                        label="Введите сообщение"
                        fullWidth
                        multiline
                        minRows={2}
                        value={newMessage}
                        onChange={(e) => setNewMessage(e.target.value)}
                        disabled={submitting}
                        sx={{ mb: 2 }}
                    />
                    <Box display="flex" justifyContent="flex-end">
                        <Button
                            variant="contained"
                            onClick={handleSubmitMessage}
                            disabled={submitting}
                        >
                            {submitting ? "Отправляем..." : "Отправить"}
                        </Button>
                    </Box>
                </Paper>
            </Grid>

            <Grid item>
                <Paper elevation={3} sx={{ p: 2, borderRadius: 2, maxHeight: 400, overflowY: "auto" }}>
                    {loading ? (
                        <Box display="flex" justifyContent="center">
                            <CircularProgress />
                        </Box>
                    ) : messages.length > 0 ? (
                        <List>
                            {messages.map((msg, idx) => (
                                <Box key={idx}>
                                    <ListItem alignItems="flex-start">
                                        <ListItemText
                                            primary={`${msg.author} (${new Date(msg.createdAt ?? '').toLocaleString()})`}
                                            secondary={msg.text}
                                        />
                                    </ListItem>
                                    <Divider />
                                </Box>
                            ))}
                            <div ref={messagesEndRef} />
                        </List>
                    ) : (
                        <Typography variant="body2">Сообщений пока нет.</Typography>
                    )}
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
