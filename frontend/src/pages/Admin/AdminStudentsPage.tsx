import React, { useEffect, useState } from "react";
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
    FormControl,
    InputLabel,
    Select,
    MenuItem
} from "@mui/material";
import {
    getAllStudents,
    createStudent,
    updateStudent,
    deleteStudent,
    getAllGroups,
    getAllUsers
} from "@/shared/services/axios.service";
import { Student } from "@/entities/Student";
import { Group } from "@/entities/Group";
import { User } from "@/entities/User";
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import AddIcon from '@mui/icons-material/Add';

export function AdminStudentsPage() {
    const [students, setStudents] = useState<Student[]>([]);
    const [groups, setGroups] = useState<Group[]>([]);
    const [users, setUsers] = useState<User[]>([]);
    const [loading, setLoading] = useState(true);
    const [openDialog, setOpenDialog] = useState(false);
    const [currentStudent, setCurrentStudent] = useState<Student | null>(null);
    const [snackbar, setSnackbar] = useState({
        open: false,
        message: '',
        severity: 'success'
    });

    useEffect(() => {
        loadData();
    }, []);

    const loadData = () => {
        setLoading(true);
        Promise.all([getAllStudents(), getAllGroups(), getAllUsers()])
            .then(([studentsRes, groupsRes, usersRes]) => {
                setStudents(Array.isArray(studentsRes?.data) ? studentsRes.data : []);
                setGroups(Array.isArray(groupsRes?.data) ? groupsRes.data : []);
                setUsers(Array.isArray(usersRes?.data) ? usersRes.data : []);
            })
            .catch(error => {
                console.error("Error loading data:", error);
                setSnackbar({
                    open: true,
                    message: 'Ошибка при загрузке данных',
                    severity: 'error'
                });
            })
            .finally(() => setLoading(false));
    };

    const handleOpenCreate = () => {
        setCurrentStudent({
            id: 0,
            firstName: '',
            lastName: '',
            middleName: '',
            groupId: 0,
            group: {} as Group,
            userid: '' 
        });
        setOpenDialog(true);
    };

    const handleOpenEdit = (student: Student) => {
        setCurrentStudent({ ...student });
        setOpenDialog(true);
    };

    const handleCloseDialog = () => {
        setOpenDialog(false);
        setCurrentStudent(null);
    };

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | { name?: string; value: unknown }>) => {
        const { name, value } = e.target as HTMLInputElement;
        setCurrentStudent(prev => ({
            ...prev!,
            [name!]: value
        }));
    };

    const handleGroupChange = (e: any) => {
        const groupId = e.target.value;
        const selectedGroup = groups.find(g => g.id === groupId) || {} as Group;
        setCurrentStudent(prev => ({
            ...prev!,
            groupId: groupId,
        }));
    };

    const handleUserSelectChange = (e: any) => {
        const selectedUserId = e.target.value as string;
        if (selectedUserId === '' || selectedUserId == null) {
            setCurrentStudent(prev => ({
                ...prev!,
                userid: ''
            }));
        } else {
            const selectedUser = users.find(u => u.userId === selectedUserId);
            if (selectedUser) {
                setCurrentStudent(prev => ({
                    ...prev!,
                    userid: selectedUserId,
                    firstName: selectedUser.firstName,
                    lastName: selectedUser.lastName,
                    middleName: selectedUser.middleName || ''
                }));
            }
        }
    };

    const handleSubmit = () => {
        if (!currentStudent) return;

        const operation = currentStudent.id ? updateStudent : createStudent;

        operation(currentStudent)
            .then(() => {
                loadData();
                setSnackbar({
                    open: true,
                    message: currentStudent.id ? 'Студент обновлен' : 'Студент создан',
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
        if (window.confirm('Вы уверены, что хотите удалить этого студента?')) {
            deleteStudent(id)
                .then(() => {
                    loadData();
                    setSnackbar({
                        open: true,
                        message: 'Студент удален',
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

    const getGroupName = (group: Group) => {
        return group ? `${group.name} (${group.program}, ${group.year} год)` : 'Не указана';
    };

    return (
        <Container>
            <Typography variant="h4" gutterBottom>
                Управление студентами
            </Typography>

            <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={handleOpenCreate}
                sx={{ mb: 2 }}
            >
                Добавить студента
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
                                <TableCell>Группа</TableCell>
                                <TableCell>Пользователь</TableCell>
                                <TableCell>Действия</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {(students || []).map((student) => (
                                <TableRow key={student.id}>
                                    <TableCell>{student.id}</TableCell>
                                    <TableCell>{student.lastName}</TableCell>
                                    <TableCell>{student.firstName}</TableCell>
                                    <TableCell>{student.middleName || '-'}</TableCell>
                                    <TableCell>{getGroupName(student.group)}</TableCell>
                                    <TableCell>{student.userid || 'Не привязан'}</TableCell>
                                    <TableCell>
                                        <IconButton onClick={() => handleOpenEdit(student)} color="primary">
                                            <EditIcon />
                                        </IconButton>
                                        <IconButton onClick={() => handleDelete(student.id)} color="error">
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
                    {currentStudent?.id ? 'Редактирование студента' : 'Создание студента'}
                </DialogTitle>
                <DialogContent>
                    <FormControl fullWidth margin="dense">
                        <InputLabel>Пользователь</InputLabel>
                        <Select
                            value={currentStudent?.userid ?? null}
                            onChange={handleUserSelectChange}
                        >
                            <MenuItem value="">Не выбран</MenuItem>
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
                        value={currentStudent?.firstName || ''}
                        onChange={handleInputChange}
                        required
                        sx={{ mt: 2 }}
                    />
                    <TextField
                        margin="dense"
                        name="lastName"
                        label="Фамилия"
                        fullWidth
                        value={currentStudent?.lastName || ''}
                        onChange={handleInputChange}
                        required
                    />
                    <TextField
                        margin="dense"
                        name="middleName"
                        label="Отчество"
                        fullWidth
                        value={currentStudent?.middleName || ''}
                        onChange={handleInputChange}
                    />
                    <FormControl fullWidth margin="dense">
                        <InputLabel>Группа *</InputLabel>
                        <Select
                            value={currentStudent?.groupId || 0}
                            onChange={handleGroupChange}
                            required
                        >
                            {groups.map((group) => (
                                <MenuItem key={group.id} value={group.id}>
                                    {group.name} ({group.program}, {group.year} год)
                                </MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleCloseDialog}>Отмена</Button>
                    <Button
                        onClick={handleSubmit}
                        variant="contained"
                        disabled={
                            !currentStudent?.firstName ||
                            !currentStudent?.lastName ||
                            !currentStudent?.groupId
                        }
                    >
                        Сохранить
                    </Button>
                </DialogActions>
            </Dialog>

            <Snackbar open={snackbar.open} autoHideDuration={6000} onClose={handleCloseSnackbar}>
                <Alert onClose={handleCloseSnackbar} severity={snackbar.severity} sx={{ width: '100%' }}>
                    {snackbar.message}
                </Alert>
            </Snackbar>
        </Container>
    );
}