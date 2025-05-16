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
    getAllConsultants,
    createConsultant,
    updateConsultant,
    deleteConsultant
} from "@/shared/services/axios.service";
import { Consultant } from "@/entities/Consultant";
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import AddIcon from '@mui/icons-material/Add';

export function AdminConsultantsPage() {
    const [consultants, setConsultants] = useState<Consultant[]>([]);
    const [loading, setLoading] = useState(true);
    const [openDialog, setOpenDialog] = useState(false);
    const [currentConsultant, setCurrentConsultant] = useState<Consultant | null>(null);
    const [snackbar, setSnackbar] = useState({
        open: false,
        message: '',
        severity: 'success'
    });

    useEffect(() => {
        loadConsultants();
    }, []);

    const loadConsultants = () => {
        setLoading(true);
        getAllConsultants()
            .then(res => {
                const response = res?.data;
                setConsultants(Array.isArray(response) ? response : []);
            })
            .finally(() => setLoading(false));
    };

    const handleOpenCreate = () => {
        setCurrentConsultant({
            id: 0,
            firstName: '',
            lastName: '',
            middleName: '',
            contact: ''
        });
        setOpenDialog(true);
    };

    const handleOpenEdit = (consultant: Consultant) => {
        setCurrentConsultant({...consultant});
        setOpenDialog(true);
    };

    const handleCloseDialog = () => {
        setOpenDialog(false);
        setCurrentConsultant(null);
    };

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setCurrentConsultant(prev => ({
            ...prev!,
            [name]: value
        }));
    };

    const handleSubmit = () => {
        if (!currentConsultant) return;

        const operation = currentConsultant.id ? updateConsultant : createConsultant;

        operation(currentConsultant)
            .then(() => {
                loadConsultants();
                setSnackbar({
                    open: true,
                    message: currentConsultant.id ? 'Консультант обновлен' : 'Консультант создан',
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
        if (window.confirm('Вы уверены, что хотите удалить этого консультанта?')) {
            deleteConsultant(id)
                .then(() => {
                    loadConsultants();
                    setSnackbar({
                        open: true,
                        message: 'Консультант удален',
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
                Управление консультантами
            </Typography>

            <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={handleOpenCreate}
                sx={{ mb: 2 }}
            >
                Добавить консультанта
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
                                <TableCell>Контакт</TableCell>
                                <TableCell>Действия</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {consultants?.map((consultant) => (
                                <TableRow key={consultant.id}>
                                    <TableCell>{consultant.id}</TableCell>
                                    <TableCell>{consultant.lastName}</TableCell>
                                    <TableCell>{consultant.firstName}</TableCell>
                                    <TableCell>{consultant.middleName || '-'}</TableCell>
                                    <TableCell>{consultant.contact}</TableCell>
                                    <TableCell>
                                        <IconButton
                                            onClick={() => handleOpenEdit(consultant)}
                                            color="primary"
                                        >
                                            <EditIcon />
                                        </IconButton>
                                        <IconButton
                                            onClick={() => handleDelete(consultant.id)}
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
                    {currentConsultant?.id ? 'Редактирование консультанта' : 'Создание консультанта'}
                </DialogTitle>
                <DialogContent>
                    <TextField
                        margin="dense"
                        name="firstName"
                        label="Имя"
                        fullWidth
                        value={currentConsultant?.firstName || ''}
                        onChange={handleInputChange}
                        required
                        sx={{ mt: 2 }}
                    />
                    <TextField
                        margin="dense"
                        name="lastName"
                        label="Фамилия"
                        fullWidth
                        value={currentConsultant?.lastName || ''}
                        onChange={handleInputChange}
                        required
                    />
                    <TextField
                        margin="dense"
                        name="middleName"
                        label="Отчество"
                        fullWidth
                        value={currentConsultant?.middleName || ''}
                        onChange={handleInputChange}
                    />
                    <TextField
                        margin="dense"
                        name="contact"
                        label="Контактная информация"
                        fullWidth
                        value={currentConsultant?.contact || ''}
                        onChange={handleInputChange}
                        required
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleCloseDialog}>Отмена</Button>
                    <Button
                        onClick={handleSubmit}
                        variant="contained"
                        disabled={
                            !currentConsultant?.firstName ||
                            !currentConsultant?.lastName ||
                            !currentConsultant?.contact
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