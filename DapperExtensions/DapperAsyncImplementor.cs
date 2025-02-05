﻿using Dapper;
using DapperExtensions.Mapper;
using DapperExtensions.Predicate;
using DapperExtensions.Sql;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static DapperExtensions.Snapshotter;
using AutoMapper = Slapper.AutoMapper;

namespace DapperExtensions
{
    /// <summary>
    /// Interface for asyncImplementator
    /// </summary>
    public interface IDapperAsyncImplementor : IDapperImplementor
    {
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Get{T}"/>.
        /// </summary>
        Task<(T, Snapshot<T>)> GetAsync<T>(IDbConnection connection, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false,
            IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity;
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Get{T}"/>.
        /// </summary>
        Task<(T, Snapshot<T>)> GetAsync<T>(IDbConnection connection, PredicateGroup predicates, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false,
            IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity;
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetList{T}"/>.
        /// </summary>
        Task<(IEnumerable<T>, Snapshot<T>)> GetListAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null,
            int? commandTimeout = null, bool buffered = false, IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity;
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetListAutoMap{T}"/>.
        /// </summary>
        Task<(IEnumerable<T>, Snapshot<T>)> GetListAutoMapAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null,
            int? commandTimeout = null, bool buffered = false, IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity;
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetPage{T}"/>.
        /// </summary>
        Task<IEnumerable<T>> GetPageAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, int page = 1, int resultsPerPage = 10,
            IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false, IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class;
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetPageAutoMap{T}"/>.
        /// </summary>
        Task<IEnumerable<T>> GetPageAutoMapAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, int page = 1, int resultsPerPage = 10,
            IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false, IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class;
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetSet{T}"/>.
        /// </summary>
        Task<IEnumerable<T>> GetSetAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10,
            IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false, IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class;
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Count{T}"/>.
        /// </summary>
        Task<int> CountAsync<T>(IDbConnection connection, object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class;

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Insert{T}(IDbConnection, IEnumerable{T}, IDbTransaction, int?)"/>.
        /// </summary>
        Task InsertAsync<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = default) where T : class;
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Insert{T}(IDbConnection, T, IDbTransaction, int?)"/>.
        /// </summary>
        Task<dynamic> InsertAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = default) where T : class;
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Update{T}(IDbConnection, T, IDbTransaction, int?)"/>.
        /// </summary>
        Task<bool> UpdateAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout, bool ignoreAllKeyProperties = false, IList<IProjection> colsToUpdate = null, Snapshot<T> snapshot = null, bool useUpdateLock = false) where T : BaseEntity;
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Delete{T}(IDbConnection, T, IDbTransaction, int?)"/>.
        /// </summary>
        Task<bool> DeleteAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class;
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Delete{T}(IDbConnection, object, IDbTransaction, int?)"/>.
        /// </summary>
        Task<bool> DeleteAsync<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout) where T : class;

