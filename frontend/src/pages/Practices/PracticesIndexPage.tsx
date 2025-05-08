import {Layout} from "@shared/ui/layout/Layout.tsx";
import {getJWTToken} from "@/shared/services/localStorage.service.ts";
import {Navigate, useNavigate} from "react-router-dom";
import {useEffect, useState} from "react";
import {getUserPractices} from "@/shared/services/axios.service.ts";
import {Practice} from "@/entities/Practice.ts";
import {
    Container,
    Grid,
    Card,
    CardContent,
    Typography,
    Button,
    Tabs,
    Tab,
    Box,
    Paper,
    Chip,
    Divider
} from "@mui/material";
import AddIcon from '@mui/icons-material/Add';
import { User } from "@/entities/User.ts";
import { getMe } from "@/shared/services/axios.service.ts";

export function PracticesIndexPage() {
    const tokenIsEmpty = getJWTToken() === "";
    const [practices, setPractices] = useState<Practice[]>([]);
    const [me, setMe] = useState<User>();
    const [activeTab, setActiveTab] = useState(0);
    const navigate = useNavigate();

    useEffect(() => {
        getMe().then(response => {
            const data: User = response.data;
            setMe(data);

            getUserPractices(data.userId).then(response => {
                setPractices(response.data);
            });
        });
    }, []);

    const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
        setActiveTab(newValue);
    };

    const activePractices = practices.filter(practice => practice.status != 'Завершено');
    const completedPractices = practices.filter(practice => practice.status == 'Завершено');

    return tokenIsEmpty ? <Navigate to="/login" replace/> : (
        <Layout>
            <Container maxWidth="lg" sx={{mt: 4, p: 4}}>
                <Typography variant="h4" align="center" gutterBottom>
                    Мои практики
                </Typography>

                <Box sx={{display: 'flex', justifyContent: 'flex-end', mb: 3}}>
                    <Button
                        variant="contained"
                        startIcon={<AddIcon/>}
                        onClick={() => navigate("/create/practice")}
                    >
                        Создать практику
                    </Button>
                </Box>

                <Paper sx={{mb: 3}}>
                    <Tabs value={activeTab} onChange={handleTabChange} centered>
                        <Tab label="Активные практики"/>
                        <Tab label="Завершенные практики"/>
                    </Tabs>
                </Paper>

                <Grid container spacing={3}>
                    {activeTab === 0 ? (
                        activePractices.length > 0 ? (
                            activePractices.map((practice, index) => (
                                <Grid item xs={12} key={index}>
                                    <PracticeCard
                                        practice={practice}
                                        onClick={() => navigate(`/practices/${practice.id}`)}
                                    />
                                </Grid>
                            ))
                        ) : (
                            <Grid item xs={12}>
                                <Typography variant="h6" align="center">
                                    Нет активных практик
                                </Typography>
                            </Grid>
                        )
                    ) : (
                        completedPractices.length > 0 ? (
                            completedPractices.map((practice, index) => (
                                <Grid item xs={12} key={index}>
                                    <PracticeCard
                                        practice={practice}
                                        onClick={() => navigate(`/practices/${practice.id}`)}
                                    />
                                </Grid>
                            ))
                        ) : (
                            <Grid item xs={12}>
                                <Typography variant="h6" align="center">
                                    Нет завершенных практик
                                </Typography>
                            </Grid>
                        )
                    )}
                </Grid>
            </Container>
        </Layout>
    );
}

interface PracticeCardProps {
    practice: Practice;
    onClick: () => void;
}

function PracticeCard({ practice, onClick }: PracticeCardProps) {
    return (
        <Card
            sx={{
                cursor: 'pointer',
                '&:hover': {
                    boxShadow: 4
                }
            }}
            onClick={onClick}
        >
            <CardContent>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="h6">
                        {practice.theme?.title || "Без темы"}
                    </Typography>
                    <Chip
                        label={practice.status}
                        color={practice.status === "Завершено" ? "success" : "primary"}
                    />
                </Box>

                <Divider sx={{ my: 2 }} />

                <Typography variant="body2" color="text.secondary">
                    Тип практики: {practice.type}
                </Typography>

                <Typography variant="body2" color="text.secondary">
                    Итоговая оценка: {practice.finalgrade || "Не указана"}
                </Typography>

                <Typography variant="body2" color="text.secondary">
                    Дата создания: {new Date(practice.createddate).toLocaleDateString()}
                </Typography>

                <Typography variant="body2" color="text.secondary">
                    Последнее обновление: {new Date(practice.updateddate).toLocaleDateString()}
                </Typography>
            </CardContent>

        </Card>
    );
}