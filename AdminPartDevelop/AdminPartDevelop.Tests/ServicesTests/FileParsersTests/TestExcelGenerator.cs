using Aspose.Cells;
using System.IO;
using System.Threading.Tasks;

namespace AdminPartDevelop.Tests.Helpers
{
    public static class TestExcelGenerator
    {
        /// <summary>
        /// Creates a test referees Excel file with the expected structure
        /// </summary>
        public static async Task<string> CreateRefereesExcelFile(string outputPath)
        {
            var workbook = new Workbook();
            var worksheet = workbook.Worksheets[0];

            // Add header cells for league categories
            worksheet.Cells["A1"].PutValue("PŘEBOR");
            worksheet.Cells["A2"].PutValue("PŘÍJMENÍ JMÉNO");

            worksheet.Cells["E1"].PutValue("1.A TŘÍDA");
            worksheet.Cells["E2"].PutValue("PŘÍJMENÍ JMÉNO");

            worksheet.Cells["I1"].PutValue("1.B TŘÍDA");
            worksheet.Cells["I2"].PutValue("PŘÍJMENÍ JMÉNO");

            worksheet.Cells["M1"].PutValue("2. - 3.TŘÍDA");
            worksheet.Cells["M2"].PutValue("PŘÍJMENÍ JMÉNO");

            worksheet.Cells["Q1"].PutValue("KATEGORIE M");
            worksheet.Cells["Q2"].PutValue("PŘÍJMENÍ JMÉNO");

            worksheet.Cells["U1"].PutValue("KATEGORIE N");
            worksheet.Cells["U2"].PutValue("PŘÍJMENÍ JMÉNO");

            // Add referee data in the first league (PŘEBOR)
            worksheet.Cells["A3"].PutValue("Novák");
            worksheet.Cells["B3"].PutValue("Jan");
            worksheet.Cells["C3"].PutValue(35);

            worksheet.Cells["A4"].PutValue("Svoboda");
            worksheet.Cells["B4"].PutValue("Petr");
            worksheet.Cells["C4"].PutValue(42);

            // Add referee data in the second league (1.A TŘÍDA)
            worksheet.Cells["E3"].PutValue("Dvořák");
            worksheet.Cells["F3"].PutValue("Martin");
            worksheet.Cells["G3"].PutValue(28);

            // Add referee data in the third league (1.B TŘÍDA)
            worksheet.Cells["I3"].PutValue("Procházka");
            worksheet.Cells["J3"].PutValue("Tomáš");
            worksheet.Cells["K3"].PutValue(32);

            // Save the workbook
            string filePath = Path.Combine(outputPath, "referees.xlsx");
            workbook.Save(filePath);

            return filePath;
        }

        /// <summary>
        /// Creates a test matches Excel file with the expected structure
        /// </summary>
        public static async Task<string> CreateMatchesExcelFile(string outputPath)
        {
            var workbook = new Workbook();
            var worksheet = workbook.Worksheets[0];

            // Add headers
            worksheet.Cells["A1"].PutValue("Číslo zápasu");
            worksheet.Cells["B1"].PutValue("Domácí");
            worksheet.Cells["C1"].PutValue("Hosté");
            worksheet.Cells["D1"].PutValue("Datum zápasu");
            worksheet.Cells["M1"].PutValue("Hřiště");

            worksheet.Cells["A2"].PutValue("2023001");
            worksheet.Cells["B2"].PutValue("1001 - SK Slavia");
            worksheet.Cells["C2"].PutValue("1002 - AC Sparta");
            worksheet.Cells["D2"].PutValue("15.08.2023 18:00");
            worksheet.Cells["M2"].PutValue("Eden Arena");

            worksheet.Cells["A3"].PutValue("2023002");
            worksheet.Cells["B3"].PutValue("1003 - FK Viktoria");
            worksheet.Cells["C3"].PutValue("1004 - FC Baník");
            worksheet.Cells["D3"].PutValue("16.08.2023 17:30");
            worksheet.Cells["M3"].PutValue("Doosan Arena");

            // Save the workbook
            string filePath = Path.Combine(outputPath, "matches.xlsx");
            workbook.Save(filePath);

            return filePath;
        }

