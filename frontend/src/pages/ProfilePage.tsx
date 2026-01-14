import { Layout } from "@shared/ui/layout/Layout.tsx";
import { getJWTToken } from "../shared/services/localStorage.service.ts";
import { Navigate, useNavigate } from "react-router-dom";
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
    Skeleton,
    Card,
    CardContent,
    Stack,
    IconButton,
    Tooltip,
    InputAdornment
} from "@mui/material";
import {
    Email as EmailIcon,
    School as SchoolIcon,
    Person as PersonIcon,
    Badge as BadgeIcon,
    Edit as EditIcon,
    ArrowBack as ArrowBackIcon,
    Work as WorkIcon,
    ContactPhone as ContactPhoneIcon,
    Groups as GroupsIcon
} from "@mui/icons-material";
import { User } from "@/entities/User.ts";
import { Student } from "@/entities/Student";
import { Lecturer } from "@/entities/Lecturer";
import { Consultant } from "@/entities/Consultant";
import { UserRole } from "@/entities/UserRoles";
import { Group } from "@/entities/Group";

const ProfileField = ({ icon, label, value }: { icon: React.ReactNode, label: string, value: React.ReactNode }) => (
    <Box display="flex" alignItems="flex-start" gap={2} mb={2}>
        <Box sx={{ color: 'text.secondary', mt: 0.5 }}>{icon}</Box>
        <Box>
            <Typography variant="caption" color="text.secondary" display="block">
                {label}
            </Typography>
            <Typography variant="body1" color="text.primary">
                {value}
            </Typography>
        </Box>
    </Box>
);

