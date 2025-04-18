import {Layout} from "@shared/ui/layout/Layout.tsx";
import {getJWTToken} from "../shared/services/localStorage.service.ts";
import {Navigate, useNavigate} from "react-router-dom";
import {useEffect, useState} from "react";
import {getMe, getThemes, putTheme} from "../shared/services/axios.service.ts";
import {Theme} from "../entities/Theme.ts";
import {
    Container,
    Grid,
    Card,
    CardContent,
    CardHeader,
    Typography,
    MenuItem,
    Button,
    Pagination, TextField, Paper, Box, Checkbox, FormControlLabel
} from "@mui/material";
import ArchiveIcon from '@mui/icons-material/Archive';
import EditIcon from '@mui/icons-material/Edit';

export function BasePage() {
    const tokenIsEmpty = getJWTToken() === "";
    const [themes, setThemes] = useState<Theme[]>([]);
    const [filteredThemes, setFilteredThemes] = useState<Theme[]>([]);
    const [currentPage, setCurrentPage] = useState(1);
    const itemsPerPage = 5; // Themes per page
    const navigate = useNavigate();
    const levels = ["2 курс", "3 курс", "Бакалаврская ВКР", "Магистерская ВКР"]
    const [me, setMe] = useState<string>("");

    // Filter state
    const [level, setLevel] = useState<string>("");
    const [department, setDepartment] = useState<string>("");
    const [source, setSource] = useState<string>("");
    const [supervisor, setSupervisor] = useState<string>("");
    const [isArchived, setIsArchived] = useState(false);

    useEffect(() => {
        getThemes().then(response => {
            setThemes(response.data);
            setFilteredThemes(response.data);
        });

        getMe().then(response => {
            setMe(response.data);
        })
    }, []);

    // Apply filters whenever a filter changes
    useEffect(() => {
        let filtered = themes;

        if (level) filtered = filtered.filter(theme => theme.level.includes(level));
        if (department) filtered = filtered.filter(theme => theme.department === department);
        if (source) filtered = filtered.filter(theme => theme.source === source);
        if (supervisor) filtered = filtered.filter(theme => theme.supervisorid.toString() === supervisor);
        filtered = filtered.filter(theme => theme.isarchived == isArchived);

        setFilteredThemes(filtered);
        setCurrentPage(1);
    }, [level, department, source, supervisor, themes, isArchived]);

    // Pagination calculations
    const indexOfLastTheme = currentPage * itemsPerPage;
    const indexOfFirstTheme = indexOfLastTheme - itemsPerPage;
    const currentThemes = filteredThemes.slice(indexOfFirstTheme, indexOfLastTheme);

    const rearchiveTheme = async (id: number) => {
        const theme = themes.filter(t => t.id == id)[0];
        theme.isarchived = !isArchived;
        await putTheme(theme);
        setFilteredThemes(filteredThemes.filter(t => t.id != id))
    }

    const handlePageChange = (_event: React.ChangeEvent<unknown>, value: number) => setCurrentPage(value);

    return tokenIsEmpty ? <Navigate to="/login" replace/> : (
        <Layout>
            <Container maxWidth="lg" sx={{mt: 4, p: 4}}>
                <Typography variant="h4" align="center" gutterBottom>
                    Список тем
                </Typography>

                <Grid container spacing={3}>
                    <Grid maxWidth="350px" item xs={12} md={3}>
                        <Box
                            sx={{
                                display: 'flex',
                                justifyContent: 'center',
                                mb: 2
                            }}
                        >
                            <Button
                                variant="contained"
                                onClick={() => navigate("/createTheme")}
                                sx={{
                                    width: '100%',
                                    maxWidth: 300
                                }}
                            >
                                Предложить тему
                            </Button>
                        </Box>

                        <Paper elevation={2} sx={{p: 3, borderRadius: 2}}>
                            <Typography variant="h6" gutterBottom>Фильтры</Typography>

                            <TextField
                                select
                                fullWidth
                                label="Уровень"
                                value={level}
                                onChange={(e) => setLevel(e.target.value)}
                                variant="outlined"
                                margin="dense"
                            >
                                <MenuItem value="">Все</MenuItem>
                                {levels.map((lvl, i) => (
                                    <MenuItem key={i} value={lvl}>{lvl}</MenuItem>
                                ))}
                            </TextField>

                            <TextField
                                select
                                fullWidth
                                label="Кафедра"
                                value={department}
                                onChange={(e) => setDepartment(e.target.value)}
                                variant="outlined"
                                margin="dense"
                            >
                                <MenuItem value="">Все</MenuItem>
                                {Array.from(new Set(themes.map((t) => t.department))).map((dep, i) => (
                                    <MenuItem key={i} value={dep}>{dep}</MenuItem>
                                ))}
                            </TextField>

                            <TextField
                                select
                                fullWidth
                                label="Источник"
                                value={source}
                                onChange={(e) => setSource(e.target.value)}
                                variant="outlined"
                                margin="dense"
                            >
                                <MenuItem value="">Все</MenuItem>
                                {Array.from(new Set(themes.map((t) => t.source))).map((src, i) => (
                                    <MenuItem key={i} value={src}>{src}</MenuItem>
                                ))}
                            </TextField>

                            <TextField
                                select
                                fullWidth
                                label="Руководитель"
                                value={supervisor}
                                onChange={(e) => setSupervisor(e.target.value)}
                                variant="outlined"
                                margin="dense"
                            >
                                <MenuItem value="">Все</MenuItem>
                                {Array.from(new Set(themes.map((t) => t.supervisorid?.toString()))).map((sup, i) => (
                                    <MenuItem key={i} value={sup}>{sup}</MenuItem>
                                ))}
                            </TextField>

                            <FormControlLabel
                                control={
                                    <Checkbox
                                        checked={isArchived}
                                        onChange={(e) => setIsArchived(e.target.checked)}
                                        color="primary"
                                    />
                                }
                                label="Показать архивные"
                                sx={{mt: 1, mb: 1}}
                            />

                            <Button
                                variant="contained"
                                color="secondary"
                                fullWidth
                                sx={{mt: 2}}
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
                    </Grid>

                    <Grid item xs={12} md={9} container direction="column" spacing={2} sx={{
                        justifyContent: "flex-start"
                    }}>
                        <>
                            {currentThemes.length > 0 ? (
                                currentThemes.map((theme, index) => (
                                    <Grid key={index} container sx={{
                                        alignItems: "center"
                                    }}>
                                        <Grid item xs={11}>
                                            <Card
                                                sx={{
                                                    cursor: "pointer",
                                                    boxShadow: 3,
                                                    width: "30vw",
                                                    minWidth: "250px",
                                                    display: 'flex',
                                                    flexDirection: 'column',
                                                    justifyContent: 'space-between'
                                                }}
                                                onClick={() => navigate(`/theme/${theme.id}`)}
                                            >
                                                <CardHeader title={theme.title}/>
                                                <CardContent>
                                                    <Typography variant="subtitle1">Уровень: {theme.level}</Typography>
                                                    <Typography variant="body2">Кафедра: {theme.department}</Typography>
                                                    <Typography variant="body2">Источник: {theme.source}</Typography>
                                                    <Typography variant="body2">Научный
                                                        руководитель: {theme.supervisor ? `${theme.supervisor.lastName} ${theme.supervisor.firstName} ${theme.supervisor.middleName}` : "Не назначен"}</Typography>
                                                    <Typography
                                                        variant="body2">Консультант: {theme.consultant?.name ?? "Не назначен"}</Typography>
                                                </CardContent>
                                            </Card>
                                        </Grid>
                                        <Grid item key={index} xs={1}>
                                            {me == theme.suggestedby ?
                                                <Grid spacing={2} container item direction="column">
                                                    <Button
                                                        variant="contained"
                                                        onClick={() => navigate(`/editTheme/${theme.id}`)}
                                                    >
                                                        <EditIcon/>
                                                    </Button>
                                                    <Button
                                                        variant="contained"
                                                        onClick={() => {
                                                            rearchiveTheme(theme.id);
                                                        }}
                                                    >
                                                        <ArchiveIcon/>
                                                    </Button>
                                                </Grid> : <></>}
                                        </Grid>
                                    </Grid>
                                ))
                            ) : (
                                <Typography variant="h6" align="center" sx={{width: "100%"}}>
                                    Нет доступных тем
                                </Typography>
                            )}
                        </>


                        <>
                            {filteredThemes.length > itemsPerPage && (
                                <Grid item sx={{display: "flex", justifyContent: "center"}}>
                                    <Pagination
                                        count={Math.ceil(filteredThemes.length / itemsPerPage)}
                                        page={currentPage}
                                        onChange={handlePageChange}
                                        sx={{mt: 2}}
                                    />
                                </Grid>
                            )}
                        </>
                    </Grid>
                </Grid>
            </Container>
        </Layout>
    );
}
