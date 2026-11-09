import { useEffect, useMemo } from "react";
import { Alert, Button, Paper, Stack } from "@mui/material";
import DownloadIcon from "@mui/icons-material/Download";

interface PdfResultProps {
    blob: Blob;
    fileName: string;
    annotated: boolean;
}

export function PdfResult({ blob, fileName, annotated }: PdfResultProps) {
    const url = useMemo(() => URL.createObjectURL(blob), [blob]);
    useEffect(() => () => URL.revokeObjectURL(url), [url]);

    return (
        <Stack spacing={2}>
            <Alert
                severity="success"
                action={
                    <Button color="inherit" size="small" startIcon={<DownloadIcon />} component="a" href={url} download={fileName}>
                        Скачать
                    </Button>
                }
            >
                {annotated
                    ? "Готово. Найденные места выделены в документе, комментарий виден при наведении или клике на выделение."
                    : "Готово. Отчёт о проверке сформирован."}
            </Alert>
            <Paper variant="outlined" sx={{ borderRadius: 3, overflow: "hidden" }}>
                <iframe title="Результат проверки" src={url} style={{ width: "100%", height: "75vh", border: 0 }} />
            </Paper>
        </Stack>
    );
}
