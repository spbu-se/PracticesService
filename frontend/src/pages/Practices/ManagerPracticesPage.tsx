import { Layout } from "@shared/ui/layout/Layout.tsx";
import { getJWTToken } from "@/shared/services/localStorage.service.ts";
import { Navigate, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { getPractices, getAllGroups, getMe } from "@/shared/services/axios.service.ts";
import { Practice } from "@/entities/Practice.ts";
import { Group } from "@/entities/Group.ts";
import { User } from "@/entities/User.ts";
import {
    Container,
    Grid,
    Typography,
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
} from "@mui/material";
import SearchIcon from "@mui/icons-material/Search";
import { PracticeCard } from "./PracticeCard";
import { UserRole } from "../../entities/UserRoles";

export function ManagerPracticesPage() {
    const tokenIsEmpty = getJWTToken() === "";
    const [practices, setPractices] = useState<Practice[]>([]);
    const [groups, setGroups] = useState<Group[]>([]);
    const [me, setMe] = useState<User>();
    const [searchTerm, setSearchTerm] = useState("");
    const [selectedGroup, setSelectedGroup] = useState<string>("");
    const [selectedType, setSelectedType] = useState<string>("");
    const [selectedProgram, setSelectedProgram] = useState<string>("");

    const navigate = useNavigate();

    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("sm"));

    const practiceTypes = [
        "Практика осенняя, 2 курс", "Практика весенняя, 2 курс", "Практика осенняя, 3 курс",
        "Практика весенняя, 3 курс", "Производственная практика", "Преддипломная практика",
        "Бакалаврская ВКР", "Магистерская ВКР"
    ];

    useEffect(() => {
        getMe().then(({ data }) => {
            if (!data.roles.includes(UserRole.PRACTICE_SUPERVISOR)) {
                navigate("/");
            } else {
                setMe(data);
                loadGroups();
                loadPractices();
            }
        });
    }, []);

    const loadGroups = () => {
        getAllGroups()
            .then((res) => setGroups(res.data))
            .catch(() => setGroups([]));
    };

    const loadPractices = () => {
        getPractices()
            .then((res) => setPractices(res.data))
            .catch(() => setPractices([]));
    };

    // Соберём уникальные программы из групп для фильтра
    const uniquePrograms = Array.from(new Set(groups.map(group => group.program)));

    // Фильтрация на клиенте
    const filteredPractices = practices.filter((p) => {
        const matchesSearch =
            p.theme.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
            (p.student?.lastName.toLowerCase().includes(searchTerm.toLowerCase()) ?? false);

        const matchesGroup = selectedGroup ? p.student.group?.id.toString() === selectedGroup : true;

        const matchesType = selectedType ? p.type === selectedType : true;

        const matchesProgram = selectedProgram ? p.student.group?.program === selectedProgram : true;

        return matchesSearch && matchesGroup && matchesType && matchesProgram;
    });

    return tokenIsEmpty ? (
        <Navigate to="/login" replace />
    ) : (
        <Layout>
            <Container maxWidth="md" sx={{ mt: 2, p: isMobile ? 1 : 4 }}>
                <Typography variant={isMobile ? "h5" : "h4"} align="center" gutterBottom>
                    Работы
                </Typography>

                <Box
                    sx={{
                        display: "flex",
                        flexDirection: isMobile ? "column" : "row",
                        gap: 2,
                        justifyContent: "center",
                        mb: 2,
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
                        sx={{ width: isMobile ? "100%" : 300 }}
                    />

                    <FormControl sx={{ minWidth: 150 }}>
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
                                    {`${group.name} (${group.program}, ${group.year})`}
                                </MenuItem>
                            ))}
                        </Select>
                    </FormControl>

                    <FormControl sx={{ minWidth: 150 }}>
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

                    <FormControl sx={{ minWidth: 150 }}>
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
                    >
                        Сбросить
                    </Button>
                </Box>

                {filteredPractices.length > 0 ? (
                    <Grid container spacing={isMobile ? 1 : 3}>
                        {filteredPractices.map((practice) => (
                            <Grid item xs={12} sm={6} md={4} key={practice.id}>
                                <PracticeCard
                                    practice={practice}
                                    onClick={() => navigate(`/practice-staff/${practice.id}`)}
                                />
                            </Grid>
                        ))}
                    </Grid>
                ) : (
                    <Typography variant="h6" align="center" sx={{ mt: 4 }}>
                        Нет практик, соответствующих запросу.
                    </Typography>
                )}
            </Container>
        </Layout>
    );
}
