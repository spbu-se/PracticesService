import { Layout } from "@shared/ui/layout/Layout.tsx";
import { getJWTToken } from "@/shared/services/localStorage.service.ts";
import { Navigate, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { getSupervisorPractices, getMe, getAllGroups } from "@/shared/services/axios.service.ts";
import { Practice } from "@/entities/Practice.ts";
import { User } from "@/entities/User.ts";
import { Group } from "@/entities/Group.ts";
import {
    Container,
    Grid,
    Typography,
    Paper,
    Box,
    useMediaQuery,
    useTheme,
    TextField,
    InputAdornment,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    Button,
    Tabs,
    Tab,
} from "@mui/material";
import SearchIcon from "@mui/icons-material/Search";
import { PracticeCard } from "./PracticeCard";
import { UserRole } from "../../entities/UserRoles";

export function SupervisorPracticesPage() {
    const tokenIsEmpty = getJWTToken() === "";
    const [practices, setPractices] = useState<Practice[]>([]);
    const [groups, setGroups] = useState<Group[]>([]);
    const [me, setMe] = useState<User>();
    const [searchTerm, setSearchTerm] = useState("");
    const [selectedGroup, setSelectedGroup] = useState<string>("");
    const [selectedType, setSelectedType] = useState<string>("");
    const [selectedProgram, setSelectedProgram] = useState<string>("");
    const [activeTab, setActiveTab] = useState(0); // 0 = Active, 1 = Completed
    const navigate = useNavigate();

    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("sm"));

    const practiceTypes = [
        "Практика осенняя, 2 курс",
        "Практика весенняя, 2 курс",
        "Практика осенняя, 3 курс",
        "Практика весенняя, 3 курс",
        "Производственная практика",
        "Преддипломная практика",
        "Бакалаврская ВКР",
        "Магистерская ВКР"
    ];

    useEffect(() => {
        getMe().then((response) => {
            const data: User = response.data;
            if (!data.roles.includes(UserRole.SUPERVISOR)) {
                navigate("/");
            }

            setMe(data);
            loadGroups();
            getSupervisorPractices(data.userId).then((response) => {
                setPractices(response.data);
            });
        });
    }, []);

    const loadGroups = () => {
        getAllGroups()
            .then((res) => setGroups(res.data))
            .catch(() => setGroups([]));
    };

    const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
        setActiveTab(newValue);
    };

    const uniquePrograms = Array.from(new Set(groups.map(group => group.program)));

    const activePractices = practices.filter((p) => p.status !== "Завершено");
    const completedPractices = practices.filter((p) => p.status === "Завершено");

    const filterPractices = (practicesList: Practice[]) => {
        return practicesList.filter((p) => {
            const matchesSearch =
                p.theme.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
                (p.student?.lastName.toLowerCase().includes(searchTerm.toLowerCase()) ?? false) ||
                (p.student?.firstName.toLowerCase().includes(searchTerm.toLowerCase()) ?? false) ||
                (p.student?.middleName?.toLowerCase().includes(searchTerm.toLowerCase()) ?? false);

            const matchesGroup = selectedGroup ? p.student.group?.id.toString() === selectedGroup : true;
            const matchesType = selectedType ? p.type === selectedType : true;
            const matchesProgram = selectedProgram ? p.student.group?.program === selectedProgram : true;

            return matchesSearch && matchesGroup && matchesType && matchesProgram;
        });
    };

    const filteredActivePractices = filterPractices(activePractices);
    const filteredCompletedPractices = filterPractices(completedPractices);

    const renderPractices = (practicesList: Practice[], emptyMessage: string) => {
        if (practicesList.length === 0) {
            return (
                <Paper
                    elevation={0}
                    sx={{
                        p: 4,
                        textAlign: "center",
                        backgroundColor: 'background.default',
                    }}
                >
                    <Typography variant="h6" color="text.secondary">
                        {emptyMessage}
                    </Typography>
                </Paper>
            );
        }

        return (
            <Grid container spacing={isMobile ? 1 : 3}>
                {practicesList.map((practice) => (
                    <Grid item xs={12} sm={6} md={4} lg={3} key={practice.id}>
                        <PracticeCard
                            practice={practice}
                            onClick={() => navigate(`/practice-staff/${practice.id}`)}
                        />
                    </Grid>
                ))}
            </Grid>
        );
    };

    return tokenIsEmpty ? (
        <Navigate to="/login" replace />
    ) : (
        <Layout>
            <Container maxWidth="xl" sx={{ mt: 2, p: isMobile ? 1 : 4 }}>
                <Paper
                    elevation={0}
                    sx={{
                        p: isMobile ? 2 : 4,
                        borderRadius: 2,
                        backgroundColor: 'background.default',
                    }}
                >
                    <Typography
                        variant={isMobile ? "h5" : "h4"}
                        align="center"
                        gutterBottom
                        fontWeight="bold"
                    >
                        Практики под вашим руководством
                    </Typography>

                    <Paper sx={{ mb: 2 }}>
                        <Tabs
                            value={activeTab}
                            onChange={handleTabChange}
                            variant={isMobile ? "scrollable" : "standard"}
                            scrollButtons={isMobile ? "auto" : undefined}
                            centered={!isMobile}
                        >
                            <Tab label="Активные практики" />
                            <Tab label="Завершенные практики" />
                        </Tabs>
                    </Paper>

                    <Paper
                        elevation={2}
                        sx={{
                            p: isMobile ? 2 : 3,
                            mb: 3,
                            borderRadius: 2,
                            backgroundColor: 'background.paper',
                        }}
                    >
                        <Box
                            sx={{
                                display: "flex",
                                flexDirection: isMobile ? "column" : "row",
                                gap: 2,
                                alignItems: "center",
                                flexWrap: "wrap",
                            }}
                        >
                            <TextField
                                variant="outlined"
                                placeholder="Поиск по теме или студенту"
                                size={isMobile ? "small" : "medium"}
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                                InputProps={{
                                    startAdornment: (
                                        <InputAdornment position="start">
                                            <SearchIcon />
                                        </InputAdornment>
                                    ),
                                }}
                                sx={{ flex: 1, minWidth: isMobile ? "100%" : 200 }}
                            />

                            <FormControl size={isMobile ? "small" : "medium"} sx={{ minWidth: 150, flex: 1 }}>
                                <InputLabel>Группа</InputLabel>
                                <Select
                                    value={selectedGroup}
                                    label="Группа"
                                    onChange={(e) => setSelectedGroup(e.target.value)}
                                >
                                    <MenuItem value="">
                                        <em>Все</em>
                                    </MenuItem>
                                    {groups.map((group) => (
                                        <MenuItem key={group.id} value={group.id.toString()}>
                                            {`${group.name}`}
                                        </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>

                            <FormControl size={isMobile ? "small" : "medium"} sx={{ minWidth: 150, flex: 1 }}>
                                <InputLabel>Тип практики</InputLabel>
                                <Select
                                    value={selectedType}
                                    label="Тип практики"
                                    onChange={(e) => setSelectedType(e.target.value)}
                                >
                                    <MenuItem value="">
                                        <em>Все</em>
                                    </MenuItem>
                                    {practiceTypes.map((type) => (
                                        <MenuItem key={type} value={type}>
                                            {type}
                                        </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>

                            <FormControl size={isMobile ? "small" : "medium"} sx={{ minWidth: 150, flex: 1 }}>
                                <InputLabel>Программа</InputLabel>
                                <Select
                                    value={selectedProgram}
                                    label="Программа"
                                    onChange={(e) => setSelectedProgram(e.target.value)}
                                >
                                    <MenuItem value="">
                                        <em>Все</em>
                                    </MenuItem>
                                    {uniquePrograms.map((program) => (
                                        <MenuItem key={program} value={program}>
                                            {program}
                                        </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>

                            <Button
                                variant="outlined"
                                onClick={() => {
                                    setSearchTerm("");
                                    setSelectedGroup("");
                                    setSelectedType("");
                                    setSelectedProgram("");
                                }}
                                sx={{ height: isMobile ? 40 : 56, minWidth: isMobile ? "100%" : 100 }}
                            >
                                Сбросить
                            </Button>
                        </Box>
                    </Paper>

                    {activeTab === 0
                        ? renderPractices(filteredActivePractices, "Нет активных практик под вашим руководством")
                        : renderPractices(filteredCompletedPractices, "Нет завершенных практик под вашим руководством")
                    }
                </Paper>
            </Container>
        </Layout>
    );
}