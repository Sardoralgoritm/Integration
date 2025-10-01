using System.ComponentModel.DataAnnotations;

namespace IntegrationWithExternalParty.Web.Models.Entities;

public class PersonnelRecord
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string PayrollNumber { get; set; }

    [Required]
    [StringLength(100)]
    public string Forenames { get; set; }

    [Required]
    [StringLength(100)]
    public string Surname { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }

    [StringLength(20)]
    public string Telephone { get; set; }

    [StringLength(20)]
    public string Mobile { get; set; }

    [StringLength(200)]
    public string Address { get; set; }

    [StringLength(200)]
    public string Address2 { get; set; }

    [StringLength(20)]
    public string Postcode { get; set; }

    [StringLength(100)]
    public string EmailHome { get; set; }

    [Required]
    public DateTime StartDate { get; set; }
}