export function ProfilePage() {
    const navigate = useNavigate();
    const tokenIsEmpty = getJWTToken() === "";
    
    const [user, setUser] = useState<User>();
    const [studentInfo, setStudentInfo] = useState<Student | null>(null);
    const [lecturerInfo, setLecturerInfo] = useState<Lecturer | null>(null);
    const [consultantInfo, setConsultantInfo] = useState<Consultant | null>(null);
    
    const [loading, setLoading] = useState(true);
    const [groups, setGroups] = useState<Group[]>([]);
    
    const [editDialogOpen, setEditDialogOpen] = useState(false);
    const [editedUser, setEditedUser] = useState<Partial<User>>({});
    const [editedStudent, setEditedStudent] = useState<Partial<Student>>({});
    const [editedLecturer, setEditedLecturer] = useState<Partial<Lecturer>>({});
    const [editedConsultant, setEditedConsultant] = useState<Partial<Consultant>>({});

    useEffect(() => {
        fetchUserData();
        getAllGroups().then(response => setGroups(response.data));
    }, []);

    const fetchUserData = () => {
        setLoading(true);
        getMe().then(response => {
            const data: User = response.data;
            setUser(data);
            setEditedUser({...data});

            const promises = [];

            if (data.roles.includes(UserRole.STUDENT)) {
                promises.push(getStudentByUserId(data.userId).then(res => {
                    setStudentInfo(res.data);
                    setEditedStudent({...res.data});
                }));
            }
            if (data.roles.includes(UserRole.SUPERVISOR)) {
                promises.push(getLecturerByUserId(data.userId).then(res => {
                    setLecturerInfo(res.data);
                    setEditedLecturer({...res.data});
                }));
            }
            if (data.roles.includes(UserRole.CONSULTANT)) {
                promises.push(getConsultantByUserId(data.userId).then(res => {
                    setConsultantInfo(res.data);
                    setEditedConsultant({...res.data});
                }));
            }

            Promise.allSettled(promises).finally(() => setLoading(false));
        }).catch(() => {
            setLoading(false);
        });
    };

    const handleSaveChanges = async () => {
        try {
            if (editedUser) await updateUser(editedUser);
            if (user?.roles?.includes(UserRole.STUDENT)) await updateStudent(editedStudent);
            if (user?.roles?.includes(UserRole.SUPERVISOR)) await updateLecturer(editedLecturer);
            if (user?.roles?.includes(UserRole.CONSULTANT)) await updateConsultant(editedConsultant);

            fetchUserData();
            setEditDialogOpen(false);
        } catch (error) {
            console.error("Error updating profile:", error);
        }
    };

    const handleCancelEdit = () => {
        setEditDialogOpen(false);
        if (user) setEditedUser({...user});
        if (studentInfo) setEditedStudent({...studentInfo});
        if (lecturerInfo) setEditedLecturer({...lecturerInfo});
        if (consultantInfo) setEditedConsultant({...consultantInfo});
    };

    if (tokenIsEmpty) {
        return <Navigate to="/login" replace />;
    }

    const getGroupName = (group: Group) => {
        return group ? `${group.name} (${group.year} г.)` : 'Не указана';
    };

    const fullName = user ? `${user.lastName} ${user.firstName} ${user.middleName || ''}`.trim() : "";

    return (
        <Layout>
            <Container maxWidth="md" sx={{ mt: 4, mb: 4 }}>
                <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
                    <Button 
                        startIcon={<ArrowBackIcon />} 
                        onClick={() => navigate(-1)}
                        color="inherit"
                    >
                        Назад
                    </Button>
                </Box>

                {loading || !user ? (
                    <Paper elevation={0} sx={{ p: 4, borderRadius: 2 }}>
                        <Box display="flex" flexDirection="column" alignItems="center" gap={2}>
                            <Skeleton variant="circular" width={120} height={120} />
                            <Skeleton variant="text" width={200} height={40} />
                            <Skeleton variant="text" width={150} height={20} />
                            <Skeleton variant="rectangular" width="100%" height={200} sx={{ mt: 2 }} />
                        </Box>
                    </Paper>
                ) : (
                    <Card elevation={2} sx={{ borderRadius: 3, overflow: 'visible' }}>
                        <Box 
                            sx={{ 
                                height: 140, 
                                bgcolor: 'primary.main',
                                background: 'linear-gradient(45deg, #1976d2 30%, #21CBF3 90%)',
                                borderRadius: '12px 12px 0 0'
                            }} 
                        />
                        
                        <CardContent sx={{ position: 'relative', pt: 0, pb: 4 }}>
                            <Box 
                                display="flex" 
                                flexDirection={{ xs: 'column', sm: 'row' }} 
                                alignItems={{ xs: 'center', sm: 'flex-end' }}
                                sx={{ mt: -6, mb: 4, px: 2 }}
                            >
                                <Avatar
                                    sx={{
                                        width: 120,
                                        height: 120,
                                        fontSize: 48,
                                        bgcolor: 'background.paper',
                                        color: 'primary.main',
                                        border: '4px solid white',
                                        boxShadow: 2
                                    }}
                                >
                                    {user.firstName?.[0]}{user.lastName?.[0]}
                                </Avatar>

                                <Box sx={{ ml: { xs: 0, sm: 3 }, mt: { xs: 2, sm: 0 }, textAlign: { xs: 'center', sm: 'left' }, flexGrow: 1 }}>
                                    <Typography variant="h4" fontWeight="bold">
                                        {fullName}
                                    </Typography>
                                </Box>

                                <Box sx={{ mt: { xs: 2, sm: 0 } }}>
                                    <Button 
                                        variant="outlined" 
                                        startIcon={<EditIcon />} 
                                        onClick={() => setEditDialogOpen(true)}
                                        sx={{ borderRadius: 20 }}
                                    >
                                        Редактировать
                                    </Button>
                                </Box>
                            </Box>

                            <Divider sx={{ mb: 4 }} />
                            
                            <Grid container spacing={4} px={2}>
                                <Grid item xs={12} md={6}>
                                    <Typography variant="h6" gutterBottom sx={{ mb: 3 }}>
                                        Личные данные
                                    </Typography>

                                    <ProfileField 
                                        icon={<EmailIcon />} 
                                        label="Email" 
                                        value={user.email || "Не указан"} 
                                    />
                                    
                                    <ProfileField 
                                        icon={<BadgeIcon />} 
                                        label="Роли в системе" 
                                        value={
                                            <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
                                                {user.roles.map(role => (
                                                    <Chip
                                                        key={role}
                                                        label={role}
                                                        size="small"
                                                        color={
                                                            role === UserRole.ADMIN ? 'error' :
                                                            role === UserRole.SUPERVISOR ? 'primary' :
                                                            role === UserRole.STUDENT ? 'success' :
                                                            role === UserRole.CONSULTANT ? 'info' : 'default'
                                                        }
                                                        variant="outlined"
                                                    />
                                                ))}
                                            </Stack>
                                        } 
                                    />
                                </Grid>
                                
                                <Grid item xs={12} md={6}>
                                    <Typography variant="h6" gutterBottom sx={{ mb: 3 }}>
                                        Информация об обучении/работе
                                    </Typography>

                                    {user.roles.includes(UserRole.STUDENT) && studentInfo && (
                                        <ProfileField 
                                            icon={<SchoolIcon />} 
                                            label="Учебная группа" 
                                            value={studentInfo.group ? getGroupName(studentInfo.group) : 'Группа не назначена'} 
                                        />
                                    )}

                                    {user.roles.includes(UserRole.SUPERVISOR) && lecturerInfo && (
                                        <ProfileField 
                                            icon={<WorkIcon />} 
                                            label="Статус преподавателя" 
                                            value={lecturerInfo.cansupervisevkr ? 'Может руководить ВКР' : 'Не руководит ВКР'} 
                                        />
                                    )}

                                    {user.roles.includes(UserRole.CONSULTANT) && consultantInfo && (
                                        <ProfileField 
                                            icon={<ContactPhoneIcon />} 
                                            label="Контактные данные для консультаций" 
                                            value={consultantInfo.contact || 'Не указаны'} 
                                        />
                                    )}

                                    {!user.roles.includes(UserRole.STUDENT) && 
                                     !user.roles.includes(UserRole.SUPERVISOR) && 
                                     !user.roles.includes(UserRole.CONSULTANT) && (
                                        <Typography variant="body2" color="text.secondary">
                                            Нет дополнительной информации для отображения.
                                        </Typography>
                                     )}
                                </Grid>
                            </Grid>
                        </CardContent>
                    </Card>
                )}
            </Container>
            
            <Dialog
                open={editDialogOpen}
                onClose={handleCancelEdit}
                maxWidth="sm"
                fullWidth
                PaperProps={{
                    sx: { borderRadius: 3 }
                }}
            >
                <DialogTitle sx={{ pb: 1, display: 'flex', alignItems: 'center', gap: 1 }}>
                    <EditIcon color="primary" />
                    Редактирование профиля
                </DialogTitle>
                <Divider />
                <DialogContent>
                    <Box component="form" sx={{ mt: 2 }}>
                        <Stack spacing={3}>
                            
                            <Box>
                                <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 2, textTransform: 'uppercase', fontSize: '0.75rem', letterSpacing: 1 }}>
                                    Личные данные
                                </Typography>
                                <Stack spacing={2}>
                                    <TextField
                                        fullWidth
                                        label="Фамилия"
                                        value={editedUser.lastName || ''}
                                        onChange={(e) => setEditedUser({...editedUser, lastName: e.target.value})}
                                        InputProps={{
                                            startAdornment: (
                                                <InputAdornment position="start">
                                                    <PersonIcon color="action" />
                                                </InputAdornment>
                                            ),
                                        }}
                                    />
                                    <TextField
                                        fullWidth
                                        label="Имя"
                                        value={editedUser.firstName || ''}
                                        onChange={(e) => setEditedUser({...editedUser, firstName: e.target.value})}
                                        InputProps={{
                                            startAdornment: (
                                                <InputAdornment position="start">
                                                    <PersonIcon color="action" />
                                                </InputAdornment>
                                            ),
                                        }}
                                    />
                                    <TextField
                                        fullWidth
                                        label="Отчество"
                                        value={editedUser.middleName || ''}
                                        onChange={(e) => setEditedUser({...editedUser, middleName: e.target.value})}
                                        InputProps={{
                                            startAdornment: (
                                                <InputAdornment position="start">
                                                    <PersonIcon color="action" />
                                                </InputAdornment>
                                            ),
                                        }}
                                    />
                                    <TextField
                                        fullWidth
                                        disabled
                                        label="Email"
                                        value={editedUser.email || ''}
                                        helperText="Email изменить нельзя"
                                        InputProps={{
                                            startAdornment: (
                                                <InputAdornment position="start">
                                                    <EmailIcon color="action" />
                                                </InputAdornment>
                                            ),
                                        }}
                                    />
                                </Stack>
                            </Box>
                            
                            {user?.roles.includes(UserRole.STUDENT) && (
                                <Box>
                                    <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 2, textTransform: 'uppercase', fontSize: '0.75rem', letterSpacing: 1 }}>
                                        Студент
                                    </Typography>
                                    <FormControl fullWidth>
                                        <InputLabel id="group-select-label">Учебная группа</InputLabel>
                                        <Select
                                            labelId="group-select-label"
                                            id="group-select"
                                            value={editedStudent?.groupId || ''}
                                            label="Учебная группа"
                                            startAdornment={
                                                <InputAdornment position="start" sx={{ ml: 1 }}>
                                                    <SchoolIcon color="action" />
                                                </InputAdornment>
                                            }
                                            onChange={(e) => setEditedStudent({...editedStudent, groupId: e.target.value as string})}
                                        >
                                            <MenuItem value="">
                                                <em>Не выбрана</em>
                                            </MenuItem>
                                            {groups.map((group) => (
                                                <MenuItem key={group.id} value={group.id}>
                                                    {group.name} ({group.program}, {group.year})
                                                </MenuItem>
                                            ))}
                                        </Select>
                                    </FormControl>
                                </Box>
                            )}
                            
                            {user?.roles.includes(UserRole.CONSULTANT) && (
                                <Box>
                                    <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 2, textTransform: 'uppercase', fontSize: '0.75rem', letterSpacing: 1 }}>
                                        Консультант
                                    </Typography>
                                    <TextField
                                        fullWidth
                                        label="Контактные данные"
                                        value={editedConsultant.contact || ''}
                                        onChange={(e) => setEditedConsultant({...editedConsultant, contact: e.target.value})}
                                        multiline
                                        minRows={3}
                                        placeholder="Например: Telegram @username или почта..."
                                        InputProps={{
                                            startAdornment: (
                                                <InputAdornment position="start" sx={{ mt: 1 }}>
                                                    <ContactPhoneIcon color="action" />
                                                </InputAdornment>
                                            ),
                                            alignItems: 'flex-start'
                                        }}
                                    />
                                </Box>
                            )}
                        </Stack>
                    </Box>
                </DialogContent>
                <Divider />
                <DialogActions sx={{ p: 3, justifyContent: 'space-between' }}>
                    <Button onClick={handleCancelEdit} color="inherit" variant="text">
                        Отмена
                    </Button>
                    <Button
                        onClick={handleSaveChanges}
                        variant="contained"
                        color="primary"
                        disableElevation
                        sx={{ px: 4 }}
                    >
                        Сохранить
                    </Button>
                </DialogActions>
            </Dialog>
        </Layout>
    );
}