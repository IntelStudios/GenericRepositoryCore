using GenericRepository.Enums;
using GenericRepository.Exceptions;
using GenericRepository.Helpers;
using GenericRepository.Interfaces;
using GenericRepository.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Contexts
{
    public partial class GRMSSQLContext : GRContext, IGRContext, IDisposable
    {
        #region Saving enqueued entities
        public override void SaveChangesInTransaction()
        {
            SqlConnection sqlConnection = GetSqlConnection();

            LogDebug("Transaction - start.");

            try
            {
                BeginTransaction();
            }
            catch (Exception exc)
            {
                LogDebug("Async Transaction - start failed: {0}.", exc.Message);
                LogError(exc, "Async Transaction - start failed.");
                throw;
            }

            try
            {
                SaveChanges();
                CommitTransaction();

                LogDebug("Transaction - success.");
            }
            catch
            {
                LogDebug("Transaction - fail.");
                RollbackTransaction();
                throw;
            }
        }

        public override async Task SaveChangesInTransactionAsync()
        {
            LogDebug("Async Transaction - start.");

            try
            {
                BeginTransaction();
            }
            catch (Exception exc)
            {
                LogDebug("Async Transaction - start failed: {0}.", exc.Message);
                LogError(exc, "Async Transaction - start failed.");
                throw;
            }

            try
            {
                await SaveChangesAsync();
                CommitTransaction();

                LogDebug("Async Transaction - success.");
            }
            catch
            {
                LogDebug("Async Transaction - fail.");
                RollbackTransaction();
                throw;
            }
        }

        public override void SaveChanges()
        {
            try
            {
                List<GRContextQueueItem> currentContextQueue = new List<GRContextQueueItem>(contextQueue);

                foreach (var item in currentContextQueue)
                {
                    string methodName = GetSaveMethodName(item.Action, false);
                    MethodInfo method = typeof(GRMSSQLContext).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                    MethodInfo genericMethod = method.MakeGenericMethod(item.Item.Type);
                    genericMethod.Invoke(this, new object[] { item.Item });
                    contextQueue.Remove(item);
                }
            }
            catch (Exception exc)
            {
                ProcessSaveException(exc);
            }
        }

        public override async Task SaveChangesAsync()
        {
            try
            {
                List<GRContextQueueItem> currentContextQueue = new List<GRContextQueueItem>(contextQueue);

                foreach (var item in currentContextQueue)
                {
                    string methodName = GetSaveMethodName(item.Action, true);
                    MethodInfo method = typeof(GRMSSQLContext).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                    MethodInfo genericMethod = method.MakeGenericMethod(item.Item.Type);
                    Task task = (Task)genericMethod.Invoke(this, new object[] { item.Item });
                    await task;
                    contextQueue.Remove(item);
                }
            }
            catch (Exception exc)
            {
                ProcessSaveException(exc);
            }
        }

        private void ProcessSaveException(Exception exc)
        {
            if (exc is TargetInvocationException)
            {
                ProcessSaveException(exc.InnerException);
                return;
            }

            if (exc is GRModelNotValidException)
            {
                GRModelNotValidException excModel = exc as GRModelNotValidException;
                LogError(exc, excModel.ToString());
            }

            if (exc is GRQueryExecutionFailedException)
            {
                LogError(exc, exc.Message);
            }

            throw exc;
        }
        #endregion

        #region Immediate saving of a single entity
        public override GRExecutionStatistics Execute<T>(IGRUpdatable<T> updatable)
        {
            GRContextQueueItem item = DequeueItem(updatable);

            updatable.Repository.PrepareForSave();

            if (item.Action == GRContextQueueAction.Insert)
            {
                return InsertEntity(updatable);
            }
            else
            {
                return UpdateEntity(updatable);
            }
        }

        private GRContextQueueItem DequeueItem<T>(IGRUpdatable<T> updatable)
        {
            GRContextQueueItem item = null;

            string commonError = "Avoid reusing the same entity for multiple operations.";

            try
            {
                item = contextQueue.Where(i => i.Item == updatable).SingleOrDefault();

                if (item == null)
                {
                    throw new GRQueryExecutionFailedException("Entity is not presented in a context queue! " + commonError);
                }
            }
            catch (Exception exc)
            {
                throw new GRQueryExecutionFailedException(exc, "Entity was presented in a context queue more than once! " + commonError);
            }

            contextQueue.Remove(item);
            return item;
        }

        public override async Task<GRExecutionStatistics> ExecuteAsync<T>(IGRUpdatable<T> updatable)
        {
            GRContextQueueItem item = DequeueItem(updatable);

            await updatable.Repository.PrepareForSaveAsync();

            if (item.Action == GRContextQueueAction.Insert)
            {
                return await InsertEntityAsync(updatable);
            }
            else
            {
                return await UpdateEntityAsync(updatable);
            }
        }
        #endregion

        #region Pre-save methods
        public void ValidateModel<T>(T entity, List<GRDBProperty> properties)
        {

            List<GRModelError> errors = new List<GRModelError>();
            ValidationContext context = new ValidationContext(entity, null, null);

            foreach (var property in properties)
            {
                context.MemberName = property.PropertyInfo.Name;
                object value = property.PropertyInfo.GetValue(entity);
                ICollection<ValidationResult> result = new List<ValidationResult>();
                bool isValid = Validator.TryValidateProperty(value, context, result);

                if (!isValid)
                {
                    errors.Add(new GRModelError(property.PropertyInfo, value, result));
                }
            }

            if (errors.Any())
            {
                throw new GRModelNotValidException(errors);
            }
        }

        private void CallPreSaveMethods<T>(T entity, GRPreSaveActionType action)
        {
            GRDBStructure structure = GRDataTypeHelper.GetDBStructure<T>();

            List<MethodInfo> methods = action == GRPreSaveActionType.Insert ? structure.BeforeInsertMethods : structure.BeforeUpdateMethods;

            foreach (var method in methods)
            {
                try
                {
                    method.Invoke(entity, null);
                }
                catch (Exception exc)
                {
                    throw new GRPreSaveException(exc, method, action, typeof(T));
                }
            }
        }
        #endregion

        #region Auxiliary methods
        private string[] GetPropertiesToSave<T>(IGRUpdatable<T> update, GRContextQueueAction action)
        {
            string[] propertiesToStore = null;

            List<string> autoUpdatePropertyNames = null;

            if (action == GRContextQueueAction.Insert)
            {
                autoUpdatePropertyNames = update.Structure.AutoInsertProperties
                    .Select(p => p.PropertyInfo.Name).ToList();
            }
            else
            {
                autoUpdatePropertyNames = update.Structure.AutoUpdateProperties
                    .Select(p => p.PropertyInfo.Name).ToList();
            }

            // updating all properties
            if (!update.HasIncludedProperties && !update.HasExcludedProperties && !update.HasForceExcludedProperties)
            {
                return null;
            }
            // updating only selected properties + autoproperties
            else if (!update.HasExcludedProperties && !update.HasForceExcludedProperties)
            {
                propertiesToStore = update.IncludedProperties.Select(p => p.Property.PropertyInfo.Name).ToArray();
                propertiesToStore = propertiesToStore.Union(autoUpdatePropertyNames).ToArray();
                return propertiesToStore;
            }
            else
            {
                List<string> excludedProperties = update.ExcludedProperties.Select(p => p.Property.PropertyInfo.Name).ToList();
                propertiesToStore = update.Structure.NonKeyProperties
                    .Where(p => !excludedProperties.Contains(p.PropertyInfo.Name))
                    .Select(p => p.PropertyInfo.Name)
                    .ToArray();
                propertiesToStore = propertiesToStore.Union(autoUpdatePropertyNames).ToArray();
                propertiesToStore = propertiesToStore.Except(update.ForceExcludedProperties.Select(p => p.Property.PropertyInfo.Name)).ToArray();
                return propertiesToStore;
            }
        }

        private string GetSaveMethodName(GRContextQueueAction action, bool isAsync)
        {
            if (isAsync)
            {
                return action.ToString() + "EntityAsync";
            }
            else
            {
                return action.ToString() + "Entity";
            }
        }
        #endregion
    }
}
