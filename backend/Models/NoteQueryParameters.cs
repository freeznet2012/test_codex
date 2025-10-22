using System;
using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class NoteQueryParameters
{
    private const int MaxPageSize = 100;

    private int _pageSize = 10;

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, MaxPageSize)]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, 1, MaxPageSize);
    }

    [MaxLength(200)]
    public string? Search { get; set; }
}
