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
    Alert
} from "@mui/material";
import {
    getAllGroups,
    createGroup,
    updateGroup,
    deleteGroup
} from "@/shared/services/axios.service";
import { Group } from "@/entities/Group";
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import AddIcon from '@mui/icons-material/Add';

export function AdminGroupsPage() {
    const [groups, setGroups] = useState<Group[]>([]);
    const [loading, setLoading] = useState(true);
    const [openDialog, setOpenDialog] = useState(false);
    const [currentGroup, setCurrentGroup] = useState<Group | null>(null);
    const [snackbar, setSnackbar] = useState({
        open: false,
        message: '',
        severity: 'success'
    });

    useEffect(() => {
        loadGroups();
    }, []);

    const loadGroups = () => {
        setLoading(true);
        getAllGroups()
            .then(res => {
                const response = res?.data;
                setGroups(Array.isArray(response) ? response : []);
            })
            .catch(error => {
                console.error("Error loading groups:", error);
                setSnackbar({
                    open: true,
                    message: 'Ошибка при загрузке групп',
                    severity: 'error'
                });
            })
            .finally(() => setLoading(false));
    };

    const handleOpenCreate = () => {
        setCurrentGroup({
            id: 0,
            name: '',
            program: '',
            year: new Date().getFullYear()
        });
        setOpenDialog(true);
    };

    const handleOpenEdit = (group: Group) => {
        setCurrentGroup({...group});
        setOpenDialog(true);
    };

    const handleCloseDialog = () => {
        setOpenDialog(false);
        setCurrentGroup(null);
    };

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setCurrentGroup(prev => ({
            ...prev!,
            [name]: name === 'year' ? parseInt(value) || 0 : value
        }));
    };

    const handleSubmit = () => {
        if (!currentGroup) return;

        const operation = currentGroup.id ? updateGroup : createGroup;

        operation(currentGroup)
            .then(() => {
                loadGroups();
                setSnackbar({
                    open: true,
                    message: currentGroup.id ? 'Группа обновлена' : 'Группа создана',
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
        if (window.confirm('Вы уверены, что хотите удалить эту группу?')) {
            deleteGroup(id)
                .then(() => {
                    loadGroups();
                    setSnackbar({
                        open: true,
                        message: 'Группа удалена',
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

    return (
        <Container>
            <Typography variant="h4" gutterBottom>
                Управление группами
            </Typography>

            <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={handleOpenCreate}
                sx={{ mb: 2 }}
            >
                Добавить группу
            </Button>

            {loading ? (
                <CircularProgress />
            ) : (
                <TableContainer component={Paper}>
                    <Table>
                        <TableHead>
                            <TableRow>
                                <TableCell>ID</TableCell>
                                <TableCell>Название</TableCell>
                                <TableCell>Программа</TableCell>
                                <TableCell>Год</TableCell>
                                <TableCell>Действия</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {groups?.map((group) => (
                                <TableRow key={group.id}>
                                    <TableCell>{group.id}</TableCell>
                                    <TableCell>{group.name}</TableCell>
                                    <TableCell>{group.program}</TableCell>
                                    <TableCell>{group.year}</TableCell>
                                    <TableCell>
                                        <IconButton
                                            onClick={() => handleOpenEdit(group)}
                                            color="primary"
                                        >
                                            <EditIcon />
                                        </IconButton>
                                        <IconButton
                                            onClick={() => handleDelete(group.id)}
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
                    {currentGroup?.id ? 'Редактирование группы' : 'Создание группы'}
                </DialogTitle>
                <DialogContent>
                    <TextField
                        margin="dense"
                        name="name"
                        label="Название группы"
                        fullWidth
                        value={currentGroup?.name || ''}
                        onChange={handleInputChange}
                        required
                        sx={{ mt: 2 }}
                    />
                    <TextField
                        margin="dense"
                        name="program"
                        label="Программа обучения"
                        fullWidth
                        value={currentGroup?.program || ''}
                        onChange={handleInputChange}
                        required
                    />
                    <TextField
                        margin="dense"
                        name="year"
                        label="Год поступления"
                        type="number"
                        fullWidth
                        value={currentGroup?.year || ''}
                        onChange={handleInputChange}
                        required
                        inputProps={{ min: 2000, max: 2100 }}
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleCloseDialog}>Отмена</Button>
                    <Button
                        onClick={handleSubmit}
                        variant="contained"
                        disabled={
                            !currentGroup?.name ||
                            !currentGroup?.program ||
                            !currentGroup?.year
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