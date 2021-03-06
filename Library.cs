using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Composite_querries_via_SQLite
{
    abstract class Library
    {
        public static string dbFileName = "Library.db";
        // Переменные с названиями таблиц
        public static string tableBooks = "Books";
        public static string tableBookExemplars = "BookExemplars";
        public static string tableBorrowedBooks = "BorrowedBooks";


        // Создаёт файл БД с тремя таблицами
        public static void CreateBase(string fileName, string table1, string table2, string table3)
        {


            // Создание файла и таблиц, если их нет
            if (!File.Exists(fileName))
            {
                //Вручную создавать не надо, файл сам создастся при подключении
                //File.Create(dbFileName);

                string commandTextCreateTableBooks = $@"CREATE TABLE IF NOT EXISTS {table1} (
                        BookID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
                        BookName TEXT NOT NULL,
                        AuthorName TEXT NOT NULL,
                        PublisherName TEXT NOT NULL,
                        PublishYear DATETIME NOT NULL)";

                string commandTextCreateTableBookExemplars = $@"CREATE TABLE IF NOT EXISTS {table2} (
                        ExemplarID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        BookID INTEGER NOT NULL,
                        FOREIGN KEY ('BookID') REFERENCES {table1} ('BookID'))";

                string commandTextCreateTableBorrowedExemplars = $@"CREATE TABLE IF NOT EXISTS {table3} (
                        BorrowID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                        ExemplarID INTEGER NOT NULL,
                        ReaderName TEXT NOT NULL,
                        DateOfTaking DATETIME NOT NULL,
                        ReturnDate DATETIME,
                        CHECK (DateOfTaking <= ReturnDate),
                        FOREIGN KEY ('ExemplarID') REFERENCES {table2} ('ExemplarID'))";

                // Создание файла, создание таблиц с полями
                using (SqliteConnection connection = new SqliteConnection($"Data Source={fileName}"))
                {
                    connection.Open();
                    SqliteCommand command = new SqliteCommand();
                    command.Connection = connection;
                    command.CommandText = commandTextCreateTableBooks;
                    command.ExecuteNonQuery();
                    command.CommandText = commandTextCreateTableBookExemplars;
                    command.ExecuteNonQuery();
                    command.CommandText = commandTextCreateTableBorrowedExemplars;
                    command.ExecuteNonQuery();
                }


            }
        }

        // Подключение к БД и выполнение запроса без возврата таблицы
        private static void dbQuery(string query)
        {
            using (SqliteConnection connection = new SqliteConnection($"Data Source = {dbFileName}"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = query;
                command.ExecuteNonQuery();
            }
        }

        // Подключение к БД и выполнение запроса с возвратом таблицы
        private static DataTable dbQueryGetDataTable(string query)
        {
            SqliteDataReader reader;
            DataTable dataTable = new DataTable();
            string commandText = query;

            using (SqliteConnection connection = new SqliteConnection($"Data Source = {dbFileName}"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = commandText;
                reader = command.ExecuteReader();
                dataTable.Load(reader);
            }
            return dataTable;
        }

        // Внесение данных в таблицу "Books"
        public static void InsertIntoBooks(string bookName, string authorName, string publisherName, int publishYear)
        {
            dbQuery($"INSERT INTO {tableBooks} (BookName, AuthorName, PublisherName, PublishYear) VALUES ('{bookName}', '{authorName}', '{publisherName}', {publishYear})");
        }

        // Внесение данных в таблицу "BookExemplars"
        public static void InsertIntoBookExemplars(int data)
        {
            dbQuery($"INSERT INTO {tableBookExemplars} (BookID) VALUES ({data})");
        }

        // Внесение данных в таблицу "BorrowedBooks" без даты возврата
        public static void InsertIntoBorrowedBooks(int exemplarID, string readerName, DateTime dateOfTaking)
        {
            dbQuery($"INSERT INTO {tableBorrowedBooks} (ExemplarID, ReaderName, DateOfTaking) VALUES ({exemplarID}, '{readerName}', '{dateOfTaking.ToShortDateString()}')");
        }

        // Внесение данных в таблицу "BorrowedBooks" с датой возврата
        public static void InsertIntoBorrowedBooks(int exemplarID, string readerName, DateTime dateOfTaking, DateTime returnDate)
        {
            dbQuery($"INSERT INTO {tableBorrowedBooks} (ExemplarID, ReaderName, DateOfTaking, ReturnDate) VALUES ({exemplarID}, '{readerName}', '{dateOfTaking.Date.ToShortDateString()}', '{returnDate.ToShortDateString()}')");
        }

        // Вывод содержимого выбранной таблицы в консоль
        public static void ShowData(string table)
        {
            SqliteDataReader reader;
            DataTable dataTable = new DataTable();
            // Чтение всех данных из выбранной таблицы в reader
            using (SqliteConnection connection = new SqliteConnection($"Data Source = {dbFileName}"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = $"SELECT * FROM {table}";
                reader = command.ExecuteReader();
                dataTable.Load(reader);
            }


            string placeForElement = "{0,25}";

            foreach (object columnName in dataTable.Columns)
            {
                Console.Write(placeForElement, columnName);
            }
            
            Console.Write("\n");

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                for (int col = 0; col < dataTable.Columns.Count; col++)
                {
                    Console.Write(placeForElement, dataTable.Rows[i][col].ToString());
                }
                Console.Write("\n");
            }
            Console.Write("\n\n\n");
        }

        // Вывод самого популярного автора и количество выданных книг, написанных этим автором, за год
        public static void ShowMostPopularAuthor()
        {

            DataTable dataTable = new DataTable();
            string query = @" -- Подсчёт книг выданных каждому автору
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
                                    LIMIT 1";

            // Получение таблицы с выполненным запросом
            dataTable = dbQueryGetDataTable(query);

            // Проверка на наличие записей
            if (dataTable.Rows.Count != 0)
            {
                string authorName = dataTable.Rows[0][0].ToString();
                int numberOfBorrowings = Convert.ToInt32(dataTable.Rows[0][1]);
                Console.WriteLine($"Самый популярный автор: {authorName}\nКоличество выдач его книг за последний год: {numberOfBorrowings}\n\n\n");
            }
            else
            {
                throw new Exception("No borrowings for last year");
            }
        }

        // Вывод самого злостного читателя и количество взятых им книг
        // "Злостный читатель" - тот, кто в среднем дольше всего просрочивает книгу.
        // Подразумевается, что книга выдаётся на 30 дней, далее считаются дни просрочки
        // Если студент сдал книгу вовремя, то значение просрочки считается равном нулю
        public static void ShowMostSpitefulReader()
        {
            // Выборка имён студентов, дат взятия  ими книг и дат возврата, если есть
            string query = @"-- Вывод средних значений просрочки и выбор максимального значения
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
                            LIMIT 1";

            DataTable dataTable = dbQueryGetDataTable(query);

            if (dataTable.Rows.Count > 0)
            {
                string spitefulReader = dataTable.Rows[0][0].ToString();
                double averageOverdue = Convert.ToDouble(dataTable.Rows[0][1]);
                Console.WriteLine($"Самый 'злостный' читатель: {spitefulReader}\nВ среднем просрочивает книгу на {averageOverdue} дней\n\n\n");
            }
            else
            {
                throw new Exception("No readers, who have expired a book");
            }
        }

    }
}
