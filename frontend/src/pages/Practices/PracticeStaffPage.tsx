import { useState } from "react";
import {
    Container,
    Typography,
    Paper,
    Tabs,
    Tab,
    Box,
    Button,
    Divider
} from "@mui/material";
import { useNavigate, useParams } from "react-router-dom";
import { Layout } from "@shared/ui/layout/Layout.tsx";
import { StaffThemeSelection } from "./components/StaffThemeSelection";
import { StaffGoalsAndTasks } from "./components/StaffGoalsAndTasks";
import { StaffReporting } from "./components/StaffReporting";
import { Attachments } from "./components/Attachments";
import { Messages } from "./components/Messages";
import { TextWorkHistory } from "./components/TextWorkHistory";
import { PresentationHistory } from "./components/PresentationHistory";

interface TabPanelProps {
    children?: React.ReactNode;
    index: number;
    value: number;
}

function TabPanel(props: TabPanelProps) {
    const { children, value, index, ...other } = props;

    return (
        <div
            role="tabpanel"
            hidden={value !== index}
            id={`practice-tabpanel-${index}`}
            aria-labelledby={`practice-tab-${index}`}
            {...other}
        >
            {value === index && (
                <Box sx={{ p: 3 }}>
                    {children}
                </Box>
            )}
        </div>
    );
}

function a11yProps(index: number) {
    return {
        id: `practice-tab-${index}`,
        'aria-controls': `practice-tabpanel-${index}`,
    };
}

export function PracticeStaffPage() {
    const { id } = useParams();
    const navigate = useNavigate();
    const [value, setValue] = useState(0);

    const handleChange = (event: React.SyntheticEvent, newValue: number) => {
        setValue(newValue);
    };

    const isNewPractice = !id;

    return (
        <Layout>
            <Container maxWidth="xl" sx={{ mt: 4, mb: 4 }}>
                <Button
                    variant="outlined"
                    onClick={() => navigate(-1)}
                    sx={{ mb: 2 }}
                >
                    ← Назад
                </Button>

                <Paper elevation={3}>
                    <Typography variant="h4" sx={{ p: 3 }}>
                        {isNewPractice ? "Создание практики" : "Просмотр работы"}
                    </Typography>

                    <Divider />

                    <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
                        <Tabs
                            value={value}
                            onChange={handleChange}
                            variant="scrollable"
                            scrollButtons="auto"
                        >
                            <Tab label="Выбор темы" {...a11yProps(0)} />
                            <Tab label="Цели и задачи" {...a11yProps(1)} />
                            <Tab label="Отчётность" {...a11yProps(2)} />
                            <Tab label="Подготовка к защите" {...a11yProps(3)} />
                            <Tab label="История версий текстов" {...a11yProps(4)} />
                            <Tab label="История версий презентаций" {...a11yProps(5)} />
                            <Tab label="Чат" {...a11yProps(6)} />
                        </Tabs>
                    </Box>

                    <TabPanel value={value} index={0}>
                        <StaffThemeSelection practiceId={id} />
                    </TabPanel>

                    <TabPanel value={value} index={1}>
                        <StaffGoalsAndTasks practiceId={id} />
                    </TabPanel>

                    <TabPanel value={value} index={2}>
                        <StaffReporting practiceId={id} />
                    </TabPanel>

                    <TabPanel value={value} index={3}>
                        <Attachments practiceId={id} />
                    </TabPanel>

                    <TabPanel value={value} index={4}>
                        <TextWorkHistory practiceId={id} />
                    </TabPanel>
                    
                    <TabPanel value={value} index={5}>
                        <PresentationHistory practiceId={id} />
                    </TabPanel>

                    <TabPanel value={value} index={6}>
                        <Messages practiceId={id} />
                    </TabPanel>
                </Paper>
            </Container>
        </Layout>
    );
}