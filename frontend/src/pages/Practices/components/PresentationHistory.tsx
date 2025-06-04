import { useEffect, useState } from "react";
import {
    Typography,
    TextField,
    Link,
    Grid,
    Paper,
    List,
    ListItem,
    ListItemText,
    Divider,
    Box,
    Button,
    Stack
} from "@mui/material";
import { getMe, getPresentationsByPracticeId, postPresentationComment } from "@/shared/services/axios.service";
import { Presentation } from "@/entities/Presentation";
import { Comment } from "@/entities/Comment";
import { User } from "@/entities/User";

export function PresentationHistory({ practiceId }: any) {
    const [presentations, setPresentations] = useState<Presentation[]>([]);
    const [newComments, setNewComments] = useState<{ [key: string]: string }>({});
    const [user, setUser] = useState<User>();

    const fetchPresentations = async () => {
        try {
            const res = await getPresentationsByPracticeId(practiceId);
            setPresentations(res.data);
        } catch (error) {
            console.error("Ошибка при получении презентаций:", error);
        }
    };

    const handleNewCommentChange = (id: string, value: string) => {
        setNewComments((prev) => ({ ...prev, [id]: value }));
    };

    const handleAddComment = async (presentationId: string) => {
        const commentText = newComments[presentationId]?.trim();
        if (!commentText || !user) return;

        try {
            await postPresentationComment(presentationId, {
                text: commentText,
                author: `${user.lastName} ${user.firstName} ${user.middleName}`,
                createdAt: new Date()
            });
            setNewComments((prev) => ({ ...prev, [presentationId]: "" }));
            fetchPresentations();
        } catch (error) {
            console.error("Ошибка при добавлении комментария:", error);
        }
    };

    useEffect(() => {
        if (practiceId) fetchPresentations();
        getMe().then(res => setUser(res.data));
    }, [practiceId]);

    return (
        <Grid item>
            <Paper elevation={3} sx={{ p: 3, borderRadius: 2 }}>
                <Typography variant="h6" gutterBottom>
                    История версий презентации
                </Typography>

                {presentations.length === 0 ? (
                    <Typography variant="body2" color="text.secondary">
                        Пока нет загруженных версий.
                    </Typography>
                ) : (
                    <List>
                        {presentations
                            .sort((a, b) => (b.version ?? 0) - (a.version ?? 0))
                            .map((presentation) => (
                                <Box key={presentation.id}>
                                    <ListItem>
                                        <ListItemText
                                            primary={
                                                <Typography variant="subtitle1">
                                                    Версия {presentation.version} —{" "}
                                                    <Link href={presentation.link} target="_blank" rel="noopener">
                                                        {presentation.fileName}
                                                    </Link>
                                                </Typography>
                                            }
                                            secondary={`Загружено: ${new Date(presentation.uploadedAt).toLocaleString()}`}
                                        />
                                    </ListItem>

                                    <Box sx={{ pl: 2, pr: 2, pb: 2 }}>
                                        <Typography variant="body2" fontWeight="bold" gutterBottom>
                                            Комментарии:
                                        </Typography>

                                        <Stack spacing={1} sx={{ mb: 2 }}>
                                            {presentation.comments.length === 0 && (
                                                <Typography variant="body2" sx={{ ml: 2 }}>Комментариев нет.</Typography>
                                            )}
                                            {presentation.comments.map((comment: Comment, index: number) => (
                                                <Box key={index} sx={{ ml: 2, mt: 0.5 }}>
                                                    <Typography variant="body2">
                                                        <b>{comment.author}</b> ({new Date(comment.createdAt ?? '').toLocaleString()}): {comment.text}
                                                    </Typography>
                                                </Box>
                                            ))}
                                        </Stack>

                                        <TextField
                                            fullWidth
                                            label="Добавить комментарий"
                                            value={newComments[presentation.id] || ""}
                                            onChange={(e) =>
                                                handleNewCommentChange(presentation.id, e.target.value)
                                            }
                                            multiline
                                            minRows={2}
                                            sx={{ mb: 1 }}
                                        />
                                        <Button
                                            variant="contained"
                                            size="small"
                                            onClick={() => handleAddComment(presentation.id)}
                                        >
                                            Отправить
                                        </Button>
                                    </Box>
                                    <Divider />
                                </Box>
                            ))}
                    </List>
                )}
            </Paper>
        </Grid>
    );
}
