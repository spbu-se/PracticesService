import { Paper, Typography, List, ListItem, ListItemText, Link } from "@mui/material";

export function HandbookTab() {
    return (
        <Paper sx={{ p: 3 }}>
            <Typography variant="h5" gutterBottom>
                Порядок работы над учебной практикой
            </Typography>

            <List>
                <ListItem>
                    <ListItemText
                        primary="1. Выбор руководителя и темы"
                        secondary="Выбираете научного руководителя и тему из списка предложенных."
                    />
                </ListItem>
                <ListItem>
                    <ListItemText
                        primary="2. Требования"
                        secondary="От второкурсников ожидается умение писать хороший код и искать информацию, от третьекурсников – не только умение что-то сделать, но и понимание того, зачем и почему это делалось."
                    />
                </ListItem>
                <ListItem>
                    <ListItemText
                        primary="3. Процесс работы"
                        secondary="Выполняете работу, раз в неделю отчитываясь научнику (и консультанту, если есть) о том, на что ушло время."
                    />
                </ListItem>
                <ListItem>
                    <ListItemText
                        primary="Типичный план работы:"
                        secondary={
                            <List dense sx={{pl: 2}}>
                                <ListItem>
                                    <ListItemText primary="• Погружение в предметную область" />
                                </ListItem>
                                <ListItem>
                                    <ListItemText primary="• Обзор аналогов и используемых инструментов" />
                                </ListItem>
                                <ListItem>
                                    <ListItemText primary="• Проектирование и реализация" />
                                </ListItem>
                                <ListItem>
                                    <ListItemText primary="• Апробация" />
                                </ListItem>
                                <ListItem>
                                    <ListItemText primary="• Написание текста, подготовка к защите" />
                                </ListItem>
                            </List>
                        }
                    />
                </ListItem>
                <ListItem>
                    <ListItemText
                        primary="4. Завершение"
                        secondary="В конце семестра сдаёте на кафедру отчёт, отзыв научника, и проходите защиту."
                    />
                </ListItem>
                <ListItem>
                    <ListItemText
                        primary="5. Допуск к защите"
                        secondary="Для допуска к защите проходите рецензирование, а после – сдаёте презентацию."
                    />
                </ListItem>
                <ListItem>
                    <ListItemText
                        primary="6. Результаты"
                        secondary="Оценка пойдет в диплом, а курсовая - в архив кафедры."
                    />
                </ListItem>
            </List>

            <Typography variant="h6" gutterBottom sx={{mt: 3}}>
                Полезные материалы:
            </Typography>

            <List>
                <ListItem>
                    <Link href="https://se.math.spbu.ru/files/PracticesGuide.pdf" target="_blank" rel="noopener">
                        Подробный гайд по учебным практикам (рекомендуем к прочтению!)
                    </Link>
                </ListItem>

                <Typography variant="subtitle1" sx={{mt: 2}}>Шаблоны:</Typography>
                <List dense sx={{pl: 2}}>
                    <ListItem>
                        <Link href="https://github.com/spbu-se/matmex-diploma-template" target="_blank" rel="noopener">отчёта</Link>
                    </ListItem>
                    <ListItem>
                        <Link href="https://github.com/spbu-se/report_presentation_template" target="_blank" rel="noopener">презентации</Link>
                    </ListItem>
                    <ListItem>
                        отзывов:
                        <List dense sx={{pl: 2}}>
                            <ListItem>
                                <Link href="https://docs.google.com/document/d/1bjIlOsBn47HhU_q9DRnrz-yjiWtiML85uDU_n5OgQXc" target="_blank" rel="noopener">научника</Link>
                            </ListItem>
                            <ListItem>
                                <Link href="https://docs.google.com/document/d/13jq0sNdH6lWY9qg0mdDIaxcU7eEK3iGJuveiaqzJxyk" target="_blank" rel="noopener">консультанта</Link>
                            </ListItem>
                        </List>
                    </ListItem>
                    <ListItem>
                        <Link href="https://drive.google.com/file/d/1yd7DH2y94CDiBxHX59-I7BTwrtrwit6r" target="_blank" rel="noopener">акта о внедрении</Link>
                    </ListItem>
                </List>

                <Typography variant="subtitle1" sx={{mt: 2}}>Примеры курсовых работ:</Typography>
                <List dense sx={{pl: 2}}>
                    <ListItem>
                        <Link href="https://se.math.spbu.ru/theses.html?search=&worktype=8&supervisor=0&course=0&startdate=2020&enddate=2021" target="_blank" rel="noopener">
                            весенняя практика 3 курса с 2020 по 2021 год
                        </Link>
                    </ListItem>
                    <ListItem>
                        <Link href="https://se.math.spbu.ru/theses.html?search=&worktype=4&supervisor=0&course=0&startdate=2016&enddate=2018" target="_blank" rel="noopener">
                            магистерские ВКР с 2016 по 2018 год
                        </Link>
                    </ListItem>
                    <ListItem>
                        <Link href="https://se.math.spbu.ru/theses.html" target="_blank" rel="noopener">
                            и многие другие...
                        </Link>
                    </ListItem>
                </List>
            </List>
        </Paper>
    );
}