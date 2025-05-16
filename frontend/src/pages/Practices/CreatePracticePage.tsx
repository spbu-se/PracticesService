import { useEffect, useState } from "react";
import {
    Container,
    TextField,
    Button,
    MenuItem,
    Typography,
    Grid,
    Paper,
    FormControl,
    InputLabel,
    Select, Stack, FormControlLabel, Checkbox
} from "@mui/material";
import { useNavigate } from "react-router-dom";
import { getThemes, getMe, postPractice, getStudentByUserId, getLecturers, getConsultants } from "@/shared/services/axios.service";
import { Theme } from "@/entities/Theme";
import { User } from "@/entities/User";
import { InputPractice } from "@/entities/Practice";
import {Student} from "../../entities/Student";
import {Layout} from "@shared/ui/layout/Layout.tsx";
import MDEditor from "@uiw/react-md-editor";
import { Consultant } from "@/entities/Consultant";
import { Lecturer } from "@/entities/Lecturer";
import {UserRole} from "../../entities/UserRoles";

export function CreatePracticePage() {
    const [type, setType] = useState("");
    const [themeId, setThemeId] = useState<number | "">("");
    const [themes, setThemes] = useState<Theme[]>([]);
    const [consultantId, setConsultantId] = useState<number | "">("");
    const [consultants, setConsultants] = useState<Consultant[]>([]);
    const [supervisorId, setSupervisorId] = useState<number | "">("");
    const [lecturers, setLecturers] = useState<Lecturer[]>([]);
    const [me, setMe] = useState<User | null>(null);
    const [currentStudent, setCurrentStudent] = useState<Student | null>(null);

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
        if (!me || themeId === "" || !type) {
            alert("Пожалуйста, заполните все обязательные поля.");
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
            alert("Не удалось создать практику. Попробуйте позже.");
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
                    Создание новой практики
                </Typography>

                <form onSubmit={handleSubmit}>
                    <Grid container direction="column" spacing={3}>
                        <Grid item>
                            <InputLabel>Ваша группа</InputLabel>
                            {currentStudent?.group.name}
                        </Grid>
                        
                        <Grid item>
                            <FormControl fullWidth required>
                                <InputLabel>Тип практики</InputLabel>
                                <Select value={type} label="Тип практики" onChange={(e) => setType(e.target.value)}>
                                    <MenuItem value="Учебная">Учебная</MenuItem>
                                    <MenuItem value="Производственная">Производственная</MenuItem>
                                    <MenuItem value="Преддипломная">Преддипломная</MenuItem>
                                </Select>
                            </FormControl>
                        </Grid>

                        <Grid item>
                            <FormControl fullWidth required>
                                <InputLabel>Тема</InputLabel>
                                <Select
                                    value={themeId}
                                    label="Тема"
                                    onChange={(e) => setThemeId(Number(e.target.value))}
                                >
                                    {themes.map((theme) => (
                                        <MenuItem key={theme.id} value={theme.id}>
                                            {theme.title}
                                        </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                        </Grid>

                        <Grid item>
                            <FormControl fullWidth required>
                                <InputLabel>{UserRole.SUPERVISOR}</InputLabel>
                                <Select
                                    value={supervisorId}
                                    label={UserRole.SUPERVISOR}
                                    onChange={(e) => setSupervisorId(Number(e.target.value))}
                                >
                                    {lecturers.map((lecturer) => (
                                        <MenuItem key={lecturer.id} value={lecturer.id}>
                                            {`${lecturer.lastName} ${lecturer.firstName} ${lecturer.middleName}`}
                                        </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                        </Grid>

                        <Grid item>
                            <FormControl fullWidth>
                                <InputLabel>{UserRole.CONSULTANT}</InputLabel>
                                <Select
                                    value={consultantId}
                                    label={UserRole.CONSULTANT}
                                    onChange={(e) => setConsultantId(Number(e.target.value))}
                                >
                                    {consultants.map((consultant) => (
                                        <MenuItem key={consultant.id} value={consultant.id}>
                                            {`${consultant.lastName} ${consultant.firstName} ${consultant.middleName}`}
                                        </MenuItem>
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
