import { useState, useEffect } from "react";
import {
    Grid, Typography, Paper, Button, TextField, Box, List, ListItem, ListItemText, Divider
} from "@mui/material";
import MDEditor from "@uiw/react-md-editor";
// import { getReportsByPracticeId, postReport } from "@/shared/services/axios.service.ts";
// import { Report } from "@/entities/Report";

export function Reporting({ practiceId }: { practiceId?: number }) {
    const [reports, setReports] = useState<Report[]>([]);
    const [newReportTitle, setNewReportTitle] = useState("");
    const [newReportContent, setNewReportContent] = useState("");
    
    useEffect(() => {
        if (practiceId) {
            // getReportsByPracticeId(practiceId).then(response => {
            //     setReports(response.data);
            // });
        }
    }, [practiceId]);
    
    const handleSubmitReport = async () => {
        // if (newReportTitle.trim() && newReportContent.trim()) {
        //     const newReport: Partial<Report> = {
        //         practiceId,
        //         title: newReportTitle,
        //         content: newReportContent,
        //         createdDate: new Date().toISOString()
        //     };
        //     await postReport(newReport);
        //     setNewReportTitle("");
        //     setNewReportContent("");
        //     // Reload reports
        //     const updatedReports = await getReportsByPracticeId(practiceId);
        //     setReports(updatedReports.data);
        // }
    };

    return (
        <Grid container direction="column" spacing={4}>
            <Grid item>
                <Typography variant="h5" gutterBottom>Отчеты по практике</Typography>
            </Grid>
            
            <Grid item>
                <Paper elevation={3} sx={{ p: 3, borderRadius: 2 }}>
                    <Typography variant="h6" gutterBottom>Отправленные отчеты</Typography>
                    {reports.length > 0 ? (
                        <List>
                            {reports.map(report => (
                                <>test</>
                                // <Box key={report.id}>
                                //     <ListItem>
                                //         <ListItemText
                                //             primary={report.title}
                                //             secondary={`Дата: ${new Date(report.createdDate).toLocaleString()}`}
                                //         />
                                //     </ListItem>
                                //     <Divider />
                                // </Box>
                            ))}
                        </List>
                    ) : (
                        <Typography variant="body2">Отчетов пока нет.</Typography>
                    )}
                </Paper>
            </Grid>
            
            <Grid item>
                <Paper elevation={3} sx={{ p: 3, borderRadius: 2 }}>
                    <Typography variant="h6" gutterBottom>Создать новый отчет</Typography>
                    <MDEditor
                        value={newReportContent}
                        onChange={(val) => setNewReportContent(val)}
                        height={200}
                        preview="edit"
                        visibleDragbar={false}
                        textareaProps={{
                            placeholder: "Что было сделано..."
                        }}
                        style={{ borderRadius: 8 }}
                    />

                    <MDEditor
                        value={newReportContent}
                        onChange={(val) => setNewReportContent(val)}
                        height={200}
                        preview="edit"
                        visibleDragbar={false}
                        textareaProps={{
                            placeholder: "Что планируется сделать..."
                        }}
                        style={{ borderRadius: 8 }}
                    />
                    <Box display="flex" justifyContent="flex-end" sx={{ mt: 2 }}>
                        <Button variant="contained" color="primary" onClick={handleSubmitReport}>
                            Отправить отчет
                        </Button>
                    </Box>
                </Paper>
            </Grid>
        </Grid>
    );
}
