using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace CSharpAssignment
{

    public class Teacher
    {
        public string id { get; set; }
        public string Name { get; set; }
        public string Classs { get; set; }
        public string sec { get; set; }


    }
    [Serializable]
    class ImportTextFileException : Exception
    {
        public int Line
        {
            get;
            private set;
        }
        public int Column
        {
            get;
            private set;
        }
        public ImportTextFileException() : base() { }
        public ImportTextFileException(string message) : base(message) { }
        public ImportTextFileException(string message, Exception innerException, int Line, int Column) : base(string.Format("Error in line {0} column {1}. {2}", Line, Column, message), innerException)
        {
            this.Line = Line;
            this.Column = Column;
        }
        protected ImportTextFileException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.Line = info.GetInt32("Line");
            this.Column = info.GetInt32("Column");
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Line", this.Line);
            info.AddValue("Column", this.Column);
        }
    }
    static class Program
    {
        static void Main(string[] args)
        {
            /*
             * add, delete, update, exit
             * */
            
            bool exitcondition = false;
            do
            {
                Console.WriteLine("Teacher Management App");
                Console.WriteLine("-----------------------");
                Console.WriteLine("**********Menu*********");
                Console.WriteLine("-----------------------");
                Console.WriteLine("1. Add Teacher Data");
                Console.WriteLine("2. Update Teacher Data");
                Console.WriteLine("3. Delete Teacher Record");
                Console.WriteLine("4.Exit Application");
                Console.WriteLine("-----------------------");
                Console.Write("Enter your Choice (Add/Update/Delete/Exit): ");
                string choice = Console.ReadLine();
                switch (choice.ToLower())
                {
                    case "add": AddTeacher();
                        break;
                    case "update": UpdateTeacher();
                        break;
                    case "delete": DeleteTeacher();
                        break;
                    case "exit": exitcondition=true;
                        break;
                    default: break;
                }

            } while (exitcondition != true);

        }
        static List<T> ImportFile<T>(string FileName, char ColumnSeperator) where T : new()
        {
            var list = new List<T>();
            try
            {

                using (var str = File.OpenText(FileName))
                {
                    int Line = 1;
                    int Column = 0;
                    try
                    {
                        var columnsHeader = str.ReadLine().Split(ColumnSeperator);
                        var t = typeof(T);
                        var plist = new Dictionary<int,
                            PropertyInfo>();
                        for (int i = 0; i < columnsHeader.Length; i++)
                        {
                            var p = t.GetProperty(columnsHeader[i]);
                            if (p != null && p.CanWrite && p.GetIndexParameters().Length == 0) plist.Add(i, p);
                        }
                        string s;
                        while ((s = str.ReadLine()) != null)
                        {
                            Line++;
                            var data = s.Split(ColumnSeperator);
                            var obj = new T();
                            foreach (var p in plist)
                            {
                                Column = p.Key;
                                if (p.Value.PropertyType == typeof(int)) p.Value.SetValue(obj, int.Parse(data[Column]), null);
                                else if (p.Value.PropertyType == typeof(string)) p.Value.SetValue(obj, data[Column], null);
                            }
                            list.Add(obj);
                        }
                        return list;
                    }
                    catch (Exception )
                    {
                        // var teach = new Teacher[] { };
                        return list;
                    }
                }
            }
            catch(Exception)
            {

            }
            return list;
        }

        
       
        static void ExportToTextFile<T>(this IEnumerable<T> data, string FileName, char ColumnSeperator)
        {
            using (var sw = File.CreateText(FileName))
            {
                var plist = typeof(T).GetProperties().Where(p => p.CanRead && (p.PropertyType.IsValueType || p.PropertyType == typeof(string)) && p.GetIndexParameters().Length == 0).ToList();
                if (plist.Count > 0)
                {
                    var seperator = ColumnSeperator.ToString();
                    sw.WriteLine(string.Join(seperator, plist.Select(p => p.Name)));
                    foreach (var item in data)
                    {
                        var values = new List<object>();
                        foreach (var p in plist) values.Add(p.GetValue(item, null));
                        sw.WriteLine(string.Join(seperator, values));
                    }
                }
            }
        }
      
        public static void AddTeacher()
        {
            var list = ImportFile<Teacher>("Teacher.txt", ';');
            //add
            Console.WriteLine("Enter the Teacher's ID:");
            string Id = Console.ReadLine();
            foreach(var teacher in list)
            {
                if(teacher.id==Id)
                {
                    Console.WriteLine("Id already Present");
                    return;
                }
            }
            Console.WriteLine("Enter the Teacher's Name:");
            string name = Console.ReadLine();
            Console.WriteLine("Enter the Teacher's Class:");
            string classs = Console.ReadLine();
            Console.WriteLine("Enter the Teacher's Section:");
            string section = Console.ReadLine();
            list.Add(new Teacher() { id=Id,Classs=classs,Name=name,sec=section });


            //
            list.ExportToTextFile("Teacher.txt", ';');


            //save fil
        }
        public static void UpdateTeacher()
        {
            //read
            var list = ImportFile<Teacher>("Teacher.txt", ';');
            //update
            Console.WriteLine("Enter the Teacher's ID:");
            string Id = Console.ReadLine();
            foreach (var teacher in list)
            {
                if (teacher.id == Id)
                {
                    Console.WriteLine("Enter the value you want to update(Name/Class/Sec):");
                    string updatedValue = Console.ReadLine();
                    switch(updatedValue.ToLower())
                    {
                        case "name":
                            Console.WriteLine("Enter the Teacher's Name:");
                            string name = Console.ReadLine();
                            teacher.Name = name;
                            break;
                        case "class":
                            Console.WriteLine("Enter the Teacher's Class:");
                            string classs = Console.ReadLine();
                            teacher.Classs = classs;
                            break;
                        case "sec":
                            Console.WriteLine("Enter the Teacher's Section:");
                            string section = Console.ReadLine();
                            teacher.sec = section;
                            break;
                        default: Console.WriteLine("Incorrect Data");
                            break;
                    }
                    list.ExportToTextFile("Teacher.txt", ';');
                    return;
                }
            }
            Console.WriteLine("Could not find the ID");
            //save
        }
        public static void DeleteTeacher()
        {
            //read
            var list = ImportFile<Teacher>("Teacher.txt", ';');
            Console.WriteLine("Enter the Teacher's ID:");
            string Id = Console.ReadLine();
            bool ispresent = false;

            foreach(var teacher in list)
            {
                if(teacher.id==Id)
                {
                    ispresent = true;
                }
            }
            if (ispresent == true)
            {
                var item = list.Find(x => x.Name == Id);
                list.Remove(item);
                list.ExportToTextFile("Teacher.txt", ';');

            }
            else
            {
                Console.WriteLine("Record not present");

            }
            
            //delete
            //save
        }

    }
}
