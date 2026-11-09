import { ReactNode, useCallback, useMemo, useRef, useState } from "react";
import {
    Alert,
    AlertColor,
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
    Snackbar,
} from "@mui/material";
import { ConfirmOptions, Notifier, NotificationContext } from "./notificationContext";

interface Toast {
    id: number;
    message: string;
    severity: AlertColor;
}

/// One place for messages and confirmations instead of alert() and window.confirm().
export function NotificationProvider({ children }: { children: ReactNode }) {
    const [toast, setToast] = useState<Toast | null>(null);
    const [open, setOpen] = useState(false);
    const [dialog, setDialog] = useState<ConfirmOptions | null>(null);
    const resolver = useRef<((confirmed: boolean) => void) | null>(null);
    const counter = useRef(0);

    const notify = useCallback((message: string, severity: AlertColor = "info") => {
        counter.current += 1;
        setToast({ id: counter.current, message, severity });
        setOpen(true);
    }, []);

    const confirm = useCallback(
        (options: ConfirmOptions) =>
            new Promise<boolean>((resolve) => {
                resolver.current = resolve;
                setDialog(options);
            }),
        [],
    );

    const answer = (confirmed: boolean) => {
        resolver.current?.(confirmed);
        resolver.current = null;
        setDialog(null);
    };

    const value = useMemo<Notifier>(
        () => ({
            notify,
            success: (message) => notify(message, "success"),
            error: (message) => notify(message, "error"),
            confirm,
        }),
        [notify, confirm],
    );

    return (
        <NotificationContext.Provider value={value}>
            {children}

            <Snackbar
                key={toast?.id}
                open={open}
                autoHideDuration={toast?.severity === "error" ? 8000 : 4000}
                onClose={(_event, reason) => reason !== "clickaway" && setOpen(false)}
                anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
            >
                <Alert severity={toast?.severity ?? "info"} variant="filled" onClose={() => setOpen(false)} sx={{ width: "100%" }}>
                    {toast?.message}
                </Alert>
            </Snackbar>

            <Dialog open={dialog !== null} onClose={() => answer(false)} maxWidth="xs" fullWidth>
                <DialogTitle>{dialog?.title ?? "Подтверждение"}</DialogTitle>
                <DialogContent>
                    <DialogContentText>{dialog?.message}</DialogContentText>
                </DialogContent>
                <DialogActions sx={{ px: 3, pb: 2 }}>
                    <Button onClick={() => answer(false)}>{dialog?.cancelText ?? "Отмена"}</Button>
                    <Button
                        variant="contained"
                        color={dialog?.danger ? "error" : "primary"}
                        onClick={() => answer(true)}
                        autoFocus
                    >
                        {dialog?.confirmText ?? "Подтвердить"}
                    </Button>
                </DialogActions>
            </Dialog>
        </NotificationContext.Provider>
    );
}
