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
import { TextWork } from "../../../entities/TextWork";
import { Comment } from "../../../entities/Comment";
import {
    getMe,
    getTextWorksByPracticeId,
    postTextWorkComment
} from "@/shared/services/axios.service";
import { User } from "@/entities/User";

export function TextWorkHistory({ practiceId }: any) {
    const [textWorks, setTextWorks] = useState<TextWork[]>([]); // Stores all versions of text work
    const [newComments, setNewComments] = useState<{ [key: string]: string }>({}); // Track comment input per text work ID
    const [user, setUser] = useState<User>();
    
    const fetchTextWorks = async () => {
        try {
            const res = await getTextWorksByPracticeId(practiceId);
            setTextWorks(res.data);
        } catch (error) {
            console.error("Ошибка при получении версий текста:", error);
        }
    };
    
    const handleNewCommentChange = (id: string, value: string) => {
        setNewComments((prev) => ({ ...prev, [id]: value }));
    };
    
    const handleAddComment = async (textWorkId: string) => {
        const commentText = newComments[textWorkId]?.trim();
        if (!commentText) return;

        try {
            console.log(user)
            await postTextWorkComment(textWorkId, {
                text: commentText,
                author: `${user?.lastName} ${user?.firstName} ${user?.middleName}`,
                createdAt: new Date(),
            });
            setNewComments((prev) => ({ ...prev, [textWorkId]: "" }));
            fetchTextWorks();
        } catch (error) {
            console.error("Ошибка при добавлении комментария:", error);
        }
    };
    
    useEffect(() => {
        if (practiceId) fetchTextWorks();
        getMe().then(res => setUser(res.data));
    }, [practiceId]);

    return (
        <Grid item>
            <Paper elevation={3} sx={{ p: 3, borderRadius: 2 }}>
                <Typography variant="h6" gutterBottom>
                    История версий текста работы
                </Typography>
                
                {textWorks.length === 0 ? (
                    <Typography variant="body2" color="text.secondary">
                        Пока нет загруженных версий.
                    </Typography>
                ) : (
                    <List>
                        {textWorks
                            .sort((a, b) => (b.version ?? 0) - (a.version ?? 0)) // Show latest version first
                            .map((work) => (
                                <Box key={work.id}>
                                    <ListItem>
                                        <ListItemText
                                            primary={
                                                <Typography variant="subtitle1">
                                                    Версия {work.version} —{" "}
                                                    <Link href={work.link} target="_blank" rel="noopener">
                                                        {work.fileName}
                                                    </Link>
                                                </Typography>
                                            }
                                            secondary={`Загружено: ${new Date(work.uploadedAt).toLocaleString()}`}
                                        />
                                    </ListItem>

                                    <Box sx={{ pl: 2, pr: 2, pb: 2 }}>
                                        <Typography variant="body2" fontWeight="bold" gutterBottom>
                                            Комментарии:
                                        </Typography>
                                        
                                        <Stack spacing={1} sx={{ mb: 2 }}>
                                            {work.comments.length === 0 && (
                                                <Typography variant="body2" sx={{ ml: 2 }}>Комментариев нет.</Typography>
                                            )}
                                            {work.comments.map((comment: Comment, index: number) => (
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
                                            value={newComments[work.id] || ""}
                                            onChange={(e) =>
                                                handleNewCommentChange(work.id, e.target.value)
                                            }
                                            multiline
                                            minRows={2}
                                            sx={{ mb: 1 }}
                                        />
                                        <Button
                                            variant="contained"
                                            size="small"
                                            onClick={() => handleAddComment(work.id)}
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
