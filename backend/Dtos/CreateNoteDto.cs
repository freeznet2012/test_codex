using System.ComponentModel.DataAnnotations;

namespace backend.Dtos;

public class CreateNoteDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Content { get; set; }
}
