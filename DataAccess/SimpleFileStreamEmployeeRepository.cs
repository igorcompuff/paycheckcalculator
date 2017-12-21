﻿using System;
using System.IO;
using System.Text;
using Domain;

namespace DataAccess
{
    public class SimpleFileStreamEmployeeRepository : BaseFileStreamEmployeeRepository
    {
        private readonly UTF8Encoding _encoding = new UTF8Encoding();

        public SimpleFileStreamEmployeeRepository(string rootDirPath): base(rootDirPath)
        {

        }

        public override void Add(Employee obj)
        {
            //Como o id é sequencial e começa em 0, o próximo Id é igual ao número de arquivos (employees) no diretório base
            obj.Id = obj.Id < 0 ? BaseDataDirectory.GetFiles().Length : obj.Id;

            var file = new FileInfo(Path.Combine(BaseDataDirectory.FullName, $"{obj.Id}.dat"));

            using (var fstream = file.Open(FileMode.Create, FileAccess.Write))
            {
                StringBuilder recordBuilder = new StringBuilder();

                recordBuilder.Append($"{obj.Id}//");
                recordBuilder.Append($"{obj.Name}//");
                recordBuilder.Append($"{obj.Country}//");
                recordBuilder.Append($"{obj.HourlyRate}//");
                recordBuilder.Append($"{obj.HoursWorked}");

                byte[] bytes = _encoding.GetBytes(recordBuilder.ToString());
                fstream.Write(bytes, 0, bytes.Length);
            }
        }

        protected override Employee GetEmployee(FileInfo file)
        {
            Employee employee = null;

            if (file.Exists)
            {
                using (var fstream = file.OpenRead())
                {
                    byte[] buffer = new byte[fstream.Length];
                    fstream.Read(buffer, 0, buffer.Length);
                    employee = DecodeEmployee(buffer);
                }
            }

            return employee;
        }

        private Employee DecodeEmployee(byte[] encodedEmployee)
        {
            Employee employee = new Employee();
            string[] fields = _encoding.GetString(encodedEmployee).Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                employee.Id = int.Parse(fields[0]);
                employee.Name = fields[1];
                employee.Country = fields[2];
                employee.HourlyRate = double.Parse(fields[3]);
                employee.HoursWorked = int.Parse(fields[4]);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("The database is corrupted", e);
            }

            return employee;
        }
    }
}