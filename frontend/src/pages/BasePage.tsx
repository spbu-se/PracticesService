import { Layout } from "@shared/ui/layout/Layout.tsx";
import { getJWTToken } from "../shared/services/localStorage.service.ts";
import { Navigate, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { getThemes } from "../shared/services/axios.service.ts";
import { Theme } from "../entities/Theme.ts";
import {
    Container,
    Grid,
    Card,
    CardContent,
    CardHeader,
    Typography,
    MenuItem,
    Button,
    Pagination, TextField, Paper
} from "@mui/material";

export function BasePage() {
    const tokenIsEmpty = getJWTToken() === "";
    const [themes, setThemes] = useState<Theme[]>([]);
    const [filteredThemes, setFilteredThemes] = useState<Theme[]>([]);
    const [currentPage, setCurrentPage] = useState(1);
    const itemsPerPage = 5; // Themes per page
    const navigate = useNavigate();

    // Filter state
    const [level, setLevel] = useState<string>("");
    const [department, setDepartment] = useState<string>("");
    const [source, setSource] = useState<string>("");
    const [supervisor, setSupervisor] = useState<string>("");

    useEffect(() => {
        getThemes().then(response => {
            setThemes(response.data);
            setFilteredThemes(response.data);
        });
    }, []);

    // Apply filters whenever a filter changes
    useEffect(() => {
        let filtered = themes;

        if (level) filtered = filtered.filter(theme => theme.level === level);
        if (department) filtered = filtered.filter(theme => theme.department === department);
        if (source) filtered = filtered.filter(theme => theme.suggestedby === source);
        if (supervisor) filtered = filtered.filter(theme => theme.supervisorid.toString() === supervisor);

        setFilteredThemes(filtered);
        setCurrentPage(1);
    }, [level, department, source, supervisor, themes]);

    // Pagination calculations
    const indexOfLastTheme = currentPage * itemsPerPage;
    const indexOfFirstTheme = indexOfLastTheme - itemsPerPage;
    const currentThemes = filteredThemes.slice(indexOfFirstTheme, indexOfLastTheme);

    const handlePageChange = (_event: React.ChangeEvent<unknown>, value: number) => setCurrentPage(value);

    return tokenIsEmpty ? <Navigate to="/login" replace /> : (
        <Layout>
            <Container maxWidth="lg" sx={{ mt: 4, p: 4 }}>
                <Typography variant="h4" align="center" gutterBottom>
                    Список тем
                </Typography>

                <Grid container spacing={3}>
                    <Grid item xs={12} md={3}>
                        <Paper elevation={2} sx={{ p: 3, borderRadius: 2 }}>
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
                                <>
                                    {Array.from(new Set(themes.map((t) => t.level))).map((lvl, i) => (
                                        <MenuItem key={i} value={lvl}>{lvl}</MenuItem>
                                    ))}
                                </>
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
                                <>
                                    {Array.from(new Set(themes.map((t) => t.department))).map((dep, i) => (
                                        <MenuItem key={i} value={dep}>{dep}</MenuItem>
                                    ))}
                                </>
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
                                <>
                                    {Array.from(new Set(themes.map((t) => t.suggestedby))).map((src, i) => (
                                        <MenuItem key={i} value={src}>{src}</MenuItem>
                                    ))}
                                </>
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
                                <>
                                    {Array.from(new Set(themes.map((t) => t.supervisorid.toString()))).map((sup, i) => (
                                        <MenuItem key={i} value={sup}>{sup}</MenuItem>
                                    ))}
                                </>
                            </TextField>

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
                                }}
                            >
                                Сбросить фильтры
                            </Button>
                        </Paper>
                    </Grid>

                    <Grid item xs={12} md={9} container direction="column" spacing={2}>
                        <>
                            {currentThemes.length > 0 ? (
                                currentThemes.map((theme, index) => (
                                    <Grid item key={index}>
                                        <Card
                                            sx={{ cursor: "pointer", boxShadow: 3 }}
                                            onClick={() => navigate(`/theme/${theme.id}`)}
                                        >
                                            <CardHeader title={theme.title} />
                                            <CardContent>
                                                <Typography variant="subtitle1">Уровень: {theme.level}</Typography>
                                                <Typography variant="body2">Кафедра: {theme.department}</Typography>
                                                <Typography variant="body2">Источник: {theme.suggestedby}</Typography>
                                                <Typography variant="body2">Научный руководитель: {theme.supervisorid}</Typography>
                                                <Typography variant="body2">Консультант: {theme.consultantid}</Typography>
                                            </CardContent>
                                        </Card>
                                    </Grid>
                                ))
                            ) : (
                                <Typography variant="h6" align="center" sx={{ width: "100%" }}>
                                    Нет доступных тем
                                </Typography>
                            )}
                        </>

                        <>
                            {filteredThemes.length > itemsPerPage && (
                                <Grid item sx={{ display: "flex", justifyContent: "center" }}>
                                    <Pagination
                                        count={Math.ceil(filteredThemes.length / itemsPerPage)}
                                        page={currentPage}
                                        onChange={handlePageChange}
                                        sx={{ mt: 2 }}
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
