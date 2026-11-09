import { ReactNode } from "react";
import { Box, Checkbox, Paper, Radio, Typography } from "@mui/material";

interface OptionCardProps {
    selected: boolean;
    onClick: () => void;
    icon: ReactNode;
    title: string;
    description?: string;
    control: "radio" | "checkbox";
    disabled?: boolean;
}

/// Selectable card used for document types, agents and result formats.
export function OptionCard({ selected, onClick, icon, title, description, control, disabled }: OptionCardProps) {
    const Control = control === "radio" ? Radio : Checkbox;

    return (
        <Paper
            variant="outlined"
            onClick={disabled ? undefined : onClick}
            sx={{
                p: 2,
                display: "flex",
                gap: 1.5,
                alignItems: "flex-start",
                cursor: disabled ? "default" : "pointer",
                opacity: disabled ? 0.6 : 1,
                borderRadius: 3,
                borderWidth: 2,
                borderColor: selected ? "primary.main" : "divider",
                bgcolor: selected ? "action.selected" : "background.paper",
                transition: "border-color .15s, background-color .15s, box-shadow .15s",
                "&:hover": disabled ? undefined : { borderColor: "primary.main", boxShadow: 2 },
            }}
        >
            <Box sx={{ color: selected ? "primary.main" : "text.secondary", display: "flex", pt: 0.5 }}>{icon}</Box>
            <Box sx={{ flex: 1, minWidth: 0 }}>
                <Typography fontWeight={600}>{title}</Typography>
                {description && (
                    <Typography variant="body2" color="text.secondary" sx={{ mt: 0.25 }}>
                        {description}
                    </Typography>
                )}
            </Box>
            <Control checked={selected} tabIndex={-1} disableRipple sx={{ p: 0 }} />
        </Paper>
    );
}
