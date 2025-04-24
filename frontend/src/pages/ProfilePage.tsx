import { Layout } from "@shared/ui/layout/Layout.tsx";
import { getJWTToken } from "../shared/services/localStorage.service.ts";
import { Navigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { getMe } from "../shared/services/axios.service.ts";
import {
    Container,
    Paper,
    Typography,
    Avatar,
    Box,
    Button,
    Grid
} from "@mui/material";
import EditIcon from '@mui/icons-material/Edit';
import { User } from "@/entities/User.ts";

export function ProfilePage() {
    const tokenIsEmpty = getJWTToken() === "";
    const [user, setUser] = useState<User>();
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        getMe().then(response => {
            const data: User = response.data
            console.log(data)
            setUser(data);
            setLoading(false);
        }).catch(() => {
            setLoading(false);
        });
    }, []);

    if (tokenIsEmpty) {
        return <Navigate to="/login" replace />;
    }

    if (loading) {
        return (
            <Layout>
                <Container maxWidth="md" sx={{ mt: 4 }}>
                    <Typography variant="h6">Загрузка...</Typography>
                </Container>
            </Layout>
        );
    }

    const fullName = `${user.lastName} ${user.firstName} ${user.middleName}`.trim();

    return (
        <Layout>
            <Container maxWidth="md" sx={{ mt: 4 }}>
                <Paper elevation={3} sx={{ p: 4 }}>
                    {/*<Box display="flex" justifyContent="space-between" alignItems="center" mb={4}>*/}
                    {/*    <Typography variant="h4">Профиль пользователя</Typography>*/}
                    {/*    <Button*/}
                    {/*        variant="contained"*/}
                    {/*        startIcon={<EditIcon />}*/}
                    {/*        onClick={() => /!* Add edit functionality *!/}*/}
                    {/*    >*/}
                    {/*        Редактировать*/}
                    {/*    </Button>*/}
                    {/*</Box>*/}

                    <Box display="flex" alignItems="center" gap={4} mb={4}>
                        <Avatar
                            sx={{
                                width: 100,
                                height: 100,
                                fontSize: 40,
                                bgcolor: 'primary.main'
                            }}
                        >
                        </Avatar>

                        <Box>
                            <Typography variant="h5" component="div">
                                {fullName || "Не указано"}
                            </Typography>
                            <Typography variant="subtitle1" color="text.secondary">
                                {user.userName}
                            </Typography>
                        </Box>
                    </Box>

                    <Grid container spacing={3}>
                        <Grid item xs={12} md={6}>
                            <Typography variant="subtitle1" color="text.secondary">
                                Email
                            </Typography>
                            <Typography variant="body1" paragraph>
                                {user.email || "Не указан"}
                            </Typography>
                        </Grid>
                    </Grid>
                </Paper>
            </Container>
        </Layout>
    );
}