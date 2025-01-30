using System.ComponentModel.DataAnnotations;

public class CreateLocation
{
    [Required]
    public string? LocationName { get; set; }

}