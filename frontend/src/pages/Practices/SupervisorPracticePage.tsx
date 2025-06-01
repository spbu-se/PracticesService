import { Layout } from "@shared/ui/layout/Layout.tsx";
import { getJWTToken } from "@/shared/services/localStorage.service.ts";
import { Navigate, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { getSupervisorPractices, getMe } from "@/shared/services/axios.service.ts"; // you need to implement this API call
import { Practice } from "@/entities/Practice.ts";
import { User } from "@/entities/User.ts";
import {
    Container,
    Grid,
    Typography,
    Button,
    Paper,
    Box,
    useMediaQuery,
    useTheme,
    TextField,
    InputAdornment,
} from "@mui/material";
import SearchIcon from "@mui/icons-material/Search";
import { PracticeCard } from "./PracticeCard";
import {UserRole} from "../../entities/UserRoles";

export function SupervisorPracticesPage() {
    const tokenIsEmpty = getJWTToken() === "";
    const [practices, setPractices] = useState<Practice[]>([]);
    const [me, setMe] = useState<User>();
    const [searchTerm, setSearchTerm] = useState("");
    const navigate = useNavigate();

    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("sm"));

    useEffect(() => {
        getMe().then((response) => {
            const data: User = response.data;
            if (!data.roles.includes(UserRole.SUPERVISOR)) {
                navigate("/");    
            }
            
            setMe(data);
            getSupervisorPractices(data.userId).then((response) => {
                setPractices(response.data);
            });
        });
    }, []);

    // Filter practices by search term (e.g. by student name, practice title)
    const filteredPractices = practices.filter(
        (p) =>
            p.theme.title.toLowerCase().includes(searchTerm.toLowerCase())
    );

    return tokenIsEmpty ? (
        <Navigate to="/login" replace />
    ) : (
        <Layout>
            <Container maxWidth="md" sx={{ mt: 2, p: isMobile ? 1 : 4 }}>
                <Typography variant={isMobile ? "h5" : "h4"} align="center" gutterBottom>
                    Практики под вашим руководством
                </Typography>

                <Box sx={{ display: "flex", justifyContent: "center", mb: 2 }}>
                    <TextField
                        variant="outlined"
                        placeholder="Поиск практики или студента"
                        size={isMobile ? "small" : "medium"}
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        InputProps={{
                            startAdornment: (
                                <InputAdornment position="start">
                                    <SearchIcon />
                                </InputAdornment>
                            ),
                        }}
                        sx={{ width: isMobile ? "100%" : 400 }}
                    />
                </Box>

                {filteredPractices.length > 0 ? (
                    <Grid container spacing={isMobile ? 1 : 3}>
                        {filteredPractices.map((practice) => (
                            <Grid item xs={12} sm={6} md={4} key={practice.id}>
                                <PracticeCard
                                    practice={practice}
                                    onClick={() => navigate(`/practice-staff/${practice.id}`)}
                                />
                            </Grid>
                        ))}
                    </Grid>
                ) : (
                    <Typography variant="h6" align="center" sx={{ mt: 4 }}>
                        Нет практик, соответствующих запросу.
                    </Typography>
                )}
            </Container>
        </Layout>
    );
}
