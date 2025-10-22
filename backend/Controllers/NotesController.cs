using backend.Dtos;
using backend.Entities;
using backend.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using backend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotesController(INoteRepository repository) : ControllerBase
{
    private readonly INoteRepository _repository = repository;

    [HttpGet]
    public async Task<ActionResult<PagedResult<NoteDto>>> GetNotesAsync([FromQuery] NoteQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _repository.GetAsync(parameters, cancellationToken);

        var dto = new PagedResult<NoteDto>(
            result.Items.Select(MapToDto).ToList(),
            result.TotalCount,
            result.PageNumber,
            result.PageSize);

        return Ok(dto);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NoteDto>> GetNoteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var note = await _repository.GetByIdAsync(id, cancellationToken);
        if (note is null)
        {
            return NotFound();
        }

        return Ok(MapToDto(note));
    }

    [HttpPost]
    public async Task<ActionResult<NoteDto>> CreateNoteAsync([FromBody] CreateNoteDto dto, CancellationToken cancellationToken)
    {
        var note = new Note
        {
            Title = dto.Title,
            Content = dto.Content
        };

        var created = await _repository.CreateAsync(note, cancellationToken);
        var result = MapToDto(created);

        return CreatedAtAction(nameof(GetNoteByIdAsync), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateNoteAsync(Guid id, [FromBody] UpdateNoteDto dto, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return NotFound();
        }

        existing.Title = dto.Title;
        existing.Content = dto.Content;

        await _repository.UpdateAsync(existing, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteNoteAsync(Guid id, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(existing, cancellationToken);

        return NoContent();
    }

    private static NoteDto MapToDto(Note note) => new(note.Id, note.Title, note.Content, note.CreatedAt, note.UpdatedAt);
}