        /// <summary>
        /// Creates a test played matches Excel file with the expected structure
        /// </summary>
        public static async Task<string> CreatePlayedMatchesExcelFile(string outputPath)
        {
            var workbook = new Workbook();
            var worksheet = workbook.Worksheets[0];

            // Add headers
            worksheet.Cells["A1"].PutValue("Číslo zápasu");
            worksheet.Cells["E1"].PutValue("Rozhodčí");
            worksheet.Cells["F1"].PutValue("AR1");
            worksheet.Cells["G1"].PutValue("AR2");

            // Add match data
            worksheet.Cells["A2"].PutValue("2023001");
            worksheet.Cells["E2"].PutValue("Novák Jan (1001)");
            worksheet.Cells["F2"].PutValue("Svoboda Petr (1002)");
            worksheet.Cells["G2"].PutValue("Dvořák Martin (1003)");

            worksheet.Cells["A3"].PutValue("2023002");
            worksheet.Cells["E3"].PutValue("Procházka Tomáš (1004)");
            worksheet.Cells["F3"].PutValue("Novák Jan (1001)");
            worksheet.Cells["G3"].PutValue("Svoboda Petr (1002)");

            // Save the workbook
            string filePath = Path.Combine(outputPath, "played_matches.xlsx");
            workbook.Save(filePath);

            return filePath;
        }

        /// <summary>
        /// Creates a test referee information Excel file with the expected structure
        /// </summary>
        public static async Task<string> CreateRefereeInfoExcelFile(string outputPath)
        {
            var workbook = new Workbook();
            var worksheet = workbook.Worksheets[0];

            // Add headers
            worksheet.Cells["A1"].PutValue("Příjmení");
            worksheet.Cells["B1"].PutValue("Jméno");
            worksheet.Cells["C1"].PutValue("Email");
            worksheet.Cells["D1"].PutValue("ID FAČR");
            worksheet.Cells["E1"].PutValue("Telefon");

            // Add referee data
            worksheet.Cells["A2"].PutValue("Novák");
            worksheet.Cells["B2"].PutValue("Jan");
            worksheet.Cells["C2"].PutValue("jan.novak@example.com");
            worksheet.Cells["D2"].PutValue("1001");
            worksheet.Cells["E2"].PutValue("123456789");

            worksheet.Cells["A3"].PutValue("Svoboda");
            worksheet.Cells["B3"].PutValue("Petr");
            worksheet.Cells["C3"].PutValue("petr.svoboda@example.com");
            worksheet.Cells["D3"].PutValue("1002");
            worksheet.Cells["E3"].PutValue("987654321");

            // Save the workbook
            string filePath = Path.Combine(outputPath, "referee_info.xlsx");
            workbook.Save(filePath);

            return filePath;
        }

        /// <summary>
        /// Creates a test fields information Excel file with the expected structure
        /// </summary>
        public static async Task<string> CreateFieldsInfoExcelFile(string outputPath)
        {
            var workbook = new Workbook();
            var worksheet = workbook.Worksheets[0];

            // Add headers
            worksheet.Cells["A1"].PutValue("Název hřiště");
            worksheet.Cells["B1"].PutValue("Adresa");
            worksheet.Cells["C1"].PutValue("Zeměpisná šířka");
            worksheet.Cells["D1"].PutValue("Zeměpisná délka");

            // Add field data
            worksheet.Cells["A2"].PutValue("Eden Arena");
            worksheet.Cells["B2"].PutValue("U Slavie 1540/2a, 100 00 Praha 10");
            worksheet.Cells["C2"].PutValue("50.0678N");
            worksheet.Cells["D2"].PutValue("14.4724E");

            worksheet.Cells["A3"].PutValue("Doosan Arena");
            worksheet.Cells["B3"].PutValue("Štruncovy sady 2741/3, 301 00 Plzeň");
            worksheet.Cells["C3"].PutValue("49.7472N");
            worksheet.Cells["D3"].PutValue("13.3811E");

            // Save the workbook
            string filePath = Path.Combine(outputPath, "fields_info.xlsx");
            workbook.Save(filePath);

            return filePath;
        }
    }
}