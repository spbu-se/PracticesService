---
# Шаблон нового агента. Файлы, чьё имя начинается с "_", не загружаются.
# Чтобы добавить агента: скопируйте файл под новым именем (например, report-analyzer.md)
# и заполните. Имя файла без расширения — это имя агента в запросе (поле agents).
# Все поля необязательны.
name: my-agent
title: Название в отчёте
description: Что делает агент
# model: другая-модель-провайдера   (по умолчанию LLMSettings:Model)
# output: markdown (по умолчанию) — свободный ответ в Markdown;
# output: annotations — JSON со списком замечаний {summary, findings[{type, quote, suggestion, explanation}]},
#   замечания подсвечиваются в исходном PDF при annotate=true (см. Agents/spell-checker.md).
output: markdown
temperature: 0.2
---

Системный промпт агента: роль, что проверять, правила и формат ответа (Markdown).

Агент получает фрагмент текста документа и его тип (report / presentation / feedback).
Чтобы агент запускался без явного указания, добавьте его имя в LLMSettings:DefaultAgents.
