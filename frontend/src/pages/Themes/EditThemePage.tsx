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
import {useNavigate, useParams} from "react-router-dom";
import {Theme} from "@/entities/Theme.ts";
import {getConsultants, getLecturers, getMe, getTheme, putTheme} from "@/shared/services/axios.service.ts";
import MDEditor from '@uiw/react-md-editor';
import {Lecturer} from "@/entities/Lecturer.ts";
import {Consultant} from "@/entities/Consultant.ts";
import { User } from "@/entities/User.ts";
import {UserRole} from "../../entities/UserRoles";
import { useNotifier } from "@/shared/ui/notifications/notificationContext";

export function EditThemePage() {
    const notifier = useNotifier();
    const {id} = useParams();
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
    const [, setSources] = useState<string[]>();
    const [source, setSource] = useState("")
    const [me, setMe] = useState<User>()
    const navigate = useNavigate();
    const [lecturers, setLecturers] = useState<Lecturer[]>();
    const [consultants, setConsultants] = useState<Consultant[]>();



    useEffect(() => {
        getTheme(Number(id)).then(response => {
            const themes: Theme[] = response.data;
            const currentTheme = themes.find(s => s.id === Number(id));
            if (!currentTheme) return;
            setTitle(currentTheme?.title ?? title);
            setDescription(currentTheme?.description ?? description);
            setDepartment(currentTheme?.department ?? department);
            setSource(currentTheme?.source ?? source);
            transformStringToLevels(currentTheme?.level);
            setLecturerId(currentTheme.supervisorid ?? undefined);
            setConsultantId(currentTheme.consultantid ?? undefined);

            setSources(Array.from(new Set(themes.map((t) => t.source))));
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

    const transformStringToLevels = (levelString: string | undefined | null) => {
        if (!levelString) {
            setLevels({
                secondCourse: false,
                thirdCourse: false,
                bachelor: false,
                master: false
            });
            return;
        }

        const levelParts = levelString.split(',').map(part => part.trim());

        setLevels({
            secondCourse: levelParts.includes("2 курс"),
            thirdCourse: levelParts.includes("3 курс"),
            bachelor: levelParts.includes("Бакалаврская ВКР"),
            master: levelParts.includes("Магистерская ВКР")
        })
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!me) return;

        try {
            const theme: Partial<Theme> & { id: number } = {
                id: Number(id),
                title: title,
                description: description,
                level: transformLevelsToString(levels),
                source: source,
                suggestedby: me.userId,
                department: department ?? "",
                supervisorid: lecturerId ?? null,
                consultantid: consultantId ?? null
            };

            await putTheme(theme);
            navigate("/themes");

        } catch (error) {
            console.error("Ошибка при обновлении темы:", error);
            notifier.error("Не удалось сохранить тему. Проверьте данные и повторите попытку. " + error);
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
                        Изменить тему
                    </Typography>

                    <form onSubmit={handleSubmit}>
                        <Grid container direction="column" spacing={3}>
                            <Grid size="auto">
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

                            <Grid size="auto">
                                <Typography variant="subtitle1" gutterBottom>
                                    Описание:
                                </Typography>
                                <MDEditor
                                    value={description}
                                    onChange={(val) => {
                                        setDescription(val ?? "");
                                    }}
                                    height={200}
                                    preview="edit"
                                    visibleDragbar={false}
                                    textareaProps={{
                                        placeholder: 'Введите описание темы...',
                                    }}
                                />
                            </Grid>

                            <Grid size="auto">
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

                            <Grid size="auto">
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

                            <Grid size="auto">
                                <Typography variant="subtitle1" gutterBottom>
                                    Источник темы:
                                </Typography>
                                <FormControl fullWidth>
                                    <TextField
                                        fullWidth
                                        label="Источник темы"
                                        value={source}
                                        onChange={(e) => setSource(e.target.value)}
                                        required
                                    />
                                </FormControl>
                            </Grid>

                            <Grid size="auto">
                                <Typography variant="subtitle1" gutterBottom>
                                    {UserRole.CONSULTANT}:
                                </Typography>
                                <FormControl fullWidth>
                                    <InputLabel>{UserRole.CONSULTANT}</InputLabel>
                                    <Select
                                        value={consultantId}
                                        label={UserRole.CONSULTANT}
                                        onChange={(e) => setConsultantId(e.target.value === "" ? undefined : Number(e.target.value))}
                                    >
                                        {(consultants || []).map((con, i) => (
                                            <MenuItem key={i} value={con.id}>{con.lastName} {con.firstName} {con.middleName}</MenuItem>
                                        ))}
                                    </Select>
                                </FormControl>
                            </Grid>

                            <Grid size="auto">
                                <Typography variant="subtitle1" gutterBottom>
                                    {UserRole.SUPERVISOR}:
                                </Typography>
                                <FormControl fullWidth>
                                    <InputLabel>{UserRole.SUPERVISOR}</InputLabel>
                                    <Select
                                        value={lecturerId}
                                        label={UserRole.SUPERVISOR}
                                        onChange={(e) => setLecturerId(e.target.value === "" ? undefined : Number(e.target.value))}
                                    >
                                        {(lecturers || []).map((lecturer, i) => (
                                            <MenuItem key={i} value={lecturer.id}>{lecturer.lastName} {lecturer.firstName} {lecturer.middleName}</MenuItem>
                                        ))}
                                    </Select>
                                </FormControl>
                            </Grid>

                            <Grid size="auto">
                                <Button
                                    type="submit"
                                    variant="contained"
                                    color="primary"
                                    size="large"
                                    fullWidth
                                    sx={{mt: 2}}
                                >
                                    Сохранить
                                </Button>
                            </Grid>
                        </Grid>
                    </form>
                </Paper>
            </Container>
        </Layout>
    );
}