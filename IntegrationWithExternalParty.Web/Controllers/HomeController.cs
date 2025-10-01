using IntegrationWithExternalParty.Web.Data;
using IntegrationWithExternalParty.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace IntegrationWithExternalParty.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var records = await _context.PersonnelRecords
            .OrderBy(p => p.Surname)
            .ToListAsync();

        return View(records);
    }

    [HttpPost]
    public async Task<IActionResult> ImportFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select a valid file.";
            return RedirectToAction(nameof(Index));
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Please upload a CSV file only.";
            return RedirectToAction(nameof(Index));
        }

        int importedCount = 0;
        int skippedCount = 0;
        var errors = new List<string>();

        try
        {
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var headerLine = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(headerLine))
                {
                    TempData["ErrorMessage"] = "CSV file is empty or invalid.";
                    return RedirectToAction(nameof(Index));
                }

                var headers = headerLine.Split(',')
                    .Select(h => h.Trim().Trim('"'))
                    .ToList();

                var columnMap = new Dictionary<string, int>();
                for (int i = 0; i < headers.Count; i++)
                {
                    columnMap[headers[i]] = i;
                }

                int lineNumber = 1;
                string? line;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lineNumber++;

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        var values = ParseCsvLine(line);

                        if (values.Count < headers.Count)
                        {
                            errors.Add($"Line {lineNumber}: Insufficient columns");
                            continue;
                        }

                        var record = new PersonnelRecord
                        {
                            PayrollNumber = GetValue(values, columnMap, "Personnel_Records.Payroll_Number"),
                            Forenames = GetValue(values, columnMap, "Personnel_Records.Forenames"),
                            Surname = GetValue(values, columnMap, "Personnel_Records.Surname"),
                            DateOfBirth = ParseDate(GetValue(values, columnMap, "Personnel_Records.Date_of_Birth")),
                            Telephone = GetValue(values, columnMap, "Personnel_Records.Telephone"),
                            Mobile = GetValue(values, columnMap, "Personnel_Records.Mobile"),
                            Address = GetValue(values, columnMap, "Personnel_Records.Address"),
                            Address2 = GetValue(values, columnMap, "Personnel_Records.Address_2"),
                            Postcode = GetValue(values, columnMap, "Personnel_Records.Postcode"),
                            EmailHome = GetValue(values, columnMap, "Personnel_Records.EMail_Home"),
                            StartDate = ParseDate(GetValue(values, columnMap, "Personnel_Records.Start_Date"))
                        };

                        if (!IsValidRecord(record))
                        {
                            errors.Add($"Line {lineNumber}: Invalid or incomplete data");
                            continue;
                        }

                        var exists = await _context.PersonnelRecords
                            .AnyAsync(p => p.PayrollNumber == record.PayrollNumber);

                        if (!exists)
                        {
                            _context.PersonnelRecords.Add(record);
                            await _context.SaveChangesAsync();
                            importedCount++;
                        }
                        else
                        {
                            skippedCount++;
                            errors.Add($"Line {lineNumber}: Payroll Number '{record.PayrollNumber}' already exists - skipped.\n");
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Line {lineNumber}: {ex.Message}");
                    }
                }
            }

            var message = $"Successfully imported {importedCount} record(s).";
            if (skippedCount > 0)
            {
                message += $" {skippedCount} duplicate(s) skipped.";
            }

            TempData["SuccessMessage"] = message;

            if (errors.Any())
            {
                var errorMessage = "Some issues occurred:<br/>" + string.Join("<br/>", errors.Take(10));
                if (errors.Count > 10)
                {
                    errorMessage += $"<br/>... and {errors.Count - 10} more errors.";
                }
                TempData["ErrorMessage"] = errorMessage;
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error processing file: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var record = await _context.PersonnelRecords.FindAsync(id);
        if (record == null)
        {
            return NotFound();
        }

        return View(record);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, PersonnelRecord record)
    {
        if (id != record.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(record);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Record updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await RecordExists(record.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        return View(record);
    }

    private List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var currentValue = "";
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(currentValue.Trim().Trim('"'));
                currentValue = "";
            }
            else
            {
                currentValue += c;
            }
        }

        values.Add(currentValue.Trim().Trim('"'));
        return values;
    }

    private string GetValue(List<string> values, Dictionary<string, int> columnMap, string columnName)
    {
        if (columnMap.TryGetValue(columnName, out int index) && index < values.Count)
        {
            return values[index];
        }
        return string.Empty;
    }

    private bool IsValidRecord(PersonnelRecord record)
    {
        return !string.IsNullOrWhiteSpace(record.PayrollNumber) &&
               !string.IsNullOrWhiteSpace(record.Forenames) &&
               !string.IsNullOrWhiteSpace(record.Surname) &&
               record.DateOfBirth != default &&
               record.StartDate != default;
    }

    private DateTime ParseDate(string dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr))
        {
            throw new ArgumentException("Date cannot be empty");
        }

        var formats = new[] {
                "dd/MM/yyyy",
                "d/M/yyyy",
                "dd-MM-yyyy",
                "d-M-yyyy",
                "yyyy-MM-dd",
                "dd/MM/yy",
                "d/M/yy",
                "M/d/yyyy",
                "MM/dd/yyyy"
            };

        if (DateTime.TryParseExact(dateStr, formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTime result))
        {
            return result;
        }

        throw new FormatException($"Invalid date format: {dateStr}");
    }

    private async Task<bool> RecordExists(int id)
    {
        return await _context.PersonnelRecords.AnyAsync(e => e.Id == id);
    }
}