        Task<IMultipleResultReader> GetMultipleAsync(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout, IList<IReferenceMap> includedProperties = null);
    }

    public class DapperAsyncImplementor : DapperImplementor, IDapperAsyncImplementor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DapperAsyncImplementor"/> class.
        /// </summary>
        /// <param name="sqlGenerator">The SQL generator.</param>
        public DapperAsyncImplementor(ISqlGenerator sqlGenerator)
            : base(sqlGenerator) { }

        #region Implementation of IDapperAsyncImplementor
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Insert{T}(IDbConnection, IEnumerable{T}, IDbTransaction, int?)"/>.
        /// </summary>
        public async Task InsertAsync<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = default) where T : class
        {
            //Got the information here to avoid doing it for each item and so we speed up the execution
            var classMap = SqlGenerator.Configuration.GetMap<T>();
            var nonIdentityKeyProperties = classMap.Properties.Where(p => p.KeyType == KeyType.Guid || p.KeyType == KeyType.Assigned).ToList();
            var identityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.Identity);
            var triggerIdentityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.TriggerIdentity);
            var sequenceIdentityColumn = classMap.Properties.Where(p => p.KeyType == KeyType.SequenceIdentity).ToList();

            foreach (var e in entities)
                await InternalInsertAsync(connection, e, transaction, commandTimeout, classMap, nonIdentityKeyProperties, identityColumn, triggerIdentityColumn, sequenceIdentityColumn);
        }

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Insert{T}(IDbConnection, T, IDbTransaction, int?)"/>.
        /// </summary>
        public async Task<dynamic> InsertAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            var classMap = SqlGenerator.Configuration.GetMap<T>();
            var nonIdentityKeyProperties = classMap.Properties.Where(p => p.KeyType == KeyType.Guid || p.KeyType == KeyType.Assigned).ToList();
            var identityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.Identity);
            var triggerIdentityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.TriggerIdentity);
            var sequenceIdentityColumn = classMap.Properties.Where(p => p.KeyType == KeyType.SequenceIdentity).ToList();

            return await InternalInsertAsync(connection, entity, transaction, commandTimeout, classMap, nonIdentityKeyProperties, identityColumn, triggerIdentityColumn, sequenceIdentityColumn);
        }
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Update{T}(IDbConnection, T, IDbTransaction, int?)"/>.
        /// </summary>
        public async Task<bool> UpdateAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout, bool ignoreAllKeyProperties, IList<IProjection> colsToUpdate = null, Snapshot<T> snapshot = null, bool useUpdateLock = false) where T : BaseEntity
        {
            if (colsToUpdate == null)
                colsToUpdate = GetChangeTrackCols<T>(snapshot, entity);

            return await InternalUpdateAsync(connection, entity, transaction, colsToUpdate, commandTimeout, ignoreAllKeyProperties, useUpdateLock);
        }
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Delete{T}(IDbConnection, T, IDbTransaction, int?)"/>.
        /// </summary>
        public async Task<bool> DeleteAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            return await Task.FromResult(Delete<T>(connection, entity, transaction, commandTimeout));
        }
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Delete{T}(IDbConnection, object, IDbTransaction, int?)"/>.
        /// </summary>
        public async Task<bool> DeleteAsync<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            return await Task.FromResult(Delete<T>(connection, predicate, transaction, commandTimeout));
        }
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Get{T}"/>.
        /// </summary>
        public async Task<(T, Snapshot<T>)> GetAsync<T>(IDbConnection connection, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false,
            IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity
        {
            return await Task.FromResult(((T, Snapshot<T>))InternalGet<T>(connection, id, transaction, commandTimeout, colsToSelect, includedProperties, changeTrack, noLock));
        }

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Get{T}"/>.
        /// </summary>
        public async Task<(T, Snapshot<T>)> GetAsync<T>(IDbConnection connection, PredicateGroup predicates, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false,
            IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity
        {
            return await Task.FromResult(((T, Snapshot<T>))InternalGetPredicate<T>(connection, predicates, transaction, commandTimeout, colsToSelect, includedProperties, changeTrack, noLock));
        }

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetList{T}"/>.
        /// </summary>
        public async Task<(IEnumerable<T>, Snapshot<T>)> GetListAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null,
            int? commandTimeout = null, bool buffered = false, IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity
        {
            return await InternalGetListAutoMapAsync<T>(connection, predicate, sort, transaction, commandTimeout, buffered, colsToSelect, includedProperties, noLock);
        }

        public async Task<(IEnumerable<T>, Snapshot<T>)> GetListAutoMapAsync<T>(IDbConnection connection, object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout,
            bool buffered = false, IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity
        {
            return await InternalGetListAutoMapAsync<T>(connection, predicate, sort, transaction, commandTimeout, buffered, colsToSelect, includedProperties, noLock);
        }

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetPage{T}"/>.
        /// </summary>
        public async Task<IEnumerable<T>> GetPageAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, int page = 1, int resultsPerPage = 10,
            IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false, IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class
        {
            return await InternalGetPageAutoMapAsync<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered, colsToSelect, includedProperties, noLock);
        }

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetPageAutoMap{T}"/>.
        /// </summary>
        public async Task<IEnumerable<T>> GetPageAutoMapAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, int page = 1, int resultsPerPage = 10,
            IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false, IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class
        {
            return await InternalGetPageAutoMapAsync<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered, colsToSelect, includedProperties, noLock);
        }

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetSet{T}"/>.
        /// </summary>
        public async Task<IEnumerable<T>> GetSetAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10,
            IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false, IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class
        {
            return await InternalGetSetAsync<T>(connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered, colsToSelect, includedProperties, noLock);
        }

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Count{T}"/>.
        /// </summary>
        public async Task<int> CountAsync<T>(IDbConnection connection, object predicate = null, IDbTransaction transaction = null,
            int? commandTimeout = null, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class
        {
            return await Task.FromResult(Count<T>(connection, predicate, transaction, commandTimeout, includedProperties, noLock));
        }

        public async Task<IMultipleResultReader> GetMultipleAsync(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction,
            int? commandTimeout, IList<IReferenceMap> includedProperties = null)
        {
            if (SqlGenerator.SupportsMultipleStatements())
            {
                return await Task.FromResult(GetMultipleByBatch(connection, predicate, transaction, commandTimeout, includedProperties));
            }

            return await Task.FromResult(GetMultipleBySequence(connection, predicate, transaction, commandTimeout, includedProperties));
        }
        #endregion

        #region Private implementations

        private async Task<dynamic> InternalInsertAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout,
            IClassMapper classMap, IList<IMemberMap> nonIdentityKeyProperties, IMemberMap identityColumn,
            IMemberMap triggerIdentityColumn, IList<IMemberMap> sequenceIdentityColumn) where T : class
        {
            return await Task.FromResult(InternalInsert(connection, entity, transaction, commandTimeout, classMap, nonIdentityKeyProperties, identityColumn, triggerIdentityColumn, sequenceIdentityColumn));
        }

        private async Task<bool> InternalUpdateAsync<T>(IDbConnection connection, T entity, IClassMapper classMap, IPredicate predicate, IDbTransaction transaction,
            IList<IProjection> cols, int? commandTimeout, bool ignoreAllKeyProperties = false, bool useUpdateLock = false) where T : class
        {
            return await Task.FromResult(InternalUpdate(connection, entity, classMap, predicate, transaction, cols, commandTimeout, ignoreAllKeyProperties, useUpdateLock));
        }

        private async Task<bool> InternalUpdateAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, IList<IProjection> cols,
            int? commandTimeout, bool ignoreAllKeyProperties = false, bool useUpdateLock = false) where T : class
        {
            GetMapAndPredicate<T>(entity, out var classMap, out var predicate, true);
            return await InternalUpdateAsync(connection, entity, classMap, predicate, transaction, cols, commandTimeout, ignoreAllKeyProperties, useUpdateLock);
        }

        private async void InternalUpdateAsync<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction, IList<IProjection> cols,
            int? commandTimeout, bool ignoreAllKeyProperties = false, bool useUpdateLock = false) where T : class
        {
            GetMapAndPredicate<T>(entities.FirstOrDefault(), out var classMap, out var predicate, true);

            foreach (var e in entities)
                await InternalUpdateAsync(connection, e, classMap, predicate, transaction, cols, commandTimeout, ignoreAllKeyProperties, useUpdateLock);
        }

        //private async Task<T> InternalGetAsync<T>(IDbConnection connection, dynamic id, IDbTransaction transaction, int? commandTimeout, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class
        //{
        //    return await Task.FromResult(InternalGetAutoMap<T>(connection, id, null, transaction, commandTimeout, true, colsToSelect, includedProperties, noLock));
        //}

        private async Task<(IEnumerable<T>, Snapshot<T>)> InternalGetListAutoMapAsync<T>(IDbConnection connection, object predicate, IList<ISort> sort, IDbTransaction transaction,
            int? commandTimeout, bool buffered, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity
        {
            return await Task.FromResult(InternalGetListAutoMap<T>(connection, predicate, sort, transaction, commandTimeout, buffered, colsToSelect, includedProperties, changeTrack, noLock));
        }

        private async Task<IEnumerable<T>> InternalGetPageAutoMapAsync<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage,
            IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class
        {
            return await Task.FromResult(InternalGetPageAutoMap<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered, colsToSelect, includedProperties, noLock));
        }

        private async Task<IEnumerable<T>> InternalGetSetAsync<T>(IDbConnection connection, object predicate, IList<ISort> sort, int firstResult, int maxResults,
            IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class
        {
            return await Task.FromResult(InternalGetSet<T>(connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered, colsToSelect, includedProperties, noLock));
        }
        #endregion

        #region Helpers

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetList{T}"/>.
        /// </summary>
        protected async Task<IEnumerable<T>> GetListAsync<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, IList<IProjection> colsToSelect = null, bool noLock = false) where T : class
        {
            var parameters = new Dictionary<string, object>();
            var sql = SqlGenerator.Select(classMap, predicate, sort, parameters, colsToSelect, noLock: noLock);
            var dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }

            return await connection.QueryAsync<T>(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text).ConfigureAwait(false);
        }

        protected async Task<IEnumerable<T>> GetListAutoMapAsync<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, IList<IProjection> colsToSelect = null, bool noLock = false) where T : class
        {
            var query = await GetListAsync<dynamic>(connection, classMap, predicate, sort, transaction, commandTimeout, colsToSelect, noLock);
            var data = query.ToList();

            return await Task.FromResult(AutoMapper.MapDynamic<T>(data, false)).ConfigureAwait(false);
        }

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetPage{T}"/>.
        /// </summary>
        protected async Task<IEnumerable<T>> GetPageAsync<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, IList<IProjection> colsToSelect = null, bool noLock = false) where T : class
        {
            var parameters = new Dictionary<string, object>();
            var sql = SqlGenerator.SelectPaged(classMap, predicate, sort, page, resultsPerPage, parameters, colsToSelect, noLock: noLock);
            var dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }

            return await connection.QueryAsync<T>(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text).ConfigureAwait(false);
        }

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetPage{T}"/>.
        /// </summary>
        protected async Task<IEnumerable<T>> GetPageAutoMapAsync<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, IList<IProjection> colsToSelect = null, bool noLock = false) where T : class
        {
            var query = await GetPageAsync<dynamic>(connection, classMap, predicate, sort, page, resultsPerPage, transaction, commandTimeout, colsToSelect, noLock: noLock);
            var data = query.ToList();

            return await Task.FromResult(AutoMapper.MapDynamic<T>(data, false)).ConfigureAwait(false);
        }

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetSet{T}"/>.
        /// </summary>
        protected async Task<IEnumerable<T>> GetSetAsync<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, IList<IProjection> colsToSelect = null, bool noLock = false) where T : class
        {
            var parameters = new Dictionary<string, object>();
            var sql = SqlGenerator.SelectSet(classMap, predicate, sort, firstResult, maxResults, parameters, colsToSelect, noLock: noLock);
            var dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }

            return await connection.QueryAsync<T>(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text).ConfigureAwait(false);
        }
        #endregion
    }
}
