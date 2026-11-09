import { ReactNode, useEffect, useRef, useState } from "react";
import { Navigate } from "react-router-dom";
import {
    Alert,
    Box,
    Button,
    CircularProgress,
    Container,
    LinearProgress,
    Paper,
    Skeleton,
    Snackbar,
    Stack,
    Typography,
} from "@mui/material";
import SpellcheckIcon from "@mui/icons-material/Spellcheck";
import AutoAwesomeIcon from "@mui/icons-material/AutoAwesome";
import DescriptionOutlinedIcon from "@mui/icons-material/DescriptionOutlined";
import SlideshowIcon from "@mui/icons-material/Slideshow";
import RateReviewOutlinedIcon from "@mui/icons-material/RateReviewOutlined";
import BorderColorIcon from "@mui/icons-material/BorderColor";
import SummarizeOutlinedIcon from "@mui/icons-material/SummarizeOutlined";
import LockOutlinedIcon from "@mui/icons-material/LockOutlined";
import PlayArrowIcon from "@mui/icons-material/PlayArrow";
import ReplayIcon from "@mui/icons-material/Replay";
import { Layout } from "@shared/ui/layout/Layout.tsx";
import { getJWTToken } from "@/shared/services/localStorage.service.ts";
import { analyzeDocument, getLlmAgents } from "@/shared/services/axios.service.ts";
import { Agent, DocumentType, OutputMode } from "@/entities/DocumentCheck";
import { OptionCard } from "./components/OptionCard";
import { FileDropzone } from "./components/FileDropzone";
import { PdfResult } from "./components/ResultView";

const documentTypes: { value: DocumentType; title: string; description: string; icon: ReactNode }[] = [
    { value: "report", title: "Отчёт", description: "Отчёт студента о практике", icon: <DescriptionOutlinedIcon /> },
    { value: "presentation", title: "Презентация", description: "Слайды, выгруженные в PDF", icon: <SlideshowIcon /> },
    { value: "feedback", title: "Отзыв", description: "Отзыв руководителя или консультанта", icon: <RateReviewOutlinedIcon /> },
];

const outputModes: { value: OutputMode; title: string; description: string; icon: ReactNode }[] = [
    { value: "annotate", title: "Пометки в документе", description: "Ваш PDF с выделенными ошибками и комментариями к ним", icon: <BorderColorIcon /> },
    { value: "report", title: "Отчёт в PDF", description: "Отдельный документ со сводкой проверки", icon: <SummarizeOutlinedIcon /> },
];

interface Result {
    blob: Blob;
    fileName: string;
    annotated: boolean;
}

function agentIcon(name: string) {
    return name.includes("spell") ? <SpellcheckIcon /> : <AutoAwesomeIcon />;
}

/// Error responses of blob requests are blobs too, so the message has to be read from the body.
async function readError(error: any): Promise<string> {
    const data = error?.response?.data;
    try {
        const body = data instanceof Blob ? JSON.parse(await data.text()) : data;
        if (body?.error) return body.error;
        if (body?.detail) return `Сервис проверки недоступен: ${body.detail}`;
    } catch {
        // fall through to the generic message
    }
    return error?.response?.status === 502
        ? "Языковая модель сейчас недоступна. Попробуйте позже."
        : "Не удалось проверить документ. Попробуйте ещё раз.";
}

function Section({ step, title, hint, children }: { step: number; title: string; hint?: string; children: ReactNode }) {
    return (
        <Box component="section" sx={{ mb: 4 }}>
            <Stack direction="row" spacing={1.5} alignItems="center" sx={{ mb: 0.5 }}>
                <Box
                    sx={{
                        width: 28,
                        height: 28,
                        borderRadius: "50%",
                        bgcolor: "primary.main",
                        color: "primary.contrastText",
                        display: "grid",
                        placeItems: "center",
                        fontWeight: 700,
                        fontSize: 14,
                    }}
                >
                    {step}
                </Box>
                <Typography variant="h6" fontWeight={600}>{title}</Typography>
            </Stack>
            {hint && <Typography variant="body2" color="text.secondary" sx={{ mb: 1.5, ml: 5.5 }}>{hint}</Typography>}
            <Box sx={{ mt: hint ? 0 : 1.5 }}>{children}</Box>
        </Box>
    );
}

