using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
namespace sqliteTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //connect
            SQLiteConnection conn = new SQLiteConnection("Data Source =" + Environment.CurrentDirectory + "/test.db");
            conn.Open();  

            //create table
            if(false)
            {
                string sql = "CREATE TABLE IF NOT EXISTS student(id integer, name varchar(20), sex varchar(2));";
                SQLiteCommand cmdCreateTable = new SQLiteCommand(sql, conn);
                cmdCreateTable.ExecuteNonQuery();
            }


            //insert data
            if(false)
            {
                SQLiteCommand cmdInsert = new SQLiteCommand(conn);
                cmdInsert.CommandText = "INSERT INTO student VALUES(1, 'xiaohong', 'male')";
                cmdInsert.ExecuteNonQuery();
                cmdInsert.CommandText = "INSERT INTO student VALUES(2, 'xiaoli', 'female')";
                cmdInsert.ExecuteNonQuery();
                cmdInsert.CommandText = "INSERT INTO student VALUES(3, 'xiaoming', 'male')";
                cmdInsert.ExecuteNonQuery();
            }



            //read data
            if(true)
            {
                string sql = "select * from student order by id desc";
                SQLiteCommand command = new SQLiteCommand(sql, conn);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("id:" + reader["id"] + "\tName: " + reader["name"] + "\tsex: " + reader["sex"]);
                }

            }


            conn.Close();  


        }
    }
}
