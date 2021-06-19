<h1>Тестовое задание от "Финансовые Инфромационные Системы".</h1>

Консольное приложение было созданно на C# .NET 5.0. <br>База данных создавалась с помощью SQlite.

1. Описание модели данных:
<img src = "https://github.com/ShamanKing19/Composite-querries-via-SQLite/blob/master/DatabaseModel.png">

Запросы для создания таблиц:

Таблица "Books"
```SQL
CREATE TABLE IF NOT EXISTS Books (
    BookID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
    BookName TEXT NOT NULL,
    AuthorName TEXT NOT NULL,
    PublisherName TEXT NOT NULL,
    PublishYear DATETIME NOT NULL)
```
Таблица "BookExemplars"
```SQL
CREATE TABLE IF NOT EXISTS BookExemplars (
    ExemplarID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    BookID INTEGER NOT NULL,
    FOREIGN KEY ('BookID') REFERENCES Books ('BookID'))
```
Таблица "BorrowedBooks"
```SQL
CREATE TABLE IF NOT EXISTS BorrowedBooks (
    BorrowID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    ExemplarID INTEGER NOT NULL,
    ReaderName TEXT NOT NULL,
    DateOfTaking DATETIME NOT NULL,
    ReturnDate DATETIME,
    CHECK (DateOfTaking <= ReturnDate),
    FOREIGN KEY ('ExemplarID') REFERENCES BookExemplars ('ExemplarID'))
```


2. SQL-запрос, возвращающий самого популярного автора за год:
```SQL
SELECT AuthorName, count(AuthorName) as 'NumberOfBorrowings'
FROM(
    --Выборка книг, выданных за последний год
    SELECT AuthorName, DateOfTaking
    FROM(
        --Соединение всех таблиц в одну по ID и выборка столбца с автором и датой выдачи книги
        SELECT Books.AuthorName, BorrowedBooks.DateOfTaking
        FROM(Books INNER JOIN BookExemplars ON Books.BookID = BookExemplars.BookID) INNER JOIN BorrowedBooks ON BookExemplars.ExemplarID = BorrowedBooks.ExemplarID WHERE BorrowedBooks.ExemplarID = BookExemplars.ExemplarID
        )
    WHERE DateOfTaking > date('now', '-1 year')
    )
GROUP BY AuthorName
-- Сортируем по уменьшению
ORDER BY NumberOfBorrowings DESC
-- Берём только первую запись с наибольшим числом выдач
LIMIT 1
```

3. "Злостный читатель" - тот, кто в среднем просрочивает сдачу книги на наибольшее количество дней. Подразумевается, что просрочка начинается через 30 дней после даты выдачи книги.
Алгоритм поиска "злостного читателя" удалось реализовать через SQL-запрос:
```SQL
-- Вывод средних значений просрочки и выбор максимального значения
SELECT ReaderName, avg(Overdue) as 'AverageOverdue'
FROM(
    --Вывод тех, кто не просрочил со сдачей книги
    SELECT ReaderName, (Difference - 30) * 0 as 'Overdue'
    FROM(
        --Вывод имя и разницы между датой выдачи и возврата(если книга не просрочена, то значение равно 0)
        SELECT ReaderName, DateOfTaking, ReturnDate, ifnull(julianday(ReturnDate) - julianday(DateOfTaking), 0) as 'Difference'
        FROM BorrowedBooks
        )
    WHERE Overdue <= 0

    UNION

    -- Вывод тех, кто просрочил со сдачей книги
    SELECT ReaderName, Difference - 30 as 'Overdue'
    FROM(
        --Вывод имя и разницы между датой выдачи и возврата(если книга не просрочена, то значение равно 0)
        SELECT ReaderName, DateOfTaking, ReturnDate, ifnull(julianday(ReturnDate) - julianday(DateOfTaking), 0) as 'Difference'
        FROM BorrowedBooks
        )
    WHERE Overdue > 0
    )
GROUP BY ReaderName
ORDER BY AverageOverdue DESC
LIMIT 1
```
Вывод консольного приложения:
<img src = "https://github.com/ShamanKing19/Composite-querries-via-SQLite/blob/master/ProgramLibrary.png">

