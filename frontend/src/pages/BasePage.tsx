import { Layout } from "@shared/ui/layout/Layout.tsx";
import { getJWTToken } from "../shared/services/localStorage.service.ts";
import { Navigate, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { getThemes } from "../shared/services/axios.service.ts";
import { Theme } from "../entities/Theme.ts";
import { Button, Card, Container, Pagination, Form, Row, Col } from "react-bootstrap";

export function BasePage() {
    const tokenIsEmpty = getJWTToken() == "";
    const [themes, setThemes] = useState<Theme[]>([]);
    const [filteredThemes, setFilteredThemes] = useState<Theme[]>([]);
    const [currentPage, setCurrentPage] = useState(1);
    const itemsPerPage = 5; // Themes per page
    const navigate = useNavigate();

    // Filter state
    const [level, setLevel] = useState<string>("");
    const [department, setDepartment] = useState<string>("");
    const [source, setSource] = useState<string>("");
    const [supervisor, setSupervisor] = useState<string>("");

    useEffect(() => {
        getThemes().then(response => {
            setThemes(response.data);
            setFilteredThemes(response.data);
        });
    }, []);

    // Apply filters whenever a filter changes
    useEffect(() => {
        let filtered = themes;

        if (level) filtered = filtered.filter(theme => theme.level === level);
        if (department) filtered = filtered.filter(theme => theme.department === department);
        if (source) filtered = filtered.filter(theme => theme.suggestedby === source);
        if (supervisor) filtered = filtered.filter(theme => theme.supervisorid.toString() === supervisor);

        setFilteredThemes(filtered);
        setCurrentPage(1);
    }, [level, department, source, supervisor, themes]);

    // Pagination calculations
    const indexOfLastTheme = currentPage * itemsPerPage;
    const indexOfFirstTheme = indexOfLastTheme - itemsPerPage;
    const currentThemes = filteredThemes.slice(indexOfFirstTheme, indexOfLastTheme);

    const paginate = (pageNumber: number) => setCurrentPage(pageNumber);

    return tokenIsEmpty ? <Navigate to="/login" replace /> : (
        <Layout>
            <Container className="mt-4 p-4">
                <h1 className="text-center mb-4">Список тем</h1>

                <Row>
                    <Col md={3} className="mb-4">
                        <h5>Фильтры</h5>
                        <Form>
                            <Form.Group controlId="levelFilter" className="mb-3">
                                <Form.Label>Уровень</Form.Label>
                                <Form.Control as="select" value={level} onChange={(e) => setLevel(e.target.value)}>
                                    <option value="">Все</option>
                                    <>
                                        {[...new Set(themes.map(t => t.level))].map((lvl, i) => (
                                            <option key={i} value={lvl}>{lvl}</option>
                                        ))}
                                    </>
                                </Form.Control>
                            </Form.Group>

                            <Form.Group controlId="departmentFilter" className="mb-3">
                                <Form.Label>Кафедра</Form.Label>
                                <Form.Control as="select" value={department} onChange={(e) => setDepartment(e.target.value)}>
                                    <option value="">Все</option>
                                    <>
                                        {[...new Set(themes.map(t => t.department))].map((dep, i) => (
                                            <option key={i} value={dep}>{dep}</option>
                                        ))}
                                    </>
                                </Form.Control>
                            </Form.Group>

                            <Form.Group controlId="sourceFilter" className="mb-3">
                                <Form.Label>Источник</Form.Label>
                                <Form.Control as="select" value={source} onChange={(e) => setSource(e.target.value)}>
                                    <option value="">Все</option>
                                    <>
                                        {[...new Set(themes.map(t => t.suggestedby))].map((src, i) => (
                                            <option key={i} value={src}>{src}</option>
                                        ))}
                                    </>
                                </Form.Control>
                            </Form.Group>

                            <Form.Group controlId="supervisorFilter" className="mb-3">
                                <Form.Label>Руководитель</Form.Label>
                                <Form.Control as="select" value={supervisor} onChange={(e) => setSupervisor(e.target.value)}>
                                    <option value="">Все</option>
                                    <>
                                        {[...new Set(themes.map(t => t.supervisorid.toString()))].map((sup, i) => (
                                            <option key={i} value={sup}>{sup}</option>
                                        ))}
                                    </>
                                </Form.Control>
                            </Form.Group>

                            <Button variant="secondary" onClick={() => {
                                setLevel("");
                                setDepartment("");
                                setSource("");
                                setSupervisor("");
                            }}>
                                Сбросить фильтры
                            </Button>
                        </Form>
                    </Col>

                    <Col md={9}>
                        <div className="d-flex flex-column gap-3">
                            {currentThemes.length > 0 ? currentThemes.map((theme, index) => (
                                <Card key={index} className="shadow-sm" onClick={() => navigate(`/theme/${theme.id}`)} style={{ cursor: "pointer" }}>
                                    <Card.Header as="h5">{theme.title}</Card.Header>
                                    <Card.Body>
                                        <Card.Title>Уровень темы: {theme.level}</Card.Title>
                                        <Card.Text>Кафедра: {theme.department}</Card.Text>
                                        <Card.Text>Источник: {theme.suggestedby}</Card.Text>
                                        <Card.Text>Научный руководитель: {theme.supervisorid}</Card.Text>
                                        <Card.Text>Консультант: {theme.consultantid}</Card.Text>
                                    </Card.Body>
                                </Card>
                            )) : (
                                <p className="text-center">Нет доступных тем</p>
                            )}
                        </div>

                        <>
                            {filteredThemes.length > itemsPerPage && (
                                <Pagination className="mt-4 justify-content-center">
                                    <Pagination.Prev onClick={() => paginate(currentPage - 1)} disabled={currentPage === 1} />
                                    <>
                                        {Array.from({ length: Math.ceil(filteredThemes.length / itemsPerPage) }, (_, i) => (
                                            <Pagination.Item key={i + 1} active={i + 1 === currentPage} onClick={() => paginate(i + 1)}>
                                                {i + 1}
                                            </Pagination.Item>
                                        ))}
                                    </>
                                    <Pagination.Next onClick={() => paginate(currentPage + 1)} disabled={currentPage === Math.ceil(filteredThemes.length / itemsPerPage)} />
                                </Pagination>
                            )}
                        </>
                    </Col>
                </Row>
            </Container>
        </Layout>
    );
}
