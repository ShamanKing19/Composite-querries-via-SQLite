using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Composite_querries_via_SQLite
{
    class Program
    {
        static void Main(string[] args)
        {
            // Настрока ширины консоли
            Console.BufferWidth = 130;
            Console.WindowWidth = 130;
            //Console.WindowWidth = 10;


            Library.CreateBase("Library.db", "Books", "BookExemplars", "BorrowedBooks");

            
            /*Library.InsertIntoBooks("Война и мир", "Л. Н. Толстой", "Дрофа", 1979);
            Library.InsertIntoBooks("Мастер и Маргарита", "М. А. Булгаков", "Эксмо", 1990);
            Library.InsertIntoBooks("Горе от ума", "А.С. Грибоедов", "Просвещение", 1999);
            Library.InsertIntoBooks("Тихий Дон", "М. А. Шолохов", "Эксмо", 2005);
            Library.InsertIntoBooks("Идиот", "Ф. М. Достоевский", "Просвещение", 1965);
            Library.InsertIntoBooks("Преступление и наказание", "Ф. М. Достоевский", "Дрофа", 2015);
            */

            // БОЛЬШЕ ВСЕГО БРАЛИ КНИГИ БУЛГАКОВА "МАСТЕР И МАРГАРИТА"
            // БОЛЬШЕ ВСЕГО ЗА ПОСЛЕДНИЙ ГОД БРАЛИ КНИГИ ТОЛСТОГО "ВОЙНА И МИР"
            //Library.InsertIntoBorrowedBooks(1, "Максим", new DateTime(2020, 12, 29));

            Library.ShowData("Books");
            Library.ShowData("BookExemplars");
            Library.ShowData("BorrowedBooks");

        }
    }
}
