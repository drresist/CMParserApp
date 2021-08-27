using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CsvHelper;
using System.Globalization;
using System.Net;
using System.Configuration;

namespace ParserApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("НАЧИНАЕМ РАБОТУ!");
            var login = ConfigurationManager.AppSettings["Login"];
            var pass = ConfigurationManager.AppSettings["Password"];
            var folder_link = ConfigurationManager.AppSettings["FTPLink"];
            Console.WriteLine("Читаю логи из FTP " + folder_link);
            string stringLog = ReadFromFTP(login, pass, folder_link);
            Console.WriteLine("Чтение завершено. Начинаю обработку");
            var status = ParseLog(stringLog);
            if (status == true)
            {
                Console.WriteLine("Все прошло успешно. Логи можно найти в директории ./logs/");
            }
            else
            {
                Console.WriteLine("Что-то пошло не так :(");
            }
            Console.WriteLine("Нажмите любую кнопку для завершения работы");
            Console.ReadKey();

        }

        /// <summary>
        /// Read lines from icmserver.log
        /// </summary>
        /// <returns></returns>
        static string ReadFromFTP(string login, string password, string folder_link)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(folder_link);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            request.Credentials = new NetworkCredential(login, password);

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            var newString = reader.ReadToEnd().Replace(Environment.NewLine, " ");
            reader.Close();
            response.Close();
            return newString;
        }
        /// <summary>
        /// Convert log string to normal and write to csv
        /// </summary>
        /// <param name="rawLogs"></param>
        /// <returns></returns>
        static bool ParseLog(string rawLogs)
        {
            List<LogData> logs = new List<LogData>();
            string tempLine = rawLogs.Replace(Environment.NewLine, " ").Trim().Replace(@"\n", " ");
            string pattern = @"ICMPLSC2";
            string tempLineWithoutNewLines = Regex.Replace(tempLine, @"\t|\n|\r", "");
            string[] logLines = Regex.Split(tempLineWithoutNewLines, pattern);
            try
            {
                for (int i = 0; i < logLines.Length; i++)
                {
                    // Splitted line
                    var splittedLine = logLines[i].Split(';');
                    if (splittedLine.Length != 1)
                    {
                        var userName = splittedLine[3].Split(' ')[2];
                        var message = Regex.Split(splittedLine[3].Replace(Environment.NewLine, " "), userName)[1];
                        logs.Add(new LogData
                        {
                            Date = splittedLine[1],
                            PID = splittedLine[2],
                            Message = message,
                            Account = userName,
                        });
                    }
                }
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\Logs\");
                using (var writer = new StreamWriter(Environment.CurrentDirectory + @"\\Logs\\Log.csv"))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(logs);
                }
            }
            catch
            {
                Console.WriteLine("Не смог преобразовать.");
                return false;
            }
            return true;
        }
    }


}
public class LogData
{
    public string Date { get; set; }
    public string PID { get; set; }
    public string Account { get; set; }
    public string Message { get; set; }
}


