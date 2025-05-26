import { useState, useEffect } from "react";
import {
    FormControl, InputLabel, Select, MenuItem, Grid, Button, Typography, Card, CardContent,
    Modal, Box, TextField, Divider
} from "@mui/material";
import { Theme } from "@/entities/Theme";
import { getPractice, getConsultants, getThemes, getLecturers, putPractice } from "@/shared/services/axios.service.ts";
import { UserRole } from "@/entities/UserRoles";
import { Practice } from "@/entities/Practice";
import { Consultant } from "@/entities/Consultant";
import { Supervisor } from "@/entities/Supervisor";

const modalStyle = {
    position: 'absolute',
    top: '50%',
    left: '50%',
    transform: 'translate(-50%, -50%)',
    width: 500,
    bgcolor: 'background.paper',
    boxShadow: 24,
    p: 4,
    borderRadius: 2,
};

export function ThemeSelection({ practiceId }: { practiceId?: number }) {
    const [practice, setPractice] = useState<Practice>();
    const [consultants, setConsultants] = useState<Consultant[]>([]);
    const [themes, setThemes] = useState<Theme[]>([]);
    const [supervisors, setSupervisors] = useState<Supervisor[]>([]);

    const [showConsultantInput, setShowConsultantInput] = useState(false);
    const [consultantId, setConsultantId] = useState<number | null>(null);
    const [editModalOpen, setEditModalOpen] = useState(false);
    const [selectedTheme, setSelectedTheme] = useState<number | null>(null);
    const [selectedSupervisor, setSelectedSupervisor] = useState<number | null>(null);

    useEffect(() => {
        getConsultants().then(res => setConsultants(res.data));
        getThemes().then(res => setThemes(res.data));
        getLecturers().then(res => setSupervisors(res.data));
        getPractice(practiceId).then(response => {
            const selectedPractice = response.data.find((t: Practice) => t.id == practiceId);
            setPractice(selectedPractice);
            setSelectedTheme(selectedPractice?.themeid ?? null);
            setSelectedSupervisor(selectedPractice?.supervisorid ?? null);
        });
    }, [practiceId]);

    const handleEditSave = async () => {
        if (practice) {
            const updatedPractice = {
                ...practice,
                themeid: selectedTheme,
                supervisorid: selectedSupervisor
            };
            await putPractice(updatedPractice);
            const response = await getPractice(practiceId);
            const refreshedPractice = response.data.find((t: Practice) => t.id == practiceId);
            setPractice(refreshedPractice);
            setEditModalOpen(false);
        }
    };

    return (
        <Grid container spacing={3}>
            <Grid item xs={12}>
                <Typography variant="h6" gutterBottom>Выбор темы практики</Typography>
                {practice?.finalgrade && (
                    <Box sx={{ mb: 3, p: 2, borderRadius: 1 }}>
                        <Typography color="warning" fontWeight="bold" gutterBottom>Завершённая работа</Typography>
                        <Typography variant="body2">
                            Научный руководитель или руководитель практики пометил эту работу как завершенную
                        </Typography>
                    </Box>
                )}
            </Grid>
            <Grid item xs={12}>
                <Card variant="outlined" sx={{ border: 0 }}>
                    <CardContent sx={{ py: 1 }}>
                        <Typography variant="subtitle1" gutterBottom>Выбранная тема</Typography>
                        <Typography variant="h5" gutterBottom>{practice?.theme?.title}</Typography>
                        <Typography paragraph sx={{ mb: 0 }}>
                            Научный руководитель: <b>{practice?.supervisor?.lastName} {practice?.supervisor?.firstName} {practice?.supervisor?.middleName}</b><br/>
                            Тип работы: <b>{practice?.type}</b><br/>
                            {practice?.consultant == null ? (
                                <Button size="small" onClick={() => setShowConsultantInput(true)} sx={{ p: 0, textTransform: 'none' }}>Добавить консультанта</Button>
                            ) : (
                                <>Консультант: <b>{practice?.consultant?.lastName} {practice?.consultant?.firstName} {practice?.consultant?.middleName}</b><br/>
                                    Контакты: <b>{practice?.consultant?.contact}</b></>
                            )}
                        </Typography>

                        {showConsultantInput && (
                            <Box sx={{ mt: 2 }}>
                                <FormControl fullWidth>
                                    <InputLabel>{UserRole.CONSULTANT}</InputLabel>
                                    <Select value={consultantId ?? ""} onChange={e => setConsultantId(Number(e.target.value))}>
                                        {consultants.map(c => (
                                            <MenuItem key={c.id} value={c.id}>{c.lastName} {c.firstName} {c.middleName}</MenuItem>
                                        ))}
                                    </Select>
                                </FormControl>
                                <Box display="flex" justifyContent="space-between" sx={{ mt: 1 }}>
                                    <Button variant="contained" size="small" onClick={() => { practice!.consultantid = consultantId; putPractice(practice); setShowConsultantInput(false); }}>Сохранить</Button>
                                    <Button variant="outlined" color="error" size="small" onClick={() => setShowConsultantInput(false)}>Отмена</Button>
                                </Box>
                            </Box>
                        )}

                        <Box display="flex" justifyContent="space-between" sx={{ mt: 3 }}>
                            <Button variant="contained" size="small" onClick={() => setEditModalOpen(true)}>Редактировать</Button>
                            <Button variant="outlined" color="error" size="small">Отказаться от темы</Button>
                        </Box>
                    </CardContent>
                </Card>
            </Grid>
            
            <Modal open={editModalOpen} onClose={() => setEditModalOpen(false)}>
                <Box sx={modalStyle}>
                    <Typography variant="h6" gutterBottom>Редактировать тему и руководителя</Typography>
                    <FormControl fullWidth sx={{ mt: 2 }}>
                        <InputLabel>Тема</InputLabel>
                        <Select value={selectedTheme ?? ""} onChange={e => setSelectedTheme(Number(e.target.value))}>
                            {themes.map(theme => (
                                <MenuItem key={theme.id} value={theme.id}>{theme.title}</MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                    <FormControl fullWidth sx={{ mt: 2 }}>
                        <InputLabel>Научный руководитель</InputLabel>
                        <Select value={selectedSupervisor ?? ""} onChange={e => setSelectedSupervisor(Number(e.target.value))}>
                            {supervisors.map(s => (
                                <MenuItem key={s.id} value={s.id}>{s.lastName} {s.firstName} {s.middleName}</MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                    <FormControl fullWidth sx={{ mt: 2 }}>
                        <InputLabel>{UserRole.CONSULTANT}</InputLabel>
                        <Select value={consultantId ?? ""} onChange={e => setConsultantId(Number(e.target.value))}>
                            {consultants.map(c => (
                                <MenuItem key={c.id} value={c.id}>{c.lastName} {c.firstName} {c.middleName}</MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                    <Box display="flex" justifyContent="flex-end" sx={{ mt: 3 }}>
                        <Button variant="contained" onClick={handleEditSave} sx={{ mr: 2 }}>Сохранить</Button>
                        <Button variant="outlined" onClick={() => setEditModalOpen(false)}>Отмена</Button>
                    </Box>
                </Box>
            </Modal>
        </Grid>
    );
}