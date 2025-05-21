import { useState } from "react";
import {
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    Grid,
    Button,
    Typography,
    Card,
    CardContent,
    Modal,
    Box,
    TextField,
    Divider
} from "@mui/material";
import { Theme } from "@/entities/Theme";
import {getPractice} from "@/shared/services/axios.service.ts";
import { useEffect } from "react";

const style = {
    position: 'absolute' as 'absolute',
    top: '50%',
    left: '50%',
    transform: 'translate(-50%, -50%)',
    width: 400,
    bgcolor: 'background.paper',
    boxShadow: 24,
    p: 4,
    borderRadius: 1
};

export function ThemeSelection({ practiceId }: { practiceId?: number }) {
    const [practice, setPractice] = useState<Practice>();
    const [themes, setThemes] = useState<Theme[]>([]);
    const [selectedTheme, setSelectedTheme] = useState<string>("");
    const [showConsultantInput, setShowConsultantInput] = useState(false);
    const [openModal, setOpenModal] = useState(false);
    const [consultantName, setConsultantName] = useState("");

    const handleAddConsultant = () => {
        setShowConsultantInput(true);
    };

    const handleCancelConsultant = () => {
        setShowConsultantInput(false);
        setConsultantName("");
    };

    const handleSaveConsultant = () => {
        setShowConsultantInput(false);
        setConsultantName("");
    };

    useEffect(() => {
        getPractice(practiceId).then(response => {
            const selectedPractice = response.data.find((t: Practice) => t.id == practiceId);
            setPractice(selectedPractice);
        })
    }, []);

    const handleOpenModal = () => setOpenModal(true);
    const handleCloseModal = () => setOpenModal(false);

    return (
        <Grid container spacing={3}>
            <Grid item xs={12}>
                <Typography variant="h6" gutterBottom>
                    Выбор темы практики
                </Typography>

                {practice?.finalgrade && (
                    <Box sx={{ mb: 3, p: 2, borderRadius: 1 }}>
                        <Typography color="warning" fontWeight="bold" gutterBottom>
                            Завершённая работа
                        </Typography>
                        <Typography variant="body2">
                            Научный руководитель или руководитель практики пометил эту работу как завершенную
                        </Typography>
                    </Box>
                )}
            </Grid>

            <Grid item xs={12}>
                <Card variant="outlined" sx={{ border: 0 }}>
                    <CardContent sx={{ py: 1 }}>
                        <Typography variant="subtitle1" gutterBottom>
                            Выбранная тема
                        </Typography>
                        <Typography variant="h5" gutterBottom>
                            {practice?.theme?.title}
                        </Typography>
                        <Typography paragraph sx={{ mb: 0 }}>
                            Научный руководитель: <b>{practice?.supervisor.lastName} {practice?.supervisor.firstName} {practice?.supervisor.middleName}</b>
                            <br />
                            Тип работы: <b>{practice?.type}</b>
                            <br />
                            {!showConsultantInput && (
                                <Button
                                    size="small"
                                    onClick={handleAddConsultant}
                                    sx={{ p: 0, textTransform: 'none' }}
                                >
                                    Добавить консультанта
                                </Button>
                            )}
                        </Typography>

                        {showConsultantInput && (
                            <Box sx={{ mt: 2 }}>
                                <Grid container spacing={2}>
                                    <Grid item xs={12}>
                                        <TextField
                                            fullWidth
                                            size="small"
                                            label="ФИО консультанта, должность и компания"
                                            value={consultantName}
                                            onChange={(e) => setConsultantName(e.target.value)}
                                        />
                                    </Grid>
                                    <Grid item xs={12}>
                                        <Box display="flex" justifyContent="space-between">
                                            <Button
                                                variant="contained"
                                                size="small"
                                                onClick={handleSaveConsultant}
                                            >
                                                Сохранить
                                            </Button>
                                            <Button
                                                variant="outlined"
                                                color="error"
                                                size="small"
                                                onClick={handleCancelConsultant}
                                            >
                                                Отмена
                                            </Button>
                                        </Box>
                                    </Grid>
                                </Grid>
                            </Box>
                        )}

                        <Box display="flex" justifyContent="space-between" sx={{ mt: 3 }}>
                            <Button
                                variant="contained"
                                size="small"
                                href={`/practice/edit_theme/?id=${practiceId}`}
                            >
                                Редактировать
                            </Button>
                            <Button
                                variant="outlined"
                                color="error"
                                size="small"
                                onClick={handleOpenModal}
                            >
                                Отказаться от темы
                            </Button>
                        </Box>
                    </CardContent>
                </Card>
            </Grid>

            <Modal
                open={openModal}
                onClose={handleCloseModal}
                aria-labelledby="modal-title"
                aria-describedby="modal-description"
            >
                <Box sx={style}>
                    <Typography id="modal-title" variant="h6" component="h2">
                        Подтверждение
                    </Typography>
                    <Typography id="modal-description" sx={{ mt: 2 }}>
                        Вы точно хотите отказаться от данной темы?
                        <br />
                        <b>{practice?.theme.title}</b>
                    </Typography>
                    <Box display="flex" justifyContent="flex-end" sx={{ mt: 3 }}>
                        <Button
                            variant="contained"
                            color="error"
                            sx={{ mr: 2 }}
                        >
                            Да, отказываюсь!
                        </Button>
                        <Button
                            variant="outlined"
                            onClick={handleCloseModal}
                        >
                            Нет, оставить
                        </Button>
                    </Box>
                </Box>
            </Modal>
        </Grid>
    );
}