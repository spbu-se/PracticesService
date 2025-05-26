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
    Divider,
    List,
    ListItem,
    ListItemText,
    Link
} from "@mui/material";
import AddIcon from '@mui/icons-material/Add';
import { User } from "@/entities/User.ts";
import { getMe } from "@/shared/services/axios.service.ts";
import { HandbookTab } from "./HandbookTab";
import { PracticeCard } from "./PracticeCard";

export function PracticesIndexPage() {
    const tokenIsEmpty = getJWTToken() === "";
    const [practices, setPractices] = useState<Practice[]>([]);
    const [me, setMe] = useState<User>();
    const [activeTab, setActiveTab] = useState(1);
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
                        <Tab label="Справочник"/>
                        <Tab label="Активные практики"/>
                        <Tab label="Завершенные практики"/>
                    </Tabs>
                </Paper>

                <Grid container spacing={3}>
                    {activeTab === 1 ? (
                        activePractices.length > 0 ? (
                            activePractices.map((practice, index) => (
                                <Grid item xs={12} key={index}>
                                    <PracticeCard
                                        practice={practice}
                                        onClick={() => navigate(`/practice/${practice.id}`)}
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
                    ) : activeTab === 2 ? (
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
                    ) : (
                        <Grid item xs={12}>
                            <HandbookTab/>
                        </Grid>
                    )}
                </Grid>
            </Container>
        </Layout>
    );
}
