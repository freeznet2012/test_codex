using System;
using System.Threading;
using System.Threading.Tasks;
using backend.Entities;
using backend.Models;

namespace backend.Repositories;

public interface INoteRepository
{
    Task<PagedResult<Note>> GetAsync(NoteQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<Note?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Note> CreateAsync(Note note, CancellationToken cancellationToken = default);
    Task UpdateAsync(Note note, CancellationToken cancellationToken = default);
    Task DeleteAsync(Note note, CancellationToken cancellationToken = default);
}
