﻿using FreeSql.Extensions.EntityUtil;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreeSql.Internal.CommonProvider
{

    partial class UpdateProvider<T1>
    {
#if net40
#else
        async protected virtual Task<int> SplitExecuteAffrowsAsync(int valuesLimit, int parameterLimit, CancellationToken cancellationToken = default)
        {
            var ss = SplitSource(valuesLimit, parameterLimit);
            var ret = 0;
            if (ss.Length <= 1)
            {
                if (_source?.Any() == true) _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, 1, 1));
                ret = await this.RawExecuteAffrowsAsync(cancellationToken);
                ClearData();
                return ret;
            }
            if (_transaction == null)
            {
                var threadTransaction = _orm.Ado.TransactionCurrentThread;
                if (threadTransaction != null) this.WithTransaction(threadTransaction);
            }

            var before = new Aop.TraceBeforeEventArgs("SplitExecuteAffrowsAsync", null);
            _orm.Aop.TraceBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                if (_transaction != null || _batchAutoTransaction == false)
                {
                    for (var a = 0; a < ss.Length; a++)
                    {
                        _source = ss[a];
                        _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                        ret += await this.RawExecuteAffrowsAsync(cancellationToken);
                    }
                }
                else
                {
                    if (_orm.Ado.MasterPool == null) throw new Exception(CoreStrings.MasterPool_IsNull_UseTransaction);
                    using (var conn = await _orm.Ado.MasterPool.GetAsync())
                    {
                        _transaction = conn.Value.BeginTransaction();
                        var transBefore = new Aop.TraceBeforeEventArgs("BeginTransaction", null);
                        _orm.Aop.TraceBeforeHandler?.Invoke(this, transBefore);
                        try
                        {
                            for (var a = 0; a < ss.Length; a++)
                            {
                                _source = ss[a];
                                _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                                ret += await this.RawExecuteAffrowsAsync(cancellationToken);
                            }
                            _transaction.Commit();
                            _orm.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(transBefore, CoreStrings.Commit, null));
                        }
                        catch (Exception ex)
                        {
                            _transaction.Rollback();
                            _orm.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(transBefore, CoreStrings.RollBack, ex));
                            throw;
                        }
                        _transaction = null;
                    }
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.TraceAfterEventArgs(before, null, exception);
                _orm.Aop.TraceAfterHandler?.Invoke(this, after);
            }
            ClearData();
            return ret;
        }
        async protected virtual Task<List<T1>> SplitExecuteUpdatedAsync(int valuesLimit, int parameterLimit, CancellationToken cancellationToken = default)
        {
            var ss = SplitSource(valuesLimit, parameterLimit);
            var ret = new List<T1>();
            if (ss.Length <= 1)
            {
                if (_source?.Any() == true) _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, 1, 1));
                ret = await this.RawExecuteUpdatedAsync(cancellationToken);
                ClearData();
                return ret;
            }
            if (_transaction == null)
            {
                var threadTransaction = _orm.Ado.TransactionCurrentThread;
                if (threadTransaction != null) this.WithTransaction(threadTransaction);
            }

            var before = new Aop.TraceBeforeEventArgs("SplitExecuteUpdatedAsync", null);
            _orm.Aop.TraceBeforeHandler?.Invoke(this, before);
            Exception exception = null;
            try
            {
                if (_transaction != null || _batchAutoTransaction == false)
                {
                    for (var a = 0; a < ss.Length; a++)
                    {
                        _source = ss[a]; 
                        _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                        ret.AddRange(await this.RawExecuteUpdatedAsync(cancellationToken));
                    }
                }
                else
                {
                    if (_orm.Ado.MasterPool == null) throw new Exception(CoreStrings.MasterPool_IsNull_UseTransaction);
                    using (var conn = await _orm.Ado.MasterPool.GetAsync())
                    {
                        _transaction = conn.Value.BeginTransaction();
                        var transBefore = new Aop.TraceBeforeEventArgs("BeginTransaction", null);
                        _orm.Aop.TraceBeforeHandler?.Invoke(this, transBefore);
                        try
                        {
                            for (var a = 0; a < ss.Length; a++)
                            {
                                _source = ss[a];
                                _batchProgress?.Invoke(new BatchProgressStatus<T1>(_source, a + 1, ss.Length));
                                ret.AddRange(await this.RawExecuteUpdatedAsync(cancellationToken));
                            }
                            _transaction.Commit();
                            _orm.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(transBefore, CoreStrings.Commit, null));
                        }
                        catch (Exception ex)
                        {
                            _transaction.Rollback();
                            _orm.Aop.TraceAfterHandler?.Invoke(this, new Aop.TraceAfterEventArgs(transBefore, CoreStrings.RollBack, ex));
                            throw;
                        }
                        _transaction = null;
                    }
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                var after = new Aop.TraceAfterEventArgs(before, null, exception);
                _orm.Aop.TraceAfterHandler?.Invoke(this, after);
            }
            ClearData();
            return ret;
        }

        async protected Task<int> RawExecuteAffrowsAsync(CancellationToken cancellationToken = default)
        {
            var affrows = 0;
            DbParameter[] dbParms = null;
            await ToSqlFetchAsync(async sb =>
            {
                if (dbParms == null) dbParms = _params.Concat(_paramsSource).ToArray();
                var sql = sb.ToString();
                var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Update, sql, dbParms);
                _orm.Aop.CurdBeforeHandler?.Invoke(this, before);

                Exception exception = null;
                try
                {
                    var affrowstmp = await _orm.Ado.ExecuteNonQueryAsync(_connection, _transaction, CommandType.Text, sql, _commandTimeout, dbParms, cancellationToken);
                    ValidateVersionAndThrow(affrowstmp, sql, dbParms);
                    affrows += affrowstmp;
                }
                catch (Exception ex)
                {
                    exception = ex;
                    throw;
                }
                finally
                {
                    var after = new Aop.CurdAfterEventArgs(before, exception, affrows);
                    _orm.Aop.CurdAfterHandler?.Invoke(this, after);
                }
            });
            return affrows;
        }
        protected abstract Task<List<T1>> RawExecuteUpdatedAsync(CancellationToken cancellationToken = default);

        public abstract Task<int> ExecuteAffrowsAsync(CancellationToken cancellationToken = default);
        public abstract Task<List<T1>> ExecuteUpdatedAsync(CancellationToken cancellationToken = default);
#endif
    }
}
