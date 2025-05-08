import React, { useEffect, useState } from "react";
import { Container, Typography, Paper, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, CircularProgress } from "@mui/material";
import { getAllUsers } from "@/shared/services/axios.service";
import { User } from "@/entities/User";

export function AdminUsersPage() {
    const [users, setUsers] = useState<User[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        getAllUsers()
            .then(res => {
                setUsers(res.data);
            })
            .finally(() => setLoading(false));
    }, []);

    return (
        <Container>
            <Typography variant="h4" gutterBottom>
                Пользователи
            </Typography>

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
                                <TableCell>Username</TableCell>
                                <TableCell>Email</TableCell>
                                <TableCell>Роли</TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {users.map((user) => (
                                <TableRow key={user.userId}>
                                    <TableCell>{user.userId}</TableCell>
                                    <TableCell>{user.lastName}</TableCell>
                                    <TableCell>{user.firstName}</TableCell>
                                    <TableCell>{user.middleName}</TableCell>
                                    <TableCell>{user.userName}</TableCell>
                                    <TableCell>{user.email}</TableCell>
                                    <TableCell>{user.roles.join(", ")}</TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </TableContainer>
            )}
        </Container>
    );
}
