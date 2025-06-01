import { Layout } from "@shared/ui/layout/Layout.tsx";
import { getJWTToken } from "@/shared/services/localStorage.service.ts";
import { Navigate, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { getMePublic, getThemesPublic, putTheme } from "@/shared/services/axios.service.ts";
import { Theme } from "@/entities/Theme.ts";
import {
    Container,
    Card,
    CardContent,
    CardHeader,
    Typography,
    MenuItem,
    Button,
    Pagination,
    TextField,
    Paper,
    Box,
    Checkbox,
    FormControlLabel,
    Stack
} from "@mui/material";
import ArchiveIcon from "@mui/icons-material/Archive";
import EditIcon from "@mui/icons-material/Edit";
import { User } from "@/entities/User.ts";
import { UserRole } from "../../entities/UserRoles";

export function ThemesIndexPage() {
    const tokenIsEmpty = getJWTToken() === "";
    const [themes, setThemes] = useState<Theme[]>([]);
    const [filteredThemes, setFilteredThemes] = useState<Theme[]>([]);
    const [currentPage, setCurrentPage] = useState(1);
    const itemsPerPage = 5;
    const navigate = useNavigate();
    const levels = ["2 курс", "3 курс", "Бакалаврская ВКР", "Магистерская ВКР"];
    const [me, setMe] = useState<User>();

    const [level, setLevel] = useState<string>("");
    const [department, setDepartment] = useState<string>("");
    const [source, setSource] = useState<string>("");
    const [supervisor, setSupervisor] = useState<string>("");
    const [isArchived, setIsArchived] = useState(false);
    const isPracticeSupervisor = me?.roles?.includes(UserRole.PRACTICE_SUPERVISOR);

    const handleArchiveAll = async () => {
        if (window.confirm("Вы уверены, что хотите архивировать все отфильтрованные темы?")) {
            try {
                const archivePromises = filteredThemes.map(theme =>
                    putTheme({ ...theme, isarchived: !isArchived })
                );

                await Promise.all(archivePromises);

                setThemes(themes.map(theme => {
                    const isFiltered = filteredThemes.some(t => t.id === theme.id);
                    return isFiltered ? { ...theme, isarchived: !isArchived } : theme;
                }));

                alert(`Темы успешно ${isArchived ? 'восстановлены из архива' : 'архивированы'}`);
            } catch (error) {
                console.error("Ошибка при архивировании:", error);
                alert("Произошла ошибка при архивировании");
            }
        }
    };

    useEffect(() => {
        getThemesPublic().then(response => {
            const data = Array.isArray(response.data) ? response.data : [];
            setThemes(data);
            setFilteredThemes(data);
        });

        getMePublic().then(response => {
            const data: User = response.data;
            setMe(data);
        });
    }, []);

    useEffect(() => {
        let filtered = Array.isArray(themes) ? [...themes] : [];

        if (level) filtered = filtered.filter(theme => theme.level?.includes(level));
        if (department) filtered = filtered.filter(theme => theme.department === department);
        if (source) filtered = filtered.filter(theme => theme.source === source);
        if (supervisor) filtered = filtered.filter(theme => theme.supervisorid?.toString() === supervisor);
        filtered = filtered.filter(theme => theme.isarchived === isArchived);

        setFilteredThemes(filtered);
        setCurrentPage(1);
    }, [level, department, source, supervisor, themes, isArchived]);

    const indexOfLastTheme = currentPage * itemsPerPage;
    const indexOfFirstTheme = indexOfLastTheme - itemsPerPage;
    const currentThemes = filteredThemes.slice(indexOfFirstTheme, indexOfLastTheme);

    const rearchiveTheme = async (id: number) => {
        const theme = themes.find(t => t.id === id);
        if (!theme) return;
        theme.isarchived = !isArchived;
        await putTheme(theme);
        setFilteredThemes(filteredThemes.filter(t => t.id !== id));
    }

    const handlePageChange = (_event: React.ChangeEvent<unknown>, value: number) => setCurrentPage(value);

    return <Layout>
        <Container maxWidth="lg" sx={{ mt: 4, p: 4 }}>
            <Typography variant="h4" align="center" gutterBottom>Список тем</Typography>

            <Stack direction={{ xs: 'column', md: 'row' }} spacing={4}>
                <Box sx={{ width: { xs: '100%', md: '300px' } }}>
                    {isPracticeSupervisor && (
                        <Button
                            variant="contained"
                            color="secondary"
                            onClick={handleArchiveAll}
                            disabled={filteredThemes.length === 0}
                            fullWidth
                            sx={{ mb: 2 }}
                        >
                            Архивировать все
                        </Button>
                    )}
                    <Button
                        variant="contained"
                        fullWidth
                        sx={{ mb: 2 }}
                        onClick={() => navigate("/create/theme")}
                    >
                        Предложить тему
                    </Button>

                    <Paper elevation={2} sx={{ p: 3, borderRadius: 2 }}>
                        <Typography variant="h6" gutterBottom>Фильтры</Typography>

                        <TextField select fullWidth label="Уровень" value={level} onChange={(e) => setLevel(e.target.value)} margin="dense">
                            <MenuItem value="">Все</MenuItem>
                            {levels.map((lvl, i) => <MenuItem key={i} value={lvl}>{lvl}</MenuItem>)}
                        </TextField>

                        <TextField select fullWidth label="Кафедра" value={department} onChange={(e) => setDepartment(e.target.value)} margin="dense">
                            <MenuItem value="">Все</MenuItem>
                            {Array.from(new Set((Array.isArray(themes) ? themes : []).map(t => t.department))).map((dep, i) => (
                                <MenuItem key={i} value={dep}>{dep}</MenuItem>
                            ))}
                        </TextField>

                        <TextField select fullWidth label="Источник" value={source} onChange={(e) => setSource(e.target.value)} margin="dense">
                            <MenuItem value="">Все</MenuItem>
                            {Array.from(new Set((Array.isArray(themes) ? themes : []).filter(t => t.source).map(t => t.source))).map((src, i) => (
                                <MenuItem key={i} value={src}>{src}</MenuItem>
                            ))}
                        </TextField>

                        <TextField select fullWidth label={UserRole.SUPERVISOR} value={supervisor} onChange={(e) => setSupervisor(e.target.value)} margin="dense">
                            <MenuItem value="">Все</MenuItem>
                            {Array.from(new Set((Array.isArray(themes) ? themes : [])
                                .filter(t => t.supervisor)
                                .map(t => `${t.supervisor?.lastName} ${t.supervisor?.firstName} ${t.supervisor?.middleName}`)))
                                .map((sup, i) => (
                                    <MenuItem key={i} value={sup}>{sup}</MenuItem>
                                ))}
                        </TextField>

                        <FormControlLabel
                            control={<Checkbox checked={isArchived} onChange={(e) => setIsArchived(e.target.checked)} />}
                            label="Показать только архивные"
                            sx={{ mt: 1, mb: 1 }}
                        />

                        <Button
                            variant="contained"
                            color="secondary"
                            fullWidth
                            sx={{ mt: 2 }}
                            onClick={() => {
                                setLevel("");
                                setDepartment("");
                                setSource("");
                                setSupervisor("");
                                setIsArchived(false);
                            }}
                        >
                            Сбросить фильтры
                        </Button>
                    </Paper>
                </Box>

                <Stack spacing={2} flex={1}>
                    {Array.isArray(currentThemes) && currentThemes.length > 0 ? (
                        currentThemes.map((theme) => (
                            <Box key={theme.id} display="flex" alignItems="flex-start" gap={2}>
                                <Card
                                    sx={{ cursor: "pointer", boxShadow: 3, flexGrow: 1 }}
                                    onClick={() => navigate(`/theme/${theme.id}`)}
                                >
                                    <CardHeader title={theme.title} />
                                    <CardContent>
                                        <Typography variant="subtitle1">Уровень: {theme.level}</Typography>
                                        <Typography variant="body2">Кафедра: {theme.department}</Typography>
                                        <Typography variant="body2">Источник: {theme.source}</Typography>
                                        <Typography variant="body2">
                                            Научный руководитель: {theme.supervisor
                                            ? `${theme.supervisor.lastName} ${theme.supervisor.firstName} ${theme.supervisor.middleName}`
                                            : "Не назначен"}
                                        </Typography>
                                        <Typography variant="body2">
                                            Консультант: {theme.consultant
                                            ? `${theme.consultant.lastName} ${theme.consultant.firstName} ${theme.consultant.middleName}`
                                            : "Не назначен"}
                                        </Typography>
                                    </CardContent>
                                </Card>

                                {me?.userId === theme.suggestedby && (
                                    <Stack spacing={1} mt={1}>
                                        <Button variant="contained" onClick={() => navigate(`/edit/theme/${theme.id}`)}>
                                            <EditIcon />
                                        </Button>
                                        <Button variant="contained" onClick={() => rearchiveTheme(theme.id)}>
                                            <ArchiveIcon />
                                        </Button>
                                    </Stack>
                                )}
                            </Box>
                        ))
                    ) : (
                        <Typography variant="h6" align="center">Нет доступных тем</Typography>
                    )}

                    {filteredThemes.length > itemsPerPage && (
                        <Box display="flex" justifyContent="center" mt={2}>
                            <Pagination
                                count={Math.ceil(filteredThemes.length / itemsPerPage)}
                                page={currentPage}
                                onChange={handlePageChange}
                            />
                        </Box>
                    )}
                </Stack>
            </Stack>
        </Container>
    </Layout>
}
