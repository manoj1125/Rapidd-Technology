using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using System.Net.Http;

public record Employee(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("totalTimeWorked")] double TotalTimeWorked
);

public class EmployeeReport
{
    private const string ApiEndpoint = "https://api.example.com/employees/timesheet";
    private const string OutputFile = "EmployeeReport.html";

    public static async Task Main(string[] args)
    {
        Console.WriteLine("--- Employee Time Worked Report Generator ---");

        try
        { 
            var employees = await FetchEmployeeDataAsync();

            if (employees == null || !employees.Any())
            {
                Console.WriteLine("Error: Could not fetch employee data or the dataset was empty.");
                return;
            }

           
            var sortedEmployees = employees
                .OrderByDescending(e => e.TotalTimeWorked)
                .ToList();

            
            string htmlContent = GenerateHtmlReport(sortedEmployees);

            
            File.WriteAllText(OutputFile, htmlContent);

            Console.WriteLine($"\n✅ SUCCESS: Employee report generated and saved to {OutputFile}");
            Console.WriteLine("Please open the HTML file in your web browser to view the report.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ An unhandled error occurred: {ex.Message}");
            Console.WriteLine("Ensure you are running this in a modern .NET console environment.");
        }
    }

    private static async Task<List<Employee>> FetchEmployeeDataAsync()
    {
        Console.WriteLine($"Attempting to fetch data from {ApiEndpoint}...");

        string mockJson = @"[
            { ""name"": ""Rajesh"", ""totalTimeWorked"": 150.75 },
            { ""name"": ""Suraj"", ""totalTimeWorked"": 125.5 },
            { ""name"": ""Pritam"", ""totalTimeWorked"": 100.01 },
            { ""name"": ""Tania"", ""totalTimeWorked"": 99.9 },
            { ""name"": ""Amit"", ""totalTimeWorked"": 88.0 },
            { ""name"": ""Eisha"", ""totalTimeWorked"": 75.25 }
        ]";

      
        await Task.Delay(500);

        Console.WriteLine("Data successfully retrieved (using mock data).");
        try
        {
            return JsonSerializer.Deserialize<List<Employee>>(mockJson);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing JSON: {ex.Message}");
            return null;
        }
    }

    private static string GenerateHtmlReport(List<Employee> employees)
    {
        var html = new StringBuilder();

        html.Append(@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Employee Time Report</title>
    <!-- Load Tailwind CSS for modern, responsive styling -->
    <script src=""https://cdn.tailwindcss.com""></script>
    <style>
        /* Custom style for employees who worked less than 100 hours */
        .low-hours {
            background-color: #fef2f2; /* Red-50 */
            color: #b91c1c; /* Red-700 */
            border-left: 5px solid #ef4444; /* Red-500 indicator */
        }
        .container-report {
            max-width: 1000px;
        }
        /* Ensure table is fully responsive */
        @media (max-width: 600px) {
            .table-cell-name {
                font-size: 0.9rem;
            }
            .table-cell-hours {
                font-size: 0.9rem;
            }
        }
    </style>
</head>
<body class=""bg-gray-100 p-4 md:p-10 font-sans"">
    <div class=""container-report mx-auto bg-white shadow-2xl rounded-xl p-6 md:p-10"">
        <h1 class=""text-3xl md:text-4xl font-extrabold mb-4 text-gray-800"">
            Total Time Worked Report
        </h1>
        <p class=""mb-6 text-sm text-gray-500 border-b pb-4"">
            Employees are ranked by their total time worked (highest first).
            <span class=""font-semibold text-red-600 block sm:inline-block mt-2 sm:mt-0"">
                Rows highlighted in light red indicate employees who worked less than 100 hours.
            </span>
        </p>

        <div class=""overflow-x-auto rounded-lg shadow-xl"">
            <table class=""min-w-full divide-y divide-gray-200"">
                <thead class=""bg-gray-50 border-b-2 border-gray-200"">
                    <tr>
                        <th class=""px-6 py-4 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider rounded-tl-lg"">
                            Employee Name
                        </th>
                        <th class=""px-6 py-4 text-right text-xs font-semibold text-gray-600 uppercase tracking-wider rounded-tr-lg"">
                            Total Time Worked (Hours)
                        </th>
                    </tr>
                </thead>
                <tbody class=""bg-white divide-y divide-gray-100"">");

        
        foreach (var employee in employees)
        {
          
            string rowClass = employee.TotalTimeWorked < 100
                ? "low-hours transition duration-200 ease-in-out" 
                : "hover:bg-green-50 transition duration-200 ease-in-out"; 

            html.Append($@"
                    <tr class=""{rowClass}"">
                        <td class=""table-cell-name px-6 py-4 whitespace-nowrap text-sm md:text-base font-medium text-gray-900"">
                            {employee.Name}
                        </td>
                        <td class=""table-cell-hours px-6 py-4 whitespace-nowrap text-sm md:text-base text-right font-bold"">
                            {employee.TotalTimeWorked:F2}
                        </td>
                    </tr>");
        }

      
        html.Append(@"
                </tbody>
            </table>
        </div>
    </div>
</body>
</html>");

        return html.ToString();
    }
}
