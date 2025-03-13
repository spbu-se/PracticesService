import { Layout } from "@shared/ui/layout/Layout.tsx";
import { ChangeEvent, FormEvent, useState } from "react";
import { login } from "@shared/services/axios.service.ts";
import { LoginResponse } from "@entities/LoginResponse.ts";
import { setJWTToken } from "@shared/services/localStorage.service.ts";
import { Form, Button, Container, Row, Col } from "react-bootstrap";

// Page for login
export function LoginPage() {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");

    const onChangeLogin = (event: ChangeEvent<HTMLInputElement>) => {
        setEmail(event.target.value);
    };

    const onChangePassword = (event: ChangeEvent<HTMLInputElement>) => {
        setPassword(event.target.value);
    };

    const onSubmit = (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        login(email, password)
            .then(response => {
                const loginResponse: LoginResponse = response.data;
                setJWTToken(loginResponse.token);
                window.location.assign("/");
            })
            .catch(e => {
                console.log(e);
                alert("Не удалось войти");
            });
    };

    return (
        <Layout>
            <Container className="d-flex justify-content-center align-items-center" style={{ minHeight: "100vh" }}>
                <Row>
                    <Col>
                        <Form onSubmit={onSubmit} className="p-4 border rounded bg-light shadow">
                            <h3 className="text-center mb-4">Вход</h3>

                            <Form.Group className="mb-3">
                                <Form.Label>Логин:</Form.Label>
                                <Form.Control
                                    type="text"
                                    placeholder="Введите email"
                                    value={email}
                                    onChange={onChangeLogin}
                                    required
                                />
                            </Form.Group>

                            <Form.Group className="mb-3">
                                <Form.Label>Пароль:</Form.Label>
                                <Form.Control
                                    type="password"
                                    placeholder="Введите пароль"
                                    value={password}
                                    onChange={onChangePassword}
                                    required
                                />
                            </Form.Group>

                            <Button variant="primary" type="submit" className="w-100">Войти</Button>
                        </Form>
                    </Col>
                </Row>
            </Container>
        </Layout>
    );
}
