using System;

namespace backend.Dtos;

public record NoteDto(Guid Id, string Title, string? Content, DateTime CreatedAt, DateTime UpdatedAt);
