import { DragEvent, useRef, useState } from "react";
import { Box, IconButton, Paper, Typography } from "@mui/material";
import CloudUploadOutlinedIcon from "@mui/icons-material/CloudUploadOutlined";
import PictureAsPdfIcon from "@mui/icons-material/PictureAsPdf";
import CloseIcon from "@mui/icons-material/Close";

interface FileDropzoneProps {
    file: File | null;
    onChange: (file: File | null) => void;
    onReject: (message: string) => void;
    disabled?: boolean;
}

const MAX_SIZE_MB = 50;

function formatSize(bytes: number) {
    return bytes > 1024 * 1024 ? `${(bytes / 1024 / 1024).toFixed(1)} МБ` : `${Math.ceil(bytes / 1024)} КБ`;
}

/// Drag & drop area for a single PDF file.
export function FileDropzone({ file, onChange, onReject, disabled }: FileDropzoneProps) {
    const inputRef = useRef<HTMLInputElement>(null);
    const [dragging, setDragging] = useState(false);

    const accept = (candidate?: File) => {
        if (!candidate) return;
        if (!candidate.name.toLowerCase().endsWith(".pdf")) {
            onReject("Поддерживаются только файлы в формате PDF.");
            return;
        }
        if (candidate.size > MAX_SIZE_MB * 1024 * 1024) {
            onReject(`Файл больше ${MAX_SIZE_MB} МБ.`);
            return;
        }
        onChange(candidate);
    };

    const handleDrop = (e: DragEvent) => {
        e.preventDefault();
        setDragging(false);
        if (!disabled) accept(e.dataTransfer.files[0]);
    };

    if (file) {
        return (
            <Paper
                variant="outlined"
                sx={{ p: 2, borderRadius: 3, display: "flex", alignItems: "center", gap: 2, borderColor: "primary.main", borderWidth: 2 }}
            >
                <PictureAsPdfIcon color="error" fontSize="large" />
                <Box sx={{ flex: 1, minWidth: 0 }}>
                    <Typography fontWeight={600} noWrap>{file.name}</Typography>
                    <Typography variant="body2" color="text.secondary">{formatSize(file.size)}</Typography>
                </Box>
                <IconButton onClick={() => onChange(null)} disabled={disabled} aria-label="Убрать файл">
                    <CloseIcon />
                </IconButton>
            </Paper>
        );
    }

    return (
        <Paper
            variant="outlined"
            onClick={() => !disabled && inputRef.current?.click()}
            onDragOver={(e) => { e.preventDefault(); setDragging(true); }}
            onDragLeave={() => setDragging(false)}
            onDrop={handleDrop}
            sx={{
                p: 4,
                borderRadius: 3,
                borderStyle: "dashed",
                borderWidth: 2,
                borderColor: dragging ? "primary.main" : "divider",
                bgcolor: dragging ? "action.hover" : "background.paper",
                textAlign: "center",
                cursor: disabled ? "default" : "pointer",
                transition: "all .15s",
                "&:hover": { borderColor: "primary.main", bgcolor: "action.hover" },
            }}
        >
            <input
                ref={inputRef}
                type="file"
                accept="application/pdf,.pdf"
                hidden
                onChange={(e) => {
                    accept(e.target.files?.[0]);
                    e.target.value = "";
                }}
            />
            <CloudUploadOutlinedIcon sx={{ fontSize: 48, color: "primary.main" }} />
            <Typography variant="h6" sx={{ mt: 1 }}>Перетащите PDF сюда или нажмите, чтобы выбрать</Typography>
            <Typography variant="body2" color="text.secondary">
                Текст в файле должен быть выделяемым (не скан). До {MAX_SIZE_MB} МБ.
            </Typography>
        </Paper>
    );
}
