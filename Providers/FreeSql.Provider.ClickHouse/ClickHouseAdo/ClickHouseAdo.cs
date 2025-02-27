﻿using ClickHouse.Client.ADO;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using FreeSql.Internal.Model;
using FreeSql.Internal.ObjectPool;
using System;
using System.Collections;
using System.Data.Common;
using System.Linq;
using System.Threading;

namespace FreeSql.ClickHouse
{
    class ClickHouseAdo : FreeSql.Internal.CommonProvider.AdoProvider
    {

        public ClickHouseAdo() : base(DataType.ClickHouse, null, null) { }
        public ClickHouseAdo(CommonUtils util, string masterConnectionString, string[] slaveConnectionStrings, Func<DbConnection> connectionFactory) : base(DataType.ClickHouse, masterConnectionString, slaveConnectionStrings)
        {
            base._util = util;
            if (connectionFactory != null)
            {
                var pool = new FreeSql.Internal.CommonProvider.DbConnectionPool(DataType.ClickHouse, connectionFactory);
                ConnectionString = pool.TestConnection?.ConnectionString;
                MasterPool = pool;
                return;
            }

            var isAdoPool = masterConnectionString?.StartsWith("AdoConnectionPool,") ?? false;
            if (isAdoPool) masterConnectionString = masterConnectionString.Substring("AdoConnectionPool,".Length);
            if (!string.IsNullOrEmpty(masterConnectionString))
                MasterPool = isAdoPool ?
                    new DbConnectionStringPool(base.DataType, CoreStrings.S_MasterDatabase, () => new ClickHouseConnection(masterConnectionString)) as IObjectPool<DbConnection> :
                    new ClickHouseConnectionPool(CoreStrings.S_MasterDatabase, masterConnectionString, null, null);

            slaveConnectionStrings?.ToList().ForEach(slaveConnectionString =>
            {
                var slavePool = isAdoPool ?
                        new DbConnectionStringPool(base.DataType, $"{CoreStrings.S_SlaveDatabase}{SlavePools.Count + 1}", () => new ClickHouseConnection(slaveConnectionString)) as IObjectPool<DbConnection> :
                        new ClickHouseConnectionPool($"{CoreStrings.S_SlaveDatabase}{SlavePools.Count + 1}", slaveConnectionString, () => Interlocked.Decrement(ref slaveUnavailables), () => Interlocked.Increment(ref slaveUnavailables));
                SlavePools.Add(slavePool);
            });
        }
        public override object AddslashesProcessParam(object param, Type mapType, ColumnInfo mapColumn)
        {
            if (param == null) return "NULL";
            if (mapType != null && mapType != param.GetType() && (param is IEnumerable == false))
                param = Utils.GetDataReaderValue(mapType, param);

            if (param is bool || param is bool?)
                return (bool)param ? 1 : 0;
            else if (param is string)
                return string.Concat("'", param.ToString().Replace("'", "''").Replace("\\", "\\\\"), "'"); //只有 mysql 需要处理反斜杠
            else if (param is char)
                return string.Concat("'", param.ToString().Replace("'", "''").Replace("\\", "\\\\").Replace('\0', ' '), "'");
            else if (param is Enum)
                return string.Concat("'", param.ToString().Replace("'", "''").Replace("\\", "\\\\"), "'"); //((Enum)val).ToInt64();
            else if (decimal.TryParse(string.Concat(param), out var trydec))
                return param;
            else if (param is DateTime || param is DateTime?)
                return string.Concat("'", ((DateTime)param).ToString("yyyy-MM-dd HH:mm:ss"), "'");
            else if (param is TimeSpan || param is TimeSpan?)
                return ((TimeSpan)param).Ticks / 10;
            else if (param is byte[])
                return $"0x{CommonUtils.BytesSqlRaw(param as byte[])}";
            else if (param is IEnumerable)
                return AddslashesIEnumerable(param, mapType, mapColumn);

            return string.Concat("'", param.ToString().Replace("'", "''").Replace("\\", "\\\\"), "'");
        }

        public override DbCommand CreateCommand()
        {
            System.Data.IDbCommand command =  new ClickHouseCommand();
            return (DbCommand)command;
        }

        public override void ReturnConnection(IObjectPool<DbConnection> pool, Object<DbConnection> conn, Exception ex)
        {
            var rawPool = pool as ClickHouseConnectionPool;
            if (rawPool != null) rawPool.Return(conn, ex);
            else pool.Return(conn);
        }

        public override DbParameter[] GetDbParamtersByObject(string sql, object obj) => _util.GetDbParamtersByObject(sql, obj);
    }
}
