using System.Linq.Expressions;
using CvManager.Application.Common;
using CvManager.Application.Common.ErrorsCodes;
using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;

namespace CvManager.Infrastructure.Persistence;

public static class EfSave
{
    public static bool IsRowVersionMismatch(uint dbRowVersion, uint clientRowVersion) =>
        dbRowVersion != clientRowVersion;

    public static void SetRowVersion<TEntity, TProperty>(DbContext context, TEntity entity, uint rowVersion,
        Expression<Func<TEntity, TProperty>> forceModifiedProperty) where TEntity : class
    {
        var entry = context.Entry(entity);
        entry.Property("RowVersion").OriginalValue = rowVersion;
        entry.Property(forceModifiedProperty).IsModified = true;
    }

    public static async Task<ServiceResult<T>> TrySaveAsync<T>(DbContext context, Func<T> valueFactory)
    {
        try
        {
            await context.SaveChangesAsync();
            return ServiceResult<T>.Ok(valueFactory());
        }
        catch (UniqueConstraintException)
        {
            return ServiceResult<T>.FailCode(CommonErrorCodes.DuplicateName);
        }
        catch (ReferenceConstraintException)
        {
            return ServiceResult<T>.FailCode(CommonErrorCodes.InUse);
        }
        catch (MaxLengthExceededException)
        {
            return ServiceResult<T>.FailCode(CommonErrorCodes.ValueTooLong);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult<T>.FailCode(CommonErrorCodes.ConcurrencyConflict);
        }
    }
}
