import { createContext, useContext } from "react";
import { AlertColor } from "@mui/material";

export interface ConfirmOptions {
    title?: string;
    message: string;
    confirmText?: string;
    cancelText?: string;
    /** Marks a destructive action: the confirm button is red. */
    danger?: boolean;
}

export interface Notifier {
    /** Shows a short message at the bottom of the screen. */
    notify: (message: string, severity?: AlertColor) => void;
    success: (message: string) => void;
    error: (message: string) => void;
    /** Asks the user to confirm an action. Resolves to true if confirmed. */
    confirm: (options: ConfirmOptions) => Promise<boolean>;
}

export const NotificationContext = createContext<Notifier | null>(null);

export function useNotifier(): Notifier {
    const context = useContext(NotificationContext);
    if (!context) {
        throw new Error("useNotifier must be used inside NotificationProvider");
    }
    return context;
}
