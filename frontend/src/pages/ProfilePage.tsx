import { Layout } from "@shared/ui/layout/Layout.tsx";
import { getJWTToken } from "../shared/services/localStorage.service.ts";
import { Navigate } from "react-router-dom";
import React, { useEffect, useState } from "react";
import {
    getMe,
    getAllGroups,
    getStudentByUserId,
    getConsultantByUserId,
    getLecturerByUserId,
    updateUser,
    updateStudent,
    updateLecturer,
    updateConsultant
} from "../shared/services/axios.service.ts";
import {
    Container,
    Paper,
    Typography,
    Avatar,
    Box,
    Button,
    Grid,
    Chip,
    Divider,
    TextField,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    Select,
    MenuItem,
    InputLabel,
    FormControl,
    Switch,
    FormControlLabel
} from "@mui/material";
import { User } from "@/entities/User.ts";
import { useNavigate } from "react-router-dom";
import { Student } from "@/entities/Student";
import { Lecturer } from "@/entities/Lecturer";
import { Consultant } from "@/entities/Consultant";
import { UserRole } from "@/entities/UserRoles";
import { Group } from "@/entities/Group";

export function ProfilePage() {
    const navigate = useNavigate();
    const tokenIsEmpty = getJWTToken() === "";
    const [user, setUser] = useState<User>();
    const [studentInfo, setStudentInfo] = useState<Student | null>(null);
    const [lecturerInfo, setLecturerInfo] = useState<Lecturer | null>(null);
    const [consultantInfo, setConsultantInfo] = useState<Consultant | null>(null);
    const [loading, setLoading] = useState(true);
    const [editMode, setEditMode] = useState(false);
    const [editDialogOpen, setEditDialogOpen] = useState(false);
    const [editedUser, setEditedUser] = useState<Partial<User>>({});
    const [editedStudent, setEditedStudent] = useState<Partial<Student>>({});
    const [editedLecturer, setEditedLecturer] = useState<Partial<Lecturer>>({});
    const [editedConsultant, setEditedConsultant] = useState<Partial<Consultant>>({});
    const [groups, setGroups] = useState<Group[]>([]);

    useEffect(() => {
        fetchUserData();

        getAllGroups().then(response => setGroups(response.data))
    }, []);

    const fetchUserData = () => {
        setLoading(true);
        getMe().then(response => {
            const data: User = response.data;
            setUser(data);
            setEditedUser({...data});

            if (data.roles.includes(UserRole.STUDENT)) {
                getStudentByUserId(data.userId).then(res => {
                    setStudentInfo(res.data);
                    setEditedStudent({...res.data});
                    setLoading(false);
                });
            }
            if (data.roles.includes(UserRole.SUPERVISOR)) {
                getLecturerByUserId(data.userId).then(res => {
                    setLecturerInfo(res.data);
                    setEditedLecturer({...res.data});
                    setLoading(false);
                }).catch(() => {
                    setLoading(false);
                });
            }
            if (data.roles.includes(UserRole.CONSULTANT)) {
                getConsultantByUserId(data.userId).then(res => {
                    setConsultantInfo(res.data);
                    setEditedConsultant({...res.data});
                    setLoading(false);
                }).catch(() => {
                    setLoading(false);
                });
            } else {
                setLoading(false);
            }
        }).catch(() => {
            setLoading(false);
        });
    };

    const handleEditClick = () => {
        setEditMode(true);
        setEditDialogOpen(true);
    };

    const handleSaveChanges = async () => {
        try {
            // Update user basic info
            if (editedUser) {
                await updateUser(editedUser);
            }

            // Update role-specific info
            if (user?.roles.includes(UserRole.STUDENT)) {
                await updateStudent(editedStudent);
            }
            if (user?.roles.includes(UserRole.SUPERVISOR)) {
                await updateLecturer(editedLecturer);
            }
            if (user?.roles.includes(UserRole.CONSULTANT)) {
                await updateConsultant(editedConsultant);
            }

            // Refresh data
            fetchUserData();
            setEditMode(false);
            setEditDialogOpen(false);
        } catch (error) {
            console.error("Error updating profile:", error);
        }
    };

    const handleCancelEdit = () => {
        setEditMode(false);
        setEditDialogOpen(false);
        // Reset edited data
        if (user) setEditedUser({...user});
        if (studentInfo) setEditedStudent({...studentInfo});
        if (lecturerInfo) setEditedLecturer({...lecturerInfo});
        if (consultantInfo) setEditedConsultant({...consultantInfo});
    };

    const handleUserFieldChange = (field: keyof User, value: any) => {
        setEditedUser(prev => ({ ...prev, [field]: value }));
    };

    const handleStudentFieldChange = (field: keyof Student, value: any) => {
        setEditedStudent(prev => ({ ...prev, [field]: value }));
    };

    const handleLecturerFieldChange = (field: keyof Lecturer, value: any) => {
        setEditedLecturer(prev => ({ ...prev, [field]: value }));
    };

    const handleConsultantFieldChange = (field: keyof Consultant, value: any) => {
        setEditedConsultant(prev => ({ ...prev, [field]: value }));
    };

    if (tokenIsEmpty) {
        return <Navigate to="/login" replace />;
    }

    if (loading || !user) {
        return (
            <Layout>
                <Container maxWidth="md" sx={{ mt: 4 }}>
                    <Typography variant="h6">Загрузка...</Typography>
                </Container>
            </Layout>
        );
    }

    const getGroupName = (group: Group) => {
        return group ? `${group.name} (${group.program}, ${group.year} год)` : 'Не указана';
    };

    const fullName = `${user.lastName} ${user.firstName} ${user.middleName || ''}`.trim();

    return (
        <Layout>
            <Container maxWidth="md" sx={{ mt: 4 }}>
                <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
                    <Button variant="contained" color="secondary" onClick={() => navigate(-1)}>
                        ← Назад
                    </Button>
                    <Button variant="contained" color="primary" onClick={handleEditClick}>
                        Редактировать профиль
                    </Button>
                </Box>

                <Paper elevation={3} sx={{ p: 4, mt: 2 }}>
                    <Box display="flex" alignItems="center" gap={4} mb={4}>
                        <Avatar
                            sx={{
                                width: 100,
                                height: 100,
                                fontSize: 40,
                                bgcolor: 'primary.main'
                            }}
                        >
                            {user.firstName?.[0]}{user.lastName?.[0]}
                        </Avatar>

                        <Box>
                            <Typography variant="h5" component="div">
                                {fullName || "Не указано"}
                            </Typography>
                            <Typography variant="subtitle1" color="text.secondary">
                                {user.userName}
                            </Typography>
                        </Box>
                    </Box>

                    <Grid container spacing={3}>
                        <Grid item xs={12} md={6}>
                            <Typography variant="subtitle1" color="text.secondary">
                                Email
                            </Typography>
                            <Typography variant="body1" paragraph>
                                {user.email || "Не указан"}
                            </Typography>
                        </Grid>

                        <Grid item xs={12} md={6}>
                            <Typography variant="subtitle1" color="text.secondary">
                                Роли
                            </Typography>
                            <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                                {user.roles.map(role => (
                                    <Chip
                                        key={role}
                                        label={role}
                                        color={
                                            role === UserRole.ADMIN ? 'error' :
                                                role === UserRole.SUPERVISOR ? 'primary' :
                                                    role === UserRole.CONSULTANT ? 'info' :
                                                        role === UserRole.PRACTICE_SUPERVISOR ? 'secondary' :
                                                            'default'
                                        }
                                    />
                                ))}
                            </Box>
                        </Grid>

                        {user.roles.includes(UserRole.STUDENT) && studentInfo && (
                            <Grid item xs={12}>
                                <Typography variant="subtitle1" color="text.secondary">
                                    Группа
                                </Typography>
                                <Typography variant="body1" paragraph>
                                    {studentInfo.group ? getGroupName(studentInfo.group) : 'Не указана'}
                                </Typography>
                            </Grid>
                        )}

                        {user.roles.includes(UserRole.SUPERVISOR) && lecturerInfo && (
                            <Grid item xs={12}>
                                <Typography variant="subtitle1" color="text.secondary">
                                    Может руководить ВКР
                                </Typography>
                                <Typography variant="body1" paragraph>
                                    {lecturerInfo.canSuperviseVkr ? 'Да' : 'Нет'}
                                </Typography>
                            </Grid>
                        )}

                        {user.roles.includes(UserRole.CONSULTANT) && consultantInfo && (
                            <>
                                <Grid item xs={12}>
                                    <Divider sx={{ my: 2 }} />
                                    <Typography variant="h6" gutterBottom>
                                        Контактная информация
                                    </Typography>
                                </Grid>
                                <Grid item xs={12} md={6}>
                                    <Typography variant="subtitle1" color="text.secondary">
                                        Контактные данные
                                    </Typography>
                                    <Typography variant="body1" paragraph>
                                        {consultantInfo.contact || 'Не указаны'}
                                    </Typography>
                                </Grid>
                            </>
                        )}
                    </Grid>
                </Paper>
            </Container>

            {/* Edit Dialog */}
            <Dialog open={editDialogOpen} onClose={handleCancelEdit} maxWidth="md" fullWidth>
                <DialogTitle>Редактирование профиля</DialogTitle>
                <DialogContent>
                    <Grid container spacing={3} sx={{ mt: 1 }}>
                        <Grid item xs={12} md={6}>
                            <TextField
                                fullWidth
                                label="Фамилия"
                                value={editedUser.lastName || ''}
                                onChange={(e) => handleUserFieldChange('lastName', e.target.value)}
                                margin="normal"
                            />
                        </Grid>
                        <Grid item xs={12} md={6}>
                            <TextField
                                fullWidth
                                label="Имя"
                                value={editedUser.firstName || ''}
                                onChange={(e) => handleUserFieldChange('firstName', e.target.value)}
                                margin="normal"
                            />
                        </Grid>
                        <Grid item xs={12} md={6}>
                            <TextField
                                fullWidth
                                label="Отчество"
                                value={editedUser.middleName || ''}
                                onChange={(e) => handleUserFieldChange('middleName', e.target.value)}
                                margin="normal"
                            />
                        </Grid>
                        <Grid item xs={12} md={6}>
                            <TextField
                                fullWidth
                                label="Email"
                                type="email"
                                value={editedUser.email || ''}
                                onChange={(e) => handleUserFieldChange('email', e.target.value)}
                                margin="normal"
                            />
                        </Grid>

                        {user.roles.includes(UserRole.STUDENT) && (
                            <Grid item xs={12}>
                                <FormControl fullWidth margin="normal">
                                    <InputLabel id="group-select-label">Группа</InputLabel>
                                    <Select
                                        labelId="group-select-label"
                                        value={editedStudent?.groupId || ''}
                                        onChange={(e) => handleStudentFieldChange('groupId', e.target.value)}
                                        label="Группа"
                                    >
                                        <MenuItem value="" disabled>
                                            Выберите группу
                                        </MenuItem>
                                        {groups.map((group) => (
                                            <MenuItem key={group.id} value={group.id}>
                                                {group.name} ({group.program}, {group.year} год)
                                            </MenuItem>
                                        ))}
                                    </Select>
                                </FormControl>
                            </Grid>
                        )}

                        {user.roles.includes(UserRole.SUPERVISOR) && (
                            <Grid item xs={12}>
                                <FormControlLabel
                                    control={
                                        <Switch
                                            checked={editedLecturer.canSuperviseVkr || false}
                                            onChange={(e) => handleLecturerFieldChange('canSuperviseVkr', e.target.checked)}
                                        />
                                    }
                                    label="Может руководить ВКР"
                                />
                            </Grid>
                        )}

                        {user.roles.includes(UserRole.CONSULTANT) && (
                            <>
                                <Grid item xs={12}>
                                    <Divider sx={{ my: 2 }} />
                                    <Typography variant="h6">Контактная информация</Typography>
                                </Grid>
                                <Grid item xs={12}>
                                    <TextField
                                        fullWidth
                                        label="Контактные данные"
                                        value={editedConsultant.contact || ''}
                                        onChange={(e) => handleConsultantFieldChange('contact', e.target.value)}
                                        margin="normal"
                                        multiline
                                        rows={3}
                                    />
                                </Grid>
                            </>
                        )}
                    </Grid>
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleCancelEdit} color="secondary">
                        Отмена
                    </Button>
                    <Button onClick={handleSaveChanges} color="primary" variant="contained">
                        Сохранить
                    </Button>
                </DialogActions>
            </Dialog>
        </Layout>
    );
}