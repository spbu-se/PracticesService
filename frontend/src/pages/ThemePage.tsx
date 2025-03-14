import { useParams, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { getThemes } from "../shared/services/axios.service.ts";
import { Theme } from "../entities/Theme.ts";
import { Layout } from "@shared/ui/layout/Layout.tsx";
import { Button, Container } from "react-bootstrap";

export function ThemePage() {
    const { id } = useParams();
    const navigate = useNavigate();
    const [theme, setTheme] = useState<Theme | null>(null);

    useEffect(() => {
        getThemes().then(response => {
            const selectedTheme = response.data.find((t: Theme) => t.id.toString() === id);
            setTheme(selectedTheme);
        });
    }, [id]);

    if (!theme) return <p className="text-center mt-5">Загрузка...</p>;

    return (
        <Layout>
            <Container className="mt-4 p-4">
                <Button variant="secondary" onClick={() => navigate(-1)}>← Назад</Button>
                <h1 className="text-center mt-4">{theme.title}</h1>
                <p>Уровень: {theme.level}</p>
                <p>Кафедра: {theme.department}</p>
                <p>Источник: {theme.suggestedby}</p>
                <p>Научный руководитель: {theme.supervisorid}</p>
                <p>Консультант: {theme.consultantid}</p>
                <p>Описание: {theme.description}</p>
            </Container>
        </Layout>
    );
}
