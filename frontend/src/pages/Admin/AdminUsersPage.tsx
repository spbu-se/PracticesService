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
    MenuItem,
    IconButton,
    Snackbar,
    Alert,
    Chip,
    Box
} from "@mui/material";
import {
    getAllUsers,
    createUser,
    updateUser,
    deleteUser
} from "@/shared/services/axios.service";
import { User } from "@/entities/User";
import { UserRole } from "@/entities/UserRoles";
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import AddIcon from '@mui/icons-material/Add';

export function AdminUsersPage() {
    const [users, setUsers] = useState<User[]>([]);
    const [loading, setLoading] = useState(true);
    const [openDialog, setOpenDialog] = useState(false);
    const [currentUser, setCurrentUser] = useState<User | null>(null);
    const [snackbar, setSnackbar] = useState({
        open: false,
        message: '',
        severity: 'success'
    });

    // Получаем все возможные роли из enum
    const allRoles = Object.values(UserRole);

    useEffect(() => {
        loadUsers();
    }, []);

    const loadUsers = () => {
        setLoading(true);
        getAllUsers()
            .then(res => {
                setUsers(res.data);
            })
            .finally(() => setLoading(false));
    };

    const handleOpenCreate = () => {
        setCurrentUser({
            userId: '',
            firstName: '',
            lastName: '',
            middleName: '',
            userName: '',
            email: '',
            roles: [],
            password: ''
        });
        setOpenDialog(true);
    };

    const handleOpenEdit = (user: User) => {
        setCurrentUser({...user});
        setOpenDialog(true);
    };

    const handleCloseDialog = () => {
        setOpenDialog(false);
        setCurrentUser(null);
    };

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setCurrentUser(prev => ({
            ...prev!,
            [name]: value
        }));
    };

    const handleRolesChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { value } = e.target;
        setCurrentUser(prev => ({
            ...prev!,
            roles: typeof value === 'string' ? value.split(',') : value
        }));
    };

    const handleSubmit = () => {
        if (!currentUser) return;

        const operation = currentUser.userId ? updateUser : createUser;

        operation(currentUser)
            .then(() => {
                loadUsers();
                setSnackbar({
                    open: true,
                    message: currentUser.userId ? 'Пользователь обновлен' : 'Пользователь создан',
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

    const handleDelete = (userId: string) => {
        if (window.confirm('Вы уверены, что хотите удалить этого пользователя?')) {
            deleteUser(userId)
                .then(() => {
                    loadUsers();
                    setSnackbar({
                        open: true,
                        message: 'Пользователь удален',
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
        setSnackbar(prev => ({...prev, open: false}));
    };

    // Функция для отображения ролей в виде чипов
    const renderRoles = (roles: string[]) => (
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
            {roles.map(role => (
                <Chip
                    key={role}
                    label={role}
                    size="small"
                    color={
                        role === UserRole.ADMIN ? 'error' :
                            role === UserRole.SUPERVISOR ? 'primary' :
                                role === UserRole.PRACTICE_SUPERVISOR ? 'secondary' :
                                    'default'
                    }
                />
            ))}
        </Box>
    );

    return (
        <Container>
            <Typography variant="h4" gutterBottom>
                Управление пользователями
            </Typography>

            <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={handleOpenCreate}
                sx={{ mb: 2 }}
            >
                Добавить пользователя
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
                                <TableCell>Email</TableCell>
                                <TableCell>Роли</TableCell>
                                <TableCell>Действия</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {users.map((user) => (
                                <TableRow key={user.userId}>
                                    <TableCell>{user.userId}</TableCell>
                                    <TableCell>{user.lastName}</TableCell>
                                    <TableCell>{user.firstName}</TableCell>
                                    <TableCell>{user.middleName || '-'}</TableCell>
                                    <TableCell>{user.email}</TableCell>
                                    <TableCell>
                                        {renderRoles(user.roles)}
                                    </TableCell>
                                    <TableCell>
                                        <IconButton
                                            onClick={() => handleOpenEdit(user)}
                                            color="primary"
                                        >
                                            <EditIcon />
                                        </IconButton>
                                        <IconButton
                                            onClick={() => handleDelete(user.userId)}
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
                    {currentUser?.userId ? 'Редактирование пользователя' : 'Создание пользователя'}
                </DialogTitle>
                <DialogContent>
                    <TextField
                        margin="dense"
                        name="firstName"
                        label="Имя"
                        fullWidth
                        value={currentUser?.firstName || ''}
                        onChange={handleInputChange}
                        required
                        sx={{ mt: 2 }}
                    />
                    <TextField
                        margin="dense"
                        name="lastName"
                        label="Фамилия"
                        fullWidth
                        value={currentUser?.lastName || ''}
                        onChange={handleInputChange}
                        required
                    />
                    <TextField
                        margin="dense"
                        name="middleName"
                        label="Отчество"
                        fullWidth
                        value={currentUser?.middleName || ''}
                        onChange={handleInputChange}
                    />
                    <TextField
                        margin="dense"
                        name="email"
                        label="Почта"
                        fullWidth
                        value={currentUser?.email || ''}
                        onChange={handleInputChange}
                        required
                    />
                    {!currentUser?.userId && (
                        <TextField
                            margin="dense"
                            name="password"
                            label="Пароль"
                            type="password"
                            fullWidth
                            value={currentUser?.password || ''}
                            onChange={handleInputChange}
                            required
                        />
                    )}
                    <TextField
                        select
                        margin="dense"
                        name="roles"
                        label="Роли"
                        fullWidth
                        SelectProps={{
                            multiple: true,
                            value: currentUser?.roles || [],
                            onChange: handleRolesChange,
                            renderValue: (selected) => (
                                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                                    {(selected as string[]).map((value) => (
                                        <Chip key={value} label={value} size="small" />
                                    ))}
                                </Box>
                            )
                        }}
                        sx={{ mt: 2 }}
                    >
                        {allRoles.map((role) => (
                            <MenuItem key={role} value={role}>
                                {role}
                            </MenuItem>
                        ))}
                    </TextField>
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleCloseDialog}>Отмена</Button>
                    <Button
                        onClick={handleSubmit}
                        variant="contained"
                        disabled={
                            !currentUser?.firstName ||
                            !currentUser?.lastName ||
                            !currentUser?.email ||
                            (!currentUser?.userId && !currentUser?.password)
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