using System;
using System.Threading;
using System.Threading.Tasks;
using backend.Entities;
using backend.Models;
using backend.Repositories;

namespace backend.Repositories.Supabase;

/// <summary>
/// Placeholder repository that documents how to plug in an alternative provider such as Supabase.
/// Each method currently throws <see cref="NotImplementedException"/> to highlight the work that remains.
/// </summary>
public class SupabaseNoteRepository : INoteRepository
{
    public Task<PagedResult<Note>> GetAsync(NoteQueryParameters parameters, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Implement Supabase data access here.");

    public Task<Note?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Implement Supabase data access here.");

    public Task<Note> CreateAsync(Note note, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Implement Supabase data access here.");

    public Task UpdateAsync(Note note, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Implement Supabase data access here.");

    public Task DeleteAsync(Note note, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Implement Supabase data access here.");
}
