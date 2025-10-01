using IntegrationWithExternalParty.Web.Controllers;
using IntegrationWithExternalParty.Web.Data;
using IntegrationWithExternalParty.Web.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace IntegrationWithExternalParty.Tests;

public class HomeControllerTest
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private PersonnelRecord CreateTestRecord(int id = 1, string payroll = "001")
    {
        return new PersonnelRecord
        {
            Id = id,
            PayrollNumber = payroll,
            Forenames = "John",
            Surname = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            StartDate = new DateTime(2020, 1, 1),

            Telephone = "000000",
            Mobile = "111111",
            Address = "Test Street",
            Address2 = "Block A",
            Postcode = "12345",
            EmailHome = "test@example.com"
        };
    }

    [Fact]
    public async Task Index_ReturnsViewWithRecords()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.PersonnelRecords.Add(CreateTestRecord());
        context.SaveChanges();

        var controller = new HomeController(context);

        // Act
        var result = await controller.Index() as ViewResult;
        var model = Assert.IsAssignableFrom<List<PersonnelRecord>>(result.Model);

        // Assert
        Assert.Single(model);
        Assert.Equal("Doe", model.First().Surname);
    }

    [Fact]
    public async Task Edit_Get_ReturnsRecord_WhenExists()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var record = CreateTestRecord();
        context.PersonnelRecords.Add(record);
        context.SaveChanges();

        var controller = new HomeController(context);

        // Act
        var result = await controller.Edit(record.Id) as ViewResult;
        var model = Assert.IsType<PersonnelRecord>(result.Model);

        // Assert
        Assert.Equal("Doe", model.Surname);
    }

    [Fact]
    public async Task Edit_Post_UpdatesRecord_WhenValid()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var record = CreateTestRecord();
        context.PersonnelRecords.Add(record);
        context.SaveChanges();

        var controller = new HomeController(context);

        // Act
        record.Surname = "Smith";
        var result = await controller.Edit(record.Id, record) as RedirectToActionResult;

        var updated = context.PersonnelRecords.First();

        // Assert
        Assert.Equal("Smith", updated.Surname);
        Assert.Equal("Index", result.ActionName);
    }

}
