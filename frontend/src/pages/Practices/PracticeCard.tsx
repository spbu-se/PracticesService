import { Card, CardContent, Typography, Chip, Divider, Box } from "@mui/material";
import { Practice } from "@/entities/Practice";

interface PracticeCardProps {
    practice: Practice;
    onClick: () => void;
}

export function PracticeCard({ practice, onClick }: PracticeCardProps) {
    return (
        <Card
            sx={{
                cursor: 'pointer',
                display: 'flex',
                flexDirection: 'column',
                justifyContent: 'space-between',
                height: '100%', 
                '&:hover': {
                    boxShadow: 4,
                },
            }}
            onClick={onClick}
        >
            <CardContent sx={{ flexGrow: 1 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                    <Typography variant="h6">
                        {practice.theme?.title || "Без темы"}
                    </Typography>
                    <Chip
                        label={practice.status}
                        color={practice.status === "Завершено" ? "success" : "primary"}
                    />
                </Box>

                <Divider sx={{ my: 2 }} />

                <Typography variant="body2" color="text.secondary">
                    Тип практики: {practice.type}
                </Typography>

                <Typography variant="body2" color="text.secondary">
                    Итоговая оценка: {practice.finalgrade || "Не указана"}
                </Typography>

                <Typography variant="body2" color="text.secondary">
                    Научный руководитель: {practice?.supervisor?.lastName} {practice?.supervisor?.firstName} {practice?.supervisor?.middleName}
                </Typography>

                <Typography variant="body2" color="text.secondary">
                    Консультант: {practice?.consultant ? `${practice?.consultant?.lastName} ${practice?.consultant?.firstName} ${practice?.consultant?.middleName}` : "Не назначен"}
                </Typography>

                <Typography variant="body2" color="text.secondary">
                    Дата создания: {new Date(practice.createddate).toLocaleDateString()}
                </Typography>

                <Typography variant="body2" color="text.secondary">
                    Последнее обновление: {new Date(practice.updateddate).toLocaleDateString()}
                </Typography>
            </CardContent>
        </Card>
    );
}
