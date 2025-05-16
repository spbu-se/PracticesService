import { Paper, Typography, List, ListItem, ListItemText, Link, Box } from "@mui/material";

export function HandbookTab() {
    return (
        <Paper sx={{ p: 3 }}>
            <Typography variant="h5" gutterBottom component="div">
                Порядок работы над учебной практикой
            </Typography>

            <List>
                <ListItem>
                    <ListItemText
                        primary="1. Выбор руководителя и темы"
                        secondary="Выбираете научного руководителя и тему из списка предложенных."
                        secondaryTypographyProps={{ component: 'div' }}
                    />
                </ListItem>
                <ListItem>
                    <ListItemText
                        primary="2. Требования"
                        secondary="От второкурсников ожидается умение писать хороший код и искать информацию, от третьекурсников – не только умение что-то сделать, но и понимание того, зачем и почему это делалось."
                        secondaryTypographyProps={{ component: 'div' }}
                    />
                </ListItem>
                <ListItem>
                    <ListItemText
                        primary="3. Процесс работы"
                        secondary="Выполняете работу, раз в неделю отчитываясь научнику (и консультанту, если есть) о том, на что ушло время."
                        secondaryTypographyProps={{ component: 'div' }}
                    />
                </ListItem>
                <ListItem>
                    <ListItemText
                        primary="Типичный план работы:"
                        secondaryTypographyProps={{ component: 'div' }}
                        secondary={
                            <Box component="div" sx={{ pl: 2 }}>
                                <Typography component="div">• Погружение в предметную область</Typography>
                                <Typography component="div">• Обзор аналогов и используемых инструментов</Typography>
                                <Typography component="div">• Проектирование и реализация</Typography>
                                <Typography component="div">• Апробация</Typography>
                                <Typography component="div">• Написание текста, подготовка к защите</Typography>
                            </Box>
                        }
                    />
                </ListItem>
                <ListItem>
                    <ListItemText
                        primary="4. Завершение"
                        secondary="В конце семестра сдаёте на кафедру отчёт, отзыв научника, и проходите защиту."
                        secondaryTypographyProps={{ component: 'div' }}
                    />
                </ListItem>
                <ListItem>
                    <ListItemText
                        primary="5. Допуск к защите"
                        secondary="Для допуска к защите проходите рецензирование, а после – сдаёте презентацию."
                        secondaryTypographyProps={{ component: 'div' }}
                    />
                </ListItem>
                <ListItem>
                    <ListItemText
                        primary="6. Результаты"
                        secondary="Оценка пойдет в диплом, а курсовая - в архив кафедры."
                        secondaryTypographyProps={{ component: 'div' }}
                    />
                </ListItem>
            </List>

            <Typography variant="h6" gutterBottom component="div" sx={{ mt: 3 }}>
                Полезные материалы:
            </Typography>

            <Box component="div">
                <List>
                    <ListItem>
                        <Link href="https://se.math.spbu.ru/files/PracticesGuide.pdf" target="_blank" rel="noopener">
                            Подробный гайд по учебным практикам (рекомендуем к прочтению!)
                        </Link>
                    </ListItem>
                </List>

                <Typography variant="subtitle1" component="div" sx={{ mt: 2 }}>Шаблоны:</Typography>
                <List dense sx={{ pl: 2 }}>
                    <ListItem>
                        <Link href="https://github.com/spbu-se/matmex-diploma-template" target="_blank" rel="noopener">отчёта</Link>
                    </ListItem>
                    <ListItem>
                        <Link href="https://github.com/spbu-se/report_presentation_template" target="_blank" rel="noopener">презентации</Link>
                    </ListItem>
                    <ListItem>
                        <Typography component="div">отзывов:</Typography>
                        <List dense sx={{ pl: 2 }}>
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

                <Typography variant="subtitle1" component="div" sx={{ mt: 2 }}>Примеры курсовых работ:</Typography>
                <List dense sx={{ pl: 2 }}>
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
            </Box>
        </Paper>
    );
}