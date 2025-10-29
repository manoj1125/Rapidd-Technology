using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.Json;


public class WorkActivity
{
    public string Activity { get; set; }
    public int Minutes { get; set; }
}


public struct ChartEntry
{
    public string Activity { get; init; }
    public int Minutes { get; init; }
    public double Percentage { get; init; }
    public float SweepAngle { get; init; }
}

public class PieChartGenerator
{
    
    private static string GetJsonData()
    {
       
        return @"[
          { ""Activity"": ""Development"", ""Minutes"": 450 },
          { ""Activity"": ""Meetings"", ""Minutes"": 120 },
          { ""Activity"": ""Documentation"", ""Minutes"": 90 },
          { ""Activity"": ""Code Review"", ""Minutes"": 60 }
        ]";
    }

    
    public static void Main(string[] args)
    {
        Console.WriteLine("Starting Pie Chart Generation...");

        try
        {
           
            string jsonData = GetJsonData();
            var activities = JsonSerializer.Deserialize<List<WorkActivity>>(jsonData);

            if (activities == null || activities.Count == 0)
            {
                Console.WriteLine("Error: Data is empty or could not be deserialized.");
                return;
            }

          
            var totalMinutes = activities.Sum(a => a.Minutes);
            if (totalMinutes == 0)
            {
                Console.WriteLine("Error: Total work time is zero. Cannot create a chart.");
                return;
            }

            
            var chartData = activities.Select(a => new ChartEntry
            {
                Activity = a.Activity,
                Minutes = a.Minutes,
                Percentage = (double)a.Minutes / totalMinutes * 100,
                SweepAngle = (float)a.Minutes / totalMinutes * 360
            }).ToList();

            Console.WriteLine($"Total Minutes Worked: {totalMinutes}");

            // 3. Generate Chart
            string fileName = "work_time_pie_chart.png";
            GeneratePieChart(chartData, fileName);

            Console.WriteLine($"\nSuccessfully generated pie chart: {fileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine("Ensure you have the System.Drawing.Common package installed if running in .NET Core/Standard.");
        }
    }

    private static void GeneratePieChart(List<ChartEntry> chartData, string fileName)
    {
        
        int width = 800;
        int height = 600;
        int chartSize = 400; 
        int padding = 50;

       
        var colors = new Color[] {
            Color.FromArgb(63, 114, 175), 
            Color.FromArgb(204, 76, 70),   
            Color.FromArgb(92, 169, 93),  
            Color.FromArgb(250, 192, 94) 
        };

       
        Func<int, Color> getColor = (i) => colors[i % colors.Length];

        

        using (var bitmap = new Bitmap(width, height))
        using (var graphics = Graphics.FromImage(bitmap))
        {
           
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.White);

            
            Rectangle chartRect = new Rectangle(
                (width - chartSize) / 2, 
                padding,                
                chartSize,
                chartSize
            );

            float currentStartAngle = 0;
            int colorIndex = 0;
           
            int legendY = chartRect.Y;
            int legendX = chartRect.Right + 30; 

            
            using (var titleFont = new Font("Arial", 16, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(Color.Black))
            {
                string title = "Employee Activity Time Distribution";
                SizeF textSize = graphics.MeasureString(title, titleFont);
                
                float titleX = (width - textSize.Width) / 2;
                graphics.DrawString(title, titleFont, titleBrush, titleX, padding / 2);
            }

           
            foreach (var item in chartData)
            {
                Color sliceColor = getColor(colorIndex);

                
                using (var brush = new SolidBrush(sliceColor))
                {
                    graphics.FillPie(brush, chartRect, currentStartAngle, item.SweepAngle);

                   
                    graphics.DrawPie(Pens.DarkGray, chartRect, currentStartAngle, item.SweepAngle);
                }

                
                double angleInRadians = (currentStartAngle + item.SweepAngle / 2) * Math.PI / 180.0;
                float labelRadius = chartSize / 2 * 0.7f;

                
                float centerX = chartRect.X + chartRect.Width / 2;
                float centerY = chartRect.Y + chartRect.Height / 2;

                
                float labelX = centerX + (float)(labelRadius * Math.Cos(angleInRadians));
                float labelY = centerY + (float)(labelRadius * Math.Sin(angleInRadians));

                string labelText = $"{item.Percentage:F1}%";

                
                using (var labelFont = new Font("Arial", 12, FontStyle.Bold))
                using (var labelBrush = new SolidBrush(Color.White)) 
                {
                    SizeF textSize = graphics.MeasureString(labelText, labelFont);
                    
                    graphics.DrawString(labelText, labelFont, labelBrush, labelX - textSize.Width / 2, labelY - textSize.Height / 2);
                }

               
                string legendText = $"{item.Activity} ({item.Minutes} min)";
                using (var legendFont = new Font("Arial", 10))
                using (var legendBrush = new SolidBrush(Color.Black))
                {
                    
                    graphics.FillRectangle(new SolidBrush(sliceColor), legendX, legendY, 15, 15);
                    graphics.DrawRectangle(Pens.DarkGray, legendX, legendY, 15, 15);

                  
                    graphics.DrawString(legendText, legendFont, legendBrush, legendX + 25, legendY);

                    legendY += 25; 
                }

                currentStartAngle += item.SweepAngle;
                colorIndex++;
            }

            
            bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