const cardGrid = { display: "grid", gap: 2, gridTemplateColumns: { xs: "1fr", md: "repeat(3, 1fr)" } };

export function DocumentCheckPage() {
    const [agents, setAgents] = useState<Agent[] | null>(null);
    const [agentsError, setAgentsError] = useState(false);
    const [file, setFile] = useState<File | null>(null);
    const [documentType, setDocumentType] = useState<DocumentType>("report");
    const [selectedAgents, setSelectedAgents] = useState<string[]>([]);
    const [outputMode, setOutputMode] = useState<OutputMode>("annotate");
    const [loading, setLoading] = useState(false);
    const [elapsed, setElapsed] = useState(0);
    const [result, setResult] = useState<Result | null>(null);
    const [error, setError] = useState<string | null>(null);
    const resultRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        getLlmAgents()
            .then((res) => {
                setAgents(res.data);
                // The first agent is preselected so that the common case needs no extra click.
                setSelectedAgents(res.data.slice(0, 1).map((a) => a.name));
            })
            .catch(() => setAgentsError(true));
    }, []);

    useEffect(() => {
        if (!loading) return;
        setElapsed(0);
        const timer = setInterval(() => setElapsed((s) => s + 1), 1000);
        return () => clearInterval(timer);
    }, [loading]);

    useEffect(() => {
        if (result) resultRef.current?.scrollIntoView({ behavior: "smooth", block: "start" });
    }, [result]);

    if (getJWTToken() === "") {
        return <Navigate to="/login" />;
    }

    const toggleAgent = (name: string) =>
        setSelectedAgents((current) => (current.includes(name) ? current.filter((n) => n !== name) : [...current, name]));

    const canSubmit = !!file && selectedAgents.length > 0 && !loading;

    const handleSubmit = async () => {
        if (!file) return;
        setLoading(true);
        setResult(null);
        try {
            const res = await analyzeDocument({ file, documentType, agents: selectedAgents, outputMode });
            const base = file.name.replace(/\.pdf$/i, "");
            setResult({
                blob: res.data,
                fileName: outputMode === "annotate" ? `${base} — проверка.pdf` : `${base} — отчёт о проверке.pdf`,
                annotated: outputMode === "annotate",
            });
        } catch (e) {
            setError(await readError(e));
        } finally {
            setLoading(false);
        }
    };

    const reset = () => {
        setResult(null);
        setFile(null);
        window.scrollTo({ top: 0, behavior: "smooth" });
    };

    return (
        <Layout>
            <>
            <Container maxWidth="md" sx={{ py: 4 }}>
                <Typography variant="h4" fontWeight={700} gutterBottom>Проверка документов</Typography>
                <Typography color="text.secondary" sx={{ mb: 2 }}>
                    Загрузите отчёт, презентацию или отзыв, выберите проверяющих агентов — и получите список замечаний
                    или свой документ с пометками.
                </Typography>
                <Alert icon={<LockOutlinedIcon />} severity="info" variant="outlined" sx={{ mb: 4, borderRadius: 3 }}>
                    Файл не сохраняется на сервере. Перед отправкой языковой модели из текста скрываются e-mail,
                    телефоны, ссылки и ФИО.
                </Alert>

                <Section step={1} title="Документ" hint="PDF с текстовым слоем: сканы и фотографии страниц не поддерживаются.">
                    <FileDropzone file={file} onChange={setFile} onReject={setError} disabled={loading} />
                </Section>

                <Section step={2} title="Тип документа" hint="От типа зависит контекст, с которым агент читает текст.">
                    <Box sx={cardGrid}>
                        {documentTypes.map((t) => (
                            <OptionCard
                                key={t.value}
                                control="radio"
                                selected={documentType === t.value}
                                onClick={() => setDocumentType(t.value)}
                                icon={t.icon}
                                title={t.title}
                                description={t.description}
                                disabled={loading}
                            />
                        ))}
                    </Box>
                </Section>

                <Section step={3} title="Агенты" hint="Можно выбрать несколько — они отработают по очереди.">
                    {agentsError ? (
                        <Alert severity="error" sx={{ borderRadius: 3 }}>
                            Не удалось загрузить список агентов. Обновите страницу или попробуйте позже.
                        </Alert>
                    ) : !agents ? (
                        <Box sx={cardGrid}>
                            {[0, 1, 2].map((i) => <Skeleton key={i} variant="rounded" height={88} sx={{ borderRadius: 3 }} />)}
                        </Box>
                    ) : agents.length === 0 ? (
                        <Alert severity="warning" sx={{ borderRadius: 3 }}>Нет доступных агентов.</Alert>
                    ) : (
                        <Box sx={cardGrid}>
                            {agents.map((a) => (
                                <OptionCard
                                    key={a.name}
                                    control="checkbox"
                                    selected={selectedAgents.includes(a.name)}
                                    onClick={() => toggleAgent(a.name)}
                                    icon={agentIcon(a.name)}
                                    title={a.title}
                                    description={a.description}
                                    disabled={loading}
                                />
                            ))}
                        </Box>
                    )}
                </Section>

                <Section step={4} title="Результат">
                    <Box sx={{ ...cardGrid, gridTemplateColumns: { xs: "1fr", md: "repeat(2, 1fr)" } }}>
                        {outputModes.map((m) => (
                            <OptionCard
                                key={m.value}
                                control="radio"
                                selected={outputMode === m.value}
                                onClick={() => setOutputMode(m.value)}
                                icon={m.icon}
                                title={m.title}
                                description={m.description}
                                disabled={loading}
                            />
                        ))}
                    </Box>
                </Section>

                <Paper
                    variant="outlined"
                    sx={{
                        p: 2.5,
                        borderRadius: 3,
                        position: "sticky",
                        bottom: 16,
                        zIndex: 1,
                        boxShadow: 3,
                        bgcolor: "background.paper",
                    }}
                >
                    {loading ? (
                        <Box>
                            <Stack direction="row" spacing={1.5} alignItems="center" sx={{ mb: 1.5 }}>
                                <CircularProgress size={20} />
                                <Typography fontWeight={600}>Проверяем документ… {elapsed} с</Typography>
                            </Stack>
                            <LinearProgress sx={{ borderRadius: 1 }} />
                            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                                Большой документ и несколько агентов могут занять несколько минут. Не закрывайте страницу.
                            </Typography>
                        </Box>
                    ) : (
                        <Stack direction={{ xs: "column", sm: "row" }} spacing={2} alignItems={{ sm: "center" }} justifyContent="space-between">
                            <Typography variant="body2" color="text.secondary">
                                {!file
                                    ? "Выберите файл, чтобы начать"
                                    : selectedAgents.length === 0
                                        ? "Выберите хотя бы одного агента"
                                        : `${file.name} · агентов: ${selectedAgents.length}`}
                            </Typography>
                            <Button
                                variant="contained"
                                size="large"
                                startIcon={<PlayArrowIcon />}
                                disabled={!canSubmit}
                                onClick={handleSubmit}
                                sx={{ borderRadius: 2, px: 4, textTransform: "none", fontWeight: 600 }}
                            >
                                Проверить документ
                            </Button>
                        </Stack>
                    )}
                </Paper>

                {result && (
                    <Box ref={resultRef} sx={{ mt: 5, scrollMarginTop: 16 }}>
                        <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
                            <Typography variant="h5" fontWeight={700}>Результат проверки</Typography>
                            <Button startIcon={<ReplayIcon />} onClick={reset} sx={{ textTransform: "none" }}>
                                Проверить другой документ
                            </Button>
                        </Stack>
                        <PdfResult blob={result.blob} fileName={result.fileName} annotated={result.annotated} />
                    </Box>
                )}
            </Container>

            <Snackbar open={!!error} autoHideDuration={8000} onClose={() => setError(null)} anchorOrigin={{ vertical: "bottom", horizontal: "center" }}>
                <Alert severity="error" variant="filled" onClose={() => setError(null)}>{error}</Alert>
            </Snackbar>
            </>
        </Layout>
    );
}
