﻿using Microsoft.Data.Sqlite;
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

                string commandTextCreateTableBooks = $"CREATE TABLE IF NOT EXISTS {table1} (" +
                        "BookID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, " +
                        "BookName TEXT NOT NULL, " +
                        "AuthorName TEXT NOT NULL, " +
                        "PublisherName TEXT NOT NULL, " +
                        "PublishYear DATETIME NOT NULL)";



                string commandTextCreateTableBookExemplars = $"CREATE TABLE IF NOT EXISTS {table2} (" +
                        "ExemplarID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
                        "BookID INTEGER NOT NULL, " +
                        $"FOREIGN KEY (\"BookID\") REFERENCES {tableBooks} (\"BookID\")) ";


                string commandTextCreateTableBorrowedExemplars = $"CREATE TABLE IF NOT EXISTS {table3} (" +
                        "BorrowID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, " +
                        "ExemplarID INTEGER NOT NULL, " +
                        "ReaderName TEXT NOT NULL, " +
                        "DateOfTaking DATETIME NOT NULL, " +
                        "ReturnDate DATETIME, " +
                        $"FOREIGN KEY ('ExemplarID') REFERENCES {table2} ('ExemplarID'))";

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

        // Подключение к БД и выполнение запроса
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

    }
}