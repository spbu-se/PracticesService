import { useEffect, useState } from "react";
import {
    Container,
    Button,
    MenuItem,
    Typography,
    Grid,
    Paper,
    FormControl,
    InputLabel,
    Alert,
    Select,
    FormHelperText
} from "@mui/material";
import { useNavigate } from "react-router-dom";
import { getThemes, getMe, postPractice, getStudentByUserId, getLecturers, getConsultants } from "@/shared/services/axios.service";
import { Theme } from "@/entities/Theme";
import { User } from "@/entities/User";
import { InputPractice } from "@/entities/Practice";
import {Student} from "../../entities/Student";
import {Layout} from "@shared/ui/layout/Layout.tsx";
import { Consultant } from "@/entities/Consultant";
import { Group } from "@/entities/Group";
import { Lecturer } from "@/entities/Lecturer";
import {UserRole} from "../../entities/UserRoles";
import { useNotifier } from "@/shared/ui/notifications/notificationContext";

export function CreatePracticePage() {
    const notifier = useNotifier();
    const [type, setType] = useState("");
    const [themeId, setThemeId] = useState<number | null>(null);
    const [themes, setThemes] = useState<Theme[]>([]);
    const [consultantId, setConsultantId] = useState<number | null>(null);
    const [consultants, setConsultants] = useState<Consultant[]>([]);
    const [supervisorId, setSupervisorId] = useState<number | null>(null);
    const [lecturers, setLecturers] = useState<Lecturer[]>([]);
    const [me, setMe] = useState<User | null>(null);
    const [currentStudent, setCurrentStudent] = useState<Student | null>(null);
    
    const practiceTypes = [
        "Практика осенняя, 2 курс", "Практика весенняя, 2 курс", "Практика осенняя, 3 курс",
        "Практика весенняя, 3 курс", "Производственная практика", "Преддипломная практика"
        , "Бакалаврская ВКР", "Магистерская ВКР"
    ];

    const navigate = useNavigate();

    useEffect(() => {
        getThemes().then(res => setThemes(res.data));
        getLecturers().then(res => setLecturers(res.data));
        getConsultants().then(res => setConsultants(res.data));
        getMe().then(res => {
            const meData: User = res.data;
            setMe(res.data);
            
            getStudentByUserId(meData.userId).then(r => setCurrentStudent(r.data))
        });
    }, []);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!me || !currentStudent || themeId === null || supervisorId === null || !type) {
            notifier.notify("Пожалуйста, заполните все обязательные поля.", "warning");
            return;
        }

        try {
            const practice: InputPractice =  {
                studentid: currentStudent.id,
                themeid: themeId,
                type,
                finalgrade: "",
                status: "Активна",
                supervisorid: supervisorId,
                consultantid: consultantId
            };

            await postPractice(practice);
            navigate("/practices");
        } catch (error) {
            console.error("Ошибка при создании практики:", error);
            notifier.error("Не удалось создать практику. Попробуйте позже.");
        }
    };

    const getGroupName = (group?: Group | null) => {
        return group ? `${group.name} (${group.program}, ${group.year} год)` : 'Не указана';
    };

    return (
    <Layout>
        <Container maxWidth="md" sx={{mt: 4}}>
            <Button variant="contained" color="secondary" onClick={() => navigate(-1)}>
                ← Назад
            </Button>

            <Paper elevation={3} sx={{p: 4}}>
                <Typography variant="h4" gutterBottom sx={{mb: 3}}>
                    Создание новой практики
                </Typography>

                <form onSubmit={handleSubmit}>
                    <Grid container direction="column" spacing={3}>
                        <Grid size="auto">
                            <InputLabel>Ваша группа</InputLabel>
                            {getGroupName(currentStudent?.group)}
                        </Grid>

                        {!currentStudent?.group && (
                            <Grid size="auto">
                                <Alert severity="warning" sx={{ mt: 1 }}>
                                    Для создания практики необходимо сначала указать группу студента в разделе "Профиль"
                                </Alert>
                            </Grid>
                        )}

                        <Grid size="auto">
                            <FormControl fullWidth required>
                                <InputLabel>Тип практики</InputLabel>
                                <Select
                                    value={type}
                                    label="Тип практики"
                                    onChange={(e) => setType(e.target.value)}
                                    disabled={!currentStudent?.group}
                                >
                                    {practiceTypes.map((type) => (
                                        <MenuItem key={type} value={type}>
                                            {type}
                                        </MenuItem>
                                    ))}
                                </Select>
                                {!currentStudent?.group && <FormHelperText>Сначала нужно указать группу студента</FormHelperText>}
                            </FormControl>
                        </Grid>

                        <Grid size="auto">
                            <FormControl fullWidth required>
                                <InputLabel>Тема</InputLabel>
                                <Select
                                    value={themeId}
                                    label="Тема"
                                    onChange={(e) => setThemeId(Number(e.target.value))}
                                    disabled={!currentStudent?.group}
                                >
                                    {themes.map((theme) => (
                                        <MenuItem key={theme.id} value={theme.id}>
                                            {theme.title}
                                        </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                        </Grid>

                        <Grid size="auto">
                            <FormControl fullWidth required>
                                <InputLabel>{UserRole.SUPERVISOR}</InputLabel>
                                <Select
                                    value={supervisorId}
                                    label={UserRole.SUPERVISOR}
                                    onChange={(e) => setSupervisorId(Number(e.target.value))}
                                    disabled={!currentStudent?.group}
                                >
                                    {lecturers.map((lecturer) => (
                                        <MenuItem key={lecturer.id} value={lecturer.id}>
                                            {`${lecturer.lastName} ${lecturer.firstName} ${lecturer.middleName}`}
                                        </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                        </Grid>

                        <Grid size="auto">
                            <FormControl fullWidth>
                                <InputLabel>{UserRole.CONSULTANT}</InputLabel>
                                <Select
                                    value={consultantId}
                                    label={UserRole.CONSULTANT}
                                    onChange={(e) => setConsultantId(Number(e.target.value))}
                                    disabled={!currentStudent?.group}
                                >
                                    <MenuItem value="">Не выбран</MenuItem>
                                    {consultants.map((consultant) => (
                                        <MenuItem key={consultant.id} value={consultant.id}>
                                            {`${consultant.lastName} ${consultant.firstName} ${consultant.middleName}`}
                                        </MenuItem>
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
                                Создать практику
                            </Button>
                        </Grid>
                    </Grid>
                </form>
            </Paper>
        </Container>
    </Layout>
    );
}
