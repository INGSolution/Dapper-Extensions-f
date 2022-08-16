using Dapper;
using DapperExtensions.Mapper;
using DapperExtensions.Predicate;
using DapperExtensions.Sql;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using static DapperExtensions.Snapshotter;
using AutoMapper = Slapper.AutoMapper;

namespace DapperExtensions
{
    public interface IDapperImplementor
    {
        ISqlGenerator SqlGenerator { get; }
        string LastExecutedCommand { get; }
        (T, Snapshot<T>) Get<T>(IDbConnection connection, dynamic id, IDbTransaction transaction, int? commandTimeout, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity;
        (T, Snapshot<T>) Get<T>(IDbConnection connection, PredicateGroup predicates, IDbTransaction transaction, int? commandTimeout, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity;
        TOut GetPartial<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, dynamic id, IDbTransaction transaction, int? commandTimeout, IList<IReferenceMap> includedProperties = null, bool noLock = false) where TIn : BaseEntity where TOut : class;
        void Insert<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout) where T : class;
        dynamic Insert<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class;
        bool Update<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout, bool ignoreAllKeyProperties, Snapshot<T> snapshot = null) where T : BaseEntity;
        void Update<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout, bool ignoreAllKeyProperties) where T : BaseEntity;
        bool UpdatePartial<TIn, TOut>(IDbConnection connection, TIn entity, Expression<Func<TIn, TOut>> func, IDbTransaction transaction, int? commandTimeout, bool ignoreAllKeyProperties) where TIn : class;
        void UpdatePartial<TIn, TOut>(IDbConnection connection, IEnumerable<TIn> entities, Expression<Func<TIn, TOut>> func, IDbTransaction transaction, int? commandTimeout, bool ignoreAllKeyProperties) where TIn : class;
        bool Delete<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class;
        void Delete<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout) where T : class;
        bool Delete<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout) where T : class;
        (IEnumerable<T>, Snapshot<T>) GetList<T>(IDbConnection connection, object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity;
        IEnumerable<TOut> GetPartialList<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where TIn : BaseEntity;
        (IEnumerable<T>, Snapshot<T>) GetListAutoMap<T>(IDbConnection connection, object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity;
        IEnumerable<TOut> GetPartialListAutoMap<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where TIn : BaseEntity;
        IEnumerable<T> GetPage<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class;
        IEnumerable<TOut> GetPartialPage<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IReferenceMap> includedProperties = null, bool noLock = false) where TIn : class where TOut : class;
        IEnumerable<T> GetPageAutoMap<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class;
        IEnumerable<TOut> GetPartialPageAutoMap<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IReferenceMap> includedProperties = null, bool noLock = false) where TIn : class where TOut : class;
        IEnumerable<T> GetSet<T>(IDbConnection connection, object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class;
        IEnumerable<TOut> GetPartialSet<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IReferenceMap> includedProperties = null, bool noLock = false) where TIn : class where TOut : class;
        int Count<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class;
        IMultipleResultReader GetMultiple(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout, IList<IReferenceMap> includedProperties = null);
    }

    public class DapperImplementor : IDapperImplementor
    {
        private static readonly Dictionary<Type, IList<IProjection>> ColsBuffer = new Dictionary<Type, IList<IProjection>>();
        //public readonly GenericDictionary Snapper = new GenericDictionary();

        public DapperImplementor(ISqlGenerator sqlGenerator)
        {
            SqlGenerator = sqlGenerator;
        }

        public ISqlGenerator SqlGenerator { get; }
        public string LastExecutedCommand { get; protected set; }

        public (T, Snapshot<T>) Get<T>(IDbConnection connection, dynamic id, IDbTransaction transaction, int? commandTimeout, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity
        {
            return InternalGet<T>(connection, id, transaction, commandTimeout, null, includedProperties, changeTrack, noLock);
        }

        public (T, Snapshot<T>) Get<T>(IDbConnection connection, PredicateGroup predicates, IDbTransaction transaction, int? commandTimeout, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity
        {
            return InternalGetPredicate<T>(connection, predicates, transaction, commandTimeout, null, includedProperties, changeTrack, noLock);
        }

        public TOut GetPartial<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, dynamic id, IDbTransaction transaction, int? commandTimeout, IList<IReferenceMap> includedProperties = null, bool noLock = false) where TIn : BaseEntity where TOut : class
        {
            var cols = GetBufferedCols<TOut>();
            var obj = InternalGet<TIn>(connection, id, transaction, commandTimeout, cols, includedProperties, noLock:noLock);
            var f = func.Compile();
            return f.Invoke(obj);
        }

        public void Insert<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            //Got the information here to avoid doing it for each item and so we speed up the execution
            var classMap = SqlGenerator.Configuration.GetMap<T>();
            var nonIdentityKeyProperties = classMap.Properties.Where(p => p.KeyType == KeyType.Guid || p.KeyType == KeyType.Assigned).ToList();
            var identityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.Identity);
            var triggerIdentityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.TriggerIdentity);
            var sequenceIdentityColumn = classMap.Properties.Where(p => p.KeyType == KeyType.SequenceIdentity).ToList();

            foreach (var e in entities)
                InternalInsert(connection, e, transaction, commandTimeout, classMap, nonIdentityKeyProperties, identityColumn, triggerIdentityColumn, sequenceIdentityColumn);
        }

        public dynamic Insert<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            var classMap = SqlGenerator.Configuration.GetMap<T>();
            var nonIdentityKeyProperties = classMap.Properties.Where(p => p.KeyType == KeyType.Guid || p.KeyType == KeyType.Assigned).ToList();
            var identityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.Identity);
            var triggerIdentityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.TriggerIdentity);
            var sequenceIdentityColumn = classMap.Properties.Where(p => p.KeyType == KeyType.SequenceIdentity).ToList();

            return InternalInsert(connection, entity, transaction, commandTimeout, classMap, nonIdentityKeyProperties, identityColumn, triggerIdentityColumn, sequenceIdentityColumn);
        }

        public bool Update<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout, bool ignoreAllKeyProperties = false, Snapshot<T> snapshot = null) where T : BaseEntity
        {
            var cols = GetChangeTrackCols<T>(snapshot, entity);
            return InternalUpdate(connection, entity, transaction, cols, commandTimeout, ignoreAllKeyProperties);
        }

        public void Update<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout, bool ignoreAllKeyProperties = false) where T : BaseEntity
        {
            InternalUpdate(connection, entities, transaction, null, commandTimeout, ignoreAllKeyProperties);
        }

        public bool UpdatePartial<TIn, TOut>(IDbConnection connection, TIn entity, Expression<Func<TIn, TOut>> func, IDbTransaction transaction, int? commandTimeout, bool ignoreAllKeyProperties = false) where TIn : class
        {
            var cols = GetBufferedCols<TOut>();
            return InternalUpdate<TIn>(connection, entity, transaction, cols, commandTimeout, ignoreAllKeyProperties);
        }

        public void UpdatePartial<TIn, TOut>(IDbConnection connection, IEnumerable<TIn> entities, Expression<Func<TIn, TOut>> func, IDbTransaction transaction, int? commandTimeout, bool ignoreAllKeyProperties = false) where TIn : class
        {
            var cols = GetBufferedCols<TOut>();
            InternalUpdate<TIn>(connection, entities, transaction, cols, commandTimeout, ignoreAllKeyProperties);
        }

        public bool Delete<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            return InternalDelete<T>(connection, entity, transaction, commandTimeout);
        }

        public void Delete<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            foreach (var e in entities)
                InternalDelete<T>(connection, e, transaction, commandTimeout);
        }

        public bool Delete<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            GetMapAndPredicate<T>(predicate, out var classMap, out var wherePredicate);
            return Delete<T>(connection, classMap, wherePredicate, transaction, commandTimeout);
        }

        public (IEnumerable<T>, Snapshot<T>) GetList<T>(IDbConnection connection, object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity
        {
            return GetListAutoMap<T>(connection, predicate, sort, transaction, commandTimeout, buffered, includedProperties, changeTrack, noLock);
        }

        public IEnumerable<TOut> GetPartialList<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where TIn : BaseEntity
        {
            return GetPartialListAutoMap<TIn, TOut>(connection, func, predicate, sort, transaction, commandTimeout, buffered, includedProperties, changeTrack, noLock);
        }

        public (IEnumerable<T>, Snapshot<T>) GetListAutoMap<T>(IDbConnection connection, object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity
        {
            return InternalGetListAutoMap<T>(connection, predicate, sort, transaction, commandTimeout, buffered, null, includedProperties, changeTrack, noLock);
        }

        public IEnumerable<TOut> GetPartialListAutoMap<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where TIn : BaseEntity
        {
            var cols = GetBufferedCols<TOut>();
            var (te, ls) = InternalGetListAutoMap<TIn>(connection, predicate, sort, transaction, commandTimeout, buffered, cols, includedProperties, changeTrack, noLock);

            // Transform TIn object to Anonymous type
            var f = func.Compile();
            return te.ToList().Select(i => f.Invoke(i));
        }

        public IEnumerable<T> GetPage<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class
        {
            return GetPageAutoMap<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered, includedProperties, noLock);
        }

        public IEnumerable<TOut> GetPartialPage<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IReferenceMap> includedProperties = null, bool noLock = false) where TIn : class where TOut : class
        {
            return GetPartialPageAutoMap<TIn, TOut>(connection, func, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered, includedProperties, noLock);
        }

        public IEnumerable<T> GetPageAutoMap<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class
        {
            return InternalGetPageAutoMap<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered, null, includedProperties, noLock);
        }

        public IEnumerable<TOut> GetPartialPageAutoMap<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IReferenceMap> includedProperties = null, bool noLock = false) where TIn : class where TOut : class
        {
            var cols = GetBufferedCols<TOut>();
            var te = InternalGetPageAutoMap<TIn>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered, cols, includedProperties, noLock).ToList();

            // Transform TIn object to Anonymous type
            var f = func.Compile();
            return te.Select(i => f.Invoke(i));
        }

        public IEnumerable<T> GetSet<T>(IDbConnection connection, object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class
        {
            return InternalGetSet<T>(connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered, null, includedProperties, noLock);
        }

        public IEnumerable<TOut> GetPartialSet<TIn, TOut>(IDbConnection connection, Expression<Func<TIn, TOut>> func, object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IReferenceMap> includedProperties = null, bool noLock = false) where TIn : class where TOut : class
        {
            var cols = GetBufferedCols<TOut>();
            var te = InternalGetSet<TIn>(connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered, cols, includedProperties, noLock).ToList();

            // Transform TIn object to Anonymous type
            var f = func.Compile();
            return te.Select(i => f.Invoke(i));
        }

        private static object GetParameterValue(KeyValuePair<string, object> parameter)
        {
            var value = parameter.Value;
            if (parameter.Value is JToken)
            {
                var val = (JToken)value;
                value = Convert.ChangeType(val, val.Type.GetType());
            }
            return value;
        }

        public int Count<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class
        {
            GetMapAndPredicate<T>(predicate, out var classMap, out var wherePredicate);
            var parameters = new Dictionary<string, object>();
            var sql = SqlGenerator.Count(classMap, wherePredicate, parameters, includedProperties, noLock);

            var dynamicParameters = GetDynamicParameters(parameters);
            LastExecutedCommand = sql;
            return (int)connection.Query(sql, dynamicParameters, transaction, false, commandTimeout, CommandType.Text).Single().Total;
        }

        public IMultipleResultReader GetMultiple(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout, IList<IReferenceMap> includedProperties = null)
        {
            if (SqlGenerator.SupportsMultipleStatements())
            {
                return GetMultipleByBatch(connection, predicate, transaction, commandTimeout, includedProperties);
            }

            return GetMultipleBySequence(connection, predicate, transaction, commandTimeout, includedProperties);
        }

        protected (IEnumerable<T>, Snapshot<T>) GetList<T>(IDbConnection connection, IList<IProjection> colsToSelect, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity
        {
            return GetListAutoMap<T>(connection, colsToSelect, classMap, predicate, sort, transaction, commandTimeout, buffered, includedProperties, changeTrack, noLock);
        }

        protected (IEnumerable<T>, Snapshot<T>) GetListAutoMap<T>(IDbConnection connection, IList<IProjection> colsToSelect, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity
        {
            var parameters = new Dictionary<string, object>();
            var sql = SqlGenerator.Select(classMap, predicate, sort, parameters, colsToSelect, includedProperties, noLock);
            var dynamicParameters = GetDynamicParameters(parameters);

            LastExecutedCommand = sql;
            var query = connection.Query<dynamic>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);

            var result = MappColumns<T>(query);

            return (result, changeTrack ? Snapshotter.Start(result) : null);
        }

        protected IEnumerable<T> GetPageAutoMap<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class
        {
            var parameters = new Dictionary<string, object>();
            var sql = SqlGenerator.SelectPaged(classMap, predicate, sort, page, resultsPerPage, parameters, colsToSelect, includedProperties, noLock);
            var dynamicParameters = GetDynamicParameters(parameters);

            LastExecutedCommand = sql;
            var query = connection.Query<dynamic>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);

            return MappColumns<T>(query);
        }

        protected IEnumerable<T> GetPage<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class
        {
            return GetPageAutoMap<T>(connection, classMap, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered, colsToSelect, includedProperties, noLock);
        }

        protected IEnumerable<T> GetSet<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class
        {
            var parameters = new Dictionary<string, object>();
            var sql = SqlGenerator.SelectSet(classMap, predicate, sort, firstResult, maxResults, parameters, colsToSelect, includedProperties, noLock);
            var dynamicParameters = GetDynamicParameters(parameters);

            LastExecutedCommand = sql;
            return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
        }

        protected bool Delete<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            var parameters = new Dictionary<string, object>();
            var sql = SqlGenerator.Delete(classMap, predicate, parameters);
            var dynamicParameters = GetDynamicParameters(parameters);

            LastExecutedCommand = sql;
            return connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
        }

        protected static IPredicate GetPredicate(IClassMapper classMap, object predicate)
        {
            var wherePredicate = predicate as IPredicate;
            if (wherePredicate == null && predicate != null)
            {
                wherePredicate = GetEntityPredicate(classMap, predicate);
            }

            return wherePredicate;
        }

        private static IPredicate ReturnPredicate(IList<IPredicate> predicates)
        {
            return predicates.Count == 1
                                   ? predicates[0]
                                   : new PredicateGroup
                                   {
                                       Operator = GroupOperator.And,
                                       Predicates = predicates
                                   };
        }

        protected static IPredicate GetIdPredicate(IClassMapper classMap, object id)
        {
            var isSimpleType = ReflectionHelper.IsSimpleType(id.GetType());
            var keys = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey);
            IDictionary<string, Func<object>> paramValues = null;
            var predicates = new List<IPredicate>();
            if (!isSimpleType)
            {
                paramValues = ReflectionHelper.GetObjectValues(id);
            }

            foreach (var key in keys)
            {
                var value = id;
                if (!isSimpleType)
                {
                    value = paramValues[key.Name];
                }

                var predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);

                var fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
                fieldPredicate.Not = false;
                fieldPredicate.Operator = Operator.Eq;
                fieldPredicate.PropertyName = key.Name;
                fieldPredicate.Value = value;
                predicates.Add(fieldPredicate);
            }

            return ReturnPredicate(predicates);
        }

        protected static IPredicate GetKeyPredicate<T>(IClassMapper classMap, T entity, bool isEntity = true) where T : class
        {
            var whereFields = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey && p.KeyType != KeyType.ForeignKey);
            if (!whereFields.Any())
            {
                throw new ArgumentException("At least one Key column must be defined.");
            }

            IList<IPredicate> predicates = new List<IPredicate>();
            var predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);
            foreach (var field in whereFields)
            {
                var fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
                fieldPredicate.Not = false;
                fieldPredicate.Operator = Operator.Eq;
                fieldPredicate.PropertyName = field.Name;
                fieldPredicate.Value = isEntity ? field.GetValue(entity) : entity;
                predicates.Add(fieldPredicate);
            }

            return ReturnPredicate(predicates);
        }

        protected static IPredicate GetEntityPredicate(IClassMapper classMap, object entity)
        {
            var predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);
            IList<IPredicate> predicates = new List<IPredicate>();
            var notIgnoredColumns = classMap.Properties.Where(p => !p.Ignored);
            foreach (var kvp in ReflectionHelper.GetObjectValues(entity).Where(property => notIgnoredColumns.Any(c => c.Name == property.Key)))
            {
                var fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
                fieldPredicate.Not = false;
                fieldPredicate.Operator = Operator.Eq;
                fieldPredicate.PropertyName = kvp.Key;
                fieldPredicate.Value = kvp.Value is Func<object> ? kvp.Value() : kvp.Value;
                predicates.Add(fieldPredicate);
            }

            return ReturnPredicate(predicates);
        }

        protected GridReaderResultReader GetMultipleByBatch(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout, IList<IReferenceMap> includedProperties = null)
        {
            var parameters = new Dictionary<string, object>();
            var sql = new StringBuilder();
            foreach (var item in predicate.Items)
            {
                var classMap = SqlGenerator.Configuration.GetMap(item.Type);
                var itemPredicate = item.Value as IPredicate;
                if (itemPredicate == null && item.Value != null)
                {
                    itemPredicate = GetPredicate(classMap, item.Value);
                }

                _ = sql.Append(SqlGenerator.Select(classMap, itemPredicate, item.Sort, parameters, null, includedProperties)).AppendLine(SqlGenerator.Configuration.Dialect.BatchSeperator);
            }

            var dynamicParameters = GetDynamicParameters(parameters);

            var grid = connection.QueryMultiple(sql.ToString(), dynamicParameters, transaction, commandTimeout, CommandType.Text);
            return new GridReaderResultReader(grid);
        }

        protected SequenceReaderResultReader GetMultipleBySequence(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout, IList<IReferenceMap> includedProperties = null)
        {
            IList<SqlMapper.GridReader> items = new List<SqlMapper.GridReader>();
            foreach (var item in predicate.Items)
            {
                var parameters = new Dictionary<string, object>();
                var classMap = SqlGenerator.Configuration.GetMap(item.Type);
                var itemPredicate = item.Value as IPredicate;
                if (itemPredicate == null && item.Value != null)
                {
                    itemPredicate = GetPredicate(classMap, item.Value);
                }

                var sql = SqlGenerator.Select(classMap, itemPredicate, item.Sort, parameters, null, includedProperties);
                var dynamicParameters = GetDynamicParameters(parameters);

                var queryResult = connection.QueryMultiple(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
                items.Add(queryResult);
            }

            return new SequenceReaderResultReader(items);
        }

        protected void SetAutoMapperIdentifier(IList<Table> tables)
        {
            foreach (var table in tables)
            {
                var map = SqlGenerator.Configuration.GetMap(table.EntityType);

                var properties = map
                    .Properties
                    .Where(p => p.KeyType == KeyType.Assigned ||
                                p.KeyType == KeyType.Identity ||
                                p.KeyType == KeyType.SlapperIdentifierKey ||
                                p.KeyType == KeyType.SequenceIdentity)
                    .Select(p => p.Name)
                    .ToList();

                AutoMapper.Configuration.AddIdentifiers(table.IsVirtual ? table.EntityType.BaseType : table.EntityType, properties);
            }
        }

        protected string GetColumnAliasFromSimpleAlias(string simpleAlias)
        {
            if (SqlGenerator.AllColumns.Any(c => c.SimpleAlias.Equals(simpleAlias, StringComparison.InvariantCultureIgnoreCase)))
                return SqlGenerator.AllColumns.Where(c => c.SimpleAlias.Equals(simpleAlias, StringComparison.InvariantCultureIgnoreCase)).Select(c => c.Alias).Single();
            return "";
        }

        protected string GetSimpleAliasFromColumnAlias(string columnAlias)
        {
            if (SqlGenerator.AllColumns.Any(c => c.Alias.Equals(columnAlias, StringComparison.InvariantCultureIgnoreCase)))
                return SqlGenerator.AllColumns.Where(c => c.Alias.Equals(columnAlias, StringComparison.InvariantCultureIgnoreCase)).Select(c => c.SimpleAlias).Single();
            return "";
        }

        protected IEnumerable<T> MappColumns<T>(IEnumerable<dynamic> values) where T : class
        {
            var list = new List<Dictionary<string, object>>();
            foreach (var d in values.ToList())
            {
                var dictionary = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> kvp in d)
                {
                    var alias = GetColumnAliasFromSimpleAlias(kvp.Key);
                    if (!string.IsNullOrEmpty(alias))
                        dictionary.Add(alias, kvp.Value);
                }

                list.Add(dictionary);
            }

            SetAutoMapperIdentifier(SqlGenerator.MappedTables);

            return AutoMapper.Map<T>(list, false);
        }

        protected static DynamicParameters GetDynamicParameters(Dictionary<string, object> parameters)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                if (parameter.Value is Parameter p)
                {
                    dynamicParameters.Add(p.Name, p.Value, p.DbType,
                                          p.ParameterDirection, p.Size, p.Precision,
                                          p.Scale);
                }
                else
                {
                    dynamicParameters.Add(parameter.Key, GetParameterValue(parameter));
                }
            }
            return dynamicParameters;
        }

        protected virtual DynamicParameters AddParameter<T>(T entity, DynamicParameters parameters, IMemberMap prop, bool useColumnAlias = false)
        {
            var propValue = prop.GetValue(entity);
            var parameter = ReflectionHelper.GetParameter(typeof(T), SqlGenerator, prop.Name, propValue);
            var alias = GetSimpleAliasFromColumnAlias(parameter.Name);
            var name = useColumnAlias ? string.IsNullOrEmpty(alias) ? parameter.Name : alias : parameter.Name;

            parameters ??= new DynamicParameters();

            if (prop.MemberInfo.DeclaringType == typeof(bool) || (prop.MemberInfo.DeclaringType.IsGenericType && prop.MemberType.GetGenericTypeDefinition() == typeof(Nullable<>) && prop.MemberInfo.DeclaringType.GetGenericArguments()[0] == typeof(bool)))
            {
                var value = (bool?)propValue;
                if (!value.HasValue)
                {
                    parameters.Add(name, value, parameter.DbType,
                                  parameter.ParameterDirection, parameter.Size, parameter.Precision,
                                  parameter.Scale);
                }
                else
                {
                    parameters.Add(name, value.Value ? 1 : 0, parameter.DbType,
                                  parameter.ParameterDirection, parameter.Size, parameter.Precision,
                                  parameter.Scale);
                }
            }
            else
            {
                parameters.Add(name, parameter.Value, parameter.DbType,
                                  parameter.ParameterDirection, parameter.Size, parameter.Precision,
                                  parameter.Scale);
            }

            return parameters;
        }

        private DynamicParameters GetDynamicParameters<T>(T entity, IClassMapper classMap, IList<IMemberMap> sequenceColumn, IList<MemberInfo> foreignKeys, IList<MemberInfo> ignoredColumns, bool useColumnAlias, IList<IProjection> cols = null)
        {
            var keyColumns = sequenceColumn.Count == 0 ? classMap.Properties.Where(p => p.KeyType == KeyType.Assigned || p.KeyType == KeyType.Guid)?.ToList() : sequenceColumn;

            var dynamicParameters = new DynamicParameters();

            foreach (var prop in entity.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Public)
                .Where(p => !keyColumns.Any(k => k.Name.Equals(p.Name)) && !foreignKeys.Contains(p) && !ignoredColumns.Contains(p)
                && (cols == null || cols?.Any(cu => cu.PropertyName.Equals(p.Name, StringComparison.OrdinalIgnoreCase)) == true)
                ))
                dynamicParameters = AddParameter(entity, dynamicParameters, new MemberMap(prop), useColumnAlias);

            return dynamicParameters;
        }

        public DynamicParameters GetDynamicParameters<T>(IClassMapper classMap, T entity, bool useColumnAlias = false, IList<IProjection> cols = null)
        {
            var sequenceIdentityColumn = classMap.Properties.Where(p => p.KeyType == KeyType.SequenceIdentity)?.ToList();
            var foreignKeys = classMap.Properties.Where(p => p.KeyType == KeyType.ForeignKey).Select(p => p.MemberInfo).ToList();
            var ignored = classMap.Properties.Where(x => x.Ignored).Select(p => p.MemberInfo).ToList();

            if (sequenceIdentityColumn?.Count > 1)
                throw new ArgumentException("SequenceIdentity generator cannot be used with multi-column keys");

            return GetDynamicParameters(entity, classMap, sequenceIdentityColumn, foreignKeys, ignored, useColumnAlias, cols);
        }

        public DynamicParameters GetDynamicParameters<T>(T entity, DynamicParameters dynamicParameters, IMemberMap keyColumn, bool useColumnAlias = false)
        {
            dynamicParameters ??= new DynamicParameters();
            foreach (var prop in entity.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.Name != keyColumn.Name))
                AddParameter(entity, dynamicParameters, new MemberMap(prop), useColumnAlias);

            return dynamicParameters;
        }

        /// <summary>
        /// Return property liste from (anonymous) type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static IList<IProjection> GetBufferedCols<T>()
        {
            Type outType = typeof(T);

            lock (ColsBuffer)
            {
                if (ColsBuffer.TryGetValue(outType, out IList<IProjection> cols) == false)
                {
                    cols = new List<IProjection>();

                    typeof(T).GetProperties().
                        Select(i => i.Name).
                        ToList().
                        ForEach(p => cols.Add(new Projection(p)));

                    ColsBuffer.Add(outType, cols);
                }

                return cols;
            }
        }

        /// <summary>
        /// Return property liste from Snapshot
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>

        protected IList<IProjection> GetChangeTrackCols<T>(Snapshot<T> snapshot, T entity) where T : BaseEntity
        {
            var cols = new List<IProjection>();

            if (snapshot != null)
            {
                var dynparams = snapshot.Diff(entity);

                foreach (var pair in dynparams.ParameterNames)
                {
                    cols.Add(new Projection(pair));
                }
            }

            return cols.Any() ? cols : null;
        }

        private object InsertTriggered<T>(IDbConnection connection, T entity, IDbTransaction transaction,
            int? commandTimeout, string sql, IMemberMap key, DynamicParameters dynamicParameters)
        {
            // defaultValue need for identify type of parameter
            var defaultValue = entity.GetType().GetProperty(key.Name).GetValue(entity, null);
            dynamicParameters.Add("IdOutParam", direction: ParameterDirection.Output, value: defaultValue);

            LastExecutedCommand = sql;
            connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);

            return dynamicParameters.Get<object>(SqlGenerator.Configuration.Dialect.ParameterPrefix + "IdOutParam");
        }

        private object InsertIdentity(IDbConnection connection, IDbTransaction transaction,
            int? commandTimeout, IClassMapper classMap, string sql, DynamicParameters dynamicParameters)
        {
            IEnumerable<long> result;

            if (SqlGenerator.SupportsMultipleStatements())
            {
                sql += SqlGenerator.Configuration.Dialect.BatchSeperator + SqlGenerator.IdentitySql(classMap);
                result = connection.Query<long>(sql, dynamicParameters, transaction, false, commandTimeout, CommandType.Text);
            }
            else
            {
                connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
                sql = SqlGenerator.IdentitySql(classMap);
                result = connection.Query<long>(sql, dynamicParameters, transaction, false, commandTimeout, CommandType.Text);
            }
            LastExecutedCommand = sql;

            // We are only interested in the first identity, but we are iterating over all resulting items (if any).
            // This makes sure that ADO.NET drivers (like MySql) won't actively terminate the query.
            var hasResult = false;
            object keyValue = null;
            foreach (var identityValue in result)
            {
                if (hasResult)
                {
                    continue;
                }
                keyValue = identityValue;
                hasResult = true;
            }

            if (!hasResult)
            {
                throw new InvalidOperationException("The source sequence is empty.");
            }
            else
            {
                return keyValue;
            }
        }

        private IDictionary<string, object> AddSequenceParameter<T>(IDbConnection connection, T entity, 
            IMemberMap key, DynamicParameters dynamicParameters, IDictionary<string, object> keyValues)
        {
            var query = $"select {key.SequenceName}.nextval seq from dual";
            var value = connection.ExecuteScalar<int>(query);

            key.SetValue(entity, value);

            AddParameter(entity, dynamicParameters, key, true);

            keyValues ??= new ExpandoObject();
            keyValues.Add(key.Name, value);

            return keyValues;
        }

        private void AddKeyParameters<T>(T entity, IList<IMemberMap> keyList, DynamicParameters dynamicParameters, bool useColumnAlias = false)
        {
            foreach (var prop in keyList)
            {
                dynamicParameters = AddParameter(entity, dynamicParameters, prop, useColumnAlias);
            }
        }

        protected dynamic InternalInsert<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout,
            IClassMapper classMap, IList<IMemberMap> nonIdentityKeyProperties, IMemberMap identityColumn,
            IMemberMap triggerIdentityColumn, IList<IMemberMap> sequenceIdentityColumn) where T : class
        {
            DynamicParameters dynamicParameters = null;

            foreach (var column in nonIdentityKeyProperties)
            {
                if (column.KeyType == KeyType.Guid && (Guid)column.GetValue(entity) == Guid.Empty)
                {
                    var comb = SqlGenerator.Configuration.GetNextGuid();
                    column.SetValue(entity, comb);
                }
            }

            IDictionary<string, object> keyValues = new ExpandoObject();
            var sql = SqlGenerator.Insert(classMap);
            if (triggerIdentityColumn != null || identityColumn != null)
            {
                var keyColumn = triggerIdentityColumn ?? identityColumn;
                object keyValue;

                dynamicParameters = GetDynamicParameters(entity, dynamicParameters, keyColumn, true);

                if (triggerIdentityColumn != null)
                {
                    keyValue = InsertTriggered(connection, entity, transaction, commandTimeout, sql, triggerIdentityColumn, dynamicParameters);
                }
                else
                {
                    keyValue = InsertIdentity(connection, transaction, commandTimeout, classMap, sql, dynamicParameters);
                }

                keyValues.Add(keyColumn.Name, keyValue);
                keyColumn.SetValue(entity, keyValue);
            }
            else
            {
                dynamicParameters = GetDynamicParameters(classMap, entity, true);

                if (sequenceIdentityColumn.Count > 0)
                {
                    if (sequenceIdentityColumn.Count > 1)
                        throw new ArgumentException("SequenceIdentity generator cannot be used with multi-column keys");

                    AddSequenceParameter(connection, entity, sequenceIdentityColumn[0], dynamicParameters, keyValues);
                }
                else if (nonIdentityKeyProperties != null)
                {
                    AddKeyParameters(entity, nonIdentityKeyProperties, dynamicParameters, true);
                }

                LastExecutedCommand = sql;
                connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
            }

            foreach (var column in nonIdentityKeyProperties)
            {
                keyValues.Add(column.Name, column.GetValue(entity));
            }

            if (keyValues.Count == 1)
            {
                return keyValues.First().Value;
            }

            return keyValues;
        }

        protected bool InternalUpdate<T>(IDbConnection connection, T entity, IClassMapper classMap, IPredicate predicate, IDbTransaction transaction, IList<IProjection> cols, int? commandTimeout, bool ignoreAllKeyProperties = false) where T : class
        {
            var parameters = new Dictionary<string, object>();
            string sql = SqlGenerator.Update(classMap, predicate, parameters, ignoreAllKeyProperties, cols);

            var dynamicParameters = GetDynamicParameters(classMap, entity, true, cols);

            dynamicParameters.AddDynamicParams(GetDynamicParameters(parameters));

            LastExecutedCommand = sql;
            return connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
        }

        protected bool InternalUpdate<T>(IDbConnection connection, T entity, IDbTransaction transaction, IList<IProjection> cols, int? commandTimeout, bool ignoreAllKeyProperties = false) where T : class
        {
            GetMapAndPredicate<T>(entity, out var classMap, out var predicate, true);
            return InternalUpdate(connection, entity, classMap, predicate, transaction, cols, commandTimeout, ignoreAllKeyProperties);
        }

        protected void InternalUpdate<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction, IList<IProjection> cols, int? commandTimeout, bool ignoreAllKeyProperties = false) where T : class
        {
            GetMapAndPredicate<T>(entities.FirstOrDefault(), out var classMap, out var predicate, true);

            foreach (var e in entities)
                InternalUpdate(connection, e, classMap, predicate, transaction, cols, commandTimeout, ignoreAllKeyProperties);
        }

        protected (T, Snapshot<T>) InternalGet<T>(IDbConnection connection, dynamic id, IDbTransaction transaction, int? commandTimeout, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity
        {
            var result = InternalGetAutoMap<T>(connection, id, null, transaction, commandTimeout, true, colsToSelect, includedProperties, changeTrack, noLock);

            //if(changeTrack)
            //{
            //    snapshot = Snapshotter.Start(result.SingleOrDefault());
            //    //Snapper.AddOrUpdate(nameof(T), snapshotter);
            //}

            // changed from SingleOrDefault to FirstOrDefault
            return (result.Item1.FirstOrDefault(), result.Item2);
        }

        protected (T, Snapshot<T>) InternalGetPredicate<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity
        {
            var (result, shot) = InternalGetAutoMapPredicate<T>(connection, predicate, null, transaction, commandTimeout, true, colsToSelect, includedProperties, changeTrack, noLock);

            // changed from SingleOrDefault to FirstOrDefault
            return (result.FirstOrDefault(), shot);
        }

        protected (IEnumerable<T>, Snapshot<T>) InternalGetAutoMap<T>(IDbConnection connection, object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity
        {
            GetMapAndPredicate<T>(predicate, out var classMap, out var wherePredicate, true, false);
            return GetListAutoMap<T>(connection, colsToSelect, classMap, wherePredicate, sort, transaction, commandTimeout, buffered, includedProperties, changeTrack:changeTrack, noLock:noLock);
        }

        protected (IEnumerable<T>, Snapshot<T>) InternalGetAutoMapPredicate<T>(IDbConnection connection, object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity
        {
            GetMapAndPredicate<T>(predicate, out var classMap, out var wherePredicate);
            return GetListAutoMap<T>(connection, colsToSelect, classMap, wherePredicate, sort, transaction, commandTimeout, buffered, includedProperties, changeTrack: changeTrack, noLock: noLock);
        }

        protected (IEnumerable<T>, Snapshot<T>) InternalGetListAutoMap<T>(IDbConnection connection, object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null, bool changeTrack = false, bool noLock = false) where T : BaseEntity
        {
            GetMapAndPredicate<T>(predicate, out var classMap, out var wherePredicate);
            return GetListAutoMap<T>(connection, colsToSelect, classMap, wherePredicate, sort, transaction, commandTimeout, buffered, includedProperties, changeTrack: changeTrack, noLock: noLock);
        }

        protected IEnumerable<T> InternalGetPageAutoMap<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class
        {
            GetMapAndPredicate<T>(predicate, out var classMap, out var wherePredicate);
            return GetPageAutoMap<T>(connection, classMap, wherePredicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered, colsToSelect, includedProperties, noLock);
        }

        protected IEnumerable<T> InternalGetSet<T>(IDbConnection connection, object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null, bool noLock = false) where T : class
        {
            GetMapAndPredicate<T>(predicate, out var classMap, out var wherePredicate);
            return GetSet<T>(connection, classMap, wherePredicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered, colsToSelect, includedProperties, noLock);
        }

        protected bool InternalDelete<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            GetMapAndPredicate<T>(entity, out var classMap, out var predicate, true);

            return Delete<T>(connection, classMap, predicate, transaction, commandTimeout);
        }

        protected virtual void GetMapAndPredicate<T>(object predicateValue, out IClassMapper classMapper, out IPredicate wherePredicate, bool keyPredicate = false, bool isEntity = true) where T : class
        {
            classMapper = SqlGenerator.Configuration.GetMap<T>();
            wherePredicate = keyPredicate ? GetKeyPredicate(classMapper, predicateValue, isEntity) : GetPredicate(classMapper, predicateValue);
        }
    }
}
