using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using backend.Data;
using backend.Entities;
using backend.Models;
using backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.AzureSql;

public class AzureSqlNoteRepository(NotesDbContext context) : INoteRepository
{
    private readonly NotesDbContext _context = context;

    public async Task<PagedResult<Note>> GetAsync(NoteQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var query = _context.Notes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim();
            query = query.Where(n => EF.Functions.Like(n.Title, $"%{search}%") ||
                                     (n.Content != null && EF.Functions.Like(n.Content, $"%{search}%")));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var notes = await query
            .OrderByDescending(n => n.UpdatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new PagedResult<Note>(notes, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public Task<Note?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Notes.AsNoTracking().FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task<Note> CreateAsync(Note note, CancellationToken cancellationToken = default)
    {
        note.Id = Guid.NewGuid();
        note.CreatedAt = DateTime.UtcNow;
        note.UpdatedAt = note.CreatedAt;

        _context.Notes.Add(note);
        await _context.SaveChangesAsync(cancellationToken);
        return note;
    }

    public async Task UpdateAsync(Note note, CancellationToken cancellationToken = default)
    {
        note.UpdatedAt = DateTime.UtcNow;
        _context.Notes.Update(note);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Note note, CancellationToken cancellationToken = default)
    {
        _context.Notes.Remove(note);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
