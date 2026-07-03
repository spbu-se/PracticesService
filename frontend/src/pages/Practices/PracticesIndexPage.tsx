import { Layout } from "@shared/ui/layout/Layout.tsx";
import { getJWTToken } from "@/shared/services/localStorage.service.ts";
import { Navigate, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { getUserPractices, getMe } from "@/shared/services/axios.service.ts";
import { Practice } from "@/entities/Practice.ts";
import { User } from "@/entities/User.ts";
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
    useMediaQuery,
    useTheme,
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import { HandbookTab } from "./HandbookTab";
import { PracticeCard } from "./PracticeCard";

export function PracticesIndexPage() {
    const tokenIsEmpty = getJWTToken() === "";
    const [practices, setPractices] = useState<Practice[]>([]);
    const [me, setMe] = useState<User>();
    const [activeTab, setActiveTab] = useState(1);
    const navigate = useNavigate();

    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("sm")); // Detect mobile

    useEffect(() => {
        getMe().then((response) => {
            const data: User = response.data;
            setMe(data);
            getUserPractices(data.userId).then((response) => {
                setPractices(response.data);
            });
        });
    }, []);

    const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
        setActiveTab(newValue);
    };

    const activePractices = practices.filter((p) => p.status !== "Завершено");
    const completedPractices = practices.filter((p) => p.status === "Завершено");

    return tokenIsEmpty ? (
        <Navigate to="/login" replace />
    ) : (
        <Layout>
            <Container maxWidth="md" sx={{ mt: 2, p: isMobile ? 1 : 4 }}>
                <Typography variant={isMobile ? "h5" : "h4"} align="center" gutterBottom>
                    Мои практики
                </Typography>

                <Box
                    sx={{
                        display: "flex",
                        justifyContent: "flex-end",
                        mb: 2,
                    }}
                >
                    <Button
                        variant="contained"
                        startIcon={<AddIcon />}
                        onClick={() => navigate("/create/practice")}
                        size={isMobile ? "small" : "medium"}
                    >
                        Создать практику
                    </Button>
                </Box>

                <Paper sx={{ mb: 2 }}>
                    <Tabs
                        value={activeTab}
                        onChange={handleTabChange}
                        variant={isMobile ? "scrollable" : "standard"}
                        scrollButtons={isMobile ? "auto" : undefined}
                        centered={!isMobile}
                    >
                        <Tab label="Справочник" />
                        <Tab label="Активные практики" />
                        <Tab label="Завершенные практики" />
                    </Tabs>
                </Paper>

                <Grid container spacing={isMobile ? 1 : 3}>
                    {activeTab === 1 ? (
                        activePractices.length > 0 ? (
                            activePractices.map((practice) => (
                                <Grid item xs={12} sm={6} md={4} key={practice.id}>
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
                            completedPractices.map((practice) => (
                                <Grid item xs={12} sm={6} md={4} key={practice.id}>
                                    <PracticeCard
                                        practice={practice}
                                        onClick={() => navigate(`/practice/${practice.id}`)}
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
                            <HandbookTab />
                        </Grid>
                    )}
                </Grid>
            </Container>
        </Layout>
    );
}
