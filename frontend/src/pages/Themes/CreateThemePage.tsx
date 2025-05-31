import {Layout} from "@shared/ui/layout/Layout.tsx";
import {useEffect, useMemo, useState} from "react";
import {
    Container,
    TextField,
    Button,
    Grid,
    Paper,
    Typography,
    Checkbox,
    FormControlLabel,
    Select,
    MenuItem,
    InputLabel,
    FormControl,
    Stack
} from "@mui/material";
import {useNavigate} from "react-router-dom";
import {InputTheme, Theme} from "@/entities/Theme.ts";
import {getConsultants, getLecturers, getMe, getThemes, postTheme} from "@/shared/services/axios.service.ts";
import MDEditor from '@uiw/react-md-editor';
import {Lecturer} from "@/entities/Lecturer.ts";
import {Consultant} from "@/entities/Consultant.ts";
import { User } from "@/entities/User.ts";
import {UserRole} from "../../entities/UserRoles";

export function CreateThemePage() {
    const [title, setTitle] = useState("");
    const [description, setDescription] = useState("");
    const [levels, setLevels] = useState({
        secondCourse: false,
        thirdCourse: false,
        bachelor: false,
        master: false
    });
    const [lecturerId, setLecturerId] = useState<number>();
    const [consultantId, setConsultantId] = useState<number>();
    const departments = useMemo(() => ["Кафедра системного программирования", "Кафедра параллельных алгоритмов",
        "Кафедра информатики", "Кафедра информационно-аналитических систем"], [])
    const [department, setDepartment] = useState<string>();
    const [source, setSource] = useState("")
    const [me, setMe] = useState<User>()
    const navigate = useNavigate();
    const [lecturers, setLecturers] = useState<Lecturer[]>();
    const [consultants, setConsultants] = useState<Consultant[]>();

    const [isMdTouched, setIsMdTouched] = useState(false);

    const isMdError = isMdTouched && !description;

    useEffect(() => {
        getThemes().then(response => {
            const themes: Theme[] = response.data;
        });

        getLecturers().then(response => {
            setLecturers(response.data);
        });

        getConsultants().then(response => {
            setConsultants(response.data);
        });

        getMe().then(response => {
            const data: User = response.data
            setMe(data);
            setSource(`${data.lastName} ${data.firstName} ${data.middleName}`);
        })
    }, []);

    const transformLevelsToString = (levels: {
        secondCourse: boolean;
        thirdCourse: boolean;
        bachelor: boolean;
        master: boolean;
    }): string => {
        const selectedLevels: string[] = [];

        if (levels.secondCourse) selectedLevels.push("2 курс");
        if (levels.thirdCourse) selectedLevels.push("3 курс");
        if (levels.bachelor) selectedLevels.push("Бакалаврская ВКР");
        if (levels.master) selectedLevels.push("Магистерская ВКР");

        return selectedLevels.join(", ");
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!Object.values(levels).some(v => v)) return;

        try {
            const theme: InputTheme = {
                title: title,
                description: description,
                level: transformLevelsToString(levels),
                source: source,
                suggestedby: me.userId,
                department: department,
                supervisorid: lecturerId,
                consultantid: consultantId
            };

            await postTheme(theme);
            navigate("/");

        } catch (error) {
            console.error("Ошибка при создании темы:", error);
            alert("Не удалось создать тему. Проверьте данные и повторите попытку. " + error);
        }
    };

    return (
        <Layout>
            <Container maxWidth="md" sx={{mt: 4}}>
                <Button variant="contained" color="secondary" onClick={() => navigate(-1)}>
                    ← Назад
                </Button>

                <Paper elevation={3} sx={{p: 4}}>
                    <Typography variant="h4" gutterBottom sx={{mb: 3}}>
                        Предложить новую тему
                    </Typography>

                    <form onSubmit={handleSubmit}>
                        <Grid container direction="column" spacing={3}>
                            <Grid item>
                                <Typography variant="subtitle1" gutterBottom>
                                    Название:
                                </Typography>
                                <TextField
                                    fullWidth
                                    label="Название темы"
                                    value={title}
                                    onChange={(e) => setTitle(e.target.value)}
                                    required
                                />
                            </Grid>

                            <Grid item>
                                <Typography variant="subtitle1" gutterBottom>
                                    Описание:
                                </Typography>
                                <MDEditor
                                    value={description}
                                    onChange={(val) => {
                                        setDescription(val);
                                        setIsMdTouched(true);
                                    }}
                                    height={200}
                                    preview="edit"
                                    visibleDragbar={false}
                                    textareaProps={{
                                        placeholder: 'Введите описание темы...',
                                    }}
                                />
                            </Grid>

                            <Grid item>
                                <Typography variant="subtitle1" gutterBottom>
                                    Уровень
                                </Typography>
                                <Stack spacing={1}>
                                    {Object.entries(levels).map(([key, value]) => (
                                        <FormControlLabel
                                            key={key}
                                            control={
                                                <Checkbox
                                                    checked={value}
                                                    onChange={(e) => setLevels({
                                                        ...levels,
                                                        [key]: e.target.checked
                                                    })}
                                                />
                                            }
                                            label={
                                                key === 'secondCourse' ? '2 курс' :
                                                    key === 'thirdCourse' ? '3 курс' :
                                                        key === 'bachelor' ? 'Бакалаврская ВКР' :
                                                            'Магистерская ВКР'
                                            }
                                        />
                                    ))}
                                </Stack>
                            </Grid>

                            <Grid item>
                                <Typography variant="subtitle1" gutterBottom>
                                    Кафедра:
                                </Typography>
                                <FormControl fullWidth>
                                    <InputLabel>Кафедра</InputLabel>
                                    <Select
                                        value={department}
                                        label="Кафедра"
                                        onChange={(e) => setDepartment(e.target.value)}
                                    >
                                        {(departments || []).map((department, i) => (
                                            <MenuItem key={i} value={department}>{department}</MenuItem>
                                        ))}
                                    </Select>
                                </FormControl>
                            </Grid>

                            <Grid item>
                                <Typography variant="subtitle1" gutterBottom>
                                    Источник темы:
                                </Typography>
                                <TextField
                                    fullWidth
                                    label="Источник темы"
                                    value={source}
                                    onChange={(e) => setSource(e.target.value)}
                                    required
                                />
                            </Grid>

                            <Grid item>
                                <Typography variant="subtitle1" gutterBottom>
                                    {UserRole.CONSULTANT}:
                                </Typography>
                                <FormControl fullWidth>
                                    <InputLabel>{UserRole.CONSULTANT}</InputLabel>
                                    <Select
                                        value={consultantId}
                                        label={UserRole.CONSULTANT}
                                        onChange={(e) => setConsultantId(e.target.value)}
                                    >
                                        {(consultants || []).map((con, i) => (
                                            <MenuItem key={i} value={con.id}>{con.lastName} {con.firstName} {con.middleName}</MenuItem>
                                        ))}
                                    </Select>
                                </FormControl>
                            </Grid>

                            <Grid item>
                                <Typography variant="subtitle1" gutterBottom>
                                    {UserRole.SUPERVISOR}:
                                </Typography>
                                <FormControl fullWidth>
                                    <InputLabel>{UserRole.SUPERVISOR}</InputLabel>
                                    <Select
                                        value={lecturerId}
                                        label={UserRole.SUPERVISOR}
                                        onChange={(e) => setLecturerId(e.target.value)}
                                    >
                                        {(lecturers || []).map((lecturer, i) => (
                                            <MenuItem key={i} value={lecturer.id}>{lecturer.lastName} {lecturer.firstName} {lecturer.middleName}</MenuItem>
                                        ))}
                                    </Select>
                                </FormControl>
                            </Grid>

                            <Grid item>
                                <Button
                                    type="submit"
                                    variant="contained"
                                    color="primary"
                                    size="large"
                                    fullWidth
                                    sx={{mt: 2}}
                                >
                                    Предложить тему
                                </Button>
                            </Grid>
                        </Grid>
                    </form>
                </Paper>
            </Container>
        </Layout>
    );
}