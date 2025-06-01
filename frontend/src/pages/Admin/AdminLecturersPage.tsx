import React, { useEffect, useMemo, useState } from "react";
import {
    Container,
    Typography,
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    CircularProgress,
    Button,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    TextField,
    IconButton,
    Snackbar,
    Alert,
    Checkbox,
    FormControlLabel,
    InputLabel,
    Select,
    MenuItem,
    FormControl
} from "@mui/material";
import {
    getAllLecturers,
    createLecturer,
    updateLecturer,
    deleteLecturer,
    getAllUsers
} from "@/shared/services/axios.service";
import { Lecturer } from "@/entities/Lecturer";
import { User } from "@/entities/User";
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import AddIcon from '@mui/icons-material/Add';

export function AdminLecturersPage() {
    const [lecturers, setLecturers] = useState<Lecturer[]>([]);
    const [users, setUsers] = useState<User[]>([]);
    const [loading, setLoading] = useState(true);
    const [openDialog, setOpenDialog] = useState(false);
    const [currentLecturer, setCurrentLecturer] = useState<Lecturer | null>(null);
    const [snackbar, setSnackbar] = useState({
        open: false,
        message: '',
        severity: 'success'
    });
    const departments = useMemo(() => [
        "Кафедра системного программирования",
        "Кафедра параллельных алгоритмов",
        "Кафедра информатики",
        "Кафедра информационно-аналитических систем"
    ], []);

    useEffect(() => {
        loadLecturers();
        loadUsers();
    }, []);

    const loadLecturers = () => {
        setLoading(true);
        getAllLecturers()
            .then(res => {
                const response = res?.data;
                setLecturers(Array.isArray(response) ? response : []);
            })
            .catch(error => {
                console.error("Error loading lecturers:", error);
                setSnackbar({
                    open: true,
                    message: 'Ошибка при загрузке преподавателей',
                    severity: 'error'
                });
            })
            .finally(() => setLoading(false));
    };

    const loadUsers = () => {
        getAllUsers()
            .then(res => {
                const response = res?.data;
                setUsers(Array.isArray(response) ? response : []);
            })
            .catch(error => {
                console.error("Error loading users:", error);
                setSnackbar({
                    open: true,
                    message: 'Ошибка при загрузке пользователей',
                    severity: 'error'
                });
            });
    };

    const handleOpenCreate = () => {
        setCurrentLecturer({
            id: 0,
            firstName: '',
            lastName: '',
            middleName: '',
            department: '',
            cansupervisevkr: false,
            userid: ''
        });
        setOpenDialog(true);
    };

    const handleOpenEdit = (lecturer: Lecturer) => {
        setCurrentLecturer({ ...lecturer });
        setOpenDialog(true);
    };

    const handleCloseDialog = () => {
        setOpenDialog(false);
        setCurrentLecturer(null);
    };

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | { name?: string; value: unknown }>) => {
        const { name, value, type } = e.target as HTMLInputElement;
        setCurrentLecturer(prev => ({
            ...prev!,
            [name!]: type === 'checkbox' ? (e.target as HTMLInputElement).checked : value
        }));
    };

    const handleUserSelectChange = (e: React.ChangeEvent<{ value: unknown }>) => {
        const selectedUserId = e.target.value as string | '';
        if (selectedUserId === '') {
            setCurrentLecturer(prev => ({
                ...prev!,
                userId: null
            }));
        } else {
            const selectedUser = users.find(u => u.userId === selectedUserId);
            if (selectedUser) {
                setCurrentLecturer(prev => ({
                    ...prev!,
                    userId: selectedUserId,
                    firstName: selectedUser.firstName,
                    lastName: selectedUser.lastName,
                    middleName: selectedUser.middleName || ''
                }));
            }
        }
    };

    const handleSubmit = () => {
        if (!currentLecturer) return;

        const operation = currentLecturer.id ? updateLecturer : createLecturer;

        operation(currentLecturer)
            .then(() => {
                loadLecturers();
                setSnackbar({
                    open: true,
                    message: currentLecturer.id ? 'Преподаватель обновлен' : 'Преподаватель создан',
                    severity: 'success'
                });
                handleCloseDialog();
            })
            .catch(error => {
                setSnackbar({
                    open: true,
                    message: error.response?.data?.message || 'Произошла ошибка',
                    severity: 'error'
                });
            });
    };

    const handleDelete = (id: number) => {
        if (window.confirm('Вы уверены, что хотите удалить этого преподавателя?')) {
            deleteLecturer(id)
                .then(() => {
                    loadLecturers();
                    setSnackbar({
                        open: true,
                        message: 'Преподаватель удален',
                        severity: 'success'
                    });
                })
                .catch(error => {
                    setSnackbar({
                        open: true,
                        message: error.response?.data?.message || 'Произошла ошибка',
                        severity: 'error'
                    });
                });
        }
    };

    const handleCloseSnackbar = () => {
        setSnackbar(prev => ({ ...prev, open: false }));
    };

    return (
        <Container>
            <Typography variant="h4" gutterBottom>
                Управление преподавателями
            </Typography>

            <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={handleOpenCreate}
                sx={{ mb: 2 }}
            >
                Добавить преподавателя
            </Button>

            {loading ? (
                <CircularProgress />
            ) : (
                <TableContainer component={Paper}>
                    <Table>
                        <TableHead>
                            <TableRow>
                                <TableCell>ID</TableCell>
                                <TableCell>Фамилия</TableCell>
                                <TableCell>Имя</TableCell>
                                <TableCell>Отчество</TableCell>
                                <TableCell>Кафедра</TableCell>
                                <TableCell>Может руководить ВКР</TableCell>
                                <TableCell>Действия</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {(lecturers || []).map((lecturer) => (
                                <TableRow key={lecturer.id}>
                                    <TableCell>{lecturer.id}</TableCell>
                                    <TableCell>{lecturer.lastName}</TableCell>
                                    <TableCell>{lecturer.firstName}</TableCell>
                                    <TableCell>{lecturer.middleName || '-'}</TableCell>
                                    <TableCell>{lecturer.department}</TableCell>
                                    <TableCell>{lecturer.cansupervisevkr ? 'Да' : 'Нет'}</TableCell>
                                    <TableCell>
                                        <IconButton
                                            onClick={() => handleOpenEdit(lecturer)}
                                            color="primary"
                                        >
                                            <EditIcon />
                                        </IconButton>
                                        <IconButton
                                            onClick={() => handleDelete(lecturer.id)}
                                            color="error"
                                        >
                                            <DeleteIcon />
                                        </IconButton>
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </TableContainer>
            )}

            <Dialog open={openDialog} onClose={handleCloseDialog} fullWidth maxWidth="sm">
                <DialogTitle>
                    {currentLecturer?.id ? 'Редактирование преподавателя' : 'Создание преподавателя'}
                </DialogTitle>
                <DialogContent>
                    <FormControl fullWidth margin="dense">
                        <InputLabel>Пользователь</InputLabel>
                        <Select
                            value={currentLecturer?.userid ?? ''}
                            onChange={handleUserSelectChange}
                            displayEmpty
                        >
                            <MenuItem value=""><em>Без пользователя</em></MenuItem>
                            {users.map(user => (
                                <MenuItem key={user.userId} value={user.userId}>
                                    {user.lastName} {user.firstName} {user.middleName} ({user.email})
                                </MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                    <TextField
                        margin="dense"
                        name="firstName"
                        label="Имя"
                        fullWidth
                        value={currentLecturer?.firstName || ''}
                        onChange={handleInputChange}
                        required
                        sx={{ mt: 2 }}
                    />
                    <TextField
                        margin="dense"
                        name="lastName"
                        label="Фамилия"
                        fullWidth
                        value={currentLecturer?.lastName || ''}
                        onChange={handleInputChange}
                        required
                    />
                    <TextField
                        margin="dense"
                        name="middleName"
                        label="Отчество"
                        fullWidth
                        value={currentLecturer?.middleName || ''}
                        onChange={handleInputChange}
                    />
                    <FormControl fullWidth margin="dense">
                        <InputLabel>Кафедра</InputLabel>
                        <Select
                            name="department"
                            value={currentLecturer?.department || ''}
                            onChange={handleInputChange}
                            required
                        >
                            {departments.map((dept, i) => (
                                <MenuItem key={i} value={dept}>{dept}</MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                    <FormControlLabel
                        control={
                            <Checkbox
                                name="cansupervisevkr"
                                checked={currentLecturer?.cansupervisevkr || false}
                                onChange={handleInputChange}
                            />
                        }
                        label="Может руководить ВКР"
                        sx={{ mt: 1 }}
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleCloseDialog}>Отмена</Button>
                    <Button
                        onClick={handleSubmit}
                        variant="contained"
                        disabled={
                            !currentLecturer?.firstName ||
                            !currentLecturer?.lastName ||
                            !currentLecturer?.department
                        }
                    >
                        Сохранить
                    </Button>
                </DialogActions>
            </Dialog>

            <Snackbar
                open={snackbar.open}
                autoHideDuration={6000}
                onClose={handleCloseSnackbar}
            >
                <Alert
                    onClose={handleCloseSnackbar}
                    severity={snackbar.severity}
                    sx={{ width: '100%' }}
                >
                    {snackbar.message}
                </Alert>
            </Snackbar>
        </Container>
    );
}
