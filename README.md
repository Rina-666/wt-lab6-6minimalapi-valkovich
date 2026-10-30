# WT-6 Minimal API (Библиотека)

## Ссылки
- **GitHub:** https://github.com/Rina-666/wt-lab6-6minimalapi-valkovich
- **Render:** https://wt-lab6-6minimalapi-valkovich.onrender.com

## API Endpoints
| Метод | URL | Назначение |
|-------|-----|------------|
| GET | `/` | Проверка работы |
| GET | `/api/info` | Информация о приложении |
| GET | `/api/books` | Список книг (query: `?search=`) |
| GET | `/api/books/{id}` | Книга по ID |
| POST | `/api/books` | Создать книгу |
| PUT | `/api/books/{id}` | Обновить книгу |
| DELETE | `/api/books/{id}` | Удалить книгу |