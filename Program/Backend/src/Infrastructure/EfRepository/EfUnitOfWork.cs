using Application.Interfaces;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.EfRepository;

public sealed class EfUnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ProgramContext _context;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    public EfUnitOfWork(ProgramContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // Начало транзакции
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("Транзакция уже была начата. Завершите или отмените текущую транзакцию перед началом новой.");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
    }

    // Сохранение изменений
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Если транзакция активна, SaveChanges будет частью этой транзакции
            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Обработка конфликтов параллелизма
            throw new InvalidOperationException("Конфликт параллелизма при сохранении данных.", ex);
        }
        catch (DbUpdateException ex)
        {
            // Обработка ошибок базы данных
            throw new InvalidOperationException("Ошибка при сохранении данных в базу.", ex);
        }
    }

    // Завершение транзакции (коммит)
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("Нет активной транзакции для завершения.");
        }

        try
        {
            await _currentTransaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            // Освобождаем транзакцию независимо от результата
            await _currentTransaction.DisposeAsync().ConfigureAwait(false);
            _currentTransaction = null;
        }
    }

    // Отмена транзакции (откат)
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("Нет активной транзакции для отмены.");
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            // Освобождаем транзакцию независимо от результата
            await _currentTransaction.DisposeAsync().ConfigureAwait(false);
            _currentTransaction = null;
        }
    }

    // Освобождение ресурсов
    public void Dispose()
    {
        if (!_disposed)
        {
            _currentTransaction?.Dispose();
            _context?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}