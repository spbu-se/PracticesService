import { Layout } from "@shared/ui/layout/Layout.tsx";
import { getJWTToken } from "../shared/services/localStorage.service.ts";
import { Navigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { getThemes } from "../shared/services/axios.service.ts";
import { Theme } from "../entities/Theme.ts";
import { Button, Card, Container } from "react-bootstrap";

export function BasePage() {
    const tokenIsEmpty = getJWTToken() == "";
    const [themes, setThemes] = useState<Theme[]>([]);

    useEffect(() => {
        getThemes().then(response => {
            setThemes(response.data);
        });
    }, []);

    return (
        tokenIsEmpty ? <Navigate to="/login" replace /> :
            <Layout>
                <Container className="mt-4 p-4">
                    <h1 className="text-center mb-4">Список тем</h1>

                    <div className="d-flex flex-column gap-3">
                        {themes.map((theme, index) => (
                            <Card key={index} className="shadow-sm">
                                <Card.Header as="h5">{theme.title}</Card.Header>
                                <Card.Body>
                                    <Card.Title>Уровень темы: {theme.level}</Card.Title>
                                    <Card.Text>Кафедра: {theme.department}</Card.Text>
                                    <Card.Text>Предложена: {theme.suggestedby}</Card.Text>
                                    <Card.Text>Научный руководитель: {theme.supervisorid}</Card.Text>
                                    <Card.Text>Консультант: {theme.consultantid}</Card.Text>
                                    <Card.Text>Описание: {theme.description}</Card.Text>
                                </Card.Body>
                            </Card>
                        ))}
                    </div>
                </Container>
            </Layout>
    );
}